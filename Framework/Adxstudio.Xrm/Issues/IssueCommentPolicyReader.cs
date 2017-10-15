/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Issues
{
	public class IssueCommentPolicyReader : ICommentPolicyReader
	{
		private IIssue _issue;

		public IssueCommentPolicyReader(IIssue issue)
		{
			_issue = issue;
		}

		public bool IsCommentPolicyOpen
		{
			get { return _issue.CommentPolicy == IssueForumCommentPolicy.Open; }
		}

		public bool IsCommentPolicyOpenToAuthenticatedUsers
		{
			get { return _issue.CommentPolicy == IssueForumCommentPolicy.OpenToAuthenticatedUsers; }
		}

		public bool IsCommentPolicyModerated
		{
			get { return _issue.CommentPolicy == IssueForumCommentPolicy.Moderated; }
		}

		public bool IsCommentPolicyClosed
		{
			get { return _issue.CommentPolicy == IssueForumCommentPolicy.Closed; }
		}

		public bool IsCommentPolicyInherit
		{
			get { return false; }
		}

		public bool IsCommentPolicyNone
		{
			get { return _issue.CommentPolicy == IssueForumCommentPolicy.None; }
		}
	}
}
