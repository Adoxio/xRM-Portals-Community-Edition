/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.KnowledgeArticles
{
	using System.Collections.Generic;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Provides data operations for a single knowledge article.
	/// </summary>
	public interface IKnowledgeArticleDataAdapter
	{
		/// <summary>
		/// Selects the attributes of the knowledge article.
		/// </summary>
		IKnowledgeArticle Select();

		/// <summary>
		/// Selects the related knowledge articles.
		/// </summary>
		IEnumerable<IRelatedArticle> SelectRelatedArticles(IEnumerable<EntityCollection> entityCollections);

		/// <summary>
		/// Selects the related products.
		/// </summary>
		IEnumerable<IRelatedProduct> SelectRelatedProducts(IEnumerable<EntityCollection> entityCollections);

		/// <summary>
		/// Selects the related notes.
		/// </summary>
		IEnumerable<IRelatedNote> SelectRelatedNotes(IKnowledgeArticle article);

		/// <summary>
		/// Post a comment for the knowledge article this adapter applies to.
		/// </summary>
		/// <param name="content">The comment copy.</param>
		/// <param name="authorName">The name of the author for this comment (ignored if user is authenticated).</param>
		/// <param name="authorEmail">The email of the author for this comment (ignored if user is authenticated).</param>
		void CreateComment(string content, string authorName = null, string authorEmail = null);

		/// <summary>
		/// Vote for the knowledge article this adapter applies to.
		/// Creates or updates the current user's rating
		/// </summary>
		/// <param name="rating">Rating value.</param>
		/// <param name="maxRating">Maximum rating value.</param>
		/// <param name="minRating">Minimum rating value.</param>
		/// <param name="visitorID">Visitor ID.</param>
		void CreateUpdateRating(int rating, int maxRating, int minRating, string visitorID = null);
	}
}
