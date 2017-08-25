/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Messages;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Site.Pages;

namespace Site.Areas.Service.Pages
{
	public partial class ScheduleService : PortalPage
	{
		private readonly Lazy<Entity> _service;

		protected Entity Service
		{
			get { return _service.Value; }
		}

		public ScheduleService()
		{
			_service = new Lazy<Entity>(() => XrmContext.CreateQuery("service").First(s => s.GetAttributeValue<Guid>("serviceid") == new Guid(ServiceType.SelectedValue)));
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			if (IsPostBack)
			{
				return;
			}

			StartDate.SelectedDate = DateTime.Today;
			EndDate.SelectedDate = DateTime.Today.AddDays(7);

			var services = XrmContext.CreateQuery("service").Where(s => s.GetAttributeValue<bool?>("isschedulable").GetValueOrDefault(false) && s.GetAttributeValue<string>("description").Contains("*WEB*")).ToList();

			if (!services.Any())
			{
				SearchPanel.Visible = false;
				NoServicesMessage.Visible = true;

				return;
			}

			BindServicesDropDown(services);
			BindTimeZoneDropDown();
			BindTimeDropDowns();
		}

		protected void AvailableTimes_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType != DataControlRowType.DataRow) return;

			e.Row.Attributes.Add("onclick", Page.ClientScript.GetPostBackClientHyperlink(AvailableTimes, "Select$" + e.Row.RowIndex));
			e.Row.Attributes.Add("onmouseover", string.Format("this.style.cursor='pointer'"));
		}

		protected void AvailableTimes_SelectedIndexChanged(object sender, EventArgs e)
		{
			ScheduleServiceButton.Enabled = true;
		}

		protected void FindTimes_Click(object sender, EventArgs args)
		{
			var startTimeInMinutesFromMidnight = int.Parse(StartTime.SelectedValue);

			var startDate = StartDate.SelectedDate.AddMinutes(startTimeInMinutesFromMidnight);

			var endTimeInMinutesFromMidnight = int.Parse(EndTime.SelectedValue);

			var endDate = EndDate.SelectedDate.AddMinutes(endTimeInMinutesFromMidnight);

			if (!SelectedDatesAndTimesAreValid(startDate, endDate, startTimeInMinutesFromMidnight, endTimeInMinutesFromMidnight))
			{
				return;
			}

			// Add the timezone selected to the CRM Contact for next time.
			var contact = XrmContext.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == Contact.Id);

			if (contact == null)
			{
				throw new ApplicationException(string.Format("Couldn't find the user contact where contactid equals {0}.", Contact.Id));
			}

			contact.SetAttributeValue("adx_timezone", int.Parse(TimeZoneSelection.SelectedValue));

			XrmContext.UpdateObject(contact);

			XrmContext.SaveChanges();

			var usersMinutesFromGmt = GetUsersMinutesFromGmt(contact.GetAttributeValue<int?>("adx_timezone"), XrmContext);

			var appointmentRequest = new AppointmentRequest
			{
				AnchorOffset = Service.GetAttributeValue<int?>("anchoroffset").GetValueOrDefault(),
				Direction = SearchDirection.Forward,
				Duration = Service.GetAttributeValue<int?>("duration").GetValueOrDefault(60),
				NumberOfResults = 10,
				RecurrenceDuration = endTimeInMinutesFromMidnight - startTimeInMinutesFromMidnight,
				RecurrenceTimeZoneCode = contact.GetAttributeValue<int?>("adx_timezone").GetValueOrDefault(),
				SearchRecurrenceRule = "FREQ=DAILY;INTERVAL=1",
				SearchRecurrenceStart = new DateTime(startDate.AddMinutes(usersMinutesFromGmt * -1).Ticks, DateTimeKind.Utc),
				SearchWindowEnd = new DateTime(endDate.AddMinutes(usersMinutesFromGmt * -1).Ticks, DateTimeKind.Utc),
				ServiceId = Service.GetAttributeValue<Guid>("serviceid")
			};

			var service = XrmContext;

			var searchRequest = new OrganizationRequest("Search");
			searchRequest.Parameters["AppointmentRequest"] = appointmentRequest;

			var searchResults = (SearchResults)service.Execute(searchRequest).Results["SearchResults"];

			var schedules = searchResults.Proposals.Select(proposal => new
			{
				ScheduledStart = proposal.Start.GetValueOrDefault().ToUniversalTime().AddMinutes(usersMinutesFromGmt),
				ScheduledStartUniversalTime = proposal.Start.GetValueOrDefault().ToUniversalTime(),
				ScheduledEnd = proposal.End.GetValueOrDefault().ToUniversalTime().AddMinutes(usersMinutesFromGmt),
				ScheduledEndUniversalTime = proposal.End.GetValueOrDefault().ToUniversalTime(),
				AvailableResource = proposal.ProposalParties.First().ResourceId
			}).Where(proposal => proposal.ScheduledStartUniversalTime >= DateTime.UtcNow);

			if (!schedules.Any())
			{
				SearchPanel.Visible = true;
				NoTimesMessage.Visible = true;
				ResultsDisplay.Visible = false;

				return;
			}

			AvailableTimes.DataSource = schedules;
			AvailableTimes.DataBind();

			SearchPanel.Visible = false;
			ResultsDisplay.Visible = true;
			ScheduleServiceButton.Enabled = false;
		}

		protected void ScheduleService_Click(object sender, EventArgs e)
		{
			var availableResourceId = (Guid)AvailableTimes.SelectedDataKey.Values["AvailableResource"];
			var availableResource = XrmContext.CreateQuery("resource").First(r => r.GetAttributeValue<Guid>("resourceid") == availableResourceId);
			var selectedStart = (DateTime)AvailableTimes.SelectedDataKey.Values["ScheduledStartUniversalTime"];
			var selectedEnd = (DateTime)AvailableTimes.SelectedDataKey.Values["ScheduledEndUniversalTime"];

			var appointment = new Entity("serviceappointment");
			appointment.SetAttributeValue("serviceid", Service.ToEntityReference());
			appointment.SetAttributeValue("subject", "Web Service Scheduler: " + ServiceType.SelectedItem);
			appointment.SetAttributeValue("scheduledstart", selectedStart);
			appointment.SetAttributeValue("scheduledend", selectedEnd);
			
			var resourcesActivityParty = new Entity("activityparty");
			resourcesActivityParty["partyid"] = new EntityReference(availableResource.GetAttributeValue<string>("objecttypecode"), availableResource.Id);
			var resources = new EntityCollection(new List<Entity> { resourcesActivityParty });
			appointment.SetAttributeValue("resources", resources);

			var customersActivityParty = new Entity("activityparty");
			customersActivityParty["partyid"] = Contact.ToEntityReference();
			var customers = new EntityCollection(new List<Entity> { customersActivityParty });
			appointment.SetAttributeValue("customers", customers);

			XrmContext.AddObject(appointment);
			XrmContext.SaveChanges();
			XrmContext.SetState((int)ServiceAppointmentState.Scheduled, 4, appointment);

			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Service Details");

			Response.Redirect(string.Format("{0}?serviceid={1}", ServiceContext.GetUrl(page), appointment.Id));
		}

		private void BindServicesDropDown(IEnumerable<Entity> services)
		{
			ServiceType.DataSource = services.Select(s => new ListItem(s.GetAttributeValue<string>("name"), s.GetAttributeValue<Guid>("serviceid").ToString()));
			ServiceType.DataTextField = "Text";
			ServiceType.DataValueField = "Value";
			ServiceType.DataBind();
		}

		private void BindTimeDropDowns()
		{
			for (var t = DateTime.MinValue; t < DateTime.MinValue.AddDays(1); t = t.AddMinutes(30))
			{
				StartTime.Items.Add(new ListItem(t.ToString("h:mm tt"), t.Subtract(DateTime.MinValue).TotalMinutes.ToString(CultureInfo.InvariantCulture)));
				EndTime.Items.Add(new ListItem(t.ToString("h:mm tt"), t.Subtract(DateTime.MinValue).TotalMinutes.ToString(CultureInfo.InvariantCulture)));
			}

			StartTime.Text = "540"; // 9 AM
			EndTime.Text = "1020"; // 5 PM
		}

		private void BindTimeZoneDropDown()
		{
			TimeZoneSelection.DataSource = XrmContext.CreateQuery("timezonedefinition").OrderByDescending(t => t.GetAttributeValue<string>("userinterfacename")).Select(t => new ListItem(t.GetAttributeValue<string>("userinterfacename"), t.GetAttributeValue<int>("timezonecode").ToString(CultureInfo.InvariantCulture)));
			TimeZoneSelection.DataTextField = "Text";
			TimeZoneSelection.DataValueField = "Value";
			TimeZoneSelection.DataBind();

			TimeZoneSelection.Items.Insert(0, new ListItem("Please Select One..."));

			if (Contact.GetAttributeValue<int?>("adx_timezone").HasValue)
			{
				TimeZoneSelection.SelectedValue = Contact.GetAttributeValue<int?>("adx_timezone").ToString();
			}
		}

		private static int GetUsersMinutesFromGmt(int? timeZoneCode, OrganizationServiceContext crmContext)
		{
			var definition = crmContext.CreateQuery("timezonedefinition").First(timeZone => timeZone.GetAttributeValue<int>("timezonecode") == timeZoneCode);

			if (definition == null)
			{
				return 0;
			}

			var rule = definition.GetRelatedEntities(crmContext, new Relationship("lk_timezonerule_timezonedefinitionid")).ToList();

			return !rule.Any() ? 0 : rule.First().GetAttributeValue<int?>("bias").GetValueOrDefault(0) * -1;
		}

		private bool SelectedDatesAndTimesAreValid(DateTime startDate, DateTime endDate, int startTimeInMinutesFromMidnight, int endTimeInMinutesFromMidnight)
		{
			if (startDate.Date < DateTime.Now.Date)
			{
				ErrorLabel.Text = ResourceManager.GetString("Select_Date_Range_That_Is_Not_Past");
				ErrorLabel.Visible = true;
				return false;
			}

			if (startDate > endDate)
			{
				ErrorLabel.Text = ResourceManager.GetString("Select_End_Date_That_Is_After_Start_Date");
				ErrorLabel.Visible = true;
				return false;
			}

			if (TimeZoneSelection.SelectedIndex < 1)
			{
				ErrorLabel.Text = ResourceManager.GetString("Select_Your_Time_Zone");
				ErrorLabel.Visible = true;
				return false;
			}

			if (startTimeInMinutesFromMidnight >= endTimeInMinutesFromMidnight)
			{
				ErrorLabel.Text = ResourceManager.GetString("Select_End_Time_That_Is_Later_Than_Start_Time");
				ErrorLabel.Visible = true;
				return false;
			}

			// Start date and end dates are acceptable. Hide error message.
			ErrorLabel.Visible = false;
			return true;
		}
	}
}
