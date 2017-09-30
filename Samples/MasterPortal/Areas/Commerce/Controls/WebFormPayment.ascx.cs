/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Adxstudio.Xrm;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Commerce;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
#if AUTHORIZENET
using AuthorizeNet;
#endif
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.Commerce.Controls
{
	public partial class WebFormPayment : WebFormCommercePortalUserControl
	{
		private const string TestCreditCardNumber = "4111111111111111";
		private const string TestCreditCardExpiry = "0130";
		private const string TestCreditCardExpiryYear = "2030";
		private const string TestCreditCardExpiryMonth = "01";
		private const string TestCreditCardVerificationValue = "123";

		protected string FirstName;
		protected string LastName;
		protected string Email;
		protected string Address;
		protected string City;
		protected string Province;
		protected string Country;
		protected string PostalCode;
		protected string CreditCardNumber;
		protected string CreditCardExpiryMonth;
		protected string CreditCardExpiryYear;
		protected string CreditCardExpiry;
		protected string CreditCardVerificationValue;
		protected string FingerprintHash;
		protected string FingerprintSequence;
		protected string FingerprintTimestamp;
		protected string ApiLogin;
		protected string Amount;
		protected decimal DecimalAmount;
		protected string Tax;
		protected string TotalAmount;
		protected string RelayURL;
		protected string RelayResponse;
		protected string OrderID;
		protected IEnumerable<string> LineItems;
		protected bool AuthorizeNet;
		protected string ShippingAddress;
		protected string ShippingCity;
		protected string ShippingProvince;
		protected string ShippingPostalCode;
		protected string ShippingCountry;

		public string AuthorizeNetError
		{
			get { return Html.Encode(Request["AuthorizeNetError"]); }
		}

		public bool IsPaymentPaypal
		{
			get { return IsPaymentProvider("PayPal"); }
		}

		public bool IsPaymentAuthorizeNet
		{
			get { return IsPaymentProvider("Authorize.Net"); }
		}

		public bool IsPaymentDemo
		{
			get { return IsPaymentProvider("Demo"); }
		}

		public bool IsPaymentError
		{
			get { return Request["Payment"] == "Unsuccessful"; }
		}

		protected bool Paid
		{
			get
			{
				if (Request["Payment"] != "Successful")
				{
					return false;
				}

				if (Purchasable == null || Purchasable.Quote == null)
				{
					return false;
				}

				return XrmContext.CreateQuery("salesorder")
					.Where(e => e.GetAttributeValue<EntityReference>("quoteid").Equals(Purchasable.Quote))
					.ToArray()
					.Any();
			}
		}

		protected IPurchasable Purchasable { get; private set; }

		protected EntityReference Target { get; private set; }

		protected bool TestModeEnabled
		{
			get
			{
				var testModeEnabled = Html.BooleanSetting("Ecommerce/PaymentTestModeEnabled").GetValueOrDefault(false);
				return testModeEnabled || IsPaymentDemo;
			}
		}
		
		protected void Page_Init(object sender, EventArgs e)
		{
			if (Request.IsSecureConnection || IsPaymentDemo || IsPaymentPaypal)
			{
				return;
			}

			RedirectToHttpsIfNecessary();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Target = GetTargetEntityReference();

			if (Target == null)
			{
				Payment.Visible = false;
				GeneralErrorMessage.Visible = true;

				return;
			}

			Guid quoteId;

			if (IsPostBack && Guid.TryParse(QuoteId.Value, out quoteId))
			{
				WebForm.CurrentSessionHistory.QuoteId = quoteId;
			}

			var dataAdapterDependencies = new Adxstudio.Xrm.Commerce.PortalConfigurationDataAdapterDependencies(PortalName, Request.RequestContext);
			var dataAdapter = CreatePurchaseDataAdapter(Target, CurrentStepEntityPrimaryKeyLogicalName);

			Purchasable = dataAdapter.Select();

			if (Purchasable == null)
			{
				Payment.Visible = false;
				GeneralErrorMessage.Visible = true;

				return;
			}

			// If the session quote is not the purchase quote, update and persist the session, as
			// there won't necessarily be a postback to save the session later.
			if (WebForm.CurrentSessionHistory.QuoteId != Purchasable.Quote.Id)
			{
				WebForm.CurrentSessionHistory.QuoteId = Purchasable.Quote.Id;

				WebForm.SaveSessionHistory(dataAdapterDependencies.GetServiceContext());
			}

			QuoteId.Value = Purchasable.Quote.Id.ToString();

			if (Purchasable.TotalAmount == 0)
			{
				var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
				var context = dataAdapterDependencies.GetServiceContext();
				var quote = context.CreateQuery("quote").First(q => q.GetAttributeValue<Guid>("quoteid") == Purchasable.Quote.Id);
				var adapter = new CoreDataAdapter(portal, context);
				var status = quote.GetAttributeValue<OptionSetValue>("statecode").Value;

				if (status != (int)QuoteState.Active)
				{
					adapter.SetState(quote.ToEntityReference(), (int)QuoteState.Active, (int)QuoteStatusCode.InProgressActive);
				}

				if (status != (int)QuoteState.Won)
				{
					adapter.WinQuote(quote.ToEntityReference());
				}

				adapter.CovertQuoteToSalesOrder(quote.ToEntityReference());

				dataAdapter.CompletePurchase();

				SetAttributeValuesAndSave();

				MoveToNextStep();

				return;
			}

			if (Paid)
			{
				dataAdapter.CompletePurchase();

				SetAttributeValuesAndSave();

				MoveToNextStep();

				return;
			}

			Payment.Visible = true;
			GeneralErrorMessage.Visible = false;

			if (IsPaymentError)
			{
				SetErrorFields();
			}

			SetMerchantShippingFields(ServiceContext.CreateQuery("quote").FirstOrDefault(q => q.GetAttributeValue<Guid>("quoteid") == Purchasable.Quote.Id));

			if (IsPaymentAuthorizeNet || IsPaymentDemo)
			{
				CreditCardPaymentPanel.Visible = true;
				PayPalPaymentPanel.Visible = false;

				SetMerchantFields();

				PopulateContactInfo(Contact);

				EnableTestMode(TestModeEnabled);

				PurchaseDiscounts.DataSource = Purchasable.Discounts;
				PurchaseDiscounts.DataBind();

				PurchaseItems.DataSource = Purchasable.Items.Where(item => item.IsSelected && item.Quantity > 0);
				PurchaseItems.DataBind();
			}
			else if (IsPaymentPaypal)
			{
				PayPalPaymentPanel.Visible = true;
				CreditCardPaymentPanel.Visible = false;

				PayPalPurchaseDiscounts.DataSource = Purchasable.Discounts;
				PayPalPurchaseDiscounts.DataBind();

				PayPalPurchaseItems.DataSource = Purchasable.Items.Where(item => item.IsSelected && item.Quantity > 0);
				PayPalPurchaseItems.DataBind();
			}
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
			if (Purchasable == null)
			{
				WebForm.EnableDisableNextButton(false);
			}
		}

		protected override void OnSubmit(object sender, WebFormSubmitEventArgs e)
		{
			if (!IsPaymentPaypal || Paid)
			{
				return;
			}

			if (Purchasable == null)
			{
				throw new InvalidOperationException("Unable to retrieve the purchase information.");
			}

			var total = Purchasable.TotalAmount;

			if (total < 0)
			{
				throw new InvalidOperationException("Unable to retrieve the valid purchase total value.");
			}

			HandlePaypalPayment(total);
		}

		protected virtual void SetAttributeValuesAndSave()
		{
			if (CurrentStepEntityID == Guid.Empty)
			{
				return;
			}

			using (var serviceContext = PortalCrmConfigurationManager.CreateServiceContext())
			{
				var currentStepEntityUpdate = new Entity(CurrentStepEntityLogicalName)
				{
					Id = CurrentStepEntityID
				};

				SetAttributeValuesAndSave(serviceContext, currentStepEntityUpdate);
			}
		}

		protected void SetMerchantFields()
		{
#if AUTHORIZENET
			if (Purchasable == null)
			{
				throw new InvalidOperationException("Unable to retrieve the purchase information.");
			}

			var apiLogin = ServiceContext.GetSiteSettingValueByName(Website, "Ecommerce/Authorize.Net/ApiLogin");
			var transactionKey = ServiceContext.GetSiteSettingValueByName(Website, "Ecommerce/Authorize.Net/TransactionKey");

			var amount = Purchasable.TotalAmount;

			var sequence = IsPaymentDemo
				? null
				: Crypto.GenerateSequence();

			var timestamp = Crypto.GenerateTimestamp();

			var fingerprint = IsPaymentDemo
				? null
				: Crypto.GenerateFingerprint(transactionKey, apiLogin, amount, sequence, timestamp.ToString(CultureInfo.InvariantCulture));

			FingerprintHash = fingerprint;
			FingerprintSequence = sequence;
			FingerprintTimestamp = timestamp.ToString(CultureInfo.InvariantCulture);
			ApiLogin = apiLogin;
			Amount = amount.ToString(CultureInfo.InvariantCulture);
			DecimalAmount = amount;
			Tax = Purchasable.TotalTax.ToString(CultureInfo.InvariantCulture);
			RelayResponse = "TRUE";

			OrderID = new Dictionary<string, string>
			{
				{"LogicalName", "quote"},
				{"Id", Purchasable.Quote.Id.ToString()},
			}.ToQueryString();

			LineItems = Purchasable.Items
				.Where(item => item.IsSelected && item.Quantity > 0)
				.Take(30) // This is the maximum number of line items Authorize.Net supports.
				.Select(item => "{0}<|>{1}<|>{2}<|>{3:F}<|>{4:F}<|>{5}".FormatWith(
					Truncate(item.QuoteProduct.Id.ToString(), 31),
					Truncate(item.Name, 31),
					Truncate(item.Description, 255),
					item.Quantity,
					item.PricePerUnit,
					item.Tax > 0 ? "Y" : "N"));

			var returnUrl = new UrlBuilder(Request.Url.PathAndQuery);

			returnUrl.QueryString.Set("sessionid", WebForm.CurrentSessionHistory.Id.ToString());

			var handlerPath = Url.RouteUrl("PaymentHandler", new
			{
				ReturnUrl = returnUrl.PathWithQueryString,
				area = "_services"
			});

			var baseUrl = GetBaseUrl();

			RelayURL = !string.IsNullOrWhiteSpace(baseUrl)
				? ((Request.IsSecureConnection) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter + baseUrl +
				  handlerPath
				: ((Request.IsSecureConnection) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter + Request.Url.Authority +
				  handlerPath;

			BtnSubmit.PostBackUrl = GetFormAction(TestModeEnabled);
#else
			ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to configure Authorize.Net payment form. AuthorizeNet.dll and AuthorizeNet.Helpers.dll could not be found.");
#endif
		}

		private static string Truncate(string value, int maxLength)
		{
			if (value == null)
			{
				return null;
			}

			return value.Length <= maxLength ? value : value.Substring(0, maxLength);
		}

		protected void EnableTestMode(bool enable)
		{
			TestModePanel.Visible = enable;

			if (!enable)
			{
				return;
			}

			CreditCardNumber = TestCreditCardNumber;
			ExpiryMonthDefault.Value = TestCreditCardExpiryMonth;
			ExpiryYearDefault.Value = TestCreditCardExpiryYear;
			CreditCardExpiry = TestCreditCardExpiry;
			CreditCardVerificationValue = TestCreditCardVerificationValue;
		}

		protected string GetFormAction()
		{
			return GetFormAction(TestModeEnabled);
		}

		protected string GetFormAction(bool testModeEnabled)
		{
			if (IsPaymentDemo)
			{
				return RelayURL;
			}

#if AUTHORIZENET
			if (testModeEnabled)
			{
				return string.IsNullOrEmpty(PostBackUrl) ? Gateway.TEST_URL : PostBackUrl;
			}

			return string.IsNullOrEmpty(PostBackUrl) ? Gateway.LIVE_URL : PostBackUrl;
#else
			ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to get Authorize.Net Gateway URL. AuthorizeNet.dll and AuthorizeNet.Helpers.dll could not be found.");

			return PostBackUrl;
#endif
		}

		protected void PopulateContactInfo(Entity contact)
		{
			if (contact == null)
			{
				return;
			}

			FirstName = contact.GetAttributeValue<string>("firstname");
			LastName = contact.GetAttributeValue<string>("lastname");
			Email = contact.GetAttributeValue<string>("emailaddress1");
			Address = contact.GetAttributeValue<string>("address1_line1");
			City = contact.GetAttributeValue<string>("address1_city");
			Province = contact.GetAttributeValue<string>("address1_stateorprovince");
			Country = contact.GetAttributeValue<string>("address1_country");
			PostalCode = contact.GetAttributeValue<string>("address1_postalcode");
		}

		protected void SetMerchantShippingFields(Entity quote)
		{
			if (quote == null)
			{
				return;
			}

			ShippingAddress = quote.GetAttributeValue<string>("shipto_line1");
			ShippingCity = quote.GetAttributeValue<string>("shipto_city");
			ShippingProvince = quote.GetAttributeValue<string>("shipto_stateorprovince");
			ShippingCountry = quote.GetAttributeValue<string>("shipto_country");
			ShippingPostalCode = quote.GetAttributeValue<string>("shipto_postalcode");
		}

		protected void SetErrorFields()
		{
			if (!string.IsNullOrEmpty(AuthorizeNetError))
			{
				AuthorizeNetErrorMessage.Text = AuthorizeNetError;

				AuthorizeNetErrorPanel.Visible = true;
			}
			else
			{
				AuthorizeNetErrorPanel.Visible = false;
			}
		}

		private void HandlePaypalPayment(decimal total)
		{
			var payPal = new PayPalHelper(Portal);

			var currencyCode = Html.Setting("Ecommerce/Paypal/CurrencyCode");

			// Paypal Item Data for aggregateddata
			var aggregateData = Html.BooleanSetting("Ecommerce/Paypal/aggregateData").GetValueOrDefault(false);
			var itemizedData = Html.BooleanSetting("Ecommerce/Paypal/itemizedData").GetValueOrDefault(true);
			var addressOverride = Html.BooleanSetting("Ecommerce/Paypal/AddressOverride").GetValueOrDefault(true);

			HandlePayPalRedirection(payPal, total, payPal.PayPalAccountEmail, currencyCode, itemizedData, aggregateData, addressOverride);
		}

		private bool IsPaymentProvider(string provider)
		{
			var settingValue = ServiceContext.GetSiteSettingValueByName(Website, "Ecommerce/PaymentProvider") ?? "Demo";

			return string.Equals(settingValue, provider, StringComparison.InvariantCultureIgnoreCase);
		}

		private string GetBaseUrl()
		{
			return ServiceContext.GetSiteSettingValueByName(Website, "BaseURL");
		}

		/// <summary>
		/// Redirects the current request to the PayPal site by passing a querystring.
		/// PayPal then should return to this page with ?PayPal=Cancel or ?PayPal=Success
		/// This routine stores all the form vars so they can be restored later
		/// </summary>
		private void HandlePayPalRedirection(PayPalHelper payPal, decimal total, string accountEmail, string currencyCode, bool itemizedData, bool aggregateData, bool addressOverride)
		{
			var args = GetPaypalArgs(total, accountEmail, itemizedData, currencyCode, aggregateData, false, addressOverride);

			Response.Redirect(payPal.GetSubmitUrl(args));
		}

		/// <summary>
		/// Creates the dictionary of arguments for constructing the query string to send to PayPal.
		/// </summary>
		private Dictionary<string, string> GetPaypalArgs(decimal total, string accountEmail, bool itemizedData, string currencyCode, bool aggregateData, bool sendPaypalAddress, bool addressOverride)
		{
			if (Purchasable == null)
			{
				throw new InvalidOperationException("Unable to retrieve the purchase information.");
			}

			var args = new Dictionary<string, string>
			{
				{ "cmd", "_cart" },
				{ "upload", "1" },
				{ "business", accountEmail },
				{ "no_note", "1" },
				{ "invoice", Purchasable.Quote.Id.ToString() },
				{ "email", accountEmail }
			};

			if (addressOverride)
			{
				args.Add("address_override", "1");
				args.Add("first_name", Contact.GetAttributeValue<string>("firstname"));
				args.Add("last_name", Contact.GetAttributeValue<string>("lastname"));
				args.Add("address1", ShippingAddress);
				args.Add("city", ShippingCity);
				args.Add("state", ShippingProvince);
				args.Add("zip", ShippingPostalCode);
				args.Add("country", ShippingCountry);
			}

			if (!string.IsNullOrEmpty(currencyCode))
			{
				args.Add("currency_code", currencyCode);
			}

			if (aggregateData)
			{
				args.Add("item_name", "Aggregated Items");
				args.Add("amount", total.ToString("#.00"));
			}
			// Paypal Item Data for itemized data.
			else if (itemizedData)
			{
				var counter = 0;
				decimal itemDiscountsTotal = 0;

				foreach (var item in Purchasable.Items)
				{
					if (!item.IsSelected || item.Quantity < 0)
					{
						continue;
					}

					counter++;

					args.Add(string.Format("item_name_{0}", counter), item.Name);
					args.Add(string.Format("amount_{0}", counter), (item.PricePerUnit).ToString("#.00"));
					args.Add(string.Format("quantity_{0}", counter), Convert.ToInt32(item.Quantity).ToString(CultureInfo.InvariantCulture));
					args.Add(string.Format("discount_amount_{0}", counter), (item.TotalDiscountAmount).ToString("#.00"));
					itemDiscountsTotal = itemDiscountsTotal + item.TotalDiscountAmount;
					args.Add(string.Format("item_number_{0}", counter), item.QuoteProduct.Id.ToString());
				}

				if (Purchasable.ShippingAmount > 0)
				{
					args.Add("shipping", Purchasable.ShippingAmount.ToString("#.00"));
				}

				if (Purchasable.TotalTax > 0)
				{
					args.Add("tax_cart", Purchasable.TotalTax.ToString("#.00"));
				}

				if (Purchasable.TotalPreShippingAmount < Purchasable.TotalLineItemAmount)
				{
					// This variable overrides any individual item discount_amount_x values, if present therefore we must include the computed item discounts in the total.
					args.Add("discount_amount_cart", (Purchasable.TotalLineItemAmount - Purchasable.TotalPreShippingAmount + itemDiscountsTotal).ToString("#.00"));
				}
			}
			
			var cancelUrl = new UrlBuilder(Request.Url.PathAndQuery);

			cancelUrl.QueryString.Set("PayPal", "Cancel");

			var returnUrl = new UrlBuilder(Request.Url.PathAndQuery);

			returnUrl.QueryString.Set("sessionid", WebForm.CurrentSessionHistory.Id.ToString());
			returnUrl.QueryString.Set("quoteid", Purchasable.Quote.Id.ToString());
			
			var handlerPath = Url.RouteUrl("PaymentHandler", new
			{
				ReturnUrl = returnUrl.PathWithQueryString,
				area = "_services"
			});

			var baseUrl = GetBaseUrl();

			var successUrl = !string.IsNullOrWhiteSpace(baseUrl)
				? ((Request.IsSecureConnection) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter + baseUrl +
				  handlerPath
				: ((Request.IsSecureConnection) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter +
				  Request.Url.Authority + handlerPath;
			
			var cancelReturnUrl = !string.IsNullOrWhiteSpace(baseUrl)
				? ((Request.IsSecureConnection) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter + baseUrl + cancelUrl.PathWithQueryString
				: ((Request.IsSecureConnection) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter + Request.Url.Authority + cancelUrl.PathWithQueryString;
			
			args.Add("return", successUrl);
			args.Add("cancel_return", cancelReturnUrl);

			return args;
		}
	}
}
