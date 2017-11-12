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
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Multiple Line of Text field.
	/// </summary>
	public class MemoControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// MemoControlTemplate class initialization.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public MemoControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
		}

		/// <summary>
		/// Form field.
		/// </summary>
		public CrmEntityFormViewField Field { get; private set; }

		public override string CssClass
		{
			get { return "textarea form-control"; }
		}

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
			var textbox = new EnhancedTextBox
			{
				ID = ControlID,
				CssClass = string.Join(" ", CssClass, Metadata.CssClass),
				TextMode = TextBoxMode.MultiLine,
				ToolTip = Metadata.ToolTip,
			};

			textbox.Attributes.Add("onchange", "setIsDirty(this.id);");

			try
			{
				textbox.Rows = checked(Metadata.RowSpan.GetValueOrDefault(2) * 3 - 2);
			}
			catch (OverflowException)
			{
				textbox.Rows = 3;
			}

			if (Metadata.MaxLength > 0)
			{
				textbox.MaxLength = Metadata.MaxLength;
			}

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				textbox.Attributes.Add("aria-required", "true");
				textbox.Attributes["aria-label"] = string.Format(ResourceManager.GetString("Required_Field_Error"), Metadata.Label);
			}

			if (Metadata.ReadOnly)
			{
				textbox.CssClass += " readonly";
				textbox.Attributes["readonly"] = "readonly";
			}

			container.Controls.Add(textbox);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					// Browser interprets a new line in a textarea as a two characters (\n).
					// C# interprets a new line as a carriage return (\r) and a line feed (\n), this adds 2 extra characters for every new line
					// and will cause the length of the string to be larger than the field's Maximum Length property. Replace \r\n with \n
					var str = textbox.Text;
					return str != null ? str.Replace("\r\n", "\n") : string.Empty;
				},
				Set = obj => { textbox.Text = obj as string; }
			};
		}

		protected override void InstantiateValidatorsIn(Control container)
		{
			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				container.Controls.Add(new RequiredFieldValidator
				{
					ID = string.Format("RequiredFieldValidator{0}", ControlID),
					ControlToValidate = ControlID,
					ValidationGroup = ValidationGroup,
					Display = ValidatorDisplay,
					ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.RequiredFieldValidationErrorMessage) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("required")) ? ResourceManager.GetString("Required_Field_Error").FormatWith(Metadata.Label) : Metadata.Messages["required"].FormatWith(Metadata.Label) : Metadata.RequiredFieldValidationErrorMessage)),
					Text = Metadata.ValidationText,
				});
			}

			if (Metadata.MaxLength > 0)
			{
				var maxLengthValidator = new CustomValidator
				{
					ID = $"MaximumLengthValidator{ControlID}",
					ControlToValidate = ControlID,
					ValidationGroup = ValidationGroup,
					ErrorMessage = ValidationSummaryMarkup(ResourceManager.GetString("MaxLength_Validation_Error").FormatWith(Metadata.Label, Metadata.MaxLength)),
					Text = "*"
				};

				maxLengthValidator.ServerValidate += MaxLengthValidate;

				container.Controls.Add(maxLengthValidator);
			}

			this.InstantiateCustomValidatorsIn(container);
		}

		private void MaxLengthValidate(object sender, ServerValidateEventArgs args)
		{
			var text = args.Value;

			if (text.Length > Metadata.MaxLength)
			{
				args.IsValid = false;

				return;
			}
		}
	}
}
