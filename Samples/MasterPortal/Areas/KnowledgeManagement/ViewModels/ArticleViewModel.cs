/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.KnowledgeManagement.ViewModels
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Adxstudio.Xrm.KnowledgeArticles;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Data;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;

	public class ArticleViewModel
	{
		private readonly KnowledgeArticleDataAdapter articleDataAdapter;
		private readonly Lazy<ArticleCommentsViewModel> comments;
		private IEnumerable<IRelatedArticle> relatedArticles;
		private IEnumerable<IRelatedProduct> relatedProducts;
		private IEnumerable<IRelatedNote> relatedNotes; 
		private readonly Lazy<IEnumerable<EntityCollection>> relatedEntityResponses;

		public ArticleViewModel(Entity article, int? page, string code)
		{
			this.articleDataAdapter = new KnowledgeArticleDataAdapter(article, code) { ChronologicalComments = true };
			this.KnowledgeArticle = this.articleDataAdapter.Select();
			this.relatedEntityResponses = new Lazy<IEnumerable<EntityCollection>>(this.GetRelatedArticlesAndProducts);
			this.comments = new Lazy<ArticleCommentsViewModel>(() => this.GetComments(page));
		}

		public IKnowledgeArticle KnowledgeArticle { get; set; }

		public IEnumerable<IRelatedArticle> RelatedArticles
		{
			get
			{
				if (this.relatedArticles == null)
				{
					var articles = this.articleDataAdapter.SelectRelatedArticles(this.relatedEntityResponses.Value);
					this.relatedArticles = articles;
				}
				return this.relatedArticles;
			}
		}

		public IEnumerable<IRelatedProduct> RelatedProducts
		{
			get
			{
				if (this.relatedProducts == null)
				{
					var products = this.articleDataAdapter.SelectRelatedProducts(this.relatedEntityResponses.Value);
					this.relatedProducts = products;
				}
				return this.relatedProducts;
			}
		}

		public IEnumerable<IRelatedNote> RelatedNotes
		{
			get
			{
				if (this.relatedNotes == null)
				{
					var notes = this.articleDataAdapter.SelectRelatedNotes(this.KnowledgeArticle);
					this.relatedNotes = notes;
				}
				return this.relatedNotes;
			}
		}

		public ArticleCommentsViewModel Comments
		{
			get { return this.comments.Value; }
		}

		private IEnumerable<EntityCollection> GetRelatedArticlesAndProducts()
		{
			return this.articleDataAdapter.GetRelatedProductsAndArticles(this.KnowledgeArticle);
		}

		private ArticleCommentsViewModel GetComments(int? page)
		{
			var results = (page != null) ?
				new PaginatedList<IComment>(page, this.articleDataAdapter.SelectCommentCount(), this.articleDataAdapter.SelectComments)
				: new PaginatedList<IComment>(PaginatedList.Page.Last, this.articleDataAdapter.SelectCommentCount(), this.articleDataAdapter.SelectComments);

			return new ArticleCommentsViewModel { Comments = results, KnowledgeArticle = this.KnowledgeArticle };
		}
		
		public IEnumerable<IRelatedNote> Notes { get; set; }
	}
}
