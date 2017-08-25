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
	public interface IIdeaIdeaCommentPair
	{
		/// <summary>
		/// An <see cref="IIdea"/>.
		/// </summary>
		IIdea Idea { get; }

		/// <summary>
		/// An <see cref="IComment"/> for the paired Idea.
		/// </summary>
		IComment IdeaComment { get; }
	}
}
