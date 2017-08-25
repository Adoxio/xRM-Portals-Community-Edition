/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;

namespace Adxstudio.SharePoint.Collections.Generic
{
	/// <summary>
	/// Methods for constructing a connection string dictionary.
	/// </summary>
	/// <remarks>
	/// The resulting dictionary uses a case insensitive key comparer.
	/// </remarks>
	internal static class ConnectionStringExtensions
	{
		/// <summary>
		/// Builds a connection string dictionary from a named configuration connection string.
		/// </summary>
		/// <param name="connectionStringName"></param>
		/// <returns></returns>
		public static IDictionary<string, string> ToDictionaryFromConnectionStringName(this string connectionStringName)
		{
			return ToDictionary(ConfigurationManager.ConnectionStrings[connectionStringName]);
		}

		/// <summary>
		/// Builds a connection string dictionary from a configuration connection string.
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		public static IDictionary<string, string> ToDictionary(this ConnectionStringSettings connectionString)
		{
			return ToDictionary(connectionString.ConnectionString);
		}

		/// <summary>
		/// Builds a connection string dictionary that is parsed from a string.
		/// </summary>
		/// <remarks>
		/// The format of the string must follow the format that is understood by the <see cref="DbConnectionStringBuilder"/> class.
		/// </remarks>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		public static IDictionary<string, string> ToDictionary(this string connectionString)
		{
			var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
			var connection = builder.Cast<KeyValuePair<string, object>>().ToDictionary(pair => pair.Key, pair => pair.Value.ToString());
			return new Dictionary<string, string>(connection, StringComparer.OrdinalIgnoreCase);
		}
	}
}
