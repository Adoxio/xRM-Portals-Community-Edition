/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Web.Providers
{
	public interface IContentMapEntityUrlProvider
	{
		string GetUrl(ContentMap map, EntityNode node);
		ApplicationPath GetApplicationPath(ContentMap map, EntityNode node);
	}
}
