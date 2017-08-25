/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Provides methods to get aggregated Idea data for an Adxstudio Portals Website and user.
	/// </summary>
	public interface IWebsiteIdeaUserAggregationDataAdapter
	{
		/// <summary>
		/// Returns comments that have been posted for the website and user this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first comment to be returned.</param>
		/// <param name="maximumRows">The maximum number of comments to return.</param>
		IEnumerable<IIdeaIdeaCommentPair> SelectIdeaComments(int startRowIndex = 0, int maximumRows = -1);

		/// <summary>
		/// Returns ideas that have been submitted for the website and user this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first idea to be returned.</param>
		/// <param name="maximumRows">The maximum number of ideas to return.</param>
		IEnumerable<IIdea> SelectIdeas(int startRowIndex = 0, int maximumRows = -1);

		/// <summary>
		/// Returns votes that have been casted for the website and user this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first comment to be returned.</param>
		/// <param name="maximumRows">The maximum number of comments to return.</param>
		IEnumerable<IIdeaIdeaVotePair> SelectIdeaVotes(int startRowIndex = 0, int maximumRows = -1);

		/// <summary>
		/// Returns the number of comments that have been posted for the website and user this adapter applies to.
		/// </summary>
		int SelectIdeaCommentCount();

		/// <summary>
		/// Returns the number of ideas that have been submitted for the website and user this adapter applies to.
		/// </summary>
		int SelectIdeaCount();

		/// <summary>
		/// Returns the number of votes that have been casted for the website and user this adapter applies to.
		/// </summary>
		int SelectIdeaVoteCount();
	}
}
