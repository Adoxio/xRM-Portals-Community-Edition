/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Site.Pages;

namespace Site.Areas.Service.Pages
{
	public partial class ServiceDetails : PortalPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(Request["serviceid"]))
			{
				var page = ServiceContext.GetPageBySiteMarkerName(Website, "View Scheduled Services");

				Response.Redirect(ServiceContext.GetUrl(page));
			}

			var scheduledActivity = XrmContext.CreateQuery("serviceappointment").First(s => s.GetAttributeValue<Guid>("activityid") == new Guid(Request["serviceid"]));
			var userTimeZone = Contact.GetAttributeValue<int?>("adx_timezone").GetValueOrDefault();
			var timeZone = XrmContext.CreateQuery("timezonedefinition").First(t => t.GetAttributeValue<int>("timezonecode") == userTimeZone);
			var usersMinutesFromGmt = GetUsersMinutesFromGmt(userTimeZone, XrmContext);

			serviceType.Text = scheduledActivity.GetRelatedEntity(XrmContext, new Relationship("service_service_appointments")).GetAttributeValue<string>("name");
			startTime.Text = string.Format("{0} ({1})", scheduledActivity.GetAttributeValue<DateTime?>("scheduledstart").GetValueOrDefault().AddMinutes(usersMinutesFromGmt), timeZone.GetAttributeValue<string>("standardname"));
			endTime.Text = string.Format("{0} ({1})", scheduledActivity.GetAttributeValue<DateTime?>("scheduledend").GetValueOrDefault().AddMinutes(usersMinutesFromGmt), timeZone.GetAttributeValue<string>("standardname"));
		}

		private static int GetUsersMinutesFromGmt(int? timeZoneCode, OrganizationServiceContext crmContext)
		{
			var definition = crmContext.CreateQuery("timezonedefinition").FirstOrDefault(timeZone => timeZone.GetAttributeValue<int>("timezonecode") == timeZoneCode);

			if (definition == null)
			{
				return 0;
			}

			var rule = definition.GetRelatedEntities(crmContext, new Relationship("lk_timezonerule_timezonedefinitionid"));

			return rule == null ? 0 : rule.First().GetAttributeValue<int?>("bias").GetValueOrDefault() * -1;
		}
	}
}
