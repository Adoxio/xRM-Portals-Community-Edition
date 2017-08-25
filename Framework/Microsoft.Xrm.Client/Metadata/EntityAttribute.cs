/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Microsoft.Xrm.Client.Metadata
{
	/// <summary>
	/// A description of a custom entity class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
	public sealed class EntityAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the plural name of the static entity.
		/// </summary>
		public string EntitySetName { get; set; }

		public EntityAttribute(string entitySetName)
		{
			EntitySetName = entitySetName;
		}
	}
}
