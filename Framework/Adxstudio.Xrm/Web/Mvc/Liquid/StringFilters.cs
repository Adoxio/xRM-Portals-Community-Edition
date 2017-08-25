/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Net;
using Adxstudio.Xrm.Text;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public static class StringFilters
	{
		public static string TextToHtml(string input, bool linkifyUrls = true)
		{
			return input == null ? null : new SimpleHtmlFormatter(linkifyUrls).Format(input).ToString();
		}

		public static string UrlEscape(string input)
		{
			return input == null ? null : WebUtility.UrlEncode(input);
		}

		public static string XmlEscape(string input)
		{
			return input == null ? null : System.Security.SecurityElement.Escape(input);
		}
	}
}
