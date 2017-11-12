/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Two Option field.
	/// </summary>
	public class BooleanControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// BooleanControlTemplate class initialization.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public BooleanControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
		}

		public override string CssClass
		{
			get
			{
				if (Metadata != null)
				{
					if (Metadata.ClassID == DropDownClassID)
					{
						return "boolean-dropdown";
					} 
					if (Metadata.ClassID == RadioButtonClassID) {
						return "boolean-radio";
					}
				}
				return "checkbox";
			}
		}

		/// <summary>
		/// Form field.
		/// </summary>
		public CrmEntityFormViewField Field { get; private set; }

		//NOTE: this hack is neccessary until we know some other way of determining the display option for boolean fields.  Class ID in formXml determines control type
		/// <summary>
		/// Class ID used in CRM to specify format type checkbox.
		/// </summary>
		public Guid CheckBoxClassID
		{
			get { return new Guid("B0C6723A-8503-4fd7-BB28-C8A06AC933C2"); }
		}

		/// <summary>
		/// Class ID used in CRM to specify format type dropdown.
		/// </summary>
		public Guid DropDownClassID
		{
			get { return new Guid("3EF39988-22BB-4f0b-BBBE-64B5A3748AEE"); }
		}

		/// <summary>
		/// Class ID used in CRM to specify format type radio.
		/// </summary>
		public Guid RadioButtonClassID
		{
			get { return new Guid("67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED"); }
		}

		private string ValidationText
		{
			get { return Metadata.ValidationText; }
		}

		private ValidatorDisplay ValidatorDisplay
		{
			get { return string.IsNullOrWhiteSpace(ValidationText) ? ValidatorDisplay.None : ValidatorDisplay.Dynamic; }
		}

		protected override bool LabelIsAssociated
		{
			get { return Metadata.ClassID != RadioButtonClassID; }
		}

		protected override void InstantiateControlIn(Control container)
		{
			//NOTE: this hack is neccessary until we know some other way of determining the display option for boolean fields.  Class ID in formXml determines control type
			//get classid of control

			if (Metadata.ClassID == CheckBoxClassID)
			{
				InstantiateCheckboxControlIn(container);
				return;
			}

			ListControl listControl;

			if (Metadata.ClassID == RadioButtonClassID)
			{
				listControl = new RadioButtonList
				{
					RepeatLayout = RepeatLayout.Flow,
					RepeatDirection = RepeatDirection.Horizontal,
					CssClass = string.Join(" ", CssClass, Metadata.CssClass)
				};
			}
			else if (Metadata.ClassID == DropDownClassID)
			{
				listControl = new DropDownList { CssClass = string.Join(" ", "form-control", CssClass, Metadata.CssClass) };
			}
			else
			{
				throw new ArgumentNullException(Metadata.ClassID.ToString(), "Class ID doesn't match any known Boolean control.");
			}

			listControl.ID = ControlID;
			listControl.ToolTip = Metadata.ToolTip;
			listControl.Attributes.Add("onchange", "setIsDirty(this.id);");

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				if (listControl is DropDownList)
				{
					listControl.Attributes.Add("required", string.Empty);
				}
				else
				{
					listControl.Attributes.Add("data-required", "true");
				}
			}

			InstantiateListControlIn(container, listControl);
		}

		protected override void InstantiateValidatorsIn(Control container)
		{
			if ((Metadata.ClassID == CheckBoxClassID) && (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired))
			{
				container.Controls.Add(new CheckboxValidator
				{
					ID = string.Format("RequiredFieldValidator{0}", ControlID),
					ControlToValidate = ControlID,
					ValidationGroup = ValidationGroup,
					Display = ValidatorDisplay,
					ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.RequiredFieldValidationErrorMessage) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("boolean")) ? ResourceManager.GetString("Check_The_Box_Labeled").FormatWith(Metadata.Label) : Metadata.Messages["boolean"].FormatWith(Metadata.Label) : Metadata.RequiredFieldValidationErrorMessage)),
					Text = ValidationText,
				});
			}
			else if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
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
			
			this.InstantiateCustomValidatorsIn(container);
		}

		private void AddSpecialBindingAndHiddenFieldsToPersistDisabledCheckBox(Control container, CheckBox checkBox)
		{
			checkBox.InputAttributes["class"] = "{0} readonly".FormatWith(CssClass);
			checkBox.InputAttributes["disabled"] = "disabled";
			checkBox.InputAttributes["aria-disabled"] = "true";

			var hiddenValue = new HiddenField
			{
				ID = "{0}_Value".FormatWith(ControlID),
				Value = checkBox.Checked.ToString(CultureInfo.InvariantCulture)
			};
			container.Controls.Add(hiddenValue);

			RegisterClientSideDependencies(container);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					bool value;
					return bool.TryParse(hiddenValue.Value, out value) ? new bool?(value) : null;
				},
				Set = obj =>
				{
					var value = Convert.ToBoolean(obj);
					checkBox.Checked = value;
					hiddenValue.Value = value.ToString(CultureInfo.InvariantCulture);
				}
			};
		}

		private void AddSpecialBindingAndHiddenFieldsToPersistDisabledRadioButtons(Control container, ListControl radioButtonList)
		{
			radioButtonList.CssClass = "{0} readonly".FormatWith(CssClass);
			radioButtonList.Enabled = false;

			var hiddenValue = new HiddenField
			{
				ID = "{0}_Value".FormatWith(ControlID),
				Value = radioButtonList.SelectedValue
			};
			container.Controls.Add(hiddenValue);

			RegisterClientSideDependencies(container);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					int value;
					return int.TryParse(hiddenValue.Value, out value) ? (object)(value == 1) : null;
				},
				Set = obj =>
				{
					var value = new OptionSetValue(Convert.ToInt32(obj)).Value;
					radioButtonList.SelectedValue = "{0}".FormatWith(value);
					hiddenValue.Value = radioButtonList.SelectedValue;
				}
			};
		}

		private void AddSpecialBindingAndHiddenFieldsToPersistDisabledSelect(Control container, ListControl dropDown)
		{
			dropDown.CssClass = string.Join(" ", "readonly", dropDown.CssClass);
			dropDown.Attributes["disabled"] = "disabled";

			var hiddenValue = new HiddenField
			{
				ID = "{0}_Value".FormatWith(ControlID),
				Value = dropDown.SelectedValue
			};
			container.Controls.Add(hiddenValue);

			var hiddenSelectedIndex = new HiddenField
			{
				ID = "{0}_SelectedIndex".FormatWith(ControlID),
				Value = dropDown.SelectedIndex.ToString(CultureInfo.InvariantCulture)
			};
			container.Controls.Add(hiddenSelectedIndex);

			RegisterClientSideDependencies(container);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					int value;
					return int.TryParse(hiddenValue.Value, out value) ? (object)(value == 1) : null;
				},
				Set = obj =>
				{
					var value = new OptionSetValue(Convert.ToInt32(obj)).Value;
					dropDown.SelectedValue = "{0}".FormatWith(value);
					hiddenValue.Value = dropDown.SelectedValue;
					hiddenSelectedIndex.Value = dropDown.SelectedIndex.ToString(CultureInfo.InvariantCulture);
				}
			};
		}

		private void InstantiateCheckboxControlIn(Control container)
		{
			var checkbox = new CheckBox
			{
				ID = ControlID,
				CssClass = string.Join(" ", CssClass, Metadata.CssClass),
				ToolTip = Metadata.ToolTip
			};

			checkbox.Attributes.Add("onclick", "setIsDirty(this.id);");

			container.Controls.Add(checkbox);

			if (!container.Page.IsPostBack)
			{
				checkbox.Checked = Convert.ToBoolean(Metadata.DefaultValue);
			}

			if (Metadata.ReadOnly)
			{
				AddSpecialBindingAndHiddenFieldsToPersistDisabledCheckBox(container, checkbox);
			}
			else
			{
				Bindings[Metadata.DataFieldName] = new CellBinding
				{
					Get = () => checkbox.Checked,
					Set = obj => { checkbox.Checked = Convert.ToBoolean(obj); }
				};	
			}
		}

		private void InstantiateListControlIn(Control container, ListControl listControl)
		{
			container.Controls.Add(listControl);

			PopulateListControlIfFirstLoad(listControl);

			if (Metadata.ReadOnly)
			{
				if (listControl is DropDownList)
				{
					AddSpecialBindingAndHiddenFieldsToPersistDisabledSelect(container, listControl);
				}
				else if (listControl is RadioButtonList)
				{
					AddSpecialBindingAndHiddenFieldsToPersistDisabledRadioButtons(container, listControl);
				}
			}
			else
			{
				Bindings[Metadata.DataFieldName] = new CellBinding
				{
					Get = () =>
					{
						int value;
						return int.TryParse(listControl.SelectedValue, out value) ? (object)(value == 1) : null;
					},
					Set = obj =>
					{
						var value = new OptionSetValue(Convert.ToInt32(obj)).Value;
						listControl.SelectedValue = "{0}".FormatWith(value);
					}
				};
			}
		}

		private void PopulateListControlIfFirstLoad(ListControl listControl)
		{
			if (listControl.Items.Count > 0)
			{
				return;
			}

			if (Metadata.IgnoreDefaultValue && listControl is DropDownList)
			{
				var empty = new ListItem(string.Empty, string.Empty);
				empty.Attributes["label"] = " ";
				listControl.Items.Add(empty);
			}

			if (Metadata.BooleanOptionSetMetadata.FalseOption.Value != null)
			{
				var label = Metadata.BooleanOptionSetMetadata.FalseOption.Label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == Metadata.LanguageCode);

				listControl.Items.Add(new ListItem
				{
					Value = Metadata.BooleanOptionSetMetadata.FalseOption.Value.Value.ToString(CultureInfo.InvariantCulture),
					Text = (listControl is RadioButtonList ? "<span class='sr-only'>" + Metadata.Label + " </span>" : string.Empty) +
						(label == null ? Metadata.BooleanOptionSetMetadata.FalseOption.Label.GetLocalizedLabelString() : label.Label)
				});
			}
			else
			{
				listControl.Items.Add(new ListItem { Value = "0", Text = "False", });
			}

			if (Metadata.BooleanOptionSetMetadata.TrueOption.Value != null)
			{
				var label = Metadata.BooleanOptionSetMetadata.TrueOption.Label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == Metadata.LanguageCode);

				listControl.Items.Add(new ListItem
				{
					Value = Metadata.BooleanOptionSetMetadata.TrueOption.Value.Value.ToString(CultureInfo.InvariantCulture),
					Text = (listControl is RadioButtonList ? "<span class='sr-only'>" + Metadata.Label + " </span>" : string.Empty) +
						(label == null ? Metadata.BooleanOptionSetMetadata.TrueOption.Label.GetLocalizedLabelString() : label.Label)
				});
			}
			else
			{
				listControl.Items.Add(new ListItem { Value = "1", Text = "True", });
			}

			if (Metadata.IgnoreDefaultValue || Metadata.DefaultValue == null)
			{
				return;
			}

			if (Convert.ToBoolean(Metadata.DefaultValue))
			{
				if (Metadata.BooleanOptionSetMetadata.TrueOption.Value.HasValue)
				{
					listControl.SelectedValue = Metadata.BooleanOptionSetMetadata.TrueOption.Value.Value.ToString(CultureInfo.InvariantCulture);
				}
			}
			else
			{
				if (Metadata.BooleanOptionSetMetadata.FalseOption.Value.HasValue)
				{
					listControl.SelectedValue = Metadata.BooleanOptionSetMetadata.FalseOption.Value.Value.ToString(CultureInfo.InvariantCulture);
				}
			}
		}
	}
}
