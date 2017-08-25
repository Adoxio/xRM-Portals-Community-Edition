/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Provides methods to get data for an Adxstudio Portals Website for the Adxstudio.Xrm.Ideas namespace.
	/// </summary>
	public interface IWebsiteDataAdapter
	{
		/// <summary>
		/// Returns idea forums that have been created in the website this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first idea forum to be returned.</param>
		/// <param name="maximumRows">The maximum number of idea forums to return.</param>
		IEnumerable<IIdeaForum> SelectIdeaForums(int startRowIndex = 0, int maximumRows = -1);

		/// <summary>
		/// Returns the number of idea forums that have been created in the website this adapter applies to.
		/// </summary>
		int SelectIdeaForumCount();
	}
}
