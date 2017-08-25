/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public class BooleanControlTemplate : CellTemplate
	{
		public BooleanControlTemplate(ICellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings) : base(metadata, validationGroup, bindings) { }

		public override string CssClass
		{
			get { return "checkbox"; }
		}

		protected override void InstantiateControlIn(Control container)
		{
			var checkbox = new CheckBox { ID = ControlID, CssClass = CssClass, ToolTip = Metadata.ToolTip };

			container.Controls.Add(checkbox);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () => checkbox.Checked,
				Set = obj => { checkbox.Checked = Convert.ToBoolean(obj); }
			};
		}
	}
}
