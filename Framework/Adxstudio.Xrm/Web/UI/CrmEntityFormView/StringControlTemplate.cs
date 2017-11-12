/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Net;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Portal.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	public class StringControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		public StringControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
		}

		public override string CssClass
		{
			get { return "text form-control"; }
		}

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
			var textbox = new TextBox { ID = ControlID, CssClass = string.Join(" ", CssClass, Metadata.CssClass), ToolTip = Metadata.ToolTip };

			textbox.Attributes.Add("onchange", "setIsDirty(this.id);");

			if (Metadata.MaxLength > 0)
			{
				textbox.MaxLength = Metadata.MaxLength;
			}

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				textbox.Attributes.Add("aria-required", "true");
				textbox.Attributes.Add("title", string.IsNullOrWhiteSpace(Metadata.RequiredFieldValidationErrorMessage) ?
								(Metadata.Messages == null || !Metadata.Messages.ContainsKey("Required")) ?
								ResourceManager.GetString("Required_Field_Error").FormatWith(Metadata.Label) :
								Metadata.Messages["Required"].FormatWith(Metadata.Label) :
								Metadata.RequiredFieldValidationErrorMessage);
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
					ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.RequiredFieldValidationErrorMessage) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("Required")) ? ResourceManager.GetString("Required_Field_Error").FormatWith(Metadata.Label) : Metadata.Messages["Required"].FormatWith(Metadata.Label) : Metadata.RequiredFieldValidationErrorMessage)),
					Text = Metadata.ValidationText,
				});
			}

            if (PortalSettings.Instance.BingMapsSupported && Metadata.ControlStyle == WebFormMetadata.ControlStyle.GeolocationLookupValidator)
            {
                var locationValidator = new CustomValidator
				{
					ID = string.Format("GeolocationValidator{0}", ControlID),
					ControlToValidate = ControlID,
					ValidationGroup = ValidationGroup,
					Display = ValidatorDisplay.None,
					ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.GeolocationValidatorErrorMessage) ? ResourceManager.GetString("Invalid_Error").FormatWith(Metadata.Label) : Metadata.GeolocationValidatorErrorMessage))
				};

				var bingLookup = new BingMapLookup();

				locationValidator.ServerValidate += bingLookup.LocationServerValidate;
				
				container.Controls.Add(locationValidator);
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
