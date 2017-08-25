/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Client.Reflection;
using Microsoft.Xrm.Client.Runtime;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Client.Metadata
{
	/// <summary>
	/// A description of a custom <see cref="Entity"/> class.
	/// </summary>
	public sealed class EntityInfo
	{
		/// <summary>
		/// The type of the <see cref="Entity"/>.
		/// </summary>
		public Type EntityType { get; private set; }

		/// <summary>
		/// The entity set annotation of the <see cref="Entity"/>.
		/// </summary>
		public EntityAttribute Entity { get; private set; }

		/// <summary>
		/// The logical name annotation of the <see cref="Entity"/>.
		/// </summary>
		public EntityLogicalNameAttribute EntityLogicalName { get; private set; }

		public EntityInfo(Type entityType)
		{
			entityType.ThrowOnNull("entityType");

			EntityType = entityType;
			Entity = entityType.GetFirstOrDefaultCustomAttribute<EntityAttribute>();
			EntityLogicalName = entityType.GetFirstOrDefaultCustomAttribute<EntityLogicalNameAttribute>();

			_primaryKeyProperty = new Lazy<AttributeInfo>(LoadPrimaryKeyProperty);

			_attributesByLogicalName = GetLoadProperties((pi, attribute) => attribute.LogicalName);
			_attributesByPropertyName = GetLoadProperties((pi, attribute) => pi.Name);

			_relationshipsBySchemaName = GetLoadAssociations((pi, rel) => rel.SchemaName.ToRelationship(rel.PrimaryEntityRole));
			_relationshipsByPropertyName = GetLoadAssociations((pi, rel) => pi.Name);
		}

		private readonly Lazy<IDictionary<string, AttributeInfo>> _attributesByLogicalName;

		/// <summary>
		/// A lookup of <see cref="AttributeInfo"/> keyed by the attribute logical name.
		/// </summary>
		public IDictionary<string, AttributeInfo> AttributesByLogicalName
		{
			get { return _attributesByLogicalName.Value; }
		}

		private readonly Lazy<IDictionary<string, AttributeInfo>> _attributesByPropertyName;

		/// <summary>
		/// A lookup of <see cref="AttributeInfo"/> keyed by the attribute property name.
		/// </summary>
		public IDictionary<string, AttributeInfo> AttributesByPropertyName
		{
			get { return _attributesByPropertyName.Value; }
		}

		private Lazy<IDictionary<string, AttributeInfo>> GetLoadProperties(Func<PropertyInfo, AttributeLogicalNameAttribute, string> keySelector)
		{
			return new Lazy<IDictionary<string, AttributeInfo>>(() => LoadProperties(keySelector));
		}

		private IDictionary<string, AttributeInfo> LoadProperties(Func<PropertyInfo, AttributeLogicalNameAttribute, string> keySelector)
		{
			var properties = new Dictionary<string, AttributeInfo>();

			foreach (var pi in EntityType.GetProperties())
			{
				var attribute = pi.GetFirstOrDefaultCustomAttribute<AttributeLogicalNameAttribute>();
				var relationship = pi.GetFirstOrDefaultCustomAttribute<RelationshipSchemaNameAttribute>();

				if (attribute != null && pi.Name != "Id" && relationship == null)
				{
					properties.Add(keySelector(pi, attribute), new AttributeInfo(pi, attribute));
				}
			}

			return properties;
		}

		private readonly Lazy<AttributeInfo> _primaryKeyProperty;

		/// <summary>
		/// The <see cref="AttributeInfo"/> of the primary key property.
		/// </summary>
		public AttributeInfo PrimaryKeyProperty
		{
			get { return _primaryKeyProperty.Value; }
		}

		private AttributeInfo LoadPrimaryKeyProperty()
		{
			var dataServiceKey = EntityType.GetFirstOrDefaultCustomAttribute<DataServiceKeyAttribute>();
			var property = EntityType.GetProperty(dataServiceKey.KeyNames.First());

			var crmPropertyAttribute = property.GetFirstOrDefaultCustomAttribute<AttributeLogicalNameAttribute>();
			var pi = new AttributeInfo(property, crmPropertyAttribute);

			return pi;
		}

		private readonly Lazy<IDictionary<Relationship, RelationshipInfo>> _relationshipsBySchemaName;

		/// <summary>
		/// A lookup of <see cref="RelationshipInfo"/> keyed by the relationship schema name.
		/// </summary>
		public IDictionary<Relationship, RelationshipInfo> RelationshipsBySchemaName
		{
			get { return _relationshipsBySchemaName.Value; }
		}

		private readonly Lazy<IDictionary<string, RelationshipInfo>> _relationshipsByPropertyName;

		/// <summary>
		/// A lookup of <see cref="RelationshipInfo"/> keyed by the relationship property name.
		/// </summary>
		public IDictionary<string, RelationshipInfo> RelationshipsByPropertyName
		{
			get { return _relationshipsByPropertyName.Value; }
		}

		private Lazy<IDictionary<TKey, RelationshipInfo>> GetLoadAssociations<TKey>(Func<PropertyInfo, RelationshipSchemaNameAttribute, TKey> keySelector)
		{
			return new Lazy<IDictionary<TKey, RelationshipInfo>>(() => LoadAssociations(keySelector));
		}

		private IDictionary<TKey, RelationshipInfo> LoadAssociations<TKey>(Func<PropertyInfo, RelationshipSchemaNameAttribute, TKey> keySelector)
		{
			var associations = new Dictionary<TKey, RelationshipInfo>();

			foreach (var pi in EntityType.GetProperties())
			{
				var association = pi.GetFirstOrDefaultCustomAttribute<RelationshipSchemaNameAttribute>();

				if (association != null)
				{
					associations.Add(keySelector(pi, association), new RelationshipInfo(pi, association));
				}
			}

			return associations;
		}
	}
}
