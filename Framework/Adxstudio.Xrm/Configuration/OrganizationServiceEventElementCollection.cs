/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using Microsoft.Xrm.Client.Configuration;

namespace Adxstudio.Xrm.Configuration
{
	/// <summary>
	/// A collection of <see cref="OrganizationServiceEventElement"/> objects.
	/// </summary>
	[ConfigurationCollection(typeof(OrganizationServiceEventElement))]
	public class OrganizationServiceEventElementCollection : ConfigurationElementCollection<OrganizationServiceEventElement>
	{
		/// <summary>
		/// The name of the element collection.
		/// </summary>
		public const string Name = "events";

		/// <summary>
		/// The name of the element collection.
		/// </summary>
		public override string CollectionName
		{
			get { return Name; }
		}
	}
}
