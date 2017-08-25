/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstraintHit.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   The facet hit.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Facets
{
	/// <summary>
	///     The facet hit.
	/// </summary>
	public class ConstraintHit
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConstraintHit"/> class.
		/// </summary>
		/// <param name="constraintName">
		/// The facet constraint.
		/// </param>
		/// <param name="hitCount">
		/// The value.
		/// </param>
		/// <param name="constraintDisplayName">constraint display name for UI</param>
		public ConstraintHit(string constraintName, int hitCount, string constraintDisplayName)
		{
			this.ConstraintValue = constraintName;
			this.HitCount = hitCount;
			this.DisplayValue = constraintDisplayName ?? constraintName;
		}

		/// <summary>
		///     Gets the facet constraint.
		/// </summary>
		public string ConstraintValue { get; private set; }

		/// <summary>
		///     Gets the value.
		/// </summary>
		public int HitCount { get; private set; }

		/// <summary>
		///     Gets the facet constraint display name.
		/// </summary>
		public string DisplayValue { get; private set; }
	}
}
