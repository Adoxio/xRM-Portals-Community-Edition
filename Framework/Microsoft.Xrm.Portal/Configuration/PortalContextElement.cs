/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using System.Web.Routing;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Cms.WebsiteSelectors;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// The modes in which the <see cref="PortalCrmConfigurationManager"/> instantiates <see cref="IPortalContext"/> objects.
	/// </summary>
	public enum PortalContextInstanceMode
	{
		/// <summary>
		/// Create an instance for each web request.
		/// </summary>
		PerRequest,

		/// <summary>
		/// Create an instance on every invocation.
		/// </summary>
		PerInstance,
	}

	/// <summary>
	/// The configuration settings for <see cref="IPortalContext"/> dependencies.
	/// </summary>
	/// <remarks>
	/// For an example of the configuration format refer to the <see cref="PortalCrmConfigurationManager"/>.
	/// </remarks>
	/// <seealso cref="PortalCrmConfigurationManager"/>
	public sealed class PortalContextElement : InitializableConfigurationElement<IPortalContext>
	{
		/// <summary>
		/// Default virtual path prefix of embedded virtual files.
		/// </summary>
		public const string DefaultXrmFilesBaseUri = "~/xrm";
		private const string _defaultPortalContextElementTypeName = "Microsoft.Xrm.Portal.PortalContext, Microsoft.Xrm.Portal";
		private const string _defaultCmsServiceBaseUri = "~/Services/Cms.svc";

		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propName;
		private static readonly ConfigurationProperty _propType;
		private static readonly ConfigurationProperty _propContextName;
		private static readonly ConfigurationProperty _propCmsServiceBaseUri;
		private static readonly ConfigurationProperty _propInstanceMode;

		private static readonly ConfigurationProperty _propWebsiteSelector;
		private static readonly ConfigurationProperty _propCrmEntitySecurityProvider;
		private static readonly ConfigurationProperty _propDependencyProvider;

		static PortalContextElement()
		{
			_propName = new ConfigurationProperty("name", typeof(string), _defaultPortalContextElementTypeName, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			_propType = new ConfigurationProperty("type", typeof(string), _defaultPortalContextElementTypeName, ConfigurationPropertyOptions.None);
			_propContextName = new ConfigurationProperty("contextName", typeof(string), null, ConfigurationPropertyOptions.None);
			_propCmsServiceBaseUri = new ConfigurationProperty("cmsServiceBaseUri", typeof(string), _defaultCmsServiceBaseUri, ConfigurationPropertyOptions.None);
			_propInstanceMode = new ConfigurationProperty("instanceMode", typeof(PortalContextInstanceMode), PortalContextInstanceMode.PerRequest, ConfigurationPropertyOptions.None);

			_propWebsiteSelector = new ConfigurationProperty("websiteSelector", typeof(WebsiteSelectorElement), new WebsiteSelectorElement(), ConfigurationPropertyOptions.None);
			_propCrmEntitySecurityProvider = new ConfigurationProperty("crmEntitySecurityProvider", typeof(CrmEntitySecurityProviderElement), new CrmEntitySecurityProviderElement(), ConfigurationPropertyOptions.None);
			_propDependencyProvider = new ConfigurationProperty("dependencyProvider", typeof(DependencyProviderElement), new DependencyProviderElement(), ConfigurationPropertyOptions.None);

			_properties = new ConfigurationPropertyCollection
			{
				_propName,
				_propType,
				_propContextName,
				_propCmsServiceBaseUri,
				_propInstanceMode,
				_propWebsiteSelector,
				_propCrmEntitySecurityProvider,
				_propDependencyProvider, 
			};
		}

		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		/// <summary>
		/// Gets or sets the element name.
		/// </summary>
		[ConfigurationProperty("name", DefaultValue = _defaultPortalContextElementTypeName, IsKey = true, IsRequired = true)]
		public override string Name
		{
			get { return (string)base[_propName]; }
			set { base[_propName] = value; }
		}

		/// <summary>
		/// The dependency type name.
		/// </summary>
		[ConfigurationProperty("type", DefaultValue = _defaultPortalContextElementTypeName)]
		public override string Type
		{
			get { return (string)base[_propType]; }
			set { base[_propType] = value; }
		}

		/// <summary>
		/// The name of the nested <see cref="OrganizationServiceContext"/> configuration.
		/// </summary>
		[ConfigurationProperty("contextName", DefaultValue = null)]
		public string ContextName
		{
			get { return (string)base[_propContextName]; }
			set { base[_propContextName] = value; }
		}

		/// <summary>
		/// The path of the OData service endpoint.
		/// </summary>
		[ConfigurationProperty("cmsServiceBaseUri", DefaultValue = _defaultCmsServiceBaseUri)]
		public string CmsServiceBaseUri
		{
			get { return (string)base[_propCmsServiceBaseUri]; }
			set { base[_propCmsServiceBaseUri] = value; }
		}

		/// <summary>
		/// The instance mode.
		/// </summary>
		[ConfigurationProperty("instanceMode", DefaultValue = PortalContextInstanceMode.PerRequest)]
		public PortalContextInstanceMode InstanceMode
		{
			get { return (PortalContextInstanceMode)base[_propInstanceMode]; }
			set { base[_propInstanceMode] = value; }
		}

		/// <summary>
		/// The configuration of the <see cref="IWebsiteSelector"/>. provider.
		/// </summary>
		[ConfigurationProperty("websiteSelector")]
		public WebsiteSelectorElement WebsiteSelector
		{
			get { return (WebsiteSelectorElement)base[_propWebsiteSelector]; }
			set { base[_propWebsiteSelector] = value; }
		}

		/// <summary>
		/// The configuration of the <see cref="ICrmEntitySecurityProvider"/>.
		/// </summary>
		[ConfigurationProperty("crmEntitySecurityProvider")]
		public CrmEntitySecurityProviderElement CrmEntitySecurityProvider
		{
			get { return (CrmEntitySecurityProviderElement)base[_propCrmEntitySecurityProvider]; }
			set { base[_propCrmEntitySecurityProvider] = value; }
		}

		/// <summary>
		/// The configuration of the <see cref="IDependencyProvider"/>.
		/// </summary>
		[ConfigurationProperty("dependencyProvider")]
		public DependencyProviderElement DependencyProvider
		{
			get { return (DependencyProviderElement)base[_propDependencyProvider]; }
			set { base[_propDependencyProvider] = value; }
		}

		/// <summary>
		/// Creates a <see cref="IPortalContext"/> object.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public IPortalContext CreatePortalContext(RequestContext request = null)
		{
			return CreateDependencyAndInitialize(
				() => new PortalContext(ContextName, request),
				ContextName, request);
		}
	}
}
