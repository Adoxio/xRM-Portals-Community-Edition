/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Json.JsonConverter;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;

namespace Adxstudio.Xrm.Web.UI
{
	/// <summary>
	/// Defines the layout of a view
	/// </summary>
	public class ViewLayout
	{
		/// <summary>
		/// Enumeration of the type of column
		/// </summary>
		public enum ViewColumnType
		{
			/// <summary>
			/// Column contains data
			/// </summary>
			Data,
			/// <summary>
			/// Column used for row selection
			/// </summary>
			Select,
			/// <summary>
			/// Column contain row actions
			/// </summary>
			Actions
		}

		/// <summary>
		/// Details of a column in a view
		/// </summary>
		public class ViewColumn : SavedQueryView.ViewColumn
		{
			/// <summary>
			/// ViewColumn constructor
			/// </summary>
			/// <param name="logicalName">Logical Name of the column's attribute</param>
			/// <param name="name">Name of the column</param>
			/// <param name="metadata">AttributeMetadata of the column's attribute</param>
			/// <param name="width">Width of the column in pixels</param>
			/// <param name="widthAsPercent">Width of the column as percent</param>
			/// <param name="sortDisabled">True value indicates that sort is disabled, otherwise sort is enabled by default. </param>
			/// <param name="type">Type of column. Default is 'Data'. <see cref="ViewColumnType"/></param>
			public ViewColumn(string logicalName, string name, AttributeMetadata metadata, int width, double widthAsPercent, bool sortDisabled = false, ViewColumnType type = ViewColumnType.Data)
				: base(logicalName, name, metadata, width, sortDisabled)
			{
				WidthAsPercent = widthAsPercent;
				Type = type;
			}

			/// <summary>
			/// Width of the column as percent.
			/// </summary>
			public double WidthAsPercent { get; set; }

			/// <summary>
			/// Type of column. Default is 'Data'. <see cref="ViewColumnType"/>
			/// </summary>
			public ViewColumnType Type { get; set; }
		}

		/// <summary>
		/// The <see cref="ViewConfiguration"/> that defines the settings needed to be able to retrieve a view and configure its display. 
		/// </summary>
		public ViewConfiguration Configuration { get; private set; }

		/// <summary>
		/// The ViewConfiguration encrypted and converted to a Base64 string to be passed to service endpoints for secure configuration.
		/// </summary>
		public string Base64SecureConfiguration { get; private set; }

		/// <summary>
		/// The <see cref="EntityView"/> retrieved for the <see cref="ViewConfiguration"/>
		/// </summary>
		[JsonIgnore]
		public EntityView View { get; private set; }

		/// <summary>
		/// Name of the view
		/// </summary>
		public string ViewName { get; private set; }

		/// <summary>
		/// The columns of the view with overridden labels and widths as specified in the <see cref="ViewConfiguration"/>
		/// </summary>
		public IEnumerable<ViewColumn> Columns { get; private set; }

		/// <summary>
		/// The total width of all columns
		/// </summary>
		public int ColumnsTotalWidth { get; private set; }
		
		/// <summary>
		/// The sort expression defined by the orderby element of the view's fetchXml
		/// </summary>
		public string SortExpression { get; private set; }

		/// <summary>
		/// Identifier of the layout
		/// </summary>
		public Guid Id { get; private set; }

		/// <summary>
		/// Parameterless constructor
		/// </summary>
		public ViewLayout()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public ViewLayout(ViewConfiguration configuration, EntityView view = null, string portalName = null, int languageCode = 0,
			bool addSelectColumn = false, bool addActionsColumn = false, string selectColumnHeaderText = "")
		{
			Configuration = configuration;

			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(portalName);

			if (view == null)
			{
				view = Configuration.GetEntityView(serviceContext, languageCode);
			}
			
			View = view;

			ViewName = view.Name;

			Id = view.Id;

			SortExpression = view.SortExpression;
			
			var overrideColumns = new List<ViewColumn>();

			if (addSelectColumn)
			{
				overrideColumns.Add(new ViewColumn("col-select", selectColumnHeaderText ?? string.Empty, null, 20, 0, true, ViewColumnType.Select));
			}

			if (Configuration.ColumnOverrides.Any())
			{
				foreach (var columnA in View.Columns)
				{
					var match = false;
					foreach (var columnB in Configuration.ColumnOverrides)
					{
						if (columnA.LogicalName == columnB.AttributeLogicalName)
						{
							match = true;
							overrideColumns.Add(new ViewColumn(columnB.AttributeLogicalName, 
								string.IsNullOrWhiteSpace(columnB.DisplayName) ? columnA.Name : columnB.DisplayName, null, 
								columnB.Width == 0 ? columnA.Width : columnB.Width, 0, columnA.SortDisabled));
						}
					}
					if (!match)
					{
						overrideColumns.Add(new ViewColumn(columnA.LogicalName, columnA.Name, columnA.Metadata, columnA.Width, 0, columnA.SortDisabled));
					}
				}
			}
			else
			{
				overrideColumns.AddRange(View.Columns.Select(c => new ViewColumn(c.LogicalName, c.Name, c.Metadata, c.Width, 0, c.SortDisabled)));
			}

			if (addActionsColumn)
			{
				overrideColumns.Add(new ViewColumn("col-action", configuration.ActionColumnHeaderText ??
					"<span class='sr-only'>Actions</span>", null, configuration.ActionLinksColumnWidth, 0, true, ViewColumnType.Actions));
			}

			ColumnsTotalWidth = overrideColumns.Sum(c => c.Width);
			// Adjust the widths to be percentage based
			foreach (var column in overrideColumns)
			{
				var columnWidth = overrideColumns.FirstOrDefault(o => o.LogicalName == column.LogicalName);
				if (columnWidth == null)
				{
					continue;
				}
				var width = Convert.ToDouble(columnWidth.Width) / ColumnsTotalWidth * 100;
				column.WidthAsPercent = width;
			}

			Columns = overrideColumns;

			if (configuration.EnableEntityPermissions && AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled)
			{
				var insertActionEnabled = configuration.InsertActionLink.Enabled;
				var createViewActionLinks = configuration.ViewActionLinks.Where(viewAction => viewAction.Enabled &&
				viewAction.Type == LinkActionType.Insert);

				if (insertActionEnabled || createViewActionLinks.Any())
				{
					var crmEntityPermissionProvider = new CrmEntityPermissionProvider(configuration.PortalName);
					var canCreate = crmEntityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Create,
									configuration.EntityName);

					if (insertActionEnabled)
					{
						configuration.InsertActionLink.Enabled = canCreate;
					}

					foreach (var action in createViewActionLinks)
					{
						action.Enabled = canCreate;
					}
				}
			}

			// Produce a secure configuration converted to a Base64 string
			var configurationJson = JsonConvert.SerializeObject(configuration, new JsonSerializerSettings {
				Converters = new List<JsonConverter> { new UrlBuilderConverter() } });
			var configurationByteArray = Encoding.UTF8.GetBytes(configurationJson);
			var protectedByteArray = MachineKey.Protect(configurationByteArray, "Secure View Configuration");
			Base64SecureConfiguration = Convert.ToBase64String(protectedByteArray);
		}
	}
}
