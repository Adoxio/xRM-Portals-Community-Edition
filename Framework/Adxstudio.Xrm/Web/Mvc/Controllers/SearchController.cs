/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Mvc.Controllers
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web.Mvc;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Search;
	using Adxstudio.Xrm.Search.Facets;
	using Adxstudio.Xrm.Metadata;
	using Adxstudio.Xrm.Core.Flighting;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Newtonsoft.Json.Linq;
	using Adxstudio.Xrm.AspNet.Cms;

	/// <summary>
	/// Search Controller
	/// </summary>
	/// <seealso cref="System.Web.Mvc.Controller" />
	public class SearchController : Controller
	{
		private const int DefaultPageSize = 10;
		private const int DefaultMaxPageSize = 50;
		private const int DefaultInitialConstraintsToLocalize = 4;

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult Search(Dictionary<string, string> parameters, string query, string logicalNames, string filter, int pageNumber, int pageSize = DefaultPageSize,
			FacetConstraints[] facetConstraints = null, string sortingOption = "Relevance", int initialConstraintsToLocalize = DefaultInitialConstraintsToLocalize)
		{
			if (pageSize < 0)
			{
				pageSize = DefaultPageSize;
			}

			if (pageSize > DefaultMaxPageSize)
			{
				pageSize = DefaultMaxPageSize;
			}

			var queryText = GetQueryText(query, parameters);
			var filterText = GetFilterText(filter, parameters);
			ContextLanguageInfo contextLanguage;
			var multiLanguageEnabled = TryGetLanguageCode(out contextLanguage);

			if (string.IsNullOrWhiteSpace(queryText) && string.IsNullOrWhiteSpace(filterText))
			{
				return new JObjectResult(new JObject
				{
					{ "itemCount", 0 },
					{ "pageNumber", pageNumber },
					{ "pageSize", pageSize },
					{ "pageCount", 0 },
					{ "items", new JArray() }
				});
			}

			var provider = SearchManager.Provider;

			if (facetConstraints != null)
			{
				facetConstraints =
					facetConstraints.Select(
						x =>
						new FacetConstraints
						{
							FacetName = x.FacetName,
							Constraints = x.Constraints.SelectMany(c => c.Split(','))
						}).ToArray();
			}

			string searchTerm = parameters.ContainsKey("Query") ? parameters["Query"] : string.Empty; // This is needed for Notes/Attachement Search
			// If they specify facet constraints, we'll do a faceted search 
			// TODO this doesn't work, facetConstraints is never null
			var entityQuery = new CrmEntityQuery(queryText, pageNumber, pageSize, getLogicalNamesAsEnumerable(logicalNames), contextLanguage.ContextLanguage, multiLanguageEnabled, filterText, facetConstraints, sortingOption, searchTerm);

			var searcher = this.GetSearcher(provider);

			using (searcher)
			{
				var results = searcher.Search(entityQuery);
				var itemCount = results.ApproximateTotalHits;
				var pageCount = itemCount > 0 ? (int)Math.Ceiling(itemCount / (double)results.PageSize) : 0;

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Search term:{0}, length: {1}, results contain {2} items", searchTerm, searchTerm.Length, itemCount));

				var jsonResult = new JObject {
					{ "itemCount", itemCount },
					{ "pageNumber", results.PageNumber },
					{ "pageSize", results.PageSize },
					{ "pageCount", pageCount },
					{ "items", new JArray(results.Select(GetSearchResultJson)) }
				};

				if (results.FacetViews != null)
				{
					jsonResult.Add("facetViews", GetFacetViewsJson(results.FacetViews, logicalNames, initialConstraintsToLocalize));

					if (results.SortingOptions != null)
					{
						//todo: add calculation for sort options: searchOrder
						jsonResult.Add("sortingOptions", GetSortingOptionsJson(results.SortingOptions));
					}
				}
				return new JObjectResult(jsonResult);
			}
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult GetLocalizedLabels(string localizedLabelEntityName, string localizedLabelField, List<string> entityGuids)
		{
			IEnumerable<EntityReference> entityReferences = entityGuids.Select(guid => new EntityReference(localizedLabelEntityName, new Guid(guid)));
			var localizedLabelArray = new JObject();
			var localizedLabels = MapEntityReferencesToLocalizedLabels(entityReferences, localizedLabelField);
			foreach (var localizedLabel in localizedLabels)
			{
				localizedLabelArray.Add(localizedLabel.Key.ToString(), localizedLabel.Value);
			}
			return new JObjectResult(localizedLabelArray);
		}

		private JArray GetFacetViewsJson(IEnumerable<FacetView> facetViews, string logicalNames, int initialConstraintsToLocalize)
		{
			var facetViewsJson = new JArray();
			var hitCountsEnabled = FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching);

			foreach (var facetView in facetViews)
			{
				if (!IsFacetVisible(facetView.FacetName, logicalNames))
				{
					continue;
				}
				JArray facetViewJson = new JArray();
				if (facetView.FacetName == FixedFacetsConfiguration.ProductFieldFacetName)
				{
					facetViewJson = GetPartialLocalizedFacetViewJson(facetView, initialConstraintsToLocalize, "product", "name");
				}
				else
				{
					
					foreach (var constraintHit in facetView.ConstraintHits)
					{
						facetViewJson.Add(
							new JObject
							{
								{ "displayName", constraintHit.DisplayValue },
								{ "name", constraintHit.ConstraintValue },
								{ "hitCount", hitCountsEnabled ? constraintHit.HitCount.ToString() : string.Empty }
							});
					}
				}

				facetViewsJson.Add(
					new JObject
					{
						{ "facetName", facetView.FacetName },
						{ "facetDisplayName", facetView.FacetName },
						{ "facetData", facetViewJson }
					});
			}
			return facetViewsJson;
		}

		/// <summary>
		/// Partially localizes the facet view constraints (localizing them all initially is too costly).
		/// 
		/// Facet view JSON will be segmented into two objects, "localized" and "unlocalized", each with
		/// an array of constraint objects.
		/// </summary>
		/// <param name="facetView">The facet view to partially localize. Constraint values must be GUIDs.</param>
		/// <param name="initialConstraintsToLocalize">Number of constraints to initially localize.</param>
		/// <param name="localizedLabelEntityName">The name of the entity with the field to be localized.</param>
		/// <param name="localizedLabelField">Field on entity to be localized.</param>
		/// <returns></returns>
		private JArray GetPartialLocalizedFacetViewJson(FacetView facetView, int initialConstraintsToLocalize, string localizedLabelEntityName, string localizedLabelField)
		{
			JArray facetViewJson = new JArray();
			ConstraintHit[] constraintHits = facetView.ConstraintHits.ToArray();
			var hitCountsEnabled = FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching);

			// Convert the received GUID strings into entity references
			IEnumerable<EntityReference> entityReferencesToLocalize =
				facetView.ConstraintHits
					.Take(initialConstraintsToLocalize)
					.Select(constraintHit => new EntityReference(localizedLabelEntityName, new Guid(constraintHit.ConstraintValue)));
			// Get the localized labels based off the entity references and the field to localize
			var localizedLabels = MapEntityReferencesToLocalizedLabels(entityReferencesToLocalize, localizedLabelField);

			foreach (ConstraintHit constraint in constraintHits)
			{
				string localizedConstraintName;
				localizedLabels.TryGetValue(Guid.Parse(constraint.ConstraintValue), out localizedConstraintName);

				// Set localized labels, keep all not-localized labels empty
				facetViewJson.Add(
					new JObject
						{
							{ "displayName", localizedConstraintName },
							{ "name",  constraint.ConstraintValue },
							{ "hitCount", hitCountsEnabled ? constraint.HitCount.ToString() : string.Empty }
						});
			}
		
			return facetViewJson;
		}

		private Dictionary<Guid, string> MapEntityReferencesToLocalizedLabels(IEnumerable<EntityReference> entityReferences, string localizedLabelField)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = portal.ServiceContext;

			var contextLanguageInfo = this.HttpContext.GetContextLanguageInfo();
			var lcid = contextLanguageInfo.IsCrmMultiLanguageEnabled ? contextLanguageInfo.ContextLanguage.CrmLcid : CultureInfo.CurrentCulture.LCID;

			return
				entityReferences.AsParallel()
					.Select(
						entityReference =>
						new KeyValuePair<Guid, string>(
							entityReference.Id,
							serviceContext.RetrieveLocalizedLabel(entityReference, localizedLabelField, lcid)))
					.ToDictionary(x => x.Key, x => x.Value);
		}

		/// <summary>
		/// Gets the sorting options json.
		/// </summary>
		/// <param name="sortingOptions">The sorting options.</param>
		/// <returns>Sorting Options Json</returns>
		private JArray GetSortingOptionsJson(IEnumerable<string> sortingOptions)
		{
			var sortingOptionsJson = new JArray();

			foreach (var sortingOption in sortingOptions)
			{
				sortingOptionsJson.Add(sortingOption);
			}
			return sortingOptionsJson;
		}

		private bool IsFacetVisible(string facetName, string logicalNames)
		{
			return facetName != FixedFacetsConfiguration.RecordTypeFacetFieldName || string.IsNullOrEmpty(logicalNames);
		}

		private JObject GetSearchResultJson(ICrmEntitySearchResult @searchResult)
		{
			var json = new JObject
			{
				{ "entityID", @searchResult.EntityID.ToString() },
				{ "entityLogicalName", @searchResult.EntityLogicalName },
				{ "title", @searchResult.Title },
				{ "fragment", @searchResult.Fragment },
				{ "resultNumber", @searchResult.ResultNumber },
				{ "score", @searchResult.Score }
			};

			//Adding annotations to KnowledgeArticle
			var relatedNotes = @searchResult.Entity.GetAttributeValue<IEnumerable>("relatedNotes");
			if (relatedNotes != null)
			{
				var list = new JArray();
				foreach (var result in relatedNotes)
				{
					var note = result as CrmEntitySearchResult;
					if (note == null) continue;

					var relatedNote = new JObject
					{
						{ "entityID", note.EntityID },
						{ "entityLogicalName", note.EntityLogicalName },
						{ "title", note.Entity.GetAttributeValue<string>("filename") },
						{ "fragment", note.Fragment }, { "url", note.Url.ToString() },
						{ "absoluteUrl", BuildAbsoluteUrl(note.Url.ToString()) }
					};
					list.Add(relatedNote);
				}
				json["relatedNotes"] = list;
			}

			if (@searchResult.Url == null) return json;
			json["url"] = @searchResult.Url.ToString();
			json["absoluteUrl"] = BuildAbsoluteUrl(@searchResult.Url.ToString());

			return json;
		}

		protected string GetFilterText(string filter, IDictionary parameters)
		{
			if (string.IsNullOrEmpty(filter))
			{
				return string.Empty;
			}

			return Regex.Replace(filter, @"@(?<parameter>\w+)", match =>
			{
				var value = parameters[match.Groups["parameter"].Value];

				return value == null ? match.Value : value.ToString();
			});
		}

		protected bool TryGetLanguageCode(out ContextLanguageInfo contextLanguageInfo)
		{
			var contextLanguage = this.HttpContext.GetContextLanguageInfo();
			contextLanguageInfo = contextLanguage;
			return contextLanguage.IsCrmMultiLanguageEnabled;
		}

		protected string GetQueryText(string query, IDictionary parameters)
		{
			if (string.IsNullOrEmpty(query))
			{
				return string.Empty;
			}

			return Regex.Replace(query, @"@(?<parameter>\w+)", match =>
			{
				var value = parameters[match.Groups["parameter"].Value];

				return value == null ? match.Value : value.ToString();
			});
		}

		private IEnumerable<string> getLogicalNamesAsEnumerable(string logicalNames)
		{
			if (string.IsNullOrEmpty(logicalNames)) return Enumerable.Empty<string>();

			var values = logicalNames.Split(',').Where(s => !string.IsNullOrEmpty(s)).ToArray();

			return values.Any() ? values : Enumerable.Empty<string>();
		}

		/// <summary>
		/// Checks if indexexists.
		/// </summary>
		/// <param name="provider">The provider.</param>
		private ICrmEntityIndexSearcher GetSearcher(SearchProvider provider)
		{
			try
			{
				return provider.GetIndexSearcher();
			}
			catch (IndexNotFoundException)
			{
				using (var builder = provider.GetIndexBuilder())
				{
					builder.BuildIndex();
					return provider.GetIndexSearcher();
				}
			}
		}

		protected string BuildAbsoluteUrl(string url)
		{
			var baseUrl = this.HttpContext.GetSiteSetting("BaseURL");

			return !string.IsNullOrWhiteSpace(baseUrl)
				? ((Request.IsSecureConnection) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter + baseUrl + url
				: ((Request.IsSecureConnection) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter + (Request.Url != null ? Request.Url.Authority : null) + url;
		}
	}
}
