/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Feedback;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.KnowledgeArticles
{
	/// <summary>
	/// Represents an Knowledge Article in an Adxstudio.
	/// </summary>
	public interface IKnowledgeArticle
	{
		Guid Id { get; }

		string Title { get; }

		string ArticlePublicNumber { get; }

		string Content { get; }

		string Keywords { get; }

		int KnowledgeArticleViews { get; }

		EntityReference RootArticle { get; }

		Entity Entity { get; }

		EntityReference EntityReference { get; }

		int CommentCount { get; }

		decimal Rating { get; }

		bool IsRatingEnabled { get; }

		bool CurrentUserCanComment { get; }

		CommentPolicy CommentPolicy { get; }

		string Url { get; }
	}
}
