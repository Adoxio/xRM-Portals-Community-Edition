/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Represents an Issue in an Adxstudio Portals Issue Forum.
	/// </summary>
	public interface IIssue
	{
		/// <summary>
		/// The email of the author for this issue.
		/// </summary>
		string AuthorEmail { get; }

		/// <summary>
		/// The ID of the author for this issue.
		/// </summary>
		Guid? AuthorId { get; }

		/// <summary>
		/// The name of the author for this issue.
		/// </summary>
		string AuthorName { get; }

		/// <summary>
		/// The number of comments for this issue.
		/// </summary>
		int CommentCount { get; }

		/// <summary>
		/// The comment policy for this issue.
		/// </summary>
		IssueForumCommentPolicy CommentPolicy { get; }

		/// <summary>
		/// The copy/description for this issue.
		/// </summary>
		string Copy { get; }

		/// <summary>
		/// Whether or not the current user is allowed to comment on this issue.
		/// </summary>
		bool CurrentUserCanComment { get; }

		/// <summary>
		/// An adx_issue entity.
		/// </summary>
		Entity Entity { get; }

		/// <summary>
		/// The unique identifier for this issue.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// The URL slug for the parent issue forum of this issue.
		/// </summary>
		string IssueForumPartialUrl { get; }

		/// <summary>
		/// The title for the parent issue forum of this issue.
		/// </summary>
		string IssueForumTitle { get; }

		/// <summary>
		/// Whether or not this issue should be visible in the portal.
		/// </summary>
		bool IsApproved { get; }

		/// <summary>
		/// The URL slug for this issue.
		/// </summary>
		string PartialUrl { get; }

		/// <summary>
		/// The issue's current status.
		/// </summary>
		int Status { get; }

		/// <summary>
		/// Comments on the issue's current status.
		/// </summary>
		string StatusComment { get; }

		/// <summary>
		/// When the issue was submitted.
		/// </summary>
		DateTime SubmittedOn { get; }

		/// <summary>
		/// An abstract for this issue.
		/// </summary>
		string Summary { get; }

		/// <summary>
		/// The title for this issue.
		/// </summary>
		string Title { get; }
	}
}
