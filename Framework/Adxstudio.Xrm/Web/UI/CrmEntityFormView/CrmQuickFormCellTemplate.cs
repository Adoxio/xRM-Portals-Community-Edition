/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a quick form cell.
	/// </summary>
	public abstract class CrmQuickFormCellTemplate : ICellTemplate
	{
		protected CrmQuickFormCellTemplate(FormXmlCellMetadata metadata)
		{
			if (metadata == null) throw new ArgumentNullException("metadata");

			Metadata = metadata;
		}

		/// <summary>
		/// Number of columns the cell is to take up.
		/// </summary>
		public virtual int? ColumnSpan
		{
			get { return Metadata.ColumnSpan; }
		}

		/// <summary>
		/// CSS Class name assigned.
		/// </summary>
		public string CssClass
		{
			get { return "crmquickform"; }
		}

		/// <summary>
		/// Indicates if the control in the cell is enabled or disabled.
		/// </summary>
		public virtual bool Enabled
		{
			get { return !Metadata.Disabled; }
		}

		protected FormXmlCellMetadata Metadata { get; private set; }

		/// <summary>
		/// Number of rows a cell should take up
		/// </summary>
		public virtual int? RowSpan
		{
			get { return Metadata.RowSpan; }
		}

		public void InstantiateIn(Control container)
		{
			if (!Enabled)
			{
				return;
			}

			var cellInfoContainer = new HtmlGenericControl("div");

			container.Controls.Add(cellInfoContainer);

			var cellInfoClasses = new List<string> { "info" };

			cellInfoContainer.Attributes["class"] = string.Join(" ", cellInfoClasses.ToArray());

			if (Metadata.ShowLabel)
			{
				cellInfoContainer.Controls.Add(new Label { AssociatedControlID = Metadata.ControlID, Text = Metadata.Label, ToolTip = Metadata.ToolTip });
			}

			var controlContainer = new HtmlGenericControl("div");

			container.Controls.Add(controlContainer);

			controlContainer.Attributes["class"] = "control";

			InstantiateControlIn(controlContainer);
		}

		protected abstract void InstantiateControlIn(HtmlControl container);
	}
}
