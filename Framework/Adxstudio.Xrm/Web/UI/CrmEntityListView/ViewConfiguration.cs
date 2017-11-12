/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.EntityList;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Action = Adxstudio.Xrm.Web.UI.JsonConfiguration.Action;

namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
	/// <summary>
	/// Settings needed to be able to retrieve a view and configure its display.
	/// </summary>
	public class ViewConfiguration : IViewConfiguration
	{
		private List<JsonConfiguration.ViewColumn> _columnOverrides;
		private string _filterQueryStringParameterName;
		private string _sortQueryStringParameterName;
		private string _pageQueryStringParameterName;
		private string _filterByUserOptionLabel;
		private int _actionLinksColumnWidth;
		private List<ViewActionLink> _viewActionLinks;
		private List<ViewActionLink> _itemActionLinks;
		private List<ViewActionLink> _createRelatedRecordActionLinks;

		public string EntityName { get; set; }
		public string PrimaryKeyName { get; set; }
		public string ViewName { get; set; }
		public Guid ViewId { get; set; }
		public Guid Id { get; set; }
		public int PageSize { get; set; }
		public bool? DataPagerEnabled { get; set; }
		public string LayoutXml { get; set; }
		public string FetchXml { get; set; }
		public string ViewDisplayName { get; set; }
		public string CssClass { get; set; }
		public string GridCssClass { get; set; }
		public EntityGridExtensions.GridColumnWidthStyle? GridColumnWidthStyle { get; set; }
		public string LoadingMessage { get; set; }
		public string ErrorMessage { get; set; }
		public string AccessDeniedMessage { get; set; }
		public string EmptyListText { get; set; }

		public List<JsonConfiguration.ViewColumn> ColumnOverrides
		{
			get { return _columnOverrides ?? (_columnOverrides = new List<JsonConfiguration.ViewColumn>()); }
			set { _columnOverrides = value; }
		}

		public bool EnableEntityPermissions { get; set; }

		public ViewSearch Search { get; set; }

		public string FilterQueryStringParameterName
		{
			get { return string.IsNullOrWhiteSpace(_filterQueryStringParameterName) ? "filter" : _filterQueryStringParameterName; }
			set { _filterQueryStringParameterName = value; }
		}

		public string SortQueryStringParameterName
		{
			get { return string.IsNullOrWhiteSpace(_sortQueryStringParameterName) ? "sort" : _sortQueryStringParameterName; }
			set { _sortQueryStringParameterName = value; }
		}

		public string PageQueryStringParameterName
		{
			get { return string.IsNullOrWhiteSpace(_pageQueryStringParameterName) ? "page" : _pageQueryStringParameterName; }
			set { _pageQueryStringParameterName = value; }
		}

		public string FilterByUserOptionLabel
		{
			get { return string.IsNullOrWhiteSpace(_filterByUserOptionLabel) ? "My" : _filterByUserOptionLabel; }
			set { _filterByUserOptionLabel = value; }
		}

		public string FilterPortalUserAttributeName { get; set; }
		public string FilterAccountAttributeName { get; set; }
		public string FilterWebsiteAttributeName { get; set; }

		public DetailsActionLink DetailsActionLink { get; set; }

		public InsertActionLink InsertActionLink { get; set; }

		public AssociateActionLink AssociateActionLink { get; set; }

		public EditActionLink EditActionLink { get; set; }

		public DeleteActionLink DeleteActionLink { get; set; }
		public CloseIncidentActionLink CloseIncidentActionLink { get; set; }
		public ResolveCaseActionLink ResolveCaseActionLink { get; set; }
		public ReopenCaseActionLink ReopenCaseActionLink { get; set; }
		public CancelCaseActionLink CancelCaseActionLink { get; set; }
		public QualifyLeadActionLink QualifyLeadActionLink { get; set; }
		public ConvertOrderToInvoiceActionLink ConvertOrderToInvoiceActionLink { get; set; }
		public ConvertQuoteToOrderActionLink ConvertQuoteToOrderActionLink { get; set; }
		public CalculateOpportunityActionLink CalculateOpportunityActionLink { get; set; }
		public DeactivateActionLink DeactivateActionLink { get; set; }
		public ActivateActionLink ActivateActionLink { get; set; }
		public ActivateQuoteActionLink ActivateQuoteActionLink { get; set; }
		public SetOpportunityOnHoldActionLink SetOpportunityOnHoldActionLink { get; set; }
		public ReopenOpportunityActionLink ReopenOpportunityActionLink { get; set; }
		public WinOpportunityActionLink WinOpportunityActionLink { get; set; }
		public LoseOpportunityActionLink LoseOpportunityActionLink { get; set; }
		public GenerateQuoteFromOpportunityActionLink GenerateQuoteFromOpportunityActionLink { get; set; }
		public UpdatePipelinePhaseActionLink UpdatePipelinePhaseActionLink { get; set; }
		public DisassociateActionLink DisassociateActionLink { get; set; }

		public List<ViewActionLink> CreateRelatedRecordActionLinks
		{
			get { return _createRelatedRecordActionLinks ?? (_createRelatedRecordActionLinks = new List<ViewActionLink>()); } 
			set { _createRelatedRecordActionLinks = value; }
		}

		/// <summary>
		/// Actions that are applicable to the view or entire record set.
		/// </summary>
		public List<ViewActionLink> ViewActionLinks
		{
			get { return _viewActionLinks ?? (_viewActionLinks = new List<ViewActionLink>()); }
			set { _viewActionLinks = value; }
		}
		
		/// <summary>
		/// Actions that are applicable to a single record item.
		/// </summary>
		public List<ViewActionLink> ItemActionLinks
		{
			get { return _itemActionLinks ?? (_itemActionLinks = new List<ViewActionLink>()); }
			set { _itemActionLinks = value; }
		}

		public string ActionColumnHeaderText { get; set; }

		public int ActionLinksColumnWidth
		{
			get { return _actionLinksColumnWidth <= 0 ? 20 : _actionLinksColumnWidth; }
			set { _actionLinksColumnWidth = value; }
		}

		public string PortalName { get; set; }

		public int LanguageCode { get; set; }

		public MapConfiguration MapSettings { get; set; }

		public CalendarConfiguration CalendarSettings { get; set; }

		public FilterConfiguration FilterSettings { get; set; }

		public string ViewRelationshipName { get; set; }

		public string ViewTargetEntityType { get; set; }

		public string ModalLookupAttributeLogicalName { get; set; }

		public string ModalLookupEntityLogicalName { get; set; }

		public Guid? ModalLookupFormReferenceEntityId { get; set; }

		public string ModalLookupFormReferenceEntityLogicalName { get; set; }

		public string ModalLookupFormReferenceRelationshipName { get; set; }

		public string ModalLookupFormReferenceRelationshipRole { get; set; }

		public int ModalLookupGridPageSize { get; set; }

		public Guid SubgridFormEntityId { get; set; }

		public string SubgridFormEntityLogicalName { get; set; }

		/// <summary>
		/// Parameterless Constructor for MVC controller instantiation
		/// </summary>
		public ViewConfiguration()
		{
			Search = new ViewSearch();
			DetailsActionLink = new DetailsActionLink();
			InsertActionLink = new InsertActionLink();
			AssociateActionLink = new AssociateActionLink();
			EditActionLink = new EditActionLink();
			DeleteActionLink = new DeleteActionLink();
			DisassociateActionLink = new DisassociateActionLink();
			CalendarSettings = new CalendarConfiguration();
			MapSettings = new MapConfiguration();
			FilterSettings = new FilterConfiguration();
			InitializeSpecialActionsLinks();
		}

		/// <summary>
		/// Class constructor for a view by name.
		/// </summary>
		/// <param name="entityName"></param>
		/// <param name="primaryKeyName"></param>
		/// <param name="viewName"></param>
		/// <param name="pageSize"></param>
		public ViewConfiguration(string entityName, string primaryKeyName, string viewName, int pageSize = 10)
		{
			EntityName = entityName;
			PrimaryKeyName = primaryKeyName;
			ViewName = viewName;
			PageSize = pageSize;
			Guid id;
			using (var md5 = MD5.Create())
			{
				var hash = md5.ComputeHash(Encoding.Default.GetBytes(string.Format("{0}_{1}", entityName, viewName)));
				id = new Guid(hash);
			}
			Id = id;
			Search = new ViewSearch();
			DetailsActionLink = new DetailsActionLink();
			InsertActionLink = new InsertActionLink();
			AssociateActionLink = new AssociateActionLink();
			EditActionLink = new EditActionLink();
			DeleteActionLink = new DeleteActionLink();
			DisassociateActionLink = new DisassociateActionLink();
			CalendarSettings = new CalendarConfiguration();
			MapSettings = new MapConfiguration();
			FilterSettings = new FilterConfiguration();

			InitializeSpecialActionsLinks();
		}

		/// <summary>
		/// Class constructor for a view by unique identifier.
		/// </summary>
		/// <param name="entityName"></param>
		/// <param name="primaryKeyName"></param>
		/// <param name="viewId"></param>
		/// <param name="pageSize"></param>
		public ViewConfiguration(string entityName, string primaryKeyName, Guid viewId, int pageSize = 10)
		{
			EntityName = entityName;
			PrimaryKeyName = primaryKeyName;
			ViewId = Id = viewId;
			PageSize = pageSize;
			Search = new ViewSearch();
			DetailsActionLink = new DetailsActionLink();
			InsertActionLink = new InsertActionLink();
			AssociateActionLink = new AssociateActionLink();
			EditActionLink = new EditActionLink();
			DeleteActionLink = new DeleteActionLink();
			DisassociateActionLink = new DisassociateActionLink();
			CalendarSettings = new CalendarConfiguration();
			MapSettings = new MapConfiguration();
			FilterSettings = new FilterConfiguration();

			InitializeSpecialActionsLinks();
		}

		/// <summary>
		/// Class constructor for a view by savedqueryview.
		/// </summary>
		/// <param name="savedQueryView"></param>
		/// <param name="pageSize"></param>
		public ViewConfiguration(SavedQueryView savedQueryView, int pageSize = 10)
		{
			EntityName = savedQueryView.EntityLogicalName;
			PrimaryKeyName = savedQueryView.PrimaryKeyLogicalName;
			ViewId = savedQueryView.Id;
			PageSize = pageSize;
			Search = new ViewSearch();
			DetailsActionLink = new DetailsActionLink();
			InsertActionLink = new InsertActionLink();
			AssociateActionLink = new AssociateActionLink();
			EditActionLink = new EditActionLink();
			DeleteActionLink = new DeleteActionLink();
			DisassociateActionLink = new DisassociateActionLink();
			CalendarSettings = new CalendarConfiguration();
			MapSettings = new MapConfiguration();
			FilterSettings = new FilterConfiguration();

			InitializeSpecialActionsLinks();
		}

		/// <summary>
		/// Class constructor used by SubgridControlTemplate Class
		/// </summary>
		public ViewConfiguration(IPortalContext portalContext, SavedQueryView savedQueryView, int pageSize, string fetchString,
			ViewSearch viewsearch, bool enableEntityPermissions, JsonConfiguration.GridMetadata gridMetadata, int languageCode,
			string contextName, string viewRelationshipName, string viewTargetEntityType, Guid viewId, int modalLookupGridPageSize)
		{
			EntityName = savedQueryView.EntityLogicalName;
			PrimaryKeyName = savedQueryView.PrimaryKeyLogicalName;
			ViewId = savedQueryView.Id;
			PageSize = pageSize;
			Search = new ViewSearch();
			DetailsActionLink = new DetailsActionLink();
			InsertActionLink = new InsertActionLink();
			AssociateActionLink = new AssociateActionLink();
			EditActionLink = new EditActionLink();
			DeleteActionLink = new DeleteActionLink();
			DisassociateActionLink = new DisassociateActionLink();
			CalendarSettings = new CalendarConfiguration();
			MapSettings = new MapConfiguration();
			FilterSettings = new FilterConfiguration();

			InitializeSpecialActionsLinks();

			FetchXml = fetchString;
			Search = viewsearch;
			EnableEntityPermissions = enableEntityPermissions;
			PortalName = contextName;
			LanguageCode = languageCode;

			ViewRelationshipName = viewRelationshipName;
			ViewTargetEntityType = viewTargetEntityType;
			ViewId = viewId;
			ModalLookupGridPageSize = modalLookupGridPageSize;

			if (gridMetadata != null)
			{
				SetGridConfig(gridMetadata, languageCode);

				if (gridMetadata.ViewActions != null)
				{
					SetViewActions(portalContext, gridMetadata, languageCode);
				}

				if (gridMetadata.ItemActions != null)
				{
					SetItemActions(portalContext, gridMetadata, languageCode);
				}
			}
		}

		/// <summary>
		/// Class constructor used by the EntityList Control
		/// </summary>
		public ViewConfiguration(IPortalContext portalContext, OrganizationServiceContext serviceContext, Entity entitylist,
			string entityName, string primaryKeyName, Guid viewGuid, JsonConfiguration.GridMetadata gridMetadata, string portalName,
			int languageCode, bool enableEntityPermissions, string pageQueryStringField, string filterQueryStringField,
			string searchQueryStringField, string sortQueryStringField, string filterByUserOptionLabel, int actionLinksColumnWidth,
			string metadataFilterQueryStringField, string defaultDetailsButtonLabel, string actionLinkDetailsViewTooltipLabel,
			string defaultCreateButtonLabel, string actionLinkInsertTooltipLabel, string defaultEmptyListText, string viewDisplayName = null)
		{
			EntityName = entityName;
			PrimaryKeyName = primaryKeyName;
			ViewId = Id = viewGuid;
			PageSize = entitylist.GetAttributeValue<int>("adx_pagesize");
			Search = new ViewSearch();
			DetailsActionLink = new DetailsActionLink();
			InsertActionLink = new InsertActionLink();
			AssociateActionLink = new AssociateActionLink();
			EditActionLink = new EditActionLink();
			DeleteActionLink = new DeleteActionLink();
			DisassociateActionLink = new DisassociateActionLink();
			FilterSettings = new FilterConfiguration();
			EnableEntityPermissions = enableEntityPermissions;
			PortalName = portalName;
			LanguageCode = languageCode;
			ViewDisplayName = viewDisplayName;

			InitializeSpecialActionsLinks();

			var detailsViewPageReference = entitylist.GetAttributeValue<EntityReference>("adx_webpagefordetailsview");
			UrlBuilder detailsUrl = null;
			if (detailsViewPageReference != null) { detailsUrl = EntityListFunctions.GetUrlForWebPage(serviceContext, detailsViewPageReference, portalName); }

			var localizedDetailsButtonLabel = Localization.GetLocalizedString(entitylist.GetAttributeValue<string>("adx_detailsbuttonlabel"), languageCode);
			var idQueryStringParameterName = entitylist.GetAttributeValue<string>("adx_idquerystringparametername");
			var createPageReference = entitylist.GetAttributeValue<EntityReference>("adx_webpageforcreate");
			UrlBuilder createUrl = null;
			if (createPageReference != null) { createUrl = EntityListFunctions.GetUrlForWebPage(serviceContext, createPageReference, portalName); }

			var localizedCreateButtonLabel = Localization.GetLocalizedString(entitylist.GetAttributeValue<string>("adx_createbuttonlabel"), languageCode);
			var localizedEmptyListText = Localization.GetLocalizedString(entitylist.GetAttributeValue<string>("adx_emptylisttext"), languageCode);

			var searchEnabled = entitylist.GetAttributeValue<bool?>("adx_searchenabled").GetValueOrDefault(false);
			var searchLocalizedPlaceholder = Localization.GetLocalizedString(entitylist.GetAttributeValue<string>("adx_searchplaceholdertext"), languageCode);
			var searchLocalizedTooltip = Localization.GetLocalizedString(entitylist.GetAttributeValue<string>("adx_searchtooltiptext"), languageCode);

			var calendarEnabled = entitylist.GetAttributeValue<bool?>("adx_calendar_enabled").GetValueOrDefault(false);
			var calendarInitialDate = entitylist.GetAttributeValue<DateTime?>("adx_calendar_initialdate");
			var calendarInitialDateString = calendarInitialDate.HasValue ? calendarInitialDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty;

			var mapEnabled = entitylist.GetAttributeAliasedValue<bool?>("adx_map_enabled").GetValueOrDefault(false);

			var filterDefinition = entitylist.GetAttributeValue<string>("adx_filter_definition");
			var filterApplyButtonLabel = Localization.GetLocalizedString(entitylist.GetAttributeValue<string>("adx_filter_applybuttonlabel"), languageCode);
			var filterOrientation = entitylist.GetAttributeValue<OptionSetValue>("adx_filter_orientation");

			if (!string.IsNullOrWhiteSpace(pageQueryStringField)) PageQueryStringParameterName = pageQueryStringField;
			if (!string.IsNullOrWhiteSpace(filterQueryStringField)) FilterQueryStringParameterName = filterQueryStringField;
			if (!string.IsNullOrWhiteSpace(searchQueryStringField)) Search.SearchQueryStringParameterName = searchQueryStringField;
			if (!string.IsNullOrWhiteSpace(sortQueryStringField)) SortQueryStringParameterName = sortQueryStringField;
			if (!string.IsNullOrWhiteSpace(filterByUserOptionLabel)) FilterByUserOptionLabel = filterByUserOptionLabel;

			ActionLinksColumnWidth = actionLinksColumnWidth;

			if (detailsUrl != null)
			{
				DetailsActionLink = new DetailsActionLink(detailsUrl, true, null, null, idQueryStringParameterName);

				if (!string.IsNullOrWhiteSpace(localizedDetailsButtonLabel)) { DetailsActionLink.Label = localizedDetailsButtonLabel; }

				else if (!string.IsNullOrWhiteSpace(defaultDetailsButtonLabel)) { DetailsActionLink.Label = defaultDetailsButtonLabel; }

				if (!string.IsNullOrWhiteSpace(actionLinkDetailsViewTooltipLabel)) { DetailsActionLink.Tooltip = actionLinkDetailsViewTooltipLabel; }
			}

			if (createUrl != null)
			{
				InsertActionLink = new InsertActionLink(createUrl, true);

				if (!string.IsNullOrWhiteSpace(localizedCreateButtonLabel)) { InsertActionLink.Label = localizedCreateButtonLabel; }

				else if (!string.IsNullOrWhiteSpace(defaultCreateButtonLabel)) { InsertActionLink.Label = defaultCreateButtonLabel; }

				if (!string.IsNullOrWhiteSpace(actionLinkInsertTooltipLabel)) { InsertActionLink.Tooltip = actionLinkInsertTooltipLabel; }
			}

			if (!string.IsNullOrWhiteSpace(localizedEmptyListText)) { EmptyListText = localizedEmptyListText; }

			else if (!string.IsNullOrWhiteSpace(defaultEmptyListText)) { EmptyListText = defaultEmptyListText; }

			if (gridMetadata != null)
			{
				SetGridConfig(gridMetadata, languageCode);

				if (gridMetadata.ViewActions != null)
				{
					SetViewActions(portalContext, gridMetadata, languageCode);
				}

				if (gridMetadata.ItemActions != null)
				{
					SetItemActions(portalContext, gridMetadata, languageCode);
				}
			}

			FilterPortalUserAttributeName = entitylist.GetAttributeValue<string>("adx_filterportaluser");
			FilterAccountAttributeName = entitylist.GetAttributeValue<string>("adx_filteraccount");
			FilterWebsiteAttributeName = entitylist.GetAttributeValue<string>("adx_filterwebsite");

			Search = new ViewSearch(searchEnabled, "query", searchLocalizedPlaceholder, searchLocalizedTooltip);

			if (calendarEnabled)
			{
				CalendarSettings = new CalendarConfiguration(serviceContext, calendarEnabled, calendarInitialDateString,
					entitylist.GetAttributeValue<OptionSetValue>("adx_calendar_initialview"), entitylist.GetAttributeValue<OptionSetValue>("adx_calendar_style"),
					entitylist.GetAttributeValue<OptionSetValue>("adx_calendar_timezonemode"), entitylist.GetAttributeValue<int?>("adx_calendar_timezone"),
					entitylist.GetAttributeValue<string>("adx_calendar_startdatefieldname"), entitylist.GetAttributeValue<string>("adx_calendar_enddatefieldname"),
					entitylist.GetAttributeValue<string>("adx_calendar_summaryfieldname"), entitylist.GetAttributeValue<string>("adx_calendar_descriptionfieldname"),
					entitylist.GetAttributeValue<string>("adx_calendar_organizerfieldname"), entitylist.GetAttributeValue<string>("adx_calendar_locationfieldname"),
					entitylist.GetAttributeValue<string>("adx_calendar_alldayfieldname"));
			}
			else
			{
				CalendarSettings = new CalendarConfiguration { Enabled = calendarEnabled };
			}

			if (mapEnabled)
			{
				MapSettings = new MapConfiguration(portalContext, mapEnabled, entitylist.GetAttributeValue<OptionSetValue>("adx_map_distanceunits"),
					entitylist.GetAttributeValue<string>("adx_map_distancevalues"), entitylist.GetAttributeValue<int?>("adx_map_infoboxoffsety"),
					entitylist.GetAttributeValue<int?>("adx_map_infoboxoffsetx"), entitylist.GetAttributeValue<int?>("adx_map_pushpinwidth"),
					entitylist.GetAttributeValue<string>("adx_map_pushpinurl"), entitylist.GetAttributeValue<int?>("adx_map_zoom"),
					entitylist.GetAttributeAliasedValue<double?>("adx_map_longitude"), entitylist.GetAttributeAliasedValue<double?>("adx_map_latitude"),
					entitylist.GetAttributeAliasedValue<string>("adx_map_resturl"), entitylist.GetAttributeValue<string>("adx_map_credentials"),
					entitylist.GetAttributeValue<int?>("adx_map_pushpinheight"));
			}
			else
			{
				MapSettings = new MapConfiguration { Enabled = mapEnabled };
			}

			FilterSettings.Enabled = entitylist.GetAttributeValue<bool?>("adx_filter_enabled").GetValueOrDefault(false);

			if (!string.IsNullOrWhiteSpace(filterDefinition)) { FilterSettings.Definition = filterDefinition; }

			if (!string.IsNullOrWhiteSpace(filterApplyButtonLabel)) { FilterSettings.ApplyButtonLabel = filterApplyButtonLabel; }

			if (!string.IsNullOrWhiteSpace(metadataFilterQueryStringField)) { FilterSettings.FilterQueryStringParameterName = metadataFilterQueryStringField; }

			if (filterOrientation != null)
			{
				if (Enum.IsDefined(typeof(FilterConfiguration.FilterOrientation), filterOrientation.Value))
				{
					FilterSettings.Orientation = (FilterConfiguration.FilterOrientation)filterOrientation.Value;
				}
				else
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Filter Orientation value '{0}' is not a valid value defined by FilterConfiguration.FilterOrientation class.", filterOrientation.Value));
				}
			}
		}

		public SavedQueryView GetSavedQueryView(OrganizationServiceContext serviceContext, int languageCode = 0)
		{
			if (string.IsNullOrWhiteSpace(EntityName))
			{
				throw new ApplicationException("EntityName must not be null.");
			}

			if (!string.IsNullOrWhiteSpace(EntityName) && string.IsNullOrWhiteSpace(PrimaryKeyName))
			{
				PrimaryKeyName = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(serviceContext, EntityName);
			}

			if (string.IsNullOrWhiteSpace(PrimaryKeyName))
			{
				throw new ApplicationException("The entity primary key logical name couldn't be determined.");
			}

			if (string.IsNullOrEmpty(ViewName) && ViewId == Guid.Empty && LayoutXml == null && FetchXml == null)
			{
				throw new ApplicationException(string.Format("The current view configuration isn't valid. Either specify a ViewName or ViewId {0}.", " or set the LayoutXml and FetchXml."));
			}

			SavedQueryView view = null;

			if (ViewId != Guid.Empty && !string.IsNullOrEmpty(FetchXml) && !string.IsNullOrEmpty(LayoutXml))
			{
				view = new SavedQueryView(serviceContext, FetchXml, LayoutXml, ViewId, languageCode);
			}
			else if (ViewId != Guid.Empty)
			{
				view = new SavedQueryView(serviceContext, ViewId, languageCode);
			}
			else if (!string.IsNullOrEmpty(EntityName) && !string.IsNullOrEmpty(ViewName) && !string.IsNullOrEmpty(FetchXml) && !string.IsNullOrEmpty(LayoutXml))
			{
				view = new SavedQueryView(serviceContext, FetchXml, LayoutXml, EntityName, ViewName, languageCode);
			}
			else if (!string.IsNullOrEmpty(EntityName) && !string.IsNullOrEmpty(ViewName))
			{
				view = new SavedQueryView(serviceContext, EntityName, ViewName, languageCode);
			}

			if (view == null)
			{
				throw new ApplicationException("A view hasn't been properly configured.");
			}

			return view;
		}

		/// <summary>
		/// Gets the extended entity savedquery (view) for the corresponding view configuration properties.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="languageCode"></param>
		/// <returns><see cref="EntityView"/></returns>
		public EntityView GetEntityView(OrganizationServiceContext serviceContext, int languageCode = 0)
		{
			if (string.IsNullOrWhiteSpace(EntityName))
			{
				throw new ApplicationException("EntityName must not be null.");
			}

			if (!string.IsNullOrWhiteSpace(EntityName) && string.IsNullOrWhiteSpace(PrimaryKeyName))
			{
				PrimaryKeyName = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(serviceContext, EntityName);
			}

			if (string.IsNullOrWhiteSpace(PrimaryKeyName))
			{
				throw new ApplicationException("The entity primary key logical name couldn't be determined.");
			}

			if (string.IsNullOrEmpty(ViewName) && ViewId == Guid.Empty && LayoutXml == null && FetchXml == null)
			{
				throw new ApplicationException(string.Format("The current view configuration isn't valid. Either specify a ViewName or ViewId {0}.", " or set the LayoutXml and FetchXml."));
			}

			EntityView view = null;

			if (ViewId != Guid.Empty && !string.IsNullOrEmpty(FetchXml) && !string.IsNullOrEmpty(LayoutXml))
			{
				view = new EntityView(serviceContext, FetchXml, LayoutXml, ViewId, languageCode);
			}
			else if (ViewId != Guid.Empty)
			{
				view = new EntityView(serviceContext, ViewId, languageCode);
			}
			else if (!string.IsNullOrEmpty(EntityName) && !string.IsNullOrEmpty(ViewName) && !string.IsNullOrEmpty(FetchXml) && !string.IsNullOrEmpty(LayoutXml))
			{
				view = new EntityView(serviceContext, FetchXml, LayoutXml, EntityName, ViewName, languageCode);
			}
			else if (!string.IsNullOrEmpty(EntityName) && !string.IsNullOrEmpty(ViewName))
			{
				view = new EntityView(serviceContext, EntityName, ViewName, languageCode);
			}

			if (view == null)
			{
				throw new ApplicationException("A view hasn't been properly configured.");
			}

			return view;
		}

		public SavedQueryView GetSavedQueryView(IEnumerable<SavedQueryView> views)
		{
			if (string.IsNullOrWhiteSpace(EntityName))
			{
				throw new ApplicationException("EntityName must not be null.");
			}

			if (!string.IsNullOrWhiteSpace(EntityName) && string.IsNullOrWhiteSpace(PrimaryKeyName))
			{
				throw new ApplicationException("PrimaryKeyName must not be null.");
			}

			if (string.IsNullOrEmpty(ViewName) && ViewId == Guid.Empty)
			{
				throw new ApplicationException(string.Format("The current view configuration isn't valid. Either specify a ViewName or ViewId {0}.", string.Empty));
			}

			SavedQueryView view = null;

			if (ViewId != Guid.Empty)
			{
				view = views.FirstOrDefault(s => s.SavedQuery != null && s.Id == ViewId);
			}
			else if (!string.IsNullOrEmpty(EntityName) && !string.IsNullOrEmpty(ViewName))
			{
				view = views.FirstOrDefault(s => s.EntityLogicalName == EntityName && s.Name == ViewName);
			}

			if (view == null)
			{
				throw new ApplicationException("The current view configuration isn't valid. Ensure that a valid ViewName or ViewId along with EntityName and PrimaryKeyName have been specified.");
			}

			return view;
		}

		/// <summary>
		/// Gets the current view from a collection of <see cref="EntityView"/> records.
		/// </summary>
		/// <param name="views">collection of <see cref="SavedQueryView"/> records.</param>
		/// <returns><see cref="EntityView"/></returns>
		public EntityView GetEntityView(IEnumerable<EntityView> views)
		{
			if (string.IsNullOrWhiteSpace(EntityName))
			{
				throw new ApplicationException("EntityName must not be null.");
			}

			if (!string.IsNullOrWhiteSpace(EntityName) && string.IsNullOrWhiteSpace(PrimaryKeyName))
			{
				throw new ApplicationException("PrimaryKeyName must not be null.");
			}

			if (string.IsNullOrEmpty(ViewName) && ViewId == Guid.Empty)
			{
				throw new ApplicationException(string.Format("The current view configuration isn't valid. Either specify a ViewName or ViewId {0}.", string.Empty));
			}

			EntityView view = null;

			if (ViewId != Guid.Empty)
			{
				view = views.FirstOrDefault(s => s.SavedQuery != null && s.Id == ViewId);
			}
			else if (!string.IsNullOrEmpty(EntityName) && !string.IsNullOrEmpty(ViewName))
			{
				view = views.FirstOrDefault(s => s.EntityLogicalName == EntityName && s.Name == ViewName);
			}

			if (view == null)
			{
				throw new ApplicationException("The current view configuration isn't valid. Ensure that a valid ViewName or ViewId along with EntityName and PrimaryKeyName have been specified.");
			}

			return view;
		}

		private void InitializeSpecialActionsLinks()
		{
			CloseIncidentActionLink = new CloseIncidentActionLink();
			ResolveCaseActionLink = new ResolveCaseActionLink();
			ReopenCaseActionLink = new ReopenCaseActionLink();
			CancelCaseActionLink = new CancelCaseActionLink();
			QualifyLeadActionLink = new QualifyLeadActionLink();
			ConvertQuoteToOrderActionLink = new ConvertQuoteToOrderActionLink();
			ConvertOrderToInvoiceActionLink = new ConvertOrderToInvoiceActionLink();
			CalculateOpportunityActionLink = new CalculateOpportunityActionLink();
			DeactivateActionLink = new DeactivateActionLink();
			ActivateActionLink = new ActivateActionLink();
			ActivateQuoteActionLink = new ActivateQuoteActionLink();
			SetOpportunityOnHoldActionLink = new SetOpportunityOnHoldActionLink();
			ReopenOpportunityActionLink = new ReopenOpportunityActionLink();
			WinOpportunityActionLink = new WinOpportunityActionLink();
			LoseOpportunityActionLink = new LoseOpportunityActionLink();
			GenerateQuoteFromOpportunityActionLink = new GenerateQuoteFromOpportunityActionLink();
			UpdatePipelinePhaseActionLink = new UpdatePipelinePhaseActionLink();
		}

		private void SetGridConfig(JsonConfiguration.GridMetadata gridMetadata, int languageCode)
		{
			if (gridMetadata.ColumnOverrides != null && gridMetadata.ColumnOverrides.Any())
			{
				ColumnOverrides = gridMetadata.ColumnOverrides;
			}

			CssClass = gridMetadata.CssClass;
			GridCssClass = gridMetadata.GridCssClass;

			if (gridMetadata.GridColumnWidthStyle != null)
			{
				GridColumnWidthStyle = gridMetadata.GridColumnWidthStyle.GetValueOrDefault(EntityGridExtensions.GridColumnWidthStyle.Percent);
			}

			AccessDeniedMessage = Localization.GetLocalizedString(gridMetadata.AccessDeniedMessage, languageCode);
			ErrorMessage = Localization.GetLocalizedString(gridMetadata.ErrorMessage, languageCode);
			LoadingMessage = Localization.GetLocalizedString(gridMetadata.LoadingMessage, languageCode);

			EmptyListText = !string.IsNullOrEmpty(Localization.GetLocalizedString(gridMetadata.EmptyMessage, languageCode)) ? Localization.GetLocalizedString(gridMetadata.EmptyMessage, languageCode) : null;
		}

		private void SetViewActions(IPortalContext portalContext, JsonConfiguration.GridMetadata gridMetadata, int languageCode)
		{
			var viewActions = gridMetadata.ViewActions.ToList();
			var viewActionLinks = new List<ViewActionLink>();

			foreach (var action in viewActions)
			{
				if (action is CreateAction)
				{
					var createAction = (CreateAction)action;

					if (!createAction.IsConfigurationValid()) continue;

					var insertActionLink = new InsertActionLink(portalContext, gridMetadata, languageCode, createAction, true, PortalName);

					viewActionLinks.Add(insertActionLink);
				}
				if (action is SearchAction)  //only applicable in the case of a subgrid
				{
					var searchAction = (SearchAction)action;

					if (!searchAction.IsConfigurationValid()) continue;

					var searchTooltipText = Localization.GetLocalizedString(searchAction.TooltipText, languageCode);
					var searchButtonLabel = Localization.GetLocalizedString(searchAction.ButtonLabel, languageCode);
					var searchPlaceholderText = Localization.GetLocalizedString(searchAction.PlaceholderText, languageCode);

					if (!string.IsNullOrWhiteSpace(searchTooltipText)) Search.TooltipText = searchTooltipText;
					if (!string.IsNullOrWhiteSpace(searchButtonLabel)) Search.ButtonLabel = searchButtonLabel;
					if (!string.IsNullOrWhiteSpace(searchPlaceholderText)) Search.PlaceholderText = searchPlaceholderText;
				}
				if (action is DownloadAction)
				{
					var downloadAction = (DownloadAction)action;

					if (!downloadAction.IsConfigurationValid()) continue;

					var downloadActionLink = new DownloadActionLink(portalContext, languageCode, downloadAction, true, null, PortalName);

					viewActionLinks.Add(downloadActionLink);
				}
				if (action is AssociateAction && !string.IsNullOrWhiteSpace(ViewRelationshipName))
				{
					// AssociateRequest is not relevant to grids that are not showing related records.
					var associateAction = (AssociateAction)action;

					if (!associateAction.IsConfigurationValid()) continue;

					var associateActionViewConfiguration = new ViewConfiguration(ViewTargetEntityType, null,
						associateAction.ViewId != Guid.Empty ? associateAction.ViewId : ViewId, ModalLookupGridPageSize)
					{
						Search = new ViewSearch(true),
						EnableEntityPermissions = EnableEntityPermissions
					};

					var relationship = new Relationship(ViewRelationshipName);

					var associateActionLink = new AssociateActionLink(new List<ViewConfiguration> { associateActionViewConfiguration }, relationship, portalContext, associateAction, languageCode, true, null, PortalName);

					AssociateActionLink = associateActionLink;

					viewActionLinks.Add(associateActionLink);
				}
			}

			ViewActionLinks = viewActionLinks;
		}

		private void SetItemActions(IPortalContext portalContext, JsonConfiguration.GridMetadata gridMetadata, int languageCode)
		{
			var actions = GetActions(gridMetadata);

			var itemActionLinks = new List<ViewActionLink>();
			var createRecordActions = new List<ViewActionLink>();

			foreach (var action in actions)
			{
				if (action is WorkflowAction)
				{
					var workflowAction = (WorkflowAction)action;

					if (!workflowAction.IsConfigurationValid()) continue;

					var workflowActionLink = new WorkflowActionLink(portalContext, new EntityReference("workflow", workflowAction.WorkflowId), gridMetadata, languageCode, workflowAction, true, null, PortalName);

					itemActionLinks.Add(workflowActionLink);
				}
				else if (action is DetailsAction)
				{
					var detailsAction = (DetailsAction)action;

					if (!detailsAction.IsConfigurationValid()) continue;

					var detailsActionLink = new DetailsActionLink(portalContext, gridMetadata, languageCode, detailsAction, true, PortalName);

					DetailsActionLink = detailsActionLink;

					itemActionLinks.Add(detailsActionLink);
				}
				else if (action is EditAction)
				{
					var editAction = (EditAction)action;

					if (!editAction.IsConfigurationValid()) continue;

					var editActionLink = new EditActionLink(portalContext, gridMetadata, languageCode, editAction, true, PortalName);

					EditActionLink = editActionLink;

					itemActionLinks.Add(editActionLink);
				}
				else if (action is DeleteAction)
				{
					var deleteAction = (DeleteAction)action;

					if (!deleteAction.IsConfigurationValid()) continue;

					var deleteActionLink = new DeleteActionLink(portalContext, gridMetadata, languageCode, deleteAction);

					DeleteActionLink = deleteActionLink;

					itemActionLinks.Add(deleteActionLink);
				}
				else if (action is DisassociateAction && !string.IsNullOrWhiteSpace(ViewRelationshipName))  //only applicable to subgrids that are showing related records
				{
					var disassociateAction = (DisassociateAction)action;

					if (!disassociateAction.IsConfigurationValid()) continue;

					var relationship = new Relationship(ViewRelationshipName);

					var disassociateActionLink = new DisassociateActionLink(relationship, portalContext, gridMetadata, languageCode, disassociateAction, true, null, PortalName);

					DisassociateActionLink = disassociateActionLink;

					itemActionLinks.Add(disassociateActionLink);
				}
				else if (action is CreateRelatedRecordAction)
				{
					
					var createRecordAction = (CreateRelatedRecordAction)action;

					if (!createRecordAction.IsConfigurationValid()) continue;

					var createRelatedRecordActionLink = new CreateRelatedRecordActionLink(portalContext, gridMetadata, languageCode, createRecordAction, true, PortalName);
					createRecordActions.Add(createRelatedRecordActionLink);

				}
				else
				{
					SetSpecialMessageActions(portalContext, gridMetadata, languageCode, action, itemActionLinks);
				}
			}

			ItemActionLinks = itemActionLinks;
			CreateRelatedRecordActionLinks = createRecordActions;
		}

		private static List<Action> GetActions(JsonConfiguration.GridMetadata gridMetadata)
		{
			var actions = (gridMetadata.ItemActions != null) ? gridMetadata.ItemActions.ToList() : new List<Action>();

			var extendedActions = (gridMetadata.ExtendedItemActions != null) ? gridMetadata.ExtendedItemActions.ToList() : new List<Action>();

			var combinedActions = new List<Action>();

			//now we need to merge them

			combinedActions.AddRange(actions);

			combinedActions.AddRange(extendedActions);

			return combinedActions.OrderBy(a => a.ActionIndex).ToList();
		}

		private void SetSpecialMessageActions(IPortalContext portalContext, JsonConfiguration.GridMetadata gridMetadata,
			int languageCode, Action action, List<ViewActionLink> itemActionLinks)
		{
			if (action is CloseIncidentAction)
			{
				var closeIncidentAction = (CloseIncidentAction)action;
				if (!closeIncidentAction.IsConfigurationValid()) return;
				var closeIncidentActionLink = new CloseIncidentActionLink(portalContext, gridMetadata, languageCode, closeIncidentAction, true, null, PortalName);
				CloseIncidentActionLink = closeIncidentActionLink;
				itemActionLinks.Add(closeIncidentActionLink);
			}

			if (action is ResolveCaseAction)
			{
				var resolveCaseAction = (ResolveCaseAction)action;
				if (!resolveCaseAction.IsConfigurationValid()) return;
				var resolveCaseActionLink = new ResolveCaseActionLink(portalContext, gridMetadata, languageCode, resolveCaseAction, true, null, PortalName);
				ResolveCaseActionLink = resolveCaseActionLink;
				itemActionLinks.Add(resolveCaseActionLink);
			}

			if (action is ReopenCaseAction)
			{
				var reopenCaseAction = (ReopenCaseAction)action;
				if (!reopenCaseAction.IsConfigurationValid()) return;
				var reopenCaseActionLink = new ReopenCaseActionLink(portalContext, gridMetadata, languageCode, reopenCaseAction, true, null, PortalName);
				ReopenCaseActionLink = reopenCaseActionLink;
				itemActionLinks.Add(reopenCaseActionLink);
			}

			if (action is CancelCaseAction)
			{
				var cancelCaseAction = (CancelCaseAction)action;
				if (!cancelCaseAction.IsConfigurationValid()) return;
				var cancelCaseActionLink = new CancelCaseActionLink(portalContext, gridMetadata, languageCode, cancelCaseAction, true, null, PortalName);
				CancelCaseActionLink = cancelCaseActionLink;
				itemActionLinks.Add(cancelCaseActionLink);
			}

			if (action is QualifyLeadAction)
			{
				var qualifyLeadAction = (QualifyLeadAction)action;
				if (!qualifyLeadAction.IsConfigurationValid()) return;
				var qualifyLeadActionLink = new QualifyLeadActionLink(portalContext, gridMetadata, languageCode, qualifyLeadAction, true, null, PortalName);
				QualifyLeadActionLink = qualifyLeadActionLink;
				itemActionLinks.Add(qualifyLeadActionLink);
			}

			if (action is ConvertQuoteToOrderAction)
			{
				var convertQuoteToOrderAction = (ConvertQuoteToOrderAction)action;
				if (!convertQuoteToOrderAction.IsConfigurationValid()) return;
				var convertQuoteToOrderActionLink = new ConvertQuoteToOrderActionLink(portalContext, gridMetadata, languageCode, convertQuoteToOrderAction, true, null, PortalName);
				ConvertQuoteToOrderActionLink = convertQuoteToOrderActionLink;
				itemActionLinks.Add(convertQuoteToOrderActionLink);
			}

			if (action is ConvertOrderToInvoiceAction)
			{
				var convertOrderToInvoiceAction = (ConvertOrderToInvoiceAction)action;
				if (!convertOrderToInvoiceAction.IsConfigurationValid()) return;
				var convertOrderToInvoiceActionLink = new ConvertOrderToInvoiceActionLink(portalContext, gridMetadata, languageCode, convertOrderToInvoiceAction, true, null, PortalName);
				ConvertOrderToInvoiceActionLink = convertOrderToInvoiceActionLink;
				itemActionLinks.Add(convertOrderToInvoiceActionLink);
			}

			if (action is DeactivateAction)
			{
				var deactivateAction = (DeactivateAction)action;
				if (!deactivateAction.IsConfigurationValid()) return;
				var deactivateActionLink = new DeactivateActionLink(portalContext, gridMetadata, languageCode, deactivateAction, true, null, PortalName);
				DeactivateActionLink = deactivateActionLink;
				itemActionLinks.Add(deactivateActionLink);
			}

			if (action is ActivateAction)
			{
				var activateAction = (ActivateAction)action;
				if (!activateAction.IsConfigurationValid()) return;
				var activateActionLink = new ActivateActionLink(portalContext, gridMetadata, languageCode, activateAction, true, null, PortalName);
				ActivateActionLink = activateActionLink;
				itemActionLinks.Add(activateActionLink);
			}

			if (action is ActivateQuoteAction)
			{
				var activateQuoteAction = (ActivateQuoteAction)action;
				if (!activateQuoteAction.IsConfigurationValid()) return;
				var activateQuoteActionLink = new ActivateQuoteActionLink(portalContext, gridMetadata, languageCode, activateQuoteAction, true, null, PortalName);
				ActivateQuoteActionLink = activateQuoteActionLink;
				itemActionLinks.Add(activateQuoteActionLink);
			}

			if (action is SetOpportunityOnHoldAction)
			{
				var setOpportunityOnHoldAction = (SetOpportunityOnHoldAction)action;
				if (!setOpportunityOnHoldAction.IsConfigurationValid()) return;
				var setOpportunityOnHoldActionLink = new SetOpportunityOnHoldActionLink(portalContext, gridMetadata, languageCode, setOpportunityOnHoldAction, true, null, PortalName);
				SetOpportunityOnHoldActionLink = setOpportunityOnHoldActionLink;
				itemActionLinks.Add(setOpportunityOnHoldActionLink);
			}

			if (action is ReopenOpportunityAction)
			{
				var reopenOpportunityAction = (ReopenOpportunityAction)action;
				if (!reopenOpportunityAction.IsConfigurationValid()) return;
				var reopenOpportunityActionLink = new ReopenOpportunityActionLink(portalContext, gridMetadata, languageCode, reopenOpportunityAction, true, null, PortalName);
				ReopenOpportunityActionLink = reopenOpportunityActionLink;
				itemActionLinks.Add(reopenOpportunityActionLink);
			}

			if (action is CalculateOpportunityAction)
			{
				var calculateOpportunityAction = (CalculateOpportunityAction)action;
				if (!calculateOpportunityAction.IsConfigurationValid()) return;
				var calculateOpportunityActionLink = new CalculateOpportunityActionLink(portalContext, gridMetadata, languageCode, calculateOpportunityAction, true, null, PortalName);
				CalculateOpportunityActionLink = calculateOpportunityActionLink;
				itemActionLinks.Add(calculateOpportunityActionLink);
			}

			if (action is WinOpportunityAction)
			{
				var winOpportunityAction = (WinOpportunityAction)action;
				if (!winOpportunityAction.IsConfigurationValid()) return;
				var winOpportunityActionLink = new WinOpportunityActionLink(portalContext, gridMetadata, languageCode, winOpportunityAction, true, null, PortalName);
				WinOpportunityActionLink = winOpportunityActionLink;
				itemActionLinks.Add(winOpportunityActionLink);
			}

			if (action is LoseOpportunityAction)
			{
				var loseOpportunityAction = (LoseOpportunityAction)action;
				if (!loseOpportunityAction.IsConfigurationValid()) return;
				var loseOpportunityActionLink = new LoseOpportunityActionLink(portalContext, gridMetadata, languageCode, loseOpportunityAction, true, null, PortalName);
				LoseOpportunityActionLink = loseOpportunityActionLink;
				itemActionLinks.Add(loseOpportunityActionLink);
			}

			if (action is GenerateQuoteFromOpportunityAction)
			{
				var generateQuoteFromOpportunityAction = (GenerateQuoteFromOpportunityAction)action;
				if (!generateQuoteFromOpportunityAction.IsConfigurationValid()) return;
				var generateQuoteFromOpportunityActionLink = new GenerateQuoteFromOpportunityActionLink(portalContext, gridMetadata, languageCode, generateQuoteFromOpportunityAction, true, null, PortalName);
				GenerateQuoteFromOpportunityActionLink = generateQuoteFromOpportunityActionLink;
				itemActionLinks.Add(generateQuoteFromOpportunityActionLink);
			}

			if (action is UpdatePipelinePhaseAction)
			{
				var updatePipelinePhaseAction = (UpdatePipelinePhaseAction)action;
				if (!updatePipelinePhaseAction.IsConfigurationValid()) return;
				var updatePipelinePhaseActionLink = new UpdatePipelinePhaseActionLink(portalContext, gridMetadata, languageCode, updatePipelinePhaseAction, true, null, PortalName);
				UpdatePipelinePhaseActionLink = updatePipelinePhaseActionLink;
				itemActionLinks.Add(updatePipelinePhaseActionLink);
			}
		}

	}
}
