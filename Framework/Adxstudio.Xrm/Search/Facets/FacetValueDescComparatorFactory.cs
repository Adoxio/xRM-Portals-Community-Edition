/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FacetValueDescComparatorFactory.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Facets
{
	using System.Collections.Generic;

	using BoboBrowse.Net;
	using BoboBrowse.Net.Util;

	/// <summary>
	/// Facet Value Descending Comparator
	/// </summary>
	internal class FacetValueDescComparatorFactory : IComparatorFactory
	{
		/// <summary>
		/// "NewComparator"-realization for facet values
		/// </summary>
		/// <param name="fieldValueAccessor">field Value Accessor</param>
		/// <param name="counts">hit counts</param>
		/// <returns>new comparer</returns>
		public virtual IComparer<int> NewComparator(IFieldValueAccessor fieldValueAccessor, BigSegmentedArray counts)
		{
			return new FacetValueDescComparatorFactoryComparator();
		}

		/// <summary>
		/// "NewComparator"-realization for BrowseFacet values
		/// </summary>
		/// <returns>new comparer</returns>
		public virtual IComparer<BrowseFacet> NewComparator()
		{
			return new FacetValueDescComparatorFactoryBrowseFacetComparator();
		}

		/// <summary>
		/// Comparer for facet values
		/// </summary>
		private sealed class FacetValueDescComparatorFactoryComparator : IComparer<int>
		{
			/// <summary>
			/// IComparer(int).Compare - realization
			/// </summary>
			/// <param name="o1">first value</param>
			/// <param name="o2">second value</param>
			/// <returns>compare result</returns>
			public int Compare(int o1, int o2)
			{
				return o1 - o2;
			}
		}

		/// <summary>
		/// Comparer for BrowseFacet values
		/// </summary>
		private sealed class FacetValueDescComparatorFactoryBrowseFacetComparator : IComparer<BrowseFacet>
		{
			/// <summary>
			/// IComparer(BrowseFacet) - realization
			/// </summary>
			/// <param name="o1">first BrowseFacet</param>
			/// <param name="o2">second BrowseFacet</param>
			/// <returns>compare result</returns>
			public int Compare(BrowseFacet o1, BrowseFacet o2)
			{
				return string.CompareOrdinal(o2.Value, o1.Value);
			}
		}
	}
}
