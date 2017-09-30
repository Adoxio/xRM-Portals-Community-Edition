/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Web.Mvc
{
	public abstract class PortalViewPage : Page
	{
		private Lazy<HtmlHelper> _html;
		private Lazy<UrlHelper> _url;

		public HtmlHelper Html
		{
			get { return _html.Value; }
		}

		public UrlHelper Url
		{
			get { return _url.Value; }
		}

		protected virtual string PortalName { get; set; }

		protected override void OnInit(EventArgs args)
		{
			_html = GetLazyHtmlHelper(PortalName, Request.RequestContext, Response);
			_url = GetLazyUrlHelper(Request.RequestContext);

			base.OnInit(args);
		}

		public static Lazy<HtmlHelper> GetLazyHtmlHelper(string portalName, RequestContext requestContext, HttpResponse response)
		{
			return new Lazy<HtmlHelper>(() =>
			{
				var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName, requestContext);
				var controllerContext = new ControllerContext(requestContext, new MockController());

				if (!controllerContext.RouteData.Values.ContainsKey("controller"))
				{
					controllerContext.RouteData.Values["controller"] = "__PortalViewPage__";
				}

				var portalViewContext = new PortalViewContext(new PortalContextDataAdapterDependencies(portal));

				var htmlHelper = new HtmlHelper(new ViewContext(controllerContext, new MockView(), new ViewDataDictionary(), new TempDataDictionary(), response.Output)
				{
					ViewData = new ViewDataDictionary
					{
						{ PortalExtensions.PortalViewContextKey, portalViewContext }
					}
				}, new ViewPage());

				htmlHelper.ViewData[PortalExtensions.PortalViewContextKey] = portalViewContext;

				return htmlHelper;

			}, LazyThreadSafetyMode.None);
		}

		public static Lazy<UrlHelper> GetLazyUrlHelper(RequestContext requestContext)
		{
			return new Lazy<UrlHelper>(() => new UrlHelper(requestContext), LazyThreadSafetyMode.None);
		}

		internal class MockController : Controller { }

		internal class MockView : IView
		{
			public void Render(ViewContext viewContext, TextWriter writer) { }
		}
	}
}
