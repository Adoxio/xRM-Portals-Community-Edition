/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	public static class WebControlExtensions
	{
		// appends a string class to the html controls class attribute
		public static void AddClass(this WebControl control, string newClass)
		{
			if (!string.IsNullOrEmpty(control.Attributes["class"]))
			{
				control.Attributes["class"] += " " + newClass;
			}
			else
			{
				control.Attributes["class"] = newClass;
			}
		}
	}

	public static class HtmlControlExtensions
	{
		// appends a string class to the html controls class attribute
		public static void AddClass(this HtmlControl control, string newClass)
		{
			if (!string.IsNullOrEmpty(control.Attributes["class"]))
			{
				control.Attributes["class"] += " " + newClass;
			}
			else
			{
				control.Attributes["class"] = newClass;
			}
		}
	}
}
