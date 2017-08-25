/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web
{
	public static class UrlMapping
	{
		public static Entity LookupPageByUrlPath(OrganizationServiceContext context, Entity website, string urlPath)
		{
			var applicationPath = GetApplicationPath(urlPath);

			CrmEntityInactiveInfo inactiveInfo;

			var filter = CrmEntityInactiveInfo.TryGetInfo("adx_webpage", out inactiveInfo)
				? entity => !inactiveInfo.IsInactive(entity)
				: new Func<Entity, bool>(entity => true);

			return LookupPageByUrlPath(context, website, applicationPath.PartialPath, filter);
		}

		private static ApplicationPath GetApplicationPath(string rawUrl)
		{
			var appRelativePath = VirtualPathUtility.ToAppRelative(rawUrl);

			return VirtualPathUtility.IsAppRelative(appRelativePath)
				? ApplicationPath.FromAppRelativePath(appRelativePath)
				: ApplicationPath.FromPartialPath(appRelativePath);
		}

		private static Entity LookupPageByUrlPath(OrganizationServiceContext context, Entity website, string urlPath, Func<Entity, bool> predicate)
		{
			website.AssertEntityName("adx_website");

			if (website.Id == Guid.Empty) throw new NullReferenceException("Unable to retrieve the Id of the website.  Lookup failed.");

			var urlWithoutWebsitePath = WebsitePathUtility.ToRelative(website, urlPath);

			var webPages = website.GetRelatedEntities(context, "adx_website_webpage").Where(predicate);

			// Check if we can find a page with the exact URL.
			var page = webPages.Where(wp => string.Compare(wp.GetAttributeValue<string>("adx_partialurl"), urlWithoutWebsitePath, StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();

			if (page != null)
			{
				// We found the page (probably root).
				return page;
			}

			string parentPath;
			string thisPath;

			if (ParseParentPath(urlWithoutWebsitePath, out parentPath, out thisPath))
			{
				var parentPage = LookupPageByUrlPath(context, website, parentPath, predicate);

				if (parentPage != null)
				{
					page = context.GetChildPages(parentPage).Where(p => predicate(p) && string.Equals(p.GetAttributeValue<string>("adx_partialurl"), thisPath, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

					if (page != null)
					{
						return page;
					}
				}
			}

			return null;
		}

		public static Entity LookupFileByUrlPath(OrganizationServiceContext context, Entity website, string urlPath)
		{
			CrmEntityInactiveInfo inactiveInfo;

			var filter = CrmEntityInactiveInfo.TryGetInfo("adx_webfile", out inactiveInfo)
				? entity => !inactiveInfo.IsInactive(entity)
				: new Func<Entity, bool>(entity => true);

			return LookupFileByUrlPath(context, website, urlPath, filter);
		}

		private static Entity LookupFileByUrlPath(OrganizationServiceContext context, Entity website, string urlPath, Func<Entity, bool> predicate)
		{
			website.AssertEntityName("adx_website");

			if (website.Id == Guid.Empty) throw new NullReferenceException("Unable to retrieve the Id of the website.  Lookup failed.");

			var urlWithoutWebsitePath = WebsitePathUtility.ToRelative(website, urlPath);

			string parentPath;
			string thisPath;

			if (ParseParentPath(urlWithoutWebsitePath, out parentPath, out thisPath))
			{
				var parentPage = LookupPageByUrlPath(context, website, parentPath);

				if (parentPage != null)
				{
					var file = context.GetChildFiles(parentPage).Where(f => predicate(f) && string.Equals(f.GetAttributeValue<string>("adx_partialurl"), thisPath, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

					if (file != null)
					{
						return file;
					}
				}
			}

			return null;
		}

		private static bool ParseParentPath(string path, out string parentPath, out string lastPathComponent)
		{
			parentPath = null;
			lastPathComponent = null;

			if (path == "/") return false;

			var lastIndex = path.LastIndexOf('/');

			if (lastIndex >= 0)
			{
				var newPath = path.Remove(lastIndex);

				if (newPath == string.Empty) 
				{
					parentPath = "/";
					lastPathComponent = path.Remove(0, 1);
					return true;
				}

				parentPath = newPath;
				lastPathComponent = path.Remove(0, newPath.Length + 1);

				return true;
			}

			return false;
		}
	}
}
