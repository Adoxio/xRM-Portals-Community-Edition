/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Specialized;

namespace Microsoft.Xrm.Client.Configuration
{
	/// <summary>
	/// Represents a dependency class that can be initialized with configuration values after instantiation.
	/// </summary>
	/// <remarks>
	/// A custom dependency class should implement this interface if it needs to perform custom initialization based on the configuration settings.
	/// </remarks>
	public interface IInitializable
	{
		/// <summary>
		/// Performs custom initialization.
		/// </summary>
		/// <param name="name">The name of the configuration element.</param>
		/// <param name="config">The attributes of the configuration element.</param>
		void Initialize(string name, NameValueCollection config);
	}
}
