/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	
	/// <summary>
	/// Default template used to render the action for navigating to the next step.
	/// </summary>
	public class DefaultNextStepTemplate : ITemplate
	{
		internal static string DefaultNextButtonCssClass = "btn btn-primary navbar-btn button next";
		internal static string DefaultNextButtonText = "Next";

		private readonly string _validationGroup;
		private readonly string _nextButtonText;
		private readonly string _nextButtonCssClass;

		/// <summary>
		/// DefaultNextStepTemplate Class Initialization
		/// </summary>
		/// <param name="validationGroup"></param>
		public DefaultNextStepTemplate(string validationGroup)
		{
			_validationGroup = validationGroup;
		}

		/// <summary>
		/// DefaultNextStepTemplate Class Initialization
		/// </summary>
		/// <param name="validationGroup"></param>
		/// <param name="nextButtonText"></param>
		/// <param name="nextButtonCssClass"></param>
		public DefaultNextStepTemplate(string validationGroup, string nextButtonText, string nextButtonCssClass)
		{
			_validationGroup = validationGroup;
			_nextButtonText = nextButtonText;
			_nextButtonCssClass = nextButtonCssClass;
		}

		public void InstantiateIn(Control container)
		{
			var nextButton = new Button
			{
				ID = string.Format("NextButton{0}", container.ID),
				CommandName = "Next",
				Text = string.IsNullOrWhiteSpace(_nextButtonText) ? DefaultNextButtonText : _nextButtonText,
				ValidationGroup = _validationGroup,
				CausesValidation = true,
				CssClass = string.IsNullOrWhiteSpace(_nextButtonCssClass) ? DefaultNextButtonCssClass : _nextButtonCssClass
			};

			if (string.IsNullOrEmpty(nextButton.CssClass) || nextButton.CssClass == "button next" || nextButton.CssClass == "button submit"
				|| nextButton.CssClass == "btn btn-primary")
			{
				nextButton.CssClass = "btn btn-primary navbar-btn button next";
			}

			container.Controls.Add(nextButton);
		}
	}
}
