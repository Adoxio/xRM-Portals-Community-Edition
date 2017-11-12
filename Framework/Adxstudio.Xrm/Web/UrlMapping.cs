/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// Provides for lookup of adx_webpage and adx_webfile <see cref="Entity">entities</see> by URL path.
	/// </summary>
	public static class UrlMapping
	{
		/// <summary>
		/// Searches for a adx_webpage that matches a given <paramref name="urlPath"/>.
		/// </summary>
		/// <param name="context">
		/// The <see cref="OrganizationServiceContext"/> to be used by the lookup operation for CRM data access.
		/// </param>
		/// <param name="website">The adx_website to which the search will be scoped.</param>
		/// <param name="urlPath">The URL path to match.</param>
		/// <returns>
		/// An adx_webpage <see cref="Entity"/> whose adx_partial URL, and those of its parent pages, matches <paramref name="urlPath"/>. If no
		/// match is found, this method will return null.
		/// </returns>
		public static UrlMappingResult<Entity> LookupPageByUrlPath(OrganizationServiceContext context, Entity website, string urlPath)
		{
			var applicationPath = GetApplicationPath(urlPath);

			CrmEntityInactiveInfo inactiveInfo;

			var filter = CrmEntityInactiveInfo.TryGetInfo("adx_webpage", out inactiveInfo)
				? entity => !inactiveInfo.IsInactive(entity)
				: new Func<Entity, bool>(entity => true);

			return LookupPageByUrlPath(context, website, applicationPath.PartialPath, filter);
		}

		/// <summary>
		/// Searches for a adx_webfile that matches a given <paramref name="urlPath"/>.
		/// </summary>
		/// <param name="context">
		/// The <see cref="OrganizationServiceContext"/> to be used by the lookup operation for CRM data access.
		/// </param>
		/// <param name="website">The adx_website to which the search will be scoped.</param>
		/// <param name="urlPath">The URL path to match.</param>
		/// <returns>
		/// An adx_webfile <see cref="Entity"/> whose adx_partial URL, and those of its parent pages, matches <paramref name="urlPath"/>. If no
		/// match is found, this method will return null.
		/// </returns>
		public static Entity LookupFileByUrlPath(OrganizationServiceContext context, Entity website, string urlPath)
		{
			return Microsoft.Xrm.Portal.Web.UrlMapping.LookupFileByUrlPath(context, website, urlPath);
		}

		internal static ApplicationPath GetApplicationPath(string rawUrl)
		{
			var appRelativePath = VirtualPathUtility.ToAppRelative(rawUrl);

			return VirtualPathUtility.IsAppRelative(appRelativePath)
				? ApplicationPath.FromAppRelativePath(appRelativePath)
				: ApplicationPath.FromPartialPath(appRelativePath);
		}

		internal static readonly Regex _pagePathRegex = new Regex(@"
			^                        # Match the start of the path string.
				(?<full>             # This captures the full path match, which must include an end '/'. This will also capture the root path '/'.
					(?<parent>/.*)?  # Optionally capture a parent path, which must begin with '/'. This capture will either contain a parent path starting with '/', or will be empty in the case of a root path ('/') match.
					(?<child>[^/]*)  # Match the child (right-most path segment) path. This will either contain some number of non-/ characters, or be empty, in the case of a root path ('/') match.
				/)                   # Match the trailing '/' required on a web page path.
			$                        # Match the end of the path string.
			",
			RegexOptions.RightToLeft // This regex matches right-to-left, so that the child path segment is captured first/more greedily.
				| RegexOptions.Compiled
				| RegexOptions.IgnorePatternWhitespace
				| RegexOptions.CultureInvariant);

		private static UrlMappingResult<Entity> LookupPageByUrlPath(OrganizationServiceContext context, Entity website, string urlPath, Func<Entity, bool> predicate)
		{
			website.AssertEntityName("adx_website");

			if (website.Id == Guid.Empty)
			{
				throw new ArgumentException(string.Format("Unable to retrieve the Id of the website. {0}", string.Empty), "website");
			}

			var urlWithoutWebsitePath = WebsitePathUtility.ToRelative(website, urlPath);

			if (!context.IsAttached(website))
			{
				context.ReAttach(website);
			}

			var pages = website.GetRelatedEntities(context, "adx_website_webpage").Where(predicate).ToArray();

			// Use _pagePathRegex to extract the right-most path segment (child path), and the remaining left
			// part of the path (parent path), while enforcing that web page paths must end in a '/'.
			var pathMatch = _pagePathRegex.Match(urlWithoutWebsitePath);

			if (!pathMatch.Success)
			{
				// If we don't have a valid path match, still see if there is a page with the entire literal
				// path as its partial URL. (The previous iteration of this method has this behaviour, so we
				// maintain it here.)
				return GetResultFromQuery(pages.Where(p => IsPartialUrlMatch(p, urlWithoutWebsitePath)));
			}

			var fullPath = pathMatch.Groups["full"].Value;
			var parentPath = pathMatch.Groups["parent"].Value;
			var childPath = pathMatch.Groups["child"].Value;

			// Check if we can find a page with the exact fullPath match. This may be a web page with a
			// partial URL that matches the entire path, but in the more common case, it will match the
			// root URL path "/".
			var fullPathMatchPageResult = GetResultFromQuery(pages.Where(p => IsPartialUrlMatch(p, fullPath)));

			if (fullPathMatchPageResult.Node != null)
			{
				return fullPathMatchPageResult;
			}

			// If we don't have non-empty parentPath and childPath, lookup fails.
			if (string.IsNullOrEmpty(parentPath) || string.IsNullOrEmpty(childPath))
			{
				return UrlMappingResult<Entity>.MatchResult(null);
			}

			// Look up the parent page, using the parent path. This will generally recurse until parentPath
			// is the root path "/", at which point fullPath will match the Home page and the recursion will
			// unwind.
			var parentPageFilterResult = LookupPageByUrlPath(context, website, parentPath, predicate);

			// If we can't find a parent page, lookup fails.
			if (parentPageFilterResult.Node == null)
			{
				return parentPageFilterResult;
			}

			// Look for a partial URL match for childPath, among the children of the returned parent page.
			var query = context.GetChildPages(parentPageFilterResult.Node).Where(p => predicate(p) && IsPartialUrlMatch(p, childPath));
			return GetResultFromQuery(query);
		}

		private static bool IsPartialUrlMatch(Entity webPage, string path)
		{
			return webPage != null
				&& string.Equals(webPage.GetAttributeValue<string>("adx_partialurl"), path, StringComparison.InvariantCultureIgnoreCase);
		}

		private static UrlMappingResult<Entity> GetResultFromQuery(IEnumerable<Entity> query)
		{
			// select only root pages or pages where isroot = null 
			var duplicateCheckArray = query.Where(p => p.GetAttributeValue<bool?>("adx_isroot") != false).ToArray();
			var result = query.FirstOrDefault();
			return duplicateCheckArray.Length > 1
				? UrlMappingResult<Entity>.DuplicateResult(result)
				: UrlMappingResult<Entity>.MatchResult(result);
		}
	}
}
