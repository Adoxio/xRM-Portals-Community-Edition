/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Issues;

namespace Site.Areas.Issues.ViewModels
{
	public class IssueForumViewModel
	{
		public IIssueForum IssueForum { get; set; }

		public PaginatedList<IIssue> Issues { get; set; }
	}
}
