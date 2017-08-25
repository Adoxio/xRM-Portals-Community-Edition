/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	using System;
	using System.Text.RegularExpressions;
	using System.Globalization;
	using System.Linq;
	using System.Web;
	using System.Web.Optimization;
	using AspNet.Cms;

	/// <summary>
	/// Helper to render localized versions of js optimization bundles
	/// </summary>
	public static class LocalizedScripts
	{
		/// <summary>
		/// Regexp to parse bundle name
		/// </summary>
		private static readonly Regex BundleNameRegex = new Regex("^(.*?)(\\.bundle\\.(?:js|css))$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// Returns localized bundle name.
		/// e.g. "~/js/default-1033.bundle.js" for "~/js/default.bundle.js"
		/// </summary>
		/// <param name="input">original name</param>
		/// <param name="cultureLcid">culture info</param>
		/// <returns>localized name</returns>
		public static string GetLocalizedBundleName(string input, int cultureLcid)
		{
			if (string.IsNullOrEmpty(input))
			{
				return input;
			}

			// Adding lcid before .bundle.XXX because otherwise routing rule will be not satisfied

			var match = BundleNameRegex.Match(input);

			if (!match.Success)
			{
				throw new InvalidOperationException("Invalid name template. Should ends with .bundle.XXX");
			}

			var name = match.Groups[1];
			var postfix = match.Groups[2];

			return string.Format("{0}-{1}{2}", name, cultureLcid, postfix);
		}

		/// <summary>
		/// Renders localized script tags for the following paths.
		/// </summary>
		/// <param name="paths">Set of virtual paths for which to generate script tags.</param>
		/// <returns>The HTML string containing the script tag or tags for the bundle.</returns>
		public static IHtmlString Render(params string[] paths)
		{
			var bundles = BundleTable.Bundles;
			var current = CultureInfo.CurrentCulture;

			return Scripts.Render(paths.Select(path => GetLocalizedBundleName(path, ContextLanguageInfo.ResolveCultureLcid(current.LCID))).ToArray());
		}
	}
}
