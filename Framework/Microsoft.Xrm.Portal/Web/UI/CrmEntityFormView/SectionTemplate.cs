/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public class SectionTemplate : CellContainerTemplate
	{
		public SectionTemplate(XNode sectionNode, int languageCode, EntityMetadata entityMetadata, ICellTemplateFactory cellTemplateFactory, RowTemplateFactory rowTemplateFactory)
			: base(sectionNode, languageCode, entityMetadata, cellTemplateFactory)
		{
			rowTemplateFactory.ThrowOnNull("rowTemplateFactory");

			RowTemplateFactory = rowTemplateFactory;

			string description;

			if (XNodeUtility.TryGetAttributeValue(sectionNode, "labels/label[@languagecode='{0}']".FormatWith(LanguageCode), "description", out description))
			{
				Label = description;
			}

			bool showLabel;

			if (XNodeUtility.TryGetBooleanAttributeValue(sectionNode, ".", "showlabel", out showLabel))
			{
				ShowLabel = showLabel;
			}

			bool showBar;

			if (XNodeUtility.TryGetBooleanAttributeValue(sectionNode, ".", "showbar", out showBar))
			{
				ShowBar = showBar;
			}
		}

		public string Label { get; private set; }

		public bool ShowLabel { get; private set; }

		public bool ShowBar { get; private set; }

		protected RowTemplateFactory RowTemplateFactory { get; private set; }

		public override void InstantiateIn(Control container)
		{
			var sectionContainer = new HtmlGenericControl("fieldset");

			container.Controls.Add(sectionContainer);

			if (ShowLabel)
			{
				var control = new HtmlGenericControl("legend") { InnerText = Label };

				if (ShowBar)
				{
					control.Attributes.Add("class", "show-bar");
				}

				sectionContainer.Controls.Add(control);
			}

			var rowTemplates = Node.XPathSelectElements("rows/row").Select(row => RowTemplateFactory.CreateTemplate(row, EntityMetadata, CellTemplateFactory));

			foreach (var template in rowTemplates)
			{
				template.InstantiateIn(sectionContainer);
			}
		}
	}
}
