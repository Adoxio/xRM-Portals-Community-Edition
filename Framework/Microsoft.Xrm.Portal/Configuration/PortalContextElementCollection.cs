/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using Microsoft.Xrm.Client.Configuration;

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// A collection of <see cref="PortalContextElement"/> objects.
	/// </summary>
	[ConfigurationCollection(typeof(PortalContextElement))]
	public sealed class PortalContextElementCollection : ConfigurationElementCollection<PortalContextElement>
	{
		/// <summary>
		/// The name of the element collection.
		/// </summary>
		public const string Name = "portals";

		/// <summary>
		/// The name of the element collection.
		/// </summary>
		public override string CollectionName
		{
			get { return Name; }
		}
	}
}
