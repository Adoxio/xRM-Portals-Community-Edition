/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.IO;
using System.Web;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Commerce
{
	/// <summary>
	/// Process PayPal's Instant Payment Notification (IPN) message regarding a transaction. Since IPN is not a real-time service, our checkout flow should not wait for the IPN message before it is allowed to complete. Instead, Payment Data Transfer (PDT) is used to confirm the payment completed immediately upon the redirect back to us from PayPal. In the event that the user has successfully paid but then closes the browser before being redirected back to our site, this IPN can check if a salesorder exists for the quote then PDT confirmed the sale otherwise convert the quote to salesorder.
	/// https://developer.paypal.com/docs/classic/products/instant-payment-notification/
	/// </summary>
	public class PayPalPaymentHandler : PaymentHandler
	{
		public PayPalPaymentHandler(string portalName) : base(portalName) { }

		protected override void HandleSuccessfulPayment(HttpContext context, Tuple<Guid, string> quoteAndReturnUrl) { }

		protected override void HandleUnsuccessfulPayment(HttpContext context, Tuple<Guid, string> quoteAndReturnUrl, string errorMessage) { }

		protected override bool TryGetQuoteAndReturnUrl(HttpRequest request, IDataAdapterDependencies dataAdapterDependencies, out Tuple<Guid, string> quoteAndReturnUrl)
		{
			quoteAndReturnUrl = null;

			Guid quoteId;

			if (Guid.TryParse(request.Form["invoice"], out quoteId))
			{
				quoteAndReturnUrl = new Tuple<Guid, string>(quoteId, null);

				return true;
			}

			return false;
		}

		protected override IPaymentValidation ValidatePayment(HttpContext context, IDataAdapterDependencies dataAdapterDependencies, Tuple<Guid, string> quoteAndReturnUrl)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var payPalHelper = new PayPalHelper(portalContext);
			var requestForm = context.Request.Form.ToString();

			var response = payPalHelper.GetPaymentWebResponse(requestForm);

			if (response == null)
			{
				return new UnsuccessfulPaymentValidation(requestForm, ResourceManager.GetString("Get_Payment_Web_Response_Failed"));
			}

			var responseStream = response.GetResponseStream();

			if (responseStream == null)
			{
				return new UnsuccessfulPaymentValidation(requestForm, ResourceManager.GetString("Get_Payment_Web_Response_Stream_Failed"));
			}

			using (var reader = new StreamReader(responseStream))
			{
				var responseText = reader.ReadToEnd();

				if (!string.Equals(responseText, "VERIFIED", StringComparison.InvariantCultureIgnoreCase))
				{
					return new UnsuccessfulPaymentValidation(requestForm, responseText);
				}

				if (!payPalHelper.VerifyIPNOrder(context.Request.Form, portalContext))
				{
					return new UnsuccessfulPaymentValidation(requestForm, responseText);
				}

				return new SuccessfulPaymentValidation(requestForm);
			}
		}

		protected override bool TryGetReceiptNumber(HttpContext context, out string receiptNumber)
		{
			receiptNumber = string.Empty;
			var receipt = context.Request.Form["txn_id"];

			if (string.IsNullOrWhiteSpace(receipt))
			{
				return false;
			}

			receiptNumber = receipt;

			return true;
		}
	}
}
