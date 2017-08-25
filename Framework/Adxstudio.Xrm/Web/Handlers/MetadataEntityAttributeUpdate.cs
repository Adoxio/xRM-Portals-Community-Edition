/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Handlers
{
	/// <summary>
	/// Updates an attribute value for a given entity from a <see cref="JToken"/> property value, using
	/// <see cref="AttributeMetadata"/> to do the appropriate type conversions.
	/// </summary>
	internal class MetadataEntityAttributeUpdate : EntityAttributeUpdate
	{
		public MetadataEntityAttributeUpdate(Entity entity, AttributeMetadata attribute) : base(entity)
		{
			if (attribute == null)
			{
				throw new ArgumentNullException("attribute");
			}

			Attribute = attribute;
		}

		protected AttributeMetadata Attribute { get; private set; }

		public override void Apply(JToken token)
		{
			if (token == null)
			{
				throw new ArgumentNullException("token");
			}

			if (Attribute.AttributeType == null)
			{
				throw new InvalidOperationException("Unable to determine attribute type from metadata.");
			}

			var value = GetValue(token, Attribute.AttributeType.Value);

			Entity.SetAttributeValue(Attribute.LogicalName, value);
		}

		protected object GetValue(JToken token, AttributeTypeCode attributeType)
		{
			var value = token.ToObject<object>();

			if (value == null)
			{
				return null;
			}

			if (attributeType == AttributeTypeCode.Customer || attributeType == AttributeTypeCode.Lookup || attributeType == AttributeTypeCode.Owner)
			{
				EntityReference entityReference;

				if (TryGetEntityReference(value, out entityReference))
				{
					return entityReference;
				}

				throw new FormatException("Unable to convert value {0} for attribute {1} to {2}.".FormatWith(value, Attribute.LogicalName, typeof(EntityReference)));
			}

			// Option set values will be in Int64 form from the JSON deserialization -- convert those to OptionSetValues
			// for option set attributes.
			if (attributeType == AttributeTypeCode.EntityName || attributeType == AttributeTypeCode.Picklist || attributeType == AttributeTypeCode.State || attributeType == AttributeTypeCode.Status)
			{
				return new OptionSetValue(Convert.ToInt32(value));
			}

			if (attributeType == AttributeTypeCode.Memo || attributeType == AttributeTypeCode.String)
			{
				return value is string ? value : value.ToString();
			}

			if (attributeType == AttributeTypeCode.BigInt)
			{
				return Convert.ToInt64(value);
			}

			if (attributeType == AttributeTypeCode.Boolean)
			{
				return Convert.ToBoolean(value);
			}

			if (attributeType == AttributeTypeCode.DateTime)
			{
				var dateTimeValue = Convert.ToDateTime(value);

				return dateTimeValue.Kind == DateTimeKind.Utc ? dateTimeValue : dateTimeValue.ToUniversalTime();
			}

			if (attributeType == AttributeTypeCode.Decimal)
			{
				return Convert.ToDecimal(value);
			}

			if (attributeType == AttributeTypeCode.Double)
			{
				return Convert.ToDouble(value);
			}

			if (attributeType == AttributeTypeCode.Integer)
			{
				return Convert.ToInt32(value);
			}

			if (attributeType == AttributeTypeCode.Money)
			{
				return new Money(Convert.ToDecimal(value));
			}

			if (attributeType == AttributeTypeCode.Uniqueidentifier)
			{
				return value is Guid ? value : new Guid(value.ToString());
			}

			return value;
		}
	}
}
