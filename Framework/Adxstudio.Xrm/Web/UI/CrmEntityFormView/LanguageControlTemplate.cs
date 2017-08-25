/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Whole Number field when Format equals Language.
	/// </summary>
	public class LanguageControlTemplate : IntegerControlTemplate
	{
		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public LanguageControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings) : base(field, metadata, validationGroup, bindings)
		{
		}

		protected override void InstantiateControlIn(Control container)
		{
			var dropDown = new CrmLanguage { ID = ControlID, CssClass = string.Join(" ", CssClass, Metadata.CssClass), ToolTip = Metadata.ToolTip, LanguageCode = Metadata.LanguageCode };

			dropDown.Attributes.Add("onchange", "setIsDirty(this.id);");

			container.Controls.Add(dropDown);

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				dropDown.Attributes.Add("required", string.Empty);
			}

			if (Metadata.ReadOnly)
			{
				AddSpecialBindingAndHiddenFieldsToPersistDisabledSelect(container, dropDown);
			}
			else
			{
				Bindings[Metadata.DataFieldName] = new CellBinding
				{
					Get = () =>
					{
						int value;
						return int.TryParse(dropDown.SelectedValue, out value) ? new int?(value) : null;
					},
					Set = obj =>
					{
						dropDown.SelectedValue = "{0}".FormatWith(obj);
					}
				};
			}
		}

		private void AddSpecialBindingAndHiddenFieldsToPersistDisabledSelect(Control container, DropDownList dropDown)
		{
			dropDown.CssClass = "{0} readonly".FormatWith(CssClass);
			dropDown.Attributes["disabled"] = "disabled";
			dropDown.Attributes["aria-disabled"] = "true";

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
					return int.TryParse(hiddenValue.Value, out value) ? new int?(value) : null;
				},
				Set = obj =>
				{
					var value = "{0}".FormatWith(obj);
					dropDown.SelectedValue = value;
					hiddenValue.Value = dropDown.SelectedValue;
					hiddenSelectedIndex.Value = dropDown.SelectedIndex.ToString(CultureInfo.InvariantCulture);
				}
			};
		}
	}
}
