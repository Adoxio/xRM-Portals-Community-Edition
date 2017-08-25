/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Memberships
{
	public static class OrganizationServiceContextExtensions
	{
		public static Entity GetMembershipByContact(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			var findMembership =
				from m in context.CreateQuery("adx_membership")
				where m.GetAttributeValue<EntityReference>("adx_administrativecontactid") == contact.ToEntityReference()
				select m;

			return findMembership.FirstOrDefault();
		}
	}
}
