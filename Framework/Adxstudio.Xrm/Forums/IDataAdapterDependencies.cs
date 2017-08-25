/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Web.Routing;
using Adxstudio.Xrm.Collections.Generic;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public interface IDataAdapterDependencies : Cms.IDataAdapterDependencies
	{
		IForumCounterStrategy GetCounterStrategy();

		ILatestPostUrlProvider GetLatestPostUrlProvider();
	}
}
