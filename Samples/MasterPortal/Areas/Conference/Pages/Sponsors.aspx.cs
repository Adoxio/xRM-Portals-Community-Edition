/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Conference.Pages
{
	public partial class Sponsors : PortalPage
	{
		private Entity[] _sponsors;

		protected void Page_Init(object sender, EventArgs e)
		{
			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider();

			_sponsors = ServiceContext.CreateQuery("adx_eventsponsor")
				.Where(es => es.GetAttributeValue<EntityReference>("adx_websiteid") == Website.ToEntityReference())
				.ToArray()
				.Where(es => securityProvider.TryAssert(ServiceContext, es, CrmEntityRight.Read))
				.ToArray();
		}

		protected IEnumerable<Entity> GetSponsorsByCategory(int? category)
		{
			return category == null ?
				new Entity[] { }
				: _sponsors.Where(es => es.GetAttributeValue<OptionSetValue>("adx_sponsorshipcategory") != null
					&& es.GetAttributeValue<OptionSetValue>("adx_sponsorshipcategory").Value == category);
		}
	}
}
