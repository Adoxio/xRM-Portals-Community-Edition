/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public sealed class GridLayoutRowTemplate : RowTemplate
	{
		public GridLayoutRowTemplate(XNode rowNode, int languageCode, EntityMetadata entityMetadata, ICellTemplateFactory cellTemplateFactory) : base(rowNode, languageCode, entityMetadata, cellTemplateFactory) { }

		protected override void InstantiateLayoutIn(Control container, IEnumerable<ICellTemplate> cellTemplates)
		{
			var layoutContainer = new HtmlGenericControl("div");

			container.Controls.Add(layoutContainer);

			var layoutContainerCssClasses = new List<string>();

			var hasLayout = false;

			switch (cellTemplates.Count())
			{
			case 2:
				layoutContainerCssClasses.Add("yui-g");
				hasLayout = true;
				break;
			case 3:
				layoutContainerCssClasses.Add("yui-gb");
				hasLayout = true;
				break;
			}

			if (layoutContainerCssClasses.Any())
			{
				layoutContainer.Attributes["class"] = string.Join(" ", layoutContainerCssClasses.ToArray());
			}

			var index = 0;

			foreach (var template in cellTemplates)
			{
				var cellContainer = new HtmlGenericControl("div");

				layoutContainer.Controls.Add(cellContainer);

				var cellContainerCssClasses = new List<string> { "cell", "{0}-cell".FormatWith(template.CssClass) };

				if (hasLayout)
				{
					cellContainerCssClasses.Add("yui-u");
				}

				if (hasLayout && index == 0)
				{
					cellContainerCssClasses.Add("first");
				}

				cellContainer.Attributes["class"] = string.Join(" ", cellContainerCssClasses.ToArray());

				template.InstantiateIn(cellContainer);

				index++;
			}
		}
	}
}
