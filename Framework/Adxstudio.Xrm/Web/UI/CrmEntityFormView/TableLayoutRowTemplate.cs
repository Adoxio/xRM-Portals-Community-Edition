/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used hen rendering layout for a row.
	/// </summary>
	public class TableLayoutRowTemplate : RowTemplate
	{
		/// <summary>
		/// TableLayoutRowTemplate class initialization.
		/// </summary>
		/// <param name="rowNode"></param>
		/// <param name="languageCode"></param>
		/// <param name="entityMetadata"></param>
		/// <param name="cellTemplateFactory"></param>
		public TableLayoutRowTemplate(XNode rowNode, int languageCode, EntityMetadata entityMetadata, ICellTemplateFactory cellTemplateFactory) : base(rowNode, languageCode, entityMetadata, cellTemplateFactory) { }

		public override void InstantiateIn(Control container)
		{
			var cellTemplates = Node.XPathSelectElements("cell")
				.Select(cell => CellTemplateFactory.CreateTemplate(cell, EntityMetadata))
				.Where(cell => cell.Enabled);

			InstantiateContainerIn(container, cellTemplates);
		}

		protected override void InstantiateContainerIn(Control container, IEnumerable<ICellTemplate> cellTemplates)
		{
		    var tableRow = new HtmlTableRow();
			container.Controls.Add(tableRow);

			InstantiateLayoutIn(tableRow, cellTemplates);
		}

		protected override void InstantiateLayoutIn(Control container, IEnumerable<ICellTemplate> cellTemplates)
		{
			foreach (var template in cellTemplates)
			{
				var tableCell = new HtmlTableCell
				{
					ColSpan = template.ColumnSpan.GetValueOrDefault(1), 
					RowSpan = template.RowSpan.GetValueOrDefault(1)
				};
				container.Controls.Add(tableCell);

				var sb = new StringBuilder("clearfix cell");
				if (!string.IsNullOrEmpty(template.CssClass))
				{
					sb.AppendFormat(CultureInfo.InvariantCulture, " {0}-cell", template.CssClass);
				}
				tableCell.Attributes.Add("class", sb.ToString());

				template.InstantiateIn(tableCell);
			}

			var cell = new HtmlTableCell();
			cell.Attributes.Add("class", "cell zero-cell");
			container.Controls.Add(cell);
		}
	}
}
