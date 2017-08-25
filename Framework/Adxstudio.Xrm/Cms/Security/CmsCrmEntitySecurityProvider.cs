/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms.Security
{
	using System;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.Linq;
	using System.Web;
	using Adxstudio.Xrm.Blogs;
	using Adxstudio.Xrm.Cases;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Events.Security;
	using Adxstudio.Xrm.Forums.Security;
	using Adxstudio.Xrm.Ideas;
	using Adxstudio.Xrm.Issues;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Diagnostics.Trace;
	using Adxstudio.Xrm.Web;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Microsoft.Xrm.Sdk.Query;

	public class CmsCrmEntitySecurityProvider : CrmEntitySecurityProvider
	{
		private ICacheSupportingCrmEntitySecurityProvider _underlyingProvider;

		public override void Initialize(string name, NameValueCollection config)
		{
			base.Initialize(name, config);

			var portalName = config["portalName"];

			var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider(portalName);

			_underlyingProvider = contentMapProvider != null
				? new ContentMapUncachedProvider(contentMapProvider)
				: new UncachedProvider(portalName);

			var cacheInfoFactory = new CrmEntitySecurityCacheInfoFactory(GetType().FullName);

			bool cachingEnabled;
			if (!bool.TryParse(config["cachingEnabled"], out cachingEnabled)) { cachingEnabled = true; }

			if (cachingEnabled)
			{
				_underlyingProvider = new ApplicationCachingCrmEntitySecurityProvider(_underlyingProvider, cacheInfoFactory);
			}

			bool requestCachingEnabled;
			if (!bool.TryParse(config["requestCachingEnabled"], out requestCachingEnabled)) { requestCachingEnabled = true; }

			if (requestCachingEnabled && HttpContext.Current != null)
			{
				_underlyingProvider = new RequestCachingCrmEntitySecurityProvider(_underlyingProvider, cacheInfoFactory);
			}
		}

		public override bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right)
		{
			return _underlyingProvider.TryAssert(context, entity, right);
		}

		internal class UncachedProvider : CacheSupportingCrmEntitySecurityProvider
		{
			private readonly WebPageAccessControlSecurityProvider _webPageAccessControlProvider;
			private readonly PublishedDatesAccessProvider _publishedDatesAccessProvider;
			private readonly PublishingStateAccessProvider _publishingStateAccessProvider;
			private readonly EventAccessPermissionProvider _eventAccessPermissionProvider;
			private readonly ForumAccessPermissionProvider _forumAccessPermissionProvider;
			private readonly BlogSecurityProvider _blogSecurityProvider;
			private readonly IdeaSecurityProvider _ideaSecurityProvider;
			private readonly IssueSecurityProvider _issueSecurityProvider;
			private readonly HttpContext current;

			public UncachedProvider(string portalName = null)
				: this(new WebPageAccessControlSecurityProvider(HttpContext.Current), new PublishedDatesAccessProvider(HttpContext.Current), new PublishingStateAccessProvider(HttpContext.Current), portalName)
			{
				this.current = HttpContext.Current;
			}

			protected UncachedProvider(
				WebPageAccessControlSecurityProvider webPageAccessControlProvider,
				PublishedDatesAccessProvider publishedDatesAccessProvider,
				PublishingStateAccessProvider publishingStateAccessProvider, string portalName = null)
			{
				_webPageAccessControlProvider = webPageAccessControlProvider;
				_publishedDatesAccessProvider = publishedDatesAccessProvider;
				_publishingStateAccessProvider = publishingStateAccessProvider;
				_eventAccessPermissionProvider = new EventAccessPermissionProvider();
				_forumAccessPermissionProvider = new ForumAccessPermissionProvider(this.current);
				_blogSecurityProvider = new BlogSecurityProvider(_webPageAccessControlProvider, this.current, portalName);
				_ideaSecurityProvider = new IdeaSecurityProvider(this.current, portalName);
				_issueSecurityProvider = new IssueSecurityProvider(portalName);

				PortalName = portalName;
			}

			protected string PortalName { get; private set; }

			public override bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				if (entity != null)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(
						"right={0}, logicalName={1}, id={2}",
						right, EntityNamePrivacy.GetEntityName(entity.LogicalName),
						entity.Id));
				}
				else
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("right={0}", right));
				}

				var timer = Stopwatch.StartNew();

				var result = InnerTryAssert(context, entity, right, dependencies);

				timer.Stop();

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("result={0}, duration={1} ms", result, timer.ElapsedMilliseconds));

				return result;
			}

			private bool InnerTryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				if (entity == null)
				{
					return false;
				}

				if (right == CrmEntityRight.Read && (!_publishedDatesAccessProvider.TryAssert(context, entity) || !_publishingStateAccessProvider.TryAssert(context, entity)))
				{
					// We let the date and state access providers handle their own caching logic, so we signal any
					// caching providers above this one to not cache this result.
					dependencies.IsCacheable = false;

					return false;
				}

				dependencies.AddEntityDependency(entity);

				var entityName = entity.LogicalName;

				CrmEntityInactiveInfo inactiveInfo;

				if (CrmEntityInactiveInfo.TryGetInfo(entityName, out inactiveInfo) && inactiveInfo.IsInactive(entity))
				{
					return false;
				}

				if (entityName == "adx_webpage")
				{
					return TestWebPage(context, entity, right, dependencies);
				}

				if (entityName == "feedback")
				{
					return TestFeedback(context, entity, right, dependencies);
				}

				if (entityName == "adx_event")
				{
					return TestEvent(context, entity, right, dependencies);
				}

				if (entityName == "adx_eventschedule")
				{
					return TestEventSchedule(context, entity, right, dependencies);
				}

				if ((entityName == "adx_eventspeaker" || entityName == "adx_eventsponsor") && right == CrmEntityRight.Read)
				{
					return true;
				}

				if (entityName == "adx_communityforum")
				{
					return TestForum(context, entity, right, dependencies);
				}

				if (entityName == "adx_communityforumthread")
				{
					return TestForumThread(context, entity, right, dependencies);
				}

				if (entityName == "adx_communityforumpost")
				{
					return TestForumPost(context, entity, right, dependencies);
				}

				if (entityName == "adx_communityforumannouncement")
				{
					return TestForumAnnouncement(context, entity, right, dependencies);
				}

				if (entityName == "adx_forumthreadtype")
				{
					return right == CrmEntityRight.Read;
				}

				if ((entityName == "adx_pagetemplate" || entityName == "adx_publishingstate" || "adx_websitelanguage" == entityName) && right == CrmEntityRight.Read)
				{
					return true;
				}

				if (entityName == "adx_webfile")
				{
					return TestWebFile(context, entity, right, dependencies);
				}

				if (entityName == "adx_contentsnippet" || entityName == "adx_weblinkset" || entityName == "adx_weblink" || entityName == "adx_sitemarker")
				{
					return TestWebsiteAccessPermission(context, entity, right, dependencies);
				}

				if (entityName == "adx_shortcut")
				{
					return TestShortcut(context, entity, right, dependencies);
				}

				if (entityName == "adx_blog")
				{
					return TestBlog(context, entity, right, dependencies);
				}

				if (entityName == "adx_blogpost")
				{
					return TestBlogPost(context, entity, right, dependencies);
				}

				if (entityName == "adx_ideaforum")
				{
					return TestIdeaForum(context, entity, right, dependencies);
				}

				if (entityName == "adx_idea")
				{
					return TestIdea(context, entity, right, dependencies);
				}

				if (entityName == "adx_ideacomment")
				{
					return TestIdeaComment(context, entity, right, dependencies);
				}

				if (entityName == "adx_ideavote")
				{
					return TestIdeaVote(context, entity, right, dependencies);
				}

				if (entityName == "adx_issueforum")
				{
					return TestIssueForum(context, entity, right, dependencies);
				}

				if (entityName == "adx_issue")
				{
					return TestIssue(context, entity, right, dependencies);
				}

				if (entityName == "adx_issuecomment")
				{
					return TestIssueComment(context, entity, right, dependencies);
				}

				if (entityName == "adx_adplacement" || entityName == "adx_ad")
				{
					return right == CrmEntityRight.Read;
				}

				if (entityName == "email")
				{
					return right == CrmEntityRight.Read;
				}

				if (entityName == "incident" && right == CrmEntityRight.Read)
				{
					return IsPublicCase(entity);
				}

				if (IsPortalKbArticle(entity))
				{
					return right == CrmEntityRight.Read;
				}

				if (IsPublicKnowledgeArticle(entity))
				{
					return right == CrmEntityRight.Read;
				}

				if (IsCategory(entity))
				{
					return right == CrmEntityRight.Read;
				}

				if (IsAnnotation(entity))
				{
					return right == CrmEntityRight.Read;
				}

				// To allow note attachments to be read by the customer to which the order or quote belongs.
				if (entityName == "salesorder" || entityName == "quote")
				{
					var customerid = entity.GetAttributeValue<EntityReference>("customerid");
					var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);

					return right == CrmEntityRight.Read
						&& customerid != null
						&& portalContext != null
						&& portalContext.User != null
						&& customerid.Equals(portalContext.User.ToEntityReference());
				}

				if (TestServiceRequest(context, entity))
				{
					return right == CrmEntityRight.Read;
				}

				Entity parentPermit;

				if (TryGetParentPermit(context, entity, out parentPermit))
				{
					return TestParentPermit(context, parentPermit, right);
				}

				return false;
			}

			protected virtual bool TestBlog(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && _blogSecurityProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestBlogPost(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && _blogSecurityProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestBlogPost(OrganizationServiceContext context, EntityReference entityReference, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entityReference != null) && _blogSecurityProvider.TryAssert(context, entityReference, right, dependencies);
			}

			protected virtual bool TestEvent(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && _eventAccessPermissionProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestEventSchedule(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null)
					&& entity.GetAttributeValue<EntityReference>("adx_eventid") != null
					&& TestEvent(context, entity.GetRelatedEntity(context, "adx_event_eventschedule"), right, dependencies);
			}

			protected virtual bool TestForum(OrganizationServiceContext context, EntityReference entityReference, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entityReference != null) && _forumAccessPermissionProvider.TryAssert(context, entityReference, right, dependencies);
			}

			protected virtual bool TestForum(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && _forumAccessPermissionProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestForumAnnouncement(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				if (entity == null)
				{
					return false;
				}

				return this.TestForum(context, entity.GetAttributeValue<EntityReference>("adx_forumid"), right, dependencies);
			}

			protected virtual bool TestForumThread(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				if (entity == null)
				{
					return false;
				}

				return this.TestForum(context, entity.GetAttributeValue<EntityReference>("adx_forumid"), right, dependencies);
			}

			protected virtual bool TestForumPost(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				if (entity == null || entity.GetAttributeValue<EntityReference>("adx_forumthreadid") == null)
				{
					return false;
				}

				var threadRef = entity.GetAttributeValue<EntityReference>("adx_forumthreadid");

				var fetch = new Fetch
				{
					Entity = new FetchEntity("adx_communityforumthread", new[] { "adx_forumid" })
					{
						Filters = new[]
						{
							new Filter
							{
								Conditions = new[] { new Condition("adx_communityforumthreadid", ConditionOperator.Equal, threadRef.Id) }
							}
						}
					}
				};

				var thread = context.RetrieveSingle(fetch);

				return this.TestForumThread(context, thread, right, dependencies);
			}

			protected virtual bool TestIdeaForum(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && _ideaSecurityProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestIdea(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && _ideaSecurityProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestIdeaComment(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && _ideaSecurityProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestIdeaVote(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && _ideaSecurityProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestIssueForum(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && _issueSecurityProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestIssue(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && _issueSecurityProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestIssueComment(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && _issueSecurityProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestSurvey(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return true;
			}

			protected virtual bool TestShortcut(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				// For shortcut change permission, always test rights on shortcut parent.
				if (right == CrmEntityRight.Change)
				{
					return TestShortcutParent(context, entity, right, dependencies);
				}

				return entity.GetAttributeValue<bool?>("adx_disabletargetvalidation").GetValueOrDefault(false)
					? TestShortcutParent(context, entity, right, dependencies)
					: TestShortcutTarget(context, entity, right, dependencies);
			}

			protected virtual bool TestShortcutTarget(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				if (entity == null)
				{
					return false;
				}

				if (!string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_externalurl")))
				{
					return true;
				}

				if (entity.GetAttributeValue<EntityReference>("adx_webpageid") != null)
				{
					return this.TestWebPage(context, entity.GetAttributeValue<EntityReference>("adx_webpageid"), right, dependencies);
				}

				if (entity.GetAttributeValue<EntityReference>("adx_webfileid") != null)
				{
					var reference = entity.GetAttributeValue<EntityReference>("adx_webfileid");
					var webfile = context.RetrieveSingle(
						"adx_webfile",
						new[] { "adx_blogpostid", "adx_parentpageid" },
						new Condition("adx_webfileid", ConditionOperator.Equal, reference.Id));

					return this.TestWebFile(context, webfile, right, dependencies);
				}

				if (entity.GetAttributeValue<EntityReference>("adx_forumid") != null)
				{
					return this.TestForum(context, entity.GetAttributeValue<EntityReference>("adx_forumid"), right, dependencies);
				}

				// legacy entities
				if (entity.GetAttributeValue<EntityReference>("adx_surveyid") != null)
				{
					return TestSurvey(context, entity.GetRelatedEntity(context, "adx_survey_shortcut"), right, dependencies);
				}

				if (entity.GetAttributeValue<EntityReference>("adx_eventid") != null)
				{
					return TestEvent(context, entity.GetRelatedEntity(context, "adx_event_shortcut"), right, dependencies);
				}

				return false;
			}

			protected virtual bool TestShortcutParent(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && TestWebPage(context, entity.GetAttributeValue<EntityReference>("adx_webpageid"), right, dependencies);
			}

			protected virtual bool TestParentWebPage(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && TestWebPage(context, entity.GetAttributeValue<EntityReference>("adx_parentpageid"), right, dependencies);
			}

			protected virtual bool TestWebFile(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				if (entity == null)
				{
					return false;
				}

				var parentBlogPostReference = entity.GetAttributeValue<EntityReference>("adx_blogpostid");

				if (parentBlogPostReference != null)
				{
					var post = context.RetrieveSingle(parentBlogPostReference, new ColumnSet());
					return this.TestBlogPost(context, post, right, dependencies);
					}

				return TestParentWebPage(context, entity, right, dependencies);
			}

			protected virtual bool TestWebPage(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entity != null) && _webPageAccessControlProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestFileWebPage(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
			{
				return (entity != null) && _webPageAccessControlProvider.TryAssert(context, entity, right, dependencies, map, true);
			}

			protected virtual bool TestWebPage(OrganizationServiceContext context, EntityReference entityReference, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return (entityReference != null) && _webPageAccessControlProvider.TryAssert(context, entityReference, right, dependencies);
			}

			protected virtual WebsiteAccessPermissionProvider CreateWebsiteAccessPermissionProvider(Entity website, WebPageAccessControlSecurityProvider webPageAccessControlProvider)
			{
				return new WebsiteAccessPermissionProvider(website, HttpContext.Current);
			}

			protected virtual bool TestWebsiteAccessPermission(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				var website = context.GetWebsite(entity);

				if (website == null) return false;

				var securityProvider = CreateWebsiteAccessPermissionProvider(website, _webPageAccessControlProvider);

				return securityProvider.TryAssert(context, entity, right, dependencies);
			}

			protected virtual bool TestFeedback(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(@"Testing right {0} on feedback ({1}).", right, entity.Id));

				dependencies.AddEntityDependency(entity);

				EntityReference relatedReference = entity.GetAttributeValue<EntityReference>("regardingobjectid");
				if (relatedReference == null)
				{
					return false;
				}

				// Determine the primary ID attribute of the regarding object
				var request = new RetrieveEntityRequest
				{
					LogicalName = relatedReference.LogicalName,
					EntityFilters = EntityFilters.Entity
				};

				var response = context.Execute(request) as RetrieveEntityResponse;
				if (response == null || response.EntityMetadata == null)
				{
					return false;
				}

				var primaryIdAttribute = response.EntityMetadata.PrimaryIdAttribute;

				// Retrieve the regarding object
				var relatedEntity = context.CreateQuery(relatedReference.LogicalName)
					.FirstOrDefault(e => e.GetAttributeValue<Guid>(primaryIdAttribute) == relatedReference.Id);

				if (relatedEntity == null)
				{
					return false;
				}

				var approved = entity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);

				// If the right being asserted is Read, and the comment is approved, assert whether the post is readable.
				if (right == CrmEntityRight.Read && approved)
				{
					return TryAssert(context, relatedEntity, right, dependencies);
				}

				var author = entity.GetAttributeValue<EntityReference>("createdbycontact");

				// If there's no author on the post for some reason, only allow posts that are published, and pass the same assertion on the blog.
				if (author == null)
				{
					return approved && TryAssert(context, relatedEntity, right, dependencies);
				}

				var portal = PortalCrmConfigurationManager.CreatePortalContext();

				// If we can't get a current portal user, only allow posts that are published, and pass the same assertion on the blog.
				if (portal == null || portal.User == null)
				{
					return approved && TryAssert(context, relatedEntity, right, dependencies);
				}
				
				return TryAssert(context, relatedEntity, right, dependencies);
			}

			protected virtual bool TestServiceRequest(OrganizationServiceContext context, Entity entity)
			{
				return (entity.GetAttributeValue<EntityReference>("adx_servicerequest") ?? entity.GetAttributeValue<EntityReference>("adx_servicerequestid")) != null;
			}

			protected virtual bool TryGetParentPermit(OrganizationServiceContext context, Entity entity, out Entity parentPermit)
			{
				var permitReference = entity.GetAttributeValue<EntityReference>("adx_permit") ??
									  entity.GetAttributeValue<EntityReference>("adx_permitid");

				if (permitReference != null)
				{
					parentPermit = context.CreateQuery("adx_permit").FirstOrDefault(
							p => p.GetAttributeValue<Guid>("adx_permitid") == permitReference.Id);
					return true;
				}
				parentPermit = null;
				return false;
			}

			protected virtual bool TestParentPermit(OrganizationServiceContext context, Entity entity, CrmEntityRight right)
			{
				var contactid = entity.GetAttributeValue<EntityReference>("adx_regardingcontact")
					?? entity.GetAttributeValue<EntityReference>("adx_regardingcontactid");

				var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);

				return right == CrmEntityRight.Read
						&& contactid != null
						&& portalContext != null
						&& portalContext.User != null
						&& contactid.Equals(portalContext.User.ToEntityReference());
			}

			private static bool IsPublicCase(Entity entity)
			{
				var statecode = entity.GetAttributeValue<OptionSetValue>("statecode");

				return entity.GetAttributeValue<bool?>("adx_publishtoweb").GetValueOrDefault()
					&& statecode != null
					&& statecode.Value == (int)IncidentState.Resolved;
			}

			private enum KbArticleState
			{
				Draft = 1,
				Unapproved = 2,
				Published = 3,
			}

			private static bool IsPortalKbArticle(Entity entity)
			{
				return entity != null
					&& entity.LogicalName == "kbarticle"
					&& (entity.GetAttributeValue<OptionSetValue>("statecode") != null && entity.GetAttributeValue<OptionSetValue>("statecode").Value == (int)KbArticleState.Published)
					&& entity.GetAttributeValue<bool?>("msa_publishtoweb").GetValueOrDefault();
			}

			private enum KnowledgeArticleState
			{
				Draft = 0,
				Approved = 1,
				Scheduled = 2,
				Published = 3,
				Expired = 4,
				Archived = 5,
				Discarded = 6
			}

			private static bool IsPublicKnowledgeArticle(Entity entity)
			{
				return entity != null
					&& entity.LogicalName == "knowledgearticle"
					&& (entity.GetAttributeValue<OptionSetValue>("statecode") != null && entity.GetAttributeValue<OptionSetValue>("statecode").Value == (int)KnowledgeArticleState.Published)
					&& !entity.GetAttributeValue<bool?>("isrootarticle").GetValueOrDefault(false)
					&& !entity.GetAttributeValue<bool?>("isinternal").GetValueOrDefault(false);
			}

			private static bool IsCategory(Entity entity)
			{
				return entity != null && entity.LogicalName == "category";
			}

			private static bool IsAnnotation(Entity entity)
			{
				var notesFilter = HttpContext.Current.GetSiteSetting("KnowledgeManagement/NotesFilter") ?? string.Empty;

				return entity != null 
					&& entity.LogicalName == "annotation"
					&& entity.GetAttributeValue<string>("objecttypecode") == "knowledgearticle"
					&& entity.GetAttributeValue<string>("notetext").StartsWith(notesFilter);
			}
		}

		internal class ContentMapUncachedProvider : UncachedProvider
		{
			private readonly IContentMapProvider _contentMapProvider;

			public ContentMapUncachedProvider(IContentMapProvider contentMapProvider)
				: base(
					new WebPageAccessControlSecurityProvider(contentMapProvider),
					new PublishedDatesAccessProvider(contentMapProvider),
					new PublishingStateAccessProvider(contentMapProvider))
			{
				_contentMapProvider = contentMapProvider;
			}

			protected override WebsiteAccessPermissionProvider CreateWebsiteAccessPermissionProvider(Entity website, WebPageAccessControlSecurityProvider webPageAccessControlProvider)
			{
				return new WebsiteAccessPermissionProvider(website, webPageAccessControlProvider, _contentMapProvider);
			}

			protected override bool TestShortcutTarget(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				if (entity == null)
				{
					return false;
				}

				return _contentMapProvider.Using(map => TestShortcutTarget(context, entity, right, dependencies, map));
			}

			private bool TestShortcutTarget(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
			{
				ShortcutNode shortcut;

				if (map.TryGetValue(entity, out shortcut))
				{
					if (!string.IsNullOrWhiteSpace(shortcut.ExternalUrl))
					{
						return true;
					}

					if (shortcut.WebPage != null)
					{
						return !shortcut.WebPage.IsReference && TestWebPage(context, shortcut.WebPage.ToEntity(), right, dependencies);
					}

					if (shortcut.WebFile != null)
					{
						return !shortcut.WebFile.IsReference && TestWebFile(context, shortcut.WebFile.ToEntity(), right, dependencies);
					}
				}

				return base.TestShortcutTarget(context, entity, right, dependencies);
			}

			protected override bool TestShortcutParent(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return _contentMapProvider.Using(map =>
				{
					ShortcutNode shortcut;

					if (map.TryGetValue(entity, out shortcut))
					{
						return shortcut.Parent != null
							&& !shortcut.Parent.IsReference
							&& TestWebPage(context, shortcut.Parent.ToEntity(), right, dependencies);
					}

					return base.TestShortcutParent(context, entity, right, dependencies);
				});
			}

			protected override bool TestWebFile(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				return _contentMapProvider.Using(map =>
				{
					WebFileNode file;

					if (map.TryGetValue(entity, out file) && file.Parent != null)
					{
						return !file.Parent.IsReference
							&& TestFileWebPage(context, file.Parent.ToEntity(), right, dependencies, map);
					}

					return base.TestWebFile(context, entity, right, dependencies);
				});
			}
		}
	}
}
