/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Partner
{
	public static class OrganizationServiceContextExtensions
	{
		private enum OpportunityAccessScope
		{
			Self = 100000000,
			Account = 100000001
		}

		private enum ContactAccessScope
		{
			Self = 1,
			Account = 2
		}

		private enum OpportunityState
		{
			Open = 0,
			Won = 1,
			Lost = 2
		}

		public static Entity GetOpportunity(this OrganizationServiceContext context, Guid opportunityId)
		{
			return context.CreateQuery("opportunity").FirstOrDefault(l => l.GetAttributeValue<Guid>("opportunityid") == opportunityId);
		}

		/// <summary>
		/// Get open opportunities for contact
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetOpportunitiesSpecificToContact(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var access = context.CreateQuery("adx_opportunitypermissions").Where(c =>
				c.GetAttributeValue<EntityReference>("adx_contactid") == contact.ToEntityReference() &&
				c.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false));

			if (!access.ToList().Any())
			{
				return Enumerable.Empty<Entity>();
			}

			var opportunities = context.CreateQuery("opportunity").Where(i =>
				i.GetAttributeValue<EntityReference>("msa_partneroppid") == contact.ToEntityReference());

			return opportunities;
		}

		public static IEnumerable<Entity> GetContactsSpecificToContact(this OrganizationServiceContext context, Entity contact)
		{
			return Enumerable.Empty<Entity>();
		}

		/// <summary>
		/// Get Accounts from Case Access rules for a specified contact record.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetAccountsRelatedToOpportunityAccessForContact(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var accounts = from account in context.CreateQuery("account")
						   join opportunityaccess in context.CreateQuery("adx_opportunitypermissions")
						   on account.GetAttributeValue<Guid>("accountid") equals opportunityaccess.GetAttributeValue<EntityReference>("adx_accountid").Id
						   where opportunityaccess.GetAttributeValue<EntityReference>("adx_accountid") != null && opportunityaccess.GetAttributeValue<EntityReference>("adx_contactid") == contact.ToEntityReference()
						   && opportunityaccess.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false) && opportunityaccess.GetAttributeValue<OptionSetValue>("adx_scope") != null && opportunityaccess.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)OpportunityAccessScope.Account
						   select account;

			return accounts.Distinct();
		}

		public static IEnumerable<Entity> GetAccountsRelatedToContactAccessForContact(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var accounts = from account in context.CreateQuery("account")
						   join contactaccess in context.CreateQuery("adx_contactaccess")
						   on account.GetAttributeValue<Guid>("accountid") equals contactaccess.GetAttributeValue<EntityReference>("adx_accountid").Id
						   where contactaccess.GetAttributeValue<EntityReference>("adx_accountid") != null && contactaccess.GetAttributeValue<EntityReference>("adx_contactid") == contact.ToEntityReference()
						   && contactaccess.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false) && contactaccess.GetAttributeValue<OptionSetValue>("adx_scope") != null && contactaccess.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)ContactAccessScope.Account
						   select account;

			return accounts.Distinct();
		}

		public static IEnumerable<Entity> GetAccountsRelatedToChannelAccessForContact(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var accounts = from account in context.CreateQuery("account")
						   join channelaccess in context.CreateQuery("adx_channelpermissions")
						   on account.GetAttributeValue<Guid>("accountid") equals channelaccess.GetAttributeValue<EntityReference>("adx_accountid").Id
						   where channelaccess.GetAttributeValue<EntityReference>("adx_accountid") != null && 
						   channelaccess.GetAttributeValue<EntityReference>("adx_contactid") == contact.ToEntityReference() &&
						   channelaccess.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false)
						   select account;

			return accounts.Distinct();
		}

		/// <summary>
		/// Get active cases where the customer is the account of any of the accounts assigned to any caseaccess rule for the current contact that allows read and scoped to account.
		/// Also get cases where the customer is a contact and where the parent customer of that contact is an account of any of the accounts assigned to any caseaccess rule for the current contact that allows read and scoped to account.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <param name="accountid">Id of the Account record to return cases for</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetOpportunitiesForContactByAccountId(this OrganizationServiceContext context, Entity contact, Guid accountid)
		{
			contact.AssertEntityName("contact");

			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			// Get opportunities where the Partner is the account of assigned to any opportunity access rule for the current contact that allows read and scoped to account
			var accountopportunities = from opportunity in context.CreateQuery("opportunity")
									   join opportunityaccess in context.CreateQuery("adx_opportunitypermissions")
									   on opportunity.GetAttributeValue<EntityReference>("msa_partnerid") equals opportunityaccess.GetAttributeValue<EntityReference>("adx_accountid")
									   where opportunityaccess.GetAttributeValue<EntityReference>("adx_contactid") == contact.ToEntityReference()
									   && opportunityaccess.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false) && opportunityaccess.GetAttributeValue<OptionSetValue>("adx_scope") != null && opportunityaccess.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)OpportunityAccessScope.Account
									   && opportunityaccess.GetAttributeValue<EntityReference>("adx_accountid") == new EntityReference("account", accountid)
									   select opportunity;

			// Get cases where the partner contact is not null and where the parent customer of the contact is an account assigned to any caseaccess rule for the current contact that allows read and scoped to account
			var parentaccountopportunities = from opportunity in context.CreateQuery("opportunity")
											 join c in context.CreateQuery("contact") on opportunity.GetAttributeValue<EntityReference>("msa_partneroppid").Id equals c.GetAttributeValue<Guid>("contactid")
											 join account in context.CreateQuery("account") on c.GetAttributeValue<EntityReference>("parentcustomerid").Id equals account.GetAttributeValue<Guid>("accountid")
											 join opportunityaccess in context.CreateQuery("adx_opportunitypermissions") on account.GetAttributeValue<Guid>("accountid") equals opportunityaccess.GetAttributeValue<EntityReference>("adx_accountid").Id
											 where opportunity.GetAttributeValue<EntityReference>("msa_partneroppid") != null
											 where c.GetAttributeValue<EntityReference>("parentcustomerid") != null
											 where opportunityaccess.GetAttributeValue<EntityReference>("adx_contactid") == contact.ToEntityReference()
											 && opportunityaccess.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false) && opportunityaccess.GetAttributeValue<OptionSetValue>("adx_scope") != null && opportunityaccess.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)OpportunityAccessScope.Account
											 && opportunityaccess.GetAttributeValue<EntityReference>("adx_accountid") != null && opportunityaccess.GetAttributeValue<EntityReference>("adx_accountid") == new EntityReference("account", accountid)
											 select opportunity;

			var opportunities = accountopportunities.AsEnumerable().Union(parentaccountopportunities.AsEnumerable()).Distinct();

			return opportunities;
		}

		public static IEnumerable<Entity> GetContactsForContactByAccountId(this OrganizationServiceContext context, Entity contact, Guid accountid)
		{
			contact.AssertEntityName("contact");

			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var contactAccess = context.GetContactAccessByContact(contact).Where(ca => ca.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false)
				&& ca.GetAttributeValue<OptionSetValue>("adx_scope") != null && ca.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)ContactAccessScope.Account
				&& ca.GetAttributeValue<EntityReference>("adx_accountid") != null && ca.GetAttributeValue<EntityReference>("adx_accountid").Equals(new EntityReference("account", accountid)));

			var returnSet = new List<Entity>();

			foreach (Entity access in contactAccess)
			{
				var accountcontacts =
					context.CreateQuery("contact").Where(
						c => c.GetAttributeValue<EntityReference>("parentcustomerid") == access.GetAttributeValue<EntityReference>("adx_accountid"));

				returnSet.AddRange(accountcontacts);

			}

			return returnSet.AsEnumerable();
		}

		/// <summary>
		/// Get Case Access for a specified contact record.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="contact">Contact record</param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetOpportunityAccessByContact(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var access = context.CreateQuery("adx_opportunitypermissions").Where(oa => oa.GetAttributeValue<EntityReference>("adx_contactid") == contact.ToEntityReference());

			return access;
		}

		public static IEnumerable<Entity> GetOpportunityAccessByContactForParentAccount(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var access = context.CreateQuery("adx_opportunitypermissions")
				.Where(oa => oa.GetAttributeValue<EntityReference>("adx_contactid") == contact.ToEntityReference()
						&& oa.GetAttributeValue<EntityReference>("adx_accountid") == contact.GetAttributeValue<EntityReference>("parentcustomerid")
						&& oa.GetAttributeValue<OptionSetValue>("adx_scope") != null && oa.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)OpportunityAccessScope.Account);

			return access;
		}

		public static IEnumerable<Entity> GetContactAccessByContact(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var access = context.CreateQuery("adx_contactaccess").Where(ca => ca.GetAttributeValue<EntityReference>("adx_contactid") == contact.ToEntityReference());

			return access;
		}

		public static IEnumerable<Entity> GetAccountAccessByContact(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			if (contact == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var access = context.CreateQuery("adx_accountaccess").Where(aa => aa.GetAttributeValue<EntityReference>("adx_contactid") == contact.ToEntityReference());

			return access;
		}

		public static IEnumerable<Entity> GetOpportunitiesForContact(this OrganizationServiceContext context, Entity contact)
		{
			var accounts = context.GetAccountsRelatedToOpportunityAccessForContact(contact).OrderBy(a => a.GetAttributeValue<string>("name"));

			var opportunities = context.GetOpportunitiesSpecificToContact(contact);

			return accounts.Aggregate(opportunities, (current, account) => current.Union(context.GetOpportunitiesForContactByAccountId(contact, (Guid)account.GetAttributeValue<Guid>("accountid"))).Distinct());
		}

		public static IEnumerable<Entity> GetContactsForContact(this OrganizationServiceContext context, Entity contact)
		{
			var accounts = context.GetAccountsRelatedToContactAccessForContact(contact).OrderBy(a => a.GetAttributeValue<string>("name"));

			var contacts = context.GetContactsSpecificToContact(contact);

			return accounts.Aggregate(contacts, (current, account) => current.Union(context.GetContactsForContactByAccountId(contact, (Guid)account.GetAttributeValue<Guid>("accountid"))).Distinct());
		}

		public static Entity GetChannelAccessByContact(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			if (contact == null)
			{
				return null;
			}

			var currentContact = context.MergeClone(contact);

			if (currentContact == null)
			{
				return null;
			}

			var parentCustomerAccount = currentContact.GetRelatedEntity(context, "contact_customer_accounts");

			if (parentCustomerAccount == null)
			{
				return context.CreateQuery("adx_channelpermissions").FirstOrDefault(c =>
						c.GetAttributeValue<EntityReference>("adx_contactid") != null &&
						c.GetAttributeValue<EntityReference>("adx_contactid").Equals(new EntityReference(contact.LogicalName, contact.GetAttributeValue<Guid>("contactid"))));
			}

			return context.CreateQuery("adx_channelpermissions").FirstOrDefault(c =>
				c.GetAttributeValue<EntityReference>("adx_contactid") != null &&
				c.GetAttributeValue<EntityReference>("adx_contactid").Equals(new EntityReference(contact.LogicalName, contact.GetAttributeValue<Guid>("contactid"))) &&
				c.GetAttributeValue<EntityReference>("adx_accountid") != null &&
				c.GetAttributeValue<EntityReference>("adx_accountid").Equals(new EntityReference(parentCustomerAccount.LogicalName, parentCustomerAccount.GetAttributeValue<Guid>("accountid"))));
		}

		public static int GetInactiveDaysUntilOverdue(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("adx_website");

			int inactiveDaysUntilOverdue;

			return int.TryParse(context.GetSiteSettingValueByName(website, "alerts/inactivity-in-days-for-overdue"), out inactiveDaysUntilOverdue)
				? inactiveDaysUntilOverdue
				: default(int);
		}

		public static int GetInactiveDaysUntilPotentiallyStalled(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("adx_website");

			int inactiveDaysUntilPotentiallyStalled;

			return int.TryParse(context.GetSiteSettingValueByName(website, "alerts/inactivity-in-days-for-potentially-stalled"), out inactiveDaysUntilPotentiallyStalled)
				? inactiveDaysUntilPotentiallyStalled
				: default(int);
		}

		/// <summary>
		/// Retrieves chronologically ordered annotations associated to a provided opportunity based when the annotation was created.
		/// Only histories in which a "status," "pipeline phase," "details," or "assigned to" change occur will be returned.
		/// </summary>
		public static IEnumerable<OpportunityHistory> GetOpportunityHistories(this OrganizationServiceContext context, Entity opportunity)
		{
			opportunity.AssertEntityName("opportunity");

			var allOpportunityHistories = context.CreateQuery("adx_opportunitynote")
				.Where(on => on.GetAttributeValue<EntityReference>("adx_opportunityid") == opportunity.ToEntityReference())
				.OrderBy(on => on.GetAttributeValue<DateTime?>("createdon"))
				.Select(on => new OpportunityHistory(on))
				.ToList();

			var opportunityHistories = new List<OpportunityHistory>();

			var distinctHistories = allOpportunityHistories
				.Where(history => !string.IsNullOrEmpty(history.Name))
				.Select(history => history.Name + history.PartnerAssignedTo + history.Details)
				.Distinct();

			opportunityHistories.AddRange(distinctHistories.Select(uniqueHistory => allOpportunityHistories.FirstOrDefault(history => history.Name + history.PartnerAssignedTo + history.Details == uniqueHistory)));

			return opportunityHistories;
		}

		public static DateTime? GetOpportunityLatestStatusModifiedOn(this OrganizationServiceContext context, Entity opportunity)
		{
			opportunity.AssertEntityName("opportunity");

			var opportunityHistories = context.GetOpportunityHistories(opportunity);

			return opportunityHistories.Any()
				? (DateTime?)opportunityHistories.Max(history => history.NoteCreatedOn)
				: null;
		}
	}
}
