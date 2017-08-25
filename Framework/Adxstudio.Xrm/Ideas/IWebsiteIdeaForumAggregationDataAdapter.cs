/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Provides methods to get aggregated Idea Forum data for an Adxstudio Portals Website.
	/// </summary>
	public interface IWebsiteIdeaForumAggregationDataAdapter
	{
		/// <summary>
		/// Returns ideas that have been submitted to the idea forums this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first idea to be returned.</param>
		/// <param name="maximumRows">The maximum number of ideas to return.</param>
		IEnumerable<IIdea> SelectIdeas(int startRowIndex = 0, int maximumRows = -1);
	}
}
