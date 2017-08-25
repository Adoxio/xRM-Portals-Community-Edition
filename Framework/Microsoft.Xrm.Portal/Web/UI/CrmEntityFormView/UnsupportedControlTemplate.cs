/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public class UnsupportedControlTemplate : CellTemplate
	{
		private readonly bool _enabled;

		public UnsupportedControlTemplate(ICellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings, bool enabled) : base(metadata, validationGroup, bindings)
		{
			_enabled = enabled;
		}

		public override string CssClass
		{
			get { return "unsupported"; }
		}

		public override bool Enabled
		{
			get { return _enabled; }
		}

		protected override void InstantiateControlIn(Control container)
		{
			// If the field is enabled, print some debugging info about the unsupported field.
			container.Controls.Add(new Label { ID = ControlID, Text = @"Unsupported: DataFieldName=""{0}"" Label=""{1}"", AttributeType=""{2}"", Format=""{3}""".FormatWith(Metadata.DataFieldName, Metadata.Label, Metadata.AttributeType, Metadata.Format) });
		}
	}
}
