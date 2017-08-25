/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.Mvc;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Metadata;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public interface IPortalLiquidContext
	{
		HtmlHelper Html { get; }

		IOrganizationMoneyFormatInfo OrganizationMoneyFormatInfo { get; }

		IPortalViewContext PortalViewContext { get; }

		Random Random { get; }

		UrlHelper UrlHelper { get; }

		ContextLanguageInfo ContextLanguageInfo { get; }

		IOrganizationService PortalOrganizationService { get; }
	}
}
