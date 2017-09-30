/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Text
{
	/// <summary>
	/// Simple formatter that takes plain text input and does a simple transformation to HTML. The input text
	/// is HTML-encoded, blank lines (double linebreaks) are wrapped in paragraphs, and single linebreaks are
	/// replaced with HTML breaks. Optionally, any URLs in the text can be converted to HTML links.
	/// </summary>
	public class SimpleHtmlFormatter
	{
		public SimpleHtmlFormatter(bool linkifyUrls = true)
		{
			LinkifyUrls = linkifyUrls;
		}

		protected bool LinkifyUrls { get; private  set; }

		public IHtmlString Format(string text)
		{
			if (text == null)
			{
				return null;
			}

			text = WebUtility.HtmlEncode(text);
			text = LinkifyUrls ? ReplaceUrlsWithLinks(text) : text;

			// Split text on blank lines.
			var blocks = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

			var html = new StringBuilder();

			foreach (var block in blocks)
			{
				// Replace line breaks with <br> tags.
				var withLineBreaks = block
					.Replace("\r\n", "<br />")
					.Replace("\n", "<br />");

				// Wrap the block in a paragraph.
				html.AppendFormat(CultureInfo.InvariantCulture, "<p>{0}</p>", withLineBreaks);
			}

			return new HtmlString(html.ToString());
		}

		private static readonly Regex UrlReplacementRegex = new Regex(@"(?<url>http(s?)://\S+[/\w])", RegexOptions.IgnoreCase);

		private static string ReplaceUrlsWithLinks(string text)
		{
			return UrlReplacementRegex.Replace(text, match =>
			{
				Uri uri;

				return Uri.TryCreate(match.Groups["url"].Value, UriKind.Absolute, out uri)
					? @"<a href=""{0}"" rel=""nofollow"">{0}</a>".FormatWith(WebUtility.HtmlEncode(uri.ToString()))
					: match.Value;
			});
		}
	}
}
