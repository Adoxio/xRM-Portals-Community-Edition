/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Issues
{
	internal class IssueExtendedData
	{
		private IssueExtendedData()
		{
			IssueForumCommentPolicy = IssueForumCommentPolicy.Closed;
		}

		public IssueExtendedData(
			string authorName,
			string authorEmail,
			string issueForumPartialUrl,
			string issueForumTitle,
			IssueForumCommentPolicy issueForumCommentPolicy)
		{
			AuthorName = authorName;
			AuthorEmail = authorEmail;
			IssueForumPartialUrl = issueForumPartialUrl;
			IssueForumTitle = issueForumTitle;
			IssueForumCommentPolicy = issueForumCommentPolicy;
		}

		public static IssueExtendedData Default { get { return new IssueExtendedData(); } }

		public string AuthorName { get; private set; }

		public string AuthorEmail { get; private set; }

		public string IssueForumPartialUrl { get; private set; }
		
		public string IssueForumTitle { get; private set; }

		public IssueForumCommentPolicy IssueForumCommentPolicy { get; private set; }
	}
}
