/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Adxstudio.Xrm.Text
{
	/// <summary>
	/// String extension that extends string class with additional helper methods
	/// </summary>
	public static class StringHelper
	{
		private const int CommentTitleLength = 155;

		/// <summary>
		/// Return default title from comment content 
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public static string GetCommentTitleFromContent(string content)
		{
			if (string.IsNullOrEmpty(content))
			{
				return content;
			}
			else
			{
				string strippedContent = StripHtml(content);

				return strippedContent.Length <= CommentTitleLength ? strippedContent : strippedContent.Substring(0, CommentTitleLength);
			}
		}

		/// <summary>
		/// Strips html and removes new line characters
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		public static string StripHtml(string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				return null;
			}

			var document = new HtmlAgilityPack.HtmlDocument();

			document.LoadHtml(html);

			return Regex.Replace(HttpUtility.HtmlDecode(document.DocumentNode.InnerText), @"[\s\r\n]+", " ").Trim();
		}
	}
}
