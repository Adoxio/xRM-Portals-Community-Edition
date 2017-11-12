/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Data.SqlTypes;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Collections.Generic;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web.UI.CrmEntityListView;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.ContentAccess;
	using Adxstudio.Xrm.Services;

	/// <summary>
	/// Data Adapter class for retrieving records defined by a view.
	/// </summary>
	public class ViewDataAdapter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="configuration"></param>
		/// <param name="dependencies"></param>
		/// <param name="page"></param>
		/// <param name="search"></param>
		/// <param name="order"></param>
		/// <param name="filter"></param>
		/// <param name="metaFilter"></param>
		/// <param name="applyRecordLevelFilters"></param>
		/// <param name="entityMetadata"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ViewDataAdapter(ViewConfiguration configuration, IDataAdapterDependencies dependencies, int page = 1, string search = null, string order = null, string filter = null, string metaFilter = null, bool applyRecordLevelFilters = true, EntityMetadata entityMetadata = null, IDictionary<string, string> customParameters = null, EntityReference regarding = null)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Configuration = configuration;
			Page = page;
			Search = search;
			Order = order;
			Filter = filter;
			MetaFilter = metaFilter;
			Dependencies = dependencies;
			ApplyRecordLevelFilters = applyRecordLevelFilters;
		    FilterCollection = new List<Filter>();
		    Regarding = regarding;

			if (entityMetadata == null)
			{
				var serviceContext = Dependencies.GetServiceContext();
				var response = (RetrieveEntityResponse)serviceContext.Execute(new RetrieveEntityRequest
				{
					LogicalName = configuration.EntityName,
					EntityFilters = EntityFilters.Attributes,
				});

				EntityMetadata = response.EntityMetadata;
			}
			else
			{
				EntityMetadata = entityMetadata;
			}

			CustomParameters = customParameters;
		}

		/// <summary>
		/// Constructor that will filter the query further by applying a filter condition based on the properties.
		/// </summary>
		/// <param name="configuration"></param>
		/// <param name="dependencies"></param>
		/// <param name="filterRelationshipName">Schema name of the relationship to filter on.</param>
		/// <param name="filterEntityName">Entity logical name of the related record to filter on.</param>
		/// <param name="filterAttributeName">Logical name of the attribute containing the value to filter on.</param>
		/// <param name="filterValue">Uniqueidentifier of the related record to be assigned to the filter condition value.</param>
		/// <param name="page"></param>
		/// <param name="search"></param>
		/// <param name="order"></param>
		/// <param name="filter"></param>
		/// <param name="metaFilter"></param>
		/// <param name="applyRecordLevelFilters"></param>
		/// <param name="entityMetadata"></param>
		public ViewDataAdapter(ViewConfiguration configuration, IDataAdapterDependencies dependencies, string filterRelationshipName, string filterEntityName, string filterAttributeName, Guid filterValue, int page = 1, string search = null, string order = null, string filter = null, string metaFilter = null, bool applyRecordLevelFilters = true, EntityMetadata entityMetadata = null, IDictionary<string, string> customParameters = null, EntityReference regarding = null)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Configuration = configuration;
			Page = page;
			Search = search;
			Order = order;
			Filter = filter;
			MetaFilter = metaFilter;
			Dependencies = dependencies;
			ApplyRecordLevelFilters = applyRecordLevelFilters;
			ApplyRelatedRecordFilter = !string.IsNullOrWhiteSpace(filterRelationshipName) &&
										!string.IsNullOrWhiteSpace(filterEntityName) && filterValue != Guid.Empty;
			FilterRelationshipName = filterRelationshipName;
			FilterEntityName = filterEntityName;
			FilterAttributeName = filterAttributeName;
			FilterValue = filterValue;
            FilterCollection = new List<Filter>();
            Regarding = regarding;

			if (entityMetadata == null)
			{
				var serviceContext = Dependencies.GetServiceContext();
				var response = (RetrieveEntityResponse)serviceContext.Execute(new RetrieveEntityRequest
				{
					LogicalName = configuration.EntityName,
					EntityFilters = EntityFilters.All,
				});

				EntityMetadata = response.EntityMetadata;
			}
			else
			{
				EntityMetadata = entityMetadata;
			}

			CustomParameters = customParameters;
		}

		/// <summary>
		/// Data adataper dependencies
		/// </summary>
		protected IDataAdapterDependencies Dependencies { get; private set; }

		/// <summary>
		/// Current configuration settings
		/// </summary>
		protected ViewConfiguration Configuration { get; private set; }

		protected IDictionary<string, string> CustomParameters { get; private set; }

		public bool DoQueryPerRecordLevelFilter { get; set; }

		/// <summary>
		/// Current page number
		/// </summary>
		public int Page { get; set; }

		/// <summary>
		/// Search query
		/// </summary>
		public string Search { get; set; }

		/// <summary>
		/// Sort expression
		/// </summary>
		public string Order { get; set; }

		/// <summary>
		/// Selectable filter
		/// </summary>
		public string Filter { get; set; }

		/// <summary>
		/// Metadata filter
		/// </summary>
		public string MetaFilter { get; set; }

		/// <summary>
		/// Entity Metadata containing attributes
		/// </summary>
		public EntityMetadata EntityMetadata { get; set; }

        /// <summary>
		/// Collection of Filters that can be added to the Fetch
		/// </summary>
		public List<Filter> FilterCollection { get; set; }

        /// <summary>
		/// EntityReference
		/// </summary>
		public EntityReference Regarding { get; set; }

		private bool EntityPermissionDenied { get; set; }

		private bool ApplyRecordLevelFilters { get; set; }

		private bool ApplyRelatedRecordFilter { get; set; }

		private string FilterRelationshipName { get; set; }

		private string FilterEntityName { get; set; }

		private string FilterAttributeName { get; set; }

		private Guid FilterValue { get; set; }

		private const string DisplayAllActivitiesOnTimeline = "CustomerSupport/DisplayAllUserActivitiesOnTimeline";

		public virtual FetchResult FetchEntities()
		{
			return DoQueryPerRecordLevelFilter
				? FetchEntitiesWithQueryPerRecordLevelLevelFilter()
				: FetchEntitiesWithSingleQuery();
		}

		/// <summary>
		/// Executes a Fetch based on the view configuration and retrieves the resulting entity records.
		/// </summary>
		protected virtual FetchResult FetchEntitiesWithSingleQuery()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var view = Configuration.GetEntityView(serviceContext, Configuration.LanguageCode);

			var fetchXml = string.IsNullOrWhiteSpace(Configuration.FetchXml) ? view.FetchXml.ToString() : Configuration.FetchXml;

			var fetch = Fetch.Parse(fetchXml);

			AddSelectableFilterToFetchEntity(fetch.Entity, Configuration, Filter);
			AddWebsiteFilterToFetchEntity(fetch.Entity, Configuration);
			AddOrderToFetch(fetch.Entity, Order);
			AddPaginationToFetch(fetch, Configuration.DataPagerEnabled ?? true, fetch.PagingCookie, Page, Configuration.PageSize,
				true);
			AddSearchFilterToFetchEntity(fetch.Entity, Configuration, Search);
			AddMetadataFilterToFetch(fetch, Configuration, MetaFilter);

			if (Configuration.EntityName == "entitlement")
			{
				// To ensure that we only return the entitlement records that are applicable to an incident for the given dynamic parameters from the incident submission form we must dynamically and conditionally generate the necessary joins and filter conditions and add them to the FetchXml query.

				if (CustomParameters != null && CustomParameters.Any())
				{
					ApplyEntitlementFilter(fetch, CustomParameters);
					AddAttributesToFetchEntity(fetch.Entity, new List<string> { "isdefault" });
				}
				else
				{
					return new FetchResult(Enumerable.Empty<Entity>());
				}
			}
			else
			{
				if (ApplyRecordLevelFilters)
				{
					AddRecordLevelFiltersToFetch(fetch, Configuration, CrmEntityPermissionRight.Read);
				}
				else
				{
					TryAssert(fetch, Configuration, CrmEntityPermissionRight.Read);
				}

				ApplyTimelineFilterToFetch(serviceContext, fetch);
			}

			if (ApplyRelatedRecordFilter)
			{
				AddRelatedRecordFilterToFetch(fetch, FilterRelationshipName, FilterEntityName, FilterValue);
			}
            
            foreach (var currentFilter in FilterCollection)
            {
                AddFilterToFetch(fetch, currentFilter);
            }

            //check metadata to ensure statecode and statuscode are present

            if (EntityMetadata.Attributes.Any(a => a.LogicalName == "statecode"))
			{
				AddAttributesToFetchEntity(fetch.Entity, new List<string> { "statecode" });

				if (EntityMetadata.Attributes.Any(a => a.LogicalName == "statuscode"))
				{
					AddAttributesToFetchEntity(fetch.Entity, new List<string> { "statuscode" });
				}
			}

			// Apply any special case view modifications for this entity
			IViewSpecialCase specialCase;

			if (SpecialCases.TryGetValue(Configuration.EntityName, out specialCase))
			{
				specialCase.TryApply(Configuration, Dependencies, CustomParameters, fetch);
			}

			return FetchEntities(serviceContext, fetch);
		}

		/// <summary>
		/// Executes a Fetch based on the view configuration and retrieves the resulting entity records.
		/// </summary>
		protected virtual FetchResult FetchEntitiesWithQueryPerRecordLevelLevelFilter()
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Fetching entities with a seperate query for each record level filter.");

			var serviceContext = Dependencies.GetServiceContext();

			var view = Configuration.GetEntityView(serviceContext, Configuration.LanguageCode);

			var fetchXml = string.IsNullOrWhiteSpace(Configuration.FetchXml) ? view.FetchXml.ToString() : Configuration.FetchXml;

			var fetch = Fetch.Parse(fetchXml);

			AddSelectableFilterToFetchEntity(fetch.Entity, Configuration, Filter);
			AddWebsiteFilterToFetchEntity(fetch.Entity, Configuration);
			AddOrderToFetch(fetch.Entity, Order);
			AddPaginationToFetch(fetch, Configuration.DataPagerEnabled ?? true, null, 1, (Page * Configuration.PageSize) + 1, false);
			AddSearchFilterToFetchEntity(fetch.Entity, Configuration, Search);
			AddMetadataFilterToFetch(fetch, Configuration, MetaFilter);

			if (ApplyRelatedRecordFilter)
			{
				AddRelatedRecordFilterToFetch(fetch, FilterRelationshipName, FilterEntityName, FilterValue);
			}
			
			foreach (var currentFilter in FilterCollection)
			{
				AddFilterToFetch(fetch, currentFilter);
			}

			//check metadata to ensure statecode and statuscode are present

			if (EntityMetadata.Attributes.Any(a => a.LogicalName == "statecode"))
			{
				AddAttributesToFetchEntity(fetch.Entity, new List<string> { "statecode" });

				if (EntityMetadata.Attributes.Any(a => a.LogicalName == "statuscode"))
				{
					AddAttributesToFetchEntity(fetch.Entity, new List<string> { "statuscode" });
				}
			}

			// Apply any special case view modifications for this entity
			IViewSpecialCase specialCase;

			if (SpecialCases.TryGetValue(Configuration.EntityName, out specialCase))
			{
				specialCase.TryApply(Configuration, Dependencies, CustomParameters, fetch);
			}

			if (Configuration.EntityName == "entitlement")
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Doing special case behavior for entitlements.");

				// To ensure that we only return the entitlement records that are applicable to an incident for the given dynamic parameters from the incident submission form we must dynamically and conditionally generate the necessary joins and filter conditions and add them to the FetchXml query.
				if (CustomParameters != null && CustomParameters.Any())
				{
					ApplyEntitlementFilter(fetch, CustomParameters);
					AddAttributesToFetchEntity(fetch.Entity, new List<string> { "isdefault" });
				}
				else
				{
					return new FetchResult(Enumerable.Empty<Entity>());
				}

				return FetchEntities(serviceContext, fetch);
			}

			ApplyTimelineFilterToFetch(serviceContext, fetch);

			if (!ApplyRecordLevelFilters)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not applying record-level filters. Falling back to single-query behavior.");

				TryAssert(fetch, Configuration, CrmEntityPermissionRight.Read);

				return FetchEntities(serviceContext, fetch);
			}

			var queries = GenerateFetchForEachRecordLevelFilter(fetch, Configuration, CrmEntityPermissionRight.Read).ToArray();

			// If we don't actually have multiple queries to execute, fall back to that path instead so as to 
			// allow CRM to do the ordering, and support normal pagination.
			if (queries.Length <= 1)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Only one record-level filter necessary, falling back to single-query behavior.");

				return FetchEntitiesWithSingleQuery();
			}

			var results = new ConcurrentBag<FetchResult>();

			Parallel.ForEach(queries, query => results.Add(FetchEntities(serviceContext, query)));

			return UnionResults(results.AsEnumerable(), fetch.Entity.Orders);
		}

		private FetchResult UnionResults(IEnumerable<FetchResult> results, IEnumerable<Order> orders)
		{
			var union = results
				.SelectMany(e => e.Records)
				.Distinct(new EntityEqualityComparer())
				.ToArray();

			IOrderedEnumerable<Entity> ordered = null;

			foreach (var order in orders)
			{
				var orderAttribute = order.Attribute;

				ordered = ordered == null
					? (order.Direction.GetValueOrDefault(OrderType.Ascending) == OrderType.Ascending
						? OrderBy(union, orderAttribute)
						: OrderByDescending(union, orderAttribute))
					: (order.Direction.GetValueOrDefault(OrderType.Ascending) == OrderType.Ascending
						? ThenBy(ordered, orderAttribute)
						: ThenByDescending(ordered, orderAttribute));
			}

			// Default ordering is by SQL unique identifier, which has different sorting rules than standard
			// Guids. Apply in absense of explicit order, or as secondary or tertiary order.
			ordered = ordered == null
				? union.OrderBy(e => new SqlGuid(e.Id))
				: ordered.ThenBy(e => new SqlGuid(e.Id));

			var page = ordered
				.Skip((Page - 1) * Configuration.PageSize)
				.Take(Configuration.PageSize)
				.ToArray();

			return new FetchResult(page, union.Length > (Page * Configuration.PageSize), EntityPermissionDenied);
		}

		private IOrderedEnumerable<Entity> OrderBy(IEnumerable<Entity> records, string orderAttribute)
		{
			var attributeMetadata = EntityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == orderAttribute);

			if (attributeMetadata == null)
			{
				return records.OrderBy(e => e.GetAttributeValue(orderAttribute));
			}

			switch (attributeMetadata.AttributeType)
			{
				case AttributeTypeCode.Memo:
				case AttributeTypeCode.String:
					return records.OrderBy(e => e.GetAttributeValue<string>(orderAttribute), StringComparer.CurrentCulture);

				case AttributeTypeCode.Boolean:
					var booleanAttributeMetadata = attributeMetadata as BooleanAttributeMetadata;

					if (booleanAttributeMetadata == null)
					{
						return records.OrderBy(e => e.GetAttributeValue<bool?>(orderAttribute));
					}

					var booleanLabelLookup = GetBooleanAttributeLocalizedLabelLookup(booleanAttributeMetadata);

					var @default = booleanAttributeMetadata.DefaultValue.GetValueOrDefault();

					return records.OrderBy(e => booleanLabelLookup[e.GetAttributeValue<bool?>(orderAttribute).GetValueOrDefault(@default)], StringComparer.CurrentCulture);

				case AttributeTypeCode.Picklist:
				case AttributeTypeCode.State:
				case AttributeTypeCode.Status:
					var enumAttributeMetadata = attributeMetadata as EnumAttributeMetadata;

					if (enumAttributeMetadata == null)
					{
						return records.OrderBy(e =>
						{
							var optionSetValue = e.GetAttributeValue<OptionSetValue>(orderAttribute);

							return optionSetValue == null ? null : new int?(optionSetValue.Value);
						});
					}

					var enumLabelLookup = GetEnumAttributeLocalizedLabelLookup(enumAttributeMetadata);

					return records.OrderBy(e =>
					{
						var optionSetValue = e.GetAttributeValue<OptionSetValue>(orderAttribute);

						if (optionSetValue == null)
						{
							return null;
						}

						string label;

						return enumLabelLookup.TryGetValue(optionSetValue.Value, out label)
							? label
							: optionSetValue.Value.ToString();
					}, StringComparer.CurrentCulture);

				case AttributeTypeCode.Lookup:
				case AttributeTypeCode.Customer:
				case AttributeTypeCode.Owner:
					return records.OrderBy(e =>
					{
						var reference = e.GetAttributeValue<EntityReference>(orderAttribute);

						return reference == null ? string.Empty : reference.Name;
					}, StringComparer.CurrentCulture);

				case AttributeTypeCode.BigInt:
					return records.OrderBy(e => e.GetAttributeValue<long?>(orderAttribute));

				case AttributeTypeCode.Integer:
					return records.OrderBy(e => e.GetAttributeValue<int?>(orderAttribute));

				case AttributeTypeCode.Double:
					return records.OrderBy(e => e.GetAttributeValue<double?>(orderAttribute));

				case AttributeTypeCode.Decimal:
					return records.OrderBy(e => e.GetAttributeValue<decimal?>(orderAttribute));

				case AttributeTypeCode.Money:
					return records.OrderBy(e =>
					{
						var money = e.GetAttributeValue<Money>(orderAttribute);

						return money == null ? null : new decimal?(money.Value);
					});

				case AttributeTypeCode.DateTime:
					return records.OrderBy(e => e.GetAttributeValue<DateTime?>(orderAttribute));

				case AttributeTypeCode.Uniqueidentifier:
					return records.OrderBy(e => new SqlGuid(e.GetAttributeValue<Guid?>(orderAttribute).GetValueOrDefault()));

				default:
					return records.OrderBy(e => e.GetAttributeValue(orderAttribute));
			}
		}

		private IOrderedEnumerable<Entity> OrderByDescending(IEnumerable<Entity> records, string orderAttribute)
		{
			var attributeMetadata = EntityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == orderAttribute);

			if (attributeMetadata == null)
			{
				return records.OrderByDescending(e => e.GetAttributeValue(orderAttribute));
			}

			switch (attributeMetadata.AttributeType)
			{
				case AttributeTypeCode.Memo:
				case AttributeTypeCode.String:
					return records.OrderByDescending(e => e.GetAttributeValue<string>(orderAttribute), StringComparer.CurrentCulture);

				case AttributeTypeCode.Boolean:
					var booleanAttributeMetadata = attributeMetadata as BooleanAttributeMetadata;

					if (booleanAttributeMetadata == null)
					{
						return records.OrderByDescending(e => e.GetAttributeValue<bool?>(orderAttribute));
					}

					var booleanLabelLookup = GetBooleanAttributeLocalizedLabelLookup(booleanAttributeMetadata);

					var @default = booleanAttributeMetadata.DefaultValue.GetValueOrDefault();

					return records.OrderByDescending(e => booleanLabelLookup[e.GetAttributeValue<bool?>(orderAttribute).GetValueOrDefault(@default)], StringComparer.CurrentCulture);

				case AttributeTypeCode.Picklist:
				case AttributeTypeCode.State:
				case AttributeTypeCode.Status:
					var enumAttributeMetadata = attributeMetadata as EnumAttributeMetadata;

					if (enumAttributeMetadata == null)
					{
						return records.OrderByDescending(e =>
						{
							var optionSetValue = e.GetAttributeValue<OptionSetValue>(orderAttribute);

							return optionSetValue == null ? null : new int?(optionSetValue.Value);
						});
					}

					var enumLabelLookup = GetEnumAttributeLocalizedLabelLookup(enumAttributeMetadata);

					return records.OrderByDescending(e =>
					{
						var optionSetValue = e.GetAttributeValue<OptionSetValue>(orderAttribute);

						if (optionSetValue == null)
						{
							return null;
						}

						string label;

						return enumLabelLookup.TryGetValue(optionSetValue.Value, out label)
							? label
							: optionSetValue.Value.ToString();
					}, StringComparer.CurrentCulture);

				case AttributeTypeCode.Lookup:
				case AttributeTypeCode.Customer:
				case AttributeTypeCode.Owner:
					return records.OrderByDescending(e =>
					{
						var reference = e.GetAttributeValue<EntityReference>(orderAttribute);

						return reference == null ? string.Empty : reference.Name;
					}, StringComparer.CurrentCulture);

				case AttributeTypeCode.BigInt:
					return records.OrderByDescending(e => e.GetAttributeValue<long?>(orderAttribute));

				case AttributeTypeCode.Integer:
					return records.OrderByDescending(e => e.GetAttributeValue<int?>(orderAttribute));

				case AttributeTypeCode.Double:
					return records.OrderByDescending(e => e.GetAttributeValue<double?>(orderAttribute));

				case AttributeTypeCode.Decimal:
					return records.OrderByDescending(e => e.GetAttributeValue<decimal?>(orderAttribute));

				case AttributeTypeCode.Money:
					return records.OrderByDescending(e =>
					{
						var money = e.GetAttributeValue<Money>(orderAttribute);

						return money == null ? null : new decimal?(money.Value);
					});

				case AttributeTypeCode.DateTime:
					return records.OrderByDescending(e => e.GetAttributeValue<DateTime?>(orderAttribute));

				case AttributeTypeCode.Uniqueidentifier:
					return records.OrderByDescending(e => new SqlGuid(e.GetAttributeValue<Guid?>(orderAttribute).GetValueOrDefault()));

				default:
					return records.OrderByDescending(e => e.GetAttributeValue(orderAttribute));
			}
		}

		private IOrderedEnumerable<Entity> ThenBy(IOrderedEnumerable<Entity> records, string orderAttribute)
		{
			var attributeMetadata = EntityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == orderAttribute);

			if (attributeMetadata == null)
			{
				return records.ThenBy(e => e.GetAttributeValue(orderAttribute));
			}

			switch (attributeMetadata.AttributeType)
			{
				case AttributeTypeCode.Memo:
				case AttributeTypeCode.String:
					return records.ThenBy(e => e.GetAttributeValue<string>(orderAttribute), StringComparer.CurrentCulture);

				case AttributeTypeCode.Boolean:
					var booleanAttributeMetadata = attributeMetadata as BooleanAttributeMetadata;

					if (booleanAttributeMetadata == null)
					{
						return records.ThenBy(e => e.GetAttributeValue<bool?>(orderAttribute));
					}

					var booleanLabelLookup = GetBooleanAttributeLocalizedLabelLookup(booleanAttributeMetadata);

					var @default = booleanAttributeMetadata.DefaultValue.GetValueOrDefault();

					return records.ThenBy(e => booleanLabelLookup[e.GetAttributeValue<bool?>(orderAttribute).GetValueOrDefault(@default)], StringComparer.CurrentCulture);

				case AttributeTypeCode.Picklist:
				case AttributeTypeCode.State:
				case AttributeTypeCode.Status:
					var enumAttributeMetadata = attributeMetadata as EnumAttributeMetadata;

					if (enumAttributeMetadata == null)
					{
						return records.ThenBy(e =>
						{
							var optionSetValue = e.GetAttributeValue<OptionSetValue>(orderAttribute);

							return optionSetValue == null ? null : new int?(optionSetValue.Value);
						});
					}

					var enumLabelLookup = GetEnumAttributeLocalizedLabelLookup(enumAttributeMetadata);

					return records.ThenBy(e =>
					{
						var optionSetValue = e.GetAttributeValue<OptionSetValue>(orderAttribute);

						if (optionSetValue == null)
						{
							return null;
						}

						string label;

						return enumLabelLookup.TryGetValue(optionSetValue.Value, out label)
							? label
							: optionSetValue.Value.ToString();
					}, StringComparer.CurrentCulture);

				case AttributeTypeCode.Lookup:
				case AttributeTypeCode.Customer:
				case AttributeTypeCode.Owner:
					return records.ThenBy(e =>
					{
						var reference = e.GetAttributeValue<EntityReference>(orderAttribute);

						return reference == null ? string.Empty : reference.Name;
					}, StringComparer.CurrentCulture);

				case AttributeTypeCode.BigInt:
					return records.ThenBy(e => e.GetAttributeValue<long?>(orderAttribute));

				case AttributeTypeCode.Integer:
					return records.ThenBy(e => e.GetAttributeValue<int?>(orderAttribute));

				case AttributeTypeCode.Double:
					return records.ThenBy(e => e.GetAttributeValue<double?>(orderAttribute));

				case AttributeTypeCode.Decimal:
					return records.ThenBy(e => e.GetAttributeValue<decimal?>(orderAttribute));

				case AttributeTypeCode.Money:
					return records.ThenBy(e =>
					{
						var money = e.GetAttributeValue<Money>(orderAttribute);

						return money == null ? null : new decimal?(money.Value);
					});

				case AttributeTypeCode.DateTime:
					return records.ThenBy(e => e.GetAttributeValue<DateTime?>(orderAttribute));

				case AttributeTypeCode.Uniqueidentifier:
					return records.ThenBy(e => new SqlGuid(e.GetAttributeValue<Guid?>(orderAttribute).GetValueOrDefault()));

				default:
					return records.ThenBy(e => e.GetAttributeValue(orderAttribute));
			}
		}

		private IOrderedEnumerable<Entity> ThenByDescending(IOrderedEnumerable<Entity> records, string orderAttribute)
		{
			var attributeMetadata = EntityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == orderAttribute);

			if (attributeMetadata == null)
			{
				return records.ThenByDescending(e => e.GetAttributeValue(orderAttribute));
			}

			switch (attributeMetadata.AttributeType)
			{
				case AttributeTypeCode.Memo:
				case AttributeTypeCode.String:
					return records.ThenByDescending(e => e.GetAttributeValue<string>(orderAttribute), StringComparer.CurrentCulture);

				case AttributeTypeCode.Boolean:
					var booleanAttributeMetadata = attributeMetadata as BooleanAttributeMetadata;

					if (booleanAttributeMetadata == null)
					{
						return records.ThenByDescending(e => e.GetAttributeValue<bool?>(orderAttribute));
					}

					var booleanLabelLookup = GetBooleanAttributeLocalizedLabelLookup(booleanAttributeMetadata);

					var @default = booleanAttributeMetadata.DefaultValue.GetValueOrDefault();

					return records.ThenByDescending(e => booleanLabelLookup[e.GetAttributeValue<bool?>(orderAttribute).GetValueOrDefault(@default)], StringComparer.CurrentCulture);

				case AttributeTypeCode.Picklist:
				case AttributeTypeCode.State:
				case AttributeTypeCode.Status:
					var enumAttributeMetadata = attributeMetadata as EnumAttributeMetadata;

					if (enumAttributeMetadata == null)
					{
						return records.ThenByDescending(e =>
						{
							var optionSetValue = e.GetAttributeValue<OptionSetValue>(orderAttribute);

							return optionSetValue == null ? null : new int?(optionSetValue.Value);
						});
					}

					var enumLabelLookup = GetEnumAttributeLocalizedLabelLookup(enumAttributeMetadata);

					return records.ThenByDescending(e =>
					{
						var optionSetValue = e.GetAttributeValue<OptionSetValue>(orderAttribute);

						if (optionSetValue == null)
						{
							return null;
						}

						string label;

						return enumLabelLookup.TryGetValue(optionSetValue.Value, out label)
							? label
							: optionSetValue.Value.ToString();
					}, StringComparer.CurrentCulture);

				case AttributeTypeCode.Lookup:
				case AttributeTypeCode.Customer:
				case AttributeTypeCode.Owner:
					return records.ThenByDescending(e =>
					{
						var reference = e.GetAttributeValue<EntityReference>(orderAttribute);

						return reference == null ? string.Empty : reference.Name;
					}, StringComparer.CurrentCulture);

				case AttributeTypeCode.BigInt:
					return records.ThenByDescending(e => e.GetAttributeValue<long?>(orderAttribute));

				case AttributeTypeCode.Integer:
					return records.ThenByDescending(e => e.GetAttributeValue<int?>(orderAttribute));

				case AttributeTypeCode.Double:
					return records.ThenByDescending(e => e.GetAttributeValue<double?>(orderAttribute));

				case AttributeTypeCode.Decimal:
					return records.ThenByDescending(e => e.GetAttributeValue<decimal?>(orderAttribute));

				case AttributeTypeCode.Money:
					return records.ThenByDescending(e =>
					{
						var money = e.GetAttributeValue<Money>(orderAttribute);

						return money == null ? null : new decimal?(money.Value);
					});

				case AttributeTypeCode.DateTime:
					return records.ThenByDescending(e => e.GetAttributeValue<DateTime?>(orderAttribute));
				
				case AttributeTypeCode.Uniqueidentifier:
					return records.ThenByDescending(e => new SqlGuid(e.GetAttributeValue<Guid?>(orderAttribute).GetValueOrDefault()));

				default:
					return records.ThenByDescending(e => e.GetAttributeValue(orderAttribute));
			}
		}

		private IDictionary<bool, string> GetBooleanAttributeLocalizedLabelLookup(BooleanAttributeMetadata booleanAttributeMetadata)
		{
			var lcid = Configuration.LanguageCode;

			var falseLabel = booleanAttributeMetadata.OptionSet.FalseOption.Label.LocalizedLabels
				.FirstOrDefault(e => e.LanguageCode == lcid);

			var falseString = falseLabel == null
				? booleanAttributeMetadata.OptionSet.FalseOption.Label.UserLocalizedLabel.Label
				: falseLabel.Label;

			var trueLabel = booleanAttributeMetadata.OptionSet.TrueOption.Label.LocalizedLabels
				.FirstOrDefault(e => e.LanguageCode == lcid);

			var trueString = trueLabel == null
				? booleanAttributeMetadata.OptionSet.TrueOption.Label.UserLocalizedLabel.Label
				: trueLabel.Label;

			return new Dictionary<bool, string>
			{
				{ false, falseString },
				{ true,  trueString  }
			};
		}

		private IDictionary<int, string> GetEnumAttributeLocalizedLabelLookup(EnumAttributeMetadata enumAttributeMetadata)
		{
			var lcid = Configuration.LanguageCode;

			return enumAttributeMetadata.OptionSet.Options.ToDictionary(e => e.Value.GetValueOrDefault(), e =>
			{
				var localizedLabel = e.Label.LocalizedLabels.FirstOrDefault(label => label.LanguageCode == lcid);

				return localizedLabel == null ? e.Label.UserLocalizedLabel.Label : localizedLabel.Label;
			});
		}

		/// <summary>
		/// Adds the necessary joins and filter conditions to reduce the set of applicable entitlement records for the given filter parameters provided by the case submission form.
		/// </summary>
		/// <example>
		/// <code>
		/// <![CDATA[
		/// <fetch mapping="logical" version="1.0">
		///   <entity name="entitlement">
		///     <attribute name="entitlementid" />
		///     <attribute name="name" />
		///     <attribute name="createdon" />
		///     <filter type="and">
		///       <condition attribute="statecode" operator="eq" value="1" />
		///       <condition attribute="statecode" operator="eq" value="1" />
		///       <condition attribute="customerid" operator="eq" value="191ef49f-5cd7-e511-80d6-00155d038109" />
		///       <condition attribute="startdate" operator="on-or-before" value="2016-03-01T19:32:10.6844243Z" />
		///       <condition attribute="enddate" operator="on-or-after" value="2016-03-01T19:32:10.6844243Z" />
		///       <filter type="or">
		///         <condition attribute="restrictcasecreation" operator="eq" value="0" />
		///         <filter type="and">
		///           <condition attribute="restrictcasecreation" operator="eq" value="1" />
		///           <condition attribute="remainingterms" operator="gt" value="0" />
		///         </filter>
		///       </filter>
		///       <filter type="or">
		///         <condition entityname="restrictedcontacts" attribute="contactid" operator="null" />
		///         <filter type="and">
		///           <condition entityname="restrictedcontacts" attribute="contactid" operator="not-null" />
		///           <condition entityname="restrictedcontacts" attribute="contactid" operator="eq" value="99db51a2-c34e-e111-bb8d-00155d03a715" />
		///         </filter>
		///       </filter>
		///       <filter type="or">
		///         <condition entityname="restrictedproducts" attribute="productid" operator="null" />
		///         <filter type="and">
		///           <condition entityname="restrictedproducts" attribute="productid" operator="not-null" />
		///           <condition entityname="restrictedproducts" attribute="productid" operator="eq" value="1fb0dfb4-5ad7-e511-80d6-00155d038109" />
		///         </filter>
		///       </filter>
		///     </filter>
		///     <link-entity name="entitlementchannel" from="entitlementid" to="entitlementid" alias="restrictedchannels" link-type="outer">
		///       <attribute name="channel" />
		///       <attribute name="remainingterms" />
		///     </link-entity>
		///     <link-entity name="entitlementcontacts" from="entitlementid" to="entitlementid" link-type="outer" visible="false" intersect="true">
		///       <link-entity name="contact" from="contactid" to="contactid" alias="restrictedcontacts" link-type="outer" />
		///     </link-entity>
		///     <link-entity name="entitlementproducts" from="entitlementid" to="entitlementid" link-type="outer" visible="false" intersect="true">
		///       <link-entity name="product" from="productid" to="productid" alias="restrictedproducts" link-type="outer" />
		///     </link-entity>
		///   </entity>
		/// </fetch>
		/// ]]>
		/// </code>
		/// </example>
		protected void ApplyEntitlementFilter(Fetch fetch, IDictionary<string, string> filterParameters)
		{
			Guid customerId;
			Guid primaryContactId;
			Guid productId;
			string customerIdString;
			string customerTypeString;
			string primaryContactIdString;
			string productIdString;

			filterParameters.TryGetValue("customerid", out customerIdString);
			filterParameters.TryGetValue("customertype", out customerTypeString);
			filterParameters.TryGetValue("primarycontactid", out primaryContactIdString);
			filterParameters.TryGetValue("productid", out productIdString);

			Guid.TryParse(customerIdString, out customerId);

			var channelLink = new Link
			{
				Name = "entitlementchannel",
				FromAttribute = "entitlementid",
				ToAttribute = "entitlementid",
				Type = JoinOperator.LeftOuter,
				Alias = "restrictedchannels",
				Attributes = new List<FetchAttribute>
				{
					new FetchAttribute("channel"),
					new FetchAttribute("remainingterms")
				}
			};

			AddLinkToFetch(fetch, channelLink);

			var filter = new Filter
			{
				Type = LogicalOperator.And,
				Conditions = new List<Condition>
					{
						new Condition
						{
							Attribute = "statecode",
							Operator = ConditionOperator.Equal,
							Value = 1
						},
						new Condition
						{
							Attribute = "customerid",
							Operator = ConditionOperator.Equal,
							Value = customerId
						},
						new Condition
						{
							Attribute = "startdate",
							Operator = ConditionOperator.OnOrBefore,
							Value = DateTime.UtcNow
						},
						new Condition
						{
							Attribute = "enddate",
							Operator = ConditionOperator.OnOrAfter,
							Value = DateTime.UtcNow
						}
					},
				Filters = new List<Filter>
				{
					new Filter
						{
							Type = LogicalOperator.Or,
							Conditions = new List<Condition>
							{
								new Condition
								{
									Attribute = "restrictcasecreation",
									Operator = ConditionOperator.Equal,
									Value = 0
								}
							},
							Filters = new List<Filter>
							{
								new Filter
								{
									Type = LogicalOperator.And,
									Conditions = new List<Condition>
									{
										new Condition
										{
											Attribute = "restrictcasecreation",
											Operator = ConditionOperator.Equal,
											Value = 1
										},
										new Condition
										{
											Attribute = "remainingterms",
											Operator = ConditionOperator.GreaterThan,
											Value = 0
										}
									}
								}
							}
						}
				}
			};

			AddFilterToFetch(fetch, filter);

			if (customerTypeString == "account" && !string.IsNullOrWhiteSpace(primaryContactIdString) && Guid.TryParse(primaryContactIdString, out primaryContactId))
			{
				var contactLink = new Link
				{
					Name = "entitlementcontacts",
					FromAttribute = "entitlementid",
					ToAttribute = "entitlementid",
					Intersect = true,
					Visible = false,
					Type = JoinOperator.LeftOuter,
					Links = new List<Link>
						{
							new Link
							{
								Name = "contact",
								FromAttribute = "contactid",
								ToAttribute = "contactid",
								Alias = "restrictedcontacts",
								Type = JoinOperator.LeftOuter
							}
						}
				};

				AddLinkToFetch(fetch, contactLink);

				var contactFilter = new Filter
				{
					Type = LogicalOperator.And,
					Filters = new List<Filter>
					{
						new Filter
						{
							Type = LogicalOperator.Or,
							Conditions = new List<Condition>
							{
								new Condition
								{
									EntityName = "restrictedcontacts",
									Attribute = "contactid",
									Operator = ConditionOperator.Null
								}
							},
							Filters = new List<Filter>
							{
								new Filter
								{
									Type = LogicalOperator.And,
									Conditions = new List<Condition>
									{
										new Condition
										{
											EntityName = "restrictedcontacts",
											Attribute = "contactid",
											Operator = ConditionOperator.NotNull
										},
										new Condition
										{
											EntityName = "restrictedcontacts",
											Attribute = "contactid",
											Operator = ConditionOperator.Equal,
											Value = primaryContactId
										}
									}
								}
							}
						}

					}
				};

				AddFilterToFetch(fetch, contactFilter);
			}

			if (!string.IsNullOrWhiteSpace(productIdString) && Guid.TryParse(productIdString, out productId))
			{
				var productLink = new  Link
				{
					Name = "entitlementproducts",
					FromAttribute = "entitlementid",
					ToAttribute = "entitlementid",
					Intersect = true,
					Visible = false,
					Type = JoinOperator.LeftOuter,
					Links = new List<Link>
						{
							new Link
							{
								Name = "product",
								FromAttribute = "productid",
								ToAttribute = "productid",
								Alias = "restrictedproducts",
								Type = JoinOperator.LeftOuter
							}
						}
				};

				AddLinkToFetch(fetch, productLink);

				var productFilter = new Filter
				{
					Type = LogicalOperator.And,
					Filters = new List<Filter>
					{
						new Filter
						{
							Type = LogicalOperator.Or,
							Conditions = new List<Condition>
							{
								new Condition
								{
									EntityName = "restrictedproducts",
									Attribute = "productid",
									Operator = ConditionOperator.Null
								}
							},
							Filters = new List<Filter>
							{
								new Filter
								{
									Type = LogicalOperator.And,
									Conditions = new List<Condition>
									{
										new Condition
										{
											EntityName = "restrictedproducts",
											Attribute = "productid",
											Operator = ConditionOperator.NotNull
										},
										new Condition
										{
											EntityName = "restrictedproducts",
											Attribute = "productid",
											Operator = ConditionOperator.Equal,
											Value = productId
										}
									}
								}
							}
						}
						
					}
				};

				AddFilterToFetch(fetch, productFilter);
			}
		}

		/// <summary>
		/// Assertion of the <see cref="CrmEntityPermissionRight"/> for the given entity type set and the current user. Applies the appropriate contact/account scoped filters to an existing <see cref="Fetch"/> to achieve record level security trimming.
		/// </summary>
		/// <param name="fetch"></param>
		/// <param name="configuration"></param>
		/// <param name="right"></param>
		/// <remarks>Note: EntityPermissionDenied will be set to false if privileges are denied</remarks>
		protected void AddRecordLevelFiltersToFetch(Fetch fetch, ViewConfiguration configuration, CrmEntityPermissionRight right)
		{
			if (!configuration.EnableEntityPermissions)
			{
				return;
			}

			if (!AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled)
			{
				return;
			}

			var serviceContext = Dependencies.GetServiceContext();
			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			var result = crmEntityPermissionProvider.TryApplyRecordLevelFiltersToFetch(serviceContext, right, fetch, Regarding);

            // Apply Content Access Level filtering
            var contentAccessLevelProvider = new ContentAccessLevelProvider();
            contentAccessLevelProvider.TryApplyRecordLevelFiltersToFetch(right, fetch);
            
            // Apply Product filtering
            var productAccessProvider = new ProductAccessProvider();
            productAccessProvider.TryApplyRecordLevelFiltersToFetch(right, fetch);

            EntityPermissionDenied = !result.GlobalPermissionGranted && !result.PermissionGranted;
		}

		private void ApplyTimelineFilterToFetch(OrganizationServiceContext serviceContext, Fetch fetch)
		{
			// Get activities for cases on portal timeline
			if (Configuration.EntityName != "activitypointer" || Regarding?.LogicalName != "incident")
			{
				return;
			}

			bool displayAllActivities;
			var settingValue = serviceContext.GetSiteSettingValueByName(PortalContext.Current.Website, DisplayAllActivitiesOnTimeline);

			if (bool.TryParse(settingValue, out displayAllActivities) && displayAllActivities)
			{
				return;
			}

			AddLinkToFetch(fetch, new Link
			{
				FromAttribute = "activityid",
				ToAttribute = "activityid",
				Type = JoinOperator.Inner,
				Name = "adx_portalcomment"
			});
		}

		protected IEnumerable<Fetch> GenerateFetchForEachRecordLevelFilter(Fetch fetch, ViewConfiguration configuration, CrmEntityPermissionRight right)
		{
			if (!configuration.EnableEntityPermissions)
			{
				return new[] { fetch };
			}

			if (!AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled)
			{
				return new[] { fetch };
			}

			var serviceContext = Dependencies.GetServiceContext();
			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();
			var contentAccessLevelProvider = new ContentAccessLevelProvider();
			var productAccessProvider = new ProductAccessProvider();

			var result = crmEntityPermissionProvider.GenerateFetchForEachRecordLevelFilter(serviceContext, right, fetch, Regarding);

			EntityPermissionDenied = !result.Item1.GlobalPermissionGranted && !result.Item1.PermissionGranted;

			return result.Item2.Select(e =>
			{
				// Apply Content Access Level filtering
				contentAccessLevelProvider.TryApplyRecordLevelFiltersToFetch(right, e);
			
				// Apply Product filtering
				productAccessProvider.TryApplyRecordLevelFiltersToFetch(right, e);

				return e;
			});
		}

		/// <summary>
		/// Try Assert
		/// </summary>
		/// <param name="fetch"></param>
		/// <param name="configuration"></param>
		/// <param name="right"></param>
		protected bool TryAssert(Fetch fetch, ViewConfiguration configuration, CrmEntityPermissionRight right)
		{
			if (!configuration.EnableEntityPermissions)
			{
				return true;
			}

			if (!AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled)
			{
				return true;
			}

			var serviceContext = Dependencies.GetServiceContext();
			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			EntityPermissionDenied = !crmEntityPermissionProvider.TryAssert(serviceContext, right, fetch.Entity.Name);

			return EntityPermissionDenied;
		}

		/// <summary>
		/// Add conditions to the fetch for the search to reduce the records that will be returned to match the query.
		/// </summary>
		/// <param name="fetchEntity"></param>
		/// <param name="configuration"></param>
		/// <param name="search"></param>
		/// <param name="searchableAttributes"></param>
		protected void AddSearchFilterToFetchEntity(FetchEntity fetchEntity, ViewConfiguration configuration, string search, IEnumerable<string> searchableAttributes = null)
		{
			if (!configuration.Search.Enabled || string.IsNullOrWhiteSpace(search))
			{
				return;
			}

			var serviceContext = Dependencies.GetServiceContext();

			var response = (RetrieveEntityResponse)serviceContext.Execute(new RetrieveEntityRequest
			{
				LogicalName = fetchEntity.Name,
				EntityFilters = EntityFilters.Attributes,
			});

			var entityMetadata = response.EntityMetadata;
			var query = search.Trim().Replace("*", "%");

			if (searchableAttributes == null)
			{
				var attributes = fetchEntity.Attributes.Select(a => a.Name).ToArray();

				if (fetchEntity.Orders == null || !fetchEntity.Orders.Any())
				{
					searchableAttributes = attributes;
				}
				else
				{
					// a default savedquery view will contain the name attribute in the orders but not in the attributes collection.
					var orderAttributes = fetchEntity.Orders.Select(o => o.Attribute).ToArray();

					searchableAttributes = attributes.Union(orderAttributes);
				}
			}

			var conditions = searchableAttributes
				.Select(attribute => GetSearchFilterConditionForAttribute(attribute, query, entityMetadata))
				.Where(condition => condition != null)
				.ToList();

			if (!conditions.Any())
			{
				return;
			}

			fetchEntity.Filters.Add(new Filter
			{
				Type = LogicalOperator.And,
				Filters = new List<Filter> { new Filter { Type = LogicalOperator.Or, Conditions = conditions } }
			});
		}

		/// <summary>
		/// Add conditions to the fetch to filter by the current contact and/or current contact's parent customer account
		/// </summary>
		/// <param name="fetchEntity"></param>
		/// <param name="configuration"></param>
		/// <param name="filter"></param>
		protected void AddSelectableFilterToFetchEntity(FetchEntity fetchEntity, ViewConfiguration configuration, string filter)
		{
			var user = Dependencies.GetPortalUser();

			foreach (var condition in GetConditions(fetchEntity).Where(condition => condition.UiType == "contact"))
			{
				condition.Value = user == null ? Guid.Empty : user.Id;
			}

			var userAccount = GetPortalUserAccount(user);

			foreach (var condition in GetConditions(fetchEntity).Where(condition => condition.UiType == "account"))
			{
				condition.Value = userAccount == null ? Guid.Empty : userAccount.Id;
			}

			// Build dictionary of available selectable filters.
			var filters = new Dictionary<string, Action>(StringComparer.InvariantCultureIgnoreCase);

			if (!string.IsNullOrWhiteSpace(configuration.FilterPortalUserAttributeName))
			{
				filters["user"] = () => AddEntityReferenceFilterToFetchEntity(fetchEntity, configuration.FilterPortalUserAttributeName, user);
			}

			if (!string.IsNullOrWhiteSpace(configuration.FilterAccountAttributeName))
			{
				filters["account"] = () => AddEntityReferenceFilterToFetchEntity(fetchEntity, configuration.FilterAccountAttributeName, userAccount);
			}

			// If there are no filters, apply nothing.
			if (filters.Count < 1)
			{
				return;
			}

			// If there is only one filter defined, apply it automatically.
			if (filters.Count == 1)
			{
				filters.Single().Value();

				return;
			}

			Action applyFilter;

			// Try look up the specified filter in the filter dictionary. Apply it if found.
			if (filter != null && filters.TryGetValue(filter, out applyFilter))
			{
				applyFilter();

				return;
			}

			// If the specified filter is not found, try apply the user filter.
			if (filters.TryGetValue("user", out applyFilter))
			{
				applyFilter();

				return;
			}

			// If the user filter is not found, try apply the account filter.
			if (filters.TryGetValue("account", out applyFilter))
			{
				applyFilter();
			}
		}

		/// <summary>
		/// Add a condition to the fetch to filter by the current website.
		/// </summary>
		/// <param name="fetchEntity"></param>
		/// <param name="configuration"></param>
		protected void AddWebsiteFilterToFetchEntity(FetchEntity fetchEntity, ViewConfiguration configuration)
		{
			if (string.IsNullOrWhiteSpace(configuration.FilterWebsiteAttributeName))
			{
				return;
			}

			var website = Dependencies.GetWebsite();

			foreach (var condition in GetConditions(fetchEntity).Where(condition => condition.UiType == "adx_website"))
			{
				condition.Value = website == null ? Guid.Empty : website.Id;
			}

			if (website == null)
			{
				return;
			}

			fetchEntity.Filters.Add(new Filter
			{
				Type = LogicalOperator.And,
				Conditions = new List<Condition>
				{
					new Condition(configuration.FilterWebsiteAttributeName, ConditionOperator.Equal, website.Id),
				}
			});
		}

		/// <summary>
		/// For the given FetchEntity, clear any existing order elements and generate new order elements for the specified sort orders.  Any existing order attributes need to be added to the attributes collection in the event that they do not already exist or attribute value will be null.
		/// </summary>
		protected void AddOrderToFetch(FetchEntity fetchEntity, string sortExpression)
		{
			var sorts = ViewSort.ParseSortExpression(sortExpression).ToList();

			if (!sorts.Any())
			{
				return;
			}

			if (fetchEntity.Orders == null)
			{
				fetchEntity.Orders = new List<Order>();
			}
			else
			{
				// any existing order attributes need to be added to the attributes collection in the event that they do not already exist or attribute value will be null.

				var attributes = fetchEntity.Attributes.Select(a => a.Name).ToArray();
				var orderAttributes = fetchEntity.Orders.Select(o => o.Attribute).ToArray();
				var newAttributes = attributes.Union(orderAttributes).Select(a => new FetchAttribute(a)).ToList();
				fetchEntity.Attributes = newAttributes;

				fetchEntity.Orders.Clear();
			}

			foreach (var sort in sorts)
			{
				fetchEntity.Orders.Add(new Order(sort.Item1, sort.Item2 == ViewSort.Direction.Descending ? OrderType.Descending : OrderType.Ascending));
			}
		}

		/// <summary>
		/// Set the pagination parameters on the given Fetch.
		/// </summary>
		/// <param name="fetch"></param>
		/// <param name="pagerEnabled"></param>
		/// <param name="cookie"></param>
		/// <param name="page"></param>
		/// <param name="count"></param>
		/// <param name="returnTotalRecordCount"></param>
		protected void AddPaginationToFetch(Fetch fetch, bool pagerEnabled, string cookie, int page, int count, bool returnTotalRecordCount)
		{
			if (fetch.Entity.Name == "entitlement") return;

			fetch.PagingCookie = pagerEnabled && cookie != null ? cookie : null;

			fetch.PageNumber = fetch.PageNumber ?? (pagerEnabled ? (int?)page : null);
			
			fetch.PageSize = fetch.PageSize ?? (pagerEnabled ? (int?)count : null);
			
			fetch.ReturnTotalRecordCount = pagerEnabled && returnTotalRecordCount;
		}

		/// <summary>
		/// Apply filter conditions to the FetchXml for the metadata filter query specified.
		/// </summary>
		protected void AddMetadataFilterToFetch(Fetch fetch, ViewConfiguration configuration, string metadataFilter)
		{
			if (configuration == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(metadataFilter))
			{
				return;
			}

			if (!configuration.FilterSettings.Enabled)
			{
				return;
			}

			var filterDefinition = configuration.FilterSettings.Definition;

			if (string.IsNullOrWhiteSpace(filterDefinition))
			{
				return;
			}

			var filterDefinitionFetch = Fetch.FromJson(filterDefinition);
			var query = !string.IsNullOrWhiteSpace(metadataFilter) ? HttpUtility.ParseQueryString(metadataFilter) : new NameValueCollection();

			if (filterDefinitionFetch.Distinct != null)
			{
				fetch.Distinct = filterDefinitionFetch.Distinct.Value;
			}

			var filters = ToFilters(filterDefinitionFetch.Entity.Filters, query).ToList();

			if (filters.Any() && fetch.Entity.Filters == null)
			{
				fetch.Entity.Filters = new List<Filter>();
			}

			foreach (var filter in filters)
			{
				fetch.Entity.Filters.Add(filter);
			}

			var links = ToLinks(filterDefinitionFetch.Entity.Links, query).ToList();

			if (links.Any() && fetch.Entity.Links == null)
			{
				fetch.Entity.Links = new List<Link>();
			}

			foreach (var link in links)
			{
				fetch.Entity.Links.Add(link);
			}
		}

		private static IEnumerable<Filter> ToFilters(IEnumerable<Filter> filters, NameValueCollection query)
		{
			foreach (var filter in filters)
			{
				var id = filter.Extensions.GetExtensionValue("id");
				var selected = query.GetValues(id);

				if (selected != null)
				{
					var conditions = ToConditions(filter.Conditions, selected).ToList();
					var subFilters = ToFilters(filter.Filters, selected).ToList();

					if (conditions.Any() || subFilters.Any())
					{
						yield return new Filter
						{
							Type = filter.Type,
							Conditions = conditions,
							Filters = subFilters,
						};
					}
				}
			}
		}

		private static IEnumerable<Filter> ToFilters(IEnumerable<Filter> filters, string[] selected)
		{
			foreach (var filter in filters)
			{
				var id = filter.Extensions.GetExtensionValue("id");

				if (selected.Contains(id))
				{
					yield return filter;
				}
			}
		}

		private static IEnumerable<Condition> ToConditions(IEnumerable<Condition> conditions, string[] selected)
		{
			foreach (var condition in conditions)
			{
				var id = condition.Extensions.GetExtensionValue("id");
				var type = condition.Extensions.GetExtensionValue("uiinputtype");

				switch (type)
				{
				case "text":
					foreach (var value in selected)
					{
						if (!string.IsNullOrWhiteSpace(value))
						{
							yield return new Condition
							{
								Attribute = condition.Attribute,
								Operator = condition.Operator,
								Value = "{0}%".FormatWith(value.ToFilterLikeString()),
							};
						}
					}
					break;
				case "dynamic":
					// possibly change "dynamic" to something else
					foreach (var value in selected)
					{
						if (!string.IsNullOrWhiteSpace(value))
						{
							yield return new Condition
							{
								Attribute = condition.Attribute,
								Operator = condition.Operator,
								Value = value
							};
						}
					}
					break;
				default:
					if (selected.Contains(id))
					{
						yield return condition;
					}
					break;
				}
			}
		}

		private static IEnumerable<Link> ToLinks(IEnumerable<Link> links, NameValueCollection query)
		{
			foreach (var link in links)
			{
				var id = link.Extensions.GetExtensionValue("id");
				var selected = query.GetValues(id);

				if (selected != null)
				{
					if (link.Intersect.GetValueOrDefault())
					{
						var next = link.Links.FirstOrDefault();

						if (next != null)
						{
							var filters = ToLinkFilters(next.Filters, selected).ToList();

							if (filters.Any())
							{
								yield return new Link
								{
									Name = link.Name,
									FromAttribute = link.FromAttribute,
									ToAttribute = link.ToAttribute,
									Links = new[]
									{
										new Link
										{
											Name = next.Name,
											FromAttribute = next.FromAttribute,
											ToAttribute = next.ToAttribute,
											Filters = filters,
										}
									}
								};
							}
						}
					}
					else
					{
						var filters = ToLinkFilters(link.Filters, selected).ToList();

						if (filters.Any())
						{
							yield return new Link
							{
								Name = link.Name,
								FromAttribute = link.FromAttribute,
								ToAttribute = link.ToAttribute,
								Filters = filters,
							};
						}
					}
				}
			}
		}

		private static IEnumerable<Filter> ToLinkFilters(IEnumerable<Filter> filters, string[] selected)
		{
			foreach (var filter in filters)
			{
				var conditions = ToConditions(filter.Conditions, selected).ToList();

				if (conditions.Any())
				{
					yield return new Filter
					{
						Type = filter.Type,
						Conditions = conditions,
					};
				}
			}
		}

		/// <summary>
		/// Executes the Fetch and returns the resulting records.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="fetch"></param>
		/// <returns><see cref="FetchResult"/></returns>
		protected virtual FetchResult FetchEntities(OrganizationServiceContext serviceContext, Fetch fetch)
		{
			if (fetch == null || EntityPermissionDenied)
			{
				return new FetchResult(Enumerable.Empty<Entity>(), 0, EntityPermissionDenied);
			}

			var entities = new List<Entity>();
			int totalRecordCount;

			while (true)
			{
				var response = (RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest());

				if (!string.IsNullOrEmpty(response.EntityCollection.PagingCookie))
				{
					fetch.PagingCookie = response.EntityCollection.PagingCookie;
				}

				if (fetch.Entity != null && fetch.Entity.Name == "entitlement")
				{
					var entitlementsInvalidForWebChannel =
						response.EntityCollection.Entities.Where(
							e =>
								e.GetAttributeAliasedValue<OptionSetValue>("channel", "restrictedchannels") != null &&
								e.GetAttributeAliasedValue<OptionSetValue>("channel", "restrictedchannels").Value == 3 &&
								e.GetAttributeAliasedValue<decimal>("remainingterms", "restrictedchannels") <= 0);

					var entitlementsValidForWebChannel =
						response.EntityCollection.Entities.Except(entitlementsInvalidForWebChannel, new EntityEqualityComparer()).Distinct(new EntityEqualityComparer()).ToList();

					return new FetchResult(entitlementsValidForWebChannel, entitlementsValidForWebChannel.Count());
				}

				// For the normal page size case, just return directly.
				if (Configuration.PageSize <= Fetch.MaximumPageSize)
				{
					return new FetchResult(response.EntityCollection.Entities, response.EntityCollection.TotalRecordCount);
				}

				entities.AddRange(response.EntityCollection.Entities);
				totalRecordCount = response.EntityCollection.TotalRecordCount;

				// If we haven't fulfilled the requested page size and there are more records, keep querying.
				if (entities.Count < Configuration.PageSize && response.EntityCollection.MoreRecords)
				{
					fetch.PageNumber = fetch.PageNumber.GetValueOrDefault(1) + 1;

					continue;
				}

				break;
			}

			return new FetchResult(entities, totalRecordCount);
		}



		/// <summary>
		/// Retrieves the parent customer account associated with the user contact record.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		protected EntityReference GetPortalUserAccount(EntityReference user)
		{
			if (user == null)
			{
				return null;
			}

			var portalOrganizationService = this.Dependencies.GetRequestContext().HttpContext.GetOrganizationService();
			var contact = portalOrganizationService.RetrieveSingle(
				user.LogicalName,
				new[] { "parentcustomerid" },
				new[] {
					new Condition("statecode", ConditionOperator.Equal, 0),
					new Condition("contactid", ConditionOperator.Equal, user.Id)
				});

			return contact == null ? null : contact.GetAttributeValue<EntityReference>("parentcustomerid");
		}

		/// <summary>
		/// Add attributes to the Fetch Entity
		/// </summary>
		/// <param name="fetchEntity"></param>
		/// <param name="attributes"></param>
		protected static void AddAttributesToFetchEntity(FetchEntity fetchEntity, IEnumerable<string> attributes)
		{
			foreach (var attribute in attributes)
			{
				if (fetchEntity.Attributes.Any(a => a.Name == attribute))
				{
					continue;
				}

				fetchEntity.Attributes.Add(new FetchAttribute(attribute));
			}
		}

		/// <summary>
		/// Add a condition to the Fetch Entity where attribute equals the ID for a given entity reference
		/// </summary>
		/// <param name="fetchEntity"></param>
		/// <param name="attribute"></param>
		/// <param name="entity"></param>
		protected static void AddEntityReferenceFilterToFetchEntity(FetchEntity fetchEntity, string attribute, EntityReference entity)
		{
			fetchEntity.Filters.Add(new Filter
			{
				Type = LogicalOperator.And,
				Conditions = new List<Condition>
				{
					new Condition(attribute, ConditionOperator.Equal, entity == null ? Guid.Empty : entity.Id)
				}
			});
		}

		/// <summary>
		/// Gets the conditions defined on the Fetch Entity
		/// </summary>
		/// <param name="fetchEntity"></param>
		/// <returns></returns>
		protected static IEnumerable<Condition> GetConditions(FetchEntity fetchEntity)
		{
			if (fetchEntity == null)
			{
				yield break;
			}

			if (fetchEntity.Filters != null)
			{
				foreach (var condition in fetchEntity.Filters.SelectMany(GetConditions))
				{
					yield return condition;
				}
			}

			if (fetchEntity.Links != null)
			{
				foreach (var condition in fetchEntity.Links.SelectMany(GetConditions))
				{
					yield return condition;
				}
			}
		}

		/// <summary>
		/// Gets the conditions defined on a filter
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		protected static IEnumerable<Condition> GetConditions(Filter filter)
		{
			if (filter == null)
			{
				yield break;
			}

			if (filter.Conditions != null)
			{
				foreach (var condition in filter.Conditions)
				{
					yield return condition;
				}
			}

			if (filter.Filters != null)
			{
				foreach (var condition in filter.Filters.SelectMany(GetConditions))
				{
					yield return condition;
				}
			}
		}

		/// <summary>
		/// Gets the conditions defined on a link
		/// </summary>
		/// <param name="link"></param>
		/// <returns></returns>
		protected static IEnumerable<Condition> GetConditions(Link link)
		{
			if (link == null)
			{
				yield break;
			}

			if (link.Filters != null)
			{
				foreach (var condition in link.Filters.SelectMany(GetConditions))
				{
					yield return condition;
				}
			}

			if (link.Links != null)
			{
				foreach (var condition in link.Links.SelectMany(GetConditions))
				{
					yield return condition;
				}
			}
		}

		/// <summary>
		/// Creates the condition for the search query
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="query"></param>
		/// <param name="entityMetadata"></param>
		/// <returns></returns>
		protected static Condition GetSearchFilterConditionForAttribute(string attribute, string query, EntityMetadata entityMetadata)
		{
			var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attribute);

			if (attributeMetadata == null)
			{
				return null;
			}

			switch (attributeMetadata.AttributeType)
			{
				case AttributeTypeCode.String:
					return new Condition(attribute, ConditionOperator.Like, "{0}%".FormatWith(query));

				case AttributeTypeCode.Lookup:
				case AttributeTypeCode.Customer:
				case AttributeTypeCode.Picklist:
				case AttributeTypeCode.State:
				case AttributeTypeCode.Status:
				case AttributeTypeCode.Owner:
					return new Condition("{0}name".FormatWith(attribute), ConditionOperator.Like, "{0}%".FormatWith(query));

				case AttributeTypeCode.BigInt:
					long parsedLong;
					return long.TryParse(query, out parsedLong)
						? new Condition(attribute, ConditionOperator.Equal, parsedLong)
						: null;

				case AttributeTypeCode.Integer:
					int parsedInt;
					return int.TryParse(query, out parsedInt)
						? new Condition(attribute, ConditionOperator.Equal, parsedInt)
						: null;

				case AttributeTypeCode.Double:
					double parsedDouble;
					return double.TryParse(query, out parsedDouble)
						? new Condition(attribute, ConditionOperator.Equal, parsedDouble)
						: null;

				case AttributeTypeCode.Decimal:
				case AttributeTypeCode.Money:
					decimal parsedDecimal;
					return decimal.TryParse(query, out parsedDecimal)
						? new Condition(attribute, ConditionOperator.Equal, parsedDecimal)
						: null;

				case AttributeTypeCode.DateTime:
					DateTime parsedDate;
					return DateTime.TryParse(query, out parsedDate)
						? new Condition(attribute, ConditionOperator.On, parsedDate.ToString("yyyy-MM-dd"))
						: null;

				default:
					return null;
			}
		}

		/// <summary>
		/// Result returned by executing a fetch expression
		/// </summary>
		public class FetchResult
		{
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="records">Collection of <see cref="Entity"/> records returned by the execution of a fetch expression</param>
			/// <param name="totalRecordCount">Total number of records</param>
			/// <param name="entityPermissionDenied">Indicates if access to the records was denied or granted.</param>
			public FetchResult(IEnumerable<Entity> records, int totalRecordCount = 0, bool entityPermissionDenied = false)
			{
				Records = records;
				TotalRecordCount = totalRecordCount;
				EntityPermissionDenied = entityPermissionDenied;
			}

			public FetchResult(IEnumerable<Entity> records, bool moreRecords, bool entityPermissionDenied = false)
			{
				Records = records;
				TotalRecordCount = -1;
				EntityPermissionDenied = entityPermissionDenied;
				MoreRecords = moreRecords;
			}

			public bool? MoreRecords { get; private set; }

			/// <summary>
			/// Collection of <see cref="Entity"/> records
			/// </summary>
			public IEnumerable<Entity> Records { get; private set; }

			/// <summary>
			/// The total number of records
			/// </summary>
			public int TotalRecordCount { get; private set; }

			/// <summary>
			/// Indicates if the user does not have permission to read the entity records.
			/// </summary>
			public bool EntityPermissionDenied { get; private set; }
		}

		/// <summary>
		/// Add the necessary filter condition to filter by related record.
		/// </summary>
		/// <param name="fetch"><see cref="Fetch"/></param>
		/// <param name="filterRelationshipName">Schema name of the relationship between the fetch entity and the entity specified by the filterEntityName.</param>
		/// <param name="filterEntityName">Logical name of the entity to filter on.</param>
		/// <param name="filterValue">Uniqueidentifier of the record to filter.</param>
		protected void AddRelatedRecordFilterToFetch(Fetch fetch, string filterRelationshipName, string filterEntityName, Guid filterValue)
		{
			if (fetch == null || fetch.Entity == null)
			{
				return;
			}

			var entityName = fetch.Entity.Name;

			if (string.IsNullOrWhiteSpace(filterRelationshipName) || string.IsNullOrWhiteSpace(filterEntityName) || filterValue == Guid.Empty)
			{
				return;
			}

			var entityRequest = new RetrieveEntityRequest
			{
				RetrieveAsIfPublished = false,
				LogicalName = entityName,
				EntityFilters = EntityFilters.Relationships
			};

			var serviceContext = Dependencies.GetServiceContext();
			var entityResponse = serviceContext.Execute(entityRequest) as RetrieveEntityResponse;

			if (entityResponse == null)
			{
				throw new ApplicationException(string.Format("RetrieveEntityRequest failed for lookup target entity type {0}", entityName));
			}

			var relationshipManyToOne = entityResponse.EntityMetadata.ManyToOneRelationships.FirstOrDefault(r => r.SchemaName == filterRelationshipName);

			if (relationshipManyToOne != null)
			{
				var attribute = relationshipManyToOne.ReferencingAttribute;

				var filter = new Filter
				{
					Type = LogicalOperator.And,
					Conditions = new List<Condition>
					{
						new Condition
						{
							Attribute = attribute,
							Operator = ConditionOperator.Equal,
							Value = filterValue
						}
					}
				};

				AddFilterToFetch(fetch, filter);
			}
			else
			{
				throw new ApplicationException(string.Format("RetrieveRelationshipRequest failed for lookup filter relationship name {0}", filterRelationshipName));
			}
		}

		private void AddFilterToFetch(Fetch fetch, Filter filter)
		{
			if (fetch.Entity.Filters == null)
			{
				fetch.Entity.Filters = new List<Filter>
				{
					new Filter { Type = LogicalOperator.And, Filters = new List<Filter> { filter } }
				};
			}
			else
			{
				var rootAndFilter = fetch.Entity.Filters.FirstOrDefault(f => f.Type == LogicalOperator.And);

				if (rootAndFilter != null)
				{
					if (filter.Conditions != null && filter.Conditions.Any())
					{
						if (rootAndFilter.Conditions == null)
						{
							rootAndFilter.Conditions = filter.Conditions;
						}
						else
						{
							foreach (var condition in filter.Conditions)
							{
								rootAndFilter.Conditions.Add(condition);
							}
						}
					}

					if (filter.Filters != null && filter.Filters.Any())
					{
						if (rootAndFilter.Filters == null)
						{
							rootAndFilter.Filters = filter.Filters;
						}
						else
						{
							foreach (var f in filter.Filters)
							{
								rootAndFilter.Filters.Add(f);
							}
						}
					}
				}
				else
				{
					fetch.Entity.Filters.Add(filter);
				}
			}
		}
		private static void AddLinkToFetch(Fetch fetch, Link link)
		{
			if (fetch.Entity.Links == null)
			{
				fetch.Entity.Links = new List<Link> { link };
			}
			else
			{
				fetch.Entity.Links.Add(link);
			}
		}

		private static readonly IDictionary<string, IViewSpecialCase> SpecialCases = new Dictionary<string, IViewSpecialCase>(StringComparer.InvariantCulture)
		{
			{ "product", new OpportunityProductExistingProductLookupViewSpecialCase() },
			{ "uom", new OpportunityProductUnitLookupViewSpecialCase() },
			{ "adx_portallanguage", new PortalLanguageViewSpecialCase() }
		};
	}
}
