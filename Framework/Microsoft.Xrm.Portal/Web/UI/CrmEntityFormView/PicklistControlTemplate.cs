/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using Microsoft.Xrm.Client;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public class PicklistControlTemplate : CellTemplate
	{
		public PicklistControlTemplate(ICellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings) : base(metadata, validationGroup, bindings) { }

		public override string CssClass
		{
			get { return "picklist"; }
		}

		protected override void InstantiateControlIn(Control container)
		{
			var dropDown = new DropDownList { ID = ControlID, ToolTip = Metadata.ToolTip };

			container.Controls.Add(dropDown);

			dropDown.Items.Clear();

			if (!Metadata.IsRequired)
			{
				dropDown.Items.Add(new ListItem(string.Empty, string.Empty));
			}

			foreach (var option in Metadata.PicklistOptions)
			{
				dropDown.Items.Add(new ListItem
				{
					Value = option.Value.Value.ToString(CultureInfo.InvariantCulture),
					Text = option.Label.UserLocalizedLabel.Label,
				});
			}

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					int value;

					return int.TryParse(dropDown.SelectedValue, out value) ? new int?(value) : null;
				},
				Set = obj =>
				{
					dropDown.Text = "{0}".FormatWith(obj);
				}
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
