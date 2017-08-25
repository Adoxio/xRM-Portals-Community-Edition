/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Site.Helpers
{
	public static class DataTableExtensions
	{
		public static void ExportToCsv(this DataTable table, string name, HttpContext context, IList<string> hideColumns = null)
		{
			context.Response.Clear();
			context.Response.ClearHeaders();
			context.Response.ClearContent();
			context.Response.Charset = Encoding.UTF8.WebName;
			context.Response.ContentEncoding = Encoding.UTF8;
			context.Response.BinaryWrite(Encoding.UTF8.GetPreamble());
			
			if (hideColumns == null)
			{
				hideColumns = new List<string>();
			}

			var columns = table.Columns.Cast<DataColumn>().Where(column => !hideColumns.Contains(column.Caption)).ToList();

			foreach (var column in columns)
			{
				context.Response.Write(EncodeCommaSeperatedValue(column.Caption));
			}

			context.Response.Write(Environment.NewLine);

			foreach (DataRow row in table.Rows)
			{
				foreach (var column in columns)
				{
					context.Response.Write(EncodeCommaSeperatedValue(row[column.ColumnName].ToString()));
				}

				context.Response.Write(Environment.NewLine);
			}

			context.Response.ContentType = "text/csv";

			context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + name);

			context.Response.End();
		}

		private static string EncodeCommaSeperatedValue(string value)
		{
			return !string.IsNullOrEmpty(value)
				? string.Format(@"""{0}"",", value.Replace(@"""", @""""""))
				: ",";
		}
	}
}
