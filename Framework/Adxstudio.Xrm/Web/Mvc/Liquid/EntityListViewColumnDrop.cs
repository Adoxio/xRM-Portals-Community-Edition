/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Web.UI;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EntityListViewColumnDrop : PortalDrop
	{
		public EntityListViewColumnDrop(IPortalLiquidContext portalLiquidContext, SavedQueryView.ViewColumn column)
			: base(portalLiquidContext)
		{
			if (column == null) throw new ArgumentNullException("column");

			Column = column;

			SortAscending = Column.LogicalName + " ASC";
			SortDescending = Column.LogicalName + " DESC";
		}

		public string AttributeType { get { return Column.Metadata.AttributeType.ToString(); } }

		public string LogicalName { get { return Column.LogicalName; } }

		public string Name { get { return Column.Name; } }

		public string SortAscending { get; private set; }

		public string SortDescending { get; private set; }

		public bool SortDisabled { get { return Column.SortDisabled; } }

		public bool SortEnabled { get { return !SortDisabled; } }

		public int Width { get { return Column.Width; } }

		protected SavedQueryView.ViewColumn Column { get; private set; }

		public override object BeforeMethod(string method)
		{
			return string.Equals(method, "logicalname", StringComparison.OrdinalIgnoreCase)
				? LogicalName
				: base.BeforeMethod(method);
		}
	}
}
