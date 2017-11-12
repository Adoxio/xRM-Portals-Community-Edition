/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Decimal Number field.
	/// </summary>
	public class DecimalControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// DecimalControlTemplate class initialization.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public DecimalControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
		}

		public override string CssClass
		{
			get { return "decimal form-control"; }
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
					decimal value;

					if (decimal.TryParse(textbox.Text, out value))
					{
						if (Metadata.Precision != null)
						{
							value = decimal.Round(value, (int)Metadata.Precision);
						}
						return value;
					}
					return null;
				},
				Set = obj =>
				{
					var value = (decimal)obj;

					textbox.Text = value.ToString("N{0}".FormatWith(Metadata.Precision.GetValueOrDefault(2)));
				}
			};
		}

		protected override void InstantiateValidatorsIn(Control container)
		{
			var floatValidator = new CustomValidator
			{
				ID = string.Format("FloatValidator{0}", ControlID),
				ControlToValidate = ControlID,
				ValidationGroup = ValidationGroup,
				ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.ValidationErrorMessage) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("decimal")) ? ResourceManager.GetString("Invalid_Decimal_Value_Error").FormatWith(Metadata.Label) : Metadata.Messages["decimal"].FormatWith(Metadata.Label) : Metadata.ValidationErrorMessage)),
				Text = "*",
			};

			floatValidator.ServerValidate += OnDecimalFormatValidate;

			container.Controls.Add(floatValidator);

			var maxValue = Metadata.MaxValue.HasValue ? Convert.ToDecimal(Metadata.MaxValue.Value) : decimal.MaxValue;
			var minValue = Metadata.MinValue.HasValue ? Convert.ToDecimal(Metadata.MinValue.Value) : decimal.MinValue;

			var rangeValidator = new CustomValidator
			{
				ID = string.Format("RangeValidator{0}", ControlID),
				ControlToValidate = ControlID,
				ValidationGroup = ValidationGroup,
				ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.RangeValidationErrorMessage) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("range")) ? ResourceManager.GetString("Invalid_Range_Error").FormatWith(Metadata.Label, minValue.ToString("N{0}".FormatWith(Metadata.Precision.GetValueOrDefault(2))), maxValue.ToString("N{0}".FormatWith(Metadata.Precision.GetValueOrDefault(2)))) : Metadata.Messages["range"].FormatWith(Metadata.Label, minValue.ToString("N{0}".FormatWith(Metadata.Precision.GetValueOrDefault(2))), maxValue.ToString("N{0}".FormatWith(Metadata.Precision.GetValueOrDefault(2)))) : Metadata.RangeValidationErrorMessage)),
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
				decimal value;

				args.IsValid = decimal.TryParse(args.Value, out value) && (value >= minValue) && (value <= maxValue);
			};
		}

		private static void OnDecimalFormatValidate(object source, ServerValidateEventArgs args)
		{
			decimal value;

			args.IsValid = decimal.TryParse(args.Value, out value);
		}

	}
}
