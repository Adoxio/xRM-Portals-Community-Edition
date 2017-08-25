/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	internal interface ICustomFieldControlTemplate
	{
		string ControlID { get; }
		
		CrmEntityFormViewField Field { get; }
	}

	internal static class ICustomFieldControlTemplateExtensions
	{
		public static void InstantiateCustomValidatorsIn(this ICustomFieldControlTemplate controlTemplate, Control container)
		{
			if (controlTemplate == null ||
				controlTemplate.Field == null ||
				controlTemplate.Field.CustomValidatorsTemplate == null)
			{
				return;
			}

			controlTemplate.Field.CustomValidatorsTemplate.InstantiateIn(container);

			if (controlTemplate is BooleanControlTemplate)
			{
				//setting the ControlToValidate on a checkbox control will generate a runtime error
				//as the argument passed to the ServerValidate event always contains an empty string
				return;
			}

			foreach (var validator in container.Controls.All().OfType<BaseValidator>())
			{
				validator.ControlToValidate = controlTemplate.ControlID;
			}
		}

		private static IEnumerable<Control> All(this ControlCollection controls)
		{
			foreach (Control control in controls)
			{
				foreach (var child in control.Controls.All())
				{
					yield return child;
				}

				yield return control;
			}
		}
	}
}
