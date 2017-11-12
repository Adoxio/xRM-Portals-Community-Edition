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

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Picklist (Option Set) field in the special custom case where the user should be allowed to select multiple options.
	/// </summary>
	public class MultiSelectPicklistControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// MultiSelectPicklistControlTemplate class initialization.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public MultiSelectPicklistControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
		}

		public override string CssClass
		{
			get { return "multiselectpicklist"; }
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
			var checkboxList = new CheckBoxList { ID = ControlID, CssClass = string.Join(" ", CssClass, Metadata.CssClass), ToolTip = Metadata.ToolTip };

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				checkboxList.Attributes.Add("required", string.Empty);
			}

			if (Metadata.ReadOnly)
			{
				checkboxList.CssClass += " readonly";
				checkboxList.Attributes["readonly"] = "readonly";
			}

			checkboxList.Attributes.Add("onclick", "setIsDirty(this.id);");

			container.Controls.Add(checkboxList);

			PopulateListIfFirstLoad(checkboxList);

			//Bind to the singleline text attribute that holds to comma delimited list of optionset values of the picklist.
			Bindings[string.Format("{0}selectedvalues", Metadata.DataFieldName)] = new CellBinding
			{
				Get = () =>
				{
					var selected = new List<string>();

					for (int i = 0, j = checkboxList.Items.Count - 1; i <= j; i++)
					{
						if (checkboxList.Items[i].Selected)
						{
							selected.Add(checkboxList.Items[i].Value);
						}
					}

					var stringSelected = string.Join(",", selected.ToArray());

					return stringSelected;
				},
				Set = obj =>
				{
					var stringValues = (string)obj;

					if (string.IsNullOrWhiteSpace(stringValues))
					{
						return;
					}

					var values = stringValues.Split(',');

					for (int i = 0, j = values.Length - 1; i <= j; i++)
					{
						for (int k = 0, l = checkboxList.Items.Count - 1; k <= l; k++)
						{
							if (checkboxList.Items[k].Value == values[i])
							{
								checkboxList.Items[k].Selected = true;
							}
						}
					}
				}
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

			this.InstantiateCustomValidatorsIn(container);
		}

		private void PopulateListIfFirstLoad(ListControl checkboxList)
		{
			if (checkboxList.Items.Count > 0)
			{
				return;
			}

			var options = Metadata.RandomizeOptionSetValues ? Metadata.PicklistOptions.Randomize() : Metadata.PicklistOptions;

			foreach (var option in options)
			{
				if (option.Value != null)
				{
					var value = option.Value.Value.ToString(CultureInfo.InvariantCulture);
					var label = option.Label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == Metadata.LanguageCode);

					checkboxList.Items.Add(new ListItem
					{
						Value = value,
						Text = label == null ? option.Label.GetLocalizedLabelString() : label.Label
					});
				}
			}

			if (!Metadata.IgnoreDefaultValue && Metadata.DefaultValue != null)
			{
				checkboxList.SelectedValue = Convert.ToInt32(Metadata.DefaultValue) == -1 ? string.Empty : Metadata.DefaultValue.ToString();
			}
		}
	}
}
