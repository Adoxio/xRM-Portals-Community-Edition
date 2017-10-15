/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Cms;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Handlers;
using System.Web.Routing;

namespace Adxstudio.Xrm.Commerce
{
	public class PaymentProcessingRequestHandlerFactory : IHttpHandler
	{
		public string PortalName { get; set; }

		public bool IsReusable
		{
			get { return true; }
		}

		public IPortalContext Portal { get; set; }

		public PaymentProcessingRequestHandlerFactory()
		{
			Portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
		}

		public PaymentProcessingRequestHandlerFactory(RequestContext requestContext)
		{
			Portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName, requestContext);
		}

		public void ProcessRequest(HttpContext context)
		{
			var paypalRequestHandler = GetHandler(context);

			paypalRequestHandler.ProcessRequest(context);
		}

		IHttpHandler GetHandler(HttpContext context)
		{
			var requestType = context.Request.RequestType;

			var paymentProvider = Portal.ServiceContext.GetSiteSettingValueByName(Portal.Website, "Ecommerce/PaymentProvider") ?? "PayPal";
			
			if (requestType == "GET")
			{
				if (paymentProvider == "PayPal")
				{
					return new PayPalPaymentDataTransferHandler(PortalName);
				}

				return new BadConfigurationHandler(paymentProvider);
			}

			if (requestType == "POST")
			{
				if (paymentProvider == "PayPal")
				{
					return new PayPalPaymentHandler(PortalName);
				}
				if (paymentProvider == "Authorize.Net")
				{
					return new AuthorizeNetPaymentHandler(PortalName);
				}
				if (paymentProvider == "Demo")
				{
					return new DemoPaymentHandler(PortalName);
				}

				return new BadConfigurationHandler(paymentProvider);
			}

			return new PortalInvalidRequestTypeHandler();
		}
	}

	public class BadConfigurationHandler : IHttpHandler
	{
		private readonly string _paymentProviderName;

		public BadConfigurationHandler(string paymentProviderName)
		{
			_paymentProviderName = paymentProviderName;
		}

		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			throw new InvalidOperationException("Configured payment provider {0} is not supported by this handler.".FormatWith(_paymentProviderName));
		}
	}

	public class BadMethodRequestHandler : IHttpHandler
	{
		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				context.Response.ContentType = "text/plain";
				context.Response.Write("HTTP method {0} is not supported by this handler.".FormatWith(context.Request.HttpMethod));
				context.Response.End();
		}
	}
}
