/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cases
{
	/// <summary>
	/// Specifies a set of access rights to cases.
	/// </summary>
	public interface ICaseAccessPermissions
	{
		bool Create { get; }

		bool Delete { get; }

		bool Read { get; }

		bool Write { get; }
	}

	/// <summary>
	/// Specifies a set of access rights to cases, scoped to a given account.
	/// </summary>
	public interface IAccountCaseAccessPermissions : ICaseAccessPermissions
	{
		EntityReference Account { get; }
	}

	internal class AccountCaseAccessPermissions : IAccountCaseAccessPermissions
	{
		public AccountCaseAccessPermissions(EntityReference account, bool create = false, bool delete = false, bool read = false, bool write = false)
		{
			if (account == null) throw new ArgumentNullException("account");
			if (account.LogicalName != "account") throw new ArgumentException(string.Format(ResourceManager.GetString("Value_Missing_For_LogicalName"), account.LogicalName), "account");

			Account = account;
			Create = create;
			Delete = delete;
			Read = read;
			Write = write;
		}

		public EntityReference Account { get; private set; }

		public bool Create { get; private set; }

		public bool Delete { get; private set; }

		public bool Read { get; private set; }

		public bool Write { get; private set; }
	}

	internal class CaseAccessPermissions : ICaseAccessPermissions
	{
		public static readonly ICaseAccessPermissions Full = new CaseAccessPermissions(true, true, true, true);
		public static readonly ICaseAccessPermissions None = new CaseAccessPermissions();

		public CaseAccessPermissions(bool create = false, bool delete = false, bool read = false, bool write = false)
		{
			Create = create;
			Delete = delete;
			Read = read;
			Write = write;
		}

		public bool Create { get; private set; }

		public bool Delete { get; private set; }

		public bool Read { get; private set; }

		public bool Write { get; private set; }
	}
}
