/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;
	using Microsoft.Xrm.Portal;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Query;
	using Newtonsoft.Json;
	using Adxstudio.Xrm.Core;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web.Mvc.Html;
	using Adxstudio.Xrm.Web.UI.CrmEntityListView;
	using Adxstudio.Xrm.Web.UI.JsonConfiguration;
	using Adxstudio.Xrm.Web.UI.WebForms;
	using GridMetadata = Adxstudio.Xrm.Web.UI.JsonConfiguration.GridMetadata;
	using GuidConverter = Adxstudio.Xrm.Web.UI.JsonConfiguration.GuidConverter;

	/// <summary>
	/// Entity List control retrieves the Entity List record defined for the Web Page containing this control. Users can add tabular lists of records within the portal without the need for developer intervention.
	/// </summary>
	[Description("Entity List control retrieves the Entity List record defined for the Web Page containing this control. Users can add tabular lists of records within the portal without the need for developer intervention.")]
	[ToolboxData(@"<{0}:EntityList runat=""server""></{0}:EntityList>")]
	[DefaultProperty("")]
	public class EntityList : CompositeControl
	{
		/// <summary>
		/// Indicates whether the list is a gallery or not.
		/// </summary>
		public bool IsGallery
		{
			get { return (bool)(ViewState["IsGallery"] ?? false); }
			set { ViewState["IsGallery"] = value; }
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

		/// <summary>
		/// Gets or sets the List CSS Class.
		/// </summary>
		[Description("The CSS Class assigned to the List.")] [DefaultValue("")]
		public string ListCssClass
		{
			get { return ((string)ViewState["ListCssClass"]) ?? string.Empty; }
			set { ViewState["ListCssClass"] = value; }
		}

		/// <summary>
		/// Gets or sets the Empty List Text.
		/// </summary>
		[Description("Text to display when the list is empty.")] [DefaultValue("")]
		public string DefaultEmptyListText
		{
			get { return ((string)ViewState["DefaultEmptyListText"]) ?? string.Empty; }
			set { ViewState["DefaultEmptyListText"] = value; }
		}

		/// <summary>
		/// Gets or sets the create button label.
		/// </summary>
		[Description("Text to display for the create button.")] [DefaultValue("")]
		public string DefaultCreateButtonLabel
		{
			get { return ((string)ViewState["DefaultCreateButtonLabel"]) ?? string.Empty; }
			set { ViewState["DefaultCreateButtonLabel"] = value; }
		}

		/// <summary>
		/// Gets or sets the details button label.
		/// </summary>
		[Description("Text to display for the details button.")] [DefaultValue("")]
		public string DefaultDetailsButtonLabel
		{
			get { return ((string)ViewState["DefaultDetailsButtonLabel"]) ?? string.Empty; }
			set { ViewState["DefaultDetailsButtonLabel"] = value; }
		}

		/// <summary>
		/// Gets or sets the Query String parameter name for the page number.
		/// </summary>
		[Description("The Query String parameter name for the page number.")] [DefaultValue("page")]
		public string PageQueryStringField
		{
			get { return ((string)ViewState["PageQueryStringField"]) ?? "page"; }
			set { ViewState["PageQueryStringField"] = value; }
		}

		/// <summary>
		/// Gets or sets the Query String parameter name for the filter.
		/// </summary>
		[Description("The Query String parameter name for the filter.")] [DefaultValue("filter")]
		public string FilterQueryStringField
		{
			get { return ((string)ViewState["FilterQueryStringField"]) ?? "filter"; }
			set { ViewState["FilterQueryStringField"] = value; }
		}

		/// <summary>
		/// Gets or sets the Query String parameter name for the metadata filter.
		/// </summary>
		[Description("The Query String parameter name for the metadata filter.")] [DefaultValue("mf")]
		public string MetadataFilterQueryStringField
		{
			get { return ((string)ViewState["MetadataFilterQueryStringField"]) ?? "mf"; }
			set { ViewState["MetadataFilterQueryStringField"] = value; }
		}

		/// <summary>
		/// Gets or sets the Query String parameter name for the selected view.
		/// </summary>
		[Description("The Query String parameter name for the selected view.")] [DefaultValue("view")]
		public string ViewQueryStringField
		{
			get { return ((string)ViewState["ViewQueryStringField"]) ?? "view"; }
			set { ViewState["ViewQueryStringField"] = value; }
		}

		/// <summary>
		/// Gets or sets the Query String parameter name for the search query.
		/// </summary>
		[Description("The Query String parameter name for the search query.")] [DefaultValue("query")]
		public string SearchQueryStringField
		{
			get
			{
				var parameterName = ((string)ViewState["SearchQueryStringField"]);
				return string.IsNullOrWhiteSpace(parameterName) ? "query" : parameterName;
			}
			set { ViewState["SearchQueryStringField"] = value; }
		}

		/// <summary>
		/// Gets or sets the Query String parameter name for the sort expression.
		/// </summary>
		[Description("The Query String parameter name for the sort expression.")] [DefaultValue("sort")]
		public string SortQueryStringField
		{
			get { return ((string)ViewState["SortQueryStringField"]) ?? "sort"; }
			set { ViewState["SortQueryStringField"] = value; }
		}

		/// <summary>
		/// Gets or sets the Text to display when the rendering a filter dropdown to select 'my' records.
		/// </summary>
		[Description("Text to display when the rendering a filter dropdown to select 'my' records.")] [DefaultValue("My")]
		public string FilterByUserOptionLabel
		{
			get { return ((string)ViewState["FilterByUserOptionLabel"]) ?? "My"; }
			set { ViewState["FilterByUserOptionLabel"] = value; }
		}

		/// <summary>
		/// Width in pixels of the column containing the action links to the details view page.
		/// </summary>
		[Description("Width in pixels of the column containing the action links to the details view page.")]
		public int ActionLinksColumnWidth
		{
			get { return (int)(ViewState["ActionLinksColumnWidth"] ?? 20); }
			set { ViewState["ActionLinksColumnWidth"] = value; }
		}

		/// <summary>
		/// Gets or sets the Text to display when the rendering the details view button's tooltip.
		/// </summary>
		[Description("Text to display when the rendering the details view button's tooltip.")] [DefaultValue("View details")]
		public string ActionLinkDetailsViewTooltipLabel
		{
			get { return ((string)ViewState["ActionLinkDetailsViewTooltipLabel"]) ?? ResourceManager.GetString("View_Details_Tooltip"); }
			set { ViewState["ActionLinkDetailsViewTooltipLabel"] = value; }
		}

		/// <summary>
		/// Gets or sets the Text to display when the rendering the edit button's tooltip.
		/// </summary>
		[Description("Text to display when the rendering the edit button's tooltip.")] [DefaultValue("Edit")]
		public string ActionLinkEditTooltipLabel
		{
			get { return ((string)ViewState["ActionLinkEditTooltipLabel"]) ?? "Edit"; }
			set { ViewState["ActionLinkEditTooltipLabel"] = value; }
		}

		/// <summary>
		/// Gets or sets the Text to display when the rendering the delete button's tooltip.
		/// </summary>
		[Description("Text to display when the rendering the delete button's tooltip.")] [DefaultValue("Delete")]
		public string ActionLinkDeleteTooltipLabel
		{
			get { return ((string)ViewState["ActionLinkDeleteTooltipLabel"]) ?? "Delete"; }
			set { ViewState["ActionLinkDeleteTooltipLabel"] = value; }
		}

		/// <summary>
		/// Gets or sets the Text to display when the rendering the insert button's tooltip.
		/// </summary>
		[Description("Text to display when the rendering the insert button's tooltip.")] [DefaultValue("Insert")]
		public string ActionLinkInsertTooltipLabel
		{
			get { return ((string)ViewState["ActionLinkInsertTooltipLabel"]) ?? "Insert"; }
			set { ViewState["ActionLinkInsertTooltipLabel"] = value; }
		}
		
		/// <summary>
		/// Indicates whether or not the entity permission provider will add record level filters to the view's fetch query to assert privileges.
		/// </summary>
		protected bool EnableEntityPermissions
		{
			get { return (bool)(ViewState["EnableEntityPermissions"] ?? false); }
			set { ViewState["EnableEntityPermissions"] = value; }
		}

		/// <summary>
		/// Add the necessary script files to the <see cref="ScriptManager"/> if one exists.
		/// </summary>
		protected virtual string[] ScriptIncludes { get { return new[] { string.Empty }; } }

		protected override HtmlTextWriterTag TagKey { get { return HtmlTextWriterTag.Div; } }

		protected override void CreateChildControls()
		{
			Controls.Clear();

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			Entity entitylist;
			bool updateEntityListReference;

			if (!TryGetEntityList(portalContext, serviceContext, out entitylist, out updateEntityListReference))
			{
				Visible = false;

				return;
			}

			if (updateEntityListReference) { EntityListReference = entitylist.ToEntityReference(); }

			if (LanguageCode <= 0) { LanguageCode = this.Context.GetPortalSolutionsDetails().OrganizationBaseLanguageCode; }

			RegisterClientSideDependencies(this);

			var registerStartupScript = entitylist.GetAttributeValue<string>("adx_registerstartupscript");

			if (!string.IsNullOrWhiteSpace(registerStartupScript))
			{
				var html = Mvc.Html.EntityExtensions.GetHtmlHelper(PortalName, Page.Request.RequestContext, Page.Response);

				var control = new HtmlGenericControl() { };

				var script = html.ScriptAttribute(serviceContext, entitylist, "adx_registerstartupscript");

				control.InnerHtml = script.ToString();

				Controls.Add(control);
			}

			var entityName = entitylist.GetAttributeValue<string>("adx_entityname");
			var primaryKeyName = entitylist.GetAttributeValue<string>("adx_primarykeyname");
			var view = entitylist.GetAttributeValue<string>("adx_view"); // old comma delimited list of views
			var viewMetadataJson = entitylist.GetAttributeValue<string>("adx_views");
			EnableEntityPermissions = entitylist.GetAttributeValue<bool?>("adx_entitypermissionsenabled").GetValueOrDefault(false);

			if (string.IsNullOrWhiteSpace(entityName))
			{
				throw new ApplicationException("Entity Name (adx_entityname) attribute on Entity List (adx_entitylist) is null or empty. Please specify the logical name of the entity.");
			}

			if (!string.IsNullOrWhiteSpace(entityName) && string.IsNullOrWhiteSpace(primaryKeyName))
			{
				primaryKeyName = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(serviceContext, entityName);
			}

			if (string.IsNullOrWhiteSpace(primaryKeyName))
			{
				throw new ApplicationException(string.Format("The entity primary key logical name couldn't be determined.", entityName));
			}

			if (string.IsNullOrWhiteSpace(view) && string.IsNullOrWhiteSpace(viewMetadataJson))
			{
				throw new ApplicationException("View selection on Entity List (adx_entitylist) is null or empty. Specify the savedquery views.");
			}

			var gridMetadataJson = entitylist.GetAttributeValue<string>("adx_settings");
			GridMetadata gridMetadata = null;
			if (!string.IsNullOrWhiteSpace(gridMetadataJson))
			{
				try
				{
					gridMetadata = JsonConvert.DeserializeObject<GridMetadata>(gridMetadataJson,
						new JsonSerializerSettings
						{
							ContractResolver = JsonConfigurationContractResolver.Instance,
							TypeNameHandling = TypeNameHandling.Objects,
							Converters = new List<JsonConverter> { new GuidConverter() },
							Binder = new ActionSerializationBinder()
						});
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());
				}
			}

			var viewConfigurations = new List<ViewConfiguration>();

			if (string.IsNullOrWhiteSpace(viewMetadataJson))
			{
				if (string.IsNullOrWhiteSpace(view)) return;
				var viewids = view.Split(',');
				var viewGuids = viewids.Length < 1 ? null : Array.ConvertAll(viewids, Guid.Parse);

				if (viewGuids == null || !viewGuids.Any())
				{
					throw new ApplicationException(
						ResourceManager.GetString("ADX_View_Attribute_On_Entity_List_Contains_Invalid_Data_Exception"));
				}

				viewConfigurations =
					viewGuids.Select(
						viewGuid =>
							new ViewConfiguration(portalContext, serviceContext, entitylist, entityName, primaryKeyName, viewGuid,
								gridMetadata, PortalName, LanguageCode, EnableEntityPermissions, PageQueryStringField, FilterQueryStringField,
								SearchQueryStringField, SortQueryStringField, FilterByUserOptionLabel, ActionLinksColumnWidth,
								MetadataFilterQueryStringField, DefaultDetailsButtonLabel, ActionLinkDetailsViewTooltipLabel,
								DefaultCreateButtonLabel, ActionLinkInsertTooltipLabel, DefaultEmptyListText)).ToList();
			}
			else
			{
				ViewMetadata viewMetadata = null;
				try
				{
					viewMetadata = JsonConvert.DeserializeObject<ViewMetadata>(viewMetadataJson,
						new JsonSerializerSettings { ContractResolver = JsonConfigurationContractResolver.Instance, TypeNameHandling = TypeNameHandling.Objects, Binder = new ActionSerializationBinder(), Converters = new List<JsonConverter> { new GuidConverter() } });
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());
				}

				if (viewMetadata != null && viewMetadata.Views != null && viewMetadata.Views.Any())
				{
					foreach (var viewMeta in viewMetadata.Views)
					{
						var viewConfiguration = new ViewConfiguration(portalContext, serviceContext, entitylist, entityName,
							primaryKeyName, viewMeta.ViewId,
							gridMetadata, PortalName, LanguageCode, EnableEntityPermissions, PageQueryStringField, FilterQueryStringField,
							SearchQueryStringField, SortQueryStringField, FilterByUserOptionLabel, ActionLinksColumnWidth,
							MetadataFilterQueryStringField, DefaultDetailsButtonLabel, ActionLinkDetailsViewTooltipLabel,
							DefaultCreateButtonLabel, ActionLinkInsertTooltipLabel, DefaultEmptyListText);
						if (viewMeta.DisplayName != null)
						{
							var displayName = Localization.GetLocalizedString(viewMeta.DisplayName, LanguageCode);
							if (!string.IsNullOrWhiteSpace(displayName))
							{
								viewConfiguration.ViewDisplayName = displayName;
							}
						}
						viewConfigurations.Add(viewConfiguration);
					}
				}
			}

			var crmEntityListView = new CrmEntityListView
			{
				EnableEntityPermissions = EnableEntityPermissions,
				EntityListReference = EntityListReference,
				ViewConfigurations = viewConfigurations,
				ListCssClass = ListCssClass,
				PortalName = PortalName,
				LanguageCode = LanguageCode,
				ViewQueryStringParameterName = ViewQueryStringField,
				IsGallery = IsGallery
			};

			if (gridMetadata != null)
			{
				if (gridMetadata.ErrorDialog != null)
				{
					crmEntityListView.ErrorModal = new ViewErrorModal
					{
						CloseButtonCssClass = gridMetadata.ErrorDialog.CloseButtonCssClass,
						CloseButtonText = gridMetadata.ErrorDialog.CloseButtonText.GetLocalizedString(LanguageCode),
						CssClass = gridMetadata.ErrorDialog.CssClass,
						DismissButtonSrText = gridMetadata.ErrorDialog.DismissButtonSrText.GetLocalizedString(LanguageCode),
						Body = gridMetadata.ErrorDialog.Body.GetLocalizedString(LanguageCode),
						Size = gridMetadata.ErrorDialog.Size,
						Title = gridMetadata.ErrorDialog.Title.GetLocalizedString(LanguageCode),
						TitleCssClass = gridMetadata.ErrorDialog.TitleCssClass
					};
				}
			}

			Controls.Add(crmEntityListView);
		}

		/// <summary>
		/// Add the <see cref="ScriptIncludes"/> to the <see cref="ScriptManager"/> if one exists.
		/// </summary>
		/// <param name="control"></param>
		protected virtual void RegisterClientSideDependencies(Control control)
		{
			foreach (var script in ScriptIncludes)
			{
				if (string.IsNullOrWhiteSpace(script)) continue;

				var scriptManager = ScriptManager.GetCurrent(control.Page);

				if (scriptManager == null) continue;

				var absolutePath = script.StartsWith("http", true, CultureInfo.InvariantCulture) ? script : VirtualPathUtility.ToAbsolute(script);

				scriptManager.Scripts.Add(new ScriptReference(absolutePath));
			}
		}

		/// <summary>
		/// Get the entity list by id
		/// </summary>
		/// <param name="portalContext"></param>
		/// <param name="serviceContext"></param>
		/// <param name="entitylist"></param>
		/// <param name="updateEntityListReference"></param>
		protected virtual bool TryGetEntityList(IPortalContext portalContext, OrganizationServiceContext serviceContext, out Entity entitylist, out bool updateEntityListReference)
		{
			entitylist = null;
			updateEntityListReference = false;

			if (EntityListReference != null)
			{
				entitylist = serviceContext.RetrieveSingle(
					"adx_entitylist",
					FetchAttribute.All,
					new Condition("adx_entitylistid", ConditionOperator.Equal, EntityListReference.Id));

				if (entitylist == null)
				{
					throw new ApplicationException(string.Format("Couldn't find an Entity List (adx_entitylist) record where id equals {0}.", EntityListReference.Id));
				}

				return true;
			}

			var entity = portalContext.Entity;

			if (entity.LogicalName != "adx_webpage")
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "The current entity must be of type adx_webpage. Please select the correct template for this entity type.");

				return false;
			}

			var entitylistReference = entity.GetAttributeValue<EntityReference>("adx_entitylist");

			if (entitylistReference == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Could not find an Entity List (adx_entitylist) value on Web Page (adx_webpage) where id equals {0}.", entity.Id));

				return false;
			}

			entitylist = serviceContext.RetrieveSingle(
				"adx_entitylist",
				FetchAttribute.All,
				new Condition("adx_entitylistid", ConditionOperator.Equal, entitylistReference.Id));

			if (entitylist == null)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Could not find an Entity List (adx_webpage_entitylist) value where id equals {0} on Web Page (adx_webpage) where id equals {1}.", entitylistReference.Id, entity.Id));

				return false;
			}

			updateEntityListReference = true;

			return true;
		}
	}
}
