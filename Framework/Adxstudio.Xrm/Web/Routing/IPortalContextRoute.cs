/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Portal;

namespace Adxstudio.Xrm.Web.Routing
{
	public interface IPortalContextRoute
	{
		string GetPortalContextPath(IPortalContext portalContext, string path);

		string GetPortalContextPath(ContentMap contentMap, WebsiteNode website, string path);
	}
}
