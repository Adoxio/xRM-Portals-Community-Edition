/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public sealed class EmptyCellTemplate : ICellTemplate
	{
		public int? ColumnSpan
		{
			get { return null; }
		}

		public string CssClass
		{
			get { return "empty"; }
		}

		public bool Enabled
		{
			get { return true; }
		}

		public int? RowSpan
		{
			get { return null; }
		}

		public void InstantiateIn(Control container)
		{
		}
	}
}
