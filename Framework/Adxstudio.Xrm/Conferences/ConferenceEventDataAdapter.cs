/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Conferences
{
	/// <summary>
	/// Aggregates all <see cref="EventOccurrence">occurrences</see> of all adx_events associated with
	/// the current portal adx_conference.
	/// </summary>
	public class ConferenceEventDataAdapter : EventAggregationDataAdapter
	{
		private Entity _conference;

		public ConferenceEventDataAdapter(IDataAdapterDependencies dependencies, Entity conference)
			: base(dependencies)
		{
			_conference = conference;
		}

		//public ConferenceEventDataAdapter(IDataAdapterDependencies dependencies) : base(dependencies) {}

		public override IEnumerable<Entity> SelectEvents()
		{
			var serviceContext = Dependencies.GetServiceContext();
			//var website = Dependencies.GetWebsite();
			var conference = _conference.ToEntityReference();
			var security = Dependencies.GetSecurityProvider();

			return serviceContext.CreateQuery("adx_event")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_conferenceid") == conference)
				.ToArray()
				.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read))
				.ToArray();
		}
	}
}
