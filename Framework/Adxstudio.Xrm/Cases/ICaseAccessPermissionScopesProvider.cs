/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cases
{
	/// <summary>
	/// Provides data access to <see cref="ICaseAccessPermissionScopes"/>.
	/// </summary>
	public interface ICaseAccessPermissionScopesProvider
	{
		ICaseAccessPermissionScopes SelectPermissionScopes();
	}

	internal class NoCaseAccessPermissionScopesProvider : ICaseAccessPermissionScopesProvider
	{
		public ICaseAccessPermissionScopes SelectPermissionScopes()
		{
			return CaseAccessPermissionScopes.None;
		}
	}
}
