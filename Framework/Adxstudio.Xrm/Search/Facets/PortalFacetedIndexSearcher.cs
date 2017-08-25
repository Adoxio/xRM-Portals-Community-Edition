/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PortalFacetedIndexSearcher.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   Overrides search behavior from CrmEntityIndexSearcher to do a faceted search instead of vanilla lucene search.
//   Faceted search is done using the faceting library BoboBrowse.Net:
//   https://github.com/NightOwl888/BoboBrowse.Net/wiki
//   The facets need to be configured to be requested/returned. IFacetedSearchConfigurationProvider defines the
//   interface
//   for all sources of configuration. As of 7/19/2016, the configuration comes from the FixedFacetsConfiguration class,
//   which just hardcodes all the facets we'll allow.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Facets
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Diagnostics;
	using System.Web;
	using System.Web.Security;
	using System.Text.RegularExpressions;
	using Adxstudio.Xrm.ContentAccess;
	using Adxstudio.Xrm.Core.Flighting;
	using BoboBrowse.Net;
	using BoboBrowse.Net.Facets;
	using BoboBrowse.Net.Facets.Impl;
	using BoboBrowse.Net.Support;
	using BoboBrowse.Net.Facets.Data;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Search.Sorting;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Web;
	using BoboBrowse.Net.Util;
	using Lucene.Net.Search;
	using Lucene.Net.Documents;
	using Lucene.Net.Index;
	using Microsoft.Xrm.Portal;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Query;
	using Xrm.Configuration;
	using Xrm.Services.Query;

	/// <summary>
	///     Overrides search behavior from <see cref="CrmEntityIndexSearcher"/> to do a faceted search instead of vanilla lucene search.
	///     Faceted search is done using the faceting library BoboBrowse.Net:
	///     https://github.com/NightOwl888/BoboBrowse.Net/wiki
	///     
	///     An example facet looks like this:
	///     _____________________________
	///     | Record Type               |
	///     |   Knowledge Article   15  |
	///     |   Blog                12  |
	///     |   Forum Posts          8  |
	///     |___________________________|
	///     
	///     Some terminology: that whole box of data is called the "facet view"; "Record Type" is the "facet (view) name"; "Knowledge Article", 
	///     "Blog", etc. are the "facet constraints", and the numbers are the corresponding "hit counts". Similar names are used in the code.
	///     
	///     The facets need to be configured to be requested/returned. <see cref="IFacetedSearchConfigurationProvider"/> defines the
	///     interface for all sources of configuration. As of 7/19/2016, the configuration comes from the <see cref="FixedFacetsConfiguration"/>
	///      class, which just hardcodes all the facets we'll allow.
	/// </summary>
	internal class PortalFacetedIndexSearcher : PortalIndexSearcher
	{
		/// <summary>
		/// The default page size
		/// </summary>
		private const int DefaultPageSize = 10;

		/// <summary>
		/// The default maximum page size
		/// </summary>
		private const int DefaultMaxPageSize = 50;

		/// <summary>
		///     The _bobo reader.
		/// </summary>
		private readonly BoboIndexReader boboReader;

		/// <summary>
		///     The _specs.
		/// </summary>
		private readonly IDictionary<string, FacetSpec> specs;

		/// <summary>
		///     The _config.
		/// </summary>
		private readonly IFacetedSearchConfigurationProvider config;

		/// <summary>
		/// Initializes a new instance of the <see cref="PortalFacetedIndexSearcher"/> class. 
		/// The portal faceted index searcher.
		/// </summary>
		/// <param name="index">
		/// The index.
		/// </param>
		/// <param name="websiteID">
		/// The website id.
		/// </param>
		public PortalFacetedIndexSearcher(ICrmEntityIndex index, Guid websiteID)
			: base(index, websiteID)
		{
			this.config = new FixedFacetsConfiguration();

			// The facet engine requires you to define handlers (these just tell BoboBrowse how to handle 
			// the field's data) and specs (these tell BoboBrowse how to return the facets during a search)
			// for each faceted field. Since our facets are preconfigured, we'll just maintain these in
			// the faceted searcher instance
			this.specs = new Dictionary<string, FacetSpec>();

			var handlers = new List<IFacetHandler>();

			foreach (var facetConfiguration in this.config.GetConfiguredFacets())
			{
				if (facetConfiguration.FacetHandlerType == FacetHandlerType.Static)
				{
					handlers.Add(facetConfiguration.FacetHandler);
				}
				this.specs.Put(facetConfiguration.FieldName, facetConfiguration.Spec);
			}
			//// The BoboIndexReader is just a wrapper around the lucene IndexReader
			this.boboReader = BoboIndexReader.GetInstance(this.Searcher.IndexReader, handlers);
		}

		/// <summary>
		///     The dispose.
		/// </summary>
		public override void Dispose()
		{
			this.boboReader.Dispose();
			base.Dispose();
		}

		/// <summary>
		/// Overrides Search behavior to do faceted search with BoboBrowse.Net
		/// </summary>
		/// <param name="query">
		/// The search query.
		/// </param>
		/// <returns>
		/// The <see cref="Query"/>.
		/// </returns>
		protected override Query CreateQuery(ICrmEntityQuery query)
		{
			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching))
			{
				var baseQuery = base.CreateQuery(query);
				var compositeQuery = new BooleanQuery()
									 {
										 { baseQuery, Occur.MUST }
									 };
				var contentAccessLevelProvider = new ContentAccessLevelProvider();

				compositeQuery.Add(new TermQuery(new Term("_logicalname", "annotation")), Occur.MUST_NOT);

				if (contentAccessLevelProvider.IsEnabled())
				{
					var calQuery = new BooleanQuery();
					var userCals = contentAccessLevelProvider.GetContentAccessLevels();

					ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "Adding User CALs to Lucene query");

					foreach (var cal in userCals)
					{
						ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, string.Format("User CAL {0}", cal.Id));
						calQuery.Add(new TermQuery(new Term(FixedFacetsConfiguration.ContentAccessLevel, cal.Id.ToString())), Occur.SHOULD);
					}
					calQuery.Add(new TermQuery(new Term(FixedFacetsConfiguration.ContentAccessLevel, "public")), Occur.SHOULD);

					compositeQuery.Add(calQuery, Occur.MUST);
				}

				var productAccessProvider = new ProductAccessProvider();

				if (productAccessProvider.IsEnabled())
				{
					var productFilteringQuery = new BooleanQuery
											{
												{
													new TermQuery(
														new Term(FixedFacetsConfiguration.ProductFieldFacetName, this.Index.ProductAccessNonKnowledgeArticleDefaultValue)),
															Occur.SHOULD
												}
											};
					var userProducts = productAccessProvider.GetProducts();
					ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "Adding User products to Lucene query");

					foreach (var product in userProducts)
					{
						ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, string.Format("User product {0}", product));
						productFilteringQuery.Add(
							new TermQuery(new Term(FixedFacetsConfiguration.ProductFieldFacetName, product.ToString())),
							Occur.SHOULD);
					}

					if (PortalContext.Current.User != null)
					{
						if (productAccessProvider.DisplayArticlesWithoutAssociatedProductsEnabled())
						{
							productFilteringQuery.Add(
								new TermQuery(new Term(FixedFacetsConfiguration.ProductFieldFacetName, this.Index.ProductAccessDefaultValue)),
								Occur.SHOULD);
						}
					}
					else
					{
						productFilteringQuery.Add(
							new TermQuery(new Term(FixedFacetsConfiguration.ProductFieldFacetName, "unauthenticatedUser")),
							Occur.SHOULD);
					}

					compositeQuery.Add(productFilteringQuery, Occur.MUST);
				}

				ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, string.Format("Adding User WebRoleDefaultValue to Lucene query: {0}", this.Index.WebRoleDefaultValue));

				var cmsQuery = new BooleanQuery
							{
								{
									new TermQuery(
										new Term(this.Index.WebRoleFieldName, this.Index.WebRoleDefaultValue)),
											Occur.SHOULD
								}
							};

				// Windows Live ID Server decided to return null for an unauthenticated user's name
				// A null username, however, breaks the Roles.GetRolesForUser() because it expects an empty string.
				var currentUsername = (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
					? HttpContext.Current.User.Identity.Name ?? string.Empty
					: string.Empty;

				var userRoles = Roles.GetRolesForUser(currentUsername);

				ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, "Adding user role to Lucene query");
				foreach (var role in userRoles)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Monitoring, string.Format("User role: {0}", role));
					cmsQuery.Add(new TermQuery(new Term(this.Index.WebRoleFieldName, role)), Occur.SHOULD);
				}

				compositeQuery.Add(cmsQuery, Occur.MUST);

				// Add the Url Defined Part to the Query. 
				var urlDefinedQuery = new BooleanQuery
							{
								{
									new TermQuery(
										new Term(this.Index.IsUrlDefinedFieldName, bool.TrueString)),
											Occur.SHOULD
								}
							};
				compositeQuery.Add(urlDefinedQuery, Occur.MUST);

				// Add knowledgearticle to the query
				compositeQuery.Add(
					new TermQuery(new Term(FixedFacetsConfiguration.RecordTypeFacetFieldName, FixedFacetsConfiguration.KnowledgeArticleConstraintName)),
					Occur.SHOULD);

				return compositeQuery;
			}
			else
			{
				return base.CreateQuery(query);
			}
		}

		/// <summary>
		/// Override to get faceted results 
		/// </summary>
		/// <param name="query">The query</param>
		/// <param name="searchLimit">The max number of results we want</param>
		/// <param name="offset">The number of already processed results to skip.</param>
		/// <returns>The results of the search unfiltered for the user.</returns>
		protected override RawSearchResultSet GetRawSearchResults(ICrmEntityQuery query, int searchLimit, int offset)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var br = new BrowseRequest();
			br.Count = searchLimit;
			br.Sort = this.GetSortField(query);
			br.Query = this.CreateQuery(query);
			br.Offset = offset;

			this.AddFacetConstraints(br, query.FacetConstraints);

			// add preconfigured facet specs
			foreach (var fieldToSpec in this.specs)
			{
				br.SetFacetSpec(fieldToSpec.Key, fieldToSpec.Value);
			}

			// execute the query
			IBrowsable browser = new BoboBrowser(this.boboReader);
			foreach (var facetConfiguration in this.config.GetConfiguredFacets())
			{
				if (facetConfiguration.FacetHandlerType == FacetHandlerType.Dynamic)
				{
					browser.SetFacetHandler(facetConfiguration.FacetHandler);
				}
			}
			var browseResult = browser.Browse(br);

			stopwatch.Stop();

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Lucene(faceted/BoboBrowse): {0} total hits ({1}ms)", browseResult.NumHits, stopwatch.ElapsedMilliseconds));

            PortalFeatureTrace.TraceInstance.LogSearch(FeatureTraceCategory.Search, browseResult.NumHits, stopwatch.ElapsedMilliseconds, string.Format("Lucene(faceted/BoboBrowse): {0} total hits ({1}ms)", browseResult.NumHits, stopwatch.ElapsedMilliseconds));

			return this.ConvertBoboBrowseResultsToRawSearchResultSet(browseResult, offset, query.FacetConstraints);
		}

		/// <summary>
		/// Gets the unprocessed results for a search query.
		/// </summary>
		/// <param name="query">The query</param>
		/// <param name="searchLimit">The max number of results we want</param>
		/// <param name="offset">The number of already processed results to skip.</param>
		/// <returns>The results of the search unfiltered for the user.</returns>
		protected override RawSearchResultSet GetRawSearchResults(Query query, int searchLimit, int offset)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var br = new BrowseRequest();
			br.Count = searchLimit;
			br.Query = query;
			br.Offset = offset;

			// execute the query
			IBrowsable browser = new BoboBrowser(this.boboReader);

			var browseResult = browser.Browse(br);

			stopwatch.Stop();

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Lucene(faceted/BoboBrowse): {0} total hits ({1}ms)", browseResult.NumHits, stopwatch.ElapsedMilliseconds));

            PortalFeatureTrace.TraceInstance.LogSearch(FeatureTraceCategory.Search, browseResult.NumHits, stopwatch.ElapsedMilliseconds, string.Format("Lucene(faceted/BoboBrowse): {0} total hits ({1}ms)", browseResult.NumHits, stopwatch.ElapsedMilliseconds));

            return this.ConvertBoboBrowseResultsToRawSearchResultSet(browseResult, offset, Enumerable.Empty<FacetConstraints>());
		}

		/// <summary>
		/// Override to return facets and sorting options.
		/// </summary>
		/// <param name="results">Search results to display to user.</param>
		/// <param name="approximateTotalHits">Estimate of the number of results.</param>
		/// <param name="pageNumber">The page number.</param>
		/// <param name="pageSize">The Page size.</param>
		/// <param name="rawSearchResultSet">The raw search result set, which contains the raw results and other search related info.</param>
		/// <returns>The result page to return to the user.</returns>
		protected override ICrmEntitySearchResultPage GenerateResultPage(ICollection<ICrmEntitySearchResult> results, int approximateTotalHits, int pageNumber, int pageSize, RawSearchResultSet rawSearchResultSet)
		{
			var resultOffset = (pageNumber - 1) * pageSize;
 
			var pageResults = results.Skip(resultOffset).Take(pageSize).ToList();

			return new CrmEntitySearchResultPage(pageResults, approximateTotalHits, pageNumber, pageSize, rawSearchResultSet.FacetViews, rawSearchResultSet.SortingOptions);
		}

		/// <summary>
		/// Converts the BoboBrowse BrowseResult object into the generic RawSearchResultSet for processing.
		/// </summary>
		/// <param name="browseResults">The BoboBrowse BrowseResult for the search</param>
		/// <param name="offset">The numbers of results to skip since they've already been processed</param>
		/// <param name="facetConstraints">The constraint hit map given by BoboBrowse.</param>
		/// <returns>The converted raw search result set.</returns>
		private RawSearchResultSet ConvertBoboBrowseResultsToRawSearchResultSet(BrowseResult browseResults, int offset, IEnumerable<FacetConstraints> facetConstraints)
		{
			List<RawSearchResult> rawSearchResults = new List<RawSearchResult>();
			for (int i = offset; i < browseResults.Hits.Length; i++)
			{
				rawSearchResults.Add(new RawSearchResult(browseResults.Hits[i].DocId, browseResults.Hits[i].Score));
			}

			// transform the BoboBrowse-specific facet map to our more general data structure
			var facetViews = this.FacetMapToFacetViews(browseResults, facetConstraints ?? Enumerable.Empty<FacetConstraints>());

			var sortingOptions = this.GetSortingOptions(facetViews, facetConstraints);

			return new RawSearchResultSet(rawSearchResults, browseResults.NumHits, facetViews, sortingOptions);
		}

		/// <summary>
		/// Adds facet constraints to a browse request.
		/// </summary>
		/// <param name="br">The browse request.</param>
		/// <param name="facetConstraintsSet">The facet constraints for this request.</param>
		private void AddFacetConstraints(BrowseRequest br, IEnumerable<FacetConstraints> facetConstraintsSet)
		{
			// add facet constraints specified in query to the browse request
			foreach (var facetConstraints in facetConstraintsSet)
			{
				if (this.config.IsFacetConfigured(facetConstraints.FacetName))
				{
					var bs = br.GetSelection(facetConstraints.FacetName) ?? new BrowseSelection(facetConstraints.FacetName);
					foreach (var constraint in facetConstraints.Constraints)
					{
						bs.AddValue(constraint);
					}

					br.AddSelection(bs);
				}
			}
		}

		/// <summary>
		/// Converts the BoboBrowse FacetMap object into a list of FacetViews
		/// </summary>
		/// <param name="browseResults">BoboBrowse results</param>
		/// <param name="clientFacetConstraints">facet constraints selected on client side</param>
		/// <returns>A list of facet views</returns>
		private IEnumerable<FacetView> FacetMapToFacetViews(BrowseResult browseResults, IEnumerable<FacetConstraints> clientFacetConstraints)
		{
			var facetViews = new List<FacetView>();

			foreach (var facet in browseResults.FacetMap)
			{
				var facetConstraints = this.FacetToConstraintHits(facet, clientFacetConstraints, browseResults);

				if (facetConstraints.Any())
				{
					var allConstraintHits = new List<ConstraintHit>();
					if (facet.Key == FixedFacetsConfiguration.RecordTypeFacetFieldName)
					{
						allConstraintHits.Add(
							new ConstraintHit(
								string.Empty,
								facet.Value.GetFacets().Where(h => h.Value != "annotation").Sum(hit => hit.FacetValueHitCount),
								ResourceManager.GetString("Facet_All")));
					}

					
					allConstraintHits.AddRange(facetConstraints);
					facetViews.Add(new FacetView(facet.Key, allConstraintHits));
				}
			}
			return facetViews;
		}

		/// <summary>
		/// Converts the BoboBrowse Facet object into a list of ConstraintHits
		/// </summary>
		/// <param name="facet">BoboBrowse Facet object</param>
		/// <param name="clientFacetConstraints">facet constraints selected on client side</param>
		/// <param name="browseResults"> Browse results</param>
		/// <returns>a list of constraint hits</returns>
		private IEnumerable<ConstraintHit> FacetToConstraintHits(KeyValuePair<string, IFacetAccessible> facet, IEnumerable<FacetConstraints> clientFacetConstraints, BrowseResult browseResults)
		{
			if (facet.Key == FixedFacetsConfiguration.RecordTypeFacetFieldName)
			{
				return this.GroupRecordTypeFacet(facet, clientFacetConstraints);
			}
			return this.BrowseFacetsToConstraintHits(facet, clientFacetConstraints, browseResults);
		}

		/// <summary>
		/// Converts the BoboBrowse BrowseFacet list into a list of ConstraintHits
		/// </summary>
		/// <param name="currentFacet">BoboBrowse BrowseFacet</param>
		/// <param name="clientFacetConstraints">facet constraints selected on client side</param>
		/// <param name="browseResults"> Browse Results</param>
		/// <returns>a list of constraint hits</returns>
		private IEnumerable<ConstraintHit> BrowseFacetsToConstraintHits(KeyValuePair<string, IFacetAccessible> currentFacet, IEnumerable<FacetConstraints> clientFacetConstraints, BrowseResult browseResults = null)
		{
			var currentFacetConfiguration = this.config.GetConfiguredFacets().FirstOrDefault(configuration => configuration.FieldName == currentFacet.Key);
			if (currentFacetConfiguration == null)
			{
				return Enumerable.Empty<ConstraintHit>();
			}

			var browseFacets = currentFacet.Value.GetFacets();
			////we display the facet either we have non-zero hit count(s) in it, or we have selected ("sticky") facet constraint(s) in it on client side
			var facetsToDisplay = browseFacets.Where(facet => facet.FacetValueHitCount > 0 
				|| clientFacetConstraints.Where(constraints => constraints.FacetName == currentFacet.Key).SelectMany(constraints => constraints.Constraints).Any(constraintName => facet.Value == constraintName));
			if (!facetsToDisplay.Any())
			{
				return Enumerable.Empty<ConstraintHit>();
			}

			var resultConstraintHits = new List<ConstraintHit>();
			IEnumerable<ConstraintHit> currentFacetConstraintHits = Enumerable.Empty<ConstraintHit>();
			switch (currentFacet.Key)
			{
				case FixedFacetsConfiguration.ModifiedDateFacetFieldName:
					currentFacetConstraintHits = browseFacets.Where(facet => facet.Value == "[* TO *]")
							.Concat(browseFacets.Where(facet => facet.Value != "[* TO *]"))
							.Select(browseFacet => new ConstraintHit(browseFacet.Value, browseFacet.FacetValueHitCount, ModifiedOnDateRange.GetRangeDisplayName(browseFacet.Value)));
					break;
				case FixedFacetsConfiguration.RecordTypeFacetFieldName:
					currentFacetConstraintHits = facetsToDisplay.Select(browseFacet => new ConstraintHit(browseFacet.Value, browseFacet.FacetValueHitCount, this.LocalizeRecordTypeName(browseFacet.Value.Split(',')[0])));
					break;
				case FixedFacetsConfiguration.ProductFieldFacetName:
					facetsToDisplay = facetsToDisplay.Where(facet => facet.Value != this.Index.ProductAccessNonKnowledgeArticleDefaultValue && facet.Value != this.Index.ProductAccessDefaultValue);
					if (!facetsToDisplay.Any())
					{
						return Enumerable.Empty<ConstraintHit>();
					}
					currentFacetConstraintHits = facetsToDisplay.Select(browseFacet => new ConstraintHit(browseFacet.Value, browseFacet.FacetValueHitCount, browseFacet.Value));
					break;
				case FixedFacetsConfiguration.RatingFieldFacetName:
					currentFacetConstraintHits = browseFacets.Select(browseFacet => new ConstraintHit(browseFacet.Value, browseFacet.FacetValueHitCount, browseFacet.Value));
					break;
				default:
					currentFacetConstraintHits = facetsToDisplay.Select(browseFacet => new ConstraintHit(browseFacet.Value, browseFacet.FacetValueHitCount, browseFacet.Value));
					break;
			}
			resultConstraintHits.AddRange(currentFacetConstraintHits);
			return resultConstraintHits;
		}

		/// <summary>
		/// Localizes the name of the record type.
		/// </summary>
		/// <param name="recordTypeName">Name of the record type.</param>
		/// <returns>localized record type</returns>
		protected virtual string LocalizeRecordTypeName(string recordTypeName)
		{
			return Web.Extensions.LocalizeRecordTypeName(recordTypeName);
		}

		/// <summary>
		/// Groups related BoboBrowse BrowseFacet list by record type and converts into a list of ConstraintHits
		/// </summary>
		/// <param name="currentFacet">BoboBrowse BrowseFacet</param>
		/// <param name="clientFacetConstraints">facet constraints selected on client side</param>
		/// <returns>a list of constraint hits</returns>
		private IEnumerable<ConstraintHit> GroupRecordTypeFacet(KeyValuePair<string, IFacetAccessible> currentFacet, IEnumerable<FacetConstraints> clientFacetConstraints)
		{
			var entityToRecordMap = new Dictionary<string, string>();
			var groupedFacets = new Dictionary<string, int>();
			var settingsString = this.RecordTypeFacetsEntities;
			var result = new List<ConstraintHit>();

			settingsString = this.RemoveDuplicates(settingsString);

			if (string.IsNullOrEmpty(settingsString))
			{
				return this.BrowseFacetsToConstraintHits(currentFacet, clientFacetConstraints);
			}

			foreach (var groupName in Web.Mvc.Html.SettingExtensions.SplitSearchFilterOptions(settingsString))
			{
				var trimmedGroupName = groupName.Value.Trim();

				if (string.IsNullOrEmpty(trimmedGroupName))
				{
					continue;
				}

				foreach (var logicalName in trimmedGroupName.Split(','))
				{
					var trimmedLogicalName = logicalName.Trim();

					if (string.IsNullOrEmpty(trimmedLogicalName))
					{
						continue;
					}

					entityToRecordMap.Add(trimmedLogicalName, trimmedGroupName);
				}

				groupedFacets.Add(trimmedGroupName, 0);
			}

			var browseFacets = currentFacet.Value.GetFacets();
			foreach (var facet in browseFacets)
			{
				var logicalName = facet.Value;

				// adds browseFacet if it's not present in site setting grouping
				if (!entityToRecordMap.ContainsKey(logicalName))
				{
					groupedFacets.Add(logicalName, facet.FacetValueHitCount);
					continue;
				}

				groupedFacets[entityToRecordMap[logicalName]] += facet.FacetValueHitCount;
			}

			foreach (var facet in groupedFacets)
			{
				if (facet.Value > 0 || clientFacetConstraints.Where(constraints => constraints.FacetName == currentFacet.Key).SelectMany(constraints => constraints.Constraints).Any(constraintName => facet.Key.Contains(constraintName)))
				{
					result.Add(new ConstraintHit(facet.Key, facet.Value, Web.Extensions.LocalizeRecordTypeName(facet.Key.Split(',')[0])));
				}
			}

			return result.OrderByDescending(x => x.HitCount);
		}

		/// <summary>
		/// Gets the record type facets entities.
		/// </summary>
		/// <value>
		/// The record type facets entities.
		/// </value>
		protected virtual string RecordTypeFacetsEntities
		{
			get
			{
				return HttpContext.Current.GetSiteSetting("Search/RecordTypeFacetsEntities");
			}
		}

		/// <summary>
		/// Removes duplicate words
		/// </summary>
		/// <param name="input">input string</param>
		/// <returns>string with no duplicate words</returns>
		private string RemoveDuplicates(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return input;
			}

			var uniqueWords = new List<string>();
			var duplicateWords = new List<string>();

			foreach (var word in input.Split(';', ','))
			{
				var trimmedWord = word.Trim();

				if (string.IsNullOrEmpty(trimmedWord))
				{
					continue;
				}

				if (uniqueWords.Contains(trimmedWord))
				{
					duplicateWords.Add(trimmedWord);
					continue;
				}

				uniqueWords.Add(trimmedWord);
			}

			foreach (var duplicateWord in duplicateWords)
			{
				var startIndex = input.IndexOf(duplicateWord, StringComparison.InvariantCulture) + duplicateWord.Length;
				var secondDuplicateIndex = input.IndexOf(duplicateWord, startIndex, StringComparison.InvariantCulture);
				input = input.Remove(secondDuplicateIndex, duplicateWord.Length);
			}

			return input;
		}

		/// <summary>
		/// Gets the sort field.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>Sort Field</returns>
		private SortField[] GetSortField(ICrmEntityQuery query)
		{
			var sortingOption = query.SortingOption;

			if (sortingOption == SortingFields.Rating || sortingOption == SortingFields.ViewCount)
			{
				return new SortField[] { new SortField(sortingOption, SortField.DOUBLE, true) };
			}

			return new SortField[] { SortField.FIELD_SCORE };
		}

		/// <summary>
		/// Gets the sorting options.
		/// </summary>
		/// <param name="facetViews">The facet views.</param>
		/// <param name="facetConstraints"> The facet constraints. </param>
		/// <returns> Sorting Options</returns>
		internal IEnumerable<string> GetSortingOptions(IEnumerable<FacetView> facetViews, IEnumerable<FacetConstraints> facetConstraints)
		{
			var facetViewList = facetViews as IList<FacetView> ?? facetViews.ToList();

			var isKnowledgeArticleInFacet =
				facetViewList.Where(facetView => facetView.FacetName == FixedFacetsConfiguration.RecordTypeFacetFieldName)
					.SelectMany(facetView => facetView.ConstraintHits)
					.Any(constraint => constraint.ConstraintValue.Split(',').Contains(FixedFacetsConfiguration.KnowledgeArticleConstraintName));

			var recordTypeConstraint =
				facetConstraints.FirstOrDefault(item => item.FacetName == FixedFacetsConfiguration.RecordTypeFacetFieldName);

			var isAllConstraint = recordTypeConstraint == null;

			var isKnowledgeArticleInConstraints = recordTypeConstraint != null 
				&& recordTypeConstraint.Constraints.Contains(FixedFacetsConfiguration.KnowledgeArticleConstraintName);

			if (facetViews != null && isKnowledgeArticleInFacet && (isAllConstraint || isKnowledgeArticleInConstraints))
			{
				var isRatingOptionVisible = facetViewList.Where(f => f.FacetName == FixedFacetsConfiguration.RatingFieldFacetName).SelectMany(f => f.ConstraintHits).Skip(1).Sum(c => c.HitCount) > 0;
				var result = new List<string> { SortingFields.Relevance, SortingFields.ViewCount };

				if (isRatingOptionVisible)
				{
					result.Add(SortingFields.Rating);
				}

				return result;
			}
			return null;
		}

		/// <summary>
		/// Gets the entity field value by guids.
		/// </summary>
		/// <typeparam name="T">type of value in "entityFieldName"-field</typeparam>
		/// <param name="entityGuids">The entity guids.</param>
		/// <param name="entityName">Name of the entity.</param>
		/// <param name="entityFieldName">Name of the entity field.</param>
		/// <param name="entityFieldNameForGuid">The entity field name for unique identifier.</param>
		/// <returns>Dictionary of "entityName"-entity guid and "entityFieldName"-field value</returns>
		protected virtual Dictionary<Guid, T> GetEntityFieldValueByGuids<T>(IEnumerable<Guid> entityGuids, string entityName, string entityFieldName, string entityFieldNameForGuid)
		{
			var entityFieldsFetch = new Fetch
			{
				Distinct = true,
				Entity = new FetchEntity
				{
					Name = entityName,
					Attributes = new List<FetchAttribute>
					{
						new FetchAttribute(entityFieldNameForGuid),
						new FetchAttribute(entityFieldName)
					},
					Filters = new List<Xrm.Services.Query.Filter>()
					{
						new Xrm.Services.Query.Filter()
						{
							Type = LogicalOperator.And,
							Conditions = new List<Condition>()
							{
								new Condition
								{
									Attribute = entityFieldNameForGuid,
									Operator = ConditionOperator.In,
									Values = entityGuids.Distinct().Cast<object>().ToList()
								}
							}
						}
					}
				}
			};
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			using (var serviceContext = portalContext.ServiceContext)
			{
				var entityCollection = entityFieldsFetch.Execute(serviceContext as IOrganizationService);
				return entityCollection.Entities.ToDictionary(entity => entity.GetAttributeValue<Guid>(entityFieldNameForGuid), entity => entity.GetAttributeValue<T>(entityFieldName));
			}
		}
	}
}
