/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Security
{
	/// <summary>
	/// Permission record that defines the relationships and privileges on a given entity type for record level security.
	/// </summary>
	public class EntityPermission : IEntityPermission
	{
		/// <summary>
		/// Entity Reference <see cref="EntityReference"/>
		/// </summary>
		public EntityReference EntityReference { get; private set; }
		/// <summary>
		/// Name/Description of the permission
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Logical name of the entity
		/// </summary>
		public string EntityName { get; private set; }
		/// <summary>
		/// Scope of the permissions
		/// </summary>
		public EntityPermissionScope? Scope { get; private set; }
		/// <summary>
		/// Indicates whether read privilege is granted or not
		/// </summary>
		public bool Read { get; private set; }
		/// <summary>
		/// Indicates whether write privilege is granted or not
		/// </summary>
		public bool Write { get; private set; }
		/// <summary>
		/// Indicates whether create privilege is granted or not
		/// </summary>
		public bool Create { get; private set; }
		/// <summary>
		/// Indicates whether delete privilege is granted or not
		/// </summary>
		public bool Delete { get; private set; }
		/// <summary>
		/// Indicates whether append privilege is granted or not.
		/// </summary>
		public bool Append { get; private set; }
		/// <summary>
		/// Indicates whether append to privilege is granted or not.
		/// </summary>
		public bool AppendTo { get; private set; }
		/// <summary>
		/// If scope <see cref="Scope"/> is "Account" this is the schema name of the relationship between account and the entity. If scope is "Global" or "Parent" this value is ignored.
		/// </summary>
		public string AccountRelationshipName { get; private set; }
		/// <summary>
		/// If scope <see cref="Scope"/> is "Contact" this is the schema name of the relationship between contact and the entity. If scope is "Global" or "Parent" this value is ignored.
		/// </summary>
		public string ContactRelationshipName { get; private set; }
		/// <summary>
		/// The entity reference to the parent permission. <see cref="EntityReference"/>
		/// </summary>
		public EntityReference ParentEntityPermission { get; private set; }
		/// <summary>
		/// If scope <see cref="Scope"/> is "Parent" this is the schema name of the relationship between the parent entity permission's entity and this permission's entity. If scope is "Global" or "Contact" or "Account" this value is ignored.
		/// </summary>
		public string ParentRelationshipName { get; private set; }
		/// <summary>
		/// Web Roles associated with the permission
		/// </summary>
		public IEnumerable<Entity> WebRoles { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public EntityPermission()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="entity"><see cref="Entity"/></param>
		/// <exception cref="ArgumentNullException"></exception>
		public EntityPermission(Entity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			entity.AssertEntityName("adx_entitypermission");

			EntityReference = entity.ToEntityReference();

			Name = entity.GetAttributeValue<string>("adx_entityname");
			EntityName = entity.GetAttributeValue<string>("adx_entitylogicalname");

			Read = entity.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false);
			Write = entity.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false);
			Create = entity.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false);
			Delete = entity.GetAttributeValue<bool?>("adx_delete").GetValueOrDefault(false);
			Append = entity.GetAttributeValue<bool?>("adx_append").GetValueOrDefault(false);
			AppendTo = entity.GetAttributeValue<bool?>("adx_appendto").GetValueOrDefault(false);

			Scope = null;

			var scopeOption = entity.GetAttributeValue<OptionSetValue>("adx_scope");

			if (scopeOption != null)
			{
				foreach (var scope in Enum.GetValues(typeof(EntityPermissionScope)).Cast<EntityPermissionScope>().Where(scope => (int)scope == scopeOption.Value))
				{
					Scope = scope;
				}
			}

			AccountRelationshipName = entity.GetAttributeValue<string>("adx_accountrelationship");
			ContactRelationshipName = entity.GetAttributeValue<string>("adx_contactrelationship");
			ParentRelationshipName = entity.GetAttributeValue<string>("adx_parentrelationship");
			ParentEntityPermission = entity.GetAttributeValue<EntityReference>("adx_parententitypermission");
		}
	}
}
