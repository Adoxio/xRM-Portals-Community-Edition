/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Ideas
{
	internal class IdeaFactory
	{
		private readonly OrganizationServiceContext _serviceContext;
		private readonly HttpContextBase _httpContext;
		private readonly EntityReference _portalUser;
		
		public IdeaFactory(OrganizationServiceContext serviceContext, HttpContextBase httpContext, EntityReference portalUser)
		{
			serviceContext.ThrowOnNull("serviceContext");
			httpContext.ThrowOnNull("httpContext");

			_serviceContext = serviceContext;
			_httpContext = httpContext;
			_portalUser = portalUser;
		}

		public IEnumerable<IIdea> Create(IEnumerable<Entity> ideaEntities)
		{
			var ideas = ideaEntities.ToArray();
			var ideaIds = ideas.Select(e => e.Id).ToArray();

			var extendedDatas = _serviceContext.FetchIdeaExtendedData(ideaIds);
			var commentCounts = _serviceContext.FetchIdeaCommentCounts(ideaIds);
			var ideaVoteCountsTuples = _serviceContext.FetchIdeaVoteCounts(ideaIds);
			var voteCountsForUserTuples = _serviceContext.FetchIdeaVoteCountsForUser(ideas, _httpContext, _portalUser);

			return ideas.Select(e =>
			{
				IdeaExtendedData extendedDataValue;
				var extendedData = extendedDatas.TryGetValue(e.Id, out extendedDataValue)
					? extendedDataValue
					: IdeaExtendedData.Default;

				int commentCountValue;
				var commentCount = commentCounts.TryGetValue(e.Id, out commentCountValue) ? commentCountValue : 0;

				var authorName = extendedData.AuthorName;
				var authorEmail = e.GetAttributeValue<string>("adx_authoremail") ?? extendedData.AuthorEmail;

				var ideaCommentPolicy = e.GetAttributeValue<OptionSetValue>("adx_commentpolicy") == null ? IdeaCommentPolicy.Inherit : (IdeaCommentPolicy)e.GetAttributeValue<OptionSetValue>("adx_commentpolicy").Value;
				var ideaForumCommentPolicy = ideaCommentPolicy == IdeaCommentPolicy.Inherit
					? extendedData.IdeaForumCommentPolicy
					: (IdeaForumCommentPolicy)(int)ideaCommentPolicy;

				var ideaForumVotesPerIdea = extendedData.IdeaForumVotingType == IdeaForumVotingType.UpOrDown
					? 1
					: extendedData.IdeaForumVotesPerIdea;

				Tuple<int, int, int> ideaVoteCountsTuple;
				var ideaVoteCounts = ideaVoteCountsTuples.TryGetValue(e.Id, out ideaVoteCountsTuple)
					? ideaVoteCountsTuple
					: new Tuple<int, int, int>(0, 0, 0);

				var voteUpCount = ideaVoteCounts.Item1;
				var voteDownCount = ideaVoteCounts.Item2;
				var voterCount = ideaVoteCounts.Item3;
				
				Tuple<int, int> voteCountsForUserTuple;
				var voteCountsForUser = voteCountsForUserTuples.TryGetValue(e.Id, out voteCountsForUserTuple)
					? voteCountsForUserTuple
					: new Tuple<int, int>(0, 0);
				
				var currentUserIdeaForumActiveVoteCount = voteCountsForUser.Item1;
				var currentUserIdeaVoteCount = voteCountsForUser.Item2;

				return new Idea(
					e,
					_httpContext,
					commentCount,
					authorName,
					authorEmail,
					extendedData.IdeaForumTitle,
					extendedData.IdeaForumPartialUrl,
					ideaForumCommentPolicy,
					extendedData.IdeaForumVotingPolicy,
					extendedData.IdeaForumVotingType,
					ideaForumVotesPerIdea,
					extendedData.IdeaForumVotesPerUser,
					voteUpCount,
					voteDownCount,
					voterCount,
					currentUserIdeaForumActiveVoteCount,
					currentUserIdeaVoteCount);
			}).ToArray();
		}
	}
}
