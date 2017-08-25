/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Service311.Pages
{
	public partial class ServiceRequests : PortalPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var serviceRequestTypes = ServiceContext.CreateQuery("adx_servicerequesttype").Where(s => s.GetAttributeValue<OptionSetValue>("statecode") != null && s.GetAttributeValue<OptionSetValue>("statecode").Value == 0).OrderBy(s => s.GetAttributeValue<string>("adx_name"));

			ServiceRequestTypesListView.DataSource = serviceRequestTypes;

			ServiceRequestTypesListView.DataBind();
		}
	}
}
