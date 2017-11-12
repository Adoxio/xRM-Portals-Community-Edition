/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Whole Number field.
	/// </summary>
	public class IntegerControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// IntegerControlTemplate class intialization.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public IntegerControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
		}

		public override string CssClass
		{
			get { return "integer form-control"; }
		}

		/// <summary>
		/// Form field.
		/// </summary>
		public CrmEntityFormViewField Field { get; private set; }

		private string ValidationText
		{
			get { return Metadata.ValidationText; }
		}

		private ValidatorDisplay ValidatorDisplay
		{
			get { return string.IsNullOrWhiteSpace(ValidationText) ? ValidatorDisplay.None : ValidatorDisplay.Dynamic; }
		}

		protected override void InstantiateControlIn(Control container)
		{
			var textbox = new TextBox { ID = ControlID, TextMode = TextBoxMode.SingleLine, CssClass = string.Join(" ", "text", CssClass, Metadata.CssClass), ToolTip = Metadata.ToolTip };

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				textbox.Attributes.Add("required", string.Empty);
			}

			if (Metadata.ReadOnly)
			{
				textbox.CssClass += " readonly";
				textbox.Attributes["readonly"] = "readonly";
			}

			textbox.Attributes["onchange"] = "setIsDirty(this.id);";

			container.Controls.Add(textbox);

			RegisterClientSideDependencies(container);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					int value;

					return int.TryParse(textbox.Text, out value) ? new int?(value) : null;
				},
				Set = obj =>
				{
					textbox.Text = string.Format("{0}", obj);
				}
			};
		}

		protected override void InstantiateValidatorsIn(Control container)
		{
			var formatValidator = new CustomValidator
			{
				ID = string.Format("IntegerValidator{0}", ControlID),
				ControlToValidate = ControlID,
				ValidationGroup = ValidationGroup,
				ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.ValidationErrorMessage) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("integer")) ? ResourceManager.GetString("Invalid_Integer_Value_Error").FormatWith(Metadata.Label) : Metadata.Messages["integer"].FormatWith(Metadata.Label) : Metadata.ValidationErrorMessage)),
				Text = "*",
			};

			formatValidator.ServerValidate += OnIntegerFormatValidate;

			container.Controls.Add(formatValidator);

			var maxValue = Metadata.MaxValue.HasValue ? Metadata.MaxValue.Value : int.MaxValue;
			var minValue = Metadata.MinValue.HasValue ? Metadata.MinValue.Value : int.MinValue;

			var rangeValidator = new CustomValidator
			{
				ID = string.Format("RangeValidator{0}", ControlID),
				ControlToValidate = ControlID,
				ValidationGroup = ValidationGroup,
				ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.RangeValidationErrorMessage) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("range")) ? ResourceManager.GetString("Invalid_Range_Error").FormatWith(Metadata.Label, minValue, maxValue) : Metadata.Messages["range"].FormatWith(Metadata.Label, minValue, maxValue) : Metadata.RangeValidationErrorMessage)),
				Text = "*",
			};

			rangeValidator.ServerValidate += GetRangeValidationHandler(minValue, maxValue);

			container.Controls.Add(rangeValidator);

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				container.Controls.Add(new RequiredFieldValidator
				{
					ID = string.Format("RequiredFieldValidator{0}", ControlID),
					ControlToValidate = ControlID,
					ValidationGroup = ValidationGroup,
					Display = ValidatorDisplay,
					ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.RequiredFieldValidationErrorMessage) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("required")) ? ResourceManager.GetString("Required_Field_Error").FormatWith(Metadata.Label) : Metadata.Messages["required"].FormatWith(Metadata.Label) : Metadata.RequiredFieldValidationErrorMessage)),
					Text = ValidationText,
				});
			}

			if (!string.IsNullOrWhiteSpace(Metadata.ValidationRegularExpression))
			{
				container.Controls.Add(new RegularExpressionValidator
				{
					ID = string.Format("RegularExpressionValidator{0}", ControlID),
					ControlToValidate = ControlID,
					ValidationGroup = ValidationGroup,
					ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.ValidationRegularExpressionErrorMessage) ? ResourceManager.GetString("Invalid_Error").FormatWith(Metadata.Label) : Metadata.ValidationRegularExpressionErrorMessage)),
					Text = "*",
					ValidationExpression = Metadata.ValidationRegularExpression
				});
			}

			this.InstantiateCustomValidatorsIn(container);
		}

		private static ServerValidateEventHandler GetRangeValidationHandler(decimal minValue, decimal maxValue)
		{
			return (sender, args) =>
			{
				int value;

				args.IsValid = int.TryParse(args.Value, out value) && (value >= minValue) && (value <= maxValue);
			};
		}

		private static void OnIntegerFormatValidate(object source, ServerValidateEventArgs args)
		{
			int value;

			args.IsValid = int.TryParse(args.Value, out value);
		}
	}
}
