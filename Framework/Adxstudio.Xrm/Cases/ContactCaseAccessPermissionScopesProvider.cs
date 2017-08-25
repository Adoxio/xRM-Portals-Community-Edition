/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cases
{
	/// <summary>
	/// Implements <see cref="ICaseAccessPermissionScopesProvider"/> for adx_caseaccess permission rules.
	/// </summary>
	public class ContactCaseAccessPermissionScopesProvider : ICaseAccessPermissionScopesProvider
	{
		internal enum CaseAccessPermissionScope
		{
			Self = 1,
			Account = 2,
		}

		public ContactCaseAccessPermissionScopesProvider(EntityReference contact, IDataAdapterDependencies dependencies)
		{
			if (contact == null) throw new ArgumentNullException("contact");
			if (contact.LogicalName != "contact") throw new ArgumentException(string.Format(ResourceManager.GetString("Value_Missing_For_LogicalName"), contact.LogicalName), "contact");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Contact = contact;
			Dependencies = dependencies;
		}

		protected EntityReference Contact { get; private set; }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		public ICaseAccessPermissionScopes SelectPermissionScopes()
		{
			var serviceContext = Dependencies.GetServiceContext();
			var adx_caseaccesses = serviceContext.GetCaseAccessByContact(Contact).ToArray();

			// If no permissions are defined for Contact at all, default to full Self permissions, but no account permissions.
			if (!adx_caseaccesses.Any())
			{
				return CaseAccessPermissionScopes.SelfOnly;
			}

			var self = new MutableCaseAccessPermissions();
			var accounts = new Dictionary<Guid, Tuple<EntityReference, MutableCaseAccessPermissions>>();
			
			// Equivalent permission rules get their individual rights grants OR'ed together.
			foreach (var adx_caseaccess in adx_caseaccesses)
			{
				var scope = adx_caseaccess.GetAttributeValue<OptionSetValue>("adx_scope") ?? new OptionSetValue((int)CaseAccessPermissionScope.Self);
				var account = GetAccount(serviceContext, adx_caseaccess);

				var grantCreate = adx_caseaccess.GetAttributeValue<bool?>("adx_create").GetValueOrDefault();
				var grantDelete = adx_caseaccess.GetAttributeValue<bool?>("adx_delete").GetValueOrDefault();
				var grantWrite  = adx_caseaccess.GetAttributeValue<bool?>("adx_write").GetValueOrDefault();
				var grantRead   = adx_caseaccess.GetAttributeValue<bool?>("adx_read").GetValueOrDefault();
				
				if (scope.Value == (int)CaseAccessPermissionScope.Self)
				{
					self.Create = grantCreate || self.Create;
					self.Delete = grantDelete || self.Delete;
					self.Read   = grantRead   || self.Read;
					self.Write  = grantWrite  || self.Write;

					continue;
				}

				if (scope.Value == (int)CaseAccessPermissionScope.Account && account != null)
				{
					Tuple<EntityReference, MutableCaseAccessPermissions> accountPermissions;

					if (accounts.TryGetValue(account.Id, out accountPermissions))
					{
						var permissions = accountPermissions.Item2;

						permissions.Create = grantCreate || permissions.Create;
						permissions.Delete = grantDelete || permissions.Delete;
						permissions.Read   = grantRead   || permissions.Read;
						permissions.Write  = grantWrite  || permissions.Write;
					}
					else
					{
						accounts.Add(
							account.Id,
							new Tuple<EntityReference, MutableCaseAccessPermissions>(
								account,
								new MutableCaseAccessPermissions(grantCreate, grantDelete, grantRead, grantWrite)));
					}
				}
			}

			return new CaseAccessPermissionScopes(
				new CaseAccessPermissions(self.Create, self.Delete, self.Read, self.Write),
				from e in accounts
				let account = e.Value.Item1
				let permissions = e.Value.Item2
				select new AccountCaseAccessPermissions(account, permissions.Create, permissions.Delete, permissions.Read, permissions.Write));
		}

		private static EntityReference GetAccount(OrganizationServiceContext serviceContext, Entity adx_caseaccess)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (adx_caseaccess == null) throw new ArgumentNullException("adx_caseaccess");

			var accountReference = adx_caseaccess.GetAttributeValue<EntityReference>("adx_accountid");

			if (accountReference == null)
			{
				return null;
			}

			if (!string.IsNullOrEmpty(accountReference.Name))
			{
				return accountReference;
			}

			// If the account EntityReference retrieved from the adx_caseaccess entity does not have its Name
			// set, retrieve the full account to get its name and rebuild the EntityReference with this data.
			// We want consumers of the API to be able to rely on that value being present.
			var account = serviceContext.CreateQuery("account").FirstOrDefault(e => e.GetAttributeValue<Guid>("accountid") == accountReference.Id);

			if (account == null)
			{
				return accountReference;
			}

			return new EntityReference(account.LogicalName, account.Id)
			{
				Name = account.GetAttributeValue<string>("name")
			};
		}

		private class MutableCaseAccessPermissions : ICaseAccessPermissions
		{
			public MutableCaseAccessPermissions(bool create = false, bool delete = false, bool read = false, bool write = false)
			{
				Create = create;
				Delete = delete;
				Read = read;
				Write = write;
			}

			public bool Create { get; set; }

			public bool Delete { get; set; }

			public bool Read { get; set; }

			public bool Write { get; set; }
		}
	}
}
