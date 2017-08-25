/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Provides methods to get and set data for an Adxstudio Portals Idea Forum such as ideas.
	/// </summary>
	public interface IIdeaForumDataAdapter
	{
		/// <summary>
		/// Min Date from which to search for ideas
		/// </summary>
		DateTime? MinDate { get; set; }

		/// <summary>
		/// Type of ideas in which to display
		/// </summary>
		int? Status { get; set; }

		/// <summary>
		/// Submit an idea to the idea forum this aadapter applies to.
		/// </summary>
		/// <param name="title">The title of the idea.</param>
		/// <param name="copy">The copy of the idea.</param>
		/// <param name="authorName">The name of the author for the idea (ignored if user is authenticated).</param>
		/// <param name="authorEmail">The email of the author for the idea (ignored if user is authenticated).</param>
		void CreateIdea(string title, string copy, string authorName = null, string authorEmail = null);

		/// <summary>
		/// Returns the <see cref="IIdeaForum"/> that this adapter applies to.
		/// </summary>
		IIdeaForum Select();

		/// <summary>
		/// Returns ideas that have been submitted to the idea forum this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first idea to be returned.</param>
		/// <param name="maximumRows">The maximum number of ideas to return.</param>
		IEnumerable<IIdea> SelectIdeas(int startRowIndex = 0, int maximumRows = -1);

		/// <summary>
		/// Returns the number of ideas that have been submitted to the idea forum this adapter applies to.
		/// </summary>
		/// <returns></returns>
		int SelectIdeaCount();
	}
}
