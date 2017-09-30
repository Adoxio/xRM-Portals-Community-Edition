/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Provides methods to get data for an Adxstudio Portals Website for the Adxstudio.Xrm.Ideas namespace.
	/// </summary>
	/// <remarks>Idea Forums are returned alphabetically by their title.</remarks>
	public class WebsiteDataAdapter : IWebsiteDataAdapter
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="dependencies">The dependencies to use for getting data.</param>
		public WebsiteDataAdapter(IDataAdapterDependencies dependencies)
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
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public WebsiteDataAdapter(string portalName = null) : this(new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected EntityReference Website { get; private set; }

		/// <summary>
		/// Returns idea forums that have been created in the website this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first idea forum to be returned.</param>
		/// <param name="maximumRows">The maximum number of idea forums to return.</param>
		public virtual IEnumerable<IIdeaForum> SelectIdeaForums(int startRowIndex = 0, int maximumRows = -1)
		{
			if (startRowIndex < 0)
			{
                throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IIdeaForum[] { };
			}

			var serviceContext = Dependencies.GetServiceContext();
			var security = Dependencies.GetSecurityProvider();

			var query = serviceContext.CreateQuery("adx_ideaforum")
				.Where(ideaForum => ideaForum.GetAttributeValue<EntityReference>("adx_websiteid") == Website
					&& ideaForum.GetAttributeValue<OptionSetValue>("statecode") != null && ideaForum.GetAttributeValue<OptionSetValue>("statecode").Value == 0);

			var languageInfo = HttpContext.Current.GetContextLanguageInfo();
			if (languageInfo.IsCrmMultiLanguageEnabled)
			{
				query = query.Where(ideaForum => ideaForum.GetAttributeValue<EntityReference>("adx_websitelanguageid") == null
						|| ideaForum.GetAttributeValue<EntityReference>("adx_websitelanguageid").Id == languageInfo.ContextLanguage.EntityReference.Id);
			}

			query = query.OrderBy(ideaForum => ideaForum.GetAttributeValue<string>("adx_name"));

			if (maximumRows < 0)
			{
				var entities = query.ToArray()
					.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read))
					.Skip(startRowIndex);

				return new IdeaForumFactory(serviceContext, Dependencies.GetHttpContext(), Dependencies.GetPortalUser()).Create(entities);
			}

			var pagedQuery = query;

			var paginator = new PostFilterPaginator<Entity>(
				(offset, limit) => pagedQuery.Skip(offset).Take(limit).ToArray(),
				e => security.TryAssert(serviceContext, e, CrmEntityRight.Read),
				2);

			return new IdeaForumFactory(serviceContext, Dependencies.GetHttpContext(), Dependencies.GetPortalUser()).Create(paginator.Select(startRowIndex, maximumRows));
		}

		/// <summary>
		/// Returns the number of idea forums that have been created in the website this adapter applies to.
		/// </summary>
		public virtual int SelectIdeaForumCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			return serviceContext.FetchCount("adx_ideaforum", "adx_ideaforumid", addCondition =>
			{
				addCondition("adx_websiteid", "eq", Website.Id.ToString());
				addCondition("statecode", "eq", "0");
			});
		}
	}
}
