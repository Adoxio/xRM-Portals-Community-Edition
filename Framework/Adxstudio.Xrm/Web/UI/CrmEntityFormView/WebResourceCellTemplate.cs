/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a cell containing a web resource.
	/// </summary>
	public abstract class WebResourceCellTemplate : ICellTemplate
	{
		protected const string WebResourceRouteFormat = "/_webresource/{0}";

		protected WebResourceCellTemplate(FormXmlCellMetadata metadata)
		{
			if (metadata == null) throw new ArgumentNullException("metadata");

			Metadata = metadata;
		}

		/// <summary>
		/// Number of columns the cell should take up.
		/// </summary>
		public virtual int? ColumnSpan
		{
			get { return Metadata.ColumnSpan; }
		}

		/// <summary>
		/// CSS Class name assigned to the element.
		/// </summary>
		public string CssClass
		{
			get { return "web-resource"; }
		}

		/// <summary>
		/// Indicates if the element is enabled or disabled.
		/// </summary>
		public virtual bool Enabled
		{
			get { return !Metadata.Disabled; }
		}

		protected FormXmlCellMetadata Metadata { get; private set; }

		/// <summary>
		/// Number of rows the cell should take up.
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

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				cellInfoClasses.Add("required");
			}

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
