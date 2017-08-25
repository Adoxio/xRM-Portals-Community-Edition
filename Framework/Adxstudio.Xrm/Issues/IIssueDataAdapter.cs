/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Provides methods to get and set data for an Adxstudio Portals Issue such as comments.
	/// </summary>
	public interface IIssueDataAdapter
	{
		/// <summary>
		/// Create an issue alert entity (subscription) for the user.
		/// </summary>
		/// <param name="user">The user to create an issue alert entity (subsciption) for.</param>
		void CreateAlert(EntityReference user);

		/// <summary>
		/// Post a comment for the issue this adapter applies to.
		/// </summary>
		/// <param name="content">The comment copy.</param>
		/// <param name="authorName">The name of the author for this comment (ignored if user is authenticated).</param>
		/// <param name="authorEmail">The email of the author for this comment (ignored if user is authenticated).</param>
		void CreateComment(string content, string authorName = null, string authorEmail = null);

		/// <summary>
		/// Delete an issue alert entity (subscription) for the user.
		/// </summary>
		/// <param name="user">The user to remove an issue alert entity (subsciption) for.</param>
		void DeleteAlert(EntityReference user);

		/// <summary>
		/// Returns whether or not an issue alert entity (subscription) exists for the user.
		/// </summary>
		bool HasAlert();

		/// <summary>
		/// Returns the <see cref="IIssue"/> that this adapter applies to.
		/// </summary>
		IIssue Select();

		/// <summary>
		/// Returns comments that have been posted for the issue this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first comment to be returned.</param>
		/// <param name="maximumRows">The maximum number of comments to return.</param>
		IEnumerable<IComment> SelectComments(int startRowIndex = 0, int maximumRows = -1);

		/// <summary>
		/// Returns the number of comments that have been posted for the issue this adapter applies to.
		/// </summary>
		int SelectCommentCount();
	}
}
