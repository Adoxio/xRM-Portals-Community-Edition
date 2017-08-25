/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// View helpers for rendering Site Settings (adx_sitesetting) in Adxstudio Portals applications.
	/// </summary>
	public static class SettingExtensions
	{
		/// <summary>
		/// Gets the Boolean value of a Site Setting (adx_sitesetting), by name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="settingName">The name (adx_name) of the setting to retrieve.</param>
		/// <returns>
		/// The value of the Site Setting specified by <paramref name="settingName"/>. If the setting is not found, or cannot be parsed
		/// as a valid Boolean value, returns null.
		/// </returns>
		public static bool? BooleanSetting(this HtmlHelper html, string settingName)
		{
			return PortalExtensions.GetPortalViewContext(html).Settings.GetBooleanValue(settingName);
		}

		/// <summary>
		/// Gets the decimal value of a Site Setting (adx_sitesetting), by name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="settingName">The name (adx_name) of the setting to retrieve.</param>
		/// <returns>
        /// The value of the Site Setting specified by <paramref name="settingName"/>. If the setting is not found, or cannot be parsed
		/// as a valid decimal value, returns null.
		/// </returns>
		public static decimal? DecimalSetting(this HtmlHelper html, string settingName)
		{
			return PortalExtensions.GetPortalViewContext(html).Settings.GetDecimalValue(settingName);
		}

		/// <summary>
		/// Gets the integer value of a Site Setting (adx_sitesetting), by name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="settingName">The name (adx_name) of the setting to retrieve.</param>
		/// <returns>
        /// The value of the Site Setting specified by <paramref name="settingName"/>. If the setting is not found, or cannot be parsed
		/// as a valid integer value, returns null.
		/// </returns>
		public static int? IntegerSetting(this HtmlHelper html, string settingName)
		{
			return PortalExtensions.GetPortalViewContext(html).Settings.GetIntegerValue(settingName);
		}

		/// <summary>
		/// Gets the value of a Site Setting (adx_sitesetting), and attempts to render a partial view with that name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="settingName">The name (adx_name) of the setting to retrieve.</param>
		/// <param name="defaultPartialViewName">An optional default partial view name value to be rendered if the setting does not exist, has no value, or has an invalid value.</param>
		public static void RenderPartialFromSetting(this HtmlHelper html, string settingName, string defaultPartialViewName = null)
		{
			var value = html.Setting(settingName, defaultPartialViewName);

			if (string.IsNullOrEmpty(value))
			{
				return;
			}

			try
			{
				html.RenderPartial(value);
			}
			catch (InvalidOperationException e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format(@"Attempt to render partial with setting {0}=""{1}"" resulted in an exception: {2}", settingName, value, e.ToString()));

                if (defaultPartialViewName != null)
				{
                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(@"Rendering partial with default partial view name ""{0}"".", defaultPartialViewName));

					html.RenderPartial(defaultPartialViewName);
				}
			}
		}

		/// <summary>
		/// Gets the value of a Site Setting (adx_sitesetting), by name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="settingName">The name (adx_name) of the setting to retrieve.</param>
		/// <param name="defaultValue">An optional default value to be returned if the setting does not exist or has no value.</param>
		/// <returns>
        /// The value of the Site Setting specified by <paramref name="settingName"/>. If the setting is not found, and no
		/// <paramref name="defaultValue"/> is specified, returns an empty string.
		/// </returns>
		public static string Setting(this HtmlHelper html, string settingName, string defaultValue = null)
		{
			var setting = PortalExtensions.GetPortalViewContext(html).Settings.Select(settingName);

			return setting == null
				? defaultValue ?? string.Empty
				: Setting(html, setting, defaultValue);
		}

		/// <summary>
		/// Gets the value of a Site Setting (adx_sitesetting), by name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="setting">The <see cref="ISetting"/> whose value will be returned.</param>
		/// <param name="defaultValue">An optional default value to be returned if the setting does not exist or has no value.</param>
		/// <returns>
        /// The value of <paramref name="setting"/>. If <paramref name="setting"/> is null, returns an empty string.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if <see cref="ArgumentNullException"/> is null.</exception>
		public static string Setting(this HtmlHelper html, ISetting setting, string defaultValue = null)
		{
			if (setting == null)
			{
				throw new ArgumentNullException("setting");
			}

			return setting.Value ?? defaultValue ?? string.Empty;
		}

		/// <summary>
		/// Gets a collection of search logical name filter options, parsed from a given Site Setting (adx_sitesetting).
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="settingName">The name (adx_name) of the setting to retrieve.</param>
		/// <param name="defaultValue">An optional default value to be returned if the setting does not exist or has no value.</param>
		/// <remarks>
		/// The site setting value should be in the form of name/value pairs, with name and value separated by a colon, and pairs
		/// separated by a semicolon. For example: "Forums:adx_communityforum,adx_communityforumthread,adx_communityforumpost;Blogs:adx_blog,adx_blogpost,adx_blogpostcomment".
		/// </remarks>
		public static IEnumerable<KeyValuePair<string, string>> SearchFilterOptions(this HtmlHelper html, string settingName = "search/filters", string defaultValue = null)
		{
			var value = html.Setting(settingName, defaultValue);

			if (string.IsNullOrEmpty(value))
			{
				return Enumerable.Empty<KeyValuePair<string, string>>();
			}

			return SplitSearchFilterOptions(value);
		}

		/// <summary>
		/// Gets the <see cref="TimeSpan"/> value of a Site Setting (adx_sitesetting), by name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="settingName">The name (adx_name) of the setting to retrieve.</param>
		/// <returns>
        /// The value of the Site Setting specified by <paramref name="settingName"/>. If the setting is not found, or cannot be parsed
		/// as a valid <see cref="TimeSpan"/> value, returns null.
		/// </returns>
		public static TimeSpan? TimeSpanSetting(this HtmlHelper html, string settingName)
		{
			return PortalExtensions.GetPortalViewContext(html).Settings.GetTimeSpanValue(settingName);
		}

		public static IEnumerable<KeyValuePair<string, string>> SplitSearchFilterOptions(string searchFilterOptions)
		{
			var matches = Regex.Matches(searchFilterOptions, @"\s*(?<name>[^:;]+?)\s*:\s*(?<value>[^;]+)\s*", RegexOptions.ExplicitCapture);

			return matches.Cast<Match>()
				.Select(match => new KeyValuePair<string, string>(match.Groups["name"].Value, match.Groups["value"].Value.Replace(" ", string.Empty)));
		}
	}
}
