/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Forums
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Web.Mvc;
	using Adxstudio.Xrm.Core.Flighting;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Cms;

	public abstract class ForumAggregationDataAdapter : IForumAggregationDataAdapter
	{
		protected ForumAggregationDataAdapter(IDataAdapterDependencies dependencies)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; set; }

		public IEnumerable<IForum> SelectForums()
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var serviceContext = Dependencies.GetServiceContext();
			var languageInfo = Dependencies.GetRequestContext().HttpContext.GetContextLanguageInfo();
			var filter = GetWhereExpression();

			if (languageInfo.IsCrmMultiLanguageEnabled)
			{
				filter.Filters = new[]
				{
					new Filter
					{
						Type = LogicalOperator.Or,
						Conditions = new[]
						{
							new Condition("adx_websitelanguageid", ConditionOperator.Null),
							new Condition("adx_websitelanguageid", ConditionOperator.Equal, languageInfo.ContextLanguage.EntityReference.Id)
						}
					}
				};
			}

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_communityforum")
				{
					Filters = new[] { filter },
					Orders = new[]
					{
						new Order("adx_displayorder"),
						new Order("adx_name")
					}
				}
			};

			var entities = serviceContext.RetrieveMultiple(fetch).Entities;
			var securityProvider = Dependencies.GetSecurityProvider();
			var readableEntities = entities.Where(e => securityProvider.TryAssert(serviceContext, e, CrmEntityRight.Read));

			var counterStrategy = Dependencies.GetCounterStrategy();
			var countss = counterStrategy.GetForumCounts(serviceContext, readableEntities);
			var infos = serviceContext.FetchForumInfos(readableEntities.Select(e => e.Id));
			var urlProvider = Dependencies.GetUrlProvider();

			var forums = readableEntities.Select(entity =>
			{
				IForumInfo info;
				info = infos.TryGetValue(entity.Id, out info) ? info : new UnknownForumInfo();

				ForumCounts counts;
				counts = countss.TryGetValue(entity.Id, out counts) ? counts : new ForumCounts(0, 0);

				var viewEntity = new PortalViewEntity(serviceContext, entity, securityProvider, urlProvider);

				return new Forum(entity, viewEntity, info, counts);
			}).ToArray();

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: Count={0}", forums.Length));

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Forum, HttpContext.Current, "read_forums", "community forum", forums.Length, "adx_communityforum", "read");
			}

			return forums;
		}

		public IForum Select(Guid forumId)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}", forumId));

			var forum = Select(e => e.GetAttributeValue<Guid>("adx_communityforumid") == forumId);

			if (forum == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}", forumId));

			return forum;
		}

		public IForum Select(string forumName)
		{
			if (string.IsNullOrEmpty(forumName))
			{
				return null;
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

            var forum = Select(e => e.GetAttributeValue<string>("adx_name") == forumName);

			if (forum == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

            return forum;
		}

		protected virtual IForum Select(Predicate<Entity> match)
		{
			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();

			var publishingStateAccessProvider = new PublishingStateAccessProvider(Dependencies.GetRequestContext().HttpContext);

			// Bulk-load all ad entities into cache.
			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_communityforum")
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions = new[] { new Condition("adx_websiteid", ConditionOperator.Equal, website.Id) }
						}
					}
				}
			};

			var allEntities = serviceContext.RetrieveMultiple(fetch).Entities;

			var entity = allEntities.FirstOrDefault(e =>
				match(e)
				&& IsActive(e)
				&& publishingStateAccessProvider.TryAssert(serviceContext, e));

			if (entity == null)
			{
				return null;
			}

			var securityProvider = Dependencies.GetSecurityProvider();

			if (!securityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Read))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: Not Found", entity.Id));

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

			return forum;
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

		protected abstract Filter GetWhereExpression();
	}
}
