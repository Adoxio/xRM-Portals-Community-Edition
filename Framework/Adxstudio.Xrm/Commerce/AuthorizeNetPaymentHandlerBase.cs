/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Commerce
{
	public abstract class AuthorizeNetPaymentHandlerBase : PaymentHandler
	{
		protected AuthorizeNetPaymentHandlerBase(string portalName) : base(portalName) { }

		protected override void HandleSuccessfulPayment(HttpContext context, Tuple<Guid, string> quoteAndReturnUrl)
		{
			var returnUrl = new UrlBuilder(quoteAndReturnUrl.Item2);

			returnUrl.QueryString.Set("Payment", "Successful");

			var redirectUrl = ConvertUrlBuilderToAbsoluteUrl(context, returnUrl);

			Redirect(context, redirectUrl);
		}

		protected override void HandleUnsuccessfulPayment(HttpContext context, Tuple<Guid, string> quoteAndReturnUrl, string errorMessage)
		{
			var returnUrl = new UrlBuilder(quoteAndReturnUrl.Item2);

			returnUrl.QueryString.Set("Payment", "Unsuccessful");
			returnUrl.QueryString.Set("AuthorizeNetError", errorMessage);

			var redirectUrl = ConvertUrlBuilderToAbsoluteUrl(context, returnUrl);

			Redirect(context, redirectUrl);
		}

		private string GetBaseUrl()
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var serviceContext = portal.ServiceContext;
			return serviceContext.GetSiteSettingValueByName(portal.Website, "BaseURL");
		}

		protected virtual string ConvertUrlBuilderToAbsoluteUrl(HttpContext context, UrlBuilder url)
		{
			var baseUrl = GetBaseUrl();

			return !string.IsNullOrWhiteSpace(baseUrl)
				? ((context.Request.IsSecureConnection) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter + baseUrl +
				  url.PathWithQueryString
				: ((context.Request.IsSecureConnection) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter +
				  context.Request.Url.Authority + url.PathWithQueryString;
		}

		protected virtual void Redirect(HttpContext context, string url)
		{
			// Authorize.Net POST expects an HTML content response, JavaScript will redirect to the desired location within our website
			var response = @"
				<html>
					<head>
						<script type=""text/javascript"" charset=""utf-8"">
							window.location ='{0}';
						</script>
						<noscript>
							<meta http-equiv=""refresh"" content=""1;url={0}"">
						</noscript>
					</head>
					<body></body>
				</html>".FormatWith(url);

			context.Response.Write(response);
		}

		protected override bool TryGetQuoteAndReturnUrl(HttpRequest request, IDataAdapterDependencies dataAdapterDependencies, out Tuple<Guid, string> quoteAndReturnUrl)
		{
			quoteAndReturnUrl = null;

			var param = request.Form["order_id"];

			if (string.IsNullOrEmpty(param))
			{
				return false;
			}

			var values = HttpUtility.ParseQueryString(param);

			var logicalName = values["LogicalName"];

			if (string.IsNullOrEmpty(logicalName))
			{
				return false;
			}

			Guid id;

			if (!Guid.TryParse(values["Id"], out id))
			{
				return false;
			}

			var returnUrlParam = request.QueryString["ReturnUrl"];

			if (string.IsNullOrEmpty(returnUrlParam))
			{
				return false;
			}

			var returnUrl = new UrlBuilder(returnUrlParam);

			if (string.Equals(logicalName, "quote", StringComparison.InvariantCultureIgnoreCase))
			{
				quoteAndReturnUrl = new Tuple<Guid, string>(id, returnUrl.PathWithQueryString);
				
				return true;
			}

			if (string.Equals(logicalName, "adx_webformsession", StringComparison.InvariantCultureIgnoreCase))
			{
				var serviceContext = dataAdapterDependencies.GetServiceContext();

				var webFormSession = serviceContext.CreateQuery("adx_webformsession")
					.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_webformsessionid") == id);

				if (webFormSession == null)
				{
					return false;
				}

				var webFormSessionQuote = webFormSession.GetAttributeValue<EntityReference>("adx_quoteid");

				if (webFormSessionQuote == null)
				{
					return false;
				}

				quoteAndReturnUrl = new Tuple<Guid, string>(webFormSessionQuote.Id, returnUrl.PathWithQueryString);

				return true;
			}

			return false;
		}
	}
}
