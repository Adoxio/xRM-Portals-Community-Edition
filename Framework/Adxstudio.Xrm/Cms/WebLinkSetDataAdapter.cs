/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Adxstudio.Xrm.Services;

namespace Adxstudio.Xrm.Cms
{
	public class WebLinkSetDataAdapter : IWebLinkSetDataAdapter
	{
		public WebLinkSetDataAdapter(IDataAdapterDependencies dependencies)
		{
			if (dependencies == null)
			{
				throw new ArgumentNullException("dependencies");
			}

			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		public IWebLinkSet Select(Guid webLinkSetId)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}", webLinkSetId));

			var webLinkSet = Select(e => e.GetAttributeValue<Guid>("adx_weblinksetid") == webLinkSetId);

			if (webLinkSet == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}", webLinkSetId));

			return webLinkSet;
		}

		public IWebLinkSet Select(string webLinkSetName)
		{
			if (string.IsNullOrEmpty(webLinkSetName))
			{
				return null;
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var webLinkSet = Select(e => e.GetAttributeValue<string>("adx_name") == webLinkSetName);

			if (webLinkSet == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return webLinkSet;
		}

	
		protected virtual IWebLinkSet Select(Predicate<Entity> match)
		{
			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();
			ContextLanguageInfo languageInfo = HttpContext.Current.GetContextLanguageInfo();

			// Bulk-load all web link set entities into cache.
			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_weblinkset")
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions = !languageInfo.IsCrmMultiLanguageEnabled 
								? new[] { new Condition("adx_websiteid", ConditionOperator.Equal, website.Id) } 
								: new[] { new Condition("adx_websiteid", ConditionOperator.Equal, website.Id),
											new Condition("adx_websitelanguageid", ConditionOperator.Equal, languageInfo.ContextLanguage.EntityReference.Id) }
						}
					}
				}
			};

			var allEntities = HttpContext.Current.GetOrganizationService().RetrieveMultiple(fetch).Entities;
			var entity = allEntities.FirstOrDefault(e => match(e) && IsActive(e));

			if (entity == null)
			{
				return null;
			}

			var securityProvider = Dependencies.GetSecurityProvider();

			if (!securityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Read))
			{
				return null;
			}

			var urlProvider = Dependencies.GetUrlProvider();

			fetch = new Fetch
			{
				Entity = new FetchEntity("adx_weblink")
				{
					Orders = new[] { new Order("adx_displayorder") },
					Filters = new[]
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("statecode", ConditionOperator.Equal, 0),
								new Condition("adx_weblinksetid", ConditionOperator.Equal, entity.Id)
							}
						}
					}
				}
			};

			var webLinkEntities = HttpContext.Current.GetOrganizationService().RetrieveMultiple(fetch).Entities
				.Where(e => securityProvider.TryAssert(serviceContext, e, CrmEntityRight.Read)).ToArray();

			var childWebLinkLookup = webLinkEntities
				.Where(e => e.GetAttributeValue<EntityReference>("adx_parentweblinkid") != null)
				.ToLookup(e => e.GetAttributeValue<EntityReference>("adx_parentweblinkid").Id);

			var topLevelWebLinks = webLinkEntities
				.Where(e => e.GetAttributeValue<EntityReference>("adx_parentweblinkid") == null)
				.Select(e => CreateWebLink(serviceContext, e, securityProvider, urlProvider, childWebLinkLookup));

			var webLinkSet = new WebLinkSet(entity, new PortalViewEntity(serviceContext, entity, securityProvider, urlProvider), topLevelWebLinks);

			return webLinkSet;
		}

		public IEnumerable<IWebLink> SelectWebLinks(Guid webLinkSetId)
		{
			var webLinkSet = Select(webLinkSetId);

			return webLinkSet == null ? new IWebLink[] { } : webLinkSet.WebLinks;
		}

		public IEnumerable<IWebLink> SelectWebLinks(string webLinkSetName)
		{
			var webLinkSet = Select(webLinkSetName);

			return webLinkSet == null ? new IWebLink[] { } : webLinkSet.WebLinks;
		}

		private static IWebLink CreateWebLink(OrganizationServiceContext serviceContext, Entity entity, ICrmEntitySecurityProvider securityProvider, IEntityUrlProvider urlProvider, ILookup<Guid, Entity> childWebLinkLookup)
		{
			return new WebLink(
				entity,
				new PortalViewEntity(serviceContext, entity, securityProvider, urlProvider),
				urlProvider.GetApplicationPath(serviceContext, entity),
				CreateWebLinkSubTree(serviceContext, entity.ToEntityReference(), securityProvider, urlProvider, childWebLinkLookup));
		}

		private static IEnumerable<IWebLink> CreateWebLinkSubTree(OrganizationServiceContext serviceContext, EntityReference entity, ICrmEntitySecurityProvider securityProvider, IEntityUrlProvider urlProvider, ILookup<Guid, Entity> childWebLinkLookup)
		{
			return childWebLinkLookup[entity.Id]
				.Select(e => CreateWebLink(serviceContext, e, securityProvider, urlProvider, childWebLinkLookup));
		}

		private static bool IsActive(Entity entity)
		{
			if (entity == null)
			{
				return false;
			}

			var statecode = entity.GetAttributeValue<OptionSetValue>("statecode");

			return statecode != null && statecode.Value == 0;
		}
	}
}
