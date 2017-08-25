/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.UI.WebControls;
using Adxstudio.Xrm;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Events.Pages
{
	public partial class Event : PortalPage
	{
		protected const string EventRegistrationIdQueryStringParameterName = "id";

		private readonly Lazy<IPortalContext> _portal = new Lazy<IPortalContext>(() => PortalCrmConfigurationManager.CreatePortalContext(), LazyThreadSafetyMode.None);

		protected bool CanRegister { get; private set; }

		protected bool IsRegistered { get; private set; }

		protected IEventOccurrence RequestEventOccurrence { get; private set; }

		protected bool RequiresRegistration { get; private set; }

		protected bool RegistrationRequiresPayment { get; private set; }

		protected enum EventStatusCode
		{
			Started = 1,
			Completed = 756150000
		}

		protected void Page_Load(object sender, EventArgs args)
		{
			var @event = _portal.Value.Entity;

			if (@event == null || @event.LogicalName != "adx_event")
			{
				return;
			}

			var dataAdapter = new EventDataAdapter(@event, new PortalContextDataAdapterDependencies(_portal.Value, PortalName));
			var now = DateTime.UtcNow;

			var past = Html.TimeSpanSetting("Events/DisplayTimeSpan/Past").GetValueOrDefault(TimeSpan.FromDays(90));
			var future = Html.TimeSpanSetting("Events/DisplayTimeSpan/Future").GetValueOrDefault(TimeSpan.FromDays(90));

			var occurrences = dataAdapter.SelectEventOccurrences(now.Subtract(past), now.Add(future)).ToArray();

			IEventOccurrence requestOccurrence;

			RequestEventOccurrence = dataAdapter.TryMatchRequestEventOccurrence(Request, occurrences, out requestOccurrence)
				? requestOccurrence
				: occurrences.Length == 1 ? occurrences.Single() : null;

			var user = _portal.Value.User;

			CanRegister = Request.IsAuthenticated && user != null && RequestEventOccurrence != null &&
						RequestEventOccurrence.Start >= now && RequestEventOccurrence.EventSchedule != null &&
						RequestEventOccurrence.Event != null;

			RequiresRegistration = @event.GetAttributeValue<bool?>("adx_requiresregistration").GetValueOrDefault()
				&& RequestEventOccurrence != null
				&& RequestEventOccurrence.Start >= now;

			
			RegistrationRequiresPayment =
				_portal.Value.ServiceContext.CreateQuery("adx_eventproduct")
							.Where(ep => ep.GetAttributeValue<EntityReference>("adx_event") == @event.ToEntityReference())
							.ToList()
							.Any();

			if (CanRegister)
			{
				var registration = _portal.Value.ServiceContext.CreateQuery("adx_eventregistration")
					.FirstOrDefault(e => e.GetAttributeValue<EntityReference>("adx_attendeeid") == user.ToEntityReference()
						&& e.GetAttributeValue<EntityReference>("adx_eventscheduleid") == RequestEventOccurrence.EventSchedule.ToEntityReference()
						&& e.GetAttributeValue<OptionSetValue>("statuscode") != null && e.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)EventStatusCode.Completed);

				if (registration != null)
				{
					IsRegistered = true;
					Unregister.CommandArgument = registration.Id.ToString();
				}
			}

			OtherOccurrences.DataSource = occurrences
				.Where(e => e.Start >= now)
				.Where(e => RequestEventOccurrence == null || !(e.EventSchedule.Id == RequestEventOccurrence.EventSchedule.Id && e.Start == RequestEventOccurrence.Start));

			OtherOccurrences.DataBind();

			var sessionEvent = @event;

			Speakers.DataSource = sessionEvent.GetRelatedEntities(_portal.Value.ServiceContext, new Relationship("adx_eventspeaker_event"))
				.OrderBy(e => e.GetAttributeValue<string>("adx_name"));

			Speakers.DataBind();
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

		protected void Register_Click(object sender, EventArgs e)
		{
			var user = _portal.Value.User;

			if (!Request.IsAuthenticated || user == null || RequestEventOccurrence == null || RequestEventOccurrence.Event == null || RequestEventOccurrence.EventSchedule == null)
			{
				return;
			}

			var registration = new Entity("adx_eventregistration");

			registration["adx_attendeeid"] = user.ToEntityReference();
			registration["adx_eventid"] = RequestEventOccurrence.Event.ToEntityReference();
			registration["adx_eventscheduleid"] = RequestEventOccurrence.EventSchedule.ToEntityReference();
			registration["adx_registrationdate"] = DateTime.UtcNow;
			if (!RegistrationRequiresPayment)
			{
				registration["adx_registrationconfirmed"] = true;
			}
			_portal.Value.ServiceContext.AddObject(registration);
			_portal.Value.ServiceContext.SaveChanges();

			if (!RegistrationRequiresPayment)
			{
				_portal.Value.ServiceContext.SetState(0, (int)EventStatusCode.Completed, registration);
			}

			var registrationUrl = RegistrationRequiresPayment
									? GetRegistrationPaymentUrl(registration.Id)
									: GetRegistrationUrl(registration.Id);

			Response.Redirect(string.IsNullOrWhiteSpace(registrationUrl) ? Request.Url.PathAndQuery : registrationUrl);
		}

		protected string GetRegistrationUrl(Guid eventRegistrationId)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var website = portal.ServiceContext.CreateQuery("adx_website").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_websiteid") == portal.Website.Id);
			var page = portal.ServiceContext.GetPageBySiteMarkerName(website, "Event Registration");

			if (page == null)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Page could not be found for Site Marker named 'Event Registration'");
                return null;
			}

			var url = portal.ServiceContext.GetUrl(page);

			if (string.IsNullOrWhiteSpace(url))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Url could not be determined for Site Marker named 'Event Registration'");
                return null;
			}

			var urlBuilder = new UrlBuilder(url);

			urlBuilder.QueryString.Add(EventRegistrationIdQueryStringParameterName, eventRegistrationId.ToString());

			return urlBuilder.PathWithQueryString;
		}

		protected string GetRegistrationPaymentUrl(Guid eventRegistrationId)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var website = portal.ServiceContext.CreateQuery("adx_website").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_websiteid") == portal.Website.Id);
			var page = portal.ServiceContext.GetPageBySiteMarkerName(website, "Event Registration - Payment Required");

			if (page == null)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Page could not be found for Site Marker named 'Event Registration - Payment Required'");
                return null;
			}

			var url = portal.ServiceContext.GetUrl(page);

			if (string.IsNullOrWhiteSpace(url))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Url could not be determined for Site Marker named 'Event Registration - Payment Required'");
                return null;
			}

			var urlBuilder = new UrlBuilder(url);

			urlBuilder.QueryString.Add(EventRegistrationIdQueryStringParameterName, eventRegistrationId.ToString());

			return urlBuilder.PathWithQueryString;
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

			var registration = _portal.Value.ServiceContext.CreateQuery("adx_eventregistration")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_eventregistrationid") == registrationId
					&& e.GetAttributeValue<EntityReference>("adx_attendeeid") == user.ToEntityReference());

			if (registration != null)
			{
				_portal.Value.ServiceContext.DeleteObject(registration);
				_portal.Value.ServiceContext.SaveChanges();
			}

			Response.Redirect(Request.Url.PathAndQuery);
		}
	}
}
