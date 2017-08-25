/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Routing;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Cms.WebsiteSelectors
{
	/// <summary>
	/// Provides the ability to select the current website <see cref="Entity"/>.
	/// </summary>
	public interface IWebsiteSelector
	{
		/// <summary>
		/// Selects the website entity.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		Entity GetWebsite(OrganizationServiceContext context, RequestContext request);
	}
}
