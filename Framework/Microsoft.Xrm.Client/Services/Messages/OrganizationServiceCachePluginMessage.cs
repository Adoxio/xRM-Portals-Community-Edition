/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Runtime.Serialization;

namespace Microsoft.Xrm.Client.Services.Messages
{
	[Serializable]
	[DataContract(Namespace = V5.Contracts)]
	public class OrganizationServiceCachePluginMessage : PluginMessage
	{
		[DataMember]
		public string ConnectionStringName;

		[DataMember]
		public string ServiceCacheName;

		[DataMember]
		public string Secret;

		[DataMember]
		public CacheItemCategory? Category;
	}
}
