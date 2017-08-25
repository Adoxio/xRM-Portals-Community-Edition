/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Notes;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Conference.Pages
{
	public partial class ConferenceSpeakers : PortalPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider();

			Speakers.DataSource = ServiceContext.CreateQuery("adx_eventspeaker").Where(es => es.GetAttributeValue<EntityReference>("adx_websiteid") == Website.ToEntityReference()).ToArray()
				.Where(es => securityProvider.TryAssert(ServiceContext, es, CrmEntityRight.Read))
				.OrderBy(es => es.GetAttributeValue<string>("adx_name"))
				.ToArray();

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
	}
}
