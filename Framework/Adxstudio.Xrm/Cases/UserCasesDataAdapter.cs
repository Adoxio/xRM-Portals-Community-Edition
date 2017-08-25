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
	/// Provides data operations on the set of cases associated with a given portal user, taking into account
	/// case access permissions.
	/// </summary>
	public class UserCasesDataAdapter : ICaseAggregationDataAdapter, ICaseAccessPermissionScopesProvider
	{
		public UserCasesDataAdapter(IDataAdapterDependencies dependencies)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		public virtual IEnumerable<ICase> SelectCases()
		{
			return SelectCases(CaseState.Active);
		}

		public virtual IEnumerable<ICase> SelectCases(CaseState state)
		{
			var serviceContext = Dependencies.GetServiceContext();
			var user = Dependencies.GetPortalUser();

			if (user == null)
			{
				return Enumerable.Empty<ICase>();
			}

			var entities = state == CaseState.Active
				? serviceContext.GetActiveCasesForContact(user)
				: serviceContext.GetClosedCasesForContact(user);

			var urlProvider = Dependencies.GetUrlProvider();

			return entities
				.Select(e => new Case(e, CaseDataAdapter.GetIncidentMetadata(serviceContext), urlProvider.GetUrl(serviceContext, e), GetResponsibleContact(serviceContext, e)))
				.OrderByDescending(e => e.CreatedOn)
				.ToArray();
		}

		public virtual IEnumerable<ICase> SelectCases(Guid account)
		{
			return SelectCases(account, CaseState.Active);
		}

		public virtual IEnumerable<ICase> SelectCases(Guid account, CaseState state)
		{
			var serviceContext = Dependencies.GetServiceContext();
			var user = Dependencies.GetPortalUser();

			if (user == null)
			{
				return Enumerable.Empty<ICase>();
			}

			var entities = state == CaseState.Active
				? serviceContext.GetActiveCasesForContactByAccountId(user, account)
				: serviceContext.GetClosedCasesForContactByAccountId(user, account);

			var urlProvider = Dependencies.GetUrlProvider();

			return entities
				.Select(e => new Case(e, CaseDataAdapter.GetIncidentMetadata(serviceContext), urlProvider.GetUrl(serviceContext, e), GetResponsibleContact(serviceContext, e)))
				.OrderByDescending(e => e.CreatedOn)
				.ToArray();
		}

		public virtual ICaseAccessPermissionScopes SelectPermissionScopes()
		{
			return Dependencies.GetPermissionScopesProviderForPortalUser().SelectPermissionScopes();
		}

		private static Entity GetResponsibleContact(OrganizationServiceContext serviceContext, Entity incident)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (incident == null) throw new ArgumentNullException("incident");

			var responsibleContact = incident.GetAttributeValue<EntityReference>("responsiblecontactid");

			if (responsibleContact != null)
			{
				return serviceContext.CreateQuery("contact").FirstOrDefault(e => e.GetAttributeValue<Guid>("contactid") == responsibleContact.Id);
			}

			var customer = incident.GetAttributeValue<EntityReference>("customerid");

			if (customer != null && customer.LogicalName == "contact")
			{
				return serviceContext.CreateQuery("contact").FirstOrDefault(e => e.GetAttributeValue<Guid>("contactid") == customer.Id);
			}

			return null;
		}
	}
}
