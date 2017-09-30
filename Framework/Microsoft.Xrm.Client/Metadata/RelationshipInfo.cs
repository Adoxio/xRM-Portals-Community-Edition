/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Client.Metadata
{
	/// <summary>
	/// Represents metadata describing a relationship property.
	/// </summary>
	public sealed class RelationshipInfo
	{
		/// <summary>
		/// The reflection object for the relationship property.
		/// </summary>
		public PropertyInfo Property { get; private set; }

		/// <summary>
		/// The annotation for the relationship property.
		/// </summary>
		public RelationshipSchemaNameAttribute CrmAssociationAttribute { get; private set; }

		/// <summary>
		/// Indicates that the relationship property returns a collection of entities.
		/// </summary>
		public bool IsCollection { get; private set; }

		public RelationshipInfo(PropertyInfo property, RelationshipSchemaNameAttribute crmAssociationAttribute)
		{
			Property = property;
			CrmAssociationAttribute = crmAssociationAttribute;
			IsCollection = property.PropertyType.GetInterfaces().Any(i => i == typeof(IEnumerable));
		}
	}
}
