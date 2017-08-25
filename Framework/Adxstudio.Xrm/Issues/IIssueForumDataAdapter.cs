/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Provides methods to get and set data for an Adxstudio Portals Issue Forum such as issues.
	/// </summary>
	public interface IIssueForumDataAdapter
	{
		/// <summary>
		/// Submit an issue to the issue forum this aadapter applies to.
		/// </summary>
		/// <param name="title">The title of the issue.</param>
		/// <param name="copy">The copy of the issue.</param>
		/// <param name="track">Create an issue alert for the current user (user must be authenticated).</param>
		/// <param name="authorName">The name of the author for the issue (ignored if user is authenticated).</param>
		/// <param name="authorEmail">The email of the author for the issue (ignored if user is authenticated).</param>
		void CreateIssue(string title, string copy, bool track = false, string authorName = null, string authorEmail = null);

		/// <summary>
		/// Returns the <see cref="IIssueForum"/> that this adapter applies to.
		/// </summary>
		IIssueForum Select();

		/// <summary>
		/// Returns issues that have been submitted to the issue forum this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first issue to be returned.</param>
		/// <param name="maximumRows">The maximum number of issues to return.</param>
		IEnumerable<IIssue> SelectIssues(int startRowIndex = 0, int maximumRows = -1);

		/// <summary>
		/// Returns the number of issues that have been submitted to the issue forum this adapter applies to.
		/// </summary>
		/// <returns></returns>
		int SelectIssueCount();
	}
}
