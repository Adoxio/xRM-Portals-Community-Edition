/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using Microsoft.Xrm.Portal;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources; 
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Commerce
{
	/// <summary>
	/// Used to provide Paypal actions for connecting to paypal and processing paypal data.
	/// </summary>
	public class PayPalHelper 
	{
		public const string SandboxURL = "https://www.sandbox.paypal.com/us/cgi-bin/webscr";
		public const string LiveURL = "https://www.paypal.com/cgi-bin/webscr";

		/// <summary>
		/// The base Paypal URL
		/// </summary>
		public string PayPalBaseUrl { get; private set; }

		public string PayPalAccountEmail { get; private set; }


		public PayPalHelper(IPortalContext xrm) : this(GetPaypalBaseUrl(xrm), GetPaypalAccountEmail(xrm))
		{
			
		}

		public PayPalHelper(string baseURL, string accountEmail)
		{
			PayPalBaseUrl = baseURL;

			PayPalAccountEmail = accountEmail;
		}

		/// <summary>
		/// Creates the URL used to submit verification to paypal.
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public string GetSubmitUrl(Dictionary<string, string> values)
		{
			StringBuilder url = new StringBuilder();

			if (values.ContainsKey("cmd") && values.ContainsKey("business"))
			{
				//Do I need to check if dict contains "upload" and put it right after "cmd"????
				url.AppendFormat("{0}?cmd={1}&business={2}", PayPalBaseUrl, values["cmd"], values["business"]);
			}
			else
			{
				throw new Exception("Invalid Paypal submit URL.  No value for cmd or business found in query string");
			}

			foreach (var value in values)
			{
				if (value.Key == "cmd" || value.Key == "business")
				{
					continue;
				}

				if (value.Key.Contains("amount") || value.Key.Contains("shipping") || value.Key.Contains("handling"))
				{
					if (decimal.Parse(value.Value) != 0.00M)
					{
						url.AppendFormat("&{0}={1:f2}", value.Key, decimal.Parse(value.Value));
					}
				}

				url.AppendFormat("&{0}={1}", value.Key, HttpUtility.UrlEncode(value.Value));

			}

			return url.ToString();
		}

		public WebResponse GetPaymentWebResponse(string incomingReqStr)
		{
			var newReq = (HttpWebRequest)WebRequest.Create(PayPalBaseUrl);

			//Set values for the request back
			newReq.Method = "POST";
			newReq.ContentType = "application/x-www-form-urlencoded";

			var newRequestStr = incomingReqStr + "&cmd=_notify-validate";
			newReq.ContentLength = newRequestStr.Length;

			//write out the full parameters into the request
			var streamOut = new StreamWriter(newReq.GetRequestStream(), System.Text.Encoding.ASCII);
			streamOut.Write(newRequestStr);
			streamOut.Close();

			//Send request back to Paypal and receive response
			var response = newReq.GetResponse();
			return response;
		}

		public static string GetPaypalBaseUrl(IPortalContext xrm)
		{
			var paypalBaseUrl = xrm.ServiceContext.GetSiteSettingValueByName(xrm.Website, "Ecommerce/Paypal/PaypalBaseUrl");

			if (string.IsNullOrWhiteSpace(paypalBaseUrl))
			{
				paypalBaseUrl = SandboxURL;
			}

			return paypalBaseUrl;
		}

		public static string GetPaypalAccountEmail(IPortalContext xrm)
		{
			var website = xrm.Website;

			var accountEmailSetting = xrm.ServiceContext.CreateQuery("adx_sitesetting")
				.Where(ss => ss.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference())
				.FirstOrDefault(purl => purl.GetAttributeValue<string>("adx_name") == "Ecommerce/Paypal/AccountEmail");

			if (accountEmailSetting == null)
			{
                throw new Exception("An account email for PayPal must be provided as a site setting.  Use site setting PayPal/AccountEmail.");
			}
			var accountEmail = accountEmailSetting.GetAttributeValue<string>("adx_value");
			return accountEmail;
		}

		public bool VerifyIPNOrder(NameValueCollection values, IPortalContext xrm)
		{
			var dict = ToDictionary(values);
			return VerifyIPNOrder(dict, xrm);
		}

		public bool VerifyIPNOrder(Dictionary<string, string> values, IPortalContext xrm)
		{
			System.Diagnostics.Debug.Write("Now verifying the IPN order...");
			var context = xrm.ServiceContext;

			//check the payment_status is Completed
			if (values["payment_status"] != "Completed")
			{
				System.Diagnostics.Debug.Write("The payment_status is not completed...");
				return false;
			}

			if (!values.ContainsKey("invoice"))
			{
				System.Diagnostics.Debug.Write("There is no invoice set...");
				return false;
			}

			//for aggregated data
			if (values.ContainsKey("item_name"))
			{
				//then we are dealing with aggregated data
				if (values["item_name"] != "Aggregated Items")
				{
					return false;
				}
			}
			else
			{
				if (!ItemizedDataVerification(values, context)) return false;
			}

			//check that receiver_email is your Primary PayPal email

			if (!values.ContainsKey("receiver_email"))
			{
				if (values["receiver_email"] != PayPalAccountEmail)
				{
					return false;
				}
			}

			//otherwise, we are golden!

			return true;
		}

		private static bool ItemizedDataVerification(Dictionary<string, string> values, OrganizationServiceContext context)
		{
			System.Diagnostics.Debug.Write("We are performing itemized data verification...");
			//for itemized data
			int count = 1;

			while (values.ContainsKey(string.Format("item_number{0}", count)))
			{
				var guid = Guid.Parse(values[string.Format("item_number{0}", count)]);

				var quantity = decimal.Parse(values[string.Format("quantity{0}", count)] ?? "0.0");

				var gross = decimal.Parse(values[string.Format("mc_gross_{0}", count)] ?? "0.0");
				var shipping = decimal.Parse(values[string.Format("mc_shipping{0}", count)] ?? "0.0");
				var handling = decimal.Parse(values[string.Format("mc_handling{0}", count)] ?? "0.0");
				var tax = decimal.Parse(values[string.Format("tax{0}", count)] ?? "0.0");

				var price = gross - shipping - handling - tax;

				var quoteLineItem =
					context.CreateQuery("quotedetail").FirstOrDefault(
						sci => sci.GetAttributeValue<Guid>("quotedetailid") == guid);

				if (quoteLineItem == null)
				{
					return false;
				}

				var quoteLineItemQuantity = quoteLineItem.GetAttributeValue<decimal?>("quantity").GetValueOrDefault(0);
				var quoteLineQuotedPrice = quoteLineItem.GetAttributeValue<Money>("priceperunit") == null ? 0 : quoteLineItem.GetAttributeValue<Money>("priceperunit").Value;

				if ((quoteLineItemQuantity != quantity) || (quoteLineQuotedPrice * quoteLineItemQuantity != price))
				{
					return false;
				}

				count++;
			}
			return true;
		}

		public static Dictionary<string, string> ToDictionary(NameValueCollection source)
		{
			return source.Cast<string>().Select(s => new { Key = s, Value = source[s] }).ToDictionary(p => p.Key, p => p.Value);
		}

		public static string GetPayPalPdtIdentityToken(IPortalContext xrm)
		{
			var token = xrm.ServiceContext.GetSiteSettingValueByName(xrm.Website, "Ecommerce/PayPal/PDTIdentityToken");

			return token;
		}

		/// <summary>
		/// Send a request to PayPal to get the Payment Data Transfer (PDT) response to confirm successful payment.
		/// https://developer.paypal.com/docs/classic/paypal-payments-standard/integration-guide/paymentdatatransfer/
		/// </summary>
		/// <param name="identityToken">PDT Identity Token specified in "Website Payment Preference" under the PayPal merchant account.</param>
		/// <param name="transactionId">The transaction ID sent to us on the return URL via a HTTP GET as name/value pair tx=transactionID</param>
		public IPayPalPaymentDataTransferResponse GetPaymentDataTransferResponse(string identityToken, string transactionId)
		{
			var query = string.Format("cmd=_notify-synch&tx={0}&at={1}", transactionId, identityToken);

			var request = (HttpWebRequest)WebRequest.Create(PayPalBaseUrl);

			request.Method = WebRequestMethods.Http.Post;
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = query.Length;

			var streamOut = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
			streamOut.Write(query);
			streamOut.Close();
			var streamIn = new StreamReader(request.GetResponse().GetResponseStream());
			var response = streamIn.ReadToEnd();
			streamIn.Close();
			
			return new PayPalPaymentDataTransferResponse(response);
		}

		public enum PaymentDataTransferStatus
		{
			Success = 1,
			Fail = 2,
			Unknown = 3
		}
		
		/// <summary>
		/// PayPal returns related variables for PDT message.
		/// https://developer.paypal.com/docs/classic/ipn/integration-guide/IPNandPDTVariables/ 
		/// </summary>
		public interface IPayPalPaymentDataTransferResponse
		{
			PaymentDataTransferStatus Status { get; }

			Dictionary<string, string> Details { get; }
		}

		public class PayPalPaymentDataTransferResponse : IPayPalPaymentDataTransferResponse
		{
			public PaymentDataTransferStatus Status { get; private set; }

			public Dictionary<string, string> Details { get; private set; }

			public PayPalPaymentDataTransferResponse(string response)
			{
				Details = new Dictionary<string, string>();

				if (string.IsNullOrWhiteSpace(response))
				{
					Status = PaymentDataTransferStatus.Unknown;

					return;
				}

				using (var reader = new StringReader(response))
				{
					var line = reader.ReadLine();

					if (line == "FAIL")
					{
						Status = PaymentDataTransferStatus.Fail;

						return;
					}

					if (line == "SUCCESS")
					{
						Status = PaymentDataTransferStatus.Success;

						var results = new Dictionary<string, string>();

						while ((line = reader.ReadLine()) != null)
						{
							results.Add(line.Split('=')[0], line.Split('=')[1]);
						}

						Details = results;

						return;
					}
				}

				Status = PaymentDataTransferStatus.Unknown;
			}
		}
	}
}
