/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Specifies how idea submissions work for an idea forum and how they are displayed.
	/// </summary>
	public enum IdeaForumIdeaSubmissionPolicy
	{
		/// <summary>
		/// Submissions from all users, anonymous and authenticated, are allowed and displayed immediately.
		/// </summary>
		Open                     = 100000000,

		/// <summary>
		/// Only submissions from authenticated users are allowed and they are displayed immediately.
		/// </summary>
		OpenToAuthenticatedUsers = 100000001,

		/// <summary>
		/// Submissions from all users, anonymous or authenticated, are allowed. The submissions will not be displayed until a moderator approves them.
		/// </summary>
		Moderated                = 100000002,

		/// <summary>
		/// Existing submissions are displayed, but no new submissions are allowed.
		/// </summary>
		Closed                   = 100000003
	}
}
