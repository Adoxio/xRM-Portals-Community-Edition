/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Tagging;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Forums
{
	/// <summary>
	/// Helper class to be used internally by other <see cref="IForumThreadAggregationDataAdapter"/> implementations.
	/// </summary>
	internal class ForumThreadAggregationDataAdapter : IForumThreadAggregationDataAdapter
	{
		public ForumThreadAggregationDataAdapter(
			IDataAdapterDependencies dependencies,
			bool filterByForumReadAccess,
			Func<OrganizationServiceContext, ForumCounts> selectForumCounts,
			Func<OrganizationServiceContext, IQueryable<Entity>> selectThreadEntities,
			Func<OrganizationServiceContext, IEnumerable<ITagInfo>> selectTagInfos,
			IForumThreadUrlProvider threadUrlProvider)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");
			if (selectForumCounts == null) throw new ArgumentNullException("selectForumCounts");
			if (selectThreadEntities == null) throw new ArgumentNullException("selectThreadEntities");
			if (selectTagInfos == null) throw new ArgumentNullException("selectTagInfos");
			if (threadUrlProvider == null) throw new ArgumentNullException("threadUrlProvider");

			Dependencies = dependencies;
			FilterByForumReadAccess = filterByForumReadAccess;
			SelectForumCounts = selectForumCounts;
			SelectThreadEntities = selectThreadEntities;
			SelectTagInfos = selectTagInfos;
			ThreadUrlProvider = threadUrlProvider;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected bool FilterByForumReadAccess { get; private set; }

		protected Func<OrganizationServiceContext, ForumCounts> SelectForumCounts { get; private set; }

		protected Func<OrganizationServiceContext, IQueryable<Entity>> SelectThreadEntities { get; private set; }

		protected Func<OrganizationServiceContext, IEnumerable<ITagInfo>> SelectTagInfos { get; private set; }

		protected IForumThreadUrlProvider ThreadUrlProvider { get; private set; }

		public int SelectPostCount()
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var serviceContext = Dependencies.GetServiceContext();

			var postCount = SelectForumCounts(serviceContext).PostCount;

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return postCount;
		}

		public int SelectThreadCount()
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var serviceContext = Dependencies.GetServiceContext();

			var threadCount = SelectForumCounts(serviceContext).ThreadCount;

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return threadCount;
		}

		public IEnumerable<IForumThread> SelectThreads()
		{
			return SelectThreads(0);
		}

		public IEnumerable<IForumThread> SelectThreads(int startRowIndex, int maximumRows = -1)
		{
			if (startRowIndex < 0)
			{
                throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IForumThread[] { };
			}

			return FilterByForumReadAccess
				? SelectThreadsWithSecurity(startRowIndex, maximumRows)
				: SelectThreadsWithoutSecurity(startRowIndex, maximumRows);
		}

		public IEnumerable<IForumThreadWeightedTag> SelectWeightedTags(int weights)
		{
			var serviceContext = Dependencies.GetServiceContext();

			var infos = SelectTagInfos(serviceContext);
			var tagCloudData = new TagCloudData(weights, TagInfo.TagComparer, infos);

			return tagCloudData.Select(e => new ForumThreadWeightedTag(e.Name, e.TaggedItemCount, e.Weight));
		}

		private IEnumerable<IForumThread> SelectThreadsWithSecurity(int startRowIndex, int maximumRows)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("startRowIndex={0}, maximumRows={1}: Start", startRowIndex, maximumRows));

			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();

			var query = SelectThreadEntities(serviceContext);

			var securityProvider = Dependencies.GetSecurityProvider();
			var forumPermissionCache = new Dictionary<Guid, bool>();

			if (maximumRows < 0)
			{
				var readableEntities = query
					.ToArray()
					.Where(e => TryAssertReadPermission(serviceContext, securityProvider, e, forumPermissionCache))
					.Skip(startRowIndex);

				return CreateForumThreads(serviceContext, securityProvider, website, readableEntities);
			}

			var paginator = new PostFilterPaginator<Entity>(
				(offset, limit) => query.Skip(offset).Take(limit).ToArray(),
				e => TryAssertReadPermission(serviceContext, securityProvider, e, forumPermissionCache),
				2);

			var threads = CreateForumThreads(serviceContext, securityProvider, website, paginator.Select(startRowIndex, maximumRows));

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return threads;
		}

		private IEnumerable<IForumThread> SelectThreadsWithoutSecurity(int startRowIndex, int maximumRows)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("startRowIndex={0}, maximumRows={1}: Start", startRowIndex, maximumRows));

			var serviceContext = Dependencies.GetServiceContext();

			var query = SelectThreadEntities(serviceContext);

			if (startRowIndex > 0)
			{
				query = query.Skip(startRowIndex);
			}

			if (maximumRows > 0)
			{
				query = query.Take(maximumRows);
			}

			var securityProvider = Dependencies.GetSecurityProvider();
			var website = Dependencies.GetWebsite();

			var threads = CreateForumThreads(serviceContext, securityProvider, website, query);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return threads;
		}

		private IEnumerable<IForumThread> CreateForumThreads(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, 
			EntityReference website, IEnumerable<Entity> query)
		{
			var entities = query.ToArray();

			var counterStrategy = Dependencies.GetCounterStrategy();
			var postCounts = counterStrategy.GetForumThreadPostCounts(serviceContext, entities);
			var infos = serviceContext.FetchForumThreadInfos(entities.Select(e => e.Id), website.Id);
			var urlProvider = Dependencies.GetUrlProvider();
			var latestPostUrlProvider = Dependencies.GetLatestPostUrlProvider();

			return entities.Select(entity =>
			{
				IForumThreadInfo info;
				info = infos.TryGetValue(entity.Id, out info) ? info : new UnknownForumThreadInfo();

				int postCount;
				postCount = postCounts.TryGetValue(entity.Id, out postCount) ? postCount : 0;

				var viewEntity = new PortalViewEntity(serviceContext, entity, securityProvider, urlProvider);

				var forumThread = new ForumThread(entity, viewEntity, info, postCount, ThreadUrlProvider.GetUrl(serviceContext, entity));

				forumThread.LatestPostUrl = latestPostUrlProvider.GetLatestPostUrl(forumThread, postCount);

				return forumThread;
			}).ToArray();
		}

		protected virtual bool TryAssertReadPermission(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, Entity forumThread, IDictionary<Guid, bool> forumPermissionCache)
		{
			if (forumThread == null)
			{
				throw new ArgumentNullException("forumThread");
			}

			if (forumThread.LogicalName != "adx_communityforumthread")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", forumThread.LogicalName), "forumThread");
			}

			var forumReference = forumThread.GetAttributeValue<EntityReference>("adx_forumid");

			if (forumReference == null)
			{
				throw new ArgumentException(string.Format("Value must have entity reference attribute {0}.", "adx_forumid"), "forumThread");
			}

			bool cachedResult;

			if (forumPermissionCache.TryGetValue(forumReference.Id, out cachedResult))
			{
				return cachedResult;
			}

			var blog = forumThread.GetRelatedEntity(serviceContext, "adx_communityforum_communityforumthread");

			var result = securityProvider.TryAssert(serviceContext, blog, CrmEntityRight.Read);

			forumPermissionCache[blog.Id] = result;

			return result;
		}

		public interface IForumThreadUrlProvider
		{
			string GetUrl(OrganizationServiceContext serviceContext, Entity entity);
		}

		public class ForumThreadUrlProvider : IForumThreadUrlProvider
		{
			private readonly IEntityUrlProvider _urlProvider;

			public ForumThreadUrlProvider(IEntityUrlProvider urlProvider)
			{
				if (urlProvider == null) throw new ArgumentNullException("urlProvider");

				_urlProvider = urlProvider;
			}

			public virtual string GetUrl(OrganizationServiceContext serviceContext, Entity forumThread)
			{
				return _urlProvider.GetUrl(serviceContext, forumThread);
			}
		}

		public class SingleForumThreadUrlProvider : ForumThreadUrlProvider
		{
			private readonly Lazy<string> _forumUrl;

			public SingleForumThreadUrlProvider(IEntityUrlProvider urlProvider, Lazy<string> forumUrl) : base(urlProvider)
			{
				if (forumUrl == null) throw new ArgumentNullException("forumUrl");

				_forumUrl = forumUrl;
			}

			public override string GetUrl(OrganizationServiceContext serviceContext, Entity forumThread)
			{
				if (forumThread == null || forumThread.LogicalName != "adx_communityforumthread")
				{
					return base.GetUrl(serviceContext, forumThread);
				}

				var forumUrl = _forumUrl.Value;

				if (forumUrl == null)
				{
					return base.GetUrl(serviceContext, forumThread);
				}

				return "{0}/{1}".FormatWith(forumUrl.TrimEnd('/'), forumThread.Id);
			}
		}
	}
}
