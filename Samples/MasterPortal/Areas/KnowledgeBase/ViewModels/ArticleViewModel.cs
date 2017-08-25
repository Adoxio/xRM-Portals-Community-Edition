/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.KnowledgeBase.ViewModels
{
	public class ArticleViewModel
	{
		public ArticleViewModel(Entity kbarticle, IEnumerable<RelatedArticle> relatedArticles)
		{
			if (kbarticle == null) throw new ArgumentNullException("kbarticle");

			RelatedArticles = relatedArticles ?? Enumerable.Empty<RelatedArticle>();

			Content = ExtractContent(kbarticle.GetAttributeValue<string>("content"));
			Number = kbarticle.GetAttributeValue<string>("number");
			Title = kbarticle.GetAttributeValue<string>("title");
		}

		public IHtmlString Content { get; private set; }

		public string Number { get; private set; }

		public IEnumerable<RelatedArticle> RelatedArticles { get; private set; }

		public string Title { get; private set; }

		private static IHtmlString ExtractContent(string content)
		{
			var html = new HtmlDocument();
			html.LoadHtml(content);

			var table = html.DocumentNode.SelectSingleNode("//body/table");

			if (table == null)
			{
				return null;
			}

			using (var output = new StringWriter())
			{
				table.WriteTo(output);

				return new HtmlString(output.GetStringBuilder().ToString());
			}
		}
	}
}
