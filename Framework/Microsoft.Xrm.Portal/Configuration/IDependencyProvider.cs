/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// An inteface for retrieving dependency objects.
	/// </summary>
	public interface IDependencyProvider
	{
		/// <summary>
		/// Retrieves a dependency by type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T GetDependency<T>() where T : class;
		
		/// <summary>
		/// Retrieves a dependency by type and name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		T GetDependency<T>(string name) where T : class;
	}
}
