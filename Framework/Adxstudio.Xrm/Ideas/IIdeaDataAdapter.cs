/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Provides methods to get and set data for an Adxstudio Portals Idea such as comments and votes.
	/// </summary>
	public interface IIdeaDataAdapter
	{
		/// <summary>
		/// Post a comment for the idea this adapter applies to.
		/// </summary>
		/// <param name="content">The comment copy.</param>
		/// <param name="authorName">The name of the author for this comment (ignored if user is authenticated).</param>
		/// <param name="authorEmail">The email of the author for this comment (ignored if user is authenticated).</param>
		void CreateComment(string content, string authorName = null, string authorEmail = null);

		/// <summary>
		/// Cast a vote/votes for the idea this adapter applies to.
		/// </summary>
		/// <param name="voteValue">The number of votes to cast.</param>
		/// <param name="voterName">The name of the voter (ignored if user is authenticated).</param>
		/// <param name="voterEmail">The email of the voter (ignored if user is authenticated).</param>
		void CreateVote(int voteValue, string voterName = null, string voterEmail = null);

		/// <summary>
		/// Returns the <see cref="IIdea"/> that this adapter applies to.
		/// </summary>
		IIdea Select();

		/// <summary>
		/// Returns comments that have been posted for the idea this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first comment to be returned.</param>
		/// <param name="maximumRows">The maximum number of comments to return.</param>
		IEnumerable<IComment> SelectComments(int startRowIndex = 0, int maximumRows = -1);

		/// <summary>
		/// Returns the number of comments that have been posted for the idea this adapter applies to.
		/// </summary>
		int SelectCommentCount();
	}
}
