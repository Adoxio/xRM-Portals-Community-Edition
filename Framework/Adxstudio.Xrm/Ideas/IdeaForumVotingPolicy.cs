/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Specifies how votes can be cast for an idea in an idea forum.
	/// </summary>
	public enum IdeaForumVotingPolicy
	{
		/// <summary>
		/// Votes from all users, anonymous and authenticated, are allowed.
		/// </summary>
		Open                     = 100000000,

		/// <summary>
		/// Only votes from authenticated users are allowed.
		/// </summary>
		OpenToAuthenticatedUsers = 100000001,

		/// <summary>
		/// Existing votes are displayed, but no new votes are allowed.
		/// </summary>
		Closed                   = 100000002,

		/// <summary>
		/// Voting is disabled and votes are not displayed.
		/// </summary>
		None                     = 100000003
	}
}
