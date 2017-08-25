/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
	/// <summary>
	/// Settings needed to be able to retrieve a view and configure its display.
	/// </summary>
	public interface IViewConfiguration
	{
		/// <summary>
		/// Logical name of the entity associated with the view.
		/// </summary>
		string EntityName { get; set; }
		/// <summary>
		/// Logical name of the primary key attribute of the entity associated with the view.
		/// </summary>
		string PrimaryKeyName { get; set; }
		/// <summary>
		/// Name of a view (savedquery) record.
		/// </summary>
		string ViewName { get; set; }
		/// <summary>
		/// Unique identifier of a view (savedquery) record.
		/// </summary>
		Guid ViewId { get; set; }
		/// <summary>
		/// Unique identifier of the configuration.
		/// </summary>
		Guid Id { get; set; }
		/// <summary>
		/// Number of records per page.
		/// </summary>
		int PageSize { get; set; }
		/// <summary>
		/// Controls the visibility of the pager.
		/// </summary>
		bool? DataPagerEnabled { get; set; }
		/// <summary>
		/// XML that defines the layout of the view expressed in the LayoutXml language. See http://msdn.microsoft.com/en-us/library/gg334522.aspx
		/// </summary>
		string LayoutXml { get; set; }
		/// <summary>
		/// XML that defines the query expressed in the FetchXml language. See http://msdn.microsoft.com/en-us/library/gg309405.aspx
		/// </summary>
		string FetchXml { get; set; }
		/// <summary>
		/// Override the display name of the view. Default display name uses the ViewName.
		/// </summary>
		string ViewDisplayName { get; set; }
		/// <summary>
		/// Override the column display names and widths
		/// </summary>
		List<JsonConfiguration.ViewColumn> ColumnOverrides { get; set; }
		/// <summary>
		/// Indicates whether entity permission rules should be applied to the query.
		/// </summary>
		bool EnableEntityPermissions { get; set; }
		/// <summary>
		/// Configuration of the search control.
		/// </summary>
		ViewSearch Search { get; set; }
		/// <summary>
		/// Gets or sets the Query String parameter name for the filter.
		/// </summary>
		string FilterQueryStringParameterName { get; set; }
		/// <summary>
		/// Gets or sets the Query String parameter name for the sort expression.
		/// </summary>
		string SortQueryStringParameterName { get; set; }
		/// <summary>
		/// Gets or sets the Query String parameter name for the page number.
		/// </summary>
		string PageQueryStringParameterName { get; set; }
		/// <summary>
		/// Gets or sets the text to display when the list does not contain any records.
		/// </summary>
		string EmptyListText { get; set; }
		/// <summary>
		/// Gets or sets the Text to display when the rendering a filter dropdown to select 'my' records.
		/// </summary>
		string FilterByUserOptionLabel { get; set; }
		/// <summary>
		/// Attribute logical name on the target entity of the lookup field of type contact that is used to assign the current portal user's contact id to filter the query results.
		/// </summary>
		string FilterPortalUserAttributeName { get; set; }
		/// <summary>
		/// Attribute logical name on the target entity of the lookup field of type account that is used to assign the current portal user's contact's parent customer account id to filter the query results.
		/// </summary>
		string FilterAccountAttributeName { get; set; }
		/// <summary>
		/// Attribute logical name on the target entity of the lookup field of type adx_website that is used to assign the current website id to filter the query results.
		/// </summary>
		string FilterWebsiteAttributeName { get; set; }
		/// <summary>
		/// Link for details action.
		/// </summary>
		DetailsActionLink DetailsActionLink { get; set; }
		/// <summary>
		/// Link for insert action.
		/// </summary>
		InsertActionLink InsertActionLink { get; set; }
		/// <summary>
		/// Link for association action.
		/// </summary>
		AssociateActionLink AssociateActionLink { get; set; }
		/// <summary>
		/// Link for Edit Action
		/// </summary>
		EditActionLink EditActionLink { get; set; }
		/// <summary>
		/// Link for Delete Action
		/// </summary>
		DeleteActionLink DeleteActionLink { get; set; }
		/// <summary>
		/// Link for Disassociate Action
		/// </summary>
		DisassociateActionLink DisassociateActionLink { get; set; }
		
		/// <summary>
		/// Text displayed for the column header of the column containing row level action links.
		/// </summary>
		string ActionColumnHeaderText { get; set; }
		/// <summary>
		/// Width in pixels of the column containing the action links to the details view page. Default is 20 pixels.
		/// </summary>
		int ActionLinksColumnWidth { get; set; }
		/// <summary>
		/// Gets or sets the name of the portal configuration that the control binds to.
		/// </summary>
		string PortalName { get; set; }
		/// <summary>
		/// Gets or sets the language code
		/// </summary>
		int LanguageCode { get; set; }
		/// <summary>
		/// Configuration settings for a map view
		/// </summary>
		MapConfiguration MapSettings { get; set; }
		/// <summary>
		/// Configuration settings for a calendar view
		/// </summary>
		CalendarConfiguration CalendarSettings { get; set; }
		/// <summary>
		/// Configuration settings for the metadata filter control.
		/// </summary>
		FilterConfiguration FilterSettings { get; set; }
		/// <summary>
		/// Css class for the View
		/// </summary>
		string CssClass { get; set; }
		/// <summary>
		/// Css class for the Grid
		/// </summary>
		string GridCssClass { get; set; }
		/// <summary>
		/// Column Width setting: pixels or percentage
		/// </summary>
		EntityGridExtensions.GridColumnWidthStyle? GridColumnWidthStyle { get; set; }
		/// <summary>
		/// Message to display while loading
		/// </summary>
		string LoadingMessage { get; set; }
		/// <summary>
		/// Error message
		/// </summary>
		string ErrorMessage { get; set; }
		/// <summary>
		/// Access Denied Message
		/// </summary>
		string AccessDeniedMessage { get; set; }

		/// <summary>
		/// Actions that are applicable to the view or entire record set.
		/// </summary>
		List<ViewActionLink> ViewActionLinks { get; set; }

		/// <summary>
		/// Gets or sets the create related record action link.
		/// </summary>
		/// <value>
		/// The create related record action link.
		/// </value>
		List<ViewActionLink> CreateRelatedRecordActionLinks { get; set; }

		/// <summary>
		/// Actions that are applicable to a single record item.
		/// </summary>
		List<ViewActionLink> ItemActionLinks { get; set; }

		string ModalLookupAttributeLogicalName { get; set; }

		string ModalLookupEntityLogicalName { get; set; }

		Guid? ModalLookupFormReferenceEntityId { get; set; }

		string ModalLookupFormReferenceEntityLogicalName { get; set; }

		string ModalLookupFormReferenceRelationshipName { get; set; }

		string ModalLookupFormReferenceRelationshipRole { get; set; }

		/// <summary>
		/// Gets the savedquery (view) for the corresponding view configuration properties.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="languageCode"></param>
		/// <returns><see cref="SavedQueryView"/></returns>
		SavedQueryView GetSavedQueryView(OrganizationServiceContext serviceContext, int languageCode = 0);

		/// <summary>
		/// Gets the current view from a collection of <see cref="SavedQueryView"/> records.
		/// </summary>
		/// <param name="views">collection of <see cref="SavedQueryView"/> records.</param>
		/// <returns><see cref="SavedQueryView"/></returns>
		SavedQueryView GetSavedQueryView(IEnumerable<SavedQueryView> views);
	}
}
