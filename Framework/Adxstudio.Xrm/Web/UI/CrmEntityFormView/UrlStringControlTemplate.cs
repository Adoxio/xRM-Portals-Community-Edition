/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Single Line of Text field with format set to URL.
	/// </summary>
	public class UrlStringControlTemplate : StringControlTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// UrlStringControlTemplate initialization.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public UrlStringControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(field, metadata, validationGroup, bindings)
		{
			Field = field;
		}

		/// <summary>
		/// Form Field
		/// </summary>
		public new CrmEntityFormViewField Field { get; private set; }

		protected override void InstantiateControlIn(Control container)
		{
			var textbox = new TextBox { ID = ControlID, CssClass = string.Join(" ", CssClass, Metadata.CssClass), ToolTip = Metadata.ToolTip };

			textbox.Style.Add("text-decoration", "underline");
			textbox.Attributes["type"] = "url";
			textbox.Attributes["ondblclick"] = "launchUrl(this.value);";
			textbox.Attributes["onchange"] = string.Format("setIsDirty(this.id);validateUrlInput(this,{0});", Metadata.MaxLength);
			
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
	}
}
