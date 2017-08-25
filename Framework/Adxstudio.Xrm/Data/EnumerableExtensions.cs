/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Lucene.Net.Search;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Adxstudio.Xrm.Globalization;

namespace Adxstudio.Xrm.Data
{
	public static class EnumerableExtensions
	{
		public static DataTable ToDataTable(this IEnumerable<Entity> entities, OrganizationServiceContext serviceContext, Entity savedQuery = null, bool onlyGenerateSavedQueryColumns = false, string dateTimeFormat = null, IFormatProvider dateTimeFormatProvider = null)
		{
			if (entities == null)
			{
				throw new ArgumentNullException("entities");
			}

			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			var table = new DataTable();

			var entityArray = entities.ToArray();

			if (!entityArray.Any())
			{
				return table;
			}

			AddColumnsBasedOnSavedQuery(table, savedQuery);

			AddDataToTable(entityArray, table, !onlyGenerateSavedQueryColumns, dateTimeFormat, dateTimeFormatProvider);

			AddDisplayNamesToColumnCaptions(entityArray.First(), table, serviceContext);

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

		private static void AddDataToTable(IEnumerable<Entity> entities, DataTable table, bool autogenerateColumns, string dateTimeFormat = null, IFormatProvider dateTimeFormatProvider = null)
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
						var aliasedValue = attribute.Value as AliasedValue;

						object value = aliasedValue != null ? aliasedValue.Value : attribute.Value;

						var entityReference = value as EntityReference;

						if (entityReference != null)
						{
							row[attribute.Key] = entityReference.Name ??
								entityReference.Id.ToString();
						}
						else
						{
							var dateTime = value as DateTime?;

							if (dateTimeFormat != null && dateTime != null)
							{
								row[attribute.Key] = dateTimeFormatProvider == null
									? dateTime.Value.ToString(dateTimeFormat)
									: dateTime.Value.ToString(dateTimeFormat, dateTimeFormatProvider);
							}
							else
							{
								row[attribute.Key] = entity.FormattedValues.Contains(attribute.Key)
									? entity.FormattedValues[attribute.Key]
									: value ?? DBNull.Value;
							}
						}
					}
				}

				table.Rows.Add(row);
			}
		}

		private static void AddDisplayNamesToColumnCaptions(Entity entity, DataTable table, OrganizationServiceContext serviceContext)
		{
			var attributeMetadatas = serviceContext.RetrieveEntity(entity.LogicalName, EntityFilters.Attributes).Attributes;

			foreach (DataColumn column in table.Columns)
			{
				var attributeMetadata = attributeMetadatas.FirstOrDefault(metadata => metadata.LogicalName == column.ColumnName);

				if (attributeMetadata != null && attributeMetadata.DisplayName.UserLocalizedLabel != null)
				{
					column.Caption = attributeMetadata.DisplayName.GetLocalizedLabelString();
				}
			}
		}

		/// <summary>
		/// Loads a DataTable from a sequence of objects.
		/// </summary>
		/// <param name="source">The sequence of objects to load into the DataTable.</param>
		public static DataTable CopyToDataTable<T>(this IEnumerable<T> source)
		{
			return new ObjectShredder<T>().Shred(source, null, null);
		}

		/// <summary>
		/// Loads a DataTable from a sequence of objects.
		/// </summary>
		/// <param name="source">The sequence of objects to load into the DataTable.</param>
		/// <param name="table">The input table.</param>
		/// <param name="options">Specifies how values from the source sequence will be applied to 
		/// existing rows in the table.</param>
		public static DataTable CopyToDataTable<T>(this IEnumerable<T> source, DataTable table, LoadOption? options)
		{
			return new ObjectShredder<T>().Shred(source, table, options);
		}

		/// <summary>
		/// Filters a query based on whether a selected value is equal to one of the values in a given collection.
		/// </summary>
		/// <typeparam name="T">The query type.</typeparam>
		/// <typeparam name="TValue">The type of value to be compared.</typeparam>
		/// <param name="queryable">The query.</param>
		/// <param name="selector">A lamba expression that will select the value to be compared.</param>
		/// <param name="values">The collection of values to be compared against.</param>
		/// <returns>The query, with filter appended.</returns>
		/// <example>
		/// <![CDATA[
		/// var query = serviceContext.CreateQuery("adx_webpage").WhereIn(e => e.GetAttributeValue<Guid>("adx_webpageid"), pageIds);
		/// ]]>
		/// </example>
		public static IQueryable<T> WhereIn<T, TValue>(this IQueryable<T> queryable, Expression<Func<T, TValue>> selector, IEnumerable<TValue> values)
		{
			return values.Any() ? queryable.Where(In(selector, values)) : Enumerable.Empty<T>().AsQueryable();
		}

		private static Expression<Func<T, bool>> In<T, TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values)
		{
			return Expression.Lambda<Func<T, bool>>(
				In(selector.Body, values),
				selector.Parameters.First());
		}

		private static Expression In<TValue>(Expression selectorBody, IEnumerable<TValue> values)
		{
			return In(
				selectorBody,
				values.Skip(1),
				Expression.Equal(selectorBody, Expression.Constant(values.First())));
		}

		private static Expression In<TValue>(Expression selectorBody, IEnumerable<TValue> values, Expression expression)
		{
			return values.Any()
				? In(
					selectorBody,
					values.Skip(1),
					Expression.OrElse(expression, Expression.Equal(selectorBody, Expression.Constant(values.First()))))
				: expression;
		}
	}
}
