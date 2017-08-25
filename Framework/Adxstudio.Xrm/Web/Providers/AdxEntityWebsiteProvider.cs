/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Providers
{
	using System;
	using System.Collections.Generic;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal.Web.Providers;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Adxstudio.Xrm.Services;

	public class AdxEntityWebsiteProvider : EntityWebsiteProvider
	{
		public override Entity GetWebsite(OrganizationServiceContext context, Entity entity)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (entity == null)
			{
				return null;
			}

			var entitiesWithoutWebsites = new Dictionary<string, Relationship>
			{
				// extended portal entities
				{ "adx_communityforumthread", "adx_communityforum_communityforumthread".ToRelationship() },
				{ "adx_communityforumpost", "adx_communityforumthread_communityforumpost".ToRelationship() },
				{ "adx_eventschedule", "adx_event_eventschedule".ToRelationship() },
				{ "adx_blogpost", "adx_blog_blogpost".ToRelationship() },
				{ "adx_blogpostcomment", "adx_blogpost_blogpostcomment".ToRelationship() },

				// base portal entities
				{ "adx_weblink", "adx_weblinkset_weblink".ToRelationship() }
			};

			Relationship hasWebsiteRelationship;

			if (entitiesWithoutWebsites.TryGetValue(entity.LogicalName, out hasWebsiteRelationship))
			{
				return GetWebsite(context, context.RetrieveRelatedEntity(entity, hasWebsiteRelationship));
			}

			var lookup = new Dictionary<string, Relationship>
			{
				// extended portal entities
				{ "adx_communityforum", "adx_website_communityforum".ToRelationship() },
				{ "adx_eventsponsor", "adx_website_eventsponsor".ToRelationship() },
				{ "adx_eventspeaker", "adx_website_eventspeaker".ToRelationship() },
				{ "adx_event", "adx_website_event".ToRelationship() },
				{ "adx_publishingstate", "adx_website_publishingstate".ToRelationship() },
				{ "adx_survey", "adx_website_survey".ToRelationship() },
				{ "adx_blog", "adx_website_blog".ToRelationship() },

				// base portal entities
				{ "adx_weblinkset", "adx_website_weblinkset".ToRelationship() },
				{ "adx_webpage", "adx_website_webpage".ToRelationship() },
				{ "adx_webfile", "adx_website_webfile".ToRelationship() },
				{ "adx_sitemarker", "adx_website_sitemarker".ToRelationship() },
				{ "adx_pagetemplate", "adx_website_pagetemplate".ToRelationship() },
				{ "adx_contentsnippet", "adx_website_contentsnippet".ToRelationship() }
			};

			Relationship websiteRelationship;

			if (lookup.TryGetValue(entity.LogicalName, out websiteRelationship))
			{
				return context.RetrieveRelatedEntity(entity, websiteRelationship);
			}

			return null;
		}
	}
}
