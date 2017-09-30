/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.UI
{
	/// <summary>
	/// Implementation of <see cref="ICmsEntityEditingMetadataContainer"/> which renders to a container <see cref="Control"/>.
	/// </summary>
	public class ControlCmsEntityEditingMetadataContainer : ICmsEntityEditingMetadataContainer
	{
		public ControlCmsEntityEditingMetadataContainer(Control container)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			Container = container;
		}

		protected Control Container { get; private set; }

		public void AddAttribute(string name, string value)
		{
			var htmlControl = Container as HtmlControl;

			if (htmlControl != null)
			{
				htmlControl.Attributes[name] = value;

				return;
			}

			var webControl = Container as WebControl;

			if (webControl != null)
			{
				webControl.Attributes[name] = value;

				return;
			}
		}

		public void AddCssClass(string cssClass)
		{
			var htmlControl = Container as HtmlControl;

			if (htmlControl != null)
			{
				var existingClasses = htmlControl.Attributes["class"];

				htmlControl.Attributes["class"] = string.IsNullOrEmpty(existingClasses)
					? cssClass
					: "{0} {1}".FormatWith(existingClasses, cssClass);

				return;
			}

			var webControl = Container as WebControl;

			if (webControl == null)
			{
				return;
			}

			webControl.CssClass = string.IsNullOrEmpty(webControl.CssClass)
				? cssClass
				: "{0} {1}".FormatWith(webControl.CssClass, cssClass);
		}

		public void AddLabel(string label)
		{
			AddAttribute("data-label", label);
		}

		public void AddPicklistMetadata(string entityLogicalName, string attributeLogicalName, Dictionary<int, string> options)
		{
			var json = options.SerializeByJson(new Type[] { });

			var schemaMap = new HtmlGenericControl("span") { InnerText = json };

			schemaMap.Attributes["class"] = "xrm-entity-picklist";
			schemaMap.Attributes["title"] = "{0}.{1}".FormatWith(entityLogicalName, attributeLogicalName);
			schemaMap.Attributes["style"] = "display:none;";
			schemaMap.Attributes["aria-hidden"] = "true";

			Container.Controls.Add(schemaMap);
		}

		public void AddPreviewPermittedMetadata()
		{
			AddAttribute("data-xrm-preview-permitted", "true");
		}

		public void AddServiceReference(string servicePath, string cssClass, string title = null)
		{
			var serviceRef = new HyperLink { CssClass = cssClass, NavigateUrl = VirtualPathUtility.ToAbsolute(servicePath), ToolTip = title };

			serviceRef.Attributes["style"] = "display:none;";
			serviceRef.Attributes["aria-hidden"] = "true";

			Container.Controls.Add(serviceRef);
		}

		public void AddSiteMarkerMetadata(string entityLogicalName, string siteMarkerName)
		{
			var siteMarkerRef = new HtmlGenericControl("span");

			siteMarkerRef.Attributes["class"] = "xrm-entity-{0}_sitemarker".FormatWith(entityLogicalName);
			siteMarkerRef.Attributes["title"] = siteMarkerName;
			siteMarkerRef.Attributes["aria-hidden"] = "true";
			
			Container.Controls.Add(siteMarkerRef);
		}

		public void AddTagMetadata(string entityLogicalName, IEnumerable<string> tags)
		{
			var json = new JObject
			{
				{ "tags", new JArray(tags) }
			};

			var schemaMap = new HtmlGenericControl("span") { InnerText = json.ToString(Formatting.None) };

			schemaMap.Attributes["class"] = "xrm-entity-tags";
			schemaMap.Attributes["title"] = entityLogicalName;
			schemaMap.Attributes["style"] = "display:none;";
			schemaMap.Attributes["aria-hidden"] = "true";

			Container.Controls.Add(schemaMap);
		}
	}
}
