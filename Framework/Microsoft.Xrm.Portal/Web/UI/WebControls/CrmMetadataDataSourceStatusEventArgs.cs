/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Security.Permissions;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CrmMetadataDataSourceStatusEventArgs : EventArgs
	{
		public CrmMetadataDataSourceStatusEventArgs(int rowsAffected, Exception exception)
		{
			_rowsAffected = rowsAffected;
			_exception = exception;
			_exceptionHandled = true;
		}

		private readonly int _rowsAffected;

		public int RowsAffected
		{
			get { return _rowsAffected; }
		}

		private readonly Exception _exception;

		public Exception Exception
		{
			get { return _exception; }
		}

		private bool _exceptionHandled;

		public bool ExceptionHandled
		{
			get { return _exceptionHandled; }
			set { _exceptionHandled = value; }
		}
	}
}
