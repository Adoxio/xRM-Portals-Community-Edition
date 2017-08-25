/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Represents an Issue in an Adxstudio Portals Issue Forum.
	/// </summary>
	public class Issue : IIssue
	{
		private readonly HttpContextBase _httpContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="entity">An adx_issue entity.</param>
		/// <param name="httpContext"></param>
		/// <param name="commentCount">The number of comments for this issue.</param>
		/// <param name="authorName">The name of the author for this issue.</param>
		/// <param name="authorEmail">The email of the author for this issue.</param>
		/// <param name="issueForumPartialUrl">The URL slug for the parent issue forum of this issue.</param>
		/// <param name="issueForumTitle">The title for the parent issue forum of this issue.</param>
		/// <param name="commentPolicy">The comment policy for this issue.</param>
		public Issue(
			Entity entity,
			HttpContextBase httpContext,
			int commentCount,
			string authorName,
			string authorEmail,
			string issueForumPartialUrl,
			string issueForumTitle,
			IssueForumCommentPolicy commentPolicy)
		{
			entity.ThrowOnNull("entity");
			entity.AssertEntityName("adx_issue");
			httpContext.ThrowOnNull("httpContext");
			ThrowOnNegative(commentCount, "commentCount");

			Entity = entity;
			_httpContext = httpContext;
			AuthorName = authorName;
			AuthorEmail = authorEmail;
			AuthorId = entity.GetAttributeValue<EntityReference>("adx_authorid") == null ? Guid.Empty : entity.GetAttributeValue<EntityReference>("adx_authorid").Id;
			CommentCount = commentCount;
			CommentPolicy = commentPolicy;
			Copy = entity.GetAttributeValue<string>("adx_copy");
			Id = entity.Id;
			IssueForumPartialUrl = issueForumPartialUrl;
			IssueForumTitle = issueForumTitle;
			IsApproved = entity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);
			PartialUrl = entity.GetAttributeValue<string>("adx_partialurl");
			Status = entity.GetAttributeValue<OptionSetValue>("statuscode") == null ? (int)IssueStatusCode.Inactive : entity.GetAttributeValue<OptionSetValue>("statuscode").Value;
			StatusComment = entity.GetAttributeValue<string>("adx_statuscomment");
			SubmittedOn = entity.GetAttributeValue<DateTime?>("adx_date") ?? entity.GetAttributeValue<DateTime>("createdon");
			Summary = entity.GetAttributeValue<string>("adx_summary");
			Title = entity.GetAttributeValue<string>("adx_name");

			CurrentUserCanComment =
				CommentPolicy == IssueForumCommentPolicy.Open ||
				CommentPolicy == IssueForumCommentPolicy.Moderated ||
				CommentPolicy == IssueForumCommentPolicy.OpenToAuthenticatedUsers && _httpContext.Request.IsAuthenticated;

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Issue, httpContext, "read_issue", 1, entity.ToEntityReference(), "read");
			}
		}

		/// <summary>
		/// The email of the author for this issue.
		/// </summary>
		public string AuthorEmail { get; private set; }

		/// <summary>
		/// The ID of the author for this issue.
		/// </summary>
		public Guid? AuthorId { get; private set; }

		/// <summary>
		/// The name of the author for this issue.
		/// </summary>
		public string AuthorName { get; private set; }

		/// <summary>
		/// The number of comments for this issue.
		/// </summary>
		public int CommentCount { get; private set; }

		/// <summary>
		/// The comment policy for this issue.
		/// </summary>
		public IssueForumCommentPolicy CommentPolicy { get; private set; }

		/// <summary>
		/// The copy/description for this issue.
		/// </summary>
		public string Copy { get; private set; }

		/// <summary>
		/// Whether or not the current user is allowed to comment on this issue.
		/// </summary>
		public bool CurrentUserCanComment { get; private set; }

		/// <summary>
		/// An adx_issue entity.
		/// </summary>
		public Entity Entity { get; private set; }

		/// <summary>
		/// The unique identifier for this issue.
		/// </summary>
		public Guid Id { get; private set; }

		/// <summary>
		/// The URL slug for the parent issue forum of this issue.
		/// </summary>
		public string IssueForumPartialUrl { get; private set; }

		/// <summary>
		/// The title for the parent issue forum of this issue.
		/// </summary>
		public string IssueForumTitle { get; private set; }

		/// <summary>
		/// Whether or not this issue should be visible in the portal.
		/// </summary>
		public bool IsApproved { get; private set; }

		/// <summary>
		/// The URL slug for this issue.
		/// </summary>
		public string PartialUrl { get; private set; }

		/// <summary>
		/// The issue's current status.
		/// </summary>
		public int Status { get; private set; }

		/// <summary>
		/// Comments on the issue's current status.
		/// </summary>
		public string StatusComment { get; private set; }

		/// <summary>
		/// When the issue was submitted.
		/// </summary>
		public DateTime SubmittedOn { get; private set; }

		/// <summary>
		/// An abstract for this issue.
		/// </summary>
		public string Summary { get; private set; }

		/// <summary>
		/// The title for this issue.
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
