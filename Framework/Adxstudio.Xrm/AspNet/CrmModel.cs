/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.AspNet
{
	public interface IModel<TKey>
	{
		Entity Entity { get; }
		TKey Id { get; set; }
	}

	public abstract class CrmModel<TKey> : IModel<TKey>
	{
		protected virtual string PrimaryNameAttribute { get; private set; }

		public virtual Entity Entity { get; private set; }

		public virtual TKey Id
		{
			get { return ToKey(Entity.Id); }
			set { Entity.Id = ToGuid(value); }
		}

		public virtual string Name
		{
			get { return Entity.GetAttributeValue<string>(PrimaryNameAttribute); }
			set { Entity.SetAttributeValue(PrimaryNameAttribute, value); }
		}

		protected CrmModel(string logicalName)
			: this(logicalName, "adx_name")
		{
		}

		protected CrmModel(string logicalName, string primaryNameAttribute)
			: this(logicalName, primaryNameAttribute, null)
		{
		}

		protected CrmModel(string logicalName, string primaryNameAttribute, Entity entity)
		{
			if (string.IsNullOrWhiteSpace(logicalName)) throw new ArgumentNullException("logicalName");
			if (string.IsNullOrWhiteSpace(primaryNameAttribute)) throw new ArgumentNullException("primaryNameAttribute");

			Entity = entity ?? new Entity(logicalName);
			PrimaryNameAttribute = primaryNameAttribute;
		}

		protected virtual IEnumerable<Entity> GetRelatedEntities(Relationship relationship)
		{
			if (!Entity.RelatedEntities.ContainsKey(relationship)) yield break;

			foreach (var entity in Entity.RelatedEntities[relationship].Entities)
			{
				yield return entity;
			}
		}

		internal virtual void SetEntity(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			Entity = entity;
		}

		protected virtual Guid ToGuid(TKey key)
		{
			return key.ToGuid();
		}

		protected virtual TKey ToKey(Guid guid)
		{
			return guid.ToKey<TKey>();
		}
	}
}
