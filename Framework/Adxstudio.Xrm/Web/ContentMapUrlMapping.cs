/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web
{
	using System;
	using System.Web;
	using System.Linq;
	using Adxstudio.Xrm.Cms;
	using Microsoft.Xrm.Portal.Web;
	using Adxstudio.Xrm.Resources;
	using Microsoft.Xrm.Sdk;
	using System.Collections.Generic;
	using Adxstudio.Xrm.AspNet.Cms;

	/// <summary>
	/// Provides for lookup of adx_webpage and adx_webfile <see cref="Entity">entities</see> by URL path.
	/// </summary>
	internal static class ContentMapUrlMapping
	{
		public enum WebPageLookupOptions
		{
			/// <summary>
			/// Only search for web pages that are language root (i.e. where adx_isroot = true).
			/// This is for cases where we are explicitly looking for the non-translated root page,
			/// ex: looking for a web file which only hangs off from root web pages, or the provided urlPath is from a SiteMapNode which is not language-aware so the url will be the root's.
			/// This does NOT refer to the website root "/" (aka Home) web page.
			/// </summary>
			RootOnly,
			/// <summary>
			/// Only search for translated content web pages.
			/// </summary>
			LanguageContentOnly,
			/// <summary>
			/// Search for an any web page regardless of root status (preference to root page).
			/// </summary>
			Any
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="website"></param>
		/// <param name="urlPath"></param>
		/// <param name="getRootWebPage">Whether to get the Root version of a group of translated web pages where adx_isroot = true. 
		/// This should only be set to true in specific cases where we are explicitly looking for the non-translated root page,
		/// ex: we're looking for a web file which only hangs off from root web pages, or the provided urlPath is from a SiteMapNode which is not language-aware so the url will be the root's.
		/// This does NOT refer to the website root "/" (aka Home) web page.</param>
		/// <returns></returns>
		public static UrlMappingResult<WebPageNode> LookupPageByUrlPath(WebsiteNode website, string urlPath, WebPageLookupOptions lookupOption, ContextLanguageInfo languageContext)
		{
			var applicationPath = UrlMapping.GetApplicationPath(urlPath);

			CrmEntityInactiveInfo inactiveInfo;

			var filter = CrmEntityInactiveInfo.TryGetInfo("adx_webpage", out inactiveInfo)
				? page => !inactiveInfo.IsInactive(page.ToEntity())
				: new Func<WebPageNode, bool>(entity => true);

			var result = LookupPageByUrlPath(website, applicationPath.PartialPath, lookupOption, languageContext, filter);
			
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="website"></param>
		/// <param name="urlPath"></param>
		/// <param name="getRootWebPage">Whether to get the Root version of a group of translated web pages where adx_isroot = true. 
		/// This should only be set to true in specific cases where we are explicitly looking for the non-translated root page, 
		/// ex: we're looking for a web file which only hangs off from root web pages.
		/// This does NOT refer to the root "/" web page (typically Home page).</param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		private static UrlMappingResult<WebPageNode> LookupPageByUrlPath(WebsiteNode website, string urlPath, WebPageLookupOptions lookupOption, ContextLanguageInfo languageContext, Func<WebPageNode, bool> predicate)
		{
			if (website.Id == Guid.Empty)
			{
				throw new ArgumentException(string.Format("Unable to retrieve the Id of the website. {0}", string.Empty), "website");
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "LookupPageByUrlPath ENTER URL");

			var pages = website.WebPages.Where(predicate).ToArray();

			// Use _pagePathRegex to extract the right-most path segment (child path), and the remaining left
			// part of the path (parent path), while enforcing that web page paths must end in a '/'.
			var pathMatch = UrlMapping._pagePathRegex.Match(urlPath);

			if (!pathMatch.Success)
			{
				// If we don't have a valid path match, still see if there is a page with the entire literal
				// path as its partial URL. (The previous iteration of this method has this behaviour, so we
				// maintain it here.)
				// NOTE: requests for web files (ex: .png, .css) and bad links all come here.
				var mappingResult = FilterResultsOnLanguage(pages, p => IsPartialUrlMatch(p, urlPath), lookupOption, languageContext);
				ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, string.Format("LookupPageByUrlPath (1)pathMatch.Fail URL {0}", mappingResult.Node == null ? "NULL" : "Found"));
				return mappingResult;
			}

			var fullPath = pathMatch.Groups["full"].Value;
			var parentPath = pathMatch.Groups["parent"].Value;
			var childPath = pathMatch.Groups["child"].Value;

			// Check if we can find a page with the exact fullPath match. This may be a web page with a
			// partial URL that matches the entire path, but in the more common case, it will match the
			// root URL path "/".
			var fullPathMatchPageResult = FilterResultsOnLanguage(pages, p => IsPartialUrlMatch(p, fullPath), lookupOption, languageContext);

			if (fullPathMatchPageResult.Node != null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "LookupPageByUrlPath (2)fullPathMatchPage ");
				return fullPathMatchPageResult;
			}

			// If we don't have non-empty parentPath and childPath, lookup fails.
			if (string.IsNullOrEmpty(parentPath) || string.IsNullOrEmpty(childPath))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "LookupPageByUrlPath (3)parent/child path null ");
				return UrlMappingResult<WebPageNode>.MatchResult(null);
			}

			// Look up the parent page, using the parent path. This will generally recurse until parentPath
			// is the root path "/", at which point fullPath will match the Home page and the recursion will
			// unwind.
			// Look for the "Root" (adx_isroot=true) web page because the parent-child relationships uses the Root web pages, not the translated Content web pages.
			// (Ignoring uniquence for parent page)
			var parentPageFilterResult = LookupPageByUrlPath(website, parentPath, WebPageLookupOptions.RootOnly, languageContext, predicate);

			// If we can't find a parent page, lookup fails.
			// Ignore IsUnique here, trying to find any possible match.
			if (parentPageFilterResult.Node == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "LookupPageByUrlPath (4)parent page null ");
				return parentPageFilterResult;
			}

			// Look for a partial URL match for childPath, among the children of the returned parent page.
			var result = FilterResultsOnLanguage(parentPageFilterResult.Node.WebPages, p => predicate(p) && IsPartialUrlMatch(p, childPath), lookupOption, languageContext);
			ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, string.Format("LookupPageByUrlPath (5)searchByParent {0}", result.Node == null ? "NULL" : "Found"));
			return result;
		}

		private static UrlMappingResult<WebPageNode> FilterResultsOnLanguage(IEnumerable<WebPageNode> pages, Func<WebPageNode, bool> predicate, WebPageLookupOptions lookupOption, ContextLanguageInfo contextLanguageInfo)
		{
			var results = pages.Where(predicate);
			WebPageNode retval = null;

			if (contextLanguageInfo != null && contextLanguageInfo.IsCrmMultiLanguageEnabled)
			{
				if (lookupOption == WebPageLookupOptions.LanguageContentOnly)
				{
					// when we have only a root webpage and 0 localized webpages.
					// for example: creating new child page via portal CMS.
					if (results.Where(p => p.IsRoot == false).Count() == 0)
					{
						retval = results.FirstOrDefault();
					}
					else
					{
						var websiteLanguageId = contextLanguageInfo.ContextLanguage.EntityReference.Id;
						retval = results.FirstOrDefault(p => p.WebPageLanguage != null && p.WebPageLanguage.Id == websiteLanguageId && p.IsRoot == false);
					}
				}
				else if (lookupOption == WebPageLookupOptions.RootOnly)
				{
					retval = results.FirstOrDefault(p => p.IsRoot == true);
				}
				else
				{
					retval = results.FirstOrDefault();
				}

				// If the found page is content, but the root page is deactivated, then return null as if the page itself doesn't exist.
				if (retval != null && retval.IsRoot == false && (retval.RootWebPage == null || retval.RootWebPage.IsReference))
				{
					retval = null;
				}
			}
			else
			{
				// If multi-language is not supported, then do legacy behavior of returning first result.
				retval = results.FirstOrDefault();
			}

			// select only root pages or pages where isroot = null (MLP is not supported)
			var duplicateCheckArray = results.Where(p => p.IsRoot != false).ToArray();

			return duplicateCheckArray.Length > 1
				? UrlMappingResult<WebPageNode>.DuplicateResult(retval)
				: UrlMappingResult<WebPageNode>.MatchResult(retval);
		}

		private static bool IsPartialUrlMatch(WebPageNode page, string path)
		{
			var decodedPath = HttpUtility.UrlDecode(path);
			return page != null
					&& (string.Equals(page.PartialUrl, path, StringComparison.InvariantCultureIgnoreCase)
						|| string.Equals(page.PartialUrl, decodedPath, StringComparison.InvariantCultureIgnoreCase));
		}

		public static WebFileNode LookupFileByUrlPath(WebsiteNode website, string urlPath, ContextLanguageInfo languageContext)
		{
			CrmEntityInactiveInfo inactiveInfo;

			var filter = CrmEntityInactiveInfo.TryGetInfo("adx_webfile", out inactiveInfo)
				? file => !inactiveInfo.IsInactive(file.ToEntity())
				: new Func<WebFileNode, bool>(entity => true);

			var result = LookupFileByUrlPath(website, urlPath, languageContext, filter);

			return result;
		}

		private static WebFileNode LookupFileByUrlPath(WebsiteNode website, string urlPath, ContextLanguageInfo languageContext, Func<WebFileNode, bool> predicate)
		{
			if (website.Id == Guid.Empty) throw new NullReferenceException(string.Format("Unable to retrieve the Id of the website. {0}", "Lookup failed."));

			var urlWithoutWebsitePath = HttpUtility.UrlDecode(urlPath.TrimEnd('/'));

			string parentPath;
			string thisPath;

			if (ParseParentPath(urlWithoutWebsitePath, out parentPath, out thisPath))
			{
				// Find the language-root web page because that's what web files hang off from.
				var parentFilterResult = LookupPageByUrlPath(website, parentPath, WebPageLookupOptions.RootOnly, languageContext);

				if (parentFilterResult.Node != null)
				{
					var file = parentFilterResult.Node.WebFiles.FirstOrDefault(f => predicate(f) && string.Equals(f.PartialUrl, thisPath, StringComparison.InvariantCultureIgnoreCase));

					if (file != null)
					{
						ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "LookupFILEByUrlPath Found");
						return file;
					}
				}
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "LookupFILEByUrlPath NULL");
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
				var newPath = path.Remove(lastIndex + 1);

				if (newPath == string.Empty)
				{
					parentPath = "/";
					lastPathComponent = path.Remove(0, 1);
					return true;
				}

				parentPath = newPath;
				lastPathComponent = path.Remove(0, newPath.Length);

				return true;
			}

			return false;
		}
	}
}
