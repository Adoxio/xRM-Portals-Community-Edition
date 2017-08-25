/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Issues;

namespace Site.Areas.Issues.ViewModels
{
	public class IssueViewModel
	{
		public IssueCommentsViewModel Comments { get; set; }

		public bool CurrentUserHasAlert { get; set; }
		
		public IIssue Issue { get; set; }
		
		public IIssueForum IssueForum { get; set; }
	}
}
