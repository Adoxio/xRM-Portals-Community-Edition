/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.Xrm.Client.Services
{
	/// <summary>
	/// Serialization types.
	/// </summary>
	public static class KnownTypesProvider
	{
		/// <summary>
		/// Types for serializing a <see cref="QueryExpression"/>.
		/// </summary>
		public static IEnumerable<Type> QueryExpressionKnownTypes = new List<Type>
		{
			typeof(AliasedValue),
			typeof(Dictionary<string, string>),
			typeof(Entity),
			typeof(Entity[]),
			typeof(ColumnSet),
			typeof(EntityReferenceCollection),
			typeof(QueryBase),
			typeof(QueryExpression),
			typeof(QueryExpression[]),
			typeof(LocalizedLabel[]),
			typeof(PagingInfo),
			typeof(Relationship),
			typeof(AttributePrivilegeCollection),
			typeof(RelationshipQueryCollection),
		};

		/// <summary>
		/// Types for serializing an <see cref="Entity"/>.
		/// </summary>
		public static IEnumerable<Type> EntityKnownTypes = new List<Type>
		{
			typeof(bool),
			typeof(bool[]),
			typeof(int),
			typeof(int[]),
			typeof(string),
			typeof(string[]),
			typeof(string[][]),
			typeof(double),
			typeof(double[]),
			typeof(decimal),
			typeof(decimal[]),
			typeof(Guid),
			typeof(Guid[]),
			typeof(DateTime),
			typeof(DateTime[]),
			typeof(Money),
			typeof(Money[]),
			typeof(EntityReference),
			typeof(EntityReference[]),
			typeof(OptionSetValue),
			typeof(OptionSetValue[]),
			typeof(EntityCollection),
			typeof(Money),
			typeof(Label),
			typeof(LocalizedLabel),
			typeof(LocalizedLabelCollection),
			typeof(EntityMetadata[]),
			typeof(EntityMetadata),
			typeof(AttributeMetadata[]),
			typeof(AttributeMetadata),
			typeof(RelationshipMetadataBase[]),
			typeof(RelationshipMetadataBase),
			typeof(EntityFilters),
			typeof(OptionSetMetadataBase),
			typeof(OptionSetMetadataBase[]),
			typeof(OptionSetMetadata),
			typeof(BooleanOptionSetMetadata),
			typeof(OptionSetType),
			typeof(ManagedPropertyMetadata),
			typeof(ManagedPropertyMetadata[]),
			typeof(BooleanManagedProperty),
			typeof(AttributeRequiredLevelManagedProperty),
		};
	}
}
