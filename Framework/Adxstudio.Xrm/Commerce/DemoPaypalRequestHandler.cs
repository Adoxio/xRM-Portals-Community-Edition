/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.Routing;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Caching;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Portal.Cms;
using Adxstudio.Xrm.Web.Handlers;
using Microsoft.Xrm.Portal;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Commerce
{
	public class DemoPaypalRequestHandler : PaymentHandler
	{
		internal string _portalName;

		public new bool IsReusable
		{
			get { return true; }
		}

		public DemoPaypalRequestHandler(string portalName) : base(portalName) { }

		protected override void HandleSuccessfulPayment(HttpContext context, Tuple<Guid, string> quoteAndReturnUrl)
		{
			var redirectUrl = new UrlBuilder(quoteAndReturnUrl.Item2);

			redirectUrl.QueryString.Set("Payment", "Successful");

			context.Response.Redirect(redirectUrl.PathWithQueryString);
		}

		protected override void HandleUnsuccessfulPayment(HttpContext context, Tuple<Guid, string> quoteAndReturnUrl, string errorMessage)
		{
			var redirectUrl = new UrlBuilder(quoteAndReturnUrl.Item2);

			redirectUrl.QueryString.Set("Payment", "Unsuccessful");
			redirectUrl.QueryString.Set("AuthorizeNetError", errorMessage);

			context.Response.Redirect(redirectUrl.PathWithQueryString);
		}

		protected override bool TryGetQuoteAndReturnUrl(HttpRequest request, IDataAdapterDependencies dataAdapterDependencies, out Tuple<Guid, string> quoteAndReturnUrl)
		{
			quoteAndReturnUrl = null;

			Guid quoteId;

			//Get the original request parameters

			var incomingReqVariables = request.QueryString;

			if (Guid.TryParse(incomingReqVariables["invoice"], out quoteId))
			{
				quoteAndReturnUrl = new Tuple<Guid, string>(quoteId, incomingReqVariables["return"]);

				return true;
			}

			return false;
		}

		protected override IPaymentValidation ValidatePayment(HttpContext context, IDataAdapterDependencies dataAdapterDependencies, Tuple<Guid, string> quoteAndReturnUrl)
		{
			return new SuccessfulPaymentValidation(ResourceManager.GetString("Payment_Always_Successful_Message"));
		}

		protected override bool TryGetReceiptNumber(HttpContext context, out string receiptNumber)
		{
			receiptNumber = Guid.NewGuid().ToString();

			return true;
		}
	}
}
