/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Holds an idea and a vote for that idea.
	/// </summary>
	public class IdeaIdeaVotePair : IIdeaIdeaVotePair
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="idea">An <see cref="IIdea"/>.</param>
		/// <param name="ideaVote">An <see cref="IIdeaVote"/> for the paired Idea.</param>
		public IdeaIdeaVotePair(IIdea idea, IIdeaVote ideaVote)
		{
			idea.ThrowOnNull("idea");
			ideaVote.ThrowOnNull("ideaVote");

			Idea = idea;
			IdeaVote = ideaVote;
		}

		/// <summary>
		/// An <see cref="IIdea"/>.
		/// </summary>
		public IIdea Idea { get; private set; }

		/// <summary>
		/// An <see cref="IIdeaVote"/> for the paired Idea.
		/// </summary>
		public IIdeaVote IdeaVote { get; private set; }
	}
}
