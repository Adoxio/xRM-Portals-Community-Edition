/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Conferences
{
	public static class OrganizationServiceContextExtensions
	{
		public static Entity GetRegistrationForCurrentUser(this OrganizationServiceContext context, Entity entity, Guid contactId)
		{
			if (entity == null) return null;
			
			entity.AssertEntityName("adx_conference");

			var conference = context.CreateQuery("adx_conference").FirstOrDefault(c => c.GetAttributeValue<Guid>("adx_conferenceid") == entity.Id);

			var registrations = conference.GetRelatedEntities(context, "adx_conference_conferenceregistration");

			return registrations.FirstOrDefault(cr => cr.GetAttributeValue<EntityReference>("adx_contactid") == new EntityReference("contact", contactId));
		}

		public static IEnumerable<Entity> GetValidDiscounts(this OrganizationServiceContext context, Entity conference)
		{
			if (conference == null) return null;
			conference.AssertEntityName("adx_conference");

			var discounts = conference.GetRelatedEntities(context, "adx_conference_conferencediscount");

			return
				from d in discounts
				let startDate = d.GetAttributeValue<DateTime?>("adx_startdate")
				let endDate = d.GetAttributeValue<DateTime?>("adx_enddate")
				where (startDate == null || startDate.Value <= DateTime.UtcNow)
					&& (endDate == null || endDate.Value >= DateTime.UtcNow)
				select d;
		}

		public static bool IsOptionSelected(this OrganizationServiceContext context, Entity option, Entity registration)
		{
			option.AssertEntityName("adx_conferenceoption");
			registration.AssertEntityName("adx_conferenceregistration");

			if (registration == null) return false;

			var options = registration.GetRelatedEntities(context, "adx_conferenceregistration_option");

			return options.Any(co => co.GetAttributeValue<Guid>("adx_conferenceoptionid") == option.GetAttributeValue<Guid>("adx_conferenceoptionid"));
		}

		public static Entity GetPortalConference(this OrganizationServiceContext context, Entity website)
		{
			var conferenceName = context.GetSiteSettingValueByName(website, "conference-name");

			return context.CreateQuery("adx_conference").FirstOrDefault(c => c.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference()
													&& c.GetAttributeValue<string>("adx_name") == conferenceName);
		}

		public static void CreateEventRegistration(this OrganizationServiceContext serviceContext, EntityReference userReference,
			EntityReference eventReference, EntityReference eventScheduleReference, DateTime registrationDate)
		{
			var registration = new Entity("adx_eventregistration");

			registration["adx_attendeeid"] = userReference;
			registration["adx_eventid"] = eventReference;
			registration["adx_eventscheduleid"] = eventScheduleReference;
			registration["adx_registrationdate"] = registrationDate;
			registration["adx_registrationconfirmed"] = true;

			serviceContext.AddObject(registration);
			serviceContext.SaveChanges();
		}
	}
}
