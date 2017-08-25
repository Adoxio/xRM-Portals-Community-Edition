/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cases
{
	/// <summary>
	/// OrganizationServiceContext extension methods
	/// </summary>
	public static class OrganizationServiceContextExtensions
	{
		private enum CaseAccessScope
		{
			Self = 1,
			Account = 2
		}

		private enum IncidentState
		{
			Active = 0
		}

		/// <summary>
		/// Get active cases where the customer is the contact and has caseaccess rule that allows read 
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetActiveCasesForContact(this OrganizationServiceContext context, Entity contact)
		{
			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			contact.AssertEntityName("contact");

			return GetActiveCasesForContact(context, contact.ToEntityReference());
		}

		/// <summary>
		/// Get active cases where the customer is the contact unless has caseaccess rule that denies read 
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetActiveCasesForContact(this OrganizationServiceContext context, EntityReference contact)
		{
			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var access = context.CreateQuery("adx_caseaccess")
				.Where(c => c.GetAttributeValue<EntityReference>("adx_contactid") == contact
					&& c.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false) == false && c.GetAttributeValue<OptionSetValue>("adx_scope") != null && c.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)CaseAccessScope.Self);

			if (access.ToArray().Any())
			{
				return Enumerable.Empty<Entity>();
			}

			var cases = context.CreateQuery("incident")
				.Where(i => i.GetAttributeValue<EntityReference>("customerid") == contact
					&& i.GetAttributeValue<OptionSetValue>("statecode") != null && i.GetAttributeValue<OptionSetValue>("statecode").Value == (int)IncidentState.Active);

			return cases.ToArray();
		}

		/// <summary>
		/// Get non active cases where the customer is the contact and has caseaccess rule that allows read 
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetClosedCasesForContact(this OrganizationServiceContext context, Entity contact)
		{
			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			contact.AssertEntityName("contact");

			return GetClosedCasesForContact(context, contact);
		}

		/// <summary>
		/// Get non active cases where the customer is the contact and unless has caseaccess rule that denies read 
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetClosedCasesForContact(this OrganizationServiceContext context, EntityReference contact)
		{
			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var access = context.CreateQuery("adx_caseaccess")
				.Where(c => c.GetAttributeValue<EntityReference>("adx_contactid") == contact
					&& c.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false) == false && c.GetAttributeValue<OptionSetValue>("adx_scope") != null && c.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)CaseAccessScope.Self);

			if (access.ToArray().Any())
			{
				return Enumerable.Empty<Entity>();
			}

			var cases = context.CreateQuery("incident")
				.Where(i => i.GetAttributeValue<EntityReference>("customerid") == contact
					&& i.GetAttributeValue<OptionSetValue>("statecode") != null && i.GetAttributeValue<OptionSetValue>("statecode").Value != (int)IncidentState.Active);

			return cases;
		}

		/// <summary>
		/// Get active cases where the customer is the account of any of the accounts assigned to any caseaccess rule for the current contact that allows read and scoped to account.
		/// Also get cases where the customer is a contact and where the parent customer of that contact is an account of any of the accounts assigned to any caseaccess rule for the current contact that allows read and scoped to account.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <param name="accountid">Id of the Account record to return cases for</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetActiveCasesForContactByAccountId(this OrganizationServiceContext context, Entity contact, Guid accountid)
		{
			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			contact.AssertEntityName("contact");

			return GetActiveCasesForContactByAccountId(context, contact.ToEntityReference(), accountid);
		}

		/// <summary>
		/// Get active cases where the customer is the account of any of the accounts assigned to any caseaccess rule for the current contact that allows read and scoped to account.
		/// Also get cases where the customer is a contact and where the parent customer of that contact is an account of any of the accounts assigned to any caseaccess rule for the current contact that allows read and scoped to account.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <param name="accountid">Id of the Account record to return cases for</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetActiveCasesForContactByAccountId(this OrganizationServiceContext context, EntityReference contact, Guid accountid)
		{
			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			// Get cases where the customer is the account of assigned to any caseaccess rule for the current contact that allows read and scoped to account
			var accountcases = from incident in context.CreateQuery("incident")
				join caseaccess in context.CreateQuery("adx_caseaccess")
				on incident.GetAttributeValue<EntityReference>("customerid") equals caseaccess.GetAttributeValue<EntityReference>("adx_accountid")
				where caseaccess.GetAttributeValue<EntityReference>("adx_contactid") == contact
					&& caseaccess.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false) && caseaccess.GetAttributeValue<OptionSetValue>("adx_scope") != null && caseaccess.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)CaseAccessScope.Account
					&& caseaccess.GetAttributeValue<EntityReference>("adx_accountid") == new EntityReference("account", accountid)
				where incident.GetAttributeValue<OptionSetValue>("statecode") != null && incident.GetAttributeValue<OptionSetValue>("statecode").Value == (int)IncidentState.Active
				select incident;

			// Get cases where the customer is a contact and where the parent customer of the contact is an account assigned to any caseaccess rule for the current contact that allows read and scoped to account
			var parentaccountcases = from incident in context.CreateQuery("incident")
				join c in context.CreateQuery("contact") on incident.GetAttributeValue<EntityReference>("customerid").Id equals c.GetAttributeValue<Guid>("contactid")
				join account in context.CreateQuery("account") on c.GetAttributeValue<EntityReference>("parentcustomerid").Id equals account.GetAttributeValue<Guid>("accountid")
				join caseaccess in context.CreateQuery("adx_caseaccess") on account.GetAttributeValue<Guid>("accountid") equals caseaccess.GetAttributeValue<EntityReference>("adx_accountid").Id
				where caseaccess.GetAttributeValue<EntityReference>("adx_contactid") == contact
					&& caseaccess.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false) && caseaccess.GetAttributeValue<OptionSetValue>("adx_scope") != null && caseaccess.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)CaseAccessScope.Account
					&& caseaccess.GetAttributeValue<EntityReference>("adx_accountid") == new EntityReference("account", accountid)
				where incident.GetAttributeValue<OptionSetValue>("statecode") != null && incident.GetAttributeValue<OptionSetValue>("statecode").Value == (int)IncidentState.Active
				select incident;

			var cases = accountcases.AsEnumerable().Union(parentaccountcases.AsEnumerable()).Distinct();

			return cases;
		}

		/// <summary>
		/// Get closed cases where the customer is the account of any of the accounts assigned to any caseaccess rule for the current contact that allows read and scoped to account.
		/// Also get cases where the customer is a contact and where the parent customer of that contact is an account of any of the accounts assigned to any caseaccess rule for the current contact that allows read and scoped to account.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <param name="accountid">Id of the Account record to return cases for</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetClosedCasesForContactByAccountId(this OrganizationServiceContext context, Entity contact, Guid accountid)
		{
			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			contact.AssertEntityName("contact");

			return GetClosedCasesForContactByAccountId(context, contact.ToEntityReference(), accountid);
		}

		/// <summary>
		/// Get closed cases where the customer is the account of any of the accounts assigned to any caseaccess rule for the current contact that allows read and scoped to account.
		/// Also get cases where the customer is a contact and where the parent customer of that contact is an account of any of the accounts assigned to any caseaccess rule for the current contact that allows read and scoped to account.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <param name="accountid">Id of the Account record to return cases for</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetClosedCasesForContactByAccountId(this OrganizationServiceContext context, EntityReference contact, Guid accountid)
		{
			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			// Get cases where the customer is the account of assigned to any caseaccess rule for the current contact that allows read and scoped to account
			var accountcases = from incident in context.CreateQuery("incident")
				join caseaccess in context.CreateQuery("adx_caseaccess")
				on incident.GetAttributeValue<Guid>("customerid") equals caseaccess.GetAttributeValue<Guid>("adx_accountid")
				where caseaccess.GetAttributeValue<EntityReference>("adx_contactid") == contact
					&& caseaccess.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false) && caseaccess.GetAttributeValue<OptionSetValue>("adx_scope") != null && caseaccess.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)CaseAccessScope.Account
					&& caseaccess.GetAttributeValue<EntityReference>("adx_accountid") == new EntityReference("account", accountid)
				where incident.GetAttributeValue<OptionSetValue>("statecode") != null && incident.GetAttributeValue<OptionSetValue>("statecode").Value != (int)IncidentState.Active
				select incident;

			// Get cases where the customer is a contact and where the parent customer of the contact is an account assigned to any caseaccess rule for the current contact that allows read and scoped to account
			var parentaccountcases = from incident in context.CreateQuery("incident")
				join c in context.CreateQuery("contact") on incident.GetAttributeValue<EntityReference>("customerid").Id equals c.GetAttributeValue<Guid>("contactid")
				join account in context.CreateQuery("account") on c.GetAttributeValue<EntityReference>("parentcustomerid").Id equals account.GetAttributeValue<Guid>("accountid")
				join caseaccess in context.CreateQuery("adx_caseaccess") on account.GetAttributeValue<Guid>("accountid") equals caseaccess.GetAttributeValue<EntityReference>("adx_accountid").Id
				where caseaccess.GetAttributeValue<EntityReference>("adx_contactid") == contact
					&& caseaccess.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false) && caseaccess.GetAttributeValue<OptionSetValue>("adx_scope") != null && caseaccess.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)CaseAccessScope.Account
					&& caseaccess.GetAttributeValue<EntityReference>("adx_accountid") == new EntityReference("account", accountid)
				where incident.GetAttributeValue<OptionSetValue>("statecode") != null && incident.GetAttributeValue<OptionSetValue>("statecode").Value != (int)IncidentState.Active
				select incident;

			var cases = accountcases.AsEnumerable().Union(parentaccountcases.AsEnumerable()).Distinct();

			return cases;
		}

		/// <summary>
		/// Get Case Access for a specified contact record.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetCaseAccessByContact(this OrganizationServiceContext context, Entity contact)
		{
			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			contact.AssertEntityName("contact");

			return GetCaseAccessByContact(context, contact.ToEntityReference());
		}

		/// <summary>
		/// Get Case Access for a specified contact record.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetCaseAccessByContact(this OrganizationServiceContext context, EntityReference contact)
		{
			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			return context.CreateQuery("adx_caseaccess")
				.Where(ca => ca.GetAttributeValue<EntityReference>("adx_contactid") == contact)
				.AsEnumerable();
		}

		/// <summary>
		/// Get Accounts from Case Access rules for a specified contact record.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetAccountsRelatedToCaseAccessForContact(this OrganizationServiceContext context, Entity contact)
		{
			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			contact.AssertEntityName("contact");

			return GetAccountsRelatedToCaseAccessForContact(context, contact.ToEntityReference());
		}

		/// <summary>
		/// Get Accounts from Case Access rules for a specified contact record.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetAccountsRelatedToCaseAccessForContact(this OrganizationServiceContext context, EntityReference contact)
		{
			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var accounts = from account in context.CreateQuery("account")
				join caseaccess in context.CreateQuery("adx_caseaccess")
				on account.GetAttributeValue<Guid>("accountid") equals caseaccess.GetAttributeValue<EntityReference>("adx_accountid").Id
				where caseaccess.GetAttributeValue<EntityReference>("adx_accountid") != null && caseaccess.GetAttributeValue<EntityReference>("adx_contactid") == contact
					&& caseaccess.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false) && caseaccess.GetAttributeValue<OptionSetValue>("adx_scope") != null && caseaccess.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)CaseAccessScope.Account
				select account;

			return accounts.Distinct().AsEnumerable();
		}
	}
}
