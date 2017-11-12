/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
	/// <summary>
	/// Represents a data source that executes fetchXml to retrieve entity objects.
	/// </summary>
	public class ViewObjectDataSource : ObjectDataSource
	{
		/// <summary>
		/// Configuration properties of the view
		/// </summary>
		public class Configuration
		{
			private string _viewQueryStringParameterName;
			private IEnumerable<ViewConfiguration> _viewConfigurations;
			private IEnumerable<EntityView> _views;

			/// <summary>
			/// Gets or sets the name of the portal configuration that the control binds to.
			/// </summary>
			public string PortalName { get; set; }
			/// <summary>
			/// Language Code used to retrieve localized labels.
			/// </summary>
			public int LanguageCode { get; set; }
			/// <summary>
			/// Gets or sets the Query String parameter name for the selected view.
			/// </summary>
			public string ViewQueryStringParameterName
			{
				get
				{
					return string.IsNullOrWhiteSpace(_viewQueryStringParameterName) ? "view" : _viewQueryStringParameterName;
				}
				set
				{
					_viewQueryStringParameterName = value;
				}
			}
			/// <summary>
			/// Collection of settings to get a view and configure its display.
			/// </summary>
			public IEnumerable<ViewConfiguration> ViewConfigurations
			{
				get { return _viewConfigurations ?? new List<ViewConfiguration>(); }
				set { _viewConfigurations = value; }
			}
			/// <summary>
			/// Collection of <see cref="EntityView"/> records bound to the control consuming the data source.
			/// </summary>
			public IEnumerable<EntityView> Views
			{
				get { return _views ?? new List<EntityView>(); }
				set { _views = value; }
			}

			/// <summary>
			/// Configuration constructor
			/// </summary>
			/// <param name="portalName"></param>
			/// <param name="languageCode"></param>
			/// <param name="viewConfigurations"></param>
			/// <param name="views"></param>
			/// <param name="viewQueryStringParameterName"></param>
			public Configuration(string portalName, int languageCode, IEnumerable<ViewConfiguration> viewConfigurations, IEnumerable<EntityView>  views, string viewQueryStringParameterName)
			{
				PortalName = portalName;
				LanguageCode = languageCode;
				ViewConfigurations = viewConfigurations;
				Views = views;
				ViewQueryStringParameterName = viewQueryStringParameterName;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public ViewObjectDataSource(Configuration configuration)
		{
			TypeName = GetType().ToString();
			SelectCountMethod = "SelectCount";
			SelectMethod = "Select";
			EnablePaging = true;
			Config = configuration;
			SelectParameters.Add("configuration", TypeCode.Object, string.Empty);
			Selecting += OnSelecting;
		}

		/// <summary>
		/// Parameterless Constructor
		/// </summary>
		public ViewObjectDataSource() { }

		/// <summary>
		/// Selecting event handler that passes configuration parameters to the select method.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnSelecting(object sender, ObjectDataSourceSelectingEventArgs e)
		{
			if (e.ExecutingSelectCount)
			{
				// Prevent the select method from being executed a second time. The total count is persisted by the initial select.
				e.Arguments.RetrieveTotalRowCount = false;
				e.Arguments.TotalRowCount = TotalRecordCount;
				return;
			}

			e.InputParameters["configuration"] = Config;
		}

		/// <summary>
		/// Configuration parameters
		/// </summary>
		public Configuration Config;

		/// <summary>
		/// Paging Cookie
		/// </summary>
		public string PagingCookie = null;

		/// <summary>
		/// Total Record Count
		/// </summary>
		public int TotalRecordCount;

		/// <summary>
		/// SelectCount Method
		/// </summary>
		/// <returns>Total record count</returns>
		public int SelectCount()
		{
			return TotalRecordCount;
		}

		/// <summary>
		/// SelectCount Method
		/// </summary>
		/// <returns>Total record count</returns>
		public int SelectCount(Configuration configuration)
		{
			return TotalRecordCount;
		}

		/// <summary>
		/// Select Method
		/// </summary>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select)]
		public new DataTable Select()
		{
			return Select(0);
		}

		/// <summary>
		/// Select Method
		/// </summary>
		/// <param name="startRowIndex"></param>
		/// <param name="maximumRows"></param>
		/// <param name="configuration"></param>
		[DataObjectMethod(DataObjectMethodType.Select)]
		public DataTable Select(int startRowIndex, int maximumRows = -1, Configuration configuration = null)
		{
			if (configuration == null)
			{
				throw new ApplicationException("Invalid ViewObjectDataSource configuration. Please set the Configuration property");
			}

			if (configuration.ViewConfigurations == null || !configuration.ViewConfigurations.Any())
			{
				throw new ApplicationException("Invalid ViewObjectDataSource configuration. Please set the Configuration property");
			}

			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(configuration.PortalName);
			var viewParameterValue = HttpContext.Current.Request[configuration.ViewQueryStringParameterName];
			
			Guid selectedViewId;

			Guid.TryParse(viewParameterValue ?? string.Empty, out selectedViewId);

			var currentViewConfiguration = GetCurrentViewConfiguration(configuration, selectedViewId);

			var view = currentViewConfiguration.GetEntityView(configuration.Views);
			
			var pageParameterValue = HttpContext.Current.Request[currentViewConfiguration.PageQueryStringParameterName];
			var searchParameterValue = HttpContext.Current.Request[currentViewConfiguration.Search.SearchQueryStringParameterName];
			var sortParameterValue = HttpContext.Current.Request[currentViewConfiguration.SortQueryStringParameterName];
			var filterParameterValue = HttpContext.Current.Request[currentViewConfiguration.FilterQueryStringParameterName];
			var metadataFilterParameterValue = HttpContext.Current.Request[currentViewConfiguration.FilterSettings.FilterQueryStringParameterName];

			var pageNumber = 1;

			if (!string.IsNullOrEmpty(pageParameterValue))
			{
				int.TryParse(pageParameterValue, out pageNumber);
			}
			
			var viewDataAdapter = new ViewDataAdapter(currentViewConfiguration, new PortalConfigurationDataAdapterDependencies(configuration.PortalName), pageNumber, searchParameterValue, sortParameterValue, filterParameterValue, metadataFilterParameterValue);
			
			var result = viewDataAdapter.FetchEntities();

			var records = result.Records;

			TotalRecordCount = result.TotalRecordCount;

			return records.ToDataTable(serviceContext, view.SavedQuery, false, "r", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Gets the view configuration from the collection to provide the necessary parameters to retrieve the current savedquery (view).
		/// </summary>
		/// <param name="configuration"></param>
		/// <param name="selectedViewId">If selectedViewId is not specified, the first item in the ViewConfigurations collection will be returned, otherwise the ViewConfiguration with the matching view Id will be returned.</param>
		/// <returns></returns>
		protected ViewConfiguration GetCurrentViewConfiguration(Configuration configuration, Guid selectedViewId = new Guid())
		{
			var currentViewConfiguration = configuration.ViewConfigurations.First();
			if (selectedViewId == Guid.Empty)
			{
				return currentViewConfiguration;
			}
			var config = configuration.ViewConfigurations.FirstOrDefault(o => o.Id == selectedViewId);
			if (config != null)
			{
				currentViewConfiguration = config;
			}
			return currentViewConfiguration;
		}
	}
}
