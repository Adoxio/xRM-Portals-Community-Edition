/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public class SavedQueryRowTemplate : RowTemplate
	{
		public SavedQueryRowTemplate(XNode rowNode, int languageCode, EntityMetadata entityMetadata, ICellTemplateFactory cellTemplateFactory) : base(rowNode, languageCode, entityMetadata, cellTemplateFactory) { }

		protected override void InstantiateContainerIn(Control container, IEnumerable<ICellTemplate> cellTemplates)
		{
			var fieldset = new HtmlGenericControl("fieldset");

			container.Controls.Add(fieldset);

			InstantiateLayoutIn(fieldset, cellTemplates);
		}

		protected override void InstantiateLayoutIn(Control container, IEnumerable<ICellTemplate> cellTemplates)
		{
			foreach (var template in cellTemplates)
			{
				var rowContainer = new HtmlGenericControl("div");

				container.Controls.Add(rowContainer);

				rowContainer.Attributes["class"] = "row";

				var cellContainer = new HtmlGenericControl("div");

				rowContainer.Controls.Add(cellContainer);

				var cellContainerCssClasses = new List<string> { "cell", "{0}-cell".FormatWith(template.CssClass) };

				cellContainer.Attributes["class"] = string.Join(" ", cellContainerCssClasses.ToArray());

				template.InstantiateIn(cellContainer);
			}
		}
	}
}
