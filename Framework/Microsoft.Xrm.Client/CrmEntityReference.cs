/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Client
{
	/// <summary>
	/// An <see cref="EntityReference"/> that is compatible with WCF Data Services.
	/// </summary>
	public sealed class CrmEntityReference : IExtensibleDataObject
	{
		public CrmEntityReference()
		{
		}

		public CrmEntityReference(string logicalName, Guid id)
		{
			LogicalName = logicalName;
			Id = id;
		}

		/// <summary>
		/// The entity Id.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The entity logical name.
		/// </summary>
		public string LogicalName { get; set; }

		/// <summary>
		/// The entity name value.
		/// </summary>
		public string Name { get; set; }

		ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }

		public override bool Equals(object obj)
		{
			EntityReference reference = this;
			return reference.Equals(obj);
		}

		public override int GetHashCode()
		{
			EntityReference reference = this;
			return reference.GetHashCode();
		}

		public static implicit operator EntityReference(CrmEntityReference reference)
		{
			return reference == null
				? null
				: new EntityReference(reference.LogicalName, reference.Id) { Name = reference.Name };
		}

		public static implicit operator CrmEntityReference(EntityReference reference)
		{
			return reference == null
				? null
				: new CrmEntityReference(reference.LogicalName, reference.Id) { Name = reference.Name };
		}
	}
}
