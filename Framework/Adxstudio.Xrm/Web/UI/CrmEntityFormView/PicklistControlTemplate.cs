/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a picklist (Option Set) field.
	/// </summary>
	public class PicklistControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// PicklistControlTemplate class intialization.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public PicklistControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup,
			IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
		}

		public override string CssClass
		{
			get
			{
				switch (Metadata.ControlStyle)
				{
				case WebFormMetadata.ControlStyle.MultipleChoiceMatrix:
					return "picklist-matrix";
				default:
					return "picklist";
				}
			}
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
		
		protected override bool LabelIsAssociated
		{
			get
			{
				return Metadata.ControlStyle != WebFormMetadata.ControlStyle.VerticalRadioButtonList &&
					Metadata.ControlStyle != WebFormMetadata.ControlStyle.HorizontalRadioButtonList &&
					Metadata.ControlStyle != WebFormMetadata.ControlStyle.MultipleChoiceMatrix;
			}
		}

		protected override void InstantiateControlIn(Control container)
		{
			ListControl listControl;

			switch (Metadata.ControlStyle)
			{
			case WebFormMetadata.ControlStyle.VerticalRadioButtonList:
				listControl = new RadioButtonList
				{
					ID = ControlID,
					CssClass = string.Join(" ", "picklist", "vertical", Metadata.CssClass),
					ToolTip = Metadata.ToolTip,
					RepeatDirection = RepeatDirection.Vertical,
					RepeatLayout = RepeatLayout.Flow
				};
				break;
			case WebFormMetadata.ControlStyle.HorizontalRadioButtonList:
				listControl = new RadioButtonList
				{
					ID = ControlID,
					CssClass = string.Join(" ", "picklist", "horizontal", Metadata.CssClass),
					ToolTip = Metadata.ToolTip,
					RepeatDirection = RepeatDirection.Horizontal,
					RepeatLayout = RepeatLayout.Flow
				};
				break;
			case WebFormMetadata.ControlStyle.MultipleChoiceMatrix:
				listControl = new RadioButtonList
				{
					ID = ControlID,
					CssClass = string.Join(" ", "picklist", "horizontal", "labels-top", Metadata.CssClass),
					ToolTip = Metadata.ToolTip,
					RepeatDirection = RepeatDirection.Horizontal,
					RepeatLayout = RepeatLayout.Table,
					TextAlign = TextAlign.Left
				};
				break;
			default:
				listControl = new DropDownList
				{
					ID = ControlID,
					CssClass = string.Join(" ", "form-control", CssClass, Metadata.CssClass),
					ToolTip = Metadata.ToolTip
				};
				break;
			}

			if (listControl is RadioButtonList)
			{
				listControl.Attributes.Add("role", "presentation");
			}

			listControl.Attributes.Add("onchange", "setIsDirty(this.id);");

			container.Controls.Add(listControl);

			PopulateListControl(listControl);

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				switch (Metadata.ControlStyle)
				{
				case WebFormMetadata.ControlStyle.VerticalRadioButtonList:
				case WebFormMetadata.ControlStyle.HorizontalRadioButtonList:
				case WebFormMetadata.ControlStyle.MultipleChoiceMatrix:
					listControl.Attributes.Add("data-required", "true");
					break;
				default:
					listControl.Attributes.Add("required", string.Empty);
					break;
				}
			}

			if (Metadata.ReadOnly)
			{
				AddSpecialBindingAndHiddenFieldsToPersistDisabledSelect(container, listControl);
			}
			else
			{
				Bindings[Metadata.DataFieldName] = new CellBinding
				{
					Get = () =>
					{
						int value;
						return int.TryParse(listControl.SelectedValue, out value) ? new int?(value) : null;
					},
					Set = obj =>
					{
						var value = ((OptionSetValue)obj).Value;
						var listItem = listControl.Items.FindByValue(value.ToString(CultureInfo.InvariantCulture));
						if (listItem != null)
						{
							listControl.ClearSelection();
							listItem.Selected = true;
						}
					}
				};
			}
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
					ErrorMessage =
						ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.RequiredFieldValidationErrorMessage)
							? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("required"))
								? ResourceManager.GetString("Required_Field_Error").FormatWith(Metadata.Label)
								: Metadata.Messages["required"].FormatWith(Metadata.Label)
							: Metadata.RequiredFieldValidationErrorMessage)),
					Text = Metadata.ValidationText,
				});
			}

			this.InstantiateCustomValidatorsIn(container);
		}

		private void AddSpecialBindingAndHiddenFieldsToPersistDisabledSelect(Control container, ListControl listControl)
		{
			listControl.CssClass = string.Join(" ", "readonly", listControl.CssClass);

			listControl.Attributes["disabled"] = "disabled";
			listControl.Attributes["aria-disabled"] = "true";

			var hiddenValue = new HiddenField
			{
				ID = "{0}_Value".FormatWith(ControlID),
				Value = listControl.SelectedValue
			};

			container.Controls.Add(hiddenValue);

			var hiddenSelectedIndex = new HiddenField
			{
				ID = "{0}_SelectedIndex".FormatWith(ControlID),
				Value = listControl.SelectedIndex.ToString(CultureInfo.InvariantCulture)
			};

			container.Controls.Add(hiddenSelectedIndex);

			RegisterClientSideDependencies(container);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					int value;
					return int.TryParse(hiddenValue.Value, out value) ? new int?(value) : null;
				},
				Set = obj =>
				{
					var value = ((OptionSetValue)obj).Value;
					var listItem = listControl.Items.FindByValue(value.ToString(CultureInfo.InvariantCulture));
					if (listItem != null)
					{
						listControl.ClearSelection();
						listItem.Selected = true;
						hiddenValue.Value = listItem.Value;
						hiddenSelectedIndex.Value = listControl.SelectedIndex.ToString(CultureInfo.InvariantCulture);
					}
				}
			};
		}

		private void PopulateListControl(ListControl listControl)
		{
			if (listControl.Items.Count > 0)
			{
				return;
			}

			if (listControl is DropDownList)
			{
				var empty = new ListItem(string.Empty, string.Empty);
				empty.Attributes["label"] = " ";
				listControl.Items.Add(empty);
			}

			var options = Metadata.RandomizeOptionSetValues ? Metadata.PicklistOptions.Randomize() : Metadata.PicklistOptions;

			foreach (var option in options)
			{
				if (option == null || option.Value == null)
				{
					continue;
				}

				var label = option.Label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == Metadata.LanguageCode);

				listControl.Items.Add(new ListItem
				{
					Value = option.Value.Value.ToString(CultureInfo.InvariantCulture),
					Text = (listControl is RadioButtonList ? "<span class='sr-only'>" + Metadata.Label + " </span>" : string.Empty) +
						(label == null ? option.Label.GetLocalizedLabelString() : label.Label)
				});
			}

			if (!Metadata.IgnoreDefaultValue && Metadata.DefaultValue != null)
			{
				var value = Convert.ToInt32(Metadata.DefaultValue) == -1 ? string.Empty : Metadata.DefaultValue.ToString();
				var listItem = listControl.Items.FindByValue(value);
				if (listItem != null)
				{
					listControl.ClearSelection();
					listItem.Selected = true;
				}
			}
		}
	}
}
