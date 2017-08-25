/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Provides methods to get aggregated Idea Forum data for an Adxstudio Portals Website.
	/// </summary>
	/// <remarks>Ideas are returned reverse chronologically by their submission date.</remarks>
	public class WebsiteIdeaForumAggregationByNewDataAdapter : IWebsiteIdeaForumAggregationDataAdapter
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="dependencies">The dependencies to use for getting data.</param>
		public WebsiteIdeaForumAggregationByNewDataAdapter(IDataAdapterDependencies dependencies)
		{
			dependencies.ThrowOnNull("dependencies");

			var website = dependencies.GetWebsite();
			website.ThrowOnNull("dependencies", ResourceManager.GetString("Website_Reference_Retrieval_Exception"));
			website.AssertLogicalName("adx_website");

			Website = website;
			Dependencies = dependencies;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="portalName">The configured name of the portal to get data for.</param>
		public WebsiteIdeaForumAggregationByNewDataAdapter(string portalName = null) : this(new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected EntityReference Website { get; private set; }

		/// <summary>
		/// Returns ideas that have been submitted to the idea forums this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first idea to be returned.</param>
		/// <param name="maximumRows">The maximum number of ideas to return.</param>
		public IEnumerable<IIdea> SelectIdeas(int startRowIndex = 0, int maximumRows = -1)
		{
			if (startRowIndex < 0)
			{
                throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IIdea[] { };
			}

			var serviceContext = Dependencies.GetServiceContext();
			var security = Dependencies.GetSecurityProvider();

			var query = serviceContext.CreateQuery("adx_idea")
				.Join(serviceContext.CreateQuery("adx_ideaforum"), idea => idea.GetAttributeValue<EntityReference>("adx_ideaforumid").Id, ideaForum => ideaForum.GetAttributeValue<Guid>("adx_ideaforumid"), (idea, ideaForum) => new { Idea = idea, IdeaForum = ideaForum })
				.Where(a => a.IdeaForum.GetAttributeValue<EntityReference>("adx_websiteid") == Website)
				.Where(a => a.Idea.GetAttributeValue<EntityReference>("adx_ideaforumid") != null && a.Idea.GetAttributeValue<OptionSetValue>("statecode") != null && a.Idea.GetAttributeValue<OptionSetValue>("statecode").Value == 0
					&& a.Idea.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false));

			query = query.OrderByDescending(a => a.Idea.GetAttributeValue<DateTime?>("adx_date"));

			if (maximumRows < 0)
			{
				var entities = query.Select(a => a.Idea).ToArray()
					.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read))
					.Skip(startRowIndex);

				return new IdeaFactory(serviceContext, Dependencies.GetHttpContext(), Dependencies.GetPortalUser()).Create(entities);
			}

			var pagedQuery = query.Select(a => a.Idea);

			var paginator = new PostFilterPaginator<Entity>(
				(offset, limit) => pagedQuery.Skip(offset).Take(limit).ToArray(),
				e => security.TryAssert(serviceContext, e, CrmEntityRight.Read),
				2);

			return new IdeaFactory(serviceContext, Dependencies.GetHttpContext(), Dependencies.GetPortalUser()).Create(paginator.Select(startRowIndex, maximumRows));
		}
	}
}
