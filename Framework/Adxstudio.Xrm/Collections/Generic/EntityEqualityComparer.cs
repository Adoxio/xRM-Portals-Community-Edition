/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Collections.Generic
{
	/// <summary>
	/// Provides comparison of entity objects for equality.
	/// </summary>
	public class EntityEqualityComparer : IEqualityComparer<Entity>
	{
		/// <summary>
		/// Determines if the specified entity objects are equal by comparing the unique identifiers.
		/// </summary>
		/// <param name="x"><see cref="Entity"/></param>
		/// <param name="y"><see cref="Entity"/></param>
		/// <returns>Returns true if the unique identifiers are identical, otherwise returns false.</returns>
		public bool Equals(Entity x, Entity y)
		{
			if (x == null && y == null)
			{
				return true;
			}

			if (x == null || y == null)
			{
				return false;
			}

			return x.Id == y.Id;
		}

		/// <summary>
		/// Hash code for the specified entity object.
		/// </summary>
		/// <param name="key"><see cref="Entity"/></param>
		/// <returns>Returns the hash code for the specified entity.</returns>
		public int GetHashCode(Entity key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			return key.Id.GetHashCode();
		}
	}
}
