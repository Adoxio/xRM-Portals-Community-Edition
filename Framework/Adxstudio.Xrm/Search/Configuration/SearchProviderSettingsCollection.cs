/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using Microsoft.Xrm.Client.Configuration;

namespace Adxstudio.Xrm.Search.Configuration
{
	public class SearchProviderSettingsCollection : ConfigurationElementCollection<ProviderSettings>
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
