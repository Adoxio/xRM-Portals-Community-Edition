/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.Handlers
{
	public class CmsEntityMetadata
	{
		private readonly HashSet<string> _attributeLogicalNames;
		private readonly Lazy<EntityMetadata> _entityMetadata;
		private readonly Dictionary<Relationship, CmsEntityRelationshipInfo> _relationships;

		public CmsEntityMetadata(OrganizationServiceContext serviceContext, string logicalName)
		{
			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (string.IsNullOrWhiteSpace(logicalName))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "logicalName");
			}

			_entityMetadata = new Lazy<EntityMetadata>(() => GetEntityMetadata(serviceContext, logicalName));

			PrimaryIdAttribute = _entityMetadata.Value.PrimaryIdAttribute;
			_attributeLogicalNames = new HashSet<string>(_entityMetadata.Value.Attributes.Select(a => a.LogicalName));
			_relationships = new Dictionary<Relationship, CmsEntityRelationshipInfo>();

			foreach (var relationshipMetadata in _entityMetadata.Value.ManyToOneRelationships)
			{
				var relationship = new Relationship(relationshipMetadata.SchemaName)
				{
					PrimaryEntityRole = relationshipMetadata.ReferencedEntity == relationshipMetadata.ReferencingEntity
						? new EntityRole?(EntityRole.Referencing)
						: null
				};

				_relationships[relationship] = new CmsEntityRelationshipInfo(relationship, false, relationshipMetadata.ReferencedEntity, relationshipMetadata.ReferencingEntity);
			}

			foreach (var relationshipMetadata in _entityMetadata.Value.OneToManyRelationships)
			{
				var relationship = new Relationship(relationshipMetadata.SchemaName)
				{
					PrimaryEntityRole = relationshipMetadata.ReferencedEntity == relationshipMetadata.ReferencingEntity
						? new EntityRole?(EntityRole.Referenced)
						: null
				};

				_relationships[relationship] = new CmsEntityRelationshipInfo(relationship, true, relationshipMetadata.ReferencedEntity, relationshipMetadata.ReferencingEntity);
			}

			LogicalName = logicalName;
		}

		public IEnumerable<string> Attributes
		{
			get { return _attributeLogicalNames.OrderBy(a => a).ToArray(); }
		}

		public string LogicalName { get; private set; }

		public string PrimaryIdAttribute { get; private set; }

		public string PrimaryNameAttribute
		{
			get
			{
				// Optimization for adx_ entities, since they all use "adx_name" as their primary name attribute.
				return HasAttribute("adx_name") ? "adx_name" : _entityMetadata.Value.PrimaryNameAttribute;
			}
		}

		public IEnumerable<Relationship> Relationships
		{
			get { return _relationships.Keys.OrderBy(r => r.ToSchemaName(".")).ToArray(); }
		}

		public bool HasAttribute(string attributeLogicalName)
		{
			return _attributeLogicalNames.Contains(attributeLogicalName);
		}

		public bool TryGetAttributeMetadata(string attributeLogicalName, out AttributeMetadata attributeMetadata)
		{
			attributeMetadata = _entityMetadata.Value.Attributes.FirstOrDefault(a => a.LogicalName == attributeLogicalName);

			return attributeMetadata != null;
		}

		public bool TryGetAttributeType(string attributeLogicalName, out AttributeTypeCode attributeType)
		{
			attributeType = AttributeTypeCode.Virtual;

			var attributeMetadata = _entityMetadata.Value.Attributes.FirstOrDefault(a => a.LogicalName == attributeLogicalName);

			if (attributeMetadata == null || attributeMetadata.AttributeType == null)
			{
				return false;
			}

			attributeType = attributeMetadata.AttributeType.Value;

			return true;
		}

		public bool TryGetManyToOneRelationshipMetadata(Relationship relationship, out OneToManyRelationshipMetadata relationshipMetadata)
		{
			if (relationship == null)
			{
				throw new ArgumentNullException("relationship");
			}

			if (relationship.PrimaryEntityRole.HasValue && relationship.PrimaryEntityRole.Value == EntityRole.Referenced)
			{
				throw new ArgumentException("Primary entity role on many-to-one relationship must be Referencing, not Referenced.", "relationship");
			}

			relationshipMetadata = _entityMetadata.Value.ManyToOneRelationships
				.FirstOrDefault(r => r.SchemaName == relationship.SchemaName);

			return relationshipMetadata != null;
		}

		public bool TryGetOneToManyRelationshipMetadata(Relationship relationship, out OneToManyRelationshipMetadata relationshipMetadata)
		{
			if (relationship == null)
			{
				throw new ArgumentNullException("relationship");
			}

			if (relationship.PrimaryEntityRole.HasValue && relationship.PrimaryEntityRole.Value == EntityRole.Referencing)
			{
				throw new ArgumentException("Primary entity role on many-to-one relationship must be Referencing, not Referenced.", "relationship");
			}

			relationshipMetadata = _entityMetadata.Value.OneToManyRelationships
				.FirstOrDefault(r => r.SchemaName == relationship.SchemaName);

			return relationshipMetadata != null;
		}

		public bool TryGetRelationshipInfo(Relationship relationship, out CmsEntityRelationshipInfo relationshipInfo)
		{
			return _relationships.TryGetValue(relationship, out relationshipInfo);
		}

		private static EntityMetadata GetEntityMetadata(OrganizationServiceContext serviceContext, string logicalName)
		{
			var response = (RetrieveEntityResponse)serviceContext.Execute(new RetrieveEntityRequest
			{
				LogicalName = logicalName, EntityFilters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships
			});

			return response.EntityMetadata;
		}
	}
}
