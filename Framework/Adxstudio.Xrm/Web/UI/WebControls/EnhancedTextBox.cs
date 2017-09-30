/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	[DefaultProperty("Text")]
	[ToolboxData("<{0}:EnhancedTextBox runat=server></{0}:EnhancedTextBox>")]
	public class EnhancedTextBox : TextBox
	{
		private const string _maxLengthAttributeName = "exMaxLen";

		private static readonly string[] ScriptIncludes =
		{
			"~/xrm-adx/js/crmentityformview.js"
		};

		override protected void OnPreRender(EventArgs e) 
		{ 
			// Ensure default work takes place
			base.OnPreRender(e);

			// Configure TextArea support
			if ((TextMode == TextBoxMode.MultiLine) && (MaxLength > 0))
			{
				// If we haven't already, include the supporting
				// script that limits the content of textareas.
				foreach (var script in ScriptIncludes)
				{
					var scriptManager = ScriptManager.GetCurrent(Page);

					if (scriptManager == null)
					{
						continue;
					}

					var absolutePath = VirtualPathUtility.ToAbsolute(script);

					scriptManager.Scripts.Add(new ScriptReference(absolutePath));
				}

				// Add an expando attribute to the rendered control which sets  its maximum length (using the MaxLength Attribute)

				/* Where there is a ScriptManager on the parent page, use it to register the attribute -
				 * to ensure the control works in partial updates (like an AJAX UpdatePanel)*/

				var current = ScriptManager.GetCurrent(Page);
				if (current != null && (current.GetRegisteredExpandoAttributes().All(rea => rea.ControlId != ClientID)))
				{
					ScriptManager.RegisterExpandoAttribute(this, ClientID, _maxLengthAttributeName,
						MaxLength.ToString(CultureInfo.InvariantCulture), true);
				}
				else
				{
					try
					{
						Page.ClientScript.RegisterExpandoAttribute(ClientID, _maxLengthAttributeName,
							MaxLength.ToString(CultureInfo.InvariantCulture));
					}
					catch (ArgumentException)
					{
						// This occurs if a script with this key has already been registered. The response should be to do nothing.
					}
				}

				// Now bind the onkeydown, oninput and onpaste events to script to inject in parent page.
				Attributes.Add("onkeydown", "javascript:return LimitInput(this, event);");
				Attributes.Add("oninput",  "javascript:return LimitInput(this, event);");
				Attributes.Add("onpaste",  "javascript:return LimitPaste(this, event);");
			}
		}
	}
}
