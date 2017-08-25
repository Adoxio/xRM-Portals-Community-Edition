/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Feedback;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.KnowledgeArticles
{
	public class KnowledgeArticleFactory
	{
		private const string ArticleSiteMarker = "Knowledge Article";

		private readonly IDataAdapterDependencies _dependencies;
		private readonly HttpContextBase _httpContext;

		public KnowledgeArticleFactory(IDataAdapterDependencies dependencies)
		{
			dependencies.ThrowOnNull("dependencies");

			_dependencies = dependencies;

			var request = _dependencies.GetRequestContext();
			_httpContext = request == null ? null : request.HttpContext;
		}

		public IEnumerable<IKnowledgeArticle> Create(IEnumerable<Entity> articleEntities)
		{
			var articles = articleEntities.ToArray();
			var articleIds = articles.Select(e => e.Id).ToArray();

			var commentCounts = _dependencies.GetServiceContext().FetchArticleCommentCounts(articleIds);

			var feedbackPolicyReader = new FeedbackPolicyReader(_dependencies, ArticleSiteMarker);
			var commentPolicy = feedbackPolicyReader.GetCommentPolicy();
			var isRatingEnabled = feedbackPolicyReader.IsRatingEnabled();

			if (commentCounts == null)
			{
				return articles.Select(e => new KnowledgeArticle(e, 0, commentPolicy, isRatingEnabled, _httpContext)).ToArray();
			}

			return articles.Select(e =>
			{
				int commentCountValue;
				var commentCount = commentCounts.TryGetValue(e.Id, out commentCountValue) ? commentCountValue : 0;

				return new KnowledgeArticle(e, commentCount, commentPolicy, isRatingEnabled, _httpContext);
			}).ToArray();
		}

		
	}
}
