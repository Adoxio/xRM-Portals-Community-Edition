/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Security;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Security
{
	internal class CrmEntitySecurityCacheInfo
	{
		public CrmEntitySecurityCacheInfo(Entity entity, CrmEntityRight right, string securityContextKey)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.Id == null)
			{
				throw new ArgumentException("Parameter ID must have a value.", "entity");
			}

			if (securityContextKey == null)
			{
				throw new ArgumentNullException("securityContextKey");
			}

			Key = BuildKey(entity, right, securityContextKey);
		}

		protected CrmEntitySecurityCacheInfo() { }

		public virtual bool IsCacheable
		{
			get { return true; }
		}

		public virtual string Key { get; private set; }

		protected virtual bool TryGetCurrentIdentity(out IIdentity identity)
		{
			if (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
			{
				identity = HttpContext.Current.User.Identity;

				return true;
			}

			if (ServiceSecurityContext.Current != null && ServiceSecurityContext.Current.PrimaryIdentity != null)
			{
				identity = ServiceSecurityContext.Current.PrimaryIdentity;

				return true;
			}

			if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null)
			{
				identity = Thread.CurrentPrincipal.Identity;

				return true;
			}

			identity = null;

			return false;
		}

		private string BuildKey(Entity entity, CrmEntityRight right, string securityContextKey)
		{
			var baseKey = string.Concat(securityContextKey, ":", entity.Id, ":", right);

            IIdentity identity;

			return TryGetCurrentIdentity(out identity)
				? string.Concat(baseKey, ":Identity=", identity.Name)
                : string.Concat(baseKey, ":Anonymous");
        }
	}

	internal class UncacheableCrmEntitySecurityCacheInfo : CrmEntitySecurityCacheInfo
	{
		public override bool IsCacheable
		{
			get { return false; }
		}
	}

	internal interface ICrmEntitySecurityCacheInfoFactory
	{
		CrmEntitySecurityCacheInfo GetCacheInfo(OrganizationServiceContext context, Entity entity, CrmEntityRight right);
	}

	internal class CrmEntitySecurityCacheInfoFactory : ICrmEntitySecurityCacheInfoFactory
	{
		public CrmEntitySecurityCacheInfoFactory(string securityContextKey)
		{
			if (securityContextKey == null)
			{
				throw new ArgumentNullException("securityContextKey");
			}

			SecurityContextKey = securityContextKey;
		}

		protected string SecurityContextKey { get; private set; }

		public virtual CrmEntitySecurityCacheInfo GetCacheInfo(OrganizationServiceContext context, Entity entity, CrmEntityRight right)
		{
			if (entity == null || entity.Id == null)
			{
				return new UncacheableCrmEntitySecurityCacheInfo();
			}

			return new CrmEntitySecurityCacheInfo(entity, right, SecurityContextKey);
		}
	}

	internal class VaryByPreviewCrmEntitySecurityCacheInfoFactory : CrmEntitySecurityCacheInfoFactory
	{
		public VaryByPreviewCrmEntitySecurityCacheInfoFactory(string securityContextKey) : base(securityContextKey) { }

		public override CrmEntitySecurityCacheInfo GetCacheInfo(OrganizationServiceContext context, Entity entity, CrmEntityRight right)
		{
			if (entity == null || entity.Id == null)
			{
				return new UncacheableCrmEntitySecurityCacheInfo();
			}

			try
			{
				var website = context.GetWebsite(entity);

				if (website == null)
				{
					return new UncacheableCrmEntitySecurityCacheInfo();
				}

				var previewPermission = new PreviewPermission(context, website);

				var variedSecurityContextKey = "{0}:preview={1}".FormatWith(SecurityContextKey, previewPermission.IsEnabled);

				return new CrmEntitySecurityCacheInfo(entity, right, variedSecurityContextKey);
			}
			catch
			{
				return new UncacheableCrmEntitySecurityCacheInfo();
			}
		}
	}
}
