/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Client.Services.Messages
{
	/// <summary>
	/// Categorizations for cache items.
	/// </summary>
	[Flags]
	public enum CacheItemCategory
	{
		None = 0x0,
		Metadata = 0x1,
		Content = 0x2,
		All = Metadata | Content,
	}

	[Serializable]
	[DataContract(Namespace = V5.Contracts)]
	public class PluginMessageEntityReference
	{
		[DataMember]
		public string LogicalName;

		[DataMember]
		public Guid Id;

		[DataMember]
		public string Name;

		public EntityReference ToEntityReference()
		{
			return new EntityReference(LogicalName, Id) { Name = Name };
		}
	}

	[Serializable]
	[DataContract(Namespace = V5.Contracts)]
	public class PluginMessageRelationship
	{
		[DataMember]
		public string SchemaName;

		[DataMember]
		public EntityRole? PrimaryEntityRole;

		public Relationship ToRelationship()
		{
			return new Relationship(SchemaName) { PrimaryEntityRole = PrimaryEntityRole };
		}
	}

	[Serializable]
	[DataContract(Namespace = V5.Contracts)]
	[KnownType(typeof(OrganizationServiceCachePluginMessage))]
	public class PluginMessage
	{
		[DataMember]
		public string MessageName;

		[DataMember]
		public PluginMessageEntityReference Target;

		[DataMember]
		public PluginMessageRelationship Relationship;

		[DataMember]
		public List<PluginMessageEntityReference> RelatedEntities;
	}
}
