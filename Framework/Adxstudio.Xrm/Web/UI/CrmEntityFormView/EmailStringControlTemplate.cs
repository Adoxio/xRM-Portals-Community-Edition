/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Single Line of Text with format set to email.
	/// </summary>
	public class EmailStringControlTemplate : StringControlTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// EmailStringControlTemplate class initialization.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public EmailStringControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(field, metadata, validationGroup, bindings)
		{
			Field = field;
		}

		/// <summary>
		/// Form field.
		/// </summary>
		public new CrmEntityFormViewField Field { get; private set; }

		protected override void InstantiateControlIn(Control container)
		{
			var textbox = new TextBox { ID = ControlID, CssClass = string.Join(" ", CssClass, Metadata.CssClass), ToolTip = Metadata.ToolTip };

			textbox.Style.Add("text-decoration", "underline");
			textbox.Attributes["type"] = "email";
			textbox.Attributes["ondblclick"] = "launchEmail(this.value);";
			textbox.Attributes["onchange"] = string.Format("setIsDirty(this.id);");

			if (Metadata.MaxLength > 0)
			{
				textbox.MaxLength = Metadata.MaxLength;
			}

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				textbox.Attributes.Add("required", string.Empty);
			}

			if (Metadata.ReadOnly)
			{
				textbox.CssClass += " readonly";
				textbox.Attributes["readonly"] = "readonly";
			}

			container.Controls.Add(textbox);

			RegisterClientSideDependencies(container);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () => textbox.Text,
				Set = obj => { textbox.Text = obj as string; }
			};
		}

		protected override void InstantiateValidatorsIn(Control container)
		{
			var emailFormatValidator = new CustomValidator
			{
				ID = string.Format("EmailFormatValidator{0}", ControlID),
				ControlToValidate = ControlID,
				ValidationGroup = ValidationGroup,
				ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.ValidationErrorMessage) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("email")) ? ResourceManager.GetString("Invalid_Email_Address_Error").FormatWith(Metadata.Label) : Metadata.Messages["email"].FormatWith(Metadata.Label) : Metadata.ValidationErrorMessage)),
				Text = "*",
				ValidateEmptyText = false,
			};

			emailFormatValidator.ServerValidate += ValidateEmailFormat;

			container.Controls.Add(emailFormatValidator);

			base.InstantiateValidatorsIn(container);
		}

		[SuppressMessage("Microsoft.Security", "CA2109", Justification = "Code reviewed.  Not necessary for ASP.NET 2.0 and above.")]
		protected virtual void ValidateEmailFormat(object source, ServerValidateEventArgs args)
		{
			try
			{
				var address = new MailAddress(args.Value);

				args.IsValid = true;
			}
			catch (FormatException)
			{
				args.IsValid = false;
			}
		}
	}
}
