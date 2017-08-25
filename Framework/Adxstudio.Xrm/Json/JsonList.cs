/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Json
{
	using System.Collections.Generic;

	/// <summary>
	/// A list serialization surrogate class.
	/// </summary>
	/// <typeparam name="T">The item type.</typeparam>
	internal struct JsonList<T>
	{
		/// <summary>
		/// The nested list.
		/// </summary>
		public List<T> Value;
	}
}
