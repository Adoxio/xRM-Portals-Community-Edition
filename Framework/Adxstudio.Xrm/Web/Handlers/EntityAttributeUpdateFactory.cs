/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Handlers
{
	/// <summary>
	/// Factory for getting an appropriate <see cref="IEntityAttributeUpdate"/> instance for a given <see cref="OrganizationServiceContext"/>,
	/// <see cref="Entity"/>, and attribute.
	/// </summary>
	internal class EntityAttributeUpdateFactory
	{
		private readonly Func<string, string, AttributeMetadata> _getAttributeMetadata;

		public EntityAttributeUpdateFactory(Func<string, string, AttributeMetadata> getAttributeMetadata)
		{
			if (getAttributeMetadata == null)
			{
				throw new ArgumentNullException("getAttributeMetadata");
			}

			_getAttributeMetadata = getAttributeMetadata;
		}

		public IEntityAttributeUpdate Create(OrganizationServiceContext serviceContext, Entity entity, string attributeLogicalName)
		{
			EntitySetInfo entitySetInfo;
			AttributeInfo attributeInfo;

			if (OrganizationServiceContextInfo.TryGet(serviceContext, entity, out entitySetInfo)
				&& entitySetInfo.Entity.EntityType == entity.GetType()
					&& entitySetInfo.Entity.AttributesByLogicalName.TryGetValue(attributeLogicalName, out attributeInfo))
			{
				if (entitySetInfo.Entity.PrimaryKeyProperty.CrmPropertyAttribute.LogicalName == attributeInfo.CrmPropertyAttribute.LogicalName)
				{
					throw new InvalidOperationException("Can't create an update for the primary key attribute {0}.".FormatWith(attributeLogicalName));
				}

				return new ReflectionEntityAttributeUpdate(entity, attributeInfo);
			}

			var attributeMetadata = _getAttributeMetadata(entity.LogicalName, attributeLogicalName);

			if (attributeMetadata != null)
			{
				if (attributeMetadata.IsPrimaryId.GetValueOrDefault())
				{
					throw new InvalidOperationException("Can't create an update for the primary key attribute {0}.".FormatWith(attributeLogicalName));
				}

				return new MetadataEntityAttributeUpdate(entity, attributeMetadata);
			}

			ADXTrace.Instance.TraceWarning(TraceCategory.Application, @"Unable to create CMS update for entity ""{0}"", attribute ""{1}"". Discarding value.".FormatWith(entity.LogicalName, attributeLogicalName));

			return new NonexistentAttributeUpdate(entity);
		}

		private class NonexistentAttributeUpdate : EntityAttributeUpdate
		{
			public NonexistentAttributeUpdate(Entity entity) : base(entity) { }

			public override void Apply(JToken token)
			{
				// Do nothing, as we were unable to find attribute metadata for a given update.
			}
		}
	}
}
