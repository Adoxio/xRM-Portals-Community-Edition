/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Events
{
	/// <summary>
	/// Aggregates all <see cref="EventOccurrence">occurrences</see> of all adx_events that are children of a given Web Page (adx_webpage).
	/// </summary>
	public class WebPageChildEventDataAdapter : EventAggregationDataAdapter
	{
		public WebPageChildEventDataAdapter(EntityReference webPage, IDataAdapterDependencies dependencies) : base(dependencies)
		{
			if (webPage == null) throw new ArgumentNullException("webPage");

			if (webPage.LogicalName != "adx_webpage")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", webPage.LogicalName), "webPage");
			}

			WebPage = webPage;
		}

		public EntityReference WebPage { get; set; }

		public override IEnumerable<Entity> SelectEvents()
		{
			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();
			var security = Dependencies.GetSecurityProvider();

			return serviceContext.CreateQuery("adx_event")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website
					&& e.GetAttributeValue<EntityReference>("adx_parentpageid") == WebPage
					&& e.GetAttributeValue<int?>("statecode") == 0
					&& e.GetAttributeValue<bool?>("adx_hiddenfromsitemap") == false)
				.ToArray()
				.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read))
				.ToArray();
		}
	}
}
