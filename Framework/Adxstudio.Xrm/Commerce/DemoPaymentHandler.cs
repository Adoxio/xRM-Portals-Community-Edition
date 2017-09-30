/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;

namespace Adxstudio.Xrm.Commerce
{
	public class DemoPaymentHandler : AuthorizeNetPaymentHandlerBase
	{
		public DemoPaymentHandler(string portalName) : base(portalName) { }

		protected override IPaymentValidation ValidatePayment(HttpContext context, IDataAdapterDependencies dataAdapterDependencies, Tuple<Guid, string> quoteAndReturnUrl)
		{
			return new SuccessfulPaymentValidation(context.Request.Form.ToString());
		}

		protected override bool TryGetReceiptNumber(HttpContext context, out string receiptNumber)
		{
			receiptNumber = Guid.NewGuid().ToString();

			return true;
		}
	}
}
