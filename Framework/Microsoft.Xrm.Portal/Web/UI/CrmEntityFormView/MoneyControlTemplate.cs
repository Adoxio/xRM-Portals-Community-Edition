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
	public class MoneyControlTemplate : CellTemplate
	{
		public MoneyControlTemplate(ICellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings) : base(metadata, validationGroup, bindings) { }

		public override string CssClass
		{
			get { return "money"; }
		}

		protected override void InstantiateControlIn(Control container)
		{
			var textbox = new TextBox { ID = ControlID, TextMode = TextBoxMode.SingleLine, CssClass = "text " + CssClass, ToolTip = Metadata.ToolTip };

			container.Controls.Add(textbox);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					decimal value;

					return decimal.TryParse(textbox.Text, out value) ? new decimal?(value) : null;
				},
				Set = obj =>
				{
					textbox.Text = "{0}".FormatWith(obj);
				}
			};
		}

		protected override void InstantiateValidatorsIn(Control container)
		{
			var maxValue = Metadata.MaxValue.HasValue ? Metadata.MaxValue.Value : decimal.MaxValue;
			var minValue = Metadata.MinValue.HasValue ? Metadata.MinValue.Value : decimal.MinValue;

			var rangeValidator = new CustomValidator
			{
				ControlToValidate = ControlID,
				ValidationGroup = ValidationGroup,
				ErrorMessage = "{0} must have a value between {1} and {2}.".FormatWith(Metadata.Label, minValue, maxValue),
				Text = "*",
			};

			rangeValidator.ServerValidate += GetRangeValidationHandler(minValue, maxValue);

			container.Controls.Add(rangeValidator);

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

		private static ServerValidateEventHandler GetRangeValidationHandler(decimal minValue, decimal maxValue)
		{
			return (sender, args) =>
			{
				decimal value;

				args.IsValid = decimal.TryParse(args.Value, out value) && (value >= minValue) && (value <= maxValue);
			};
		}
	}
}
