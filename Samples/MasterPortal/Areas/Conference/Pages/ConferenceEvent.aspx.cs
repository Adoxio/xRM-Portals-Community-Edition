/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Threading;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;
using Adxstudio.Xrm.Conferences;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.Conference.Pages
{
	public partial class ConferenceEvent : ConferencePage
	{
		protected const string ConferenceIdQueryStringParameterName = "conferenceid";

		private readonly Lazy<IPortalContext> _portal = new Lazy<IPortalContext>(() => PortalCrmConfigurationManager.CreatePortalContext(), LazyThreadSafetyMode.None);

		protected bool CanRegister { get; private set; }

		protected bool IsRegistered { get; private set; }

		protected IEventOccurrence RequestEventOccurrence { get; private set; }

		protected bool RequiresRegistration { get; private set; }

		protected void Page_Load(object sender, EventArgs args)
		{
			var @event = _portal.Value.Entity;

			if (@event == null || @event.LogicalName != "adx_event")
			{
				return;
			}

			var dataAdapter = new EventDataAdapter(@event, new PortalContextDataAdapterDependencies(_portal.Value, PortalName));
			var now = DateTime.UtcNow;
			
			var occurrences = (PortalConference != null) ? dataAdapter.SelectEventOccurrences(PortalConference.GetAttributeValue<DateTime?>("adx_startingdate").GetValueOrDefault(now.AddMonths(-3)), PortalConference.GetAttributeValue<DateTime?>("adx_enddate").GetValueOrDefault(now.AddMonths(3))).ToArray() : 
				dataAdapter.SelectEventOccurrences(now.AddMonths(-3), now.AddMonths(3)).ToArray();

			IEventOccurrence requestOccurrence;

			RequestEventOccurrence = dataAdapter.TryMatchRequestEventOccurrence(Request, occurrences, out requestOccurrence)
				? requestOccurrence
				: occurrences.Length == 1 ? occurrences.Single() : null;

			var user = _portal.Value.User;

			CanRegister = Request.IsAuthenticated && user != null && RequestEventOccurrence != null && RequestEventOccurrence.Start >= now &&
				(@event.GetAttributeValue<EntityReference>("adx_conferenceid") != null && UserIsRegisteredForConference
				|| @event.GetAttributeValue<EntityReference>("adx_conferenceid") == null);

			RequiresRegistration = (@event.GetAttributeValue<bool?>("adx_requiresregistration").GetValueOrDefault()
				|| @event.GetAttributeValue<EntityReference>("adx_conferenceid") != null && UserIsRegisteredForConference)
				&& RequestEventOccurrence != null
				&& RequestEventOccurrence.Start >= now;

			if (CanRegister)
			{
				var registration = _portal.Value.ServiceContext.CreateQuery("adx_eventregistration")
					.FirstOrDefault(e => e.GetAttributeValue<EntityReference>("adx_attendeeid") == user.ToEntityReference()
						&& e.GetAttributeValue<EntityReference>("adx_eventscheduleid") == RequestEventOccurrence.EventSchedule.ToEntityReference());

				if (registration != null)
				{
					Unregister.CommandArgument = registration.Id.ToString();
					IsRegistered = true;
				}
			}

			OtherOccurrences.DataSource = occurrences
				.Where(e => e.Start >= now)
				.Where(e => RequestEventOccurrence == null || !(e.EventSchedule.Id == RequestEventOccurrence.EventSchedule.Id && e.Start == RequestEventOccurrence.Start));

			OtherOccurrences.DataBind();

			var sessionEvent = @event;

			Speakers.DataSource = sessionEvent.GetRelatedEntities(_portal.Value.ServiceContext, new Relationship("adx_eventspeaker_event"));
			Speakers.DataBind();
		}

		protected void Register_Click(object sender, EventArgs args)
		{
			var user = _portal.Value.User;

			if (!Request.IsAuthenticated || user == null || RequestEventOccurrence == null)
			{
				return;
			}

			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			serviceContext.CreateEventRegistration(user.ToEntityReference(), RequestEventOccurrence.Event.ToEntityReference(),
				RequestEventOccurrence.EventSchedule.ToEntityReference(), DateTime.UtcNow);

			Response.Redirect(Request.Url.PathAndQuery);
		}

		protected void Unregister_Click(object sender, CommandEventArgs args)
		{
			var user = _portal.Value.User;

			if (!Request.IsAuthenticated || user == null || RequestEventOccurrence == null)
			{
				return;
			}

			Guid registrationId;

			if (!Guid.TryParse(args.CommandArgument.ToString(), out registrationId))
			{
				return;
			}

			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var registration = serviceContext.CreateQuery("adx_eventregistration")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_eventregistrationid") == registrationId
					&& e.GetAttributeValue<EntityReference>("adx_attendeeid") == user.ToEntityReference());

			if (registration != null)
			{
				serviceContext.DeleteObject(registration);
				serviceContext.SaveChanges();
			}

			Response.Redirect(Request.Url.PathAndQuery);
		}

		protected void RegisterForConference_Click(object sender, EventArgs args)
		{
			if (PortalConference == null)
			{
				return;
			}

			var registrationUrl = GetRegistrationUrl(PortalConference.Id);

			Response.Redirect(registrationUrl);
		}

		protected string GetRegistrationUrl(Guid conferenceId)
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Conference Registration");

			if (page == null)
			{
				throw new ApplicationException(string.Format("A page couldn't be found for the site marker named {0}.", "Conference Registration"));
			}

			var url = ServiceContext.GetUrl(page);

			if (string.IsNullOrWhiteSpace(url))
			{
				throw new ApplicationException(string.Format("A URL couldn't be determined for the site marker named {0}.", "Conference Registration"));
			}

			var urlBuilder = new UrlBuilder(url);

			urlBuilder.QueryString.Add(ConferenceIdQueryStringParameterName, conferenceId.ToString());

			urlBuilder.QueryString.Add("ReturnURL", Request.Url.PathAndQuery);

			return urlBuilder.PathWithQueryString;
		}

		protected void Speakers_OnItemDataBound(object sender, ListViewItemEventArgs e)
		{
			var dataItem = e.Item as ListViewDataItem;

			if (dataItem == null || dataItem.DataItem == null)
			{
				return;
			}

			var speaker = dataItem.DataItem as Entity;

			if (speaker == null)
			{
				return;
			}

			var repeaterControl = (Repeater)e.Item.FindControl("SpeakerAnnotations");

			if (repeaterControl == null)
			{
				return;
			}
			
			var dataAdapterDependencies =
				new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: PortalName);
			var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);

			var annotations = XrmContext.CreateQuery("annotation")
				.Where(a => a.GetAttributeValue<EntityReference>("objectid") == speaker.ToEntityReference() &&
					a.GetAttributeValue<bool?>("isdocument").GetValueOrDefault(false))
				.OrderBy(a => a.GetAttributeValue<DateTime>("createdon"))
				.Select(entity => dataAdapter.GetAnnotation(entity));

			repeaterControl.DataSource = annotations;
			repeaterControl.DataBind();
		}
	}
}
