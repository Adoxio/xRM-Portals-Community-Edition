/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Represents an Idea in an Adxstudio Portals Idea Forum.
	/// </summary>
	public interface IIdea
	{
		/// <summary>
		/// The email of the author for this idea.
		/// </summary>
		string AuthorEmail { get; }

		/// <summary>
		/// The ID of the author for this idea.
		/// </summary>
		Guid? AuthorId { get; }

		/// <summary>
		/// The name of the author for this idea.
		/// </summary>
		string AuthorName { get; }

		/// <summary>
		/// The number of comments for this idea.
		/// </summary>
		int CommentCount { get; }

		/// <summary>
		/// The comment policy for this idea.
		/// </summary>
		IdeaForumCommentPolicy CommentPolicy { get; }

		/// <summary>
		/// The copy/description for this idea.
		/// </summary>
		string Copy { get; }

		/// <summary>
		/// Whether or not the current user is allowed to comment on this idea.
		/// </summary>
		bool CurrentUserCanComment { get; }

		/// <summary>
		/// The number of votes casted by the current user in the parent idea forum.
		/// </summary>
		int CurrentUserIdeaForumActiveVoteCount { get; }

		/// <summary>
		/// The number of votes casted by the current user for this idea.
		/// </summary>
		int CurrentUserIdeaVoteCount { get; }

		/// <summary>
		/// An adx_idea entity.
		/// </summary>
		Entity Entity { get; }

		/// <summary>
		/// The unique identifier for this idea.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// The URL slug for the parent idea forum of this idea.
		/// </summary>
		string IdeaForumPartialUrl { get; }

		/// <summary>
		/// The title for the parent idea forum of this idea.
		/// </summary>
		string IdeaForumTitle { get; }

		/// <summary>
		/// Whether or not this idea should be visible in the portal.
		/// </summary>
		bool IsApproved { get; }

		/// <summary>
		/// The URL slug for this idea.
		/// </summary>
		string PartialUrl { get; }

		/// <summary>
		/// The idea's current status.
		/// </summary>
		int Status { get; }

		/// <summary>
		/// Comments on the idea's current status.
		/// </summary>
		string StatusComment { get; }

		/// <summary>
		/// When the idea was submitted.
		/// </summary>
		DateTime SubmittedOn { get; }

		/// <summary>
		/// An abstract for this idea.
		/// </summary>
		string Summary { get; }

		/// <summary>
		/// The title for this idea.
		/// </summary>
		string Title { get; }

		/// <summary>
		/// The number of negative votes this idea has.
		/// </summary>
		int VoteDownCount { get; }

		/// <summary>
		/// The number of positive votes this idea has.
		/// </summary>
		int VoteUpCount { get; }

		/// <summary>
		/// The result from subtracting the vote down count from the vote up count.
		/// </summary>
		int VoteSum { get; }

		/// <summary>
		/// The number of users that have voted for this idea.
		/// </summary>
		int VoterCount { get; }

		/// <summary>
		/// The total number of votes casted for this idea.
		/// </summary>
		int VoteTotalCount { get; }

		/// <summary>
		/// The number of votes a user can cast for this idea.
		/// </summary>
		int VotesPerIdea { get; }

		/// <summary>
		/// The number of votes a user can cast in the parent idea forum.
		/// </summary>
		int? VotesPerUser { get; }

		/// <summary>
		/// The voting policy for this idea.
		/// </summary>
		IdeaForumVotingPolicy VotingPolicy { get; }

		/// <summary>
		/// The type of voting (up, up or down, or rating).
		/// </summary>
		IdeaForumVotingType VotingType { get; }

		/// <summary>
		/// Returns whether or not the current user is able to cast a specified number of votes.
		/// </summary>
		/// <param name="voteValue">The number of votes to check for.</param>
		bool CurrentUserCanVote(int voteValue = 1);

		/// <summary>
		/// Gets or sets the URL.
		/// </summary>
		/// <value>
		/// The URL.
		/// </value>
		string Url { get; set; }

		/// <summary>
		/// The Status Option Set Display value.
		/// </summary>
		string StatusDisplayName { get; set; }
	}
}
