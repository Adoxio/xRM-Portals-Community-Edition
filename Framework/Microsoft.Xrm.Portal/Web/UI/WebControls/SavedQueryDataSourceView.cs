/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections;
using System.Data;
using System.Linq;
using System.Web.UI;
using Microsoft.Xrm.Portal.Data;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	public sealed class SavedQueryDataSourceView : DataSourceView
	{
		/// <summary>
		/// Gets the owner data source.
		/// </summary>
		private SavedQueryDataSource Owner { get; set; }

		public SavedQueryDataSourceView(SavedQueryDataSource owner, string viewName) : base(owner, viewName)
		{
			Owner = owner;
		}

		protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
		{
			var context = OrganizationServiceContextFactory.Create(Owner.CrmDataContextName);

			var savedQuery = context.CreateQuery("savedquery")
				.FirstOrDefault(query => query.GetAttributeValue<string>("name") == Owner.SavedQueryName);

			var fetchXml = savedQuery.GetAttributeValue<string>("fetchxml");

			var queryExpression = new FetchExpression(fetchXml);

			var queryResults = context.RetrieveMultiple(queryExpression).Entities;

			var dataTable = queryResults.ToDataTable(context, savedQuery, true);

			return new DataView(dataTable);
		}
	}
}
