/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Runtime;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public abstract class CellTemplate : ICellTemplate
	{
		protected CellTemplate(ICellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
		{
			metadata.ThrowOnNull("metadata");
			bindings.ThrowOnNull("bindings");

			Bindings = bindings;
			Metadata = metadata;
			ValidationGroup = validationGroup;
		}

		public virtual int? ColumnSpan
		{
			get { return Metadata.ColumnSpan; }
		}

		public abstract string CssClass { get; }

		public virtual bool Enabled
		{
			get { return !Metadata.Disabled; }
		}

		protected IDictionary<string, CellBinding> Bindings { get; private set; }

		public virtual string ControlID
		{
			get { return Metadata.DataFieldName; }
		}

		protected ICellMetadata Metadata { get; private set; }

		public virtual int? RowSpan
		{
			get { return Metadata.RowSpan; }
		}

		protected string ValidationGroup { get; private set; }

		public virtual void InstantiateIn(Control container)
		{
			if (!Enabled)
			{
				return;
			}

			var cellInfoContainer = new HtmlGenericControl("div");

			container.Controls.Add(cellInfoContainer);

			var cellInfoClasses = new List<string> { "info" };

			if (Metadata.IsRequired)
			{
				cellInfoClasses.Add("required");
			}

			cellInfoContainer.Attributes["class"] = string.Join(" ", cellInfoClasses.ToArray());

			if (Metadata.ShowLabel)
			{
				cellInfoContainer.Controls.Add(new Label { AssociatedControlID = ControlID, Text = Metadata.Label, ToolTip = Metadata.ToolTip });
			}

			var validatorContainer = new HtmlGenericControl("div");

			cellInfoContainer.Controls.Add(validatorContainer);

			validatorContainer.Attributes["class"] = "validators";

			InstantiateValidatorsIn(validatorContainer);

			var controlContainer = new HtmlGenericControl("div");

			container.Controls.Add(controlContainer);

			controlContainer.Attributes["class"] = "control";

			InstantiateControlIn(controlContainer);
		}

		protected abstract void InstantiateControlIn(Control container);

		protected virtual void InstantiateValidatorsIn(Control container) { }
	}
}
