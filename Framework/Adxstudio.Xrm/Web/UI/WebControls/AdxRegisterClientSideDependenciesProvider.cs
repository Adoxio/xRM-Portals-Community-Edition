/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	public class AdxRegisterClientSideDependenciesProvider : IRegisterClientSideDependenciesProvider
	{
		private readonly IRegisterClientSideDependenciesProvider _baseProvider = new RegisterClientSideDependenciesProvider();

		public void Register(System.Web.UI.WebControls.WebControl control)
		{
			AddScriptReferencesToPage(control.Page, PreLoadScriptReferencePaths);

			_baseProvider.Register(control);

			AddScriptReferencesToPage(control.Page, ScriptReferencePaths);

			AddStyleReferencesToPage(control.Page);
		}

		private static readonly string[] PreLoadScriptReferencePaths = new[]
		{
			SiteSettings.XrmFilesRootPath + "/js/xrm-preload.js"
		};

		private static readonly string[] ScriptReferencePaths = new[]
		{
			SiteSettings.XrmFilesRootPath + "/js/xrm-combined-js.aspx"
		};

		private static readonly string[] StyleReferencePaths = new string[]
		{
			SiteSettings.XrmFilesRootPath + "/css/editable.css"
		};

		private static void AddScriptReferencesToPage(Page page, IEnumerable<string> referencePaths)
		{
			var scriptManager = ScriptManager.GetCurrent(page);

			if (scriptManager == null)
			{
				throw new InvalidOperationException("{0} requires an instance of ScriptManager to exist on the page.".FormatWith(typeof(RegisterClientSideDependenciesProvider).Name));
			}

			foreach (var path in referencePaths)
			{
				scriptManager.Scripts.Add(new ScriptReference(VirtualPathUtility.ToAbsolute(path)));
			}
		}

		private static void AddStyleReferencesToPage(Page page)
		{
			if (page.Header == null)
			{
				return;
			}

			foreach (var path in StyleReferencePaths)
			{
				var absolutePath = VirtualPathUtility.ToAbsolute(path);

				if (ControlContainsStylesheetLink(page.Header, absolutePath))
				{
					continue;
				}

				var link = new HtmlLink { Href = absolutePath };

				link.Attributes["rel"] = "stylesheet";
				link.Attributes["type"] = "text/css";

				page.Header.Controls.Add(link);
			}
		}

		private static bool ControlContainsStylesheetLink(Control container, string stylesheetPath)
		{
			foreach (var control in container.Controls)
			{
				if (control is HtmlLink && (control as HtmlLink).Href == stylesheetPath)
				{
					return true;
				}
			}

			return false;
		}
	}
}
