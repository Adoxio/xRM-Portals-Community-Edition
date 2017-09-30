/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Web.UI;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Mvc
{
	/// <summary>
	/// Implementation of <see cref="ICmsEntityEditingMetadataContainer"/> which renders to a container <see cref="TagBuilder"/>.
	/// </summary>
	public class TagBuilderCmsEntityEditingMetadataContainer : ICmsEntityEditingMetadataContainer
	{
		public TagBuilderCmsEntityEditingMetadataContainer(TagBuilder tag)
		{
			if (tag == null)
			{
				throw new ArgumentNullException("tag");
			}

			Tag = tag;
		}

		protected TagBuilder Tag { get; private set; }

		public void AddAttribute(string name, string value)
		{
			Tag.MergeAttribute(name, value, true);
		}

		public void AddCssClass(string cssClass)
		{
			Tag.AddCssClass(cssClass);
		}

		public void AddLabel(string label)
		{
			AddAttribute("data-label", label);
		}

		public void AddPicklistMetadata(string entityLogicalName, string attributeLogicalName, Dictionary<int, string> options)
		{
			var json = options.SerializeByJson(new Type[] { });

			var schemaMap = new TagBuilder("span");

			schemaMap.SetInnerText(json);
			schemaMap.AddCssClass("xrm-entity-picklist");
			schemaMap.Attributes["title"] = "{0}.{1}".FormatWith(entityLogicalName, attributeLogicalName);
			schemaMap.Attributes["style"] = "display:none;";
			schemaMap.Attributes["aria-hidden"] = "true";

			Tag.InnerHtml += schemaMap.ToString();
		}

		public void AddPreviewPermittedMetadata()
		{
			AddAttribute("data-xrm-preview-permitted", "true");
		}

		public void AddServiceReference(string servicePath, string cssClass, string title = null)
		{
			var serviceRef = new TagBuilder("a");

			serviceRef.AddCssClass(cssClass);
			serviceRef.Attributes["href"] = VirtualPathUtility.ToAbsolute(servicePath);
			serviceRef.Attributes["style"] = "display:none;";
			serviceRef.Attributes["aria-hidden"] = "true";

			if (!string.IsNullOrEmpty(title))
			{
				serviceRef.Attributes["title"] = title;
			}

			Tag.InnerHtml += serviceRef.ToString();
		}

		public void AddSiteMarkerMetadata(string entityLogicalName, string siteMarkerName)
		{
			var siteMarkerRef = new TagBuilder("span");

			siteMarkerRef.AddCssClass("xrm-entity-{0}_sitemarker".FormatWith(entityLogicalName));
			siteMarkerRef.Attributes["title"] = siteMarkerName;
			siteMarkerRef.Attributes["aria-hidden"] = "true";
			
			Tag.InnerHtml += siteMarkerRef.ToString();
		}

		public void AddTagMetadata(string entityLogicalName, IEnumerable<string> tags)
		{
			var json = new JObject
			{
				{ "tags", new JArray(tags) }
			};

			var schemaMap = new TagBuilder("span");

			schemaMap.SetInnerText(json.ToString(Formatting.None));
			schemaMap.AddCssClass("xrm-entity-tags");
			schemaMap.Attributes["title"] = entityLogicalName;
			schemaMap.Attributes["style"] = "display:none;";
			schemaMap.Attributes["aria-hidden"] = "true";

			Tag.InnerHtml += schemaMap.ToString();
		}
	}
}
