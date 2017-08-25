/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	/// <summary>
	/// Display name and width(pixels) of an attribute's column.
	/// </summary>
	public class ViewColumn : IViewColumn
	{
		/// <summary>
		/// Logical name of the attribute.
		/// </summary>
		public string AttributeLogicalName { get; set; }
		/// <summary>
		/// Display name.
		/// </summary>
		public string DisplayName { get; set; }
		/// <summary>
		/// Width of the column in pixels.
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Parameterless constructor
		/// </summary>
		public ViewColumn()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="attributeLogicalName"></param>
		/// <param name="displayName"></param>
		/// <param name="width"></param>
		public ViewColumn(string attributeLogicalName, string displayName, int width)
		{
			AttributeLogicalName = attributeLogicalName;
			DisplayName = displayName;
			Width = width;
		}
	}
}
