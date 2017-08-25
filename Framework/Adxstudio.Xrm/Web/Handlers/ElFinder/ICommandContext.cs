/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	public interface ICommandContext
	{
		IEnumerable<string> DisabledCommands { get; }

		HttpFileCollection Files { get; }

		NameValueCollection Parameters { get; }

		IDependencyProvider CreateDependencyProvider();

		IFileSystem CreateFileSystem();

		IPortalContext CreatePortalContext();

		ICrmEntitySecurityProvider CreateSecurityProvider();

		OrganizationServiceContext CreateServiceContext();
	}
}
