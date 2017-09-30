/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Metadata;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Handlers
{
	/// <summary>
	/// Updates an attribute value for a given entity from a <see cref="JToken"/> property value, using
	/// service context reflection to do the appropriate type conversions.
	/// </summary>
	internal class ReflectionEntityAttributeUpdate : EntityAttributeUpdate
	{
		public ReflectionEntityAttributeUpdate(Entity entity, AttributeInfo attribute) : base(entity)
		{
			if (attribute == null)
			{
				throw new ArgumentNullException("attribute");
			}

			Attribute = attribute;
		}

		protected AttributeInfo Attribute { get; private set; }

		public override void Apply(JToken token)
		{
			if (token == null)
			{
				throw new ArgumentNullException("token");
			}

			var property = Attribute.Property;

			var value = GetValue(token, property.PropertyType);

			if (value == null)
			{
				Entity.SetAttributeValue(Attribute.CrmPropertyAttribute.LogicalName, null);
			}
			else
			{
				property.SetValue(Entity, value, null);
			}
		}

		protected object GetValue(JToken token, Type propertyType)
		{
			var value = token.ToObject<object>();

			if (value == null)
			{
				return null;
			}

			if (propertyType == typeof(bool?))
			{
				return Convert.ToBoolean(value);
			}

			if (propertyType == typeof(CrmEntityReference))
			{
				EntityReference entityReference;

				if (TryGetEntityReference(value, out entityReference))
				{
					return new CrmEntityReference(entityReference.LogicalName, entityReference.Id);
				}

				throw new FormatException("Unable to convert value {0} for attribute {1} to {2}.".FormatWith(value, Attribute.CrmPropertyAttribute.LogicalName, typeof(EntityReference)));
			}

			if (propertyType == typeof(DateTime?))
			{
				var dateTimeValue = value is DateTime ? (DateTime)value : Convert.ToDateTime(value);

				return dateTimeValue.Kind == DateTimeKind.Utc ? dateTimeValue : dateTimeValue.ToUniversalTime();
			}

			if (propertyType == typeof(double?))
			{
				return Convert.ToDouble(value);
			}

			if (propertyType == typeof(decimal))
			{
				return Convert.ToDecimal(value);
			}

			if (propertyType == typeof(EntityReference))
			{
				EntityReference entityReference;

				if (TryGetEntityReference(value, out entityReference))
				{
					return entityReference;
				}

				throw new FormatException("Unable to convert value {0} for attribute {1} to {2}.".FormatWith(value, Attribute.CrmPropertyAttribute.LogicalName, typeof(EntityReference)));
			}

			if (propertyType == typeof(Guid?))
			{
				return value is Guid ? value : new Guid(value.ToString());
			}

			if (propertyType == typeof(int?))
			{
				return Convert.ToInt32(value);
			}

			if (propertyType == typeof(long?))
			{
				return Convert.ToInt64(value);
			}

			if (propertyType == typeof(string))
			{
				return value is string ? value : value.ToString();
			}

			if (propertyType.IsAssignableFrom(value.GetType()))
			{
				return value;
			}

			throw new InvalidOperationException("Unable to convert value of type".FormatWith(value.GetType(), propertyType, Attribute.CrmPropertyAttribute.LogicalName));
		}
	}
}
