/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Holds a function for getting a value of a cell and an action for setting a value of a cell.
	/// </summary>
	public class CellBinding
	{
		/// <summary>
		/// A function to get the value of the cell.
		/// </summary>
		public Func<object> Get { get; set; }

		/// <summary>
		/// An action to set the value of the cell.
		/// </summary>
		public Action<object> Set { get; set; }
	}
}
