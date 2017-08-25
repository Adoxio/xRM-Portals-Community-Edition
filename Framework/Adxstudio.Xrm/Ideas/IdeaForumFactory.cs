/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Ideas
{
	internal class IdeaForumFactory
	{
		private readonly OrganizationServiceContext _serviceContext;
		private readonly HttpContextBase _httpContext;
		private readonly EntityReference _portalUser;

		public IdeaForumFactory(OrganizationServiceContext serviceContext, HttpContextBase httpContext, EntityReference portalUser)
		{
			serviceContext.ThrowOnNull("serviceContext");
			httpContext.ThrowOnNull("httpContext");

			_serviceContext = serviceContext;
			_httpContext = httpContext;
			_portalUser = portalUser;
		}

		public IEnumerable<IIdeaForum> Create(IEnumerable<Entity> ideaForumEntities)
		{
			var ideaForums = ideaForumEntities.ToArray();
			var ideaForumIds = ideaForums.Select(e => e.Id).ToArray();

			var ideaForumActiveVoteCounts = _serviceContext.FetchIdeaForumActiveVoteCountsForUser(ideaForumIds, _httpContext, _portalUser);

			return ideaForums.Select(e =>
			{
				int currentUserActiveVoteCountValue;
				var currentUserActiveVoteCount = ideaForumActiveVoteCounts.TryGetValue(e.Id, out currentUserActiveVoteCountValue)
					? currentUserActiveVoteCountValue
					: 0;

				return new IdeaForum(
					e,
					_httpContext,
					currentUserActiveVoteCount);
			}).ToArray();
		}
	}
}
