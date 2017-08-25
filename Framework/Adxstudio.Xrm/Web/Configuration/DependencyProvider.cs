/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Specialized;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Cms.Security;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Web.Data.Services;
using Adxstudio.Xrm.Web.Providers;
using Adxstudio.Xrm.Web.Routing;
using Adxstudio.Xrm.Web.UI;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Data.Services;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Portal.Web.UI;
using Microsoft.Xrm.Portal.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.Configuration
{
	public class DependencyProvider : global::Microsoft.Xrm.Portal.Configuration.DependencyProvider
	{
		public DependencyProvider(string portalName)
			: base(portalName)
		{
		}

		public override T GetDependency<T>(string name)
		{
			if (AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled)
			{
				var dependency = GetContentMapDependency<T>();

				if (dependency != null) return dependency;
			}

			if (typeof(T) == typeof(IEntityWebsiteProvider)) return new AdxEntityWebsiteProvider() as T;
			if (typeof(T) == typeof(IEntityUrlProvider)) return new AdxEntityUrlProvider(new AdxEntityWebsiteProvider(), PortalName) as T;
			if (typeof(T) == typeof(ICmsDataServiceQueryInterceptorProvider)) return new AdxCmsDataServiceQueryInterceptorProvider(PortalName) as T;
			if (typeof(T) == typeof(IRegisterClientSideDependenciesProvider)) return new AdxRegisterClientSideDependenciesProvider() as T;
			if (typeof(T) == typeof(ICmsEntityEditingMetadataProvider)) return new CmsEntityEditingMetadataProvider(PortalName) as T;
			if (typeof(T) == typeof(ICrmEntityEditingMetadataProvider)) return new CrmEntityEditingMetadataProviderAdapter(new CmsEntityEditingMetadataProvider(PortalName), PortalName) as T;
			if (typeof(T) == typeof(ICmsDataServiceProvider)) return new AdxCmsDataServiceProvider(PortalName) as T;
			if (typeof(T) == typeof(ICmsEntityServiceProvider)) return new CmsEntityServiceProvider(PortalName) as T;
			if (typeof(T) == typeof(IRedirectProvider)) return new CompositeRedirectProvider(new RedirectProvider(PortalName), new CanonicalUrlRedirectProvider(), new UrlHistoryRedirectProvider(PortalName)) as T;
			if (typeof(T) == typeof(IPublishingStateTransitionSecurityProvider)) return new PublishingStateTransitionSecurityProvider() as T;
			if (typeof(T) == typeof(ICrmEntityFileAttachmentProvider)) return new NotesFileAttachmentProvider(PortalName) as T;
			if (typeof(T) == typeof(IPortalRouteHandlerProvider)) return new PortalRouteHandlerProvider(PortalName) as T;

			return base.GetDependency<T>(name);
		}

		private T GetContentMapDependency<T>() where T : class
		{
			if (typeof(T) == typeof(IContentMapProvider))
			{
				return GetContentMapProvider(PortalName) as T;
			}

			if (typeof(T) == typeof(IEntityWebsiteProvider))
			{
				var provider = GetContentMapProvider(PortalName);
				return new ContentMapEntityWebsiteProvider(provider) as T;
			}

			if (typeof(T) == typeof(IEntityUrlProvider) || typeof(T) == typeof(IContentMapEntityUrlProvider))
			{
				var provider = GetContentMapProvider(PortalName);
				return new ContentMapEntityUrlProvider(new ContentMapEntityWebsiteProvider(provider), provider, PortalName) as T;
			}

			if (typeof(T) == typeof(IPortalRouteHandlerProvider))
			{
				var provider = GetContentMapProvider(PortalName);
				return new ContentMapPortalRouteHandlerProvider(PortalName, provider) as T;
			}

			return null;
		}

		private static IContentMapProvider GetContentMapProvider(string portalName)
		{
			return AdxstudioCrmConfigurationManager.CreateContentMapProvider(portalName);
		}
	}
}
