/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xrm.Client.Runtime;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Client
{
	/// <summary>
	/// An <see cref="Entity"/> class compatible with WCF Data Services.
	/// </summary>
	[DataContract(Namespace = V5.Contracts)]
	[DataServiceKey("Id")]
	[IgnoreProperties("Item", "Attributes", "EntityState", "FormattedValues", "RelatedEntities", "ExtensionData")]
	public class CrmEntity : Entity, INotifyPropertyChanging, INotifyPropertyChanged
	{
		private OrganizationServiceContext _context;

		internal void Attach(OrganizationServiceContext context)
		{
			_context = context;
		}

		public CrmEntity(string entityName)
			: base(entityName)
		{
		}

		/// <summary>
		/// Retrieves the value of an attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="attributeLogicalName"></param>
		/// <returns></returns>
		public override T GetAttributeValue<T>(string attributeLogicalName)
		{
			return EntityExtensions.GetAttributeValue<T>(this, attributeLogicalName);
		}

		/// <summary>
		/// Retrieves the value of a sequence attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="attributeLogicalName"></param>
		/// <returns></returns>
		public virtual IEnumerable<T> GetAttributeCollectionValue<T>(string attributeLogicalName)
			where T : Entity
		{
			return EntityExtensions.GetAttributeCollectionValue<T>(this, attributeLogicalName);
		}

		/// <summary>
		/// Modifies the value of an attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyName"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="value"></param>
		/// <remarks>
		/// Raises the property change events.
		/// </remarks>
		protected virtual void SetAttributeValue<T>(string propertyName, string attributeLogicalName, object value)
		{
			SetAttributeValue<T>(propertyName, attributeLogicalName, null, value);
		}

		/// <summary>
		/// Modifies the value of an attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyName"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="entityLogicalName"></param>
		/// <param name="value"></param>
		/// <remarks>
		/// Raises the property change events.
		/// </remarks>
		protected virtual void SetAttributeValue<T>(string propertyName, string attributeLogicalName, string entityLogicalName, object value)
		{
			propertyName.ThrowOnNullOrWhitespace("propertyName");
			attributeLogicalName.ThrowOnNullOrWhitespace("attributeLogicalName");

			// check that the new value is different from the current value

			if (Equals(GetAttributeValue<object>(attributeLogicalName), value)) return;

			RaisePropertyChanging(propertyName);

			EntityExtensions.SetAttributeValue<T>(this, attributeLogicalName, entityLogicalName, value);

			RaisePropertyChanged(propertyName);
		}

		/// <summary>
		/// Modifies the value of a sequence attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyName"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="value"></param>
		/// <remarks>
		/// Raises the property change events.
		/// </remarks>
		protected virtual void SetAttributeCollectionValue<T>(string propertyName, string attributeLogicalName, IEnumerable<T> value)
			where T : Entity
		{
			propertyName.ThrowOnNullOrWhitespace("propertyName");
			attributeLogicalName.ThrowOnNullOrWhitespace("attributeLogicalName");

			// check that the new value is different from the current value

			if (Equals(GetAttributeValue<object>(attributeLogicalName), value)) return;

			RaisePropertyChanging(propertyName);

			this.SetAttributeCollectionValue(attributeLogicalName, value);

			RaisePropertyChanged(propertyName);
		}

		/// <summary>
		/// Modifies the value of a primary key attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyName"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="value"></param>
		/// <remarks>
		/// Raises the property change events.
		/// </remarks>
		protected virtual void SetPrimaryIdAttributeValue<T>(string propertyName, string attributeLogicalName, object value)
		{
			SetAttributeValue<T>(propertyName, attributeLogicalName, value);
			var guid = value as Guid?;
			var id = guid != null ? guid.Value : Guid.Empty;

			if (base.Id == id) return;

			RaisePropertyChanging("Id");

			base.Id = id;

			RaisePropertyChanged("Id");
		}

		/// <summary>
		/// Modifies a related entity for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="propertyName"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="entity"></param>
		/// <remarks>
		/// Raises the property change events.
		/// </remarks>
		protected virtual void SetRelatedEntity<TEntity>(string propertyName, string relationshipSchemaName, TEntity entity)
			where TEntity : Entity
		{
			SetRelatedEntity(propertyName, relationshipSchemaName, null, entity);
		}

		/// <summary>
		/// Modifies a related entity for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="propertyName"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="primaryEntityRole"></param>
		/// <param name="entity"></param>
		/// <remarks>
		/// Raises the property change events.
		/// </remarks>
		protected virtual void SetRelatedEntity<TEntity>(string propertyName, string relationshipSchemaName, EntityRole? primaryEntityRole, TEntity entity)
			where TEntity : Entity
		{
			propertyName.ThrowOnNullOrWhitespace("propertyName");
			relationshipSchemaName.ThrowOnNullOrWhitespace("relationshipSchemaName");

			// check that the new value is different from the current value
			// TODO: perform comparison

			RaisePropertyChanging(propertyName);

			EntityExtensions.SetRelatedEntity(this, relationshipSchemaName, primaryEntityRole, entity);

			RaisePropertyChanged(propertyName);
		}

		/// <summary>
		/// Modifies the collection of related entities for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="propertyName"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="entities"></param>
		/// <remarks>
		/// Raises the property change events.
		/// </remarks>
		protected virtual void SetRelatedEntities<TEntity>(string propertyName, string relationshipSchemaName, IEnumerable<TEntity> entities)
			where TEntity : Entity
		{
			SetRelatedEntities(propertyName, relationshipSchemaName, null, entities);
		}

		/// <summary>
		/// Modifies the collection of related entities for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="propertyName"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="primaryEntityRole"></param>
		/// <param name="entities"></param>
		/// <remarks>
		/// Raises the property change events.
		/// </remarks>
		protected virtual void SetRelatedEntities<TEntity>(string propertyName, string relationshipSchemaName, EntityRole? primaryEntityRole, IEnumerable<TEntity> entities)
			where TEntity : Entity
		{
			propertyName.ThrowOnNullOrWhitespace("propertyName");
			relationshipSchemaName.ThrowOnNullOrWhitespace("relationshipSchemaName");

			// check that the new value is different from the current value
			// TODO: perform comparison

			RaisePropertyChanging(propertyName);

			EntityExtensions.SetRelatedEntities(this, relationshipSchemaName, primaryEntityRole, entities);

			RaisePropertyChanged(propertyName);
		}

		protected override TEntity GetRelatedEntity<TEntity>(string relationshipSchemaName, EntityRole? primaryEntityRole)
		{
			return _context != null
				? this.GetRelatedEntity(_context, relationshipSchemaName, primaryEntityRole) as TEntity
				: base.GetRelatedEntity<TEntity>(relationshipSchemaName, primaryEntityRole);
		}

		protected override IEnumerable<TEntity> GetRelatedEntities<TEntity>(string relationshipSchemaName, EntityRole? primaryEntityRole)
		{
			return _context != null
				? this.GetRelatedEntities(_context, relationshipSchemaName, primaryEntityRole).Cast<TEntity>()
				: base.GetRelatedEntities<TEntity>(relationshipSchemaName, primaryEntityRole);
		}

		/// <summary>
		/// Occurs when an attribute or relationship is set.
		/// </summary>
		public event PropertyChangingEventHandler PropertyChanging;

		private void RaisePropertyChanging(string propertyName)
		{
			var handler = PropertyChanging;

			if (handler != null)
			{
				PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
			}
		}

		/// <summary>
		/// Occurs when an attribute or relationship is set.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;

			if (handler != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
