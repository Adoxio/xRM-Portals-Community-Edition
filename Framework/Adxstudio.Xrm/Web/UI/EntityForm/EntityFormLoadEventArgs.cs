/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Web.UI.EntityForm
{
	/// <summary>
	/// Event arguments passed to the load event.
	/// </summary>
	public class EntityFormLoadEventArgs : EventArgs
	{
		/// <summary>
		/// EntityFormLoadEventArgs class initialization.
		/// </summary>
		/// <param name="entityDefinition">Entity details that include id, logical name, primary key name.</param>
		public EntityFormLoadEventArgs(FormEntitySourceDefinition entityDefinition)
		{
			if (entityDefinition == null)
			{
				return;
			}

			EntityLogicalName = entityDefinition.LogicalName;
			EntityPrimaryKeyLogicalName = entityDefinition.PrimaryKeyLogicalName;
			EntityID = entityDefinition.ID;
		}

		/// <summary>
		/// Logical name of the entity.
		/// </summary>
		public string EntityLogicalName { get; private set; }

		/// <summary>
		/// Logical name of the Primary Key attribute of the entity.
		/// </summary>
		public string EntityPrimaryKeyLogicalName { get; private set; }

		/// <summary>
		/// Unique identitifier of the entity record.
		/// </summary>
		public Guid EntityID { get; private set; }
	}
}
