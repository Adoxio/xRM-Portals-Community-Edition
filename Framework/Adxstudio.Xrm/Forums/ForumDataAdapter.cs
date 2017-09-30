/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Forums
{
	public class ForumDataAdapter : IForumDataAdapter
	{
		private IForumThreadAggregationDataAdapter _helperDataAdapter;

		public ForumDataAdapter(IDataAdapterDependencies dependencies, 
			Func<OrganizationServiceContext, IQueryable<Entity>> selectThreadEntities)
		{
			if (dependencies == null)
			{
				throw new ArgumentNullException("dependencies");
			}

			Dependencies = dependencies;

			_helperDataAdapter = new WebsiteForumDataAdapter(dependencies, selectThreadEntities);
		}

		public ForumDataAdapter(EntityReference forum, 
			IDataAdapterDependencies dependencies, 
			Func<OrganizationServiceContext, IQueryable<Entity>> selectThreadEntities)
		{
			if (forum == null)
			{
				throw new ArgumentNullException("forum");
			}

			if (forum.LogicalName != "adx_communityforum")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", forum.LogicalName), "forum");
			}
			
			if (dependencies == null)
			{
				throw new ArgumentNullException("dependencies");
			}

			Forum = forum;
			Dependencies = dependencies;

			_helperDataAdapter = new ForumThreadAggregationDataAdapter(
				dependencies,
				false,
				serviceContext => SelectForumCounts(serviceContext, Forum),
				selectThreadEntities,
				serviceContext => serviceContext.FetchForumThreadTagInfo(Forum.Id),
				new ForumThreadAggregationDataAdapter.SingleForumThreadUrlProvider(dependencies.GetUrlProvider(), new Lazy<string>(() =>
				{
					var serviceContext = dependencies.GetServiceContext();

					var forumEntity = SelectForumEntity(serviceContext, forum);

					if (forumEntity == null) return null;

					var urlProvider = dependencies.GetUrlProvider();

					return urlProvider.GetUrl(serviceContext, forumEntity);
				}, LazyThreadSafetyMode.None)));
		}

		public ForumDataAdapter(EntityReference forum, IDataAdapterDependencies dependencies)
			: this(forum, dependencies, serviceContext => CreateThreadEntityQuery(serviceContext, forum))
		{
			
		}

		public ForumDataAdapter(IForum forum, IDataAdapterDependencies dependencies)
			: this(forum.EntityReference, dependencies) { }



		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected EntityReference Forum { get; private set; }

		public IForum Select()
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: Start", Forum.Id));

			var serviceContext = Dependencies.GetServiceContext();

			var entity = SelectForumEntity(serviceContext, Forum);

			if (entity == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: Not Found", Forum.Id));

				return null;
			}

			var securityProvider = Dependencies.GetSecurityProvider();

			if (!securityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Read))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: Not Found", Forum.Id));

				return null;
			}

			var viewEntity = new PortalViewEntity(serviceContext, entity, securityProvider, Dependencies.GetUrlProvider());
			var forumInfo = serviceContext.FetchForumInfo(entity.Id);
			var counterStrategy = Dependencies.GetCounterStrategy();

			var forum = new Forum(
				entity,
				viewEntity,
				forumInfo,
				// Only lazily get counts, because it's unlikely to be used in the common case.
				// SelectThreadCount and SelectPostCount will generally be used instead.
				() => counterStrategy.GetForumCounts(serviceContext, entity));

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: End", Forum.Id));

			return forum;
		}

		public IEnumerable<IForumAnnouncement> SelectAnnouncements()
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: Start", Forum.Id));

			var serviceContext = Dependencies.GetServiceContext();

			var fetch = new Fetch
			{
				Entity = new FetchEntity
				{
					Name = "adx_communityforumannouncement",
					Filters = new[]
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("adx_forumid", ConditionOperator.Equal, Forum.Id),
							}
						}
					},
					Orders = new[] { new Order("adx_date", OrderType.Descending) }
				}
			};

			var entities = serviceContext.RetrieveMultiple(fetch).Entities;
			var securityProvider = Dependencies.GetSecurityProvider();
			var urlProvider = Dependencies.GetUrlProvider();

			var announcements = entities.Select(entity => new ForumAnnouncement(
				entity,
				new PortalViewEntity(serviceContext, entity, securityProvider, urlProvider)))
				.ToArray();

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}, Count={1}: End", Forum.Id, announcements.Length));

			return announcements;
		}

		public int SelectPostCount()
		{
			return _helperDataAdapter.SelectPostCount();
		}

		public int SelectThreadCount()
		{
			return _helperDataAdapter.SelectThreadCount();
		}

		public IEnumerable<IForumThreadType> SelectThreadTypes()
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: Start", Forum.Id));

			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();

			var fetch = new Fetch
			{
				Entity = new FetchEntity
				{
					Name = "adx_forumthreadtype",
					Filters = new[]
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("adx_websiteid", ConditionOperator.Equal, website.Id),
							}
						}
					},
					Orders = new[] { new Order("adx_displayorder", OrderType.Ascending) }
				}
			};

			var entities = serviceContext.RetrieveMultiple(fetch).Entities;

			var threadTypes = entities.Select(entity => new ForumThreadType(entity)).ToArray();

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}, Count={1}: End", Forum.Id, threadTypes.Length));

			return threadTypes;
		}

		public IEnumerable<ListItem> SelectThreadTypeListItems()
		{
			return SelectThreadTypes()
				.Select(type => new ListItem(type.Name, type.EntityReference.Id.ToString()) { Selected = type.IsDefault });
		}

		public IForumThread CreateThread(IForumThread forumThread, IForumPostSubmission forumPost)
		{
			if (forumThread == null) throw new ArgumentNullException("forumThread");

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: Start", Forum.Id));

			var serviceContext = Dependencies.GetServiceContextForWrite();

			var entity = new Entity("adx_communityforumthread");

			entity["adx_forumid"] = Forum;
			entity["adx_name"] = Truncate(forumThread.Name, 100);
			entity["adx_sticky"] = forumThread.IsSticky;
			entity["adx_isanswered"] = forumThread.IsAnswered;
			entity["adx_locked"] = forumThread.Locked;
			entity["adx_typeid"] = forumThread.ThreadType.EntityReference;
			entity["adx_lastpostdate"] = forumPost.PostedOn;

			serviceContext.AddObject(entity);
			serviceContext.SaveChanges();

			var threadDataAdapter = new ForumThreadDataAdapter(entity.ToEntityReference(), Dependencies);

			threadDataAdapter.CreatePost(forumPost, true);

			var createdThread = threadDataAdapter.Select();

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: End", Forum.Id));

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Forum, HttpContext.Current, "create_forum_thread", 1, entity.ToEntityReference(), "create");
			}

			return createdThread;
		}

		public void DeleteThread(EntityReference forumThread)
		{
			if (forumThread == null) throw new ArgumentNullException("forumThread");

			var serviceContext = Dependencies.GetServiceContextForWrite();
			
			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_communityforumthread")
				{
					Filters =
						new[]
						{
							new Filter
							{
								Conditions =
									new[]
									{
										new Condition("adx_communityforumthreadid", ConditionOperator.Equal, forumThread.Id),
										new Condition("adx_forumid", ConditionOperator.Equal, Forum.Id)
									}
							}
						}
				}
			};
			var thread = serviceContext.RetrieveSingle(fetch);

			if (thread == null)
			{
				throw new ArgumentException("Unable to find {0} {1}.".FormatWith(forumThread.LogicalName, forumThread.Id), "forumThread");
			}

			var forum = SelectForumEntity(serviceContext, Forum);

			if (forum == null)
			{
				serviceContext.DeleteObject(thread);
				serviceContext.SaveChanges();

				return;
			}

			var counterStrategy = Dependencies.GetCounterStrategy();
			var threadPostCount = counterStrategy.GetForumThreadPostCount(serviceContext, thread);

			var forumLastPost = forum.GetRelatedEntity(serviceContext, new Relationship("adx_communityforum_lastpost"));

			var forumUpdate = new Entity(forum.LogicalName) { Id = forum.Id };

			// If the last post in the forum is from this thread, update the forum last post.
			if (forumLastPost != null && Equals(forumLastPost.GetAttributeValue<EntityReference>("adx_forumthreadid"), forumThread))
			{
				var lastPostFetch = new Fetch
				{
					Entity =
						new FetchEntity("adx_communityforumthread")
						{
							Filters =
								new[]
								{
									new Filter
									{
										Conditions =
											new[]
											{
												new Condition("adx_forumid", ConditionOperator.Equal, this.Forum.Id),
												new Condition("adx_communityforumthreadid", ConditionOperator.NotEqual, thread.Id),
												new Condition("adx_lastpostid", ConditionOperator.NotNull)
											}
									}
								},
							Orders = new[] { new Order("adx_lastpostdate", OrderType.Descending) }
						},
					PageNumber = 1,
					PageSize = 1
				};

				var lastUpdatedThread = serviceContext.RetrieveSingle(lastPostFetch);

				forumUpdate["adx_lastpostid"] = lastUpdatedThread == null
					? null
					: lastUpdatedThread.GetAttributeValue<EntityReference>("adx_lastpostid");
			}

			var forumCounts = counterStrategy.GetForumCounts(serviceContext, forum);

			forumUpdate["adx_threadcount"] = forumCounts.ThreadCount < 1 ? 0 : forumCounts.ThreadCount - 1;
			forumUpdate["adx_postcount"] = forumCounts.PostCount < threadPostCount ? 0 : forumCounts.PostCount - threadPostCount;

			// Update forum count but skip cache invalidation.
			(serviceContext as IOrganizationService).ExecuteUpdate(forumUpdate, RequestFlag.ByPassCacheInvalidation);
			
			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Forum, HttpContext.Current, "delete_forum_thread", 1, forumThread, "delete");
			}
		}

		public void UpdateLatestPost(IForumPost forumPost, bool incremementForumThreadCount = false)
		{
			UpdateLatestPost(((IForumPostInfo)forumPost).EntityReference, incremementForumThreadCount);
		}

		public void UpdateLatestPost(EntityReference forumPost, bool incremementForumThreadCount = false)
		{
			if (forumPost == null) throw new ArgumentNullException("forumPost");

			forumPost.AssertLogicalName("adx_communityforumpost");

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: Start", Forum.Id));

			var serviceContext = Dependencies.GetServiceContextForWrite();

			var forumEntity = SelectForumEntity(serviceContext, Forum);

			var forumUpdate = new Entity("adx_communityforum") { Id = forumEntity.Id };

			forumUpdate["adx_lastpostid"] = forumPost;

			var counterStrategy = Dependencies.GetCounterStrategy();
			var forumCounts = counterStrategy.GetForumCounts(serviceContext, forumEntity);

			forumUpdate["adx_postcount"] = forumCounts.PostCount + 1;

			if (incremementForumThreadCount)
			{
				forumUpdate["adx_threadcount"] = forumCounts.ThreadCount + 1;
			}

			// Update forum count but skip cache invalidation.
			(serviceContext as IOrganizationService).ExecuteUpdate(forumUpdate, RequestFlag.ByPassCacheInvalidation);

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Forum, HttpContext.Current, "edit_forum_thread", 1, forumPost, "edit");
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: End", Forum.Id));
		}

		public IEnumerable<IForumThread> SelectThreads()
		{
			return _helperDataAdapter.SelectThreads();
		}

		public IEnumerable<IForumThread> SelectThreads(int startRowIndex, int maximumRows = -1)
		{
			return _helperDataAdapter.SelectThreads(startRowIndex, maximumRows);
		}

		public IEnumerable<IForumThreadWeightedTag> SelectWeightedTags(int weights)
		{
			return _helperDataAdapter.SelectWeightedTags(weights);
		}

		private ForumCounts SelectForumCounts(OrganizationServiceContext serviceContext, EntityReference forum)
		{
			var entity = SelectForumEntity(serviceContext, forum);

			return entity == null
				? new ForumCounts(0, 0)
				: Dependencies.GetCounterStrategy().GetForumCounts(serviceContext, entity);
		}

		private static Entity SelectForumEntity(OrganizationServiceContext serviceContext, EntityReference forum)
		{
			return serviceContext.RetrieveSingle("adx_communityforum", "adx_communityforumid", forum.Id, FetchAttribute.All);
		}

		private static IQueryable<Entity> CreateThreadEntityQuery(OrganizationServiceContext serviceContext, EntityReference forum)
		{
			return serviceContext.CreateQuery("adx_communityforumthread")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_forumid") == forum)
				.OrderByDescending(e => e.GetAttributeValue<bool?>("adx_sticky"))
				.OrderByDescending(e => e.GetAttributeValue<DateTime?>("adx_lastpostdate"));
		}

		private static string Truncate(string value, int maxLength)
		{
			if (value == null) return null;

			return value.Length <= maxLength ? value : value.Substring(0, maxLength);
		}
	}
}
