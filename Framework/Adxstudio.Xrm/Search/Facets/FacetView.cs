/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FacetView.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   The facet hit.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Facets
{
	using System.Collections.Generic;

	/// <summary>
	///     The facet hit.
	/// </summary>
	public class FacetView
	{ 
		/// <summary>
		/// Initializes a new instance of the <see cref="FacetView"/> class.
		/// </summary>
		/// <param name="facetName">
		/// The facet view name.
		/// </param>
		/// <param name="constraintHits">
		/// The constraints with hits
		/// </param>
		public FacetView(string facetName, IEnumerable<ConstraintHit> constraintHits)
		{
			this.FacetName = facetName;
			this.ConstraintHits = constraintHits;
		}

		/// <summary>
		///     Gets the facet name.
		/// </summary>
		public string FacetName { get; private set; }

		/// <summary>
		///     Gets the constraints and their hits.
		/// </summary>
		public IEnumerable<ConstraintHit> ConstraintHits { get; private set; }
	}
}
