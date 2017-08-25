/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Diagnostics;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	[ToolboxData("<{0}:CaptchaValidator runat=\"server\" ErrorMessage=\"*\"></{0}:CaptchaValidator>")]
	public class CaptchaValidator : BaseValidator
	{
		[Category("Behavior")]
		public string CaptchaControlID { get; set; }

		private Captcha AssociatedCaptcha
		{
			get
			{
				if (string.IsNullOrEmpty(CaptchaControlID))
				{
					throw new InvalidOperationException("No CAPTCHA control has been specified with which validation is to be performed against. Set CaptchaControlId to the ID of a valid CAPTCHA control.");
				}

				var captchaControl = NamingContainer.FindControl(CaptchaControlID) as Captcha;

				if (captchaControl == null)
				{
					throw new InvalidOperationException(string.Format("Could not find a CAPTCHA control with the ID of {0} to validate against. Set CaptchaControlId to the ID of a valid CAPTCHA control.", CaptchaControlID));
				}

				return captchaControl;
			}
		}

		protected override bool EvaluateIsValid()
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Validating");

			var controlValue = GetControlValidationValue(ControlToValidate).Trim();

			if (controlValue == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Could not find control to validate");

				return true;
			}

			return AssociatedCaptcha.Authenticate(controlValue);
		}
	}
}
