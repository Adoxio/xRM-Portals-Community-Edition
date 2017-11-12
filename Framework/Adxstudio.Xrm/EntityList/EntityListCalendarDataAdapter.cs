/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.EntityList
{
	public class EntityListCalendarDataAdapter : EntityListDataAdapter
	{
		public EntityListCalendarDataAdapter(EntityReference entityList, EntityReference view, IDataAdapterDependencies dependencies)
			: base(entityList, view, dependencies) { }

		public IEnumerable<EntityListEvent> SelectEvents(DateTime from, DateTime to, string filter = null, string search = null)
		{
			AssertEntityListAccess();

			var serviceContext = Dependencies.GetServiceContext();

			Entity entityList;

			if (!TryGetEntityList(serviceContext, EntityList, out entityList))
			{
				throw new LocalizedException("Entity_List_Retrieve_Exception", EntityList.LogicalName, entityList.Id);
			}

			var settings = new EntityListSettings(entityList);

			if (!settings.CalendarViewEnabled)
			{
				throw new LocalizedException("Calendar_View_Not_Enabled", EntityList.LogicalName, entityList.Id);
			}

			if (string.IsNullOrEmpty(settings.StartDateFieldName))
			{
				throw new LocalizedException("Start_Date_Field_Name_NotSpecified", EntityList.LogicalName, entityList.Id);
			}

			Entity view;

			if (!TryGetView(serviceContext, entityList, View, out view))
			{
				throw new LocalizedException("Unable_To_Retrieve_View_Exception", View.LogicalName, View.Id);
			}

			var fetchXml = view.GetAttributeValue<string>("fetchxml");

			if (string.IsNullOrEmpty(fetchXml))
			{
				throw new LocalizedException("FetchXML_View_Retrieve_Exception", View.LogicalName, View.Id);
			}

			var fetch = Fetch.Parse(fetchXml);

			var searchableAttributes = fetch.Entity.Attributes.Select(a => a.Name).ToArray();

			fetch.Entity.Attributes.Clear();

			AddAttributesToFetchEntity(fetch.Entity, settings);
			AddDateRangeFilterToFetchEntity(fetch.Entity, settings, from, to);
			AddSelectableFilterToFetchEntity(fetch.Entity, settings, filter);
			AddWebsiteFilterToFetchEntity(fetch.Entity, settings);
			AddSearchFilterToFetchEntity(fetch.Entity, settings, search, searchableAttributes);
			AddPermissionFilterToFetch(fetch, settings, serviceContext, CrmEntityPermissionRight.Read);

			if (this.EntityPermissionDenied)
			{
				throw new LocalizedException("Access_Denied_No_Permissions_To_View_These_Records_Message");
			}

			return FetchEntities(serviceContext, fetch)
				.Select(CreateEvent(serviceContext, settings))
				.Where(e => e != null);
		}
		
		protected Func<Entity, EntityListEvent> CreateEvent(OrganizationServiceContext serviceContext, EntityListSettings settings)
		{
			var organizerCache = new Dictionary<string, EntityListEventOrganizer>();
			var urlFactory = GetEventUrlFactory(settings);
			var timeZone = GetTimeZoneInfo(settings);

			return e =>
			{
				var start = e.GetAttributeValue<DateTime?>(settings.StartDateFieldName);

				if (start == null)
				{
					return null;
				}

				return new EntityListEvent(e.ToEntityReference())
				{
					Start = start.Value,

					End = string.IsNullOrWhiteSpace(settings.EndDateFieldName)
						? null
						: e.GetAttributeValue<DateTime?>(settings.EndDateFieldName),

					Summary = string.IsNullOrWhiteSpace(settings.SummaryFieldName)
						? null
						: e.GetAttributeValue<string>(settings.SummaryFieldName),

					Description = string.IsNullOrWhiteSpace(settings.DescriptionFieldName)
						? null
						: e.GetAttributeValue<string>(settings.DescriptionFieldName),

					Organizer = string.IsNullOrWhiteSpace(settings.OrganizerFieldName)
						? null
						: GetEventOrganizer(serviceContext, e, settings, organizerCache),

					Location = string.IsNullOrWhiteSpace(settings.LocationFieldName)
						? null
						: e.GetAttributeValue<string>(settings.LocationFieldName),

					IsAllDay = !string.IsNullOrWhiteSpace(settings.IsAllDayFieldName)
						&& e.GetAttributeValue<bool?>(settings.IsAllDayFieldName).GetValueOrDefault(),

					TimeZone = timeZone,

					TimeZoneDisplayMode = timeZone == null
						? EntityListTimeZoneDisplayMode.UserLocalTimeZone
						: EntityListTimeZoneDisplayMode.SpecificTimeZone,

					TimeZoneCode = settings.DisplayTimeZone,

					Url = urlFactory(e),
				};
			};
		}

		protected virtual Func<Entity, string> GetEventUrlFactory(EntityListSettings settings)
		{
			if (settings.WebPageForDetailsView == null || string.IsNullOrWhiteSpace(settings.IdQueryStringParameterName))
			{
				return e => null;
			}

			var serviceContext = Dependencies.GetServiceContext();

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
								new Condition("adx_webpageid", ConditionOperator.Equal, settings.WebPageForDetailsView.Id),
								new Condition("statecode", ConditionOperator.NotNull),
								new Condition("statecode", ConditionOperator.Equal, 0)
							}
						}
					}
				}
			};

			var webPage = serviceContext.RetrieveSingle(fetch);

			if (webPage == null)
			{
				return e => null;
			}

			var webPageUrl = Dependencies.GetUrlProvider().GetUrl(serviceContext, webPage);

			if (string.IsNullOrEmpty(webPageUrl))
			{
				return e => null;
			}

			return e =>
			{
				var url = new UrlBuilder(webPageUrl);

				url.QueryString.Set(settings.IdQueryStringParameterName, e.Id.ToString());

				return url.PathWithQueryString;
			};
		}

		protected virtual TimeZoneInfo GetTimeZoneInfo(EntityListSettings settings)
		{
			if (settings.TimeZoneDisplayMode == EntityListTimeZoneDisplayMode.UserLocalTimeZone)
			{
				return null;
			}

			if (settings.DisplayTimeZone == null)
			{
				return null;
			}

			var serviceContext = Dependencies.GetServiceContext();

			var timeZoneDefinition = serviceContext.CreateQuery("timezonedefinition")
				.Where(e => e.GetAttributeValue<int?>("timezonecode") == settings.DisplayTimeZone)
				.Select(e => new { StandardName = e.GetAttributeValue<string>("standardname") })
				.ToArray()
				.FirstOrDefault();

			if (timeZoneDefinition == null || string.IsNullOrEmpty(timeZoneDefinition.StandardName))
			{
				return null;
			}

			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById(timeZoneDefinition.StandardName);
			}
			catch (InvalidTimeZoneException)
			{
				return null;
			}
			catch (TimeZoneNotFoundException)
			{
				return null;
			}
		}

		private static void AddDateRangeFilterToFetchEntity(FetchEntity fetchEntity, EntityListSettings settings, DateTime from, DateTime to)
		{
			fetchEntity.Filters.Add(new Filter
			{
				Type = LogicalOperator.And,
				Conditions = new List<Condition>
				{
					new Condition(settings.StartDateFieldName, ConditionOperator.OnOrAfter, from),
					new Condition(settings.StartDateFieldName, ConditionOperator.OnOrBefore, to)
				}
			});
		}

		private static EntityListEventOrganizer GetEventOrganizer(OrganizationServiceContext serviceContext, Entity entity, EntityListSettings settings, IDictionary<string, EntityListEventOrganizer> organizerCache)
		{
			if (string.IsNullOrWhiteSpace(settings.OrganizerFieldName))
			{
				return null;
			}

			var organizerValue = entity.GetAttributeValue(settings.OrganizerFieldName);

			if (organizerValue == null)
			{
				return null;
			}

			var organizerEntityReference = organizerValue as EntityReference;

			if (organizerEntityReference == null)
			{
				return new EntityListEventOrganizer
				{
					Name = organizerValue.ToString()
				};
			}

			var organizerCacheKey = "{0}:{1}".FormatWith(organizerEntityReference.LogicalName, organizerEntityReference.Id);

			EntityListEventOrganizer cachedOrganizer;

			if (organizerCache.TryGetValue(organizerCacheKey, out cachedOrganizer))
			{
				return cachedOrganizer;
			}

			EntityListEventOrganizer contactOrganizer;

			if (TryGetEventOrganizerFromContact(serviceContext, organizerEntityReference, out contactOrganizer))
			{
				organizerCache[organizerCacheKey] = contactOrganizer;

				return contactOrganizer;
			}

			EntityListEventOrganizer systemUserOrganizer;

			if (TryGetEventOrganizerFromSystemUser(serviceContext, organizerEntityReference, out systemUserOrganizer))
			{
				organizerCache[organizerCacheKey] = systemUserOrganizer;

				return systemUserOrganizer;
			}

			if (!string.IsNullOrWhiteSpace(organizerEntityReference.Name))
			{
				var referenceNameOrganizer = new EntityListEventOrganizer
				{
					Name = organizerEntityReference.Name
				};

				organizerCache[organizerCacheKey] = referenceNameOrganizer;

				return referenceNameOrganizer;
			}

			return null;
		}

		private static bool TryGetEventOrganizerFromContact(OrganizationServiceContext serviceContext, EntityReference organizerEntityReference, out EntityListEventOrganizer organizer)
		{
			organizer = null;

			if (organizerEntityReference == null || organizerEntityReference.LogicalName != "contact")
			{
				return false;
			}

			var contact = serviceContext.RetrieveSingle(organizerEntityReference.LogicalName,
				new[] { "fullname", "emailaddress1" },
				new[] {
					new Condition("statecode", ConditionOperator.Equal, 0),
					new Condition("contactid", ConditionOperator.Equal, organizerEntityReference.Id)
				});

			if (contact == null)
			{
				return false;
			}

			organizer = new EntityListEventOrganizer
			{
				Name = contact.GetAttributeValue<string>("fullname"),
				Email = contact.GetAttributeValue<string>("emailaddress1")
			};

			return true;
		}

		private static bool TryGetEventOrganizerFromSystemUser(OrganizationServiceContext serviceContext, EntityReference organizerEntityReference, out EntityListEventOrganizer organizer)
		{
			organizer = null;

			if (organizerEntityReference == null || organizerEntityReference.LogicalName != "systemuser")
			{
				return false;
			}

			var systemUser = serviceContext.RetrieveSingle(
				organizerEntityReference.LogicalName,
				new[] { "fullname", "internalemailaddress" },
				new Condition("systemuserid", ConditionOperator.Equal, organizerEntityReference.Id));

			if (systemUser == null)
			{
				return false;
			}

			organizer = new EntityListEventOrganizer
			{
				Name = systemUser.GetAttributeValue<string>("fullname"),
				Email = systemUser.GetAttributeValue<string>("internalemailaddress")
			};

			return true;
		}
	}

	public class EntityListEvent
	{
		public EntityListEvent(EntityReference entityReference)
		{
			if (entityReference == null) throw new ArgumentNullException("entityReference");

			EntityReference = entityReference;
		}

		public string Description { get; set; }

		public EntityReference EntityReference { get; private set; }

		public DateTime? End { get; set; }

		public bool IsAllDay { get; set; }

		public string Location { get; set; }

		public EntityListEventOrganizer Organizer { get; set; }

		public DateTime Start { get; set; }

		public string Summary { get; set; }

		public TimeZoneInfo TimeZone { get; set; }

		public int? TimeZoneCode { get; set; }

		public EntityListTimeZoneDisplayMode TimeZoneDisplayMode { get; set; }

		public string Url { get; set; }
	}

	public class EntityListEventOrganizer
	{
		public string Email { get; set; }

		public string Name { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Email)
				? Name
				: "{0} <{1}>".FormatWith(Name, Email);
		}
	}

	public enum EntityListTimeZoneDisplayMode
	{
		UserLocalTimeZone = 756150000,
		SpecificTimeZone = 756150001
	}
}
