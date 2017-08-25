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
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Mapping;
using Adxstudio.Xrm.Web.UI.HtmlControls;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering layout for a section.
	/// </summary>
	public class TableLayoutSectionTemplate : SectionTemplate
	{
		private readonly IEnumerable<Entity> _webformMetadata;

		/// <summary>
		/// TableLayoutSectionTemplate class initialization.
		/// </summary>
		/// <param name="sectionNode"></param>
		/// <param name="languageCode"></param>
		/// <param name="entityMetadata"></param>
		/// <param name="cellTemplateFactory"></param>
		/// <param name="rowTemplateFactory"></param>
		/// <param name="webformMetadata"></param>
		public TableLayoutSectionTemplate(XNode sectionNode, int languageCode, EntityMetadata entityMetadata, ICellTemplateFactory cellTemplateFactory, TableLayoutRowTemplateFactory rowTemplateFactory, IEnumerable<Entity> webformMetadata) : base(sectionNode, languageCode, entityMetadata, cellTemplateFactory, rowTemplateFactory)
		{
			_webformMetadata = webformMetadata;
		}

		public MappingFieldMetadataCollection MappingFieldCollection { get; set; }

		public override void InstantiateIn(Control container)
		{
			var wrapper = new HtmlGenericControl("fieldset");
			container.Controls.Add(wrapper);

			var sectionTable = new HtmlGenericControl("table");
			
			sectionTable.Attributes.Add("role", "presentation");

			string sectionName;
			var sectionLabel = string.Empty;
			var sectionCssClassName = string.Empty;
			string visibleProperty;
			var visible = true;

			Node.TryGetAttributeValue(".", "visible", out visibleProperty);

			if (!string.IsNullOrWhiteSpace(visibleProperty))
			{
				bool.TryParse(visibleProperty, out visible);
			}

			if (!visible)
			{
				return;
			}

			if (Node.TryGetAttributeValue(".", "name", out sectionName))
			{
				sectionTable.Attributes.Add("data-name", sectionName);

				if (_webformMetadata != null)
				{
					var sectionWebFormMetadata = _webformMetadata.FirstOrDefault(wfm => wfm.GetAttributeValue<string>("adx_sectionname") == sectionName);

					if (sectionWebFormMetadata != null)
					{
						var label = sectionWebFormMetadata.GetAttributeValue<string>("adx_label");

						if (!string.IsNullOrWhiteSpace(label))
						{
							sectionLabel = Localization.GetLocalizedString(label, LanguageCode);
						}

						sectionCssClassName = sectionWebFormMetadata.GetAttributeValue<string>("adx_cssclass") ?? string.Empty;
					}
				}
			}

			sectionTable.Attributes.Add("class", !string.IsNullOrWhiteSpace(sectionCssClassName) ? string.Join(" ", "section", sectionCssClassName) : "section");

			if (ShowLabel)
			{
				var caption = new HtmlGenericControl("legend") { InnerHtml = string.IsNullOrWhiteSpace(sectionLabel) ? Label : sectionLabel };

				var cssClass = "section-title";
				if (ShowBar)
				{
					cssClass += " show-bar";
				}

				caption.Attributes.Add("class", cssClass);

				wrapper.Controls.Add(caption);
			}

			var colgroup = new HtmlGenericControl("colgroup");
			sectionTable.Controls.Add(colgroup);

			if (PortalSettings.Instance.BingMapsSupported && !string.IsNullOrWhiteSpace(sectionName) && sectionName.EndsWith("section_map"))
			{
				var bingmap = new BingMap { ClientIDMode = ClientIDMode.Static, MappingFieldCollection = MappingFieldCollection };

				sectionTable.Controls.Add(bingmap);
			}
			
			string columns;

			if (Node.TryGetAttributeValue(".", "columns", out columns))
			{
				// For every column there is a "1" in the columns attribute... 1=1, 11=2, 111=3, etc.)
				foreach (var column in columns)
				{
					var width = 1 / (double)columns.Length * 100;
					var col = new SelfClosingHtmlGenericControl("col");
					col.Style.Add(HtmlTextWriterStyle.Width, "{0}%".FormatWith(width));
					colgroup.Controls.Add(col);
				}
				colgroup.Controls.Add(new SelfClosingHtmlGenericControl("col"));
			}

			wrapper.Controls.Add(sectionTable);

			var rowTemplates = Node.XPathSelectElements("rows/row").Select(row => RowTemplateFactory.CreateTemplate(row, EntityMetadata, CellTemplateFactory));

			foreach (var template in rowTemplates)
			{
				template.InstantiateIn(sectionTable);
			}
		}
	}
}
