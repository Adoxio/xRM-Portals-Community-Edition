/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Reflection;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Client.Metadata
{
	/// <summary>
	/// Represents a set of data describing the underlying entity set types within a <see cref="OrganizationServiceContext"/>.
	/// </summary>
	public sealed class EntitySetInfo
	{
		/// <summary>
		/// Gets the <see cref="PropertyInfo"/> reflection object for the entity set property.
		/// </summary>
		public PropertyInfo Property { get; private set; }

		/// <summary>
		/// Gets the <see cref="EntityInfo"/> reflection object for the corresponding entity type.
		/// </summary>
		public EntityInfo Entity { get; private set; }

		public EntitySetInfo(PropertyInfo property, EntityInfo entity)
		{
			Property = property;
			Entity = entity;
		}
	}
}
