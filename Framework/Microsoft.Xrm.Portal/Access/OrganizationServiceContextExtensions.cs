/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Access
{
	public static class OrganizationServiceContextExtensions
	{
		public static Entity GetAccountAccessByContact(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			if (contact == null) return null;

			var parentCustomerAccount = contact.GetRelatedEntity(context, "contact_customer_accounts");

			if (parentCustomerAccount == null) return null;

			var findAccountAccess =
				from aa in context.CreateQuery("adx_accountaccess").ToList()
				let c = aa.GetRelatedEntity(context, "adx_contact_accountaccess")
				let a = aa.GetRelatedEntity(context, "adx_account_accountaccess")
				where c != null && c.GetAttributeValue<Guid>("contactid") == contact.GetAttributeValue<Guid>("contactid")
					&& a != null && a.GetAttributeValue<Guid>("accountid") == parentCustomerAccount.GetAttributeValue<Guid>("accountid")
				select aa;

			return findAccountAccess.FirstOrDefault();
		}

		public static Entity GetCaseAccessByContact(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			if (contact == null) return null;

			var parentCustomerAccount = contact.GetRelatedEntity(context, "contact_customer_accounts");

			IEnumerable<Entity> findCaseAccess;

			if (parentCustomerAccount == null) //contact is not associated with a parent account record
			{
				findCaseAccess =
					from aa in context.CreateQuery("adx_caseaccess").ToList()
					let c = aa.GetRelatedEntity(context, "adx_contact_caseaccess")
					where c != null && c.GetAttributeValue<Guid>("contactid") == contact.GetAttributeValue<Guid>("contactid")
					select aa;
			}
			else
			{
				findCaseAccess =
					 from ca in context.CreateQuery("adx_caseaccess").ToList()
					 let c = ca.GetRelatedEntity(context, "adx_contact_caseaccess")
					 let a = ca.GetRelatedEntity(context, "adx_account_caseaccess")
					 where c != null && c.GetAttributeValue<Guid>("contactid") == contact.GetAttributeValue<Guid>("contactid")
						 && a != null && a.GetAttributeValue<Guid>("accountid") == parentCustomerAccount.GetAttributeValue<Guid>("accountid")
					 select ca;
			}

			return findCaseAccess.FirstOrDefault();
		}

		public static Entity GetContactAccessByContact(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			if (contact == null) return null;

			var findContactAccess =
				from ca in context.CreateQuery("adx_contactaccess").ToList()
				let c = ca.GetRelatedEntity(context, "adx_contact_contactaccess")
				where c != null && c.GetAttributeValue<Guid>("contactid") == contact.GetAttributeValue<Guid>("contactid")
				select ca;

			return findContactAccess.FirstOrDefault();
		}
	}
}
