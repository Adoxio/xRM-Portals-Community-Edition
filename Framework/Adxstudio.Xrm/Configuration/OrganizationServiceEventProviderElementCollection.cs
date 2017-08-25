/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using Microsoft.Xrm.Client.Configuration;

namespace Adxstudio.Xrm.Configuration
{
	/// <summary>
	/// A collection of <see cref="OrganizationServiceEventProviderElement"/> objects.
	/// </summary>
	[ConfigurationCollection(typeof(OrganizationServiceEventProviderElement))]
	public class OrganizationServiceEventProviderElementCollection : ConfigurationElementCollection<OrganizationServiceEventProviderElement>
	{
		/// <summary>
		/// The name of the element collection.
		/// </summary>
		public const string Name = "providers";

		/// <summary>
		/// The name of the element collection.
		/// </summary>
		public override string CollectionName
		{
			get { return Name; }
		}
	}
}
