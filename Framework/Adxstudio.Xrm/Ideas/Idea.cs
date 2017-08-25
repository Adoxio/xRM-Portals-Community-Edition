/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Represents an Idea in an Adxstudio Portals Idea Forum.
	/// </summary>
	public class Idea : IIdea
	{
		private readonly HttpContextBase _httpContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="entity">An adx_idea entity.</param>
		/// <param name="httpContext"></param>
		/// <param name="commentCount">The number of comments for this idea.</param>
		/// <param name="authorName">The name of the author for this idea.</param>
		/// <param name="authorEmail">The email of the author for this idea.</param>
		/// <param name="ideaForumPartialUrl">The URL slug for the parent idea forum of this idea.</param>
		/// <param name="ideaForumTitle">The title for the parent idea forum of this idea.</param>
		/// <param name="commentPolicy">The comment policy for this idea.</param>
		/// <param name="votingPolicy">The voting policy for this idea.</param>
		/// <param name="votingType">The type of voting (up, up or down, or rating).</param>
		/// <param name="votesPerIdea">The number of votes a user can use on this idea.</param>
		/// <param name="votesPerUser">The number of votes a user can use in the parent idea forum.</param>
		/// <param name="voteUpCount">The number of positive votes this idea has.</param>
		/// <param name="voteDownCount">The number of negative votes this idea has.</param>
		/// <param name="voterCount">The number of users that have voted for this idea.</param>
		/// <param name="currentUserIdeaForumActiveVoteCount">The number of votes casted by the current user in the parent idea forum.</param>
		/// <param name="currentUserIdeaVoteCount">The number of votes casted by the current user for this idea.</param>
		public Idea(
			Entity entity,
			HttpContextBase httpContext,
			int commentCount,
			string authorName,
			string authorEmail,
			string ideaForumPartialUrl,
			string ideaForumTitle,
			IdeaForumCommentPolicy commentPolicy,
			IdeaForumVotingPolicy votingPolicy,
			IdeaForumVotingType votingType,
			int votesPerIdea,
			int? votesPerUser,
			int voteUpCount,
			int voteDownCount,
			int voterCount,
			int currentUserIdeaForumActiveVoteCount,
			int currentUserIdeaVoteCount)
		{
			entity.ThrowOnNull("entity");
			entity.AssertEntityName("adx_idea");
			httpContext.ThrowOnNull("httpContext");
			ThrowOnNegative(commentCount, "commentCount");
			ThrowOnNegative(votesPerIdea, "votesPerIdea");
			ThrowOnNegative(voteUpCount, "voteUpCount");
			ThrowOnNegative(voteDownCount, "voteDownCount");
			ThrowOnNegative(currentUserIdeaForumActiveVoteCount, "currentUserIdeaForumActiveVoteCount");
			ThrowOnNegative(currentUserIdeaVoteCount, "currentUserIdeaVoteCount");

			Entity = entity;
			_httpContext = httpContext;
			AuthorName = authorName;
			AuthorEmail = authorEmail;
			AuthorId = entity.GetAttributeValue<EntityReference>("adx_authorid") == null ? Guid.Empty : entity.GetAttributeValue<EntityReference>("adx_authorid").Id;
			CommentCount = commentCount;
			CommentPolicy = commentPolicy;
			Copy = entity.GetAttributeValue<string>("adx_copy");
			Id = entity.Id;
			IdeaForumPartialUrl = ideaForumPartialUrl;
			IdeaForumTitle = ideaForumTitle;
			IsApproved = entity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);
			PartialUrl = entity.GetAttributeValue<string>("adx_partialurl");
			Status = entity.GetAttributeValue<OptionSetValue>("statuscode") == null ? (int)IdeaStatusCode.Inactive : entity.GetAttributeValue<OptionSetValue>("statuscode").Value;
			StatusDisplayName = entity.FormattedValues["statuscode"] == null ? string.Empty : entity.FormattedValues["statuscode"].ToString();
			StatusComment = entity.GetAttributeValue<string>("adx_statuscomment");
			SubmittedOn = entity.GetAttributeValue<DateTime?>("adx_date") ?? entity.GetAttributeValue<DateTime>("createdon");
			Summary = entity.GetAttributeValue<string>("adx_summary");
			Title = entity.GetAttributeValue<string>("adx_name");
			VoteUpCount = voteUpCount;
			VoteDownCount = voteDownCount;
			VoterCount = voterCount;
			VotesPerIdea = votesPerIdea;
			VotesPerUser = votesPerUser;
			VotingPolicy = votingPolicy;
			VotingType = votingType;
			CurrentUserIdeaForumActiveVoteCount = currentUserIdeaForumActiveVoteCount;
			CurrentUserIdeaVoteCount = currentUserIdeaVoteCount;

			CurrentUserCanComment =
				CommentPolicy == IdeaForumCommentPolicy.Open ||
				CommentPolicy == IdeaForumCommentPolicy.Moderated ||
				CommentPolicy == IdeaForumCommentPolicy.OpenToAuthenticatedUsers && _httpContext.Request.IsAuthenticated;
		}

		/// <summary>
		/// The email of the author for this idea.
		/// </summary>
		public string AuthorEmail { get; private set; }

		/// <summary>
		/// The ID of the author for this idea.
		/// </summary>
		public Guid? AuthorId { get; private set; }

		/// <summary>
		/// The name of the author for this idea.
		/// </summary>
		public string AuthorName { get; private set; }

		/// <summary>
		/// The number of comments for this idea.
		/// </summary>
		public int CommentCount { get; private set; }

		/// <summary>
		/// The comment policy for this idea.
		/// </summary>
		public IdeaForumCommentPolicy CommentPolicy { get; private set; }

		/// <summary>
		/// The copy/description for this idea.
		/// </summary>
		public string Copy { get; private set; }

		/// <summary>
		/// Whether or not the current user is allowed to comment on this idea.
		/// </summary>
		public bool CurrentUserCanComment { get; private set; }

		/// <summary>
		/// The number of votes casted by the current user in the parent idea forum.
		/// </summary>
		public int CurrentUserIdeaForumActiveVoteCount { get; private set; }

		/// <summary>
		/// The number of votes casted by the current user for this idea.
		/// </summary>
		public int CurrentUserIdeaVoteCount { get; private set; }

		/// <summary>
		/// An adx_idea entity.
		/// </summary>
		public Entity Entity { get; private set; }

		/// <summary>
		/// The unique identifier for this idea.
		/// </summary>
		public Guid Id { get; private set; }

		/// <summary>
		/// The URL slug for the parent idea forum of this idea.
		/// </summary>
		public string IdeaForumPartialUrl { get; private set; }

		/// <summary>
		/// The title for the parent idea forum of this idea.
		/// </summary>
		public string IdeaForumTitle { get; private set; }

		/// <summary>
		/// Whether or not this idea should be visible in the portal.
		/// </summary>
		public bool IsApproved { get; private set; }

		/// <summary>
		/// The URL slug for this idea.
		/// </summary>
		public string PartialUrl { get; private set; }

		/// <summary>
		/// The idea's current status.
		/// </summary>
		public int Status { get; private set; }

		/// <summary>
		/// Comments on the idea's current status.
		/// </summary>
		public string StatusComment { get; private set; }

		/// <summary>
		/// When the idea was submitted.
		/// </summary>
		public DateTime SubmittedOn { get; private set; }

		/// <summary>
		/// An abstract for this idea.
		/// </summary>
		public string Summary { get; private set; }

		/// <summary>
		/// The title for this idea.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// The number of negative votes this idea has.
		/// </summary>
		public int VoteDownCount { get; private set; }

		/// <summary>
		/// The number of positive votes this idea has.
		/// </summary>
		public int VoteUpCount { get; private set; }

		/// <summary>
		/// The result from subtracting the vote down count from the vote up count.
		/// </summary>
		public int VoteSum { get { return VoteUpCount - VoteDownCount; } }

		/// <summary>
		/// The number of users that have voted for this idea.
		/// </summary>
		public int VoterCount { get; private set; }

		/// <summary>
		/// The total number of votes casted for this idea.
		/// </summary>
		public int VoteTotalCount { get { return VoteUpCount + VoteDownCount; } }

		/// <summary>
		/// The number of votes a user can cast for this idea.
		/// </summary>
		public int VotesPerIdea { get; private set; }

		/// <summary>
		/// The number of votes a user can cast in the parent idea forum.
		/// </summary>
		public int? VotesPerUser { get; private set; }

		/// <summary>
		/// The voting policy for this idea.
		/// </summary>
		public IdeaForumVotingPolicy VotingPolicy { get; private set; }

		/// <summary>
		/// The type of voting (up, up or down, or rating).
		/// </summary>
		public IdeaForumVotingType VotingType { get; private set; }

		/// <summary>
		/// Returns whether or not the current user is able to cast a specified number of votes.
		/// </summary>
		/// <param name="voteValue">The number of votes to check for.</param>
		public bool CurrentUserCanVote(int voteValue = 1)
		{
			return Status == (int)IdeaStatusCode.Active &&
				CurrentUserIdeaVoteCount + voteValue <= VotesPerIdea &&
				(VotesPerUser == null || CurrentUserIdeaForumActiveVoteCount + voteValue <= VotesPerUser) &&
				(VotingPolicy == IdeaForumVotingPolicy.Open ||
				VotingPolicy == IdeaForumVotingPolicy.OpenToAuthenticatedUsers && _httpContext.Request.IsAuthenticated);
		}

		private static void ThrowOnNegative(int value, string parameterName)
		{
			if (value < 0)
			{
				throw new ArgumentException("Value can't be negative.", parameterName);
			}
		}

		/// <summary>
		/// Gets or sets the URL.
		/// </summary>
		/// <value>
		/// The URL.
		/// </value>
		public string Url { get; set; }

		/// <summary>
		/// The Status Option Set Display value.
		/// </summary>
		public string StatusDisplayName { get; set; }
	}
}
