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
	/// Template used when rendering a Whole Number field when Web Form Metadata specifies Control Style equals Constant Sum
	/// </summary>
	public class ConstantSumControlTemplate : IntegerControlTemplate
	{
		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public ConstantSumControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(field, metadata, validationGroup, bindings)
		{
		}

		public override string CssClass
		{
			get
			{
				return "constant-sum form-control";
			}
		}

		protected override void InstantiateControlIn(Control container)
		{
			var textbox = new TextBox { ID = ControlID, TextMode = TextBoxMode.SingleLine, CssClass = string.Join(" ", "text integer", Metadata.GroupName, Metadata.CssClass), ToolTip = Metadata.ToolTip, Width = 30 };

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				textbox.Attributes.Add("required", string.Empty);
			}

			if (Metadata.ReadOnly)
			{
				textbox.CssClass += " readonly";
				textbox.Attributes["readonly"] = "readonly";
			}

			textbox.Attributes["onchange"] = "setIsDirty(this.id);setPrecision(this.id);updateConstantSum('" + Metadata.GroupName + "');";

			container.Controls.Add(textbox);

			RegisterClientSideDependencies(container);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					int value;

					return int.TryParse(textbox.Text, out value) ? new int?(value) : null;
				},
				Set = obj =>
				{
					textbox.Text = "{0}".FormatWith(obj);
				}
			};

			// Add a single hidden field to store the index of the constant sum fields processed
			var index = 0;
			HiddenField hiddenField;
			var hiddenFieldControl = container.Parent.Parent.Parent.FindControl(string.Format("ConstantSumIndex{0}", Metadata.GroupName));
			if (hiddenFieldControl == null)
			{
				hiddenField = new HiddenField { ID = string.Format("ConstantSumIndex{0}", Metadata.GroupName), ClientIDMode = ClientIDMode.Static, Value = "1" };
				container.Parent.Parent.Parent.Controls.Add(hiddenField);
			}
			else
			{
				hiddenField = (HiddenField)hiddenFieldControl;
				int.TryParse(hiddenField.Value, out index);
			}
			index++;
			hiddenField.Value = index.ToString(CultureInfo.InvariantCulture);
			// Add a single field to display the sum
			if (index == Metadata.ConstantSumAttributeNames.Length)
			{
				var totalField = new TextBox { ID = string.Format("ConstantSumTotalValue{0}", Metadata.GroupName), CssClass = "total", Text = "0", Width = 30, ClientIDMode = ClientIDMode.Static };

				totalField.Attributes.Add("readonly", "readonly"); // Adding readonly attribute provides viewstate support and prevents user editing as opposed to setting the control's ReadOnly property which does not retain value in viewstate if value is modified by javascript.

				container.Controls.Add(totalField);

				var constantSumValidator = new CustomValidator
				{
					ID = string.Format("ConstantSumTotalValidator{0}", Metadata.GroupName),
					ControlToValidate = string.Format("ConstantSumTotalValue{0}", Metadata.GroupName),
					ValidationGroup = ValidationGroup,
					ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.ConstantSumValidationErrorMessage) ? ResourceManager.GetString("Constant_Sum_Validation_Error_Message") : Metadata.ConstantSumValidationErrorMessage)),
					Text = "*",
					CssClass = "validator-text"
				};

				constantSumValidator.ServerValidate += GetConstantSumValidationHandler(container.Parent.Parent.Parent, Metadata.GroupName, Metadata.ConstantSumMinimumTotal, Metadata.ConstantSumMaximumTotal);

				container.Controls.Add(constantSumValidator);
			}
		}

		private static ServerValidateEventHandler GetConstantSumValidationHandler(Control container, string groupname, int min, int max)
		{
			return (sender, args) =>
			{
				args.IsValid = IsTotalValid(container, groupname, min, max);
			};
		}

		protected static bool IsTotalValid(Control container, string groupname, int min, int max)
		{
			var total = 0;

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
									if (input is TextBox && ((TextBox)input).CssClass.Contains(groupname))
									{
										var textBox = (TextBox)input;
										int value;
										if (int.TryParse(textBox.Text, out value))
										{
											total += value;
										}
									}
								}
							}
						}
					}
				}
			}

			return total >= min && total <= max;
		}
	}
}
