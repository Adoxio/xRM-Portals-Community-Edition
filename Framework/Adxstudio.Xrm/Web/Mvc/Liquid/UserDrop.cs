/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Forums;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class UserDrop : PortalViewEntityDrop
	{
		private readonly Lazy<string> _basicBadgesUrl;
		private readonly IDictionary<string, bool> _isUserInRoleCache = new Dictionary<string, bool>();
		private readonly Lazy<string> _profileBadgesUrl;
		private readonly Lazy<string[]> _roles;
		private readonly Lazy<string[]> _roleKeys;
		private readonly Lazy<IEnumerable<ForumThreadDrop>> _forumThreadSubscriptions;


		public UserDrop(IPortalLiquidContext portalLiquidContext, IPortalViewEntity viewEntity) : base(portalLiquidContext, viewEntity)
		{
			_basicBadgesUrl = new Lazy<string>(GetBasicBadgesUrl, LazyThreadSafetyMode.None);
			_roles = new Lazy<string[]>(GetRolesForUser, LazyThreadSafetyMode.None);
			_roleKeys = new Lazy<string[]>(GetRoleKeysForUser, LazyThreadSafetyMode.None);
			_profileBadgesUrl = new Lazy<string>(GetProfileBadgesUrl, LazyThreadSafetyMode.None);
			_forumThreadSubscriptions = new Lazy<IEnumerable<ForumThreadDrop>>(() => GetForumThreadSubscriptions(portalLiquidContext), LazyThreadSafetyMode.None);
		}

		public IEnumerable<string> Roles
		{
			get { return _roles.Value; }
		}

		public IEnumerable<string> RoleKeys
		{
			get { return _roleKeys.Value; }
		}

		public string BasicBadgesURL
		{
			get { return _basicBadgesUrl.Value; }
		}

		public IEnumerable<ForumThreadDrop> ForumThreadSubscriptions
		{
			get { return _forumThreadSubscriptions.Value; }
		}

		public string ProfileBadgesURL
		{
			get { return _profileBadgesUrl.Value; }
		}

		public bool IsUserInRole(string roleName)
		{
			bool result;

			if (_isUserInRoleCache.TryGetValue(roleName, out result))
			{
				return result;
			}

			result = System.Web.Security.Roles.IsUserInRole(roleName);

			_isUserInRoleCache[roleName] = result;

			return result;
		}

		private string[] GetRolesForUser()
		{
			return System.Web.Security.Roles.GetRolesForUser();
		}

		private string[] GetRoleKeysForUser()
		{
			var userRoles = _roles.Value;

			if (!userRoles.Any())
			{
				return new string[] { };
			}

			var serviceContext = PortalViewContext.CreateServiceContext();

			var entityMetadata = serviceContext.GetEntityMetadata("adx_webrole", EntityFilters.Attributes);

			// Must check if new attribute exists to maintain compatability with previous schema versions and prevent runtime 
			// exceptions when portal code updates are pushed to web apps where new solutions have not yet been applied.
			if (entityMetadata == null || entityMetadata.Attributes == null ||
				!entityMetadata.Attributes.Select(a => a.LogicalName).Contains("adx_key"))
			{
				return new string[] { };
			}

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_webrole")
				{
					Attributes = new List<FetchAttribute> { new FetchAttribute("adx_key"), new FetchAttribute("adx_name") },
					Filters = new List<Filter>
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Filters = new List<Filter>
							{
								new Filter
								{
									Type = LogicalOperator.Or,
									Conditions = userRoles.Select(role => new Condition("adx_name", ConditionOperator.Equal, role)).ToList()
								}
							}
						}
					}
				}
			};

			var response = (RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest());

			if (response == null || !response.EntityCollection.Entities.Any())
			{
				return new string[] { };
			}

			var keys = response.EntityCollection.Entities.Where(e => e.Attributes.Contains("adx_key")).Select(e => e.Attributes["adx_key"].ToString()).ToArray();

			return keys;
		}

		private string GetBasicBadgesUrl()
		{
			return UrlHelper.RouteUrl("PortalBadges", new
			{
				__portalScopeId__ = PortalViewContext.Website.EntityReference.Id,
				userId = Id,
				type = "basic-badges"
			});
		}

		private string GetProfileBadgesUrl()
		{
			return UrlHelper.RouteUrl("PortalBadges", new
			{
				__portalScopeId__ = PortalViewContext.Website.EntityReference.Id,
				userId = Id,
				type = "profile-badges"
			});
		}

		private IEnumerable<ForumThreadDrop> GetForumThreadSubscriptions(IPortalLiquidContext portalLiquidContext)
		{
			var user = Entity;

			if (user == null || user.LogicalName != "contact")
			{
				return Enumerable.Empty<ForumThreadDrop>();
			}

			var portal = PortalViewContext.CreatePortalContext();

			var forumDependencies = new PortalContextDataAdapterDependencies(portal, new PaginatedLatestPostUrlProvider("page", portalLiquidContext.Html.IntegerSetting("Forums/PostsPerPage").GetValueOrDefault(20)));

			var dataAdapter = new ForumThreadAggregationDataAdapter(forumDependencies, true,
				serviceContext => serviceContext.FetchForumCountsForWebsite(PortalViewContext.Website.EntityReference.Id),
				serviceContext => (from thread in serviceContext.CreateQuery("adx_communityforumthread")
								   join alert in serviceContext.CreateQuery("adx_communityforumalert") on
									   thread.GetAttributeValue<Guid>("adx_communityforumthreadid") equals
									   alert.GetAttributeValue<EntityReference>("adx_threadid").Id
								   join forum in serviceContext.CreateQuery("adx_communityforum") on
									   thread.GetAttributeValue<EntityReference>("adx_forumid").Id equals
									   forum.GetAttributeValue<Guid>("adx_communityforumid")
								   where
									   forum.GetAttributeValue<EntityReference>("adx_websiteid") != null &&
									   forum.GetAttributeValue<EntityReference>("adx_websiteid").Id == PortalViewContext.Website.EntityReference.Id
								   where
									   alert.GetAttributeValue<EntityReference>("adx_subscriberid") != null &&
									   alert.GetAttributeValue<EntityReference>("adx_subscriberid").Id == user.Id
								   orderby thread.GetAttributeValue<DateTime>("adx_lastpostdate") descending
								   orderby thread.GetAttributeValue<string>("adx_name")
								   select thread),
				serviceContext => serviceContext.FetchForumThreadTagInfoForWebsite(PortalViewContext.Website.EntityReference.Id),
				new ForumThreadAggregationDataAdapter.ForumThreadUrlProvider(forumDependencies.GetUrlProvider()));

			var forumThreads = dataAdapter.SelectThreads(0).ToList();

			return !forumThreads.Any()
				? Enumerable.Empty<ForumThreadDrop>()
				: forumThreads.Select(e => new ForumThreadDrop(portalLiquidContext, forumDependencies, e));
		}
	}
}
