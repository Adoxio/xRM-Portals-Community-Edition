/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using System.Web.UI;
using System.Security.Permissions;
using System.ComponentModel;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class CrmDataSourceSelectingEventArgs : CancelEventArgs // MSBug #120088: Won't make internal, part of public event API.
	{
		public CrmDataSourceSelectingEventArgs(
			CrmDataSource dataSource,
			DataSourceSelectArguments arguments,
			string fetchXml,
			QueryByAttribute query)
		{
			_dataSource = dataSource;
			_arguments = arguments;
			_fetchXml = fetchXml;
			_query = query;
		}

		private CrmDataSource _dataSource;

		public CrmDataSource DataSource
		{
			get { return _dataSource; }
		}

		private readonly DataSourceSelectArguments _arguments;

		public DataSourceSelectArguments Arguments
		{
			get	{ return _arguments; }
		}

		private string _fetchXml;

		public string FetchXml
		{
			get { return _fetchXml; }
			set { _fetchXml = value; }
		}

		private readonly QueryByAttribute _query;

		public QueryByAttribute Query
		{
			get { return _query; }
		}
	}
}
