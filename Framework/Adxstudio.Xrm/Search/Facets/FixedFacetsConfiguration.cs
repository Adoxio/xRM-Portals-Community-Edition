/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FixedFacetsConfiguration.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   Hardcoded facet configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Facets
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using BoboBrowse.Net;
	using BoboBrowse.Net.Facets.Data;
	using BoboBrowse.Net.Facets.Impl;

	/// <summary>
	///     Hardcoded facet configuration
	/// </summary>
	internal class FixedFacetsConfiguration : IFacetedSearchConfigurationProvider
	{
		/// <summary>
		/// Lucene index field name for "ModifiedDate"-facet
		/// </summary>
		public const string ModifiedDateFacetFieldName = "modifiedon.date";

		/// <summary>
		/// Lucene index field name for "RecordType"-facet
		/// </summary>
		public const string RecordTypeFacetFieldName = "_logicalname";

		/// <summary>
		/// CRM entity name for knowledge article
		/// </summary>
		public const string KnowledgeArticleConstraintName = "knowledgearticle";
		
		/// <summary>
		/// The rating field facet name
		/// </summary>
		public const string RatingFieldFacetName = "rating";

		/// <summary>
		/// The content access level
		/// </summary>
		public const string ContentAccessLevel = "adx_contentaccesslevelid";

		/// <summary>
		/// The product field facet name
		/// </summary>
		public const string ProductFieldFacetName = "associated.product";

		/// <summary>
		/// The rating ranges
		/// </summary>
		public static readonly string[] RatingRanges = { "[0.2 TO *]", "[0.4 TO *]", "[0.6 TO *]", "[0.8 TO *]" };

		/// <summary>
		///     The _facet configs.
		/// </summary>
		private readonly List<FacetConfiguration> facetConfigs = new List<FacetConfiguration>
			{
				new FacetConfiguration(RecordTypeFacetFieldName, 
										FacetSortOrder.OrderHitsDesc, 
										FacetHandlerType.Static,
										new FacetSpec()
										{
											MinHitCount = 0,
											ExpandSelection = true
										}, 
										null),
				new FacetConfiguration(ModifiedDateFacetFieldName, 
										FacetSortOrder.OrderValueDesc, 
										FacetHandlerType.Dynamic,
										new FacetSpec()
										{
											MinHitCount = 0,
											ExpandSelection = true,
											CustomComparatorFactory = new FacetValueDescComparatorFactory()
										},
										new RangeFacetHandler(ModifiedDateFacetFieldName,
															new PredefinedTermListFactory<DateTime>("yyyyMMdd"),
															ModifiedOnDateRange.Ranges.Select(i => i.Name).ToList())),
				new FacetConfiguration(RatingFieldFacetName,
										FacetSortOrder.OrderValueAsc,
										FacetHandlerType.Static,
										new FacetSpec()
											{
												MinHitCount = 0,
												ExpandSelection = true
											},
										new RangeFacetHandler(RatingFieldFacetName, RatingRanges)),
				new FacetConfiguration(ProductFieldFacetName,
										FacetSortOrder.OrderHitsDesc,
										FacetHandlerType.Static,
										new FacetSpec()
										{
											MinHitCount = 0,
											ExpandSelection = true
										},
										new MultiValueFacetHandler(ProductFieldFacetName, TermListFactory.StringListFactory))
			};

		/// <summary>
		///     The get configured facets.
		/// </summary>
		/// <returns>
		///     The <see cref="IEnumerable" />.
		/// </returns>
		public IEnumerable<FacetConfiguration> GetConfiguredFacets()
		{
			return this.facetConfigs;
		}

		/// <summary>
		/// The is facet configured.
		/// </summary>
		/// <param name="facetName">
		/// The facet name.
		/// </param>
		/// <returns>
		/// The <see cref="bool"/>.
		/// </returns>
		public bool IsFacetConfigured(string facetName)
		{
			return this.facetConfigs.Exists(facetConfig => { return facetConfig.FieldName == facetName; });
		}
	}
}
