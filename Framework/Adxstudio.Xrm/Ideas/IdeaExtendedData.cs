/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	internal class IdeaExtendedData
	{
		private IdeaExtendedData()
		{
			IdeaForumCommentPolicy = IdeaForumCommentPolicy.Closed;
			IdeaForumVotingPolicy = IdeaForumVotingPolicy.Closed;
			IdeaForumVotingType = IdeaForumVotingType.UpOnly;
			IdeaForumVotesPerIdea = 0;
		}

		public IdeaExtendedData(
			string authorName,
			string authorEmail,
			string ideaForumPartialUrl,
			string ideaForumTitle,
			IdeaForumCommentPolicy ideaForumCommentPolicy,
			IdeaForumVotingPolicy ideaForumVotingPolicy,
			IdeaForumVotingType ideaForumVotingType,
			int ideaForumVotesPerIdea,
			int? ideaForumVotesPerUser)
		{
			AuthorName = authorName;
			AuthorEmail = authorEmail;
			IdeaForumPartialUrl = ideaForumPartialUrl;
			IdeaForumTitle = ideaForumTitle;
			IdeaForumCommentPolicy = ideaForumCommentPolicy;
			IdeaForumVotingPolicy = ideaForumVotingPolicy;
			IdeaForumVotingType = ideaForumVotingType;
			IdeaForumVotesPerIdea = ideaForumVotesPerIdea;
			IdeaForumVotesPerUser = ideaForumVotesPerUser;
		}

		public static IdeaExtendedData Default { get { return new IdeaExtendedData(); } }

		public string AuthorName { get; private set; }

		public string AuthorEmail { get; private set; }

		public string IdeaForumPartialUrl { get; private set; }
		
		public string IdeaForumTitle { get; private set; }

		public IdeaForumCommentPolicy IdeaForumCommentPolicy { get; private set; }

		public IdeaForumVotingPolicy IdeaForumVotingPolicy { get; private set; }

		public IdeaForumVotingType IdeaForumVotingType { get; private set; }

		public int IdeaForumVotesPerIdea { get; private set; }

		public int? IdeaForumVotesPerUser { get; private set; }
	}
}
