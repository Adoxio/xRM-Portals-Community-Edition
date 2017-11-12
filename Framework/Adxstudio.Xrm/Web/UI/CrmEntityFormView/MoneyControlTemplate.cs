/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Metadata;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Configuration;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Currency field.
	/// </summary>
	public class MoneyControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// MoneyControlTemplate class initialization.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public MoneyControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings, string contextName)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
			ContextName = contextName;
		}

		public override string CssClass
		{
			get { return "money form-control"; }
		}

		/// <summary>
		/// Form field.
		/// </summary>
		public CrmEntityFormViewField Field { get; private set; }

		private string ContextName { get; set; }

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

			var inputGroup = new HtmlGenericControl("div");
			var inputGroupAddonFirst = new HtmlGenericControl("span") { Visible = false };
			var inputGroupAddonLast = new HtmlGenericControl("span") { Visible = false };

			inputGroupAddonFirst.Attributes["class"] = "input-group-addon";
			inputGroupAddonLast.Attributes["class"] = "input-group-addon";

			inputGroup.Controls.Add(inputGroupAddonFirst);
			inputGroup.Controls.Add(textbox);
			inputGroup.Controls.Add(inputGroupAddonLast);

			container.Controls.Add(inputGroup);

			RegisterClientSideDependencies(container);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					decimal value;

					return decimal.TryParse(textbox.Text, out value) ? new decimal?(value) : null;
				},
				Set = obj =>
				{
					var data = obj as Tuple<Entity, Money>;

					if (data == null)
					{
						textbox.Text = ((Money)obj).Value.ToString("N{0}".FormatWith(Metadata.Precision));

						return;
					}

					using (var serviceContext = CrmConfigurationManager.CreateContext(ContextName))
					{
						var moneyFormatter = new MoneyFormatter(
							new OrganizationMoneyFormatInfo(serviceContext),
							new EntityRecordMoneyFormatInfo(serviceContext, data.Item1),
							Metadata.Precision,
							Metadata.PrecisionSource,
							Metadata.IsBaseCurrency);

						// Include the currency symbol in the field value if it's read-only (for nicer display, as
						// it won't need to be parsed later.
						textbox.Text = string.Format(moneyFormatter, Metadata.ReadOnly ? "{0}" : "{0:N}", data.Item2);

						if (!(Metadata.ReadOnly || string.IsNullOrEmpty(moneyFormatter.CurrencySymbol)))
						{
							inputGroup.Attributes["class"] = "input-group";

							var addon = moneyFormatter.CurrencySymbolComesFirst
								? inputGroupAddonFirst
								: inputGroupAddonLast;

							addon.InnerText = moneyFormatter.CurrencySymbol;
							addon.Visible = true;
						}
					}
				}
			};
		}

		protected override void InstantiateValidatorsIn(Control container)
		{
			var maxValue = Metadata.MaxValue.HasValue ? Metadata.MaxValue.Value : decimal.MaxValue;
			var minValue = Metadata.MinValue.HasValue ? Metadata.MinValue.Value : decimal.MinValue;

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
	}
}
