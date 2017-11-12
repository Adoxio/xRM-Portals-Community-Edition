/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Whole Number field and Web Form Metadata specifies Control Style equals Rank Order
	/// </summary>
	public class RankOrderControlTemplate : IntegerControlTemplate
	{
		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public RankOrderControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings) : base(field, metadata, validationGroup, bindings)
		{
		}

		public override string CssClass
		{
			get
			{
				return "rank-order";
			}
		}

		protected override void InstantiateControlIn(Control container)
		{
			var textbox = new TextBox { ID = ControlID, TextMode = TextBoxMode.SingleLine, CssClass = string.Join(" ", "form-control", "text", "integer", Metadata.CssClass, Metadata.GroupName), ToolTip = Metadata.ToolTip, Width = 30 };

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				textbox.Attributes.Add("required", string.Empty);
			}

			if (Metadata.ReadOnly)
			{
				textbox.CssClass += " readonly";
				textbox.Attributes["readonly"] = "readonly";
			}

			textbox.Attributes["onchange"] = "setPrecision(this.id);setIsDirty(this.id);";

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

			if (Metadata.ControlStyle == WebFormMetadata.ControlStyle.RankOrderNoTies)
			{
				var rankOrderTieValidator = new CustomValidator
				{
					ID = string.Format("RankOrderTieValidator{0}", ControlID),
					ControlToValidate = ControlID,
					ValidationGroup = ValidationGroup,
					ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.RankOrderNoTiesValidationErrorMessage) ? ResourceManager.GetString("RankOrderNoTies_Validation_ErrorMessage") : Metadata.RankOrderNoTiesValidationErrorMessage)),
					Text = "*",
					CssClass = "validator-text"
				};

				rankOrderTieValidator.ServerValidate += GetRankOrderTieValidationHandler(ControlID, container.Parent.Parent.Parent, Metadata.GroupName);

				container.Controls.Add(rankOrderTieValidator);
			}
		}

		private static ServerValidateEventHandler GetRankOrderTieValidationHandler(string id, Control container, string groupname)
		{
			return (sender, args) =>
			{
				int value;

				var valid = int.TryParse(args.Value, out value) && IsRankValid(value, id, container, groupname);

				args.IsValid = valid;
			};
		}

		protected static bool IsRankValid(int value, string id, Control container, string groupname)
		{
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
									if (input is TextBox && input.ID != id && ((TextBox)input).CssClass.Contains(groupname))
									{
										var tie = RankOrderHasTie(value, input);

										if (tie)
										{
											return false;
										}
									}
								}
							}
						}
					}
				}
			}

			return true;
		}

		protected static bool RankOrderHasTie(int value, Control control)
		{
			if (control is TextBox)
			{
				var textBox = (TextBox)control;
				int valueCompare;
				if (int.TryParse(textBox.Text, out valueCompare))
				{
					if (value == valueCompare)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
