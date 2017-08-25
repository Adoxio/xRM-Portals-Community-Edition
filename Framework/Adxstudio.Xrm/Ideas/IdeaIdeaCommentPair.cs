/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Holds an idea and a comment for that idea.
	/// </summary>
	public class IdeaIdeaCommentPair : IIdeaIdeaCommentPair
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="idea">An <see cref="IIdea"/>.</param>
		/// <param name="ideaComment">An <see cref="IComment"/> for the paired Idea.</param>
		public IdeaIdeaCommentPair(IIdea idea, IComment ideaComment)
		{
			idea.ThrowOnNull("idea");
			ideaComment.ThrowOnNull("ideaComment");

			Idea = idea;
			IdeaComment = ideaComment;
		}

		/// <summary>
		/// An <see cref="IIdea"/>.
		/// </summary>
		public IIdea Idea { get; private set; }

		/// <summary>
		/// An <see cref="IComment"/> for the paired Idea.
		/// </summary>
		public IComment IdeaComment { get; private set; }
	}
}
