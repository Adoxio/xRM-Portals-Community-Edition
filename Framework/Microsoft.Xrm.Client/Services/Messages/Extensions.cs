/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Client.Services.Messages
{
	public static class Extensions
	{
		public static EntityReferenceCollection ToEntityReferenceCollection(this List<PluginMessageEntityReference> entities)
		{
			return new EntityReferenceCollection(entities.Select(reference => reference.ToEntityReference()).ToList());
		}

		public static PluginMessageEntityReference ToPluginMessageEntityReference(this EntityReference reference)
		{
			return new PluginMessageEntityReference { LogicalName = reference.LogicalName, Id = reference.Id, Name = reference.Name };
		}

		public static PluginMessageRelationship ToPluginMessageRelationship(this Relationship relationship)
		{
			return new PluginMessageRelationship { SchemaName = relationship.SchemaName, PrimaryEntityRole = relationship.PrimaryEntityRole };
		}

		public static List<PluginMessageEntityReference> ToPluginMessageEntityReferenceCollection(this EntityReferenceCollection entities)
		{
			return new List<PluginMessageEntityReference>(entities.Select(reference => reference.ToPluginMessageEntityReference()));
		}
	}
}
