/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Commerce
{
	public class PayPalPaymentDataTransferHandler : PaymentHandler
	{
		public PayPalPaymentDataTransferHandler(string portalName) : base(portalName) { }

		protected override void HandleSuccessfulPayment(HttpContext context, Tuple<Guid, string> quoteAndReturnUrl)
		{
			var returnUrl = new UrlBuilder(quoteAndReturnUrl.Item2);

			returnUrl.QueryString.Set("Payment", "Successful");

			context.Response.Redirect(returnUrl.PathWithQueryString, false);

			context.ApplicationInstance.CompleteRequest();
		}

		protected override void HandleUnsuccessfulPayment(HttpContext context, Tuple<Guid, string> quoteAndReturnUrl, string errorMessage)
		{
			var returnUrl = new UrlBuilder(quoteAndReturnUrl.Item2);

			returnUrl.QueryString.Set("Payment", "Unsuccessful");
			returnUrl.QueryString.Set("PayPalError", errorMessage);

			context.Response.Redirect(returnUrl.PathWithQueryString, false);

			context.ApplicationInstance.CompleteRequest();
		}

		protected override bool TryGetQuoteAndReturnUrl(HttpRequest request, IDataAdapterDependencies dataAdapterDependencies, out Tuple<Guid, string> quoteAndReturnUrl)
		{
			quoteAndReturnUrl = null;

			Guid quoteGuid;

			var returnUrl = request.QueryString["ReturnUrl"];

			if (string.IsNullOrWhiteSpace(returnUrl))
			{
				return false;
			}

			var url = new UrlBuilder(returnUrl);
			var quoteid = url.QueryString.Get("quoteid");
			
			if (string.IsNullOrWhiteSpace(quoteid)) return false;

			if (Guid.TryParse(quoteid, out quoteGuid))
			{
				quoteAndReturnUrl = new Tuple<Guid, string>(quoteGuid, returnUrl);

				return true;
			}

			return false;
		}

		protected override IPaymentValidation ValidatePayment(HttpContext context, IDataAdapterDependencies dataAdapterDependencies, Tuple<Guid, string> quoteAndReturnUrl)
		{
			var transactionId = context.Request["tx"];
			var data = context.Request.QueryString.ToString();

			if (string.IsNullOrWhiteSpace(transactionId))
			{
				return new UnsuccessfulPaymentValidation(data, ResourceManager.GetString("TransactionID_Parameter_Not_Provided_Validation_Message"));
			}

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var identityToken = PayPalHelper.GetPayPalPdtIdentityToken(portalContext);

			if (string.IsNullOrWhiteSpace(identityToken))
			{
				return new UnsuccessfulPaymentValidation(data, ResourceManager.GetString("Required_Site_Setting_Not_Exist_Validation_Message"));
			}

			var payPalHelper = new PayPalHelper(portalContext);
			var response = payPalHelper.GetPaymentDataTransferResponse(identityToken, transactionId);

			switch (response.Status)
			{
				case PayPalHelper.PaymentDataTransferStatus.Unknown:
					return new UnsuccessfulPaymentValidation(data,
						ResourceManager.GetString("Null_PDT_Response"));
				case PayPalHelper.PaymentDataTransferStatus.Fail:
					return new UnsuccessfulPaymentValidation(data,
						ResourceManager.GetString("PDT_Returned_Fail_Response"));
				case PayPalHelper.PaymentDataTransferStatus.Success:
					//if (!payPalHelper.VerifyIPNOrder(new Dictionary<string, string>(), portalContext))
					if (!payPalHelper.VerifyIPNOrder(response.Details, portalContext))
					{
						string paymentStatus;
						response.Details.TryGetValue("payment_status", out paymentStatus);
						return new UnsuccessfulPaymentValidation(data, paymentStatus);
					}
					return new SuccessfulPaymentValidation(data);
				default:
					return new UnsuccessfulPaymentValidation(data,
						ResourceManager.GetString("PDT_Response_Not_Valid"));
			}
		}

		protected override bool TryGetReceiptNumber(HttpContext context, out string receiptNumber)
		{
			receiptNumber = string.Empty;
			var transactionId = context.Request["tx"];

			if (string.IsNullOrWhiteSpace(transactionId))
			{
				return false;
			}

			receiptNumber = transactionId;

			return true;
		}
	}
}
