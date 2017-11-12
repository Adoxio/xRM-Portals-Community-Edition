/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// The default template used when rendering an edit form.
	/// </summary>
	public class DefaultUpdateItemTemplate : ITemplate
	{
		internal static string DefaultUpdateButtonText = "Update";

		private readonly string _validationGroup;
		private readonly string _submitButtonText;
		private readonly string _submitButtonCssClass;

		/// <summary>
		/// DefaultUpdateItemTemplate Class Initialization.
		/// </summary>
		/// <param name="validationGroup"></param>
		public DefaultUpdateItemTemplate(string validationGroup)
		{
			_validationGroup = validationGroup;
		}

		/// <summary>
		/// DefaultUpdateItemTemplate Class Initialization.
		/// </summary>
		/// <param name="validationGroup"></param>
		/// <param name="submitButtonText"></param>
		/// <param name="submitButtonCssClass"></param>
		public DefaultUpdateItemTemplate(string validationGroup, string submitButtonText, string submitButtonCssClass)
		{
			_validationGroup = validationGroup;
			_submitButtonText = submitButtonText;
			_submitButtonCssClass = submitButtonCssClass;
		}

		public void InstantiateIn(Control container)
		{
			var button = new Button
			{
				ID = string.Format("UpdateButton{0}", container.ID),
				CommandName = "Update",
				Text = string.IsNullOrWhiteSpace(_submitButtonText) ? DefaultUpdateButtonText : _submitButtonText,
				ValidationGroup = _validationGroup,
				CausesValidation = true,
				CssClass = string.IsNullOrWhiteSpace(_submitButtonCssClass) ? WebControls.CrmEntityFormView.DefaultSubmitButtonCssClass : _submitButtonCssClass,
				OnClientClick = "javascript:if(typeof Page_ClientValidate === 'function'){if(Page_ClientValidate()){clearIsDirty();}}else{clearIsDirty();}"
			};

			if (string.IsNullOrEmpty(button.CssClass) || button.CssClass == "button submit"
				|| button.CssClass == "btn btn-primary")
			{
				button.CssClass = "btn btn-primary navbar-btn button submit-btn";
			}

			container.Controls.Add(button);
		}
	}
}
