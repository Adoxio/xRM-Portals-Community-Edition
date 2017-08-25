/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal.Web;

namespace Site.Areas.Conference.Pages
{
	public partial class Home : ConferencePage
	{
		protected const string ConferenceIdQueryStringParameterName = "conferenceid";

		protected void Page_Load(object sender, EventArgs e)
		{
		}

		protected void Register_Click(object sender, EventArgs args)
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

			return urlBuilder.PathWithQueryString;
		}
	}
}
