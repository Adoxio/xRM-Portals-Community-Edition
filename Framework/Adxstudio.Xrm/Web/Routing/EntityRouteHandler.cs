/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.ContentAccess;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Diagnostics.Trace;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.SharePoint;
using Adxstudio.Xrm.SharePoint.Handlers;
using Microsoft.SharePoint.Client;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Handlers;
using Microsoft.Xrm.Portal.Web.Modules;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.Routing
{
	/// <summary>
	/// Handles requests to <see cref="Entity"/> objects.
	/// </summary>
	/// <seealso cref="PortalRoutingModule"/>
	/// <seealso cref="AnnotationHandler"/>
	public class EntityRouteHandler : Microsoft.Xrm.Portal.Web.Routing.EntityRouteHandler
	{
		/// <summary>
		/// Class Initialization
		/// </summary>
		/// <param name="portalName">Portal Name</param>
		public EntityRouteHandler(string portalName)
			: base(portalName)
		{
		}

		protected override bool TryCreateHandler(OrganizationServiceContext context, string logicalName, Guid id, out IHttpHandler handler)
		{
			if (string.Equals(logicalName, "annotation", StringComparison.InvariantCulture))
			{
				var annotation = context.CreateQuery(logicalName).FirstOrDefault(e => e.GetAttributeValue<Guid>("annotationid") == id);

				if (annotation != null)
				{
					var regarding = annotation.GetAttributeValue<EntityReference>("objectid");

					if (regarding != null && string.Equals(regarding.LogicalName, "knowledgearticle", StringComparison.InvariantCulture))
					{
						// Access to a note associated to a knowledge article requires the CMS Security to grant read of the annotation and the related knowledge article. 
						// Additionally, if CAL or Product filtering is enabled and the CAL and/or Product providers reject access to the knowledge article 
						// then access to the note is denied. If CAL and Product filtering are NOT enabled or CAL and/or Product Provider assertion passed, 
						// we must continue to check the Entity Permissions. If the Entity Permission Provider grants read to the knowledge article then the 
						// note can be accessed, otherwise access will be denied.

						// Assert CMS Security on the annotation and knowledge article.
						if (TryAssertByCrmEntitySecurityProvider(context, annotation.ToEntityReference()) && TryAssertByCrmEntitySecurityProvider(context, regarding))
						{
							// Assert CAL and/or Product Filtering if enabled.
							var contentAccessLevelProvider = new ContentAccessLevelProvider();
							var productAccessProvider = new ProductAccessProvider();

							if (contentAccessLevelProvider.IsEnabled() || productAccessProvider.IsEnabled())
							{
								if (!AssertKnowledgeArticleCalAndProductFiltering(annotation, context, contentAccessLevelProvider, productAccessProvider))
								{
									ADXTrace.Instance.TraceInfo(TraceCategory.Application, $"Access to {EntityNamePrivacy.GetEntityName(annotation.LogicalName)} was denied. Id:{id} RegardingId={regarding.Id} RegardingLogicalName={EntityNamePrivacy.GetEntityName(regarding.LogicalName)}");
									handler = null;
									return false;
								}
							}

							// Assert Entity Permissions on the knowledge article.
							if (TryAssertByCrmEntityPermissionProvider(context, regarding))
							{
								ADXTrace.Instance.TraceInfo(TraceCategory.Application, $"Access to {EntityNamePrivacy.GetEntityName(annotation.LogicalName)} was granted. Id:{id} RegardingId={regarding.Id} RegardingLogicalName={EntityNamePrivacy.GetEntityName(regarding.LogicalName)}");
								handler = CreateAnnotationHandler(annotation);
								return true;
							}
						}

						ADXTrace.Instance.TraceInfo(TraceCategory.Application, $"Access to {EntityNamePrivacy.GetEntityName(annotation.LogicalName)} was denied. Id:{id} RegardingId={regarding.Id} RegardingLogicalName={EntityNamePrivacy.GetEntityName(regarding.LogicalName)}");
						handler = null;
						return false;
					}

					// Assert CMS security on the regarding entity or assert entity permission on the annotation and the regarding entity.
					if (TryAssertByCrmEntitySecurityProvider(context, regarding) || TryAssertByCrmEntityPermissionProvider(context, annotation, regarding))
					{
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, $"Access to {EntityNamePrivacy.GetEntityName(annotation.LogicalName)} was granted. Id={id} RegardingId={regarding?.Id} RegardingLogicalName={EntityNamePrivacy.GetEntityName(regarding?.LogicalName)}");
						handler = CreateAnnotationHandler(annotation);
						return true;
					}
				}
			}

			if (string.Equals(logicalName, "salesliteratureitem", StringComparison.InvariantCulture))
			{
				var salesliteratureitem = context.CreateQuery(logicalName).FirstOrDefault(e => e.GetAttributeValue<Guid>("salesliteratureitemid") == id);

				if (salesliteratureitem != null)
				{
					//Currently salesliteratureitem.iscustomerviewable is not exposed to CRM UI, therefore get the parent and check visibility.
					//var isCustomerViewable = salesliteratureitem.GetAttributeValue<bool?>("iscustomerviewable").GetValueOrDefault();
					var salesliterature =
						context.CreateQuery("salesliterature")
						       .FirstOrDefault(
							       e =>
							       e.GetAttributeValue<Guid>("salesliteratureid") ==
							       salesliteratureitem.GetAttributeValue<EntityReference>("salesliteratureid").Id);

					if (salesliterature != null)
					{
						var isCustomerViewable = salesliterature.GetAttributeValue<bool?>("iscustomerviewable").GetValueOrDefault();

						if (isCustomerViewable)
						{
							handler = CreateSalesAttachmentHandler(salesliteratureitem);
							return true;
						}
					}
				}
			}

			if (string.Equals(logicalName, "sharepointdocumentlocation", StringComparison.InvariantCulture))
			{
				var location = context.CreateQuery(logicalName).FirstOrDefault(e => e.GetAttributeValue<Guid>("sharepointdocumentlocationid") == id);

				if (location != null)
				{
					var httpContext = HttpContext.Current;
					var regardingId = location.GetAttributeValue<EntityReference>("regardingobjectid");

					// assert CMS access to the regarding entity or assert entity permission on the entity
					if (TryAssertByCrmEntitySecurityProvider(context, regardingId) || TryAssertByCrmEntityPermissionProvider(context, location, location.GetAttributeValue<EntityReference>("regardingobjectid")))
					{
						var locationUrl = context.GetDocumentLocationUrl(location);
						var fileName = httpContext.Request["file"];
						
						// Ensure safe file URL - it cannot begin or end with dot, contain consecutive dots, or any of ~ " # % & * : < > ? \ { | }
						fileName = Regex.Replace(fileName, @"(\.{2,})|([\~\""\#\%\&\*\:\<\>\?\/\\\{\|\}])|(^\.)|(\.$)", string.Empty); // also removes solidus

						var folderPath = httpContext.Request["folderPath"];
						
						Uri sharePointFileUrl;

						if (!string.IsNullOrWhiteSpace(folderPath))
						{
							// Ensure safe folder URL - it cannot begin or end with dot, contain consecutive dots, or any of ~ " # % & * : < > ? \ { | }
							folderPath = Regex.Replace(folderPath, @"(\.{2,})|([\~\""\#\%\&\*\:\<\>\?\\\{\|\}])|(^\.)|(\.$)", string.Empty).Trim('/');

							sharePointFileUrl = new Uri("{0}/{1}/{2}".FormatWith(locationUrl.OriginalString, folderPath, fileName));
						}
						else
						{
							sharePointFileUrl = new Uri("{0}/{1}".FormatWith(locationUrl.OriginalString, fileName));
						}

						

						handler = CreateSharePointFileHandler(sharePointFileUrl, fileName);
						return true;
					}

					if (!httpContext.Request.IsAuthenticated)
					{
						httpContext.Response.ForbiddenAndEndResponse();
					}
					else
					{
						// Sending Forbidden gets caught by the Application_EndRequest and throws an error trying to redirect to the Access Denied page.
						// Send a 404 instead with plain text indicating Access Denied.
						httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
						httpContext.Response.ContentType = "text/plain";
						httpContext.Response.Write("Access Denied");
						httpContext.Response.End();
					}
				}
			}

			if (string.Equals(logicalName, "activitymimeattachment", StringComparison.InvariantCulture))
			{
				var attachment = context.CreateQuery(logicalName).FirstOrDefault(e => e.GetAttributeValue<Guid>("attachmentid") == id);

				if (attachment != null)
				{
					// retrieve the parent object for the annoation

					var objectId = attachment.GetAttributeValue<EntityReference>("objectid");

					// assert CMS access to the regarding entity or assert entity permission on the entity

					if (TryAssertByCrmEntitySecurityProvider(context, objectId) || TryAssertByCrmEntityPermissionProvider(context, attachment, attachment.GetAttributeValue<EntityReference>("objectid")))
					{
						handler = CreateActivityMimeAttachmentHandler(attachment);
						return true;
					}
				}
			}

			handler = null;
			return false;
		}

		protected virtual bool TryAssertByCrmEntitySecurityProvider(OrganizationServiceContext context, EntityReference regardingId)
		{
			if (regardingId == null) return false;

			// determine the primary ID attribute

			var request = new RetrieveEntityRequest { LogicalName = regardingId.LogicalName, EntityFilters = EntityFilters.Entity };
			var response = context.Execute(request) as RetrieveEntityResponse;
			var primaryIdAttribute = response.EntityMetadata.PrimaryIdAttribute;

			var regarding = context.CreateQuery(regardingId.LogicalName).FirstOrDefault(e => e.GetAttributeValue<Guid>(primaryIdAttribute) == regardingId.Id);

			if (regarding == null) return false;

			// assert read access on the regarding entity

			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);

			return securityProvider.TryAssert(context, regarding, CrmEntityRight.Read);
		}

		protected virtual bool TryAssertByCrmEntityPermissionProvider(OrganizationServiceContext context, EntityReference entityReference)
		{
			if (!AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled) return false;

			var crmEntityPermissionProvider = new CrmEntityPermissionProvider(PortalName);
			return crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Read, entityReference);
		}

		protected virtual bool TryAssertByCrmEntityPermissionProvider(OrganizationServiceContext context, Entity entity)
		{
			if (!AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled) return false;

			var crmEntityPermissionProvider = new CrmEntityPermissionProvider(PortalName);
			return crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Read, entity);
		}

		protected virtual bool TryAssertByCrmEntityPermissionProvider(OrganizationServiceContext context, Entity entity, EntityReference regarding)
		{
			if (!AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled) return false;

			var crmEntityPermissionProvider = new CrmEntityPermissionProvider(PortalName);

			if (string.Equals(entity.LogicalName, "annotation", StringComparison.InvariantCulture)
				&& regarding != null
				&& string.Equals(regarding.LogicalName, "adx_portalcomment", StringComparison.InvariantCulture))
			{
				// If can read portal comment, bypass assertion check on notes and assume read permission.
				return TryAssertPortalCommentPermission(context, crmEntityPermissionProvider, CrmEntityPermissionRight.Read, regarding);
			}

			return crmEntityPermissionProvider.TryAssert(context, CrmEntityPermissionRight.Read, entity, regarding: regarding);
		}

		protected override IHttpHandler CreateAnnotationHandler(Entity entity)
		{
			return new Handlers.AnnotationHandler(entity);
		}

		protected virtual IHttpHandler CreateSalesAttachmentHandler(Entity entity)
		{
			return new Handlers.SalesAttachmentHandler(entity);
		}

		protected virtual IHttpHandler CreateSharePointFileHandler(Uri sharePointFileUrl, string fileName)
		{
			return new SharePointFileHandler(sharePointFileUrl, fileName);
		}

		protected IHttpHandler CreateActivityMimeAttachmentHandler(Entity entity)
		{
			return new Handlers.ActivityMimeAttachmentHandler(entity);
		}

		private static bool TryAssertPortalCommentPermission(OrganizationServiceContext context, CrmEntityPermissionProvider entityPermissionProvider, CrmEntityPermissionRight right, EntityReference target)
		{
			var response = context.Execute<RetrieveResponse>(new RetrieveRequest
			{
				Target = target,
				ColumnSet = new ColumnSet("regardingobjectid"),
			});

			var regarding = response.Entity.GetAttributeValue<EntityReference>("regardingobjectid");

			return regarding != null
				&& entityPermissionProvider.TryAssert(context, right, target, regarding: regarding);
		}

		/// <summary>
		/// Adds Content Access Level and Product Filtering to fetch
		/// </summary>
		/// <param name="annotation">Annotation</param>
		/// <param name="context">Context</param>
		/// <param name="contentAccessLevelProvider">content Access Level Provider</param>
		/// <param name="productAccessProvider">product Access Provider</param>
		private bool AssertKnowledgeArticleCalAndProductFiltering(Entity annotation, OrganizationServiceContext context, ContentAccessLevelProvider contentAccessLevelProvider, ProductAccessProvider productAccessProvider)
		{
			if (!contentAccessLevelProvider.IsEnabled() & !productAccessProvider.IsEnabled())
			{
				// If CAL and Product Filtering is not enabled then we must not restrict access to the article. This will also eliminate an unnecessary knowledge article query.

				return true;
			}

			var entityReference = annotation.GetAttributeValue<EntityReference>("objectid");

			var fetch = new Fetch();
			var knowledgeArticleFetch = new FetchEntity("knowledgearticle")
			{
				Filters = new List<Filter>
						{
							new Filter
							{
								Type = LogicalOperator.And,
								Conditions = new List<Condition>
								{
									new Condition("knowledgearticleid", ConditionOperator.Equal, entityReference.Id)
								}
							}
						},
				Links = new List<Link>()
			};

			fetch.Entity = knowledgeArticleFetch;

			// Apply Content Access Level filtering. If it is not enabled the fetch will not be modified
			contentAccessLevelProvider.TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight.Read, fetch);

			// Apply Product filtering. If it is not enabled the fetch will not be modified.
			productAccessProvider.TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight.Read, fetch);

			var kaResponse = (RetrieveMultipleResponse)context.Execute(fetch.ToRetrieveMultipleRequest());

			var isValid = kaResponse.EntityCollection.Entities.Any();

			if (isValid)
			{
				if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
				{
					PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Note, HttpContext.Current, "TryCreateHandler CAL/PF passed", 1, annotation.ToEntityReference(), "read");
					
				}
				return true;
			}
			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Note, HttpContext.Current, "TryCreateHandler CAL/PF failed", 1, annotation.ToEntityReference(), "read");
			}
			return false;
		}
	}
}
