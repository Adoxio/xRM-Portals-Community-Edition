/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Events
{
	/// <summary>
	/// Aggregates all <see cref="EventOccurrence">occurrences</see> of all adx_events associated with
	/// the current portal adx_website.
	/// </summary>
	public class WebsiteEventDataAdapter : EventAggregationDataAdapter
	{
		public WebsiteEventDataAdapter(IDataAdapterDependencies dependencies) : base(dependencies) { }

		public override IEnumerable<Entity> SelectEvents()
		{
			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();
			var security = Dependencies.GetSecurityProvider();

			return serviceContext.CreateQuery("adx_event")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website
					&& e.GetAttributeValue<int?>("statecode") == 0)
				.ToArray()
				.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read))
				.ToArray();
		}
	}
}
