/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Handlers
{
	/// <summary>
	/// Abstract base class for an update to an attribute value for a given entity from a
	/// <see cref="JToken"/> property value.
	/// </summary>
	internal abstract class EntityAttributeUpdate : IEntityAttributeUpdate
	{
		protected EntityAttributeUpdate(Entity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			Entity = entity;
		}

		protected Entity Entity { get; private set; }

		public abstract void Apply(JToken token);

		protected bool TryGetEntityReference(object value, out EntityReference entityReference)
		{
			entityReference = value as EntityReference;

			if (entityReference != null)
			{
				return true;
			}

			var crmEntityReference = value as CrmEntityReference;

			if (crmEntityReference != null)
			{
				entityReference = new EntityReference(crmEntityReference.LogicalName, crmEntityReference.Id);

				return true;
			}

			var jobject = value as JObject;

			if (jobject != null)
			{
				var idProperty = jobject.Property("Id");
				var logicalNameProperty = jobject.Property("LogicalName");

				if (idProperty == null || logicalNameProperty == null)
				{
					return false;
				}

				Guid id;

				if (idProperty.Value == null || !Guid.TryParse(idProperty.Value.ToString(), out id))
				{
					return false;
				}

				if (logicalNameProperty.Value == null || string.IsNullOrWhiteSpace(logicalNameProperty.Value.ToString()))
				{
					return false;
				}

				entityReference = new EntityReference(logicalNameProperty.Value.ToString(), id);

				return true;
			}

			return false;
		}
	}
}
