/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Adxstudio.Xrm.Web.UI.EntityForm
{
	/// <summary>
	/// Event arguments passed to the saving event.
	/// </summary>
	public class EntityFormSavingEventArgs : CancelEventArgs
	{
		/// <summary>
		/// EntityFormSavingEventArgs Class Initialization.
		/// </summary>
		/// <param name="values">Dictionary of keys and values being saved.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public EntityFormSavingEventArgs(IDictionary<string, object> values)
		{
			if (values == null) throw new ArgumentNullException("values");

			Values = values;
		}

		/// <summary>
		/// Values assigned to the key to be updated or inserted.
		/// </summary>
		public IDictionary<string, object> Values { get; set; }
	}
}
