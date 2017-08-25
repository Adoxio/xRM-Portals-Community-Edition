/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Specialized;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Data.Services;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Portal.Web.UI;
using Microsoft.Xrm.Portal.Web.UI.WebControls;

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// A set of portal dependency classes.
	/// </summary>
	public class DependencyProvider : IDependencyProvider, IInitializable // MSBug #119983: Won't seal, this is an expected extension point.
	{
		/// <summary>
		/// The portal name used by dependencies that require a portal.
		/// </summary>
		public string PortalName { get; private set; }

		public DependencyProvider(string portalName)
		{
			PortalName = portalName;
		}

		/// <summary>
		/// Retrieves a dependency by type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual T GetDependency<T>() where T : class
		{
			return GetDependency<T>(null);
		}

		/// <summary>
		/// Retrieves a dependency by type and name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual T GetDependency<T>(string name) where T : class
		{
			if (typeof(T) == typeof(IEntityWebsiteProvider)) return new EntityWebsiteProvider() as T;
			if (typeof(T) == typeof(IEntityUrlProvider)) return new EntityUrlProvider(new EntityWebsiteProvider()) as T;
			if (typeof(T) == typeof(ICmsDataServiceQueryInterceptorProvider)) return new CmsDataServiceQueryInterceptorProvider(PortalName) as T;
			if (typeof(T) == typeof(IRegisterClientSideDependenciesProvider)) return new RegisterClientSideDependenciesProvider() as T;
			if (typeof(T) == typeof(ICrmEntityEditingMetadataProvider)) return new CmsDataServiceCrmEntityEditingMetadataProvider() as T;
			if (typeof(T) == typeof(ICmsDataServiceProvider)) return new CmsDataServiceProvider(PortalName) as T;
			if (typeof(T) == typeof(ICrmEntityContentFormatter)) return new PassthroughCrmEntityContentFormatter() as T;
			if (typeof(T) == typeof(ICrmEntityFileAttachmentProvider)) return new NotesFileAttachmentProvider(PortalName) as T;

			if (typeof(T) == typeof(INodeValidatorProvider))
			{
				if (name == "node") return new NodeValidatorProvider(PortalName) as T;
				if (name == "securityTrimming") return new SecurityTrimmingValidatorProvider(PortalName) as T;
				if (name == "childNode") return new ChildNodeValidatorProvider(PortalName) as T;
			}

			return null;
		}

		public virtual void Initialize(string name, NameValueCollection config)
		{
		}
	}
}
