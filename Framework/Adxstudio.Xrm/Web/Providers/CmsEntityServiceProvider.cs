/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Cms.Security;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Tagging;
using Adxstudio.Xrm.Web.Handlers;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using Adxstudio.Xrm.Core.Flighting;

namespace Adxstudio.Xrm.Web.Providers
{
	public class CmsEntityServiceProvider : ICmsEntityServiceProvider
	{
		public CmsEntityServiceProvider(string portalName = null)
		{
			PortalName = portalName;
		}

		protected virtual string PortalName { get; private set; }

		public Entity ExecuteEntityQuery(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, EntityReference entityReference, CmsEntityMetadata entityMetadata)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (portal == null)
			{
				throw new ArgumentNullException("portal");
			}

			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (entityReference == null)
			{
				throw new ArgumentNullException("entityReference");
			}

			if (entityMetadata == null)
			{
				throw new ArgumentNullException("entityMetadata");
			}

			var website = portal.Website.ToEntityReference();

			WebsiteJoin websiteJoin;

			if (WebsiteJoins.TryGetValue(entityReference.LogicalName, out websiteJoin))
			{
				return (
					from referencing in serviceContext.CreateQuery(websiteJoin.ReferencingEntity)
					join referenced in serviceContext.CreateQuery(websiteJoin.ReferencedEntity) on referencing.GetAttributeValue<EntityReference>(websiteJoin.ReferencingAttribute) equals referenced.GetAttributeValue<EntityReference>(websiteJoin.ReferencedAttribute)
					where referenced.GetAttributeValue<EntityReference>(websiteJoin.WebsiteReferenceAttribute) == website
					where referencing.GetAttributeValue<Guid>(entityMetadata.PrimaryIdAttribute) == entityReference.Id
					select referencing).FirstOrDefault();
			}

			var query = serviceContext.CreateQuery(entityReference.LogicalName);

			// If the target entity is scoped to a website, filter the query by the current website.
			if (entityMetadata.HasAttribute("adx_websiteid"))
			{
				query = query.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == portal.Website.ToEntityReference());
			}

			return query.FirstOrDefault(e => e.GetAttributeValue<Guid>(entityMetadata.PrimaryIdAttribute) == entityReference.Id);
		}

		public IEnumerable<Entity> ExecuteEntitySetQuery(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, string entityLogicalName, CmsEntityMetadata entityMetadata, string filter = null)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (portal == null)
			{
				throw new ArgumentNullException("portal");
			}

			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (string.IsNullOrWhiteSpace(entityLogicalName))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "entityLogicalName");
			}

			if (entityMetadata == null)
			{
				throw new ArgumentNullException("entityMetadata");
			}

			var security = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);

			// Only entity sets that can be scoped/filtered to the current website can be queried through the data service.
			var website = portal.Website.ToEntityReference();

			if (entityLogicalName == "adx_webpage")
			{
				var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider(PortalName);

				if (contentMapProvider != null)
				{
					var langContext = HttpContext.Current.GetContextLanguageInfo();

					return contentMapProvider.Using(map =>
					{
						WebsiteNode websiteNode;

						var webPages = map.TryGetValue(website, out websiteNode)
							? websiteNode.WebPages
							: Enumerable.Empty<WebPageNode>();

						if (!langContext.IsCrmMultiLanguageEnabled)
						{
							return webPages;
						}

						return webPages.Where(page => page.IsRoot.HasValue && page.IsRoot.Value);
					})
					.Select(e => e.ToEntity())
					.Select(e => serviceContext.IsAttached(e) ? e : serviceContext.MergeClone(e))
					.Where(e =>
					{
						try
						{
							return security.TryAssert(serviceContext, e, CrmEntityRight.Read);
						}
						catch (InvalidOperationException exception)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Exception during Read permission assertion on {0}:{1}: {2}", e.LogicalName, e.Id, exception.ToString()));

							return false;
						}
					});
				}
			}

			if (entityLogicalName == "adx_webfile")
			{
				var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider(PortalName);

				if (contentMapProvider != null)
				{
					return contentMapProvider.Using(map =>
					{
						WebsiteNode websiteNode;

						return map.TryGetValue(website, out websiteNode)
							? websiteNode.WebFiles
							: Enumerable.Empty<WebFileNode>();
					})
					.Select(e => e.ToEntity())
					.Select(e => serviceContext.IsAttached(e) ? e : serviceContext.MergeClone(e))
					.Where(e =>
					{
						try
						{
							return security.TryAssert(serviceContext, e, CrmEntityRight.Read);
						}
						catch (InvalidOperationException exception)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Exception during Read permission assertion on {0}:{1}: {2}", e.LogicalName, e.Id, exception.ToString()));

							return false;
						}
					});
				}
			}

			if (entityLogicalName == "adx_publishingstate")
			{
				var publishingStates = serviceContext.CreateQuery("adx_publishingstate")
					.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website)
					.ToArray()
					.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read));

				Guid fromStateId;

				if (Guid.TryParse(context.Request.QueryString["FromStateID"], out fromStateId))
				{
					var stateTransitionSecurityProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<IPublishingStateTransitionSecurityProvider>();

					publishingStates = publishingStates
						.Where(e => stateTransitionSecurityProvider.TryAssert(serviceContext, portal.Website, fromStateId, e.Id))
						.ToArray();
				}

				return publishingStates;
			}

			if (entityLogicalName == "adx_pagetemplate")
			{
				return FetchPageTemplateReferences(serviceContext, portal.Website.Id, filter);
			}

			if (entityLogicalName == "subject")
			{
				return FetchEntityReferences(serviceContext, entityLogicalName, "subjectid", "title");
			}

			if (entityLogicalName == "adx_entityform")
			{
				return FetchEntityReferences(serviceContext, entityLogicalName, "adx_entityformid", "adx_name", "statecode");
			}

			if (entityLogicalName == "adx_entitylist")
			{
				return FetchEntityReferences(serviceContext, entityLogicalName, "adx_entitylistid", "adx_name", "statecode");
			}

			if (entityLogicalName == "adx_webform")
			{
				return FetchEntityReferences(serviceContext, entityLogicalName, "adx_webformid", "adx_name", "statecode");
			}

			WebsiteJoin websiteJoin;

			if (WebsiteJoins.TryGetValue(entityLogicalName, out websiteJoin))
			{
				return (
					from referencing in serviceContext.CreateQuery(websiteJoin.ReferencingEntity)
					join referenced in serviceContext.CreateQuery(websiteJoin.ReferencedEntity) on referencing.GetAttributeValue<EntityReference>(websiteJoin.ReferencingAttribute) equals referenced.GetAttributeValue<EntityReference>(websiteJoin.ReferencedAttribute)
					where referenced.GetAttributeValue<EntityReference>(websiteJoin.WebsiteReferenceAttribute) == website
					select referencing).ToArray().Where(e =>
					{
						try
						{
							return security.TryAssert(serviceContext, e, CrmEntityRight.Read);
						}
						catch (InvalidOperationException exception)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Exception during Read permission assertion on {0}:{1}: {2}", e.LogicalName, e.Id, exception.ToString()));

							return false;
						}
					});
			}

			if (!entityMetadata.HasAttribute("adx_websiteid"))
			{
				throw new CmsEntityServiceException(HttpStatusCode.Forbidden, "Entity type {0} is not accessible through this service.".FormatWith(entityLogicalName));
			}
            
            var query = serviceContext.CreateQuery(entityLogicalName)
                .Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website)
                .ToArray()
                .Where(e =>
                {
                    try
                    {
                        return security.TryAssert(serviceContext, e, CrmEntityRight.Read);
                    }
                    catch (InvalidOperationException exception)
                    {
                        ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Exception during Read permission assertion on {0}:{1}: {2}", e.LogicalName, e.Id, exception.ToString()));

                        return false;
                    }
                });

            if (entityLogicalName == "adx_websitelanguage")
            {
                query = query.OrderBy(e => e.GetAttributeValue<string>(entityMetadata.PrimaryNameAttribute), StringComparer.CurrentCultureIgnoreCase);
            }

            return query;
		}

		public void InterceptChange(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation, Entity preImage = null)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (portal == null)
			{
				throw new ArgumentNullException("portal");
			}

			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entityMetadata == null)
			{
				throw new ArgumentNullException("entityMetadata");
			}

			if (entityMetadata.HasAttribute("adx_websiteid") && entity.GetAttributeValue<EntityReference>("adx_websiteid") == null)
			{
				entity.SetAttributeValue("adx_websiteid", portal.Website.ToEntityReference());
			}

			switch (entity.LogicalName)
			{
				case "adx_blog":
					InterceptChangeOfBlog(context, portal, serviceContext, entity, entityMetadata, operation, preImage);
					break;

				case "adx_blogpost":
					InterceptChangeOfBlogPost(context, portal, serviceContext, entity, entityMetadata, operation);
					break;

				case "adx_communityforum":
					InterceptChangeOfCommunityForum(context, portal, serviceContext, entity, entityMetadata, operation, preImage);
					break;

				case "adx_communityforumpost":
					InterceptChangeOfCommunityForumPost(context, portal, serviceContext, entity, entityMetadata, operation);
					break;

				case "adx_communityforumthread":
					InterceptChangeOfCommunityForumThread(context, portal, serviceContext, entity, entityMetadata, operation);
					break;

				case "adx_contentsnippet":
					InterceptChangeOfContentSnippet(context, portal, serviceContext, entity, entityMetadata, operation);
					break;

				case "adx_event":
					InterceptChangeOfEvent(context, portal, serviceContext, entity, entityMetadata, operation, preImage);
					break;

				case "adx_eventschedule":
					InterceptChangeOfEventSchedule(context, portal, serviceContext, entity, entityMetadata, operation);
					break;

				case "adx_shortcut":
					InterceptChangeOfShortcut(context, portal, serviceContext, entity, entityMetadata, operation, preImage);
					break;

				case "adx_webfile":
					InterceptChangeOfWebFile(context, portal, serviceContext, entity, entityMetadata, operation, preImage);
					break;

				case "adx_weblink":
					InterceptChangeOfWebLink(context, portal, serviceContext, entity, entityMetadata, operation);
					break;

				case "adx_weblinkset":
					InterceptChangeOfWebLinkSet(context, portal, serviceContext, entity, entityMetadata, operation);
					break;

				case "adx_webpage":
					InterceptChangeOfWebPage(context, portal, serviceContext, entity, entityMetadata, operation, preImage);
					break;
			}
		}

		public void ExtendEntityJson(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, JObject extensions)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (portal == null)
			{
				throw new ArgumentNullException("portal");
			}

			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entityMetadata == null)
			{
				throw new ArgumentNullException("entityMetadata");
			}

			if (extensions == null)
			{
				throw new ArgumentNullException("extensions");
			}

			Tuple<string, string, string> taggableEntity;

			if (TaggableEntities.TryGetValue(entity.LogicalName, out taggableEntity))
			{
				var tags = GetTags(serviceContext, entity, portal.Website.ToEntityReference(), taggableEntity)
					.Select(e => e.GetAttributeValue<string>("adx_name"));

				extensions["tags"] = new JArray(tags.ToArray().Distinct(TagInfo.TagComparer).OrderBy(name => name));
			}
		}

		public void InterceptExtensionChange(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, JObject extensions, CmsEntityOperation operation)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (portal == null)
			{
				throw new ArgumentNullException("portal");
			}

			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entityMetadata == null)
			{
				throw new ArgumentNullException("entityMetadata");
			}

			if (extensions == null)
			{
				throw new ArgumentNullException("extensions");
			}

			Tuple<string, string, string> taggableEntity;

			if (TaggableEntities.TryGetValue(entity.LogicalName, out taggableEntity))
			{
				InterceptTagExtensionChange(context, portal, serviceContext, entity, entityMetadata, extensions, operation, taggableEntity);
			}
		}

		protected virtual void InterceptTagExtensionChange(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, JObject extensions, CmsEntityOperation operation, Tuple<string, string, string> taggableEntity)
		{
			var tagExtension = extensions.Property("tags");

			if (tagExtension == null || tagExtension.Value == null)
			{
				return;
			}

			var tagRelationship = new Relationship(taggableEntity.Item3);

			var website = portal.Website.ToEntityReference();
			var appliedTags = tagExtension.Value.Values<string>().Distinct(TagInfo.TagComparer).ToArray();

			var existingTags = operation == CmsEntityOperation.Create
				? new Entity[] { }
				: GetTags(serviceContext, entity, website, taggableEntity);

			var tagsToRemove = existingTags.Where(e => !appliedTags.Contains(e.GetAttributeValue<string>("adx_name"), TagInfo.TagComparer));

			foreach (var tagToRemove in tagsToRemove)
			{
				serviceContext.DeleteLink(entity, tagRelationship, tagToRemove);
			}

			var existingTagNames = existingTags.Select(e => e.GetAttributeValue<string>("adx_name")).ToArray();
			var tagsToAdd = appliedTags.Where(t => !existingTagNames.Contains(t, TagInfo.TagComparer));

			foreach (var tagToAdd in tagsToAdd)
			{
				var tagName = tagToAdd;

				var tag = (from t in serviceContext.CreateQuery("adx_tag")
						   where t.GetAttributeValue<EntityReference>("adx_websiteid") == website
						   where t.GetAttributeValue<string>("adx_name") == tagName
						   select t).FirstOrDefault();

				if (tag == null)
				{
					var newTag = new Entity("adx_tag");

					newTag.SetAttributeValue("adx_name", tagName);
					newTag.SetAttributeValue("adx_websiteid", website);

					var response = (CreateResponse)serviceContext.Execute(new CreateRequest
					{
						Target = newTag
					});

					newTag.Id = response.id;
					newTag.SetAttributeValue("adx_tagid", response.id);

					serviceContext.Attach(newTag);
					serviceContext.AddLink(entity, tagRelationship, newTag);
				}
				else
				{
					serviceContext.AddLink(entity, tagRelationship, tag);
				}
			}
		}

		private static Entity[] GetTags(OrganizationServiceContext serviceContext, Entity entity, EntityReference website, Tuple<string, string, string> taggableEntity)
		{
			return (from t in serviceContext.CreateQuery("adx_tag")
					join tp in serviceContext.CreateQuery(taggableEntity.Item3) on t.GetAttributeValue<Guid>("adx_tagid") equals tp.GetAttributeValue<Guid>("adx_tagid")
					join e in serviceContext.CreateQuery(taggableEntity.Item1) on tp.GetAttributeValue<Guid>(taggableEntity.Item2) equals e.GetAttributeValue<Guid>(taggableEntity.Item2)
					where e.GetAttributeValue<Guid>(taggableEntity.Item2) == entity.Id
					where t.GetAttributeValue<EntityReference>("adx_websiteid") == website
					select t).ToArray();
		}

		protected virtual void InterceptChangeOfBlog(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation, Entity preImage)
		{
			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Blogs can't have an empty name property.");
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_partialurl")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Blogs can't have an empty partial URL property.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_bloghomepagetemplateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, string.Format("Blogs must have a {0} page template ID.", "home"));
			}

			if (entity.GetAttributeValue<EntityReference>("adx_blogpostpagetemplateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, string.Format("Blogs must have a {0} page template ID.", "post"));
			}

			if (entity.GetAttributeValue<EntityReference>("adx_blogarchivepagetemplateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, string.Format("Blogs must have a {0} page template ID.", "archive"));
			}

			AssertParentUpdateIsValid(portal, serviceContext, entity, operation, preImage, "adx_parentpageid", "adx_displayorder");
		}

		protected virtual void InterceptChangeOfBlogPost(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation)
		{
			if (operation == CmsEntityOperation.Create)
			{
				SetCreateTrackingAttributes(context, entity);

				if (entity.GetAttributeValue<EntityReference>("adx_authorid") == null)
				{
					if (portal.User == null || portal.User.LogicalName != "contact")
					{
						throw new CmsEntityServiceException(HttpStatusCode.InternalServerError, "The blog post couldn't be associated with the author (portal user contact record).");
					}

					entity.SetAttributeValue("adx_authorid", portal.User.ToEntityReference());
				}

				if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_partialurl")))
				{
					var date = entity.GetAttributeValue<DateTime?>("adx_date");

					if (date.HasValue)
					{
						entity.SetAttributeValue("adx_partialurl", GetDefaultBlogPostPartialUrl(date.Value, entity.GetAttributeValue<string>("adx_name")));
					}
				}

                if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
                {
                    PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Blog, HttpContext.Current, "create_blogpost", 1, entity.ToEntityReference(), "create");
                }
            }

            if (operation == CmsEntityOperation.Update)
			{
				SetUpdateTrackingAttributes(context, entity);

                if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
                {
                    PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Blog, HttpContext.Current, "edit_blogpost", 1, entity.ToEntityReference(), "edit");
                }
            }

            if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Blog posts can't have an empty name property.");
			}

			if (entity.GetAttributeValue<DateTime?>("adx_date") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Blog posts must be assigned a date.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_authorid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Blog posts must have an associated author contact record.");
			}
		}

		private static string GetDefaultBlogPostPartialUrl(DateTime date, string title)
		{
			if (string.IsNullOrWhiteSpace(title))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "title");
			}

			string titleSlug;

			try
			{
				// Encoding the title as Cyrillic, and then back to ASCII, converts accented characters to their
				// unaccented version. We'll try/catch this, since it depends on the underlying platform whether
				// the Cyrillic code page is available.
				titleSlug = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(title)).ToLowerInvariant();
			}
			catch
			{
				titleSlug = title.ToLowerInvariant();
			}

			// Strip all disallowed characters.
			titleSlug = Regex.Replace(titleSlug, @"[^a-z0-9\s-]", string.Empty);

			// Convert all runs of multiple spaces to a single space.
			titleSlug = Regex.Replace(titleSlug, @"\s+", " ").Trim();

			// Cap the length of the title slug.
			titleSlug = titleSlug.Substring(0, titleSlug.Length <= 50 ? titleSlug.Length : 50).Trim();

			// Replace all spaces with hyphens.
			titleSlug = Regex.Replace(titleSlug, @"\s", "-").Trim('-');

			return "{0:yyyy}-{0:MM}-{0:dd}-{1}".FormatWith(date, titleSlug);
		}

		protected virtual void InterceptChangeOfCommunityForum(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation, Entity preImage)
		{
			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_partialurl")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Forums can't have an empty partial URL property.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_forumpagetemplateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Forums must have a forum page template ID.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_threadpagetemplateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Forums must have a thread page template ID.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Forums must have a publishing state ID.");
			}

			AssertStateTransitionIsValid(portal, serviceContext, entity, operation);

			AssertParentUpdateIsValid(portal, serviceContext, entity, operation, preImage, "adx_parentpageid", "adx_displayorder");
		}

		protected virtual void InterceptChangeOfCommunityForumPost(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation)
		{
			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Posts can't have an empty name property.");
			}
		}

		protected virtual void InterceptChangeOfCommunityForumThread(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation)
		{
			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Threads cannot have an empty name property.");
			}
		}

		protected virtual void InterceptChangeOfContentSnippet(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation)
		{
			SetUpdateTrackingAttributes(context, entity);
		}

		protected virtual void InterceptChangeOfEvent(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation, Entity preImage)
		{
			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Events cannot have an empty name property.");
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_partialurl")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Events cannot have an empty partial URL property.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_pagetemplateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Events must have a page template ID.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Events must have a publishing state ID.");
			}

			AssertStateTransitionIsValid(portal, serviceContext, entity, operation);

			AssertParentUpdateIsValid(portal, serviceContext, entity, operation, preImage, "adx_parentpageid");
		}

		protected virtual void InterceptChangeOfEventSchedule(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation)
		{
			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Event schedules can't have an empty name property.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Event schedules must have a publishing state ID.");
			}

			if (entity.GetAttributeValue<DateTime?>("adx_starttime") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Event schedules must have a start time value.");
			}

			if (entity.GetAttributeValue<DateTime?>("adx_endtime") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Event schedules must have an end time value.");
			}

			AssertStateTransitionIsValid(portal, serviceContext, entity, operation);
		}

		protected virtual void InterceptChangeOfShortcut(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation, Entity preImage)
		{
			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Shortcuts can't have an empty name property.");
			}

			AssertParentUpdateIsValid(portal, serviceContext, entity, operation, preImage, "adx_parentpage_webpageid", "adx_displayorder");
		}

		protected virtual void InterceptChangeOfWebFile(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation, Entity preImage)
		{
			if (operation == CmsEntityOperation.Create)
			{
				SetCreateTrackingAttributes(context, entity);
			}

			if (operation == CmsEntityOperation.Update)
			{
				SetUpdateTrackingAttributes(context, entity);
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_partialurl")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Web files can't have an empty partial URL property.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Web files must have a publishing state ID.");
			}

			AssertStateTransitionIsValid(portal, serviceContext, entity, operation);

			AssertParentUpdateIsValid(portal, serviceContext, entity, operation, preImage, "adx_parentpageid", "adx_displayorder");
		}

		protected virtual void InterceptChangeOfWebLink(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation)
		{
			if (operation == CmsEntityOperation.Create)
			{
				SetCreateTrackingAttributes(context, entity);
			}

			if (operation == CmsEntityOperation.Update)
			{
				SetUpdateTrackingAttributes(context, entity);
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Web links can't have an empty name property.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Web links must have a publishing state ID.");
			}

			AssertStateTransitionIsValid(portal, serviceContext, entity, operation);
		}

		protected virtual void InterceptChangeOfWebLinkSet(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation)
		{
		}

		protected virtual void InterceptChangeOfWebPage(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation, Entity preImage = null)
		{
			if (operation == CmsEntityOperation.Create)
			{
				SetCreateTrackingAttributes(context, entity);

				if (entity.GetAttributeValue<EntityReference>("adx_authorid") == null && portal.User != null && portal.User.LogicalName == "contact")
				{
					entity.SetAttributeValue("adx_authorid", portal.User.ToEntityReference());
				}
			}

			if (operation == CmsEntityOperation.Update)
			{
				SetUpdateTrackingAttributes(context, entity);
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Webpages can't have an empty name property.");
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_partialurl")))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Web pages cannot have an empty partial URL property.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_pagetemplateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Webpages must have a page template ID.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Web pages must have a publishing state ID.");
			}

			AssertStateTransitionIsValid(portal, serviceContext, entity, operation);

			AssertParentUpdateIsValid(portal, serviceContext, entity, operation, preImage, "adx_parentpageid", "adx_displayorder");
		}

		protected void AssertParentUpdateIsValid(IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityOperation operation, Entity preImage, string parentAttributeName, string displayOrderAttributeName = null)
		{
			if (preImage == null)
			{
				return;
			}

			var originalParent = preImage.GetAttributeValue<EntityReference>(parentAttributeName);
			var updateParent = entity.GetAttributeValue<EntityReference>(parentAttributeName);

			if (originalParent == null || updateParent == null || originalParent.Equals(updateParent))
			{
				return;
			}

			// If on create there's already a parent set though the POST (but before CmsEntityRelationshipHadler has
			// set its parent attribute), validate that value and then use it.
			if (operation == CmsEntityOperation.Create)
			{
				AssertParentUpdateIsValid(portal, serviceContext, entity.ToEntityReference(), originalParent);

				entity[parentAttributeName] = originalParent;
			}
			else
			{
				AssertParentUpdateIsValid(portal, serviceContext, entity.ToEntityReference(), updateParent);

				if (displayOrderAttributeName != null)
				{
					// If the entity is being reparented, blank out display order, as it's no longer valid -- display order is relative to siblings.
					entity[displayOrderAttributeName] = null;
				}
			}
		}

		private void AssertParentUpdateIsValid(IPortalContext portal, OrganizationServiceContext serviceContext, EntityReference child, EntityReference newParent)
		{
			if (child.Equals(newParent))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Web page parent cannot be set to self.");
			}

			var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider(PortalName);

			if (contentMapProvider == null)
			{
				var security = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);
				var website = portal.Website.ToEntityReference();

				var newParentEntity = serviceContext
					.CreateQuery("adx_webpage")
					.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_webpageid") == newParent.Id
										 && e.GetAttributeValue<EntityReference>("adx_websiteid") == website);

				if (newParentEntity == null)
				{
					throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to retrieve the updated web page parent.");
				}

				if (!security.TryAssert(serviceContext, newParentEntity, CrmEntityRight.Change))
				{
					throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "You don't have permission to update the website parent.");
				}
			}
			else
			{
				contentMapProvider.Using(map => AssertParentUpdateIsValid(portal, serviceContext, child, newParent, map));
			}
		}

		private void AssertParentUpdateIsValid(IPortalContext portal, OrganizationServiceContext serviceContext, EntityReference child, EntityReference newParent, ContentMap contentMap)
		{
			var security = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);
			var website = portal.Website.ToEntityReference();

			WebsiteNode websiteNode;

			if (!contentMap.TryGetValue(website, out websiteNode))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to retrieve the updated web page parent.");
			}

			var newParentNode = websiteNode.WebPages.FirstOrDefault(e => e.Id == newParent.Id);

			if (newParentNode == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to retrieve the updated web page parent.");
			}

			// Validate that we're not re-parenting the page to one of its own descendants, creating a cyclical parent relationship.

			var newAncestorNode = newParentNode;

			while (newAncestorNode != null)
			{
				if (newAncestorNode.ToEntityReference().Equals(child))
				{
					throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to retrieve the updated web page parent.");
				}

				newAncestorNode = newAncestorNode.Parent;
			}

			var newParentEntity = serviceContext.MergeClone(newParentNode.ToEntity());

			if (!security.TryAssert(serviceContext, newParentEntity, CrmEntityRight.Change))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "You don't have permission to update the website parent.");
			}
		}

		protected static void SetCreateTrackingAttributes(HttpContext context, Entity entity)
		{
			if (context.User == null || context.User.Identity == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to determine identity from current context.");
			}

			entity.SetAttributeValue("adx_createdbyusername", context.User.Identity.Name);
			// entity.SetAttributeValue("adx_createdbyipaddress", context.Request.UserHostAddress);
		}

		protected static void SetUpdateTrackingAttributes(HttpContext context, Entity entity)
		{
			if (context.User == null || context.User.Identity == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to determine identity from current context.");
			}

			entity.SetAttributeValue("adx_modifiedbyusername", context.User.Identity.Name);
			// entity.SetAttributeValue("adx_modifiedbyipaddress", context.Request.UserHostAddress);
		}

		protected void AssertStateTransitionIsValid(IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityOperation operation)
		{
			// This is not an update of an existing entity (may be Add, or Delete) -- nothing to check.
			if (operation != CmsEntityOperation.Update)
			{
				return;
			}

			var response = (RetrieveResponse)serviceContext.Execute(new RetrieveRequest
			{
				Target = new EntityReference(entity.LogicalName, entity.Id),
				ColumnSet = new ColumnSet("adx_publishingstateid")
			});

			var preUpdateEntity = response.Entity;

			var preUpdateState = preUpdateEntity.GetAttributeValue<EntityReference>("adx_publishingstateid");
			var postUpdateState = entity.GetAttributeValue<EntityReference>("adx_publishingstateid");

			// Publishing state has not changed -- nothing to check.
			if (postUpdateState != null && preUpdateState != null && postUpdateState.Equals(preUpdateState))
			{
				return;
			}

			var transitionSecurityProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<IPublishingStateTransitionSecurityProvider>();

			transitionSecurityProvider.Assert(
				serviceContext,
				portal.Website,
				preUpdateState == null ? Guid.Empty : preUpdateState.Id,
				postUpdateState == null ? Guid.Empty : postUpdateState.Id);
		}

		private static IEnumerable<Entity> FetchEntityReferences(OrganizationServiceContext serviceContext, string entityLogicalName, string idAttribute, string primaryNameAttribute, string stateAttribute = null, int state = 0)
		{
			var fetch = new Fetch
			{
				Distinct = true,
				Entity = new FetchEntity(entityLogicalName)
				{
					Attributes = new[]
					{
						new FetchAttribute(idAttribute), 
						new FetchAttribute(primaryNameAttribute)
					},
					Orders = new[]
					{
						new Order(primaryNameAttribute, OrderType.Ascending)
					}
				}
			};

			if (!string.IsNullOrEmpty(stateAttribute))
			{
				fetch.Entity.Filters = new[]
				{
					new Filter
					{
						Type = LogicalOperator.And,
						Conditions = new[]
						{
							new Condition(stateAttribute, ConditionOperator.Equal, state)
						}
					}
				};
			}

			return ((RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest()))
				.EntityCollection
				.Entities
				.ToArray();
		}

		private static IEnumerable<Entity> FetchPageTemplateReferences(OrganizationServiceContext serviceContext, Guid websiteId, string filter = null)
		{
			var fetch = new Fetch
			{
				Distinct = true,
				Entity = new FetchEntity("adx_pagetemplate")
				{
					Attributes = new[]
					{
						new FetchAttribute("adx_pagetemplateid"), 
						new FetchAttribute("adx_name"),
						new FetchAttribute("adx_isdefault"), 
						new FetchAttribute("adx_description"), 
					},
					Filters = new List<Filter>
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Conditions = new[]
							{
								new Condition("adx_websiteid", ConditionOperator.Equal, websiteId),
								new Condition("statecode", ConditionOperator.Equal, 0)
							}
						},
					},
					Orders = new[]
					{
						new Order("adx_name", OrderType.Ascending)
					}
				}
			};

			if (!string.IsNullOrEmpty(filter))
			{
				fetch.Entity.Filters.Add(new Filter
				{
					Type = LogicalOperator.Or,
					Conditions = new[]
					{
						new Condition("adx_entityname", ConditionOperator.Equal, filter),
						new Condition("adx_entityname", ConditionOperator.Null)
					}
				});
			}

			return ((RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest()))
				.EntityCollection
				.Entities
				.ToArray();
		}

		private static readonly IDictionary<string, Tuple<string, string, string>> TaggableEntities = new Dictionary<string, Tuple<string, string, string>>
		{
			{ "adx_blogpost", new Tuple<string, string, string>("adx_blogpost", "adx_blogpostid", "adx_blogpost_tag") },
			{ "adx_communityforumthread", new Tuple<string, string, string>("adx_communityforumthread", "adx_communityforumthreadid", "adx_communityforumthread_tag") },
		};

		private static readonly IDictionary<string, WebsiteJoin> WebsiteJoins = new Dictionary<string, WebsiteJoin>
		{
			{ "adx_communityforumthread", new WebsiteJoin("adx_communityforumthread", "adx_forumid", "adx_communityforum", "adx_communityforumid", "adx_websiteid") },
			{ "adx_eventschedule", new WebsiteJoin("adx_eventschedule", "adx_eventid", "adx_event", "adx_eventid", "adx_websiteid") },
			{ "adx_weblink", new WebsiteJoin("adx_weblink", "adx_weblinksetid", "adx_weblinkset", "adx_weblinksetid", "adx_websiteid") },
			{ "adx_blogpost", new WebsiteJoin("adx_blogpost", "adx_blogid", "adx_blog", "adx_blogid", "adx_websiteid") }
		};

		private class WebsiteJoin
		{
			public WebsiteJoin(string referencingEntity, string referencingAttribute, string referencedEntity, string referencedAttribute, string websiteReferenceAttribute)
			{
				ReferencingEntity = referencingEntity;
				ReferencingAttribute = referencingAttribute;
				ReferencedEntity = referencedEntity;
				ReferencedAttribute = referencedAttribute;
				WebsiteReferenceAttribute = websiteReferenceAttribute;
			}

			public string ReferencedAttribute { get; private set; }

			public string ReferencedEntity { get; private set; }

			public string ReferencingAttribute { get; private set; }

			public string ReferencingEntity { get; private set; }

			public string WebsiteReferenceAttribute { get; private set; }
		}
	}
}
