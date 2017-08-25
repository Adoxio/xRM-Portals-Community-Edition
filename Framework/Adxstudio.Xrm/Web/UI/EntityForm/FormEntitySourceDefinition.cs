/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Web.UI.EntityForm
{
	/// <summary>
	/// Class to define the source entity target of the Entity Form.
	/// </summary>
	[Serializable]
	public class FormEntitySourceDefinition
	{
		/// <summary>
		/// FormEntitySourceDefinition class initialization.
		/// </summary>
		/// <param name="logicalName">Logical name of the entity.</param>
		/// <param name="primaryKeyLogicalName">Primary Key of the entity.</param>
		/// <param name="id">Unique identifier of the entity record.</param>
		public FormEntitySourceDefinition(string logicalName, string primaryKeyLogicalName, Guid id)
		{
			LogicalName = logicalName;
			PrimaryKeyLogicalName = primaryKeyLogicalName;
			ID = id;
		}

		/// <summary>
		/// FormEntitySourceDefinition class initialization.
		/// </summary>
		/// <param name="logicalName">Logical name of the entity.</param>
		/// <param name="primaryKeyLogicalName">Primary Key of the entity.</param>
		/// <param name="id">Unique identifier of the entity record.</param>
		public FormEntitySourceDefinition(string logicalName, string primaryKeyLogicalName, string id)
		{
			LogicalName = logicalName;
			PrimaryKeyLogicalName = primaryKeyLogicalName;
			Guid guid;
			if (!Guid.TryParse(id, out guid))
			{

			}
			ID = guid;
		}

		/// <summary>
		/// Logical name of the entity.
		/// </summary>
		public string LogicalName { get; private set; }

		/// <summary>
		/// Logical name of the Primary Key attribute of the entity.
		/// </summary>
		public string PrimaryKeyLogicalName { get; private set; }

		/// <summary>
		/// Unique identitifier of the entity record.
		/// </summary>
		public Guid ID { get; set; }
	}
}
