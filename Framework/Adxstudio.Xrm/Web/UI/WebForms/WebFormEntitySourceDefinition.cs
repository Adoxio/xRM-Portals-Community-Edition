/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Web.UI.WebForms
{
	/// <summary>
	/// Class to define the source entity target of the Web Form.
	/// </summary>
	public class WebFormEntitySourceDefinition
	{

		/// <summary>
		/// WebFormEntitySourceDefinition class initialization.
		/// </summary>
		/// <param name="logicalName">Logical name of the entity.</param>
		/// <param name="primaryKeyLogicalName">Primary Key of the entity.</param>
		/// <param name="id">Unique identifier of the entity record.</param>
		public WebFormEntitySourceDefinition(string logicalName, string primaryKeyLogicalName, Guid id)
		{
			LogicalName = logicalName;
			PrimaryKeyLogicalName = primaryKeyLogicalName;
			ID = id;
		}

		/// <summary>
		/// WebFormEntitySourceDefinition class initialization.
		/// </summary>
		/// <param name="logicalName">Logical name of the entity.</param>
		/// <param name="primaryKeyLogicalName">Primary Key of the entity.</param>
		/// <param name="id">Unique identifier of the entity record.</param>
		public WebFormEntitySourceDefinition(string logicalName, string primaryKeyLogicalName, string id)
		{
			LogicalName = logicalName;
			PrimaryKeyLogicalName = primaryKeyLogicalName;
			Guid guid;
			if (!Guid.TryParse(id, out guid)) { }
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
		public Guid ID { get; private set; }
	}

	public enum WebFormStepMode
	{
		Insert = 100000000,
		Edit = 100000001,
		ReadOnly = 100000002
	}

	public enum WebFormStepSourceType
	{
		QueryString = 100000001,
		CurrentPortalUser = 100000002,
		ResultFromPreviousStep = 100000003
	}

	public enum WebFormStepType
	{
		Condition = 100000000,
		LoadForm = 100000001,
		LoadTab = 100000002,
		Redirect = 100000003,
		LoadUserControl = 100000004
	}

	public enum WebFormProgressPosition
	{
		Top = 756150000,
		Bottom = 756150001,
		Left = 756150002,
		Right = 756150003
	}
}
