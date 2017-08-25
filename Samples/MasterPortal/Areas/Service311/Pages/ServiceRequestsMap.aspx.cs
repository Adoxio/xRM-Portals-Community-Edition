/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Site.Pages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Site.Areas.Service311.Pages
{
	public partial class ServiceRequestsMap : PortalPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			CreateServiceRequestTypesList();

			CreateServiceRequestPriorityList();

			CreateServiceRequestStatusList();
		}

		public void CreateServiceRequestTypesList()
		{
			// Create the Service Request Types List Items & Legend

			var serviceRequestTypes = ServiceContext.CreateQuery("adx_servicerequesttype").Where(s => s.GetAttributeValue<OptionSetValue>("statecode") != null && s.GetAttributeValue<OptionSetValue>("statecode").Value == 0 && s.GetAttributeValue<string>("adx_locationfieldname") != null).OrderBy(s => s.GetAttributeValue<string>("adx_name"));

			ServiceRequestTypesLegendList.DataSource = serviceRequestTypes;

			ServiceRequestTypesLegendList.DataBind();

			var typeListItems = serviceRequestTypes.Select(s => new SearchListItem { Id = s.GetAttributeValue<Guid>("adx_servicerequesttypeid").ToString(), Name = s.GetAttributeValue<string>("adx_name") }).ToList();

			var defaultListItems = new List<SearchListItem> { new SearchListItem { Id = Guid.Empty.ToString(), Name = "All" } };

			ServiceRequestTypesList.DataSource = defaultListItems.Union(typeListItems);

			ServiceRequestTypesList.DataBind();
		}

		public void CreateServiceRequestPriorityList()
		{
			// Create the Service Request Priority List Items

			var retrieveOptionSetRequest = new RetrieveOptionSetRequest { Name = "adx_servicerequestpriority" };

			var retrieveOptionSetResponse = (RetrieveOptionSetResponse)ServiceContext.Execute(retrieveOptionSetRequest);

			if (retrieveOptionSetResponse == null)
			{
				throw new ApplicationException("Error retrieving adx_servicerequestpriority OptionSet");
			}
			
			var retrievedOptionSetMetadata = (OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata;

			var priorityOptions = retrievedOptionSetMetadata.Options;

			if (priorityOptions == null)
			{
				throw new ApplicationException("Error retrieving adx_servicerequestpriority OptionSetMetadata");
			}

			var priorityListItems = priorityOptions.Select(o => new SearchListItem { Id = o.Value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture), Name = o.Label.GetLocalizedLabelString() }).ToList();

			var defaultPriorityListItems = new List<SearchListItem> { new SearchListItem { Id = "0", Name = "Any" } };

			ServiceRequestPriorityList.DataSource = defaultPriorityListItems.Union(priorityListItems);

			ServiceRequestPriorityList.DataBind();
		}

		public void CreateServiceRequestStatusList()
		{
			// Create the Service Request Status List Items

			var retrieveOptionSetRequest = new RetrieveOptionSetRequest { Name = "adx_servicestatus" };

			var retrieveOptionSetResponse = (RetrieveOptionSetResponse)ServiceContext.Execute(retrieveOptionSetRequest);

			if (retrieveOptionSetResponse == null)
			{
				throw new ApplicationException("Error retrieving adx_servicestatus OptionSet");
			}

			var retrievedOptionSetMetadata = (OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata;

			var status = retrievedOptionSetMetadata.Options;

			if (status == null)
			{
				throw new ApplicationException("Error retrieving adx_servicestatus OptionSetMetadata");
			}

			var statusListItems = status.Select(o => new SearchListItem { Id = o.Value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture), Name = o.Label.GetLocalizedLabelString() }).ToList();

			var defaultStatusListItems = new List<SearchListItem> { new SearchListItem { Id = "0", Name = "Any" } };

			ServiceRequestStatusList.DataSource = defaultStatusListItems.Union(statusListItems);

			ServiceRequestStatusList.DataBind();
		}

		public class SearchListItem
		{
			public string Id { get; set; }
			public string Name { get; set; }
		}
	}
}
