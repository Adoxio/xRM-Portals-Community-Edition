/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Event arguments passed to the updating event.
	/// </summary>
	public class CrmEntityFormViewUpdatingEventArgs : CancelEventArgs
	{
		public CrmEntityFormViewUpdatingEventArgs() { }

		/// <summary>
		/// CrmEntityFormViewUpdatingEventArgs Class Initialization.
		/// </summary>
		/// <param name="values"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public CrmEntityFormViewUpdatingEventArgs(IDictionary<string, object> values)
		{
			if (values == null) throw new ArgumentNullException("values");

			Values = values;
		}

		/// <summary>
		/// Values assigned to the key to be updated.
		/// </summary>
		public IDictionary<string, object> Values { get; set; }
	}
}
