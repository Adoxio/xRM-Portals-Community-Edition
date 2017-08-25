/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Specifies the type of voting for an idea forum.
	/// </summary>
	public enum IdeaForumVotingType
	{
		/// <summary>
		/// All votes are cumulative; i.e. increase the total count.
		/// </summary>
		UpOnly   = 100000000,

		/// <summary>
		/// Votes can be positive or negative.
		/// </summary>
		UpOrDown = 100000001,

		/// <summary>
		/// Voting is presented to the user as a rating.
		/// </summary>
		Rating   = 100000002,

		/// <summary>
		/// Default value used for entity creation
		/// </summary>
		Default = -1,
	}
}
