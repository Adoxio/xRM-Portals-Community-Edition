/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.ComponentModel;
using System.Web.UI;
using Microsoft.Xrm.Client.Diagnostics;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	[ToolboxData("<{0}:SavedQueryDataSource runat=\"server\"> </{0}:SavedQueryDataSource>")]
	public sealed class SavedQueryDataSource : DataSourceControl
	{
		private SavedQueryDataSourceView _view;

		/// <summary>
		/// Gets or sets the name of the data context to be used to connect to Microsoft Dynamics CRM.
		/// </summary>
		[Category("Data")]
		public string CrmDataContextName { get; set; }

		/// <summary>
		/// Gets or sets the name of the saved query.
		/// </summary>
		[Category("Data")]
		[DefaultValue((string)null)]
		public string SavedQueryName { get; set; }

		protected override DataSourceView GetView(string viewName)
		{
			Tracing.FrameworkInformation(GetType().Name, "GetView", "viewName={0}", viewName);

			return _view ?? (_view = new SavedQueryDataSourceView(this, viewName));
		}
	}
}


