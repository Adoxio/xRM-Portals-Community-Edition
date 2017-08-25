/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Data
{
	public static class EnumerableExtensions
	{
		public static DataTable ToDataTable(this IEnumerable<Entity> entities, IOrganizationService service, Entity savedQuery = null, bool onlyGenerateSavedQueryColumns = false)
		{
			var table = new DataTable();

			if (entities == null || !entities.Any())
			{
				return table;
			}

			AddColumnsBasedOnSavedQuery(table, savedQuery);

			AddDataToTable(entities, table, !onlyGenerateSavedQueryColumns);

			AddDisplayNamesToColumnCaptions(entities.First(), table, service);

			return table;
		}

		private static void AddColumnsBasedOnSavedQuery(DataTable table, Entity savedQuery)
		{
			if (savedQuery == null)
			{
				return;
			}

			var layoutXml = XElement.Parse(savedQuery.GetAttributeValue<string>("layoutxml"));

			var layoutRow = layoutXml.Element("row");

			if (layoutRow == null)
			{
				return;
			}

			var cellNames = layoutRow.Elements("cell").Select(cell => cell.Attribute("name")).Where(name => name != null);

			foreach (var name in cellNames)
			{
				table.Columns.Add(name.Value);
			}
		}

		private static void AddDataToTable(IEnumerable<Entity> entities, DataTable table, bool autogenerateColumns)
		{
			foreach (var entity in entities)
			{
				var row = table.NewRow();

				foreach (var attribute in entity.Attributes)
				{
					if (!table.Columns.Contains(attribute.Key) && autogenerateColumns)
					{
						table.Columns.Add(attribute.Key);
					}

					if (table.Columns.Contains(attribute.Key))
					{
						var entityReference = attribute.Value as EntityReference;

						if (entityReference != null)
						{
							row[attribute.Key] = entityReference.Name ??
								entityReference.Id.ToString();
						}
						else
						{
							row[attribute.Key] = entity.FormattedValues.Contains(attribute.Key)
								? entity.FormattedValues[attribute.Key]
								: attribute.Value ?? DBNull.Value;
						}
					}
				}

				table.Rows.Add(row);
			}
		}

		private static void AddDisplayNamesToColumnCaptions(Entity entity, DataTable table, IOrganizationService service)
		{
			var attributeMetadatas = service.RetrieveEntity(entity.LogicalName, EntityFilters.Attributes).Attributes;

			foreach (DataColumn column in table.Columns)
			{
				var attributeMetadata = attributeMetadatas.FirstOrDefault(metadata => metadata.LogicalName == column.ColumnName);

				if (attributeMetadata != null && attributeMetadata.DisplayName.UserLocalizedLabel != null)
				{
					column.Caption = attributeMetadata.DisplayName.UserLocalizedLabel.Label;
				}
			}
		}
	}
}
