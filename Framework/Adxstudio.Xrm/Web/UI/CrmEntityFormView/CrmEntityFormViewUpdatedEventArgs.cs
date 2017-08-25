/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Event arguments passed to the updated event.
	/// </summary>
	public class CrmEntityFormViewUpdatedEventArgs : EventArgs
	{
		/// <summary>
		/// The target entity updated
		/// </summary>
		public Entity Entity { get; set; }

		/// <summary>
		/// Errors occuring during update.
		/// </summary>
		public Exception Exception { get; set; }

		/// <summary>
		/// Indicates if the exception was handled.
		/// </summary>
		public bool ExceptionHandled { get; set; }
	}
}
