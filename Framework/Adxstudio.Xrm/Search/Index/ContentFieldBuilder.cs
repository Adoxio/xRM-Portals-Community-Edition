/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Adxstudio.Xrm.Search.Index
{
	internal class ContentFieldBuilder
	{
		private static readonly Regex HtmlElementPattern = new Regex("<[^>]*>", RegexOptions.Compiled);
		private static readonly Regex LiquidTagPattern = new Regex(@"\{[\{\%][^\}]*\}", RegexOptions.Compiled);
		private static readonly Regex WhitespaceNormalizationPattern = new Regex(@"[\s\r\n]+", RegexOptions.Compiled);

		private readonly StringBuilder _builder = new StringBuilder();

		public ContentFieldBuilder Append(string content)
		{
			if (string.IsNullOrWhiteSpace(content))
			{
				return this;
			}

			_builder.AppendLine(NormalizeWhitespace(StripLiquid(StripHtml(content))));
			_builder.AppendLine();

			return this;
		}

		public override string ToString()
		{
			// We want any characters that are HTML-entity-encoded to be their literal UTF selves
			// in the index, for mathing purposes. But we also need to guard against decoding encoded
			// HTML, like &lt;. So, we strip HTML again on the decoded result to be sure.
			return StripHtml(WebUtility.HtmlDecode(_builder.ToString()));
		}

		private static string NormalizeWhitespace(string content)
		{
			return WhitespaceNormalizationPattern.Replace(content, " ").Trim();
		}

		private static string StripHtml(string content)
		{
			return HtmlElementPattern.Replace(content, string.Empty);
		}

		private static string StripLiquid(string content)
		{
			return LiquidTagPattern.Replace(content, string.Empty);
		}

		public static string StripContent(string content)
		{
			return NormalizeWhitespace(StripLiquid(StripHtml(content)));
		}
	}
}
