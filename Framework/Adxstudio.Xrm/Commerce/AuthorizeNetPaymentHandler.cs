/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Commerce
{
	public class AuthorizeNetPaymentHandler : AuthorizeNetPaymentHandlerBase
	{
		public AuthorizeNetPaymentHandler(string portalName) : base(portalName) { }

		protected override IPaymentValidation ValidatePayment(HttpContext context, IDataAdapterDependencies dataAdapterDependencies, Tuple<Guid, string> quoteAndReturnUrl)
		{
#if AUTHORIZENET
			var simResponse = new AuthorizeNet.SIMResponse(context.Request.Form);
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);


			if (!AuthorizeNetHelper.IsSIMValid(simResponse, portalContext))
			{
				return new UnsuccessfulPaymentValidation(ResourceManager.GetString("Invalid_SIM_Response_Validation_Message") + context.Request.Form, ResourceManager.GetString("Process_Payment_Failed"));
			}

			int responseCode;

			if (!int.TryParse(context.Request.Form["x_response_code"], out responseCode))
			{
				return new UnsuccessfulPaymentValidation(ResourceManager.GetString("Unable_To_Parsex_response_code") + context.Request.Form, ResourceManager.GetString("Process_Payment_Failed"));
			}

			if (responseCode != 1)
			{
				return new UnsuccessfulPaymentValidation(context.Request.Form.ToString(), context.Request.Form["x_response_reason_text"]);
			}

			return new SuccessfulPaymentValidation(context.Request.Form.ToString());
#else
			ADXTrace.Instance.TraceError(TraceCategory.Application, "Authorize.Net Payment Handler is not able to process validation. AuthorizeNet.dll and AuthorizeNet.Helpers.dll could not be found.");
			
			return new UnsuccessfulPaymentValidation(context.Request.Form.ToString(), context.Request.Form["x_response_reason_text"]);
#endif
		}

		protected override bool TryGetReceiptNumber(HttpContext context, out string receiptNumber)
		{
			receiptNumber = string.Empty;
			var receipt = context.Request.Form["x_trans_id"];

			if (string.IsNullOrWhiteSpace(receipt))
			{
				return false;
			}

			receiptNumber = receipt;
			
			return true;
		}
	}
}
