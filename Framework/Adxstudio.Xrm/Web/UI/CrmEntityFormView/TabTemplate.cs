/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.Xml.XPath;
using Adxstudio.Xrm.Mapping;
using Adxstudio.Xrm.Web.UI.HtmlControls;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when creating the layout for a tab.
	/// </summary>
	public class TabTemplate : CellContainerTemplate
	{
		private readonly IEnumerable<Entity> _webformMetadata;

		/// <summary>
		/// TableLayoutTabTemplate class initialization
		/// </summary>
		/// <param name="tabNode"></param>
		/// <param name="languageCode"></param>
		/// <param name="entityMetadata"></param>
		/// <param name="cellTemplateFactory"></param>
		/// <param name="webformMetadata"></param>
		public TabTemplate(XNode tabNode, int languageCode, EntityMetadata entityMetadata, ICellTemplateFactory cellTemplateFactory, IEnumerable<Entity> webformMetadata)
			: base(tabNode, languageCode, entityMetadata, cellTemplateFactory)
		{
			_webformMetadata = webformMetadata;

			string description;

			if (tabNode.TryGetLanguageSpecificLabelValue(this.LanguageCode, out description))
			{
				Label = description;
			}

			bool showLabel;

			if (tabNode.TryGetBooleanAttributeValue(".", "showlabel", out showLabel))
			{
				ShowLabel = showLabel;
			}
		}

		/// <summary>
		/// Text of the element's label.
		/// </summary>
		public string Label { get; private set; }

		/// <summary>
		/// Indicates if the label should be shown.
		/// </summary>
		public bool ShowLabel { get; private set; }

		public MappingFieldMetadataCollection MappingFieldCollection { get; set; }

		public override void InstantiateIn(Control container)
		{
			var tabTable = new HtmlGenericControl("div");

			string tabName;
			var tabLabel = string.Empty;
			var tabCssClassName = string.Empty;

			if (Node.TryGetAttributeValue(".", "name", out tabName))
			{
				tabTable.Attributes.Add("data-name", tabName);

				if (_webformMetadata != null)
				{
					var tabWebFormMetadata = _webformMetadata.FirstOrDefault(wfm => wfm.GetAttributeValue<string>("adx_tabname") == tabName);

					if (tabWebFormMetadata != null)
					{
						var label = tabWebFormMetadata.GetAttributeValue<string>("adx_label");

						if (!string.IsNullOrWhiteSpace(label))
						{
							tabLabel = Localization.GetLocalizedString(label, LanguageCode);
						}

						tabCssClassName = tabWebFormMetadata.GetAttributeValue<string>("adx_cssclass") ?? string.Empty;
					}
				}
			}

			tabTable.Attributes.Add("class", !string.IsNullOrWhiteSpace(tabCssClassName) ? string.Join(" ", "tab clearfix", tabCssClassName) : "tab clearfix");

			if (ShowLabel)
			{
				var caption = new HtmlGenericControl("h2") { InnerHtml = string.IsNullOrWhiteSpace(tabLabel) ? Label : tabLabel };
				caption.Attributes.Add("class", "tab-title");
				container.Controls.Add(caption);
			}

			container.Controls.Add(tabTable);

			foreach (var columnElement in Node.XPathSelectElements("columns/column"))
			{
				var col = new HtmlGenericControl("div");
				col.Attributes.Add("class", "tab-column");
				tabTable.Controls.Add(col);

				var xAttribute = columnElement.Attribute("width");
				if (xAttribute != null)
				{
					col.Style.Add("width", xAttribute.Value);
				}

				var wrapper = new HtmlGenericControl("div");
				col.Controls.Add(wrapper);

				var rowTemplateFactory = new TableLayoutRowTemplateFactory(LanguageCode);

				var sectionTemplates = columnElement.XPathSelectElements("sections/section")
					.Select(section => new TableLayoutSectionTemplate(section, LanguageCode, EntityMetadata, CellTemplateFactory, rowTemplateFactory, _webformMetadata) { MappingFieldCollection = MappingFieldCollection });

				foreach (var template in sectionTemplates)
				{
					template.InstantiateIn(wrapper);
				}
			}
		}
	}
}
