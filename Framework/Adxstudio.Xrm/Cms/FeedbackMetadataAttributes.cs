/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adxstudio.Xrm.Core.Flighting;

namespace Adxstudio.Xrm.Cms
{
	internal static class FeedbackMetadataAttributes
	{
		#region Adx_pagecomment to Feedback mapping properties

		public const string PageCommentEntityName = "feedback";

		public const string CommentAttribute = "comments";

		#endregion

		#region Adx_webpage to Feedback mapping properties
		
		public const string AuthorNameAttribute = "adx_createdbycontact";

		public const string WebPageIdAttribute = "regardingobjectid";

		public const string PageCommentIdAttribute = "feedbackid";

		#endregion

		#region Adx_rating to Feedback mapping properties

		public const string RatingEntityName = "feedback";

		public const string VisitorAttributeName = "adx_contactusername";

		public const string UserIdAttributeName = "createdbycontact";

		public const string ActivityIdAttributeName = "feedbackid";

		public const string RatingValueAttributeName = "rating";

		public const string MinRatingAttributeName = "minrating";

		public const string MaxRatingAttributeName = "maxrating";

		#endregion

		#region Adx_blogpostcomment to Feedback mapping properties

		public const string BlogPostCommentEntityName = "feedback";

		public const string BlogPostIdAttribute = "regardingobjectid";

		public const string BlogPostCommentIdAttribute = "feedbackid";

		public const string CommentContentAttribute = "comments";

		#endregion

		public const string DateAttribute = "modifiedon";
	}
}
