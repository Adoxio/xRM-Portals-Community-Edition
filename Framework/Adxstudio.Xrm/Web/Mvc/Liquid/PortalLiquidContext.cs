/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Metadata;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class PortalLiquidContext : IPortalLiquidContext
	{
		private readonly Lazy<IOrganizationMoneyFormatInfo> _organizationMoneyFormatInfo;
		private readonly Lazy<Random> _random;
		private readonly Lazy<UrlHelper> _urlHelper;
		private readonly Lazy<ContextLanguageInfo> _contextLanguageInfo;
		private readonly Lazy<IOrganizationService> _portalOrganizationService;

		public PortalLiquidContext(HtmlHelper html, IPortalViewContext portalViewContext)
		{
			if (html == null) throw new ArgumentNullException("html");
			if (portalViewContext == null) throw new ArgumentNullException("portalViewContext");

			Html = html;
			PortalViewContext = portalViewContext;

			_organizationMoneyFormatInfo = new Lazy<IOrganizationMoneyFormatInfo>(GetOrganizationMoneyFormatInfo, LazyThreadSafetyMode.None);
			_random = new Lazy<Random>(() => new Random(), LazyThreadSafetyMode.None);
			_urlHelper = new Lazy<UrlHelper>(GetUrlHelper, LazyThreadSafetyMode.None);
			_contextLanguageInfo = new Lazy<ContextLanguageInfo>(GetContextLanguageInfo, LazyThreadSafetyMode.None);
			_portalOrganizationService = new Lazy<IOrganizationService>(GetPortalOrganizationService, LazyThreadSafetyMode.None);
		}

		public HtmlHelper Html { get; private set; }

		public IOrganizationMoneyFormatInfo OrganizationMoneyFormatInfo
		{
			get { return _organizationMoneyFormatInfo.Value; }
		}

		public IPortalViewContext PortalViewContext { get; private set; }

		public Random Random
		{
			get { return _random.Value; }
		}

		public UrlHelper UrlHelper
		{
			get { return _urlHelper.Value; }
		}

		public ContextLanguageInfo ContextLanguageInfo
		{
			get { return _contextLanguageInfo.Value; }
		}

		public IOrganizationService PortalOrganizationService
		{
			get { return _portalOrganizationService.Value; }
		}

		private IOrganizationMoneyFormatInfo GetOrganizationMoneyFormatInfo()
		{
			return new OrganizationMoneyFormatInfo(PortalViewContext);
		}

		private static UrlHelper GetUrlHelper()
		{
			var current = HttpContext.Current;

			if (current == null)
			{
				throw new InvalidOperationException("Unable to retrieve the current HTTP context.");
			}

			var http = new HttpContextWrapper(current);
			var requestContext = new RequestContext(http, RouteTable.Routes.GetRouteData(http) ?? new RouteData());

			return new UrlHelper(requestContext, RouteTable.Routes);
		}

		private static ContextLanguageInfo GetContextLanguageInfo()
		{
			var current = HttpContext.Current;

			if (current == null)
			{
				throw new InvalidOperationException("Unable to retrieve the current HTTP context.");
			}

			var contextLanguageInfo = current.GetContextLanguageInfo();
			return contextLanguageInfo;
		}

		private static IOrganizationService GetPortalOrganizationService()
		{
			var current = HttpContext.Current;

			if (current == null)
			{
				throw new InvalidOperationException("Unable to retrieve the current HTTP context.");
			}

			var portalOrgService = current.GetOrganizationService();
			return portalOrgService;
		}
	}
}
