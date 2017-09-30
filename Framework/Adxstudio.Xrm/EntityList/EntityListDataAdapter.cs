/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Action = System.Action;
using Adxstudio.Xrm.ContentAccess;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Web;

namespace Adxstudio.Xrm.EntityList
{
	public abstract class EntityListDataAdapter
	{
		protected EntityListDataAdapter(EntityReference entityList, EntityReference view, IDataAdapterDependencies dependencies)
		{
			if (entityList == null) throw new ArgumentNullException("entityList");
			if (view == null) throw new ArgumentNullException("view");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			EntityList = entityList;
			View = view;
			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected EntityReference EntityList { get; private set; }

		protected EntityReference View { get; private set; }

		protected bool EntityPermissionDenied { get; set; }

		protected virtual void AssertEntityListAccess()
		{
			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();
			var securityProvider = Dependencies.GetSecurityProvider();

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_webpage")
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("adx_websiteid", ConditionOperator.Equal, website.Id),
								new Condition("adx_entitylist", ConditionOperator.Equal, EntityList.Id)
							}
						}
					}
				}
			};

			var webPages = serviceContext.RetrieveMultiple(fetch).Entities;

			if (!webPages.Any(e => securityProvider.TryAssert(serviceContext, e, CrmEntityRight.Read)))
			{
				throw new SecurityException("Read permission to this entity list is denied ({0}:{1}).".FormatWith(EntityList.LogicalName, EntityList.Id));
			}
		}

		protected void AddPermissionFilterToFetch(Fetch fetch, EntityListSettings settings, OrganizationServiceContext serviceContext, CrmEntityPermissionRight right)
		{
			if (!settings.EntityPermissionsEnabled)
			{
				return;
			}

			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			var result = crmEntityPermissionProvider.TryApplyRecordLevelFiltersToFetch(serviceContext, right, fetch);

			// Apply Content Access Level filtering
			var contentAccessLevelProvider = new ContentAccessLevelProvider();
			contentAccessLevelProvider.TryApplyRecordLevelFiltersToFetch(right, fetch);

			// Apply Product filtering
			var productAccessProvider = new ProductAccessProvider();
			productAccessProvider.TryApplyRecordLevelFiltersToFetch(right, fetch);

			EntityPermissionDenied = !result.GlobalPermissionGranted && !result.PermissionGranted;
		}

		protected void AddSearchFilterToFetchEntity(FetchEntity fetchEntity, EntityListSettings settings, string search, IEnumerable<string> searchableAttributes)
		{
			if (!settings.SearchEnabled || string.IsNullOrWhiteSpace(search))
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
				Type = LogicalOperator.Or,
				Conditions = conditions
			});
		}

		protected void AddSelectableFilterToFetchEntity(FetchEntity fetchEntity, EntityListSettings settings, string filter)
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

			if (!string.IsNullOrWhiteSpace(settings.FilterPortalUserFieldName))
			{
				filters["user"] = () => AddEntityReferenceFilterToFetchEntity(fetchEntity, settings.FilterPortalUserFieldName, user);
			}

			if (!string.IsNullOrWhiteSpace(settings.FilterAccountFieldName))
			{
				filters["account"] = () => AddEntityReferenceFilterToFetchEntity(fetchEntity, settings.FilterAccountFieldName, userAccount);
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

				return;
			}
		}

		protected void AddWebsiteFilterToFetchEntity(FetchEntity fetchEntity, EntityListSettings settings)
		{
			if (string.IsNullOrWhiteSpace(settings.FilterWebsiteFieldName))
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
					new Condition(settings.FilterWebsiteFieldName, ConditionOperator.Equal, website.Id),
				}
			});
		}

		protected virtual IEnumerable<Entity> FetchEntities(OrganizationServiceContext serviceContext, Fetch fetch)
		{
			if (fetch == null || this.EntityPermissionDenied)
			{
				return Enumerable.Empty<Entity>();
			}

			var entityResult = new List<Entity>();
			fetch.PageNumber = 1;

			while (true)
			{
				var response = (RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest());
				entityResult.AddRange(response.EntityCollection.Entities);
				

				if (!response.EntityCollection.MoreRecords || string.IsNullOrEmpty(response.EntityCollection.PagingCookie))
				{
					break;
				}

				fetch.PageNumber++;
				fetch.PagingCookie = response.EntityCollection.PagingCookie;
			}
			return entityResult;
		}

		protected EntityReference GetPortalUserAccount(EntityReference user)
		{
			if (user == null)
			{
				return null;
			}

			var portalOrganizationService = Dependencies.GetRequestContext().HttpContext.GetOrganizationService();

			var contact = portalOrganizationService.RetrieveSingle(
				user.LogicalName,
				new[] { "parentcustomerid" },
				new[] {
					new Condition("statecode", ConditionOperator.Equal, 0),
					new Condition("contactid", ConditionOperator.Equal, user.Id)
				});

			return contact == null ? null : contact.GetAttributeValue<EntityReference>("parentcustomerid");
		}

		protected static void AddAttributesToFetchEntity(FetchEntity fetchEntity, EntityListSettings settings)
		{
			foreach (var attribute in settings.DefinedAttributes)
			{
				if (fetchEntity.Attributes.Any(a => a.Name == attribute))
				{
					continue;
				}

				fetchEntity.Attributes.Add(new FetchAttribute(attribute));
			}
		}

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

		protected static bool TryGetEntityList(OrganizationServiceContext serviceContext, EntityReference entityListReference, out Entity entityList)
		{
			entityList = serviceContext.RetrieveSingle(
				"adx_entitylist",
				FetchAttribute.All,
				new Condition("adx_entitylistid", ConditionOperator.Equal, entityListReference.Id));
			
			return entityList != null;
		}

		protected static bool TryGetView(OrganizationServiceContext serviceContext, Entity entityList, EntityReference viewReference, out Entity view)
		{
			Guid? viewId;

			if (!TryGetViewId(entityList, viewReference, out viewId) || viewId == null)
			{
				view = null;

				return false;
			}

			view = serviceContext.CreateQuery("savedquery")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("savedqueryid") == viewId.Value
					&& e.GetAttributeValue<string>("returnedtypecode") == entityList.GetAttributeValue<string>("adx_entityname"));
			
			return view != null;
		}

		protected static bool TryGetViewId(Entity entityList, EntityReference viewReference, out Guid? viewId)
		{
			// First, try get the view from the newer view configuration JSON.
			var viewMetadataJson = entityList.GetAttributeValue<string>("adx_views");

			if (!string.IsNullOrWhiteSpace(viewMetadataJson))
			{
				try
				{
					var viewMetadata = ViewMetadata.Parse(viewMetadataJson);

					var view = viewMetadata.Views.FirstOrDefault(e => e.ViewId == viewReference.Id);

					if (view != null)
					{
						viewId = view.ViewId;

						return true;
					}
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error parsing adx_views JSON: {0}", e.ToString()));
				}
			}

			// Fall back to the legacy comma-delimited list of IDs.
			var viewIds = (entityList.GetAttributeValue<string>("adx_view") ?? string.Empty)
				.Split(',')
				.Select(s =>
				{
					Guid id;

					return Guid.TryParse(s, out id) ? new Guid?(id) : null;
				})
				.Where(id => id != null);

			viewId = viewIds.FirstOrDefault(id => id == viewReference.Id);

			return viewId != null;
		}

		protected class EntityListSettings
		{
			private static readonly string[] AttributeMappings =
			{
				"adx_calendar_startdatefieldname",
				"adx_calendar_enddatefieldname",
				"adx_calendar_summaryfieldname",
				"adx_calendar_descriptionfieldname",
				"adx_calendar_organizerfieldname",
				"adx_calendar_locationfieldname",
				"adx_calendar_alldayfieldname"
			};

			private readonly IDictionary<string, string> _attributeMap;

			public EntityListSettings(Entity entityList)
			{
				if (entityList == null) throw new ArgumentNullException("entityList");

				_attributeMap = AttributeMappings.ToDictionary(a => a, entityList.GetAttributeValue<string>);

				CalendarViewEnabled = entityList.GetAttributeValue<bool?>("adx_calendar_enabled").GetValueOrDefault(false);
				TimeZoneDisplayMode = GetTimeZoneDisplayMode(entityList.GetAttributeValue<OptionSetValue>("adx_calendar_timezonemode"));
				DisplayTimeZone = entityList.GetAttributeValue<int?>("adx_calendar_timezone");
				FilterAccountFieldName = entityList.GetAttributeValue<string>("adx_filteraccount");
				FilterPortalUserFieldName = entityList.GetAttributeValue<string>("adx_filterportaluser");
				FilterWebsiteFieldName = entityList.GetAttributeValue<string>("adx_filterwebsite");
				IdQueryStringParameterName = entityList.GetAttributeValue<string>("adx_idquerystringparametername");
				SearchEnabled = entityList.GetAttributeValue<bool?>("adx_searchenabled").GetValueOrDefault(false);
				WebPageForDetailsView = entityList.GetAttributeValue<EntityReference>("adx_webpagefordetailsview");
				EntityPermissionsEnabled = entityList.GetAttributeValue<bool?>("adx_entitypermissionsenabled").GetValueOrDefault(false);
			}

			public bool CalendarViewEnabled { get; private set; }

			public IEnumerable<string> DefinedAttributes
			{
				get { return _attributeMap.Where(a => !string.IsNullOrWhiteSpace(a.Value)).Select(a => a.Value); }
			}

			public string DescriptionFieldName
			{
				get { return _attributeMap["adx_calendar_descriptionfieldname"]; }
			}

			public int? DisplayTimeZone { get; private set; }

			public string EndDateFieldName
			{
				get { return _attributeMap["adx_calendar_enddatefieldname"]; }
			}

			public bool EntityPermissionsEnabled { get; private set; }

			public string FilterAccountFieldName { get; private set; }

			public string FilterPortalUserFieldName { get; private set; }

			public string FilterWebsiteFieldName { get; private set; }

			public string IdQueryStringParameterName { get; private set; }

			public string IsAllDayFieldName
			{
				get { return _attributeMap["adx_calendar_alldayfieldname"]; }
			}

			public string LocationFieldName
			{
				get { return _attributeMap["adx_calendar_locationfieldname"]; }
			}

			public string OrganizerFieldName
			{
				get { return _attributeMap["adx_calendar_organizerfieldname"]; }
			}

			public bool SearchEnabled { get; private set; }

			public string StartDateFieldName
			{
				get { return _attributeMap["adx_calendar_startdatefieldname"]; }
			}

			public string SummaryFieldName
			{
				get { return _attributeMap["adx_calendar_summaryfieldname"]; }
			}

			public EntityListTimeZoneDisplayMode TimeZoneDisplayMode { get; private set; }

			public EntityReference WebPageForDetailsView { get; private set; }
			
			private static EntityListTimeZoneDisplayMode GetTimeZoneDisplayMode(OptionSetValue option)
			{
				if (option == null || !Enum.IsDefined(typeof(EntityListTimeZoneDisplayMode), option.Value))
				{
					return EntityListTimeZoneDisplayMode.UserLocalTimeZone;
				}

				return (EntityListTimeZoneDisplayMode)option.Value;
			}
		}
	}
}
