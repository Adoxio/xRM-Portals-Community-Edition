/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;

namespace Microsoft.Xrm.Client.Configuration
{
	/// <summary>
	/// A collection of <see cref="ObjectCacheElement"/> objects.
	/// </summary>
	[ConfigurationCollection(typeof(ObjectCacheElement))]
	public sealed class ObjectCacheElementCollection : ConfigurationElementCollection<ObjectCacheElement>
	{
		/// <summary>
		/// The name of the element collection.
		/// </summary>
		public const string Name = "objectCache";

		/// <summary>
		/// The name of the element collection.
		/// </summary>
		public override string CollectionName
		{
			get { return Name; }
		}
	}
}
