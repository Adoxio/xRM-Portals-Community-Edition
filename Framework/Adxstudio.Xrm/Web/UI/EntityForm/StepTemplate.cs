/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI;
using System.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.UI.EntityForm
{
	internal class StepTemplate : ITemplate
	{
		private readonly string _buttonID;
		private readonly string _commandName;
		private readonly string _text;
		private readonly string _validationGroup;
		private readonly string _cssClass;
		private readonly bool _causeValidation;

		public StepTemplate(string buttonID, string commmandName, string text, string validationGroup, string cssClass, bool causeValidation)
		{
			_buttonID = buttonID;
			_commandName = commmandName;
			_text = text;
			_validationGroup = validationGroup;
			_cssClass = cssClass;
			_causeValidation = causeValidation;
		}

		public void InstantiateIn(Control container)
		{
			container.Controls.Add(new Button
			{
				ID = _buttonID,
				CommandName = _commandName,
				Text = _text,
				ValidationGroup = _validationGroup,
				CssClass = _cssClass,
				CausesValidation = _causeValidation
			});
		}
	}
}
