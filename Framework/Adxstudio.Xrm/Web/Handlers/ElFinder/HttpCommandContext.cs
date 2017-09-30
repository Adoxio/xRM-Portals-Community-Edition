/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Web.Providers;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	public class HttpCommandContext : ICommandContext
	{
		private readonly string _portalName;
		private readonly RequestContext _requestContext;

		public HttpCommandContext(string portalName, RequestContext requestContext, HttpRequest request, IEnumerable<string> disabledCommands)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			_portalName = portalName;
			_requestContext = requestContext;
			Files = request.Files;
			Parameters = new NameValueCollection(request.Params);
			DisabledCommands = disabledCommands.ToArray();
		}

		public IEnumerable<string> DisabledCommands { get; private set; }

		public HttpFileCollection Files { get; private set; }

		public NameValueCollection Parameters { get; private set; }

		public IDependencyProvider CreateDependencyProvider()
		{
			return PortalCrmConfigurationManager.CreateDependencyProvider(_portalName);
		}

		public IFileSystem CreateFileSystem()
		{
			var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider(_portalName);
			var contentMapUrlProvider = CreateDependencyProvider().GetDependency<IContentMapEntityUrlProvider>();
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(_portalName, _requestContext);

			return contentMapProvider == null || contentMapUrlProvider == null
				? (IFileSystem)new EntityFileSystem(dataAdapterDependencies)
				: new ContentMapFileSystem(contentMapProvider, contentMapUrlProvider, dataAdapterDependencies);
		}

		public IPortalContext CreatePortalContext()
		{
			return PortalCrmConfigurationManager.CreatePortalContext(_portalName, _requestContext);
		}

		public ICrmEntitySecurityProvider CreateSecurityProvider()
		{
			return PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(_portalName);
		}

		public OrganizationServiceContext CreateServiceContext()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(_portalName);
		}
	}
}
