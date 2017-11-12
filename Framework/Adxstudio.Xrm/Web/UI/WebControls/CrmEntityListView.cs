/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Portal.Web;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web.Mvc.Html;
	using Adxstudio.Xrm.Web.UI.CrmEntityListView;
	using Adxstudio.Xrm.Web.UI.HtmlControls;
	using Adxstudio.Xrm.Web.UI.JsonConfiguration;
	using Filter = Adxstudio.Xrm.Services.Query.Filter;

	/// <summary>
	/// Renders a tabular lists of records within the portal.
	/// </summary>
	[Description("CrmEntityListView control displays tabular lists of records within the portal.")]
	[ToolboxData(@"<{0}:CrmEntityListView runat=""server""></{0}:CrmEntityListView>")]
	[DefaultProperty("")]
	public class CrmEntityListView : CompositeControl
	{
		/// <summary>
		/// ISO 8601 DateTime format string
		/// </summary>
		public const string DateTimeClientFormat = Globalization.DateTimeFormatInfo.ISO8601Pattern;

		/// <summary>
		/// Indicates whether or not the entity permission provider will add record level filters to the view's fetch query to assert privileges.
		/// </summary>
		[Description("Indicates whether or not the entity permission provider will add record level filters to the view's fetch query to assert privileges.")]
		public bool EnableEntityPermissions
		{
			get { return (bool)(ViewState["EnableEntityPermissions"] ?? false); }
			set { ViewState["EnableEntityPermissions"] = value; }
		}

		/// <summary>
		/// Gets or sets the Entity List Entity Reference.
		/// </summary>
		[Description("The Entity Reference of the Entity List to load.")]
		public EntityReference EntityListReference
		{
			get { return ((EntityReference)ViewState["EntityListReference"]); }
			set { ViewState["EntityListReference"] = value; }
		}

		/// <summary>
		/// Gets or sets the name of the portal configuration that the control binds to.
		/// </summary>
		[Description("The portal configuration that the control binds to.")] [DefaultValue("")]
		public string PortalName
		{
			get { return ((string)ViewState["PortalName"]) ?? string.Empty; }
			set { ViewState["PortalName"] = value; }
		}

		[Description("Language Code")]
		public int LanguageCode
		{
			get
			{
				// Entity lists only supports CRM languages, so use the CRM Lcid rather than the potentially custom language Lcid.
				return Context.GetCrmLcid();
			}
			set { }
		}

		[Description("The CSS Class assigned to the List.")] [DefaultValue("")]
		public string ListCssClass
		{
			get { return ((string)ViewState["ListCssClass"]) ?? string.Empty; }
			set { ViewState["ListCssClass"] = value; }
		}

		/// <summary>
		/// Gets or sets the Query String parameter name for the selected view.
		/// </summary>
		[Description("The Query String parameter name for the selected view.")] [DefaultValue("view")]
		public string ViewQueryStringParameterName
		{
			get { return ((string)ViewState["ViewQueryStringParameterName"]) ?? "view"; }
			set { ViewState["ViewQueryStringParameterName"] = value; }
		}

		/// <summary>
		/// Indicates whether the list is a gallery or not.
		/// </summary>
		[Description("Indicates whether the list is a gallery or not.")] [DefaultValue("false")]
		public bool IsGallery
		{
			get { return (bool)(ViewState["IsGallery"] ?? false); }
			set { ViewState["IsGallery"] = value; }
		}

		/// <summary>
		/// Configuration options for the error modal.
		/// </summary>
		public ViewErrorModal ErrorModal { get; set; }

		/// <summary>
		/// Collection of settings to get a view and configure its display.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty), MergableProperty(false), DefaultValue(null)] public
			IEnumerable<ViewConfiguration> ViewConfigurations;

		/// <summary>
		/// Determines the selection mode of the grid. None, single or multiple.
		/// </summary>
		public EntityGridExtensions.GridSelectMode SelectMode
		{
			get
			{
				return (EntityGridExtensions.GridSelectMode)(ViewState["SelectMode"] ?? EntityGridExtensions.GridSelectMode.None);
			}
			set { ViewState["SelectMode"] = value; }
		}

		/// <summary>
		/// Script files to be loaded.
		/// </summary>
		protected virtual string[] ScriptIncludes { get { return new[] { string.Empty }; } }

		/// <summary>
		/// Script files to be loaded for calendar view
		/// </summary>
		protected virtual string[] CalendarScriptIncludes
		{
			get
			{
				return new[]
				{
					"~/Areas/EntityList/js/calendar.min.js",
					"~/Areas/EntityList/js/entitylist-calendar.js"
				};
			}
		}

		/// <summary>
		/// Style stylesheet files to be loaded for the calendar view.
		/// </summary>
		protected virtual string[] CalendarStylesheets
		{
			get { return new[] { "~/Areas/EntityList/css/calendar.min.css", "~/Areas/EntityList/css/entitylist-calendar.css" }; }
		}

		/// <summary>
		/// Script files to be loaded for the map view.
		/// </summary>
		protected virtual string[] MapScriptIncludes
		{
			get { return new[] { "~/xrm-adx/js/json2.min.js", "https://www.bing.com/api/maps/mapcontrol", "~/xrm-adx/js/entitylist-map.js" }; }
		}

		protected ViewConfiguration CurrentViewConfiguration;

		protected IEnumerable<EntityView> Views;

		protected EntityView CurrentView;

		protected override HtmlTextWriterTag TagKey { get { return HtmlTextWriterTag.Div; } }

		protected override void CreateChildControls()
		{
			Controls.Clear();

			CssClass = "entitylist";

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			if (LanguageCode <= 0)
			{
				LanguageCode = this.Context.GetPortalSolutionsDetails().OrganizationBaseLanguageCode;
			}

			RegisterClientSideDependencies(this);

			CurrentViewConfiguration = GetCurrentViewConfiguration(GetSelectedViewId());

			Views = GetViews(serviceContext).Select(v => new EntityView(serviceContext, v, LanguageCode));

			CurrentView = CurrentViewConfiguration.GetEntityView(Views);

			if (EntityListReference != null && IsGallery)
			{
				RenderToolbar(serviceContext, portalContext, this);

				RenderGallery(portalContext, this);
			}
			else if (EntityListReference != null && CurrentViewConfiguration.MapSettings != null &&
				CurrentViewConfiguration.MapSettings.Enabled && PortalSettings.Instance.BingMapsSupported)
			{
				RenderMap(serviceContext);
			}
			else if (EntityListReference != null && CurrentViewConfiguration.CalendarSettings != null &&
				CurrentViewConfiguration.CalendarSettings.Enabled)
			{
				RenderToolbar(serviceContext, portalContext, this);

				RenderCalendar(portalContext, this);
			}
			else
			{
				if (CurrentViewConfiguration.FilterSettings != null
					&& CurrentViewConfiguration.FilterSettings.Enabled
					&& CurrentViewConfiguration.FilterSettings.Orientation == FilterConfiguration.FilterOrientation.Vertical)
				{
					RenderFiltersVertical(serviceContext, portalContext, this);
				}
				else
				{
					RenderFilters(serviceContext, portalContext, this);

					RenderList(serviceContext, this);
				}
			}
		}

		/// <summary>
		/// Get the savedquery view records for the view configurations that have been defined.
		/// </summary>
		/// <param name="serviceContext"></param>
		protected virtual IEnumerable<Entity> GetViews(OrganizationServiceContext serviceContext)
		{
			var viewIds = this.ViewConfigurations.Where(v => v.ViewId != Guid.Empty).Select(v => v.ViewId).Cast<object>().ToArray();
			var viewNames = this.ViewConfigurations.Where(v => !string.IsNullOrWhiteSpace(v.ViewName))
					.Select(v => new Tuple<string, string>(v.EntityName, v.ViewName))
					.ToList();

			var fetch = new Fetch
			{
				Entity = new FetchEntity("savedquery")
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition { Attribute = "savedqueryid", Operator = ConditionOperator.In, Values = viewIds }
							}
						}
					}
				}
			};

			var viewsById = !viewIds.Any()
				? new List<Entity>()
				: serviceContext.RetrieveMultiple(fetch).Entities.ToList();

			var viewsByName = serviceContext.CreateQuery("savedquery").FilterByNames(viewNames).ToList();
			var views = viewsById.Union(viewsByName);
			return views;
		}

		/// <summary>
		/// Gets the ID of the selected view from the query string.
		/// </summary>
		/// <returns></returns>
		protected virtual Guid GetSelectedViewId()
		{
			Guid selectedViewId;
			Guid.TryParse(HttpContext.Current.Request[ViewQueryStringParameterName] ?? string.Empty, out selectedViewId);
			return selectedViewId;
		}

		/// <summary>
		/// Gets the current view configuration. IF no view has been selected, then the first configuration is returned.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected virtual ViewConfiguration GetCurrentViewConfiguration(Guid id = new Guid())
		{
			var currentViewConfiguration = ViewConfigurations.First();
			if (id != Guid.Empty)
			{
				var config = ViewConfigurations.FirstOrDefault(o => o.Id == id);
				if (config != null)
				{
					currentViewConfiguration = config;
				}
			}
			return currentViewConfiguration;
		}

		/// <summary>
		/// Generates a vertical column of filters beside the entity list.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="portalContext"></param>
		/// <param name="container"></param>
		protected virtual void RenderFiltersVertical(OrganizationServiceContext serviceContext, IPortalContext portalContext,
			Control container)
		{
			var columnFilter = new HtmlGenericControl("div");
			columnFilter.Attributes.Add("class", "col-md-3 filter-vertical");
			RenderFilters(serviceContext, portalContext, columnFilter);

			var columnEntityList = new HtmlGenericControl("div");
			columnEntityList.Attributes.Add("class", "col-md-9");

			RenderList(serviceContext, columnEntityList);

			var row = new HtmlGenericControl("div");
			row.Attributes.Add("class", "row");
			row.Controls.Add(columnFilter);
			row.Controls.Add(columnEntityList);

			container.Controls.Add(row);
		}

		/// <summary>
		/// Generates a toolbar above the list with with actions scoped to the entire list.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="portalContext"></param>
		/// <param name="container"></param>
		protected virtual void RenderToolbar(OrganizationServiceContext serviceContext, IPortalContext portalContext,
			Control container)
		{
			var toolbar = new HtmlGenericControl("div");
			toolbar.Attributes.Add("class", "clearfix grid-actions");

			var nav = new HtmlGenericControl("ul");
			nav.Attributes.Add("class", "nav nav-pills pull-left");

			var addViewSelection = TryRenderViewSelectionControl(serviceContext, portalContext, nav);

			var addFilter = TryRenderFilterControl(portalContext, nav);

			if (addViewSelection || addFilter)
			{
				toolbar.Controls.Add(nav);
			}

			var addSearch = TryRenderSearchControl(toolbar);

			var addToolbar = addViewSelection || addFilter || addSearch;

			if (addToolbar)
			{
				container.Controls.Add(toolbar);
			}
		}

		/// <summary>
		/// Render the currently selected view as a list.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="container"></param>
		/// <returns></returns>
		protected virtual void RenderList(OrganizationServiceContext context, Control container)
		{
			var gridHtml = BuildGrid(context);
			var gridControl = new HtmlGenericControl("div") { InnerHtml = gridHtml.ToString() };
			container.Controls.Add(gridControl);
		}

		/// <summary>
		/// Display a view selection dropdown if multiple views are specified.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="portalContext"></param>
		/// <param name="container"></param>
		/// <returns></returns>
		protected virtual bool TryRenderViewSelectionControl(OrganizationServiceContext context, IPortalContext portalContext,
			Control container)
		{
			if (Views.Count() <= 1)
			{
				return false;
			}

			var currentViewName = CurrentView == null
				? "Select a view"
				: string.IsNullOrWhiteSpace(CurrentViewConfiguration.ViewDisplayName)
					? CurrentView.Name
					: CurrentViewConfiguration.ViewDisplayName;

			var li = new HtmlGenericControl("li");
			li.Attributes.Add("class", "dropdown");

			var link = new HtmlGenericControl("a");

			link.Attributes.Add("href", "#");
			link.Attributes.Add("class", "dropdown-toggle");
			link.Attributes.Add("data-toggle", "dropdown");

			link.InnerHtml =
				@"<span class=""fa fa-list"" aria-hidden=""true""></span> <span class=""title"">{0}</span> <span class=""caret"" aria-hidden=""true""></span>"
					.FormatWith(currentViewName);

			var ulMenu = new HtmlGenericControl("ul");
			ulMenu.Attributes.Add("class", "dropdown-menu");

			foreach (var config in ViewConfigurations)
			{
				var view = config.GetEntityView(Views);
				if (view == null)
				{
					continue;
				}

				var liView = new HtmlGenericControl("li");
				var linkView = new HtmlGenericControl("a");

				linkView.Attributes.Add("href", GetLinkUrl(ViewQueryStringParameterName, config.Id.ToString(), true));

				linkView.InnerHtml = string.IsNullOrWhiteSpace(config.ViewDisplayName) ? view.Name : config.ViewDisplayName;

				if (IsGallery && EntityListReference != null)
				{
					linkView.Attributes["data-gallery-nav"] = "view";
					linkView.Attributes["data-gallery-nav-value"] = GetGalleryServiceUrl(portalContext, EntityListReference.Id, view.Id);
				}

				if (config.CalendarSettings.Enabled && EntityListReference != null)
				{
					linkView.Attributes["data-calendar-nav"] = "view";
					linkView.Attributes["data-calendar-nav-value"] = GetCalendarServiceUrl(portalContext, EntityListReference.Id,
						view.Id);
					linkView.Attributes["data-calendar-nav-download"] = GetCalendarDownloadServiceUrl(portalContext,
						EntityListReference.Id, view.Id);
				}

				if (CurrentView != null && view.Id == CurrentView.Id)
				{
					liView.Attributes.Add("class", "active");
				}

				liView.Controls.Add(linkView);
				ulMenu.Controls.Add(liView);
			}

			li.Controls.Add(link);
			li.Controls.Add(ulMenu);

			container.Controls.Add(li);

			return true;
		}

		/// <summary>
		/// Display a filter dropdown if user AND account are filter options.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="container"></param>
		/// <returns></returns>
		protected virtual bool TryRenderFilterControl(IPortalContext context, Control container)
		{
			var filterBy = HttpContext.Current.Request[CurrentViewConfiguration.FilterQueryStringParameterName];

			if (string.IsNullOrWhiteSpace(filterBy))
			{
				filterBy = !string.IsNullOrWhiteSpace(CurrentViewConfiguration.FilterPortalUserAttributeName) ? "user" : "account";
			}

			var filterbyPortalUser = !string.IsNullOrWhiteSpace(CurrentViewConfiguration.FilterPortalUserAttributeName) &&
				filterBy == "user" ||
				(string.IsNullOrWhiteSpace(CurrentViewConfiguration.FilterAccountAttributeName) &&
					!string.IsNullOrWhiteSpace(CurrentViewConfiguration.FilterPortalUserAttributeName));

			var user = context.User;

			if (string.IsNullOrWhiteSpace(CurrentViewConfiguration.FilterPortalUserAttributeName) &&
				string.IsNullOrWhiteSpace(CurrentViewConfiguration.FilterAccountAttributeName))
			{
				return false;
			}

			if (user == null)
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(CurrentViewConfiguration.FilterPortalUserAttributeName) ||
				string.IsNullOrWhiteSpace(CurrentViewConfiguration.FilterAccountAttributeName))
			{
				return false;
			}

			if (user.LogicalName != "contact")
			{
				return false;
			}

			var parentcustomerid = user.GetAttributeValue<EntityReference>("parentcustomerid");

			if (parentcustomerid == null)
			{
				return false;
			}

			var accountname = parentcustomerid.Name;

			var currentFilterLabel = filterbyPortalUser ? CurrentViewConfiguration.FilterByUserOptionLabel : accountname;

			var li = new HtmlGenericControl("li");
			li.Attributes.Add("class", "dropdown");

			var link = new HtmlGenericControl("a");
			link.Attributes.Add("href", "#");
			link.Attributes.Add("class", "dropdown-toggle");
			link.Attributes.Add("data-toggle", "dropdown");
			link.InnerHtml =
				@"<span class=""fa fa-filter"" aria-hidden=""true""></span> <span class=""title"">{0}</span> <span class=""caret"" aria-hidden=""true""></span>"
					.FormatWith(currentFilterLabel);

			var ulMenu = new HtmlGenericControl("ul");
			ulMenu.Attributes.Add("class", "dropdown-menu");

			var liUser = new HtmlGenericControl("li");
			var linkUser = new HtmlGenericControl("a");
			linkUser.Attributes.Add("href", GetLinkUrl(CurrentViewConfiguration.FilterQueryStringParameterName, "user"));
			linkUser.InnerText = CurrentViewConfiguration.FilterByUserOptionLabel;

			if (IsGallery)
			{
				linkUser.Attributes["data-gallery-nav"] = "filter";
				linkUser.Attributes["data-gallery-nav-value"] = "user";
			}

			if (CurrentViewConfiguration.CalendarSettings.Enabled)
			{
				linkUser.Attributes["data-calendar-nav"] = "filter";
				linkUser.Attributes["data-calendar-nav-value"] = "user";
			}

			liUser.Controls.Add(linkUser);

			var liAccount = new HtmlGenericControl("li");
			var linkAccount = new HtmlGenericControl("a");
			linkAccount.Attributes.Add("href", GetLinkUrl(CurrentViewConfiguration.FilterQueryStringParameterName, "account"));
			linkAccount.InnerText = accountname;

			if (IsGallery)
			{
				linkAccount.Attributes["data-gallery-nav"] = "filter";
				linkAccount.Attributes["data-gallery-nav-value"] = "account";
			}

			if (CurrentViewConfiguration.CalendarSettings.Enabled)
			{
				linkAccount.Attributes["data-calendar-nav"] = "filter";
				linkAccount.Attributes["data-calendar-nav-value"] = "account";
			}

			liAccount.Controls.Add(linkAccount);

			ulMenu.Controls.Add(liUser);
			ulMenu.Controls.Add(liAccount);
			li.Controls.Add(link);
			li.Controls.Add(ulMenu);

			container.Controls.Add(li);

			return true;
		}

		/// <summary>
		/// Render a search control if enabled.
		/// </summary>
		/// <param name="container"></param>
		/// <returns></returns>
		protected virtual bool TryRenderSearchControl(Control container)
		{
			if (CurrentViewConfiguration.Search == null || !CurrentViewConfiguration.Search.Enabled)
			{
				return false;
			}

			var query = HttpContext.Current.Request[CurrentViewConfiguration.Search.SearchQueryStringParameterName] ??
				string.Empty;

			var div = new HtmlGenericControl("div");
			div.Attributes.Add("class", "input-group pull-right entitylist-search");

			var input = new TextBox { ID = string.Format("{0}_{1}", ID, "Search"), Text = query, CssClass = "query form-control" };
			input.Attributes.Add("title", CurrentViewConfiguration.Search.TooltipText);
			input.Attributes.Add("placeholder", CurrentViewConfiguration.Search.PlaceholderText);

			var buttonContainer = new HtmlGenericControl("div");
			buttonContainer.Attributes["class"] = "input-group-btn";

			var button = new HtmlButton { InnerHtml = CurrentViewConfiguration.Search.ButtonLabel };
			button.ServerClick += SearchButton_Click;
			button.Attributes.Add("class", "btn btn-default");

			if (IsGallery)
			{
				button.Attributes["data-gallery-nav"] = "search";
			}

			if (CurrentViewConfiguration.CalendarSettings.Enabled)
			{
				button.Attributes["data-calendar-nav"] = "search";
			}

			buttonContainer.Controls.Add(button);
			div.Controls.Add(input);
			div.Controls.Add(buttonContainer);

			container.Controls.Add(div);

			return true;
		}

		/// <summary>
		/// Search button click event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void SearchButton_Click(object sender, EventArgs e)
		{
			var input = FindControl(string.Format("{0}_{1}", ID, "Search")) as TextBox;

			if (input == null)
			{
				return;
			}

			var query = input.Text;

			var urlBuilder = new UrlBuilder(HttpContext.Current.Request.Url.PathAndQuery);

			// return to the first page
			urlBuilder.QueryString.Remove(CurrentViewConfiguration.PageQueryStringParameterName);

			if (string.IsNullOrWhiteSpace(query))
			{
				urlBuilder.QueryString.Remove(CurrentViewConfiguration.Search.SearchQueryStringParameterName);
			}
			else
			{
				urlBuilder.QueryString.Set(CurrentViewConfiguration.Search.SearchQueryStringParameterName, query);
			}

			HttpContext.Current.Response.Redirect(urlBuilder.PathWithQueryString);
		}

		/// <summary>
		/// Builds a URL for an action link
		/// </summary>
		/// <param name="parameterName"></param>
		/// <param name="parameterValue"></param>
		/// <param name="clearQueryString"></param>
		/// <returns></returns>
		protected static string GetLinkUrl(string parameterName, string parameterValue, bool clearQueryString = false)
		{
			if (string.IsNullOrWhiteSpace(parameterValue))
			{
				return HttpContext.Current.Request.Url.PathAndQuery;
			}

			var urlBuilder = new UrlBuilder(HttpContext.Current.Request.Url.PathAndQuery);

			if (clearQueryString)
			{
				urlBuilder.QueryString.Clear();
			}

			urlBuilder.QueryString.Set(parameterName, parameterValue);

			return urlBuilder.PathWithQueryString;
		}

		/// <summary>
		/// Display a map view. Requires an <see cref="EntityListReference"/>.
		/// </summary>
		/// <param name="context"></param>
		protected void RenderMap(OrganizationServiceContext context)
		{
			if (EntityListReference == null || CurrentViewConfiguration.MapSettings == null ||
				!CurrentViewConfiguration.MapSettings.Enabled)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(CurrentViewConfiguration.MapSettings.Credentials))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application,
					"Map Credentials have not been specified. Map could not be rendered.");

				return;
			}

			if (string.IsNullOrWhiteSpace(CurrentViewConfiguration.MapSettings.RestUrl))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application,
					"Map REST URL has not been specified. Map could not be rendered.");

				return;
			}

			RegisterClientSideMapDependencies(this);

			var containerElement = new HtmlGenericControl("div");
			containerElement.Attributes.Add("class", "row clearfix");

			var optionsContainerElement = new HtmlGenericControl("div");
			optionsContainerElement.Attributes.Add("id", "entity-list-map-options");
			optionsContainerElement.Attributes.Add("class", "span12 col-md-12 form-inline");
			var locationLabelSnippet = new Snippet
			{
				SnippetName = "EntityList/Map/LocationInputLabel",
				DisplayName = "Location Input Label",
				EditType = "text",
				Editable = true,
				DefaultText = ResourceManager.GetString("Search_For_Location_Near_DefaultText")
			};
			var locationElement = new HtmlInputText();
			locationElement.Attributes.Add("id", "entity-list-map-location");
			locationElement.Attributes.Add("class", "form-control");
			locationElement.Attributes.Add("placeholder", "address");
			optionsContainerElement.Controls.Add(locationLabelSnippet);
			optionsContainerElement.Controls.Add(locationElement);
			var distanceLabelSnippet = new Snippet
			{
				SnippetName = "EntityList/Map/DistanceInputLabel",
				DisplayName = "Display Input Label",
				EditType = "text",
				Editable = true,
				DefaultText = ResourceManager.GetString("With_In")
			};
			optionsContainerElement.Controls.Add(distanceLabelSnippet);
			var distanceElement = new DropDownList { ID = "entity-list-map-distance", CssClass = "form-control" };
			foreach (var distanceItem in CurrentViewConfiguration.MapSettings.DistanceValues)
			{
				distanceElement.Items.Add(
					new ListItem(
						string.Format("{0} {1}", distanceItem, CurrentViewConfiguration.MapSettings.DistanceUnit.ToString().ToLower()),
						distanceItem.ToString(CultureInfo.InvariantCulture)));
			}
			optionsContainerElement.Controls.Add(distanceElement);
			var searchButton = new HtmlButton { ID = "entity-list-map-search" };
			var searchButtonSnippet = new Snippet
			{
				SnippetName = "EntityList/Map/SearchButtonLabel",
				DisplayName = "Search Button Label",
				EditType = "text",
				Editable = true,
				DefaultText = ResourceManager.GetString("Search_DefaultText")
			};
			searchButton.Controls.Add(searchButtonSnippet);
			searchButton.Attributes.Add("class", "btn btn-primary");
			optionsContainerElement.Controls.Add(new LiteralControl("&nbsp;"));
			optionsContainerElement.Controls.Add(searchButton);

			var mapContainerElement = new HtmlGenericControl("div");
			mapContainerElement.Attributes.Add("id", "entity-list-map-container");
			mapContainerElement.Attributes.Add("class", "span8 col-md-8");
			var mapElement = new HtmlGenericControl("div");
			mapElement.Attributes.Add("id", "entity-list-map");
			mapElement.Attributes.Add("data-credentials", CurrentViewConfiguration.MapSettings.Credentials);
			mapElement.Attributes.Add("data-rest-url", CurrentViewConfiguration.MapSettings.RestUrl);
			mapElement.Attributes.Add("data-zoom",
				CurrentViewConfiguration.MapSettings.DefaultZoom.ToString(CultureInfo.InvariantCulture));
			mapElement.Attributes.Add("data-latitude",
				CurrentViewConfiguration.MapSettings.DefaultCenterLatitude.ToString("N",
					new NumberFormatInfo { NumberDecimalDigits = 5 }));
			mapElement.Attributes.Add("data-longitude",
				CurrentViewConfiguration.MapSettings.DefaultCenterLongitude.ToString("N",
					new NumberFormatInfo { NumberDecimalDigits = 5 }));
			mapElement.Attributes.Add("data-pushpin-url", CurrentViewConfiguration.MapSettings.PinImageUrl);
			mapElement.Attributes.Add("data-pushpin-width",
				CurrentViewConfiguration.MapSettings.PinImageWidth.ToString(CultureInfo.InvariantCulture));
			mapElement.Attributes.Add("data-pushpin-height",
				CurrentViewConfiguration.MapSettings.PinImageHeight.ToString(CultureInfo.InvariantCulture));
			mapElement.Attributes.Add("data-infobox-offset-x",
				CurrentViewConfiguration.MapSettings.InfoboxOffsetX.ToString(CultureInfo.InvariantCulture));
			mapElement.Attributes.Add("data-infobox-offset-y",
				CurrentViewConfiguration.MapSettings.InfoboxOffsetY.ToString(CultureInfo.InvariantCulture));
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var searchUrl = WebsitePathUtility.ToAbsolute(portalContext.Website, CurrentViewConfiguration.MapSettings.SearchUrl);
			mapElement.Attributes.Add("data-search-url", searchUrl);
			mapElement.Attributes.Add("data-entity-list-id", EntityListReference.Id.ToString());
			mapElement.Attributes.Add("data-view-id", CurrentView.Id.ToString());
			mapElement.Attributes.Add("data-distance-units", CurrentViewConfiguration.MapSettings.DistanceUnit.ToString().ToLower());
			mapContainerElement.Controls.Add(mapElement);

			var locationsContainerElement = new HtmlGenericControl("div");
			locationsContainerElement.Attributes.Add("id", "entity-list-map-locations");
			locationsContainerElement.Attributes.Add("class", "span4 col-md-4");
			var errorMessageElement = new HtmlGenericControl("div");
			errorMessageElement.Attributes.Add("id", "entity-list-map-error");
			errorMessageElement.Attributes.Add("class", "alert alert-block alert-danger");
			errorMessageElement.Controls.Add(new HtmlGenericControl("p"));
			locationsContainerElement.Controls.Add(errorMessageElement);
			var locationsTableElement = new HtmlGenericControl("table");
			locationsTableElement.Attributes.Add("class", "table table-hover unstyled");
			var locationsTableBodyElement = new HtmlGenericControl("tbody");
			locationsTableElement.Controls.Add(locationsTableBodyElement);
			locationsContainerElement.Controls.Add(locationsTableElement);
			var directionsElement = new HtmlGenericControl("div");
			directionsElement.Attributes.Add("id", "entity-list-map-directions");
			containerElement.Controls.Add(optionsContainerElement);
			containerElement.Controls.Add(mapContainerElement);
			containerElement.Controls.Add(locationsContainerElement);
			containerElement.Controls.Add(directionsElement);
			Controls.Add(containerElement);

			var getDirectionsDialogElement = new HtmlGenericControl("section");
			getDirectionsDialogElement.Attributes.Add("id", "entity-list-map-directions-dialog");
			getDirectionsDialogElement.Attributes.Add("class", "modal");
			getDirectionsDialogElement.Attributes.Add("tabindex", "-1");
			getDirectionsDialogElement.Attributes.Add("role", "dialog");
			getDirectionsDialogElement.Attributes.Add("aria-labelledby", "entity-list-map-directions-dialog-label");
			getDirectionsDialogElement.Attributes.Add("aria-hidden", "true");

			var getDirectionsDialogContainer = new HtmlGenericControl("div");
			getDirectionsDialogContainer.Attributes["class"] = "modal-dialog";

			getDirectionsDialogElement.Controls.Add(getDirectionsDialogContainer);

			var getDirectionsDialogContent = new HtmlGenericControl("div");
			getDirectionsDialogContent.Attributes["class"] = "modal-content";

			getDirectionsDialogContainer.Controls.Add(getDirectionsDialogContent);

			var headerElement = new HtmlGenericControl("div");
			headerElement.Attributes.Add("class", "modal-header");
			var dismissButton = new HtmlButton { InnerHtml = "&times;" };
			dismissButton.Attributes.Add("class", "close");
			dismissButton.Attributes.Add("data-dismiss", "modal");
			dismissButton.Attributes.Add("aria-hidden", "true");
			var title = new HtmlGenericControl("h1");
			title.Attributes.Add("id", "entity-list-map-directions-dialog-label");
			title.Attributes.Add("class", "modal-title h4");
			var getDirectionsDialogTitleSnippet = new Snippet
			{
				SnippetName = "EntityList/Map/GetDirections/Title",
				DisplayName = "Get Directions Title",
				EditType = "text",
				Editable = true,
				DefaultText = ResourceManager.GetString("Get_Directions_DefaultText")
			};
			title.Controls.Add(getDirectionsDialogTitleSnippet);
			headerElement.Controls.Add(dismissButton);
			headerElement.Controls.Add(title);
			getDirectionsDialogContent.Controls.Add(headerElement);

			var bodyElement = new HtmlGenericControl("div");
			bodyElement.Attributes.Add("class", "modal-body form-horizontal");
			var controlGroup1 = new HtmlGenericControl("div");
			controlGroup1.Attributes.Add("class", "control-group form-group");

			var directionsFromLabel = new HtmlGenericControl("label");
			directionsFromLabel.Attributes["class"] = "control-label col-sm-2";
			directionsFromLabel.Attributes["for"] = "entity-list-map-directions-from";

			var directionsFromSnippet = new Snippet
			{
				SnippetName = "EntityList/Map/GetDirections/FromLabel",
				DisplayName = "Get Directions From Label",
				EditType = "text",
				Editable = true,
				DefaultText = ResourceManager.GetString("From_DefaultText"),
			};

			directionsFromLabel.Controls.Add(directionsFromSnippet);
			controlGroup1.Controls.Add(directionsFromLabel);
			var controls1 = new HtmlGenericControl("div");
			controls1.Attributes.Add("class", "controls col-sm-8");
			var directionsFromInputElement = new HtmlInputText();
			directionsFromInputElement.Attributes.Add("id", "entity-list-map-directions-from");
			directionsFromInputElement.Attributes.Add("class", "form-control");
			controls1.Controls.Add(directionsFromInputElement);

			var reverseDirectionsColumn = new HtmlGenericControl("div");
			reverseDirectionsColumn.Attributes["class"] = "col-sm-2";
			var reverseDirectionsButton = new HyperLink
			{
				ID = "entity-list-map-directions-reverse",
				NavigateUrl = "#",
				CssClass = "btn btn-default"
			};
			reverseDirectionsButton.Controls.Add(new LiteralControl(@"<span class=""fa fa-refresh"" aria-hidden=""true""></span>"));
			reverseDirectionsColumn.Controls.Add(reverseDirectionsButton);

			controlGroup1.Controls.Add(controls1);
			controlGroup1.Controls.Add(reverseDirectionsColumn);
			bodyElement.Controls.Add(controlGroup1);

			var controlGroup2 = new HtmlGenericControl("div");
			controlGroup2.Attributes.Add("class", "control-group form-group");

			var directionsToLabel = new HtmlGenericControl("label");
			directionsToLabel.Attributes["class"] = "control-label col-sm-2";
			directionsToLabel.Attributes["for"] = "entity-list-map-directions-to";

			var directionsToSnippet = new Snippet
			{
				SnippetName = "EntityList/Map/GetDirections/ToLabel",
				DisplayName = "Get Directions To Label",
				EditType = "text",
				Editable = true,
				DefaultText = ResourceManager.GetString("To_DefaultText"),
			};

			directionsToLabel.Controls.Add(directionsToSnippet);
			controlGroup2.Controls.Add(directionsToLabel);
			var controls2 = new HtmlGenericControl("div");
			controls2.Attributes.Add("class", "controls col-sm-8");

			var directionsToInputElement = new HtmlInputText();
			directionsToInputElement.Attributes.Add("id", "entity-list-map-directions-to");
			directionsToInputElement.Attributes.Add("class", "form-control");
			controls2.Controls.Add(directionsToInputElement);
			controlGroup2.Controls.Add(controls2);
			bodyElement.Controls.Add(controlGroup2);
			getDirectionsDialogContent.Controls.Add(bodyElement);

			var footerElement = new HtmlGenericControl("div");
			footerElement.Attributes.Add("class", "modal-footer");

			var latitudeElement = new HtmlInputHidden();
			latitudeElement.Attributes.Add("id", "entity-list-map-directions-latitude");
			footerElement.Controls.Add(latitudeElement);
			var longitudeElement = new HtmlInputHidden();
			longitudeElement.Attributes.Add("id", "entity-list-map-directions-longitude");
			footerElement.Controls.Add(longitudeElement);
			var getDirectionsButton = new HtmlButton { ID = "entity-list-map-directions-get" };
			getDirectionsButton.Attributes.Add("class", "btn btn-primary");
			var getDirectionsButtonSnippet = new Snippet
			{
				SnippetName = "EntityList/Map/GetDirections/ButtonLabel",
				DisplayName = "Get Directions Button Label",
				EditType = "text",
				Editable = true,
				DefaultText = ResourceManager.GetString("Get_Directions_DefaultText")
			};
			getDirectionsButton.Controls.Add(getDirectionsButtonSnippet);
			footerElement.Controls.Add(getDirectionsButton);
			getDirectionsDialogContent.Controls.Add(footerElement);

			Controls.Add(getDirectionsDialogElement);
		}

		private void RenderGallery(IPortalContext portalContext, Control container)
		{
			Attributes["data-gallery-url"] = GetGalleryServiceUrl(portalContext, EntityListReference.Id, CurrentView.Id);
			Attributes["data-view"] = "gallery";

			var gallery = new HtmlGenericControl("div");

			gallery.Attributes["class"] = "view";

			container.Controls.Add(gallery);
		}

		private void RenderCalendar(IPortalContext portalContext, Control container)
		{
			Attributes["data-calendar-url"] = GetCalendarServiceUrl(portalContext, EntityListReference.Id, CurrentView.Id);
			Attributes["data-calendar-download-url"] = GetCalendarDownloadServiceUrl(portalContext, EntityListReference.Id,
				CurrentView.Id);
			Attributes["data-view"] = "calendar";

			RegisterClientSideCalendarDependencies(this, portalContext.Website.Id);

			var calendar = new HtmlGenericControl("div");

			calendar.Attributes["class"] = "view";

			container.Controls.Add(calendar);

			Attributes["data-calendar-style"] = CurrentViewConfiguration.CalendarSettings.Style.ToString().ToLower();
			Attributes["data-calendar-initial-view"] = CurrentViewConfiguration.CalendarSettings.InitialView.ToString().ToLower();
			Attributes["data-calendar-initial-date"] = CurrentViewConfiguration.CalendarSettings.InitialDateString;
		}

		/// <summary>
		/// Display the filter control.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="portalContext"></param>
		/// <param name="container"></param>
		/// <returns></returns>
		protected void RenderFilters(OrganizationServiceContext context, IPortalContext portalContext, Control container)
		{
			if (CurrentViewConfiguration.FilterSettings == null || !CurrentViewConfiguration.FilterSettings.Enabled)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(CurrentViewConfiguration.FilterSettings.Definition))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application,
					"Filter definition is not specified. Filter control cannot be rendered.");

				return;
			}

			var metadataFilterParameterValue =
				HttpContext.Current.Request[CurrentViewConfiguration.FilterSettings.FilterQueryStringParameterName];
			var query = !string.IsNullOrWhiteSpace(metadataFilterParameterValue)
				? HttpUtility.ParseQueryString(metadataFilterParameterValue)
				: new NameValueCollection();

			var containerElement = new HtmlGenericControl("div") { ID = "EntityListFilterControl" };
			containerElement.Attributes.Add("class", "content-panel panel panel-default entitylist-filter");

			var filterGroupPanel = new HtmlGenericControl("div");
			filterGroupPanel.Attributes.Add("class", "panel-body");
			containerElement.Controls.Add(filterGroupPanel);

			// set orientation

			var orientation = CurrentViewConfiguration.FilterSettings.Orientation ==
				FilterConfiguration.FilterOrientation.Vertical
				? "list-unstyled"
				: "list-inline";

			var heading = new HtmlGenericControl("h3");
			heading.Attributes.Add("class", "sr-only");
			heading.InnerHtml = "Entity List Filters"; // this should maybe be a setting on the entitylist? Not sure though because I also don't want to surface it. It's pretty important for assistive tech though.

			var filterGroupList = new HtmlGenericControl("ul");
			filterGroupList.Attributes.Add("class", orientation);
			filterGroupList.Attributes.Add("role", "presentation");
			filterGroupPanel.Controls.Add(filterGroupList);

			// parse filter definition

			var fetch = Fetch.FromJson(CurrentViewConfiguration.FilterSettings.Definition);
			var filters = FilterOptionGroup.ToFilterOptionGroups(context, portalContext, CurrentView.EntityMetadata, fetch.Entity.Filters, fetch.Entity.Links,
				query, CurrentView, CurrentViewConfiguration, GetLanguageCode());

			foreach (var filter in filters)
			{
				// build filter group

				var filterGroupListItem = new HtmlGenericControl("li");
				filterGroupListItem.Attributes.Add("class", "entitylist-filter-option-group");
				filterGroupList.Controls.Add(filterGroupListItem);

				var filterGroupLabel = new HtmlGenericControl("label") { InnerHtml = "<span class='sr-only'>Filter: </span>" + filter.Label };
				filterGroupLabel.Attributes.Add("class", "entitylist-filter-option-group-label h4");
				filterGroupListItem.Controls.Add(filterGroupLabel);

				var filterOptionList = new HtmlGenericControl("ul");
				filterOptionList.Attributes.Add("class", "list-unstyled");
				filterOptionList.Attributes.Add("role", "presentation");
				filterGroupListItem.Controls.Add(filterOptionList);

				if (filter.Options != null)
				{
					var options = filter.Options.ToArray();

					if (filter.SelectionMode == "Dropdown")
					{
						var filterOptionItem = new HtmlGenericControl("li");
						filterOptionItem.Attributes.Add("class", "entitylist-filter-option");
						filterOptionList.Controls.Add(filterOptionItem);
						
						var filterOptionLabelContainer = new HtmlGenericControl("div");
						filterOptionLabelContainer.Attributes.Add("class", "input-group entitylist-filter-option-text");
						filterOptionItem.Controls.Add(filterOptionLabelContainer);

						filterOptionLabelContainer.Controls.Add(
							new LiteralControl(
								@"<span class=""input-group-addon""><span class=""fa fa-filter"" aria-hidden=""true""></span></span>"));

						filterGroupLabel.Attributes.Add("for", "dropdownfilter_" + filter.Id);

						var filterOptionSelect = new HtmlGenericControl("select");
						filterOptionSelect.Attributes.Add("class", "form-control");
						filterOptionSelect.Attributes.Add("name", filter.Id);
						filterOptionSelect.Attributes.Add("id", "dropdownfilter_" + filter.Id);
						filterOptionLabelContainer.Controls.Add(filterOptionSelect);
						
						var blankOption = new HtmlGenericControl("option");
						blankOption.Attributes.Add("value", string.Empty);
						blankOption.Attributes.Add("label", " ");
						filterOptionSelect.Controls.Add(blankOption);

						foreach (var option in options)
						{
							var filterOptionOption = new HtmlGenericControl("option");
							filterOptionOption.Attributes.Add("value", option.Id);
							filterOptionOption.Attributes.Add("label", option.Label);
							filterOptionOption.InnerText = option.Label;

							if (option.Checked)
							{
								filterOptionSelect.Attributes.Add("selected", string.Empty);
								filterOptionSelect.Attributes.Add("data-selected", "true");
							}

							filterOptionSelect.Controls.Add(filterOptionOption);
						}
					}
					else
					{
						const int pageSize = 6;

						filterOptionList.Attributes["data-pagesize"] = string.Empty + pageSize;

						for (var i = 0; i < options.Length; i++)
						{
							// build filter option
							var option = options[i];

							var filterOptionItem = new HtmlGenericControl("li");
							filterOptionItem.Attributes.Add("class", "entitylist-filter-option");
							filterOptionList.Controls.Add(filterOptionItem);

							if (i >= pageSize)
							{
								filterOptionItem.Attributes["aria-hidden"] = "true";
							}

							if (option.Type == "text")
							{
								var filterOptionLabelContainer = new HtmlGenericControl("div");
								filterOptionLabelContainer.Attributes.Add("class", "input-group entitylist-filter-option-text");
								filterOptionItem.Controls.Add(filterOptionLabelContainer);

								filterOptionLabelContainer.Controls.Add(
									new LiteralControl(
										@"<span class=""input-group-addon""><span class=""fa fa-filter"" aria-hidden=""true""></span></span>"));

								filterGroupLabel.Attributes.Add("for", "textfilter_" + filter.Id);

								var filterOptionInput = new SelfClosingHtmlGenericControl("input");
								filterOptionInput.Attributes.Add("class", "form-control");
								filterOptionInput.Attributes.Add("type", "text");
								filterOptionInput.Attributes.Add("name", filter.Id);
								filterOptionInput.Attributes.Add("id", "textfilter_" + filter.Id);
								filterOptionLabelContainer.Controls.Add(filterOptionInput);

								if (!string.IsNullOrWhiteSpace(option.Text))
								{
									filterOptionInput.Attributes.Add("value", option.Text);
								}
							}
							else
							{
								var type = string.Equals(filter.SelectionMode, "Single") ? "radio" : "checkbox";

								var filterOptionLabelContainer = new HtmlGenericControl("div");
								filterOptionLabelContainer.Attributes.Add("class", type);
								filterOptionItem.Controls.Add(filterOptionLabelContainer);

								var filterOptionLabel = new HtmlGenericControl("label");
								filterOptionLabelContainer.Controls.Add(filterOptionLabel);

								var filterOptionInput = new SelfClosingHtmlGenericControl("input");
								filterOptionInput.Attributes.Add("type", type);
								filterOptionInput.Attributes.Add("name", filter.Id);
								filterOptionInput.Attributes.Add("value", option.Id);
								filterOptionLabel.Controls.Add(filterOptionInput);
								filterOptionLabel.Controls.Add(new Literal
								{
									Text = "<span class='sr-only'>Filter " + filter.Label + ": </span>" + option.Label
								});

								if (option.Checked)
								{
									filterOptionInput.Attributes.Add("checked", "checked");
									filterOptionInput.Attributes.Add("data-checked", "true");
								}
							}
						}

						if (options.Length > pageSize)
						{
							var breaker = new HtmlGenericControl("li");
							breaker.Attributes.Add("class", "entitylist-filter-option-breaker");
							filterOptionList.Controls.Add(breaker);

							var a = new HtmlGenericControl("a");
							a.Attributes["href"] = "#";
							a.Attributes["role"] = "button";
							a.Attributes["data-collapsed"] = "true";
							a.InnerHtml = "<span class='expand-label'>More</span> <span class='fa fa-caret-down'></span>";
							breaker.Controls.Add(a);
						}
					}
				}
			}

			// build apply button

			var submitContainer = new HtmlGenericControl("div");
			submitContainer.Attributes.Add("class", "pull-right");
			filterGroupPanel.Controls.Add(submitContainer);

			var submitButton = new HtmlButton { InnerText = CurrentViewConfiguration.FilterSettings.ApplyButtonLabel ?? "Apply" };
			submitButton.Attributes.Add("type", "button");
			submitButton.Attributes.Add("class", "btn btn-default btn-entitylist-filter-submit");
			submitButton.Attributes.Add("data-serialized-query",
				CurrentViewConfiguration.FilterSettings.FilterQueryStringParameterName);
			submitButton.Attributes.Add("data-target", "#" + containerElement.ID);
			submitContainer.Controls.Add(submitButton);

			container.Controls.Add(containerElement);
		}

		private int GetLanguageCode()
		{
			if (CurrentView.LanguageCode == 0)
			{
				// Entity lists only supports CRM languages, so use the CRM Lcid rather than the potentially custom language Lcid.
				return Context.GetContextLanguageInfo()?.ContextLanguage?.CrmLcid ?? CultureInfo.CurrentCulture.LCID;
			}
			return CurrentView.LanguageCode;
		}

		/// <summary>
		/// Add the <see cref="ScriptIncludes"/> to the <see cref="ScriptManager"/> if one exists.
		/// </summary>
		/// <param name="control"></param>
		protected virtual void RegisterClientSideDependencies(Control control)
		{
			RegisterClientSideMapDependencies(control, ScriptIncludes);
		}

		/// <summary>
		/// Add the <see cref="CalendarScriptIncludes"/> to the <see cref="ScriptManager"/> if one exists.
		/// </summary>
		/// <param name="control"></param>
		protected virtual void RegisterClientSideCalendarDependencies(Control control, Guid websiteId)
		{
			Attributes["data-calendar-template-path"] = VirtualPathUtility.ToAbsolute("~/Areas/EntityList/tmpls/");

			var scriptManager = ScriptManager.GetCurrent(control.Page);

			if (scriptManager != null)
			{
				foreach (var script in CalendarScriptIncludes)
				{
					if (string.IsNullOrWhiteSpace(script))
					{
						continue;
					}

					var absolutePath = script.StartsWith("http", true, CultureInfo.InvariantCulture)
						? script
						: VirtualPathUtility.ToAbsolute(script);

					scriptManager.Scripts.Add(new ScriptReference(absolutePath));
				}

				var url = new UrlHelper(Page.Request.RequestContext);

				scriptManager.Scripts.Add(new ScriptReference(url.RouteUrl("EntityListCalendarLanguage", new RouteValueDictionary
				{
					{ "__portalScopeId__", websiteId.ToString() }
				})));

				Attributes["lang"] = CultureInfo.CurrentCulture.ToString();
			}

			var head = Page.Header;

			if (head != null)
			{
				foreach (var stylesheet in CalendarStylesheets)
				{
					if (string.IsNullOrWhiteSpace(stylesheet))
					{
						continue;
					}

					var link = new HtmlLink { Href = stylesheet };

					link.Attributes["rel"] = "stylesheet";

					head.Controls.Add(link);
				}
			}
		}

		/// <summary>
		/// Add the <see cref="MapScriptIncludes"/> to the <see cref="ScriptManager"/> if one exists.
		/// </summary>
		/// <param name="control"></param>
		protected virtual void RegisterClientSideMapDependencies(Control control)
		{
			RegisterClientSideMapDependencies(control, MapScriptIncludes);
		}

		private static void RegisterClientSideMapDependencies(Control control, IEnumerable<string> scripts)
		{
			foreach (var script in scripts)
			{
				if (string.IsNullOrWhiteSpace(script))
				{
					continue;
				}

				var scriptManager = ScriptManager.GetCurrent(control.Page);

				if (scriptManager == null)
				{
					continue;
				}

				var absolutePath = script.StartsWith("http", true, CultureInfo.InvariantCulture)
					? script
					: VirtualPathUtility.ToAbsolute(script);

				scriptManager.Scripts.Add(new ScriptReference(absolutePath));
			}
		}

		private string GetGalleryServiceUrl(IPortalContext portalContext, Guid entityListId, Guid viewId)
		{
			var httpContextWrapper = new HttpContextWrapper(Context);
			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper);

			if (routeData == null)
			{
				return null;
			}

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			return urlHelper.Action("Index", "PackageRepository", new
			{
				__portalScopeId__ = portalContext.Website.Id,
				entityListId,
				viewId,
				area = "EntityList"
			});
		}

		private string GetCalendarServiceUrl(IPortalContext portalContext, Guid entityListId, Guid viewId)
		{
			var httpContextWrapper = new HttpContextWrapper(Context);
			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper);

			if (routeData == null)
			{
				return null;
			}

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			return urlHelper.Action("Index", "Calendar", new
			{
				__portalScopeId__ = portalContext.Website.Id,
				entityListId,
				viewId,
				area = "EntityList"
			});
		}

		private string GetCalendarDownloadServiceUrl(IPortalContext portalContext, Guid entityListId, Guid viewId)
		{
			var httpContextWrapper = new HttpContextWrapper(Context);
			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper);

			if (routeData == null)
			{
				return null;
			}

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			return urlHelper.Action("Download", "Calendar", new
			{
				__portalScopeId__ = portalContext.Website.Id,
				entityListId,
				viewId,
				area = "EntityList"
			});
		}

		/// <summary>
		/// Creates the HTML to render a subgrid listing of records defined by the associated view
		/// </summary>
		protected virtual IHtmlString BuildGrid(OrganizationServiceContext context)
		{
			if (ErrorModal == null) ErrorModal = new ViewErrorModal();

			var html = Mvc.Html.EntityExtensions.GetHtmlHelper(PortalName, Page.Request.RequestContext, Page.Response);
			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var user = portal == null ? null : portal.User;

			return html.EntityGrid(ViewConfigurations.ToList(),
				BuildControllerActionUrl("GetGridData", "EntityGrid",
					new { area = "Portal", __portalScopeId__ = portal == null ? Guid.Empty : portal.Website.Id }), user,
				string.Join(" ", new[] { CssClass, CurrentViewConfiguration.CssClass }).TrimEnd(' '),
				CurrentViewConfiguration.GridCssClass,
				CurrentViewConfiguration.GridColumnWidthStyle ?? EntityGridExtensions.GridColumnWidthStyle.Percent,
				SelectMode, null, CurrentViewConfiguration.LoadingMessage,
				CurrentViewConfiguration.ErrorMessage, CurrentViewConfiguration.AccessDeniedMessage,
				CurrentViewConfiguration.EmptyListText, PortalName, LanguageCode, false, true,
				CurrentViewConfiguration.DetailsActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Large,
				CurrentViewConfiguration.DetailsActionLink.Modal.CssClass, CurrentViewConfiguration.DetailsActionLink.Modal.Title,
				CurrentViewConfiguration.DetailsActionLink.Modal.LoadingMessage,
				CurrentViewConfiguration.DetailsActionLink.Modal.DismissButtonSrText,
				CurrentViewConfiguration.DetailsActionLink.Modal.TitleCssClass,
				CurrentViewConfiguration.InsertActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Large,
				CurrentViewConfiguration.InsertActionLink.Modal.CssClass, CurrentViewConfiguration.InsertActionLink.Modal.Title,
				CurrentViewConfiguration.InsertActionLink.Modal.LoadingMessage,
				CurrentViewConfiguration.InsertActionLink.Modal.DismissButtonSrText,
				CurrentViewConfiguration.InsertActionLink.Modal.TitleCssClass,
				CurrentViewConfiguration.EditActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Large,
				CurrentViewConfiguration.EditActionLink.Modal.CssClass, CurrentViewConfiguration.EditActionLink.Modal.Title,
				CurrentViewConfiguration.EditActionLink.Modal.LoadingMessage,
				CurrentViewConfiguration.EditActionLink.Modal.DismissButtonSrText,
				CurrentViewConfiguration.EditActionLink.Modal.TitleCssClass,
				CurrentViewConfiguration.DeleteActionLink.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
				CurrentViewConfiguration.DeleteActionLink.Modal.CssClass, CurrentViewConfiguration.DeleteActionLink.Modal.Title,
				CurrentViewConfiguration.DeleteActionLink.Confirmation,
				CurrentViewConfiguration.DeleteActionLink.Modal.PrimaryButtonText,
				CurrentViewConfiguration.DeleteActionLink.Modal.CloseButtonText,
				CurrentViewConfiguration.DeleteActionLink.Modal.DismissButtonSrText,
				CurrentViewConfiguration.DeleteActionLink.Modal.TitleCssClass,
				ErrorModal.Size ?? BootstrapExtensions.BootstrapModalSize.Default, ErrorModal.CssClass, ErrorModal.Title,
				ErrorModal.Body, ErrorModal.DismissButtonSrText, ErrorModal.CloseButtonText, ErrorModal.TitleCssClass);
		}

		private static string BuildControllerActionUrl(string actionName, string controllerName, object routeValues)
		{
			var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper) ?? new RouteData();

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			return urlHelper.Action(actionName, controllerName, routeValues);
		}
	}
}
