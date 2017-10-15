/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	public class CheckboxValidator : CustomValidator
	{
		protected CheckBox CheckBoxToValidate
		{
			get
			{
				return FindControl(ControlToValidate) as CheckBox;
			}
		}

		protected override bool ControlPropertiesValid()
		{
			// Is ControlToValidate Set
			if (this.ControlToValidate.Length == 0)
			{
				throw new HttpException(string.Format(CultureInfo.InvariantCulture, "The ControlToValidate property {0} can't be blank.", this.ID));
			}

			

			// Ensure that the control being validated is a CheckBox
			if (CheckBoxToValidate == null)
			{
				throw new HttpException(string.Format(CultureInfo.InvariantCulture, "The control for validation must be a CheckBox control."));
			}

			return true;
		}

		/// <summary>
		/// This method determines if the value of the input control is valid.
		/// </summary>
		/// <returns>
		/// true if it is valid or false if not.
		/// </returns>
		protected override bool EvaluateIsValid()
		{

			if (CheckBoxToValidate != null) return CheckBoxToValidate.Checked;

			return false;
		}
	}
}
