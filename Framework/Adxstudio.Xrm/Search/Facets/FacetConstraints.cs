/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FacetConstraints.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   The facet constraint.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Facets
{
	using System.Collections.Generic;

	/// <summary>
	///     The facet constraints.
	/// </summary>
	public class FacetConstraints
	{
		/// <summary>
		///     Gets the facet name.
		/// </summary>
		public string FacetName { get; set; }

		/// <summary>
		///     Gets the constraints.
		/// </summary>
		public IEnumerable<string> Constraints { get; set; }
	}
}
