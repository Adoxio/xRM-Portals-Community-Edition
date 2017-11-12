/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Globalization;
	using System.Linq;
	using System.Web;
	using Adxstudio.Xrm.ContentAccess;
	using Adxstudio.Xrm.Globalization;
	using Adxstudio.Xrm.Metadata;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Security;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Microsoft.Xrm.Sdk.Query;

	internal class FilterOptionGroup
	{
		public string Id { get; set; }
		public string Label { get; set; }
		public string Order { get; set; }
		public string SelectionMode { get; set; }
		public IEnumerable<FilterOption> Options { get; set; }

		public static IEnumerable<FilterOptionGroup> FromFetch(OrganizationServiceContext context, IPortalContext portalContext,
			EntityMetadata entityMetadata, Fetch fetch, NameValueCollection query, int crmLcid = 0,
			IDictionary<string, string> overrideColumnNames = null)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (portalContext == null) throw new ArgumentNullException(nameof(portalContext));
			if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
			if (fetch == null) throw new ArgumentNullException("fetch");

			// Entity list only supports CRM languages, so use the CrmLcid rather than the potentially custom language Lcid.
			if (crmLcid == 0)
			{
				crmLcid = HttpContext.Current?.GetCrmLcid() ?? CultureInfo.CurrentCulture.LCID;
			}

			overrideColumnNames = overrideColumnNames ?? new Dictionary<string, string>();

			return ToFilterOptionGroups(context, portalContext, entityMetadata, fetch.Entity.Filters, fetch.Entity.Links,
				query ?? new NameValueCollection(), crmLcid, overrideColumnNames);
		}

		internal static IEnumerable<FilterOptionGroup> ToFilterOptionGroups(OrganizationServiceContext context, IPortalContext portalContext, EntityMetadata entityMetadata,
			IEnumerable<Filter> filters, IEnumerable<Link> links, NameValueCollection query, SavedQueryView queryView, IViewConfiguration viewConfiguration, int languageCode)
		{
			return filters.Select(f => ToFilterOptionGroup(entityMetadata, f, query, queryView, viewConfiguration, languageCode))
				.Union(links.Select(l => ToFilterOptionGroup(context, portalContext, l, query, languageCode)))
				.OrderBy(f => f.Order)
				.ToList();
		}

		private static IEnumerable<FilterOptionGroup> ToFilterOptionGroups(OrganizationServiceContext context, IPortalContext portalContext,
			EntityMetadata entityMetadata, IEnumerable<Filter> filters, IEnumerable<Link> links,
			NameValueCollection query, int languageCode, IDictionary<string, string> overrideColumnNames)
		{
			return filters.Select(f => ToFilterOptionGroup(entityMetadata, f, query, languageCode, overrideColumnNames))
				.Union(links.Select(l => ToFilterOptionGroup(context, portalContext, l, query, languageCode)))
				.OrderBy(f => f.Order)
				.ToList();
		}

		private static FilterOptionGroup ToFilterOptionGroup(OrganizationServiceContext context, IPortalContext portalContext, Link link, 
			NameValueCollection query, int languageCode)
		{
			var id = link.Extensions.GetExtensionValue("id");
			var selected = query.GetValues(id) ?? new string[] { };

			return new FilterOptionGroup
			{
				Id = link.Extensions.GetExtensionValue("id"),
				Order = link.Extensions.GetExtensionValue("uiorder"),
				SelectionMode = link.Extensions.GetExtensionValue("uiselectionmode"),
				Label = ToFilterOptionGroupLabel(context, link, languageCode),
				Options = ToFilterOptions(context, portalContext, link, selected, languageCode)
			};
		}

		private static FilterOptionGroup ToFilterOptionGroup(EntityMetadata entityMetadata, Filter filter,
			NameValueCollection query, SavedQueryView currentView, IViewConfiguration viewConfiguration, int languageCode)
		{
			var id = filter.Extensions.GetExtensionValue("id");
			var selected = query.GetValues(id) ?? new string[] { };

			return new FilterOptionGroup
			{
				Id = id,
				Order = filter.Extensions.GetExtensionValue("uiorder"),
				SelectionMode = filter.Extensions.GetExtensionValue("uiselectionmode"),
				Label = ToFilterOptionGroupLabel(currentView.EntityMetadata, filter, viewConfiguration, languageCode),
				Options = GetFilterOptions(entityMetadata, filter, selected, languageCode),
			};
		}

		private static FilterOptionGroup ToFilterOptionGroup(OrganizationServiceContext context, Link link,
			NameValueCollection query, int languageCode)
		{
			var id = link.Extensions.GetExtensionValue("id");
			var selected = query.GetValues(id) ?? new string[] { };

			return new FilterOptionGroup
			{
				Id = link.Extensions.GetExtensionValue("id"),
				Order = link.Extensions.GetExtensionValue("uiorder"),
				SelectionMode = link.Extensions.GetExtensionValue("uiselectionmode"),
				Label = ToFilterOptionGroupLabel(context, link, languageCode),
				Options = ToFilterOptions(link, selected, languageCode),
			};
		}

		private static FilterOptionGroup ToFilterOptionGroup(EntityMetadata entityMetadata, Filter filter,
			NameValueCollection query, int languageCode, IDictionary<string, string> overrideColumnNames)
		{
			var id = filter.Extensions.GetExtensionValue("id");
			var selected = query.GetValues(id) ?? new string[] { };

			return new FilterOptionGroup
			{
				Id = id,
				Order = filter.Extensions.GetExtensionValue("uiorder"),
				SelectionMode = filter.Extensions.GetExtensionValue("uiselectionmode"),
				Label = ToFilterOptionGroupLabel(entityMetadata, filter, languageCode, overrideColumnNames),
				Options = GetFilterOptions(entityMetadata, filter, selected, languageCode),
			};
		}

		private static IEnumerable<FilterOption> ToFilterOptions(OrganizationServiceContext context, IPortalContext portalContext, Link link, string[] selected, int languageCode)
		{
			// check for N:N relationship

			if (link.Intersect.GetValueOrDefault())
			{
				var next = link.Links.FirstOrDefault();

				if (next != null)
				{
					return ToFilterOptions(context, portalContext, next, selected, languageCode);
				}
			}

			// this is a 1:N relationship

			var filter = link.Filters.FirstOrDefault();

			if (filter != null && filter.Conditions != null && filter.Conditions.Any())
			{
				var condition = filter.Conditions.First();
				if (condition.Extensions.GetExtensionValue("uiinputtype") == "dynamic")
				{
					var view = condition.Extensions.GetExtensionValue("view") ?? string.Empty;
					view = view.Replace("{", string.Empty).Replace("}", string.Empty);

					Guid id;
					if (Guid.TryParse(view, out id))
					{
						var labelColumn = condition.Extensions.GetExtensionValue("labelcolumn");

						var sq = context.CreateQuery("savedquery").FirstOrDefault(q => q.GetAttributeValue<Guid>("savedqueryid") == id);
						if (sq != null)
						{
							var fetch = Fetch.Parse(sq.GetAttributeValue<string>("fetchxml"));

							if (!AddPermissionFilterToFetch(fetch, context, CrmEntityPermissionRight.Read))
							{
								return new FilterOption[] { };
							}

							var user = portalContext.User;

							var filterRelationshipName = condition.Extensions.GetExtensionValue("filterrelationship");
							var filterEntityName = condition.Extensions.GetExtensionValue("filterentity");

							if (user != null || string.IsNullOrEmpty(filterEntityName))
							{
								if (!string.IsNullOrEmpty(filterEntityName) && user != null && filterEntityName == user.LogicalName)
								{
									AddRelatedRecordFilterToFetch(context, fetch, filterRelationshipName, filterEntityName, user.Id);
								}
								else if (!string.IsNullOrEmpty(filterEntityName) && !string.IsNullOrEmpty(filterRelationshipName))
								{
                                    ADXTrace.Instance.TraceInfo(TraceCategory.Application,
	                                    $"Could not filter option list: user is not of type {filterEntityName}");
								}

								var response = (RetrieveMultipleResponse)context.Execute(fetch.ToRetrieveMultipleRequest());
								return response.EntityCollection.Entities.Select(
									r => ToFilterOption(context.GetEntityMetadata(link.Name), r, labelColumn, selected));
							}

                            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(
								"Could not add filter condition: dynamic filter condition is not valid", filterEntityName));
						}
					}
					return new FilterOption[] { };
				}
				return filter.Conditions.Select(c => ToFilterOption(null, c, selected, languageCode));
			}

			return new FilterOption[] { };
		}

		private static bool AddPermissionFilterToFetch(Fetch fetch,  OrganizationServiceContext serviceContext, CrmEntityPermissionRight right)
		{
			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			var result = crmEntityPermissionProvider.TryApplyRecordLevelFiltersToFetch(serviceContext, right, fetch);

			// Apply Content Access Level filtering
			var contentAccessLevelProvider = new ContentAccessLevelProvider();
			contentAccessLevelProvider.TryApplyRecordLevelFiltersToFetch(right, fetch);

			// Apply Product filtering
			var productAccessProvider = new ProductAccessProvider();
			productAccessProvider.TryApplyRecordLevelFiltersToFetch(right, fetch);

			return result.GlobalPermissionGranted && result.PermissionGranted;
		}

		private static IEnumerable<FilterOption> ToFilterOptions(Link link, string[] selected, int languageCode)
		{
			// check for N:N relationship

			if (link.Intersect.GetValueOrDefault())
			{
				var next = link.Links.FirstOrDefault();

				if (next != null)
				{
					return ToFilterOptions(next, selected, languageCode);
				}
			}

			// this is a 1:N relationship

			var filter = link.Filters.FirstOrDefault();

			return filter != null
				? filter.Conditions.Select(c => ToFilterOption(null, c, selected, languageCode))
				: new FilterOption[] { };
		}

		private static IEnumerable<FilterOption> ToFilterOptions(EntityMetadata entityMetadata, Filter filter, string[] selected, int languageCode)
		{
			var attributeLogicalName = filter.Extensions.GetExtensionValue("attribute");
			var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeLogicalName);

			if (filter.Conditions != null && filter.Conditions.Any())
			{
				return filter.Conditions.Select(c => ToFilterOption(attributeMetadata, c, selected, languageCode));
			}

			if (filter.Filters != null && filter.Filters.Any())
			{
				return filter.Filters.Select(f => ToFilterOption(attributeMetadata, f, selected, languageCode));
			}

			return new FilterOption[] { };
		}

		private static IEnumerable<FilterOption> GetFilterOptions(EntityMetadata entityMetadata, Filter filter, string[] selected, int languageCode)
		{
			var attributeLogicalName = filter.Extensions.GetExtensionValue("attribute");
			var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeLogicalName);

			if (filter.Conditions != null && filter.Conditions.Any())
			{
				if (filter.Conditions.First().Extensions.GetExtensionValue("uiinputtype") == "dynamic")
				{
					var picklistAttributeMetadata = attributeMetadata as PicklistAttributeMetadata;
					if (picklistAttributeMetadata != null)
					{
						return
							picklistAttributeMetadata.OptionSet.Options.Select(o => ToFilterOption(o, selected));
					}
				}

				return filter.Conditions.Select(c => ToFilterOption(attributeMetadata, c, selected, languageCode));
			}

			if (filter.Filters != null && filter.Filters.Any())
			{
				return filter.Filters.Select(f => ToFilterOption(attributeMetadata, f, selected, languageCode));
			}
			return new FilterOption[] { };
		}

		private static FilterOption ToFilterOption(AttributeMetadata attributeMetadata, Condition condition, string[] selected, int languageCode)
		{
			// a scalar option

			var label = ToLabel(attributeMetadata as EnumAttributeMetadata, condition, languageCode)
				?? condition.UiName
					?? condition.Value as string;

			var id = condition.Extensions.GetExtensionValue("id");

			return new FilterOption
			{
				Id = id,
				Type = condition.Extensions.GetExtensionValue("uiinputtype"),
				Label = label,
				Checked = selected.Contains(id),
				Text = string.Join(",", selected),
			};
		}

		private static FilterOption ToFilterOption(AttributeMetadata attributeMetadata, Filter filter,
			string[] selected, int languageCode)
		{
			var label = ToLabel(attributeMetadata as EnumAttributeMetadata, filter, languageCode);
			var id = filter.Extensions.GetExtensionValue("id");

			return new FilterOption
			{
				Id = id,
				Type = filter.Extensions.GetExtensionValue("uiinputtype"),
				Label = label,
				Checked = selected.Contains(id),
				Text = string.Join(",", selected),
			};
		}

		private static FilterOption ToFilterOption(OptionMetadata optionMetadata, IEnumerable<string> selected)
		{
			var id = string.Empty + optionMetadata.Value;
			return new FilterOption
			{
				Id = id,
				Type = "dynamic",
				Label = optionMetadata.Label.GetLocalizedLabelString(),
				Checked = selected.Contains(id)
			};
		}

		private static FilterOption ToFilterOption(EntityMetadata entityMetadata, Entity entity, string labelColumn, IEnumerable<string> selected)
		{
			if (string.IsNullOrEmpty(labelColumn))
			{
				labelColumn = entityMetadata.PrimaryNameAttribute;
			}

			var id = string.Format("{{{0}}}", entity.Id);
			return new FilterOption
			{
				Id = id,
				Type = "dynamic",
				Label = entity.GetAttributeValueOrDefault(labelColumn, string.Empty),
				Checked = selected.Contains(id)
			};
		}

		private static string ToLabel(EnumAttributeMetadata attributeMetadata, Filter filter, int languageCode)
		{
			var uiname = filter.Extensions.GetExtensionValue("uiname");

			if (!string.IsNullOrWhiteSpace(uiname)) { return uiname; }

			var labels = filter.Conditions.Select(c => ToLabel(attributeMetadata, c, languageCode) ?? c.UiName ?? c.Value as string).ToArray();

			return string.Join(" - ", labels);
		}

		private static string ToLabel(EnumAttributeMetadata attributeMetadata, Condition condition, int languageCode)
		{
			if (condition == null) return null;
			if (attributeMetadata == null) return null;

			var value = Convert.ToInt32(condition.Value);

			var option = attributeMetadata.OptionSet.Options.FirstOrDefault(o => o.Value == value);

			if (option == null) return value.ToString(CultureInfo.InvariantCulture);

			var localizedLabel = option.Label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == languageCode);

			return localizedLabel == null
				? option.Label.GetLocalizedLabelString()
				: localizedLabel.Label;
		}

		protected static void AddRelatedRecordFilterToFetch(OrganizationServiceContext serviceContext, Fetch fetch, string filterRelationshipName, string filterEntityName, Guid filterValue)
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

			var entityResponse = serviceContext.Execute(entityRequest) as RetrieveEntityResponse;

			if (entityResponse == null)
			{
				throw new ApplicationException("RetrieveEntityRequest failed for lookup target entity type {0}".FormatWith(entityName));
			}

			var relationshipManyToOne = entityResponse.EntityMetadata.ManyToOneRelationships.FirstOrDefault(r => r.SchemaName == filterRelationshipName);

			if (relationshipManyToOne != null)
			{
				var attribute = relationshipManyToOne.ReferencedEntity == entityName
					? relationshipManyToOne.ReferencedAttribute
					: relationshipManyToOne.ReferencingAttribute;

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

		private static void AddFilterToFetch(Fetch fetch, Filter filter)
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
					rootAndFilter.Conditions.Add(filter.Conditions.First());
				}
				else
				{
					fetch.Entity.Filters.Add(new Filter
					{
						Type = LogicalOperator.And,
						Filters = new List<Filter> { filter }
					});
				}
			}
		}

		private static string ToFilterOptionGroupLabel(OrganizationServiceContext context, Link link, int languageCode)
		{
			// check for N:N relationship

			if (link.Intersect.GetValueOrDefault())
			{
				var next = link.Links.FirstOrDefault();

				if (next != null)
				{
					return ToFilterOptionGroupLabel(context, next, languageCode);
				}
			}

			// get filter group label from fetch

			var uiname = link.Extensions.GetExtensionValue("uiname");

			if (!string.IsNullOrWhiteSpace(uiname))
			{
				return uiname;
			}

			// get filter group label from metadata

			var response = context.Execute(new RetrieveEntityRequest { LogicalName = link.Name }) as RetrieveEntityResponse;

			if (response == null)
			{
				return null;
			}

			var metadata = response.EntityMetadata;

			// get display name of the link target entity

			var localizedLabel = metadata.DisplayName.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == languageCode);

			return localizedLabel == null
				? metadata.DisplayName.GetLocalizedLabelString()
				: localizedLabel.Label;
		}

		private static string ToFilterOptionGroupLabel(EntityMetadata entityMetadata, Filter filter,
			int languageCode, IDictionary<string, string> overrideColumnNames)
		{
			// get filter group label from fetch

			var uiname = filter.Extensions.GetExtensionValue("uiname");

			if (!string.IsNullOrWhiteSpace(uiname))
			{
				return uiname;
			}

			// get filter option name

			var attributeLogicalName = filter.Extensions.GetExtensionValue("attribute");

			string overrideName;

			if (overrideColumnNames.TryGetValue(attributeLogicalName, out overrideName))
			{
				return overrideName;
			}

			var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeLogicalName);

			if (attributeMetadata == null)
			{
				return null;
			}

			var localizedLabel = attributeMetadata.DisplayName.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == languageCode);

			return localizedLabel == null
				? attributeMetadata.DisplayName.GetLocalizedLabelString()
				: localizedLabel.Label;
		}

		private static string ToFilterOptionGroupLabel(EntityMetadata entityMetadata, Filter filter, IViewConfiguration viewConfiguration, int languageCode)
		{
			// get filter group label from fetch

			var uiname = filter.Extensions.GetExtensionValue("uiname");

			if (!string.IsNullOrWhiteSpace(uiname))
			{
				return uiname;
			}

			// get filter option name

			var attributeLogicalName = filter.Extensions.GetExtensionValue("attribute");
			var overrideColumn =
				viewConfiguration.ColumnOverrides.FirstOrDefault(c => c.AttributeLogicalName == attributeLogicalName);

			if (overrideColumn != null && !string.IsNullOrWhiteSpace(overrideColumn.DisplayName))
			{
				return overrideColumn.DisplayName;
			}

			var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeLogicalName);

			if (attributeMetadata == null)
			{
				return null;
			}

			var localizedLabel = attributeMetadata.DisplayName.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == languageCode);

			return localizedLabel == null
				? attributeMetadata.DisplayName.GetLocalizedLabelString()
				: localizedLabel.Label;
		}
	}
}
