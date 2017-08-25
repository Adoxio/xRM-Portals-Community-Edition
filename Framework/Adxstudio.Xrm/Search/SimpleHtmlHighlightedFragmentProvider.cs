/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Search
{
	public class SimpleHtmlHighlightedFragmentProvider : ICrmEntitySearchResultFragmentProvider
	{
		private const string _highlighterStartTag = @"<em class=""highlight"">";
		private const string _highlighterEndTag = @"</em>";

		private readonly Highlighter _highlighter;
		private readonly ICrmEntityIndex _index;

		public SimpleHtmlHighlightedFragmentProvider(ICrmEntityIndex index, Query query)
		{
			if (index == null)
			{
				throw new ArgumentNullException("index");
			}

			if (query == null)
			{
				throw new ArgumentNullException("query");
			}

			_index = index;

			var queryScorer = new QueryScorer(query);

			_highlighter = new Highlighter(new SimpleHTMLFormatter(_highlighterStartTag, _highlighterEndTag), queryScorer)
			{
				TextFragmenter = new SimpleSpanFragmenter(queryScorer, 160)
			};
		}

		public string GetFragment(Document document)
		{
			if (document == null)
			{
				throw new ArgumentNullException("document");
			}

			var contentField = document.GetField(_index.ContentFieldName);

			if (contentField == null || !contentField.IsStored)
			{
				return string.Empty;
			}

			var content = contentField.StringValue;

			var tokenStream = _index.Analyzer.TokenStream(contentField.Name, new StringReader(content));

			var rawFragment = _highlighter.GetBestFragments(tokenStream, content, 1, "...");

			var bestFragment = Regex.Split(rawFragment, @"^\s*$", RegexOptions.Multiline)
				.OrderByDescending(f => Regex.Matches(f, Regex.Escape(_highlighterStartTag)).Count)
				.First();

			var fragment = bestFragment.Trim();

			if (string.IsNullOrEmpty(fragment))
			{
				return string.Empty;
			}

			return "&hellip;&nbsp;{0}&nbsp;&hellip;".FormatWith(fragment);
		}
	}
}
