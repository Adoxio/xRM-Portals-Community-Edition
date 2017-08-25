/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	internal class UnsupportedControlTemplate : Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.UnsupportedControlTemplate
	{
		private readonly int? _colSpan;
		private readonly int? _rowSpan;
		
		public override int? ColumnSpan
		{
		  get
		  {
			return _colSpan;
		  }
		}

		public override int? RowSpan
		{
		  get
		  {
			return _rowSpan;
		  }
		}

		public UnsupportedControlTemplate(ICellMetadata metadata, string validationGroup,
			IDictionary<string, CellBinding> bindings, bool enabled)
			: base(metadata, validationGroup, bindings, enabled)
		{
			_colSpan = metadata.ColumnSpan;
			_rowSpan = metadata.RowSpan;
		}
	}
}
