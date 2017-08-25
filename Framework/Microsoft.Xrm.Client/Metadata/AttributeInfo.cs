/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Reflection;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Client.Metadata
{
	/// <summary>
	/// Represents metadata describing a basic property.
	/// </summary>
	public sealed class AttributeInfo
	{
		/// <summary>
		/// The <see cref="PropertyInfo"/> reflection object for the attribute property.
		/// </summary>
		public PropertyInfo Property { get; private set; }

		/// <summary>
		/// The logical name annotation of the attribute property.
		/// </summary>
		public AttributeLogicalNameAttribute CrmPropertyAttribute { get; private set; }

		public AttributeInfo(PropertyInfo property, AttributeLogicalNameAttribute crmPropertyAttribute)
		{
			Property = property;
			CrmPropertyAttribute = crmPropertyAttribute;
		}

		/// <summary>
		/// Retrieves the value of the property for an <see cref="Entity"/>.
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public object GetValue(object entity)
		{
			return Property.GetValue(entity, null);
		}
	}
}
