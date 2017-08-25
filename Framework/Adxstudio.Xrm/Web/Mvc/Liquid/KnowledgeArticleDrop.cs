/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.KnowledgeArticles;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class KnowledgeArticleDrop : EntityDrop
	{
		public KnowledgeArticleDrop(IPortalLiquidContext portalLiquidContext, IDataAdapterDependencies dependencies, IKnowledgeArticle article)
			: base(portalLiquidContext, article.Entity)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");
			if (article == null) throw new ArgumentNullException("article");

			Article = article;
		}

		protected IKnowledgeArticle Article { get; private set; }

		public string ArticlePublicNumber
		{
			get { return Article.ArticlePublicNumber; }
		}

		public int CommentCount
		{
			get { return Article.CommentCount; }
		}

		public string Content
		{
			get { return Article.Content; }
		}

		public bool CurrentUserCanComment
		{
			get { return Article.CurrentUserCanComment; }
		}

		public bool IsRatingEnabled
		{
			get { return Article.IsRatingEnabled; }
		}

		public string Keywords
		{
			get { return Article.Keywords; }
		}

		public string Name
		{
			get { return Title; }
		}

		public decimal Rating
		{
			get { return Article.Rating; }
		}

		public string Title
		{
			get { return Article.Title; }
		}

		public int ViewCount
		{
			get { return Article.KnowledgeArticleViews; }
		}

	}
}
