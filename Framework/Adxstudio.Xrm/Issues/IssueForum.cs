/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Represents an Issue Forum in an Adxstudio Portals Website.
	/// </summary>
	public class IssueForum : IIssueForum
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="entity">An adx_issueforum entity.</param>
		/// <param name="httpContext"></param>
		public IssueForum(Entity entity, HttpContextBase httpContext)
		{
			entity.ThrowOnNull("entity");
			entity.AssertEntityName("adx_issueforum");
			httpContext.ThrowOnNull("httpContext");

			Entity = entity;
			CommentPolicy = entity.GetAttributeValue<OptionSetValue>("adx_commentpolicy") == null ? IssueForumCommentPolicy.Open : (IssueForumCommentPolicy)entity.GetAttributeValue<OptionSetValue>("adx_commentpolicy").Value;
			Id = entity.Id;
			IssueSubmissionPolicy = entity.GetAttributeValue<OptionSetValue>("adx_issuesubmissionpolicy") == null ? IssueForumIssueSubmissionPolicy.Open : (IssueForumIssueSubmissionPolicy)entity.GetAttributeValue<OptionSetValue>("adx_issuesubmissionpolicy").Value;
			PartialUrl = entity.GetAttributeValue<string>("adx_partialurl");
			Summary = entity.GetAttributeValue<string>("adx_summary");
			Title = entity.GetAttributeValue<string>("adx_name");

			CurrentUserCanSubmitIssues =
				IssueSubmissionPolicy == IssueForumIssueSubmissionPolicy.Open ||
				IssueSubmissionPolicy == IssueForumIssueSubmissionPolicy.Moderated ||
				IssueSubmissionPolicy == IssueForumIssueSubmissionPolicy.OpenToAuthenticatedUsers && httpContext.Request.IsAuthenticated;
		}

		/// <summary>
		/// The comment policy for this issue forum.
		/// </summary>
		public IssueForumCommentPolicy CommentPolicy { get; private set; }

		/// <summary>
		/// Whether or not the current user is allowed to submit new issues in this issue forum.
		/// </summary>
		public bool CurrentUserCanSubmitIssues { get; private set; }

		/// <summary>
		/// An adx_issueforum entity.
		/// </summary>
		public Entity Entity { get; private set; }

		/// <summary>
		/// The unique identifier for this issue forum.
		/// </summary>
		public Guid Id { get; private set; }

		/// <summary>
		/// The policy for new issue submissions in this issue forum.
		/// </summary>
		public IssueForumIssueSubmissionPolicy IssueSubmissionPolicy { get; private set; }

		/// <summary>
		/// The URL slug for this issue forum.
		/// </summary>
		public string PartialUrl { get; private set; }

		/// <summary>
		/// An abstract or description for this issue forum.
		/// </summary>
		public string Summary { get; private set; }

		/// <summary>
		/// The title for this issue forum.
		/// </summary>
		public string Title { get; private set; }

		private static void ThrowOnNegative(int value, string parameterName)
		{
			if (value < 0)
			{
				throw new ArgumentException("Value can't be negative.", parameterName);
			}
		}
	}
}
