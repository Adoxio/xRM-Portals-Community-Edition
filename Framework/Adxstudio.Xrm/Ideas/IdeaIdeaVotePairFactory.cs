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
	internal class IdeaIdeaVotePairFactory
	{
		private readonly OrganizationServiceContext _serviceContext;
		private readonly HttpContextBase _httpContext;
		private readonly EntityReference _portalUser;

		public IdeaIdeaVotePairFactory(OrganizationServiceContext serviceContext, HttpContextBase httpContext, EntityReference portalUser)
		{
			serviceContext.ThrowOnNull("serviceContext");
			httpContext.ThrowOnNull("httpContext");

			_serviceContext = serviceContext;
			_httpContext = httpContext;
			_portalUser = portalUser;
		}

		public IEnumerable<IIdeaIdeaVotePair> Create(IEnumerable<Entity> ideaEntities, IEnumerable<Entity> ideaVoteEntities)
		{
			var ideas = new IdeaFactory(_serviceContext, _httpContext, _portalUser).Create(ideaEntities);

			var votes = ideaVoteEntities.ToArray();

			return votes.Select(vote =>
				new IdeaIdeaVotePair(
					ideas.First(idea => idea.Entity.ToEntityReference() == vote.GetAttributeValue<EntityReference>("adx_ideaid")),
					new IdeaVote(vote)));
		}
	}
}
