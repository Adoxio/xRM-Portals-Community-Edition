/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using System;

	/// <summary>
	/// EntityNodeColumn class to represent a column.
	/// </summary>
	public class EntityNodeColumn
	{
		/// <summary>
		/// CRM logical name of the column
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Introduced version of the column.
		/// </summary>
		public Version IntroducedVersion { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref = "EntityNodeColumn" /> class
		/// </summary>
		/// <param name="name">Name of the column</param>
		/// <param name="version">Introduced version of the column</param>
		public EntityNodeColumn(string name, Version version)
		{
			this.Name = name;
			this.IntroducedVersion = version;
		}
	}
}
