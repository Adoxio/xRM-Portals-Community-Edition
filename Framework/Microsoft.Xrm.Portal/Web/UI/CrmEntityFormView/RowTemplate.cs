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
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public abstract class RowTemplate : CellContainerTemplate
	{
		protected RowTemplate(XNode rowNode, int languageCode, EntityMetadata entityMetadata, ICellTemplateFactory cellTemplateFactory) : base(rowNode, languageCode, entityMetadata, cellTemplateFactory) { }

		public override void InstantiateIn(Control container)
		{
			var cellTemplates = Node.XPathSelectElements("cell")
				.Select(cell => CellTemplateFactory.CreateTemplate(cell, EntityMetadata))
				.Where(cell => cell.Enabled);

			if (cellTemplates.Count() < 1)
			{
				return;
			}

			InstantiateContainerIn(container, cellTemplates);
		}

		protected virtual void InstantiateContainerIn(Control container, IEnumerable<ICellTemplate> cellTemplates)
		{
			var rowContainer = new HtmlGenericControl("div");

			container.Controls.Add(rowContainer);

			rowContainer.Attributes["class"] = "row";

			InstantiateLayoutIn(rowContainer, cellTemplates);
		}

		protected abstract void InstantiateLayoutIn(Control container, IEnumerable<ICellTemplate> cellTemplates);
	}
}
