/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFacetedSearchConfigurationProvider.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   The FacetedSearchConfigurationProvider interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Facets
{
	using System.Collections.Generic;

	/// <summary>
	///     The FacetedSearchConfigurationProvider interface.
	/// </summary>
	internal interface IFacetedSearchConfigurationProvider
	{
		/// <summary>
		///     The get configured facets.
		/// </summary>
		/// <returns>
		///     The <see cref="IEnumerable" />.
		/// </returns>
		IEnumerable<FacetConfiguration> GetConfiguredFacets();

		/// <summary>
		/// The is facet configured.
		/// </summary>
		/// <param name="facetName">
		/// The facet name.
		/// </param>
		/// <returns>
		/// The <see cref="bool"/>.
		/// </returns>
		bool IsFacetConfigured(string facetName);
	}
}
