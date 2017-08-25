/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI
{
	public sealed class CrmSavedQueryColumnsGenerator : IAutoFieldGenerator
	{
		private readonly Entity _savedQuery;
		private readonly OrganizationServiceContext _context;

		public CrmSavedQueryColumnsGenerator(string savedQueryName) : this(savedQueryName, null) { }

		public CrmSavedQueryColumnsGenerator(string savedQueryName, string crmConnectionStringName)
		{
			_context = string.IsNullOrEmpty(crmConnectionStringName)
				? OrganizationServiceContextFactory.Create()
				: OrganizationServiceContextFactory.Create(crmConnectionStringName);

			_savedQuery = _context.CreateQuery("savedquery").FirstOrDefault(query => query.GetAttributeValue<string>("name") == savedQueryName);

			if (_savedQuery == null)
			{
				throw new ArgumentException("A saved query with the name {0} could not be found.".FormatWith(savedQueryName));
			}
		}

		public ICollection GenerateFields(Control control)
		{
			var layoutXml = XElement.Parse(_savedQuery.GetAttributeValue<string>("layoutxml"));

			var cellNames = layoutXml.Element("row").Elements("cell").Select(cell => cell.Attribute("name")).Where(name => name != null);

			var fetchXml = XElement.Parse(_savedQuery.GetAttributeValue<string>("fetchxml"));

			var entityName = fetchXml.Element("entity").Attribute("name").Value;

			var response = _context.Execute(new RetrieveEntityRequest { LogicalName = entityName, EntityFilters = EntityFilters.Attributes }) as RetrieveEntityResponse;
			var attributeMetadatas = response.EntityMetadata.Attributes;

			var fields =
				from name in cellNames
				let attributeMetadata = attributeMetadatas.FirstOrDefault(metadata => metadata.LogicalName == name.Value)
				where attributeMetadata != null
				select new BoundField
				{
					DataField = name.Value,
					SortExpression = name.Value,
					HeaderText = attributeMetadata.DisplayName.UserLocalizedLabel.Label // MSBug #120122: No need to URL encode--encoding is handled by webcontrol rendering layer.
				};

			return fields.ToList();
		}
	}
}
