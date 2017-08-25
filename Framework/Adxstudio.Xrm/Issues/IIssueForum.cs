/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Represents an Issue Forum in an Adxstudio Portals Website.
	/// </summary>
	public interface IIssueForum
	{
		/// <summary>
		/// An adx_issueforum entity.
		/// </summary>
		Entity Entity { get; }

		/// <summary>
		/// The comment policy for this issue forum.
		/// </summary>
		IssueForumCommentPolicy CommentPolicy { get; }

		/// <summary>
		/// Whether or not the current user is allowed to submit new issues in this issue forum.
		/// </summary>
		bool CurrentUserCanSubmitIssues { get; }

		/// <summary>
		/// The unique identifier for this issue forum.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// The policy for new issue submissions in this issue forum.
		/// </summary>
		IssueForumIssueSubmissionPolicy IssueSubmissionPolicy { get; }

		/// <summary>
		/// The URL slug for this issue forum.
		/// </summary>
		string PartialUrl { get; }

		/// <summary>
		/// An abstract or description for this issue forum.
		/// </summary>
		string Summary { get; }

		/// <summary>
		/// The title for this issue forum.
		/// </summary>
		string Title { get; }
	}
}
