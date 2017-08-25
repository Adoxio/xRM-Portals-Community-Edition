/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	class EmptyCellTemplate : ICellTemplate
	{
		public EmptyCellTemplate(ICellMetadata cellMetadata)
		{
			ColumnSpan = cellMetadata.ColumnSpan;
			RowSpan = cellMetadata.RowSpan;
			Enabled = true;
		}

		public void InstantiateIn(Control container)
		{
			
		}

		public int? ColumnSpan { get; private set; }
		public string CssClass { get; private set; }
		public bool Enabled { get; private set; }
		public int? RowSpan { get; private set; }
	}
}
