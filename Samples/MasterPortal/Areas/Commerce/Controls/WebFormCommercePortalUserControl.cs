/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Commerce;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Site.Controls;

namespace Site.Areas.Commerce.Controls
{
	public class WebFormCommercePortalUserControl : WebFormPortalUserControl
	{
		private readonly Lazy<OrganizationServiceContext> _xrmContext;

		public WebFormCommercePortalUserControl()
		{
			_xrmContext = new Lazy<OrganizationServiceContext>(CreateXrmServiceContext);
		}

		private OrganizationServiceContext CreateXrmServiceContext()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}

		protected IPurchaseDataAdapter CreatePurchaseDataAdapter(EntityReference target, string targetPrimaryKeyLogicalName)
		{
			var dependencies = new PortalConfigurationDataAdapterDependencies(PortalName, Request.RequestContext);

			return new WebFormPurchaseDataAdapter(
				target,
				targetPrimaryKeyLogicalName,
				new EntityReference("adx_webform", WebForm.CurrentSessionHistory.WebFormId),
				WebFormMetadata,
				WebForm.CurrentSessionHistory,
				dependencies);
		}

	}
}
