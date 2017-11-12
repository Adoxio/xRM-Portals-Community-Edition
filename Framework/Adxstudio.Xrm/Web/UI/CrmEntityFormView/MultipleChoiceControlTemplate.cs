/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Two Option field and Web Form Metadata specifies the Control Style equals Multiple Choice.
	/// </summary>
	public class MultipleChoiceControlTemplate : BooleanControlTemplate
	{
		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public MultipleChoiceControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings) : base(field, metadata, validationGroup, bindings)
		{
		}

		public override string CssClass
		{
			get
			{
				return "checkbox";
			}
		}

		protected override void InstantiateControlIn(Control container)
		{
			var checkbox = new CheckBox { ID = ControlID, CssClass = string.Join(" ", "checkbox", CssClass, Metadata.CssClass, Metadata.GroupName), ToolTip = Metadata.ToolTip };

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

			var validator = container.FindControl(string.Format("multipleChoiceValidator{0}", Metadata.GroupName));

			if (validator == null)
			{
				var multipleChoiceValidatorErrorMessage = string.Empty;
				if (Metadata.MinMultipleChoiceSelectedCount > 0 && Metadata.MaxMultipleChoiceSelectedCount > 0)
				{
					multipleChoiceValidatorErrorMessage = string.Format(ResourceManager.GetString("Minimum_Most_Options_Selection_Exception"), Metadata.MinMultipleChoiceSelectedCount, Metadata.MaxMultipleChoiceSelectedCount);
				}
				else if (Metadata.MinMultipleChoiceSelectedCount == 0 && Metadata.MaxMultipleChoiceSelectedCount > 0)
				{
					multipleChoiceValidatorErrorMessage = string.Format(ResourceManager.GetString("Maximum_Options_Selection_Exception"), Metadata.MaxMultipleChoiceSelectedCount);
				}
				else if (Metadata.MinMultipleChoiceSelectedCount > 0 && Metadata.MaxMultipleChoiceSelectedCount == 0)
				{
					multipleChoiceValidatorErrorMessage = string.Format(ResourceManager.GetString("Minimum_Options_Selection_Exception"), Metadata.MinMultipleChoiceSelectedCount);
				}
				
				var multipleChoiceValidator = new MultipleChoiceCheckboxValidator
				{
					ID = string.Format("multipleChoiceValidator{0}", Metadata.GroupName),
					ControlToValidate = ControlID,
					ValidationGroup = ValidationGroup,
					ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.MultipleChoiceValidationErrorMessage) ? multipleChoiceValidatorErrorMessage : Metadata.MultipleChoiceValidationErrorMessage)),
					Text = "*",
					CssClass = "validator-text",
					Display = ValidatorDisplay.None,
					Container = container.Parent.Parent.Parent,
					GroupName = Metadata.GroupName,
					MinSelectedCount = Metadata.MinMultipleChoiceSelectedCount,
					MaxSelectedCount = Metadata.MaxMultipleChoiceSelectedCount
				};

				container.Controls.Add(multipleChoiceValidator);
			}
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

		private class MultipleChoiceCheckboxValidator : CheckboxValidator
		{
			public Control Container;
			public string GroupName;
			public int MaxSelectedCount;
			public int MinSelectedCount;
			
			protected override bool EvaluateIsValid()
			{
				if (Container == null || string.IsNullOrWhiteSpace(GroupName))
				{
					return base.EvaluateIsValid();
				}

				return IsSelectedCountValid(Container, GroupName, MinSelectedCount, MaxSelectedCount);
			}
		}

		protected static bool IsSelectedCountValid(Control container, string groupname, int min, int max)
		{
			var selectedCount = 0;
			var controlCount = 0;

			foreach (Control control in container.Controls)
			{
				if (control is HtmlTableRow)
				{
					foreach (Control cell in control.Controls)
					{
						foreach (Control element in cell.Controls)
						{
							if (element is HtmlContainerControl && ((HtmlContainerControl)element).Attributes["class"] == "control")
							{
								foreach (Control input in element.Controls)
								{
									if (input is CheckBox && ((CheckBox)input).CssClass.Contains(groupname))
									{
										var checkbox = (CheckBox)input;
										
										if (checkbox.Checked)
										{
											selectedCount++;
										}

										controlCount++;
									}
								}
							}
						}
					}
				}
			}

			if (max == 0)
			{
				max = controlCount;
			}

			return selectedCount >= min && selectedCount <= max;
		}
	}
}
