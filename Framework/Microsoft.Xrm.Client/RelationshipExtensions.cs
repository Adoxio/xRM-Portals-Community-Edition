/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Client
{
	/// <summary>
	/// Helper methods on the <see cref="Relationship"/> class.
	/// </summary>
	public static class RelationshipExtensions
	{
		/// <summary>
		/// Converts a relationship schema name to a relationship.
		/// </summary>
		/// <param name="schemaName"></param>
		/// <param name="primaryEntityRole"></param>
		/// <returns></returns>
		public static Relationship ToRelationship(this string schemaName, EntityRole? primaryEntityRole = null)
		{
			return new Relationship(schemaName) { PrimaryEntityRole = primaryEntityRole };
		}

		/// <summary>
		/// Converts a relationship into a relationship schema name.
		/// </summary>
		/// <param name="relationship"></param>
		/// <param name="separator">Separates the schema name from the <see cref="EntityRole"/> value.</param>
		/// <returns></returns>
		public static string ToSchemaName(this Relationship relationship, string separator = ".")
		{
			var primaryEntityRole = relationship.PrimaryEntityRole != null
				? separator + relationship.PrimaryEntityRole
				: string.Empty;
			return relationship.SchemaName + primaryEntityRole;
		}
	}
}
