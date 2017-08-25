/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Cases
{
	/// <summary>
	/// Specifies scoped (account and self scopes) <see cref="ICaseAccessPermissions"/> for a given portal user.
	/// </summary>
	public interface ICaseAccessPermissionScopes
	{
		IEnumerable<IAccountCaseAccessPermissions> Accounts { get; }

		ICaseAccessPermissions Self { get; }
	}
}
