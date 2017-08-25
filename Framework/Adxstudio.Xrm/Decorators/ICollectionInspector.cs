/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Decorators
{
	/// <summary>
	/// Interface for encapsulating the Inpsection methods of a collection
	/// </summary>
	public interface ICollectionInspector
	{
		/// <summary>
		/// Get the value for the given key from the collection
		/// </summary>
		/// <param name="key">type: string</param>
		/// <returns>type: object</returns>
		object this[string key] { get; }
	}
}
