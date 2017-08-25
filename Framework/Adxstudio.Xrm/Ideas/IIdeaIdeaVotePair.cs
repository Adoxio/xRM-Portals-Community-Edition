/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Holds an idea and a vote for that idea.
	/// </summary>
	public interface IIdeaIdeaVotePair
	{
		/// <summary>
		/// An <see cref="IIdea"/>.
		/// </summary>
		IIdea Idea { get; }

		/// <summary>
		/// An <see cref="IIdeaVote"/> for the paired Idea.
		/// </summary>
		IIdeaVote IdeaVote { get; }
	}
}
