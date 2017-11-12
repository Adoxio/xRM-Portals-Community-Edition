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
	/// The default template used when rendering an action for navigating to the previous step.
	/// </summary>
	public class DefaultPreviousStepTemplate : ITemplate
	{
		internal static string DefaultPreviousButtonCssClass = "btn btn-default navbar-btn button previous";
		internal static string DefaultPreviousButtonText = "Previous";

		private readonly string _previousButtonText;
		private readonly string _previousButtonCssClass;

		/// <summary>
		/// DefaultPreviousStepTemplate Class Initialization.
		/// </summary>
		public DefaultPreviousStepTemplate() { }

		/// <summary>
		/// DefaultPreviousStepTemplate Class Initialization.
		/// </summary>
		/// <param name="previousButtonText"></param>
		/// <param name="previousButtonCssClass"></param>
		public DefaultPreviousStepTemplate(string previousButtonText, string previousButtonCssClass)
		{
			_previousButtonText = previousButtonText;
			_previousButtonCssClass = previousButtonCssClass;
		}

		public void InstantiateIn(Control container)
		{
			var previousButton = new Button
			{
				ID = string.Format("PreviousButton{0}", container.ID),
				CommandName = "Previous",
				Text = string.IsNullOrWhiteSpace(_previousButtonText) ? DefaultPreviousButtonText : _previousButtonText,
				CausesValidation = false,
				CssClass = string.IsNullOrWhiteSpace(_previousButtonCssClass) ? DefaultPreviousButtonCssClass : _previousButtonCssClass
			};

			if (string.IsNullOrEmpty(previousButton.CssClass) || previousButton.CssClass == "button next" || previousButton.CssClass == "button previous"
				|| previousButton.CssClass == "btn btn-default")
			{
				previousButton.CssClass = "btn btn-default navbar-btn button previous";
			}

			container.Controls.Add(previousButton);
		}
	}
}
