/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Marketing
{
	public class MarketingList : IMarketingList
	{
		public MarketingList(IGrouping<Guid, Entity> listGroup)
		{
			Id = listGroup.Key;
			Name = listGroup.First().GetAttributeValue<string>("listname");
			Purpose = listGroup.First().GetAttributeValue<string>("purpose");

			Subscribers = listGroup.Where(g => g.GetAttributeValue<AliasedValue>("c.contactid") != null).Select(e => new EntityReference("contact", (Guid)e.GetAttributeValue<AliasedValue>("c.contactid").Value))
				.Concat(listGroup.Where(g => g.GetAttributeValue<AliasedValue>("l.leadid") != null).Select(e => new EntityReference("lead", (Guid)e.GetAttributeValue<AliasedValue>("l.leadid").Value)))
				.Concat(listGroup.Where(g => g.GetAttributeValue<AliasedValue>("a.accountid") != null).Select(e => new EntityReference("account", (Guid)e.GetAttributeValue<AliasedValue>("a.accountid").Value)))
				.Distinct();
		}

		public Guid Id { get; private set; }
		public string Name { get; private set; }
		public string Purpose { get; private set; }
		public IEnumerable<EntityReference> Subscribers { get; private set; }
	}
}
