/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	/// <summary>
	///  Override the column display name and width.
	/// </summary>
	public interface IViewColumn
	{
		/// <summary>
		/// Logical name of the attribute.
		/// </summary>
		string AttributeLogicalName { get; set; }
		/// <summary>
		/// Display name.
		/// </summary>
		string DisplayName { get; set; }
		/// <summary>
		/// Width of the column in pixels.
		/// </summary>
		int Width { get; set; }
	}
}
