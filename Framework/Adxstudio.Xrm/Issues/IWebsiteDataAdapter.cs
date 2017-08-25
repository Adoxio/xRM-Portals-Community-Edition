/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Provides methods to get data for an Adxstudio Portals Website for the Adxstudio.Xrm.Issues namespace.
	/// </summary>
	public interface IWebsiteDataAdapter
	{
		/// <summary>
		/// Returns issue forums that have been created in the website this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first issue forum to be returned.</param>
		/// <param name="maximumRows">The maximum number of issue forums to return.</param>
		IEnumerable<IIssueForum> SelectIssueForums(int startRowIndex = 0, int maximumRows = -1);

		/// <summary>
		/// Returns the number of issue forums that have been created in the website this adapter applies to.
		/// </summary>
		int SelectIssueForumCount();
	}
}
