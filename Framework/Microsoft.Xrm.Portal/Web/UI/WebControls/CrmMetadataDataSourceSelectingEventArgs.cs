/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using System.Web.UI;
using System.Security.Permissions;
using System.ComponentModel;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CrmMetadataDataSourceSelectingEventArgs : CancelEventArgs
	{
		private CrmMetadataDataSource _dataSource;

		public CrmMetadataDataSource DataSource
		{
			get { return _dataSource; }
		}

		private readonly DataSourceSelectArguments _arguments;

		public DataSourceSelectArguments Arguments
		{
			get { return _arguments; }
		}

		private EntityFilters _metadataFlags;

		public EntityFilters MetadataFlags
		{
			get { return _metadataFlags; }
			set { _metadataFlags = value; }
		}

		private EntityFilters _entityFlags;

		public EntityFilters EntityFlags
		{
			get { return _entityFlags; }
			set { _entityFlags = value; }
		}

		private string _entityName;

		public string EntityName
		{
			get { return _entityName; }
			set { _entityName = value; }
		}

		private string _attributeName;

		public string AttributeName
		{
			get { return _attributeName; }
			set { _attributeName = value; }
		}

		private string _sortExpression;

		public string SortExpression
		{
			get { return _sortExpression; }
			set { _sortExpression = value; }
		}

		public CrmMetadataDataSourceSelectingEventArgs(
			CrmMetadataDataSource dataSource,
			DataSourceSelectArguments arguments,
			string entityName,
			string attributeName,
			EntityFilters metadataFlags,
			EntityFilters entityFlags,
			string sortExpression)
		{
			_dataSource = dataSource;
			_arguments = arguments;
			_entityName = entityName;
			_attributeName = attributeName;
			_metadataFlags = metadataFlags;
			_entityFlags = entityFlags;
			_sortExpression = sortExpression;
		}
	}
}
