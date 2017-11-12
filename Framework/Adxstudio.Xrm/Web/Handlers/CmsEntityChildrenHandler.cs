/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Data.Services;
using Adxstudio.Xrm.Web.Providers;
using Adxstudio.Xrm.Web.Routing;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Handlers
{
	public class CmsEntityChildrenHandler : CmsEntityHandler
	{
		public CmsEntityChildrenHandler() { }

		public CmsEntityChildrenHandler(string portalName, Guid? portalScopeId, string entityLogicalName, Guid? id) : base(portalName, portalScopeId, entityLogicalName, id) { }

		protected override void ProcessRequest(HttpContext context, ICmsEntityServiceProvider serviceProvider, Guid portalScopeId, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, ICrmEntitySecurityProvider security)
		{
			if (!IsRequestMethod(context.Request, "GET"))
			{
				throw new CmsEntityServiceException(HttpStatusCode.MethodNotAllowed, "Request method {0} not allowed for this resource.".FormatWith(context.Request.HttpMethod));
			}

			var children = GetChildren(serviceContext, entity, entityMetadata)
				.Where(child => security.TryAssert(serviceContext, child, CrmEntityRight.Read))
				.ToList();

			children.Sort(new EntitySiteMapDisplayOrderComparer());

			var entityMetadataCache = new Dictionary<string, CmsEntityMetadata>();

			var childInfos = children.Select(e =>
			{
				var info = new ExtendedSiteMapChildInfo
				{
					Title = GetChildTitle(e),
					EntityUri = VirtualPathUtility.ToAbsolute(CmsEntityRouteHandler.GetAppRelativePath(portalScopeId, e.ToEntityReference())),
					HasPermission = security.TryAssert(serviceContext, e, CrmEntityRight.Change),
					Id = e.Id,
					LogicalName = e.LogicalName,
					HiddenFromSiteMap = e.Attributes.Contains("adx_hiddenfromsitemap") && e.GetAttributeValue<bool?>("adx_hiddenfromsitemap").GetValueOrDefault(),
					Url = e.Contains("adx_partialurl") ? e.GetAttributeValue<string>("adx_partialurl") : null
				};

				CmsEntityMetadata childEntityMetadata;

				if (TryGetEntityMetadata(serviceContext, e.LogicalName, entityMetadataCache, out childEntityMetadata))
				{
					if (childEntityMetadata.HasAttribute("adx_displayorder"))
					{
						info.DisplayOrder = e.Attributes.Contains("adx_displayorder") ? e.GetAttributeValue<int?>("adx_displayorder") : null;
						info.DisplayOrderPropertyName = "adx_displayorder";
					}
				}

				return info;
			}).ToArray();

			var childJson = SerializeChildInfos(childInfos);

			WriteResponse(context.Response, new JObject
			{
				{ "d", new JRaw(childJson) }
			});
		}

		private string SerializeChildInfos(ExtendedSiteMapChildInfo[] childInfos)
		{
			using (var stream = new MemoryStream())
			{
				var serializer = new DataContractJsonSerializer(childInfos.GetType());

				serializer.WriteObject(stream, childInfos);

				stream.Position = 0;

				using (var reader = new StreamReader(stream, ContentEncoding))
				{
					return reader.ReadToEnd();
				}
			}
		}

		private static IEnumerable<Entity> GetChildren(OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata)
		{
			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.LogicalName == "adx_webpage")
			{
				var children = serviceContext.GetChildPages(entity)
					.Union(serviceContext.GetChildFiles(entity))
					.Union(serviceContext.GetChildShortcuts(entity));

				var extendedChildRelationships = new[]
				{
					new Relationship("adx_webpage_event"),
					new Relationship("adx_webpage_communityforum"),
					new Relationship("adx_webpage_blog"),
				};

				// Relationships are always off of the language root web page, so need to get that
				var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
				Entity rootWebPage = contextLanguageInfo.GetRootWebPageEntity(serviceContext, entity);
				foreach (var relationship in extendedChildRelationships)
				{
					CmsEntityRelationshipInfo relationshipInfo;

					if (!entityMetadata.TryGetRelationshipInfo(relationship, out relationshipInfo))
					{
						continue;
					}

					try
					{
						children = children.Union(rootWebPage.GetRelatedEntities(serviceContext, relationship));
					}
					catch
					{
						continue;
					}
				}

				return children.ToArray();
			}

			if (entity.LogicalName == "adx_blogpost")
			{
				return entity.GetRelatedEntities(serviceContext, new Relationship("adx_blogpost_webfile"));
			}

			if (entity.LogicalName == "adx_event")
			{
				return entity.GetRelatedEntities(serviceContext, new Relationship("adx_event_eventschedule"));
			}
			
			return new List<Entity>();
		}

		private static string GetChildTitle(Entity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.Attributes.Contains("adx_name"))
			{
				return entity.GetAttributeValue<string>("adx_name");
			}

			if (entity.Attributes.Contains("name"))
			{
				return entity.GetAttributeValue<string>("name");
			}

			return null;
		}

		private static bool TryGetEntityMetadata(OrganizationServiceContext serviceContext, string logicalName, IDictionary<string, CmsEntityMetadata> cache, out CmsEntityMetadata metadata)
		{
			if (cache.TryGetValue(logicalName, out metadata))
			{
				return true;
			}

			metadata = new CmsEntityMetadata(serviceContext, logicalName);

			cache[logicalName] = metadata;

			return true;
		}
	}
}
