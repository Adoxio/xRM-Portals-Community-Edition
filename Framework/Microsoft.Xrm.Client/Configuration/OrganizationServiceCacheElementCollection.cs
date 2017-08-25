/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;

namespace Microsoft.Xrm.Client.Configuration
{
	/// <summary>
	/// A collection of <see cref="OrganizationServiceCacheElement"/> objects.
	/// </summary>
	[ConfigurationCollection(typeof(OrganizationServiceCacheElement))]
	public sealed class OrganizationServiceCacheElementCollection : ConfigurationElementCollection<OrganizationServiceCacheElement>
	{
		/// <summary>
		/// The name of the element collection.
		/// </summary>
		public const string Name = "serviceCache";

		/// <summary>
		/// The name of the element collection.
		/// </summary>
		public override string CollectionName
		{
			get { return Name; }
		}
	}
}
