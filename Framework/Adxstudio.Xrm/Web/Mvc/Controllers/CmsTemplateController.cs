/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using Adxstudio.Xrm.Cms.Security;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.Mvc.Liquid;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Mvc.Controllers
{
	/// <summary>
	/// Provides services for metadata and preview rendering for CMS (Liquid) templates.
	/// </summary>
	[PortalView]
	public class CmsTemplateController : Controller
	{
		[HttpGet]
		public ActionResult GetAll(string entityLogicalName, Guid id, string currentSiteMapNodeUrl, string context = null)
		{
			if (!Authorized(entityLogicalName, id, context))
			{
				return new JContainerResult(new JArray());
			}

			var templates = GetTemplateFileSystem()
				.GetTemplateFiles()
				.OrderBy(template => template.Name)
				.Select(template => new JObject {
					{ "name", template.Name },
					{ "title", template.Title },
					{ "description", template.Description },
					{ "include", string.IsNullOrEmpty(template.DefaultArguments)
						? "{{% include '{0}' %}}".FormatWith(template.Name)
						: "{{% include '{0}' {1} %}}".FormatWith(template.Name, template.DefaultArguments)
					},
					{ "url", Url.RouteUrl("CmsTemplate_Get", new
					{
						encodedName = EncodeTemplateName(template.Name),
						context
					}) },
					{ "preview_url", Url.RouteUrl("CmsTemplate_GetPreview", new
					{
						encodedName = EncodeTemplateName(template.Name),
						__currentSiteMapNodeUrl__ = currentSiteMapNodeUrl,
						context
					}) },
					{ "live_preview_url", Url.RouteUrl("CmsTemplate_GetLivePreview", new
					{
						__currentSiteMapNodeUrl__ = currentSiteMapNodeUrl,
						context
					}) }
				});

			return new JContainerResult(new JArray(templates));
		}

		[HttpGet]
		public ActionResult Get(string entityLogicalName, Guid id, string encodedName, string context = null)
		{
			if (!Authorized(entityLogicalName, id, context))
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}

			var name = DecodeTemplateName(encodedName);

			var fileSystem = GetTemplateFileSystem();

			string template;

			return fileSystem.TryReadTemplateFile(name, out template)
				? TemplateContent(template)
				: HttpNotFound();
		}

		[HttpGet]
		[OutputCache(CacheProfile = "User")]
		public ActionResult GetPreview(string entityLogicalName, Guid id, string encodedName, string context = null)
		{
			if (!Authorized(entityLogicalName, id, context))
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}

			var name = DecodeTemplateName(encodedName);

			var fileSystem = GetTemplateFileSystem();

			string template;

			return fileSystem.TryReadTemplateFile(name, out template)
				? TemplatePreviewContent(template)
				: HttpNotFound();
		}

		[HttpPost]
		[AjaxValidateAntiForgeryToken, SuppressMessage("ASP.NET.MVC.Security", "CA5332:MarkVerbHandlersWithValidateAntiforgeryToken", Justification = "Handled with the custom attribute AjaxValidateAntiForgeryToken")]
		public ActionResult GetLivePreview(string entityLogicalName, Guid id, string source, string context = null)
		{
			if (!Authorized(entityLogicalName, id, context))
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}

			return TemplatePreviewContent(source ?? string.Empty);
		}

		private bool Authorized(string entityLogicalName, Guid id, string context = null)
		{
			var portalViewContext = ViewData[PortalExtensions.PortalViewContextKey] as IPortalViewContext;

			if (portalViewContext == null)
			{
				return false;
			}

			using (var serviceContext = portalViewContext.CreateServiceContext())
			{
				var response = (RetrieveResponse)serviceContext.Execute(new RetrieveRequest
				{
					ColumnSet = new ColumnSet(true),
					Target = new EntityReference(entityLogicalName, id)
				});

				if (response.Entity == null)
				{
					return false;
				}

				var entity = serviceContext.MergeClone(response.Entity);

				if (entity.ToEntityReference().Equals(portalViewContext.Website.EntityReference) && context != null)
				{
					if (string.Equals(context, "adx_contentsnippet", StringComparison.OrdinalIgnoreCase))
					{
						return portalViewContext.WebsiteAccessPermissionProvider.TryAssert(serviceContext, WebsiteRight.ManageContentSnippets);
					}

					if (string.Equals(context, "adx_sitemarker", StringComparison.OrdinalIgnoreCase))
					{
						return portalViewContext.WebsiteAccessPermissionProvider.TryAssert(serviceContext, WebsiteRight.ManageSiteMarkers);
					}

					if (string.Equals(context, "adx_weblinkset", StringComparison.OrdinalIgnoreCase)
						|| string.Equals(context, "adx_weblink", StringComparison.OrdinalIgnoreCase))
					{
						return portalViewContext.WebsiteAccessPermissionProvider.TryAssert(serviceContext, WebsiteRight.ManageWebLinkSets);
					}
				}

				var securityProvider = portalViewContext.CreateCrmEntitySecurityProvider();

				return securityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Change);
			}
		}

		private IComposableFileSystem GetTemplateFileSystem()
		{
			var portalViewContext = ViewData[PortalExtensions.PortalViewContextKey] as IPortalViewContext;

			if (portalViewContext == null)
			{
                throw new InvalidOperationException("Unable to retrieve the portal view context.");
			}

			return new CompositeFileSystem(
				new EntityFileSystem(portalViewContext, "adx_webtemplate", "adx_name", "adx_source"),
				new EmbeddedResourceFileSystem(Assembly.GetExecutingAssembly(), "Adxstudio.Xrm.Liquid"));
		}

		private ActionResult TemplateContent(string template)
		{
			return Content(template, "text/plain");
		}

		private ActionResult TemplatePreviewContent(string template)
		{
			return View(new TemplatePreviewView(template));
		}

		private static string DecodeTemplateName(string name)
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(name));
		}

		private static string EncodeTemplateName(string name)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(name));
		}

		private class TemplatePreviewView : IView
		{
			private readonly string _template;

			public TemplatePreviewView(string template)
			{
				if (template == null) throw new ArgumentNullException("template");

				_template = template;
			}

			public void Render(ViewContext viewContext, TextWriter writer)
			{
				viewContext.HttpContext.Response.ContentType = "text/plain";

				var html = new HtmlHelper(viewContext, new ViewPage());

				html.RenderLiquid(_template, writer, new
				{
					preview = true
				});
			}
		}
	}
}
