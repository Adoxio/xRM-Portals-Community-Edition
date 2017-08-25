/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// Represents the metadata values needed to retrieve a user entity.
	/// </summary>
	public interface IUserResolutionSettings
	{
		string MemberEntityName { get; }
		string AttributeMapUsername { get; }
	}
}
