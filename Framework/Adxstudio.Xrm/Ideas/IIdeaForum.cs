/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Represents an Idea Forum in an Adxstudio Portals Website.
	/// </summary>
	public interface IIdeaForum
	{
		/// <summary>
		/// An adx_ideaforum entity.
		/// </summary>
		Entity Entity { get; }

		/// <summary>
		/// The comment policy for this idea forum.
		/// </summary>
		IdeaForumCommentPolicy CommentPolicy { get; }

		/// <summary>
		/// The number of votes casted by the current user in this idea forum.
		/// </summary>
		int CurrentUserActiveVoteCount { get; }

		/// <summary>
		/// Whether or not the current user is allowed to submit new ideas in this idea forum.
		/// </summary>
		bool CurrentUserCanSubmitIdeas { get; }

		/// <summary>
		/// The unique identifier for this idea forum.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// The policy for new idea submissions in this idea forum.
		/// </summary>
		IdeaForumIdeaSubmissionPolicy IdeaSubmissionPolicy { get; }

		/// <summary>
		/// The URL slug for this idea forum.
		/// </summary>
		string PartialUrl { get; }

		/// <summary>
		/// An abstract or description for this idea forum.
		/// </summary>
		string Summary { get; }

		/// <summary>
		/// The title for this idea forum.
		/// </summary>
		string Title { get; }

		/// <summary>
		/// The number of votes a user can cast in this idea forum.
		/// </summary>
		int? VotesPerUser { get; }

		/// <summary>
		/// The voting policy for this idea forum.
		/// </summary>
		IdeaForumVotingPolicy VotingPolicy { get; }

		/// <summary>
		/// Gets or sets the URL.
		/// </summary>
		/// <value>
		/// The URL.
		/// </value>
		string Url { get; set; }

		/// <summary>
		/// Idea Status Reason attribute metadata.
		/// </summary>
		OptionSetMetadata IdeaStatusOptionSetMetadata { get; set; }
	}
}
