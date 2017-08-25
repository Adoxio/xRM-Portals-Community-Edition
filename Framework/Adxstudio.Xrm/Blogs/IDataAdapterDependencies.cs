/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.Routing;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Blogs
{
	public interface IDataAdapterDependencies : Adxstudio.Xrm.Cms.IDataAdapterDependencies
	{
		ApplicationPath GetBlogAggregationFeedPath();

		ApplicationPath GetBlogFeedPath(Guid blogId);
	}
}
