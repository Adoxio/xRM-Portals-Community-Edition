/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	[DataContract]
	public class DirectoryTreeNode
	{
		public DirectoryTreeNode(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			Entity = entity;
			EntityReference = entity.ToEntityReference();
		}

		internal Entity Entity { get; private set; }

		internal EntityReference EntityReference { get; private set; }

		[DataMember]
		public string name { get; set; }

		[DataMember]
		public string hash { get; set; }

		[DataMember]
		public bool read { get; set; }

		[DataMember]
		public bool write { get; set; }

		[DataMember]
		public DirectoryTreeNode[] dirs { get; set; }
	}
}
