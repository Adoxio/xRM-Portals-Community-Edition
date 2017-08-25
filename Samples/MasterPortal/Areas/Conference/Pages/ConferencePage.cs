/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using Adxstudio.Xrm.Conferences;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Providers;
using Site.Pages;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.Conference.Pages
{
	public class ConferencePage : PortalPage
	{
		protected enum ConferenceRegistrationStatusCode
		{
			Started = 1,
			Completed = 756150000
		}

		public Entity PortalConference
		{
			get { return ServiceContext.GetPortalConference(Website); }
		}

		public IPortalViewEntity PortalConferenceViewEntity
		{
			get
			{
				return new PortalViewEntity(
					ServiceContext,
					PortalConference,
					PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName),
					PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<IEntityUrlProvider>());
			}
		}

		public Entity UserRegistration
		{
			get
			{
				return PortalConference == null ? null : 
					ServiceContext.CreateQuery("adx_conferenceregistration")
					.FirstOrDefault(cr => (cr.GetAttributeValue<EntityReference>("adx_conferenceid") == PortalConference.ToEntityReference())
						&& cr.GetAttributeValue<OptionSetValue>("statuscode") != null && cr.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)ConferenceRegistrationStatusCode.Completed
						&& (cr.GetAttributeValue<EntityReference>("adx_contactid") == (Contact == null ? null : Contact.ToEntityReference())));
			}
		}

		public bool UserIsRegisteredForConference
		{
			get { return UserRegistration != null; }
		}
	}
}
