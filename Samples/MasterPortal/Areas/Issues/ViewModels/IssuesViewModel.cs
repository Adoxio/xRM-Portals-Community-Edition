/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Issues;

namespace Site.Areas.Issues.ViewModels
{
	public class IssuesViewModel
	{
		public int IssueForumCount { get; set; }

		public IEnumerable<IIssueForum> IssueForums { get; set; }
	}
}
