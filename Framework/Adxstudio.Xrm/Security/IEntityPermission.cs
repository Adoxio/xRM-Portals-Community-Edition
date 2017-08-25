/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Security
{
	interface IEntityPermission
	{
		EntityReference EntityReference { get; }
		string Name { get; }
		string EntityName { get; }
		EntityPermissionScope? Scope { get; }
		bool Read { get; }
		bool Write { get; }
		bool Create { get; }
		bool Delete { get; }
		bool Append { get; }
		bool AppendTo { get; }
		string ContactRelationshipName { get; }
		EntityReference ParentEntityPermission { get; }
		string ParentRelationshipName { get; }
		IEnumerable<Entity> WebRoles { get; }
	}

	/// <summary>
	/// Enumeration of the entity permission scope
	/// </summary>
	public enum EntityPermissionScope
	{
		/// <summary>
		/// Contact
		/// </summary>
		Contact = 756150001,
		/// <summary>
		/// Account
		/// </summary>
		Account = 756150002,
		/// <summary>
		/// Global
		/// </summary>
		Global = 756150000,
		/// <summary>
		/// Parent
		/// </summary>
		Parent = 756150003,
		/// <summary>
		/// Self
		/// </summary>
		Self = 756150004
	}
}
