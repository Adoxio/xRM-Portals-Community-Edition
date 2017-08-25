/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Site.Pages;

namespace Site.Areas.Service.Pages
{
	public partial class ViewScheduledServices : PortalPage
	{
		private const int AppointmentStatusScheduled = 3;

		private const string AppointmentFetchXmlFormat = @"
			<fetch mapping=""logical"" distinct=""true"">
				<entity name=""serviceappointment"">
					<attribute name=""serviceid"" />
					<attribute name=""scheduledstart"" />
					<attribute name=""scheduledend"" />
					<attribute name=""activityid"" />
					<attribute name=""createdon"" />
					<attribute name=""activitytypecode"" />
					<order attribute=""scheduledstart"" descending=""false"" />
					<filter type=""and"">
						<condition attribute=""statecode"" operator=""eq"" value=""{0}"" />
						<condition attribute=""scheduledstart"" operator=""on-or-after"" value=""{1:yyyy-MM-dd}"" />
					</filter>
					<link-entity name=""activityparty"" from=""activityid"" to=""activityid"" alias=""ab"">
						<filter type=""and"">
							<condition attribute=""partyid"" operator=""eq"" value=""{2}"" />
						</filter>
					</link-entity>
				</entity>
			</fetch>";

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			if (Page.IsPostBack) return;

			if (Contact == null) return;

			if (Contact.GetAttributeValue<int?>("adx_timezone") == null) return;

			var appointmentFetchXml = string.Format(AppointmentFetchXmlFormat, AppointmentStatusScheduled, DateTime.UtcNow, Contact.GetAttributeValue<Guid>("contactid"));

			var response = (RetrieveMultipleResponse)ServiceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(appointmentFetchXml)
			});

			if (response == null || response.EntityCollection == null || response.EntityCollection.Entities == null)
			{
				return;
			}

			var usersMinutesFromGmt = GetUsersMinutesFromGmt(Contact.GetAttributeValue<int?>("adx_timezone").GetValueOrDefault(), ServiceContext);

			var appointments = response.EntityCollection.Entities.Select(a => new
				{
					scheduledStart = a.GetAttributeValue<DateTime?>("scheduledstart").GetValueOrDefault().ToUniversalTime().AddMinutes(usersMinutesFromGmt),
					scheduledEnd = a.GetAttributeValue<DateTime?>("scheduledend").GetValueOrDefault().ToUniversalTime().AddMinutes(usersMinutesFromGmt),
					serviceType = a.GetAttributeValue<EntityReference>("serviceid") == null ? string.Empty : a.GetAttributeValue<EntityReference>("serviceid").Name,
					dateBooked = a.GetAttributeValue<DateTime>("createdon").ToUniversalTime().AddMinutes(usersMinutesFromGmt),
					serviceId = a.GetAttributeValue<Guid>("activityid")
				});

			BookedAppointments.DataSource = appointments;
			BookedAppointments.DataBind();
		}

		private static int GetUsersMinutesFromGmt(int? timeZoneCode, OrganizationServiceContext crmContext)
		{
			var definition = crmContext.CreateQuery("timezonedefinition").FirstOrDefault(timeZone => timeZone.GetAttributeValue<int>("timezonecode") == timeZoneCode);

			if (definition == null)
			{
				return 0;
			}

			var rule = definition.GetRelatedEntities(crmContext, new Relationship("lk_timezonerule_timezonedefinitionid")).ToList();

			return !rule.Any() ? 0 : rule.First().GetAttributeValue<int?>("bias").GetValueOrDefault(0) * -1;
		}

		protected void BookedAppointments_OnRowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandArgument == null)
			{
				return;
			}

			if (string.Equals(e.CommandName, "Cancel", StringComparison.InvariantCulture))
			{
				var serviceId = new Guid(e.CommandArgument.ToString());
				CancelService(serviceId);
			}
		}

		protected void CancelService(Guid activityID)
		{
			var appointment = XrmContext.CreateQuery("serviceappointment").FirstOrDefault(a => a.GetAttributeValue<Guid>("activityid") == activityID);

			if (appointment == null)
			{
				return;
			}

			XrmContext.SetState((int)ServiceAppointmentState.Canceled, -1, appointment);

			Response.Redirect(Request.RawUrl);
		}
	}
}
