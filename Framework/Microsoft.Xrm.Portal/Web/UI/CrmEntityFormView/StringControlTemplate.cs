/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public class StringControlTemplate : CellTemplate
	{
		public StringControlTemplate(ICellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings) : base(metadata, validationGroup, bindings) { }

		public override string CssClass
		{
			get { return "text"; }
		}

		protected override void InstantiateControlIn(Control container)
		{
			var textbox = new TextBox { ID = ControlID, CssClass = CssClass, ToolTip = Metadata.ToolTip };

			container.Controls.Add(textbox);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () => textbox.Text,
				Set = obj => { textbox.Text = obj as string; }
			};
		}

		protected override void InstantiateValidatorsIn(Control container)
		{
			if (Metadata.IsRequired)
			{
				container.Controls.Add(new RequiredFieldValidator
				{
					ControlToValidate = ControlID,
					ValidationGroup = ValidationGroup,
					ErrorMessage = "{0} is a required field.".FormatWith(Metadata.Label),
					Text = "*",
				});
			}
		}
	}
}
