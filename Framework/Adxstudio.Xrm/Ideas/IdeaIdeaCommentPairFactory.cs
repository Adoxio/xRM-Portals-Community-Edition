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
	internal class IdeaIdeaCommentPairFactory
	{
		private readonly OrganizationServiceContext _serviceContext;
		private readonly HttpContextBase _httpContext;
		private readonly EntityReference _portalUser;

		public IdeaIdeaCommentPairFactory(OrganizationServiceContext serviceContext, HttpContextBase httpContext, EntityReference portalUser)
		{
			serviceContext.ThrowOnNull("serviceContext");
			httpContext.ThrowOnNull("httpContext");

			_serviceContext = serviceContext;
			_httpContext = httpContext;
			_portalUser = portalUser;
		}

		public IEnumerable<IIdeaIdeaCommentPair> Create(IEnumerable<Entity> ideaEntities, IEnumerable<Entity> ideaCommentEntities)
		{
			var ideas = new IdeaFactory(_serviceContext, _httpContext, _portalUser).Create(ideaEntities);
			var comments = new IdeaCommentFactory(_serviceContext).Create(ideaCommentEntities);

			return comments.Select(comment =>
				new IdeaIdeaCommentPair(
					ideas.First(idea => idea.Id == (comment.Entity.GetAttributeValue<EntityReference>("adx_ideaid") == null ? Guid.Empty : comment.Entity.GetAttributeValue<EntityReference>("adx_ideaid").Id)),
					comment));
		}
	}
}
