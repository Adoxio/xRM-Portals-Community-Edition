/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Represents an Idea Forum in an Adxstudio Portals Website.
	/// </summary>
	public class IdeaForum : IIdeaForum
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="entity">An adx_ideaforum entity.</param>
		/// <param name="httpContext"></param>
		/// <param name="currentUserActiveVoteCount">The number of votes casted by the user in this idea forum.</param>
		public IdeaForum(Entity entity, HttpContextBase httpContext, int currentUserActiveVoteCount)
		{
			entity.ThrowOnNull("entity");
			entity.AssertEntityName("adx_ideaforum");
			httpContext.ThrowOnNull("httpContext");
			ThrowOnNegative(currentUserActiveVoteCount, "currentUserActiveVoteCount");

			Entity = entity;
			CurrentUserActiveVoteCount = currentUserActiveVoteCount;
			CommentPolicy = entity.GetAttributeValue<OptionSetValue>("adx_commentpolicy") == null ? IdeaForumCommentPolicy.Open : (IdeaForumCommentPolicy)entity.GetAttributeValue<OptionSetValue>("adx_commentpolicy").Value;
			Id = entity.Id;
			IdeaSubmissionPolicy = entity.GetAttributeValue<OptionSetValue>("adx_ideasubmissionpolicy") == null ? IdeaForumIdeaSubmissionPolicy.Open : (IdeaForumIdeaSubmissionPolicy)entity.GetAttributeValue<OptionSetValue>("adx_ideasubmissionpolicy").Value;
			PartialUrl = entity.GetAttributeValue<string>("adx_partialurl");
			Summary = entity.GetAttributeValue<string>("adx_summary");
			Title = entity.GetAttributeValue<string>("adx_name");
			VotesPerUser = entity.GetAttributeValue<int?>("adx_votesperuser");
			VotingPolicy = entity.GetAttributeValue<OptionSetValue>("adx_votingpolicy") == null ? IdeaForumVotingPolicy.Open : (IdeaForumVotingPolicy)entity.GetAttributeValue<OptionSetValue>("adx_votingpolicy").Value;

			CurrentUserCanSubmitIdeas = (VotesPerUser == null || CurrentUserActiveVoteCount < VotesPerUser) &&
				IdeaSubmissionPolicy == IdeaForumIdeaSubmissionPolicy.Open ||
				IdeaSubmissionPolicy == IdeaForumIdeaSubmissionPolicy.Moderated ||
				IdeaSubmissionPolicy == IdeaForumIdeaSubmissionPolicy.OpenToAuthenticatedUsers && httpContext.Request.IsAuthenticated;
		}

		/// <summary>
		/// The comment policy for this idea forum.
		/// </summary>
		public IdeaForumCommentPolicy CommentPolicy { get; private set; }

		/// <summary>
		/// The number of votes casted by the current user in this idea forum.
		/// </summary>
		public int CurrentUserActiveVoteCount { get; private set; }

		/// <summary>
		/// Whether or not the current user is allowed to submit new ideas in this idea forum.
		/// </summary>
		public bool CurrentUserCanSubmitIdeas { get; private set; }

		/// <summary>
		/// An adx_ideaforum entity.
		/// </summary>
		public Entity Entity { get; private set; }

		/// <summary>
		/// The unique identifier for this idea forum.
		/// </summary>
		public Guid Id { get; private set; }

		/// <summary>
		/// The policy for new idea submissions in this idea forum.
		/// </summary>
		public IdeaForumIdeaSubmissionPolicy IdeaSubmissionPolicy { get; private set; }

		/// <summary>
		/// The URL slug for this idea forum.
		/// </summary>
		public string PartialUrl { get; private set; }

		/// <summary>
		/// An abstract or description for this idea forum.
		/// </summary>
		public string Summary { get; private set; }

		/// <summary>
		/// The title for this idea forum.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// The number of votes a user can cast in this idea forum.
		/// </summary>
		public int? VotesPerUser { get; private set; }

		/// <summary>
		/// The voting policy for this idea forum.
		/// </summary>
		public IdeaForumVotingPolicy VotingPolicy { get; private set; }

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
		/// Idea Status Reason attribute metadata.
		/// </summary>
		public OptionSetMetadata IdeaStatusOptionSetMetadata { get; set; }
	}
}
