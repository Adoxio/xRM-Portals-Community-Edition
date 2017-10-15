/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Blogs
{
	public class BlogCommentPolicyReader : ICommentPolicyReader
	{
		private IBlogPost _blogPost;

		public BlogCommentPolicyReader(IBlogPost blogPost)
		{
			_blogPost = blogPost;
		}

		public bool IsCommentPolicyOpen
		{
			get { return _blogPost.CommentPolicy == BlogCommentPolicy.Open; }
		}

		public bool IsCommentPolicyOpenToAuthenticatedUsers
		{
			get { return _blogPost.CommentPolicy == BlogCommentPolicy.OpenToAuthenticatedUsers; }
		}

		public bool IsCommentPolicyModerated
		{
			get { return _blogPost.CommentPolicy == BlogCommentPolicy.Moderated; }
		}

		public bool IsCommentPolicyClosed
		{
			get { return _blogPost.CommentPolicy == BlogCommentPolicy.Closed; }
		}

		public bool IsCommentPolicyNone
		{
			get { return _blogPost.CommentPolicy == BlogCommentPolicy.None; }
		}
	}
}
