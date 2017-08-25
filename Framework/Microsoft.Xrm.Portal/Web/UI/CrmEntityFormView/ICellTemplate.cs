/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public interface ICellTemplate : ITemplate
	{
		int? ColumnSpan { get; }

		string CssClass { get; }

		bool Enabled { get; }

		int? RowSpan { get; }
	}
}
