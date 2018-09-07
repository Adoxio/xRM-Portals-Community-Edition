/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Cms.SolutionVersions;

namespace Adxstudio.Xrm.Web.Providers
{
	public class ContentMapEntityUrlProvider : AdxEntityUrlProvider, IContentMapEntityUrlProvider
	{
		private readonly IContentMapProvider _contentMapProvider;

		internal ContentMapEntityUrlProvider(IEntityWebsiteProvider websiteProvider, IContentMapProvider contentMapProvider, string portalName = null)
			: base(websiteProvider)
		{
			_contentMapProvider = contentMapProvider;
			PortalName = portalName;
		}

		protected new string PortalName { get; private set; }

		public override ApplicationPath GetApplicationPath(OrganizationServiceContext context, Entity entity)
		{
			return _contentMapProvider.Using(map => GetApplicationPath(context, entity, map));
		}

		/// <summary>
		/// This method is created to wrap GetApplicationPath and prepend language code to link if required
		/// </summary>
		private ApplicationPath GetApplicationPath(OrganizationServiceContext context, Entity entity, ContentMap map)
		{
			var path = GetApplicationPathBase(context, entity, map);

			if (path == null || !ContextLanguageInfo.DisplayLanguageCodeInUrl)
			{
				return path;
			}

			switch (entity.LogicalName)
			{
				case "adx_weblink":
					return string.IsNullOrWhiteSpace(path.ExternalUrl) ? ContextLanguageInfo.PrependLanguageCode(path) : path;
				case "adx_webpage":
					return ContextLanguageInfo.PrependLanguageCode(path);
			}

			return path;
		}

		private ApplicationPath GetApplicationPathBase(OrganizationServiceContext context, Entity entity, ContentMap map)
		{
			ApplicationPath result = null;

			switch (entity.LogicalName)
			{
				case "adx_weblink":
					result = GetApplicationPath(map, entity);
					//no need to continue as it will return null from the base call at this point.
					return result;
				case "adx_webpage":
					return GetApplicationPath(map, entity);
				case "adx_webfile":
					result = GetApplicationPath(map, entity);
					break;
				case "adx_shortcut":
					result = GetApplicationPath(map, entity);
					break;
				case "adx_blog":
					result = GetApplicationPath(map, entity, "adx_partialurl", "adx_parentpageid", true, true);
					break;
				case "adx_communityforum":
					result = GetApplicationPath(map, entity, "adx_partialurl", "adx_parentpageid", prependLangCode: true);
					break;
				case "adx_event":
					result = GetApplicationPath(map, entity, "adx_partialurl", "adx_parentpageid");
					break;
				case "annotation":
					result = entity.GetFileAttachmentPath();
					break;
			}

			return result ?? base.GetApplicationPath(context, entity);
		}

		private ApplicationPath GetApplicationPath(ContentMap map, Entity entity)
		{
			EntityNode node;

			if (map.TryGetValue(entity, out node))
			{
				return GetApplicationPath(map, node);
			}

			return null;
		}

		public virtual string GetUrl(ContentMap map, EntityNode node)
		{
			var applicationPath = GetApplicationPath(map, node);

			if (applicationPath != null)
			{
				return applicationPath.ExternalUrl ?? applicationPath.AbsolutePath;
			}

			return null;
		}

		public virtual ApplicationPath GetApplicationPath(ContentMap map, EntityNode node)
		{
			var link = node as WebLinkNode;

			if (link != null)
			{
				if (!string.IsNullOrWhiteSpace(link.ExternalUrl))
				{
					return ApplicationPath.FromExternalUrl(link.ExternalUrl);
				}

				ApplicationPath path = null;
				// Check .IsReference incase the linked-to root WebPage is deactivated.
				if (link.WebPage != null && !link.WebPage.IsReference)
				{
					// If language content page exists, replace web link page with the content page
					var langInfo = HttpContext.Current.GetContextLanguageInfo();
					var linkWebPage = langInfo.FindLanguageSpecificWebPageNode(link.WebPage, false);
					// If web page doesn't exist for current language, return null path
					if (linkWebPage != null)
					{
						path = GetApplicationPath(linkWebPage);
					}
				}
				return path;
			}

			var page = node as WebPageNode;

			if (page != null)
			{
				var path = GetApplicationPath(page);

				return path;
			}

			var file = node as WebFileNode;

			if (file != null)
			{
				var path = GetApplicationPath(file);

				return path;
			}

			var shortcut = node as ShortcutNode;

			if (shortcut != null)
			{
				var path = GetApplicationPath(shortcut);

				return path;
			}

			return null;
		}

		private ApplicationPath GetApplicationPath(ContentMap map, Entity entity, string partialUrlAttribute, string parentReferenceAttribute, bool trailingSlash = false, bool prependLangCode = false)
		{
			var partialUrl = entity.GetAttributeValue<string>(partialUrlAttribute);

			if (string.IsNullOrEmpty(partialUrl))
			{
				return null;
			}

			var parentReference = entity.GetAttributeValue<EntityReference>(parentReferenceAttribute);

			if (parentReference == null)
			{
				return null;
			}

			EntityNode parentNode;

			if (!map.TryGetValue(parentReference, out parentNode))
			{
				return null;
			}

			var parentPath = GetApplicationPath(map, parentNode);

			if (parentPath == null || parentPath.PartialPath == null)
			{
				return null;
			}

			partialUrl = trailingSlash && !partialUrl.EndsWith("/") ? partialUrl + "/" : partialUrl;

			var parentPartialPath = parentPath.PartialPath.EndsWith("/") ? parentPath.PartialPath : parentPath.PartialPath + "/";

			var resultAppPath = ApplicationPath.FromPartialPath(parentPartialPath + partialUrl);

			if (ContextLanguageInfo.DisplayLanguageCodeInUrl
				&& ContextLanguageInfo.IsCrmMultiLanguageEnabledInWebsite(PortalContext.Current.Website)
				&& prependLangCode)
			{
				resultAppPath = ContextLanguageInfo.PrependLanguageCode(resultAppPath);
			}

			return resultAppPath;
		}

		protected virtual string GetUrl(WebPageNode page)
		{
			var applicationPath = GetApplicationPath(page);

			if (applicationPath != null)
			{
				return applicationPath.ExternalUrl ?? applicationPath.AbsolutePath;
			}

			return null;
		}

		protected virtual string GetUrl(WebFileNode file)
		{
			var applicationPath = GetApplicationPath(file);

			if (applicationPath != null)
			{
				return applicationPath.ExternalUrl ?? applicationPath.AbsolutePath;
			}

			return null;
		}

		protected virtual string GetUrl(ShortcutNode shortcut)
		{
			var applicationPath = GetApplicationPath(shortcut);

			if (applicationPath != null)
			{
				return applicationPath.ExternalUrl ?? applicationPath.AbsolutePath;
			}

			return null;
		}

		private ApplicationPath GetApplicationPath(WebPageNode page)
		{
			var websiteRelativeUrl = InternalGetApplicationPath(page);

			if (websiteRelativeUrl == null) return null;

			var path = websiteRelativeUrl.PartialPath;
			var appPath = ApplicationPath.FromPartialPath(path);

			if (appPath.ExternalUrl != null) return appPath;

			var canonicalPath = appPath.AppRelativePath.EndsWith("/")
				? appPath
				: ApplicationPath.FromAppRelativePath("{0}/".FormatWith(appPath.AppRelativePath));

			return canonicalPath;
		}

		private ApplicationPath InternalGetApplicationPath(WebPageNode page)
		{
			var parent = page.Parent;
			var partialUrl = page.PartialUrl;
			var url = ApplicationPath.FromPartialPath(string.Empty);

			if (parent == null || parent.IsReference)
			{
				// Home page (with partial url "/") are not expected to have a parent page.
				if (partialUrl == "/")
				{
					return ApplicationPath.FromPartialPath(partialUrl);
				}

				var traceMessage = parent == null ? string.Format("Parent is Null. Page.Id = {0}", page.Id) : string.Format("Parent is Reference. Page.Id = {0}, ParentId = {1}", page.Id, parent.Id);
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, traceMessage);
				return null;
			}

			Version currentVersion = page.Website.CurrentBaseSolutionCrmVersion;
			Version centaurusVersion = BaseSolutionVersions.CentaurusVersion;

			if (currentVersion.Major >= centaurusVersion.Major && currentVersion.Minor >= centaurusVersion.Minor)
			{
				url = InternalJoinApplicationPath(parent, partialUrl);
			}

			else
			{
				if (CircularReferenceCheck(page) == true)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Circular reference with page {0}", page.Id));
					return url;
				}
				else
				{
					url = InternalJoinApplicationPath(parent, partialUrl);
				}
			}

			return url;
		}

		/// <summary>
		/// Returns application path url by joining the partial url and partial path of parent url.
		/// </summary>
		/// <param name="parent">Contains Parent page of Webpage node.</param>
		/// <param name="partialUrl">Contains partial url of Webpage node.</param>
		private ApplicationPath InternalJoinApplicationPath(WebPageNode parent, string partialUrl)
		{
			var applicationPathUrl = ApplicationPath.FromPartialPath(string.Empty);
			var parentUrl = InternalGetApplicationPath(parent);

			if (parentUrl == null)
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Parent is Null. PartialUrl = {0}", partialUrl));
				return null;
			}

			applicationPathUrl = JoinApplicationPath(parentUrl.PartialPath, partialUrl);
			return applicationPathUrl;
		}

		/// <summary>
		/// Checks for the circular reference over web pages.
		/// </summary>
		/// <param name="page">Webpage node to check for circular reference.</param>
		private bool? CircularReferenceCheck(WebPageNode page)
		{
			var pageDetails = new Dictionary<Guid, string>();

			if (page.IsCircularReference == null)
			{
				while (page.Parent != null)
				{
					if (pageDetails.ContainsKey(page.Id) && page.Id != Guid.Empty)
					{
						page.IsCircularReference = true;
						return true;
					}
					else
					{
						pageDetails.Add(page.Id, page.Name);
					}

					page = page.Parent;
				}

				page.IsCircularReference = false;
			}

			return page.IsCircularReference;
		}

		private ApplicationPath GetApplicationPath(WebFileNode file)
		{
			var websiteRelativeUrl = InternalGetApplicationPath(file);
			var path = websiteRelativeUrl.PartialPath;
			var appPath = ApplicationPath.FromPartialPath(path);

			return appPath;
		}

		private ApplicationPath InternalGetApplicationPath(WebFileNode file)
		{
			var partialUrl = file.PartialUrl;

			if (file.Parent != null)
			{
				var parentUrl = InternalGetApplicationPath(file.Parent);

				if (parentUrl == null)
				{
					return null;
				}

				return JoinApplicationPath(parentUrl.PartialPath, partialUrl);
			}

			if (file.BlogPost != null)
			{
				var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
				var blogPost = serviceContext.CreateQuery("adx_blogpost")
					.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_blogpostid") == file.BlogPost.Id);

				if (blogPost == null)
				{
					return null;
				}

				var parentUrl = GetApplicationPath(serviceContext, blogPost);

				if (parentUrl == null)
				{
					return null;
				}

				return JoinApplicationPath(parentUrl.PartialPath, partialUrl);
			}

			return ApplicationPath.FromPartialPath(partialUrl);
		}

		private ApplicationPath GetApplicationPath(ShortcutNode shortcut)
		{
			if (shortcut.WebPage != null && !shortcut.WebPage.IsReference)
			{
				return GetApplicationPath(shortcut.WebPage);
			}
			
			if (shortcut.WebFile != null && !shortcut.WebFile.IsReference)
			{
				return GetApplicationPath(shortcut.WebFile);
			}

			if (!string.IsNullOrEmpty(shortcut.ExternalUrl))
			{
				return ApplicationPath.FromExternalUrl(shortcut.ExternalUrl);
			}

			return null;
		}

		private new ApplicationPath JoinApplicationPath(string basePath, string extendedPath)
		{
			if (string.IsNullOrWhiteSpace(basePath) || basePath.Contains("?") || basePath.Contains(":") || basePath.Contains("//") || basePath.Contains("&")
				|| basePath.Contains("%3f") || basePath.Contains("%2f%2f") || basePath.Contains("%26"))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "The basePath is invalid.");

				return null;
			}

			if (string.IsNullOrWhiteSpace(extendedPath) || extendedPath.Contains("?") || extendedPath.Contains("&") || extendedPath.Contains("//")
				|| extendedPath.Contains(":") || extendedPath.Contains("%3f") || extendedPath.Contains("%2f%2f") || extendedPath.Contains("%26"))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "The extendedPath is invalid.");

				return null;
			}

			var path = "{0}/{1}".FormatWith(basePath.TrimEnd('/'), extendedPath.TrimStart('/'));

			return ApplicationPath.FromPartialPath(path);
		}
	}
}
