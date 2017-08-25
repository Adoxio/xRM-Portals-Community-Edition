/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;

namespace Adxstudio.Xrm.Cases
{
	public class CaseAccessPermissionScopes : ICaseAccessPermissionScopes
	{
		public static readonly ICaseAccessPermissionScopes None = new CaseAccessPermissionScopes();
		public static readonly ICaseAccessPermissionScopes SelfOnly = new CaseAccessPermissionScopes(CaseAccessPermissions.Full);

		public CaseAccessPermissionScopes(ICaseAccessPermissions self = null, IEnumerable<IAccountCaseAccessPermissions> accounts = null)
		{
			Self = self ?? CaseAccessPermissions.None;
			Accounts = accounts ?? Enumerable.Empty<IAccountCaseAccessPermissions>();
		}

		public IEnumerable<IAccountCaseAccessPermissions> Accounts { get; private set; }

		public ICaseAccessPermissions Self { get; private set; }
	}
}
