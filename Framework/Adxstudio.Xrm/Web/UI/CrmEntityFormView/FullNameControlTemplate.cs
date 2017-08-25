/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FullNameControlTemplate.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Web.UI.WebControls;
	using Adxstudio.Xrm.Web.UI.WebForms;
	using Adxstudio.Xrm.Globalization;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Metadata;

	/// <summary>
	/// Fullname control
	/// </summary>
	/// <seealso cref="Adxstudio.Xrm.Web.UI.CrmEntityFormView.CellTemplate" />
	/// <seealso cref="Adxstudio.Xrm.Web.UI.CrmEntityFormView.ICustomFieldControlTemplate" />
	public class FullNameControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// The first name
		/// </summary>
		private const string FirstName = "firstname";

		/// <summary>
		/// The last name
		/// </summary>
		private const string LastName = "lastname";

		/// <summary>
		/// The entity metadata
		/// </summary>
		private readonly EntityMetadata entityMetadata;

		/// <summary>
		/// The is editable
		/// </summary>
		private bool isEditable;

		/// <summary>
		/// Initializes a new instance of the <see cref="FullNameControlTemplate"/> class.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="metadata">The metadata.</param>
		/// <param name="validationGroup">The validation group.</param>
		/// <param name="bindings">The bindings.</param>
		/// <param name="entityMetadata">The entity metadata.</param>
		/// <param name="mode">The mode.</param>
		public FullNameControlTemplate(
			CrmEntityFormViewField field, 
			FormXmlCellMetadata metadata, 
			string validationGroup, 
			IDictionary<string, CellBinding> bindings, 
			EntityMetadata entityMetadata, 
			FormViewMode? mode)
			: base(metadata, validationGroup, bindings)
		{
			this.Field = field;
			this.isEditable = (mode == FormViewMode.ReadOnly) ? false : true;
			this.entityMetadata = entityMetadata;
		}

		/// <summary>
		/// CSS Class name assigned.
		/// </summary>
		public override string CssClass
		{
			get
			{
				return "text form-control";
			}
		}

		/// <summary>
		/// Gets the field.
		/// </summary>
		/// <value>
		/// The field.
		/// </value>
		public CrmEntityFormViewField Field { get; private set; }

		/// <summary>
		/// Gets the validation text.
		/// </summary>
		/// <value>
		/// The validation text.
		/// </value>
		private string ValidationText
		{
			get
			{
				return this.Metadata.ValidationText;
			}
		}

		/// <summary>
		/// Gets the validator display.
		/// </summary>
		/// <value>
		/// The validator display.
		/// </value>
		private ValidatorDisplay ValidatorDisplay
		{
			get
			{
				return string.IsNullOrWhiteSpace(this.ValidationText) ? ValidatorDisplay.None : ValidatorDisplay.Dynamic;
			}
		}

		/// <summary>
		/// Instantiates the control in.
		/// </summary>
		/// <param name="container">The container.</param>
		protected override void InstantiateControlIn(Control container)
		{
			var contentId = Guid.NewGuid();
			if (this.Metadata.ReadOnly)
			{
				this.isEditable = false;
			}
			var textboxFullname = new TextBox
									{
										ID = this.ControlID, 
										CssClass = string.Join(" ", "trigger", this.CssClass, this.Metadata.CssClass, "fullNameCompositeControl"), 
										ToolTip = this.Metadata.ToolTip
									};

			textboxFullname.Attributes.Add("data-composite-control", string.Empty);
			textboxFullname.Attributes.Add("data-content-id", contentId.ToString());
			textboxFullname.Attributes.Add("data-editable", this.isEditable.ToString());

			switch (CultureInfo.CurrentUICulture.LCID)
			{
				case LocaleIds.Japanese:
				case LocaleIds.Hebrew:
					textboxFullname.Attributes.Add("data-content-template", "{fullnamelastname} {fullnamefirstname}");
					break;
				default:
					textboxFullname.Attributes.Add("data-content-template", "{fullnamefirstname} {fullnamelastname}");
					break;
			}

			var firstNameAttribute = this.entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == FirstName);
			var lastNameAttribute = this.entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == LastName);

			// Creating container for all popover elements 
			var divContainer = new HtmlGenericControl("div");
			divContainer.Attributes["class"] = "content hide fullNameCompositeControlContainer";
			divContainer.ID = contentId.ToString();

			var labelFirstname = new Label
									{
										ID = "FirstNameLabel", 
										Text = this.GetLocalizedLabel(firstNameAttribute, "First_Name_DefaultText"), 
										CssClass = "content "
									};

			var labelLastname = new Label
									{
										ID = "LastNameLabel", 
										Text = this.GetLocalizedLabel(lastNameAttribute, "Last_Name_DefaultText"), 
										CssClass = "content "
									};

			container.Controls.Add(textboxFullname);

			// Creating textboxes for First ans Last name respectfully to localization
			if (Localization.IsWesternType())
			{
				this.Generatefield(FirstName, firstNameAttribute, divContainer, labelFirstname);
				this.Generatefield(LastName, lastNameAttribute, divContainer, labelLastname);
			}
			else
			{
				this.Generatefield(LastName, lastNameAttribute, divContainer, labelLastname);
				this.Generatefield(FirstName, firstNameAttribute, divContainer, labelFirstname);
			}

			var buttonFullname = new HtmlGenericControl("input");
			buttonFullname.Attributes["class"] = "btn btn-primary btn-block";
			buttonFullname.Attributes["readonly"] = "true";
			buttonFullname.Attributes["role"] = "button";
			buttonFullname.ID = "fullNameUpdateButton";
			buttonFullname.Attributes.Add("value", ResourceManager.GetString("Composite_Control_Done"));

			textboxFullname.Attributes.Add("onchange", "setIsDirty(this.id);");

			if (this.Metadata.MaxLength > 0)
			{
				textboxFullname.MaxLength = this.Metadata.MaxLength;
			}

			if (this.Metadata.IsRequired || this.Metadata.WebFormForceFieldIsRequired)
			{
				textboxFullname.Attributes.Add("required", string.Empty);
			}

			if (this.isEditable)
			{
				textboxFullname.Attributes.Add("aria-label", string.Format("{0}*. {1}", this.Metadata.Label, ResourceManager.GetString("Narrator_Label_For_Composite_Controls")));
				divContainer.Controls.Add(buttonFullname);
				container.Controls.Add(divContainer);
			}
			else
			{
				textboxFullname.CssClass = textboxFullname.CssClass += " readonly";
			}

			this.Bindings["fullname_fullname"] = new CellBinding
													{
														Get = () =>
															{
																var str = textboxFullname.Text;
																return str != null ? str.Replace("\r\n", "\n") : string.Empty;
															}, 
														Set = obj =>
															{
																var entity = obj as Entity;

																if (entity != null)
																{
																	textboxFullname.Text = Localization.LocalizeFullName(
																		entity.GetAttributeValue<string>(FirstName),
																		entity.GetAttributeValue<string>(LastName));
																}
															}
													};
		}

		/// <summary>
		/// Instantiates the validators in.
		/// </summary>
		/// <param name="container">The container.</param>
		protected override void InstantiateValidatorsIn(Control container)
		{
			if (this.Metadata.IsRequired || this.Metadata.WebFormForceFieldIsRequired || this.Metadata.IsFullNameControl)
			{
				container.Controls.Add(this.ConfigureFieldValidator(this.ControlID, this.Metadata.Label));
			}

			if (this.IsAttributeNeedRequiredValidation(this.entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == LastName)))
			{
				container.Controls.Add(this.ConfigureFieldValidator(this.ControlID + LastName, this.GetLocalizedLabel(LastName)));
			}
			if (this.IsAttributeNeedRequiredValidation(this.entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == FirstName)))
			{
				container.Controls.Add(this.ConfigureFieldValidator(this.ControlID + FirstName, this.GetLocalizedLabel(FirstName)));
			}

			this.InstantiateCustomValidatorsIn(container);
		}

		/// <summary>
		/// Check if field need required validation based on attribute metadata
		/// </summary>
		/// <param name="attributeMetadata">attribute metadata</param>
		/// <returns>Returns true if control is editable and attribute is required</returns>
		private bool IsAttributeNeedRequiredValidation(AttributeMetadata attributeMetadata)
		{
			return attributeMetadata != null && this.isEditable &&
					(attributeMetadata.RequiredLevel.Value == AttributeRequiredLevel.ApplicationRequired ||
					attributeMetadata.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired);
		}

		/// <summary>
		/// Configures the field validator.
		/// </summary>
		/// <param name="controlId">The control identifier.</param>
		/// <param name="localizedLabel">The localized label.</param>
		/// <returns>Field validator for Full Name control and it's required fields</returns>
		private RequiredFieldValidator ConfigureFieldValidator(string controlId, string localizedLabel)
		{
			return new RequiredFieldValidator
			{
				ID = string.Format("RequiredFieldValidator{0}", controlId),
				ControlToValidate = controlId,
				ValidationGroup = this.ValidationGroup,
				Display = this.ValidatorDisplay,
				ErrorMessage =
								this.ValidationSummaryMarkup(
									string.IsNullOrWhiteSpace(this.Metadata.RequiredFieldValidationErrorMessage)
										? (this.Metadata.Messages == null || !this.Metadata.Messages.ContainsKey("Required"))
											? ResourceManager.GetString("Required_Field_Error").FormatWith(localizedLabel)
											: this.Metadata.Messages["Required"].FormatWith(this.Metadata.Label)
										: this.Metadata.RequiredFieldValidationErrorMessage),
				Text = this.Metadata.ValidationText
			};
		}

		/// <summary>
		/// Generatefields the specified field name.
		/// </summary>
		/// <param name="fieldName">Name of the field.</param>
		/// <param name="fieldMetaData">The field meta data.</param>
		/// <param name="divContainer">The div container.</param>
		/// <param name="labelFirstname">The label firstname.</param>
		private void Generatefield(
			string fieldName, 
			AttributeMetadata fieldMetaData, 
			HtmlGenericControl divContainer, 
			Label labelFirstname)
		{
			var textBox = new TextBox
							{
								ID = this.ControlID + fieldName, 
								CssClass = string.Join(" content ", " ", this.CssClass, this.Metadata.CssClass), 
								ToolTip = this.Metadata.ToolTip
							};
			labelFirstname.AssociatedControlID = textBox.ID;

			if (this.isEditable)
			{
				// Applying required parameters to first name if it's application required
				if (fieldMetaData != null
					&& (fieldMetaData.RequiredLevel.Value == AttributeRequiredLevel.ApplicationRequired
						|| fieldMetaData.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired))
				{
					textBox.Attributes.Add("required", string.Empty);
					var requierdContainer = new HtmlGenericControl("div");
					requierdContainer.Attributes["class"] = "info required";
					requierdContainer.Controls.Add(labelFirstname);
					divContainer.Controls.Add(requierdContainer);
				}
				else
				{
					divContainer.Controls.Add(labelFirstname);
				}

				divContainer.Controls.Add(textBox);
			}

			textBox.Attributes.Add("onchange", "setIsDirty(this.id);");
			this.Bindings["fullname_" + fieldName] = new CellBinding
														{
															Get = () =>
																{
																	var str = textBox.Text;
																	return str != null ? str.Replace("\r\n", "\n") : string.Empty;
																}, 
															Set = obj =>
																{
																	var entity = obj as Entity;

																	if (entity != null)
																	{
																		textBox.Text = entity.GetAttributeValue<string>(fieldName);
																	}
																}
														};
		}

		/// <summary>
		/// Gets the localized label.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="resourceString">The resource string.</param>
		/// <returns>Localized label string</returns>
		private string GetLocalizedLabel(AttributeMetadata attribute, string resourceString)
		{
			var localizedLabel =
				attribute.DisplayName.LocalizedLabels.FirstOrDefault(lcid => lcid.LanguageCode == CultureInfo.CurrentUICulture.LCID);
			if (localizedLabel != null)
			{
				return localizedLabel.Label;
			}
			return ResourceManager.GetString(resourceString);
		}

		/// <summary>
		/// Gets the localized label.
		/// </summary>
		/// <param name="logicalName">Name of the logical.</param>
		/// <returns>Localized logical name</returns>
		private string GetLocalizedLabel(string logicalName)
		{
			return this.entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == logicalName)
						.DisplayName.UserLocalizedLabel.Label;
		}
	}
}
