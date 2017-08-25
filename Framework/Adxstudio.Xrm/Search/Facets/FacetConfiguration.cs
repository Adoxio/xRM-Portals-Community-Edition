/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FacetConfiguration.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   The facet configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Facets
{
	using System;
	using BoboBrowse.Net;
	using BoboBrowse.Net.Facets;
	using BoboBrowse.Net.Facets.Impl;
	using Resources;

	/// <summary>
	///     The facet configuration.
	/// </summary>
	internal class FacetConfiguration
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FacetConfiguration"/> class.
		/// </summary>
		/// <param name="fieldName">
		/// The name.
		/// </param>
		/// <param name="sortOrder">
		/// The sort order.
		/// </param>
		public FacetConfiguration(string fieldName, FacetSortOrder sortOrder)
			: this(fieldName, sortOrder, FacetHandlerType.Static, null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FacetConfiguration"/> class.
		/// </summary>
		/// <param name="fieldName">
		/// The name.
		/// </param>
		/// <param name="sortOrder">
		/// The sort order.
		/// </param>
		/// <param name="handlerType">
		/// Handler type
		/// </param>
		/// <param name="facetSpec">
		/// Facet spec
		/// </param>
		/// <param name="facetHandler">
		/// facet handler
		/// </param>
		public FacetConfiguration(string fieldName, FacetSortOrder sortOrder, FacetHandlerType handlerType, FacetSpec facetSpec, IFacetHandler facetHandler)
		{
			this.FieldName = fieldName;
			this.SortOrder = sortOrder;
			this.FacetHandlerType = handlerType;
			this.Spec = facetSpec ?? new FacetSpec() { MinHitCount = 1 };
			this.Spec.OrderBy = GetFacetSpecSortOrder(sortOrder);
			this.FacetHandler = facetHandler ?? new SimpleFacetHandler(fieldName);
		}

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		public string FieldName { get; set; }

		/// <summary>
		///     Gets or sets the sort order.
		/// </summary>
		public FacetSortOrder SortOrder { get; set; }

		/// <summary>
		///     Gets or sets the facet spec.
		/// </summary>
		public FacetSpec Spec { get; set; }

		/// <summary>
		/// Get or set the facet handler
		/// </summary>
		public IFacetHandler FacetHandler { get; set; }

		/// <summary>
		/// Get or set the facet handler type
		/// </summary>
		public FacetHandlerType FacetHandlerType { get; set; }


		/// <summary>
		/// The get facet spec sort order.
		/// </summary>
		/// <param name="sortOrder">
		/// The sort order.
		/// </param>
		/// <returns>
		/// The <see cref="FacetSpec.FacetSortSpec"/>.
		/// </returns>
		/// <exception cref="Exception">
		/// Throws if no mapping is defined from FacetSortOrder to FacetSpec.FacetSortSpec
		/// </exception>
		private static FacetSpec.FacetSortSpec GetFacetSpecSortOrder(FacetSortOrder sortOrder)
		{
			switch (sortOrder)
			{
				case FacetSortOrder.OrderHitsDesc:
					return FacetSpec.FacetSortSpec.OrderHitsDesc;
				case FacetSortOrder.OrderValueAsc:
					return FacetSpec.FacetSortSpec.OrderValueAsc;
				case FacetSortOrder.OrderValueDesc:
					return FacetSpec.FacetSortSpec.OrderByCustom;
				default:
					throw new Exception("No FacetSortOrder mapping defined for this FacetSpec.FacetSortSpec type.");

			}
		}
	}

	/// <summary>
	///     The facet sort order.
	/// </summary>
	internal enum FacetSortOrder
	{
		/// <summary>
		///     The order value asc.
		/// </summary>
		OrderValueAsc = 0,

		/// <summary>
		///     The order hits desc.
		/// </summary>
		OrderHitsDesc = 1,

		/// <summary>
		/// The order value desc
		/// </summary>
		OrderValueDesc = 2
	}

	/// <summary>
	///     The facet handler type.
	/// </summary>
	internal enum FacetHandlerType
	{
		/// <summary>
		/// Static type
		/// </summary>
		Static = 0,

		/// <summary>
		/// Dynamic type
		/// </summary>
		Dynamic = 1
	}
}
