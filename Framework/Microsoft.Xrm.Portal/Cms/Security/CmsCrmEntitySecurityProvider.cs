/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Security;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Cms.Security
{
	public sealed class CmsCrmEntitySecurityProvider : CrmEntitySecurityProvider
	{
		private static readonly string[] _handledEntityNames = new[]
		{
			"adx_webfile", "adx_webpage", "adx_contentsnippet", "adx_pagetemplate", "adx_sitemarker", "adx_weblinkset", "adx_weblink"
		};

		public string PortalName { get; private set; }
		public string SiteSettingName { get; private set; }

		public CmsCrmEntitySecurityProvider(string portalName)
		{
			PortalName = portalName;
		}

		public override void Initialize(string name, NameValueCollection config)
		{
			base.Initialize(name, config);

			SiteSettingName = config["siteSettingName"] ?? "Security/AdministratorsRoleName";
		}

		public override bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right)
		{
			if (entity == null || entity.Id == Guid.Empty)
			{
				return false;
			}

			entity.AssertEntityName(_handledEntityNames);

			CrmEntityInactiveInfo inactiveInfo;

			if (CrmEntityInactiveInfo.TryGetInfo(entity.LogicalName, out inactiveInfo) && inactiveInfo.IsInactive(entity))
			{
				return false;
			}

			var website = context.GetWebsite(entity);

			if (website == null)
			{
				return false;
			}

			if (right == CrmEntityRight.Read)
			{
				return true;
			}

			if (HttpContext.Current.User == null
				|| HttpContext.Current.User.Identity == null
				|| HttpContext.Current.User.Identity.Name == null
				|| !HttpContext.Current.User.Identity.IsAuthenticated)
			{
				return false;
			}

			var roleName = context.GetSiteSettingValueByName(website, SiteSettingName);

			var result = !string.IsNullOrEmpty(roleName) && Roles.IsUserInRole(roleName);

			return result;
		}
	}
}
