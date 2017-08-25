/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web.UI;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Cell Template used to ignore a cell and thefore not render a control template.
	/// </summary>
	public class IgnoreCellTemplate : Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.CellTemplate
	{
		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public IgnoreCellTemplate(ICellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{
		}

		/// <summary>
		/// CSS Class
		/// </summary>
		public override string CssClass
		{
			get { return string.Empty; }
		}

		/// <summary>
		/// Enabled
		/// </summary>
		public override bool Enabled
		{
			get { return false; }
		}

		protected override void InstantiateControlIn(Control container)
		{
		}
	}
}
