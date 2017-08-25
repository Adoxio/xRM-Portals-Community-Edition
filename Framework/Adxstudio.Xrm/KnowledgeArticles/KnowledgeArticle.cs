/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.KnowledgeArticles
{
	using System;
	using System.Threading;
	using System.Web;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Feedback;
	using Microsoft.Xrm.Sdk.Query;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Portal.Web.Providers;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Represents an Knowledge Article in an Adxstudio.
	/// </summary>
	public class KnowledgeArticle : IKnowledgeArticle
	{
		/// <summary>The timespan to keep in cache.</summary>
		private static readonly TimeSpan DefaultDuration = TimeSpan.FromHours(1);

		/// <summary>The url.</summary>
		private readonly Lazy<string> url;

		/// <summary>The article views.</summary>
		private readonly Lazy<int> articleViews;

		/// <summary>The article rating.</summary>
		private readonly Lazy<decimal> articleRating;

		/// <summary>The http context.</summary>
		private readonly HttpContextBase httpContext;

		/// <summary>Gets the id.</summary>
		public Guid Id { get; private set; }

		/// <summary>Gets the title.</summary>
		public string Title { get; private set; }

		/// <summary>Gets the article public number.</summary>
		public string ArticlePublicNumber { get; private set; }

		/// <summary>Gets the content.</summary>
		public string Content { get; private set; }

		/// <summary>Gets the keywords.</summary>
		public string Keywords { get; private set; }

		/// <summary>Gets the knowledge article views.</summary>
		public int KnowledgeArticleViews
		{
			get { return this.articleViews.Value; }
		}

		/// <summary>Gets the root article.</summary>
		public EntityReference RootArticle { get; private set; }

		/// <summary>Gets the entity.</summary>
		public Entity Entity { get; private set; }

		/// <summary>Gets the entity reference.</summary>
		public EntityReference EntityReference { get; private set; }

		/// <summary>Gets the comment count.</summary>
		public int CommentCount { get; private set; }

		/// <summary>Gets the rating.</summary>
		public decimal Rating
		{
			get { return this.articleRating.Value; }
		}

		/// <summary>Gets a value indicating whether is rating enabled.</summary>
		public bool IsRatingEnabled { get; private set; }

		/// <summary>Gets a value indicating whether current user can comment.</summary>
		public bool CurrentUserCanComment { get; private set; }

		/// <summary>Gets the comment policy.</summary>
		public CommentPolicy CommentPolicy { get; private set; }

		/// <summary>Gets the url.</summary>
		public string Url
		{
			get { return this.url.Value; }
		}

		/// <summary>Initializes a new instance of the <see cref="KnowledgeArticle"/> class. Knowledge Article initialization</summary>
		/// <param name="article">Knowledge Article record entity</param>
		/// <param name="commentCount">The comment Count.</param>
		/// <param name="commentPolicy">The comment Policy.</param>
		/// <param name="isRatingEnabled">The is Rating Enabled.</param>
		/// <param name="httpContext">The http Context.</param>
		public KnowledgeArticle(
			Entity article,
			int commentCount,
			CommentPolicy commentPolicy,
			bool isRatingEnabled,
			HttpContextBase httpContext)
		{
			article.ThrowOnNull("article");
			article.AssertEntityName("knowledgearticle");

			this.httpContext = httpContext;
			this.Entity = article;
			this.EntityReference = article.ToEntityReference();
			this.Id = article.Id;
			this.Title = article.GetAttributeValue<string>("title");
			this.ArticlePublicNumber = article.GetAttributeValue<string>("articlepublicnumber");
			this.RootArticle = article.GetAttributeValue<EntityReference>("rootarticleid");
			this.Content = article.GetAttributeValue<string>("content");
			this.Keywords = article.GetAttributeValue<string>("keywords");
			this.CommentCount = commentCount;
			this.IsRatingEnabled = isRatingEnabled;
			this.CommentPolicy = commentPolicy;
			this.CurrentUserCanComment =
				commentPolicy == CommentPolicy.Open ||
				commentPolicy == CommentPolicy.Moderated ||
				(commentPolicy == CommentPolicy.OpenToAuthenticatedUsers && httpContext.Request.IsAuthenticated);
			this.url = new Lazy<string>(this.GetUrl, LazyThreadSafetyMode.None);
			this.articleViews = new Lazy<int>(this.GetArticleViews);
			this.articleRating = new Lazy<decimal>(this.GetArticleRating);
		}

		/// <summary>Get the article rul.</summary>
		/// <returns>The article url.</returns>
		private string GetUrl()
		{
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext();
			var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider().GetDependency<IEntityUrlProvider>();

			return urlProvider.GetUrl(serviceContext, this.Entity);
		}

		/// <summary>Get the number of article views.</summary>
		/// <returns>The numbr of views.</returns>
		private int GetArticleViews()
		{
			var service = this.httpContext.GetOrganizationService();
			var attributes = service.RetrieveSingle(
				this.EntityReference,
				new ColumnSet("knowledgearticleviews"),
				RequestFlag.AllowStaleData | RequestFlag.SkipDependencyCalculation,
				DefaultDuration);

			return attributes.Contains("knowledgearticleviews") ? attributes["knowledgearticleviews"] as int? ?? 0 : 0;
		}

		/// <summary> Get the rating of the article </summary>
		/// <returns> The article rating. </returns>
		private decimal GetArticleRating()
		{
			var service = this.httpContext.GetOrganizationService();
			var entity = service.RetrieveSingle(
				this.EntityReference,
				new ColumnSet("rating"),
				RequestFlag.AllowStaleData | RequestFlag.SkipDependencyCalculation,
				DefaultDuration);

			return entity.Attributes.Contains("rating") ? entity.Attributes["rating"] as decimal? ?? 0 : 0;
		}
	}
}
