/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;

namespace Site.Areas.Portal
{
	public class PortalAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Portal"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute("Visualizations_GetChartBuilder", "_services/visualizations/get-chart-builder", new { controller = "Chart", action = "GetChartBuilder" }, new[] { "Adxstudio.Xrm.Visualizations.Controllers" });

			context.MapRoute("PortalRateit", "_services/rateit", new { controller = "Rating", action = "CreateRating" }, new[] { "Adxstudio.Xrm.Cms" });

			context.MapRoute("PortalSearch", "_services/search/{__portalScopeId__}", new { controller = "Search", action = "Search" }, new[] { "Adxstudio.Xrm.Web.MVC.Controllers" });
			context.MapRoute("PortalSearchAction", "_services/search/{__portalScopeId__}/{action}", new { controller = "Search", action = "GetLocalizedLabels" }, new[] { "Adxstudio.Xrm.Web.MVC.Controllers" });
			context.MapRoute("PortalQualifyLead", "_services/action-qualify-lead/{__portalScopeId__}", new { controller = "EntityAction", action = "QualifyLead" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalCloseCase", "_services/action-close-case/{__portalScopeId__}", new { controller = "EntityAction", action = "CloseCase" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalResolveCase", "_services/action-resolve-case/{__portalScopeId__}", new { controller = "EntityAction", action = "ResolveCase" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalReopenCase", "_services/action-reopen-case/{__portalScopeId__}", new { controller = "EntityAction", action = "ReopenCase" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalCancelCase", "_services/action-cancel-case/{__portalScopeId__}", new { controller = "EntityAction", action = "CancelCase" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalConvertQuote", "_services/action-convert-quote/{__portalScopeId__}", new { controller = "EntityAction", action = "ConvertQuoteToOrder" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalConvertInvoice", "_services/action-convert-order/{__portalScopeId__}", new { controller = "EntityAction", action = "ConvertOrderToInvoice" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalCalculateOpportunity", "_services/action-calculate-opportunity/{__portalScopeId__}", new { controller = "EntityAction", action = "CalculateActualValueOfOpportunity" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalDeactivate", "_services/action-deactivate/{__portalScopeId__}", new { controller = "EntityAction", action = "Deactivate" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalActivate", "_services/action-activate/{__portalScopeId__}", new { controller = "EntityAction", action = "Activate" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalActivateQuote", "_services/action-activate-quote/{__portalScopeId__}", new { controller = "EntityAction", action = "ActivateQuote" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalSetOpportunityOnHold", "_services/action-set-opportunity-on-hold/{__portalScopeId__}", new { controller = "EntityAction", action = "SetOpportunityOnHold" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalWinOpportunity", "_services/action-win-opportunity/{__portalScopeId__}", new { controller = "EntityAction", action = "WinOpportunity" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalLoseOpportunity", "_services/action-lose-opportunity/{__portalScopeId__}", new { controller = "EntityAction", action = "LoseOpportunity" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalGenerateQuoteFromOpportunity", "_services/action-generate-quote/{__portalScopeId__}", new { controller = "EntityAction", action = "GenerateQuoteFromOpportunity" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalUpdatePipelinePhase", "_services/action-update-pipeline-phase/{__portalScopeId__}", new { controller = "EntityAction", action = "UpdatePipelinePhase" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });
			context.MapRoute("PortalReopenOpportunity", "_services/reopen-opportunity/{__portalScopeId__}", new { controller = "EntityAction", action = "ReopenOpportunity" }, new[] { "Adxstudio.Xrm.EntityForm.Controllers" });

			context.MapRoute("PortalGetSubgridData", "_services/entity-subgrid-data.json/{__portalScopeId__}", new { controller = "EntityGrid", action = "GetSubgridData" });
			context.MapRoute("PortalGetGridData", "_services/entity-grid-data.json/{__portalScopeId__}", new { controller = "EntityGrid", action = "GetGridData" });
			context.MapRoute("PortalGetLookupGridData", "_services/entity-lookup-grid-data.json/{__portalScopeId__}", new { controller = "EntityGrid", action = "GetLookupGridData" });
			context.MapRoute("PortalAssociate", "_services/entity-lookup-associate/{__portalScopeId__}", new { controller = "EntityGrid", action = "Associate" });
			context.MapRoute("PortalDisassociate", "_services/entity-grid-disassociate/{__portalScopeId__}", new { controller = "EntityGrid", action = "Disassociate" });
			context.MapRoute("PortalDelete", "_services/entity-grid-delete/{__portalScopeId__}", new { controller = "EntityGrid", action = "Delete" });
			context.MapRoute("PortalDownloadAsCsv", "_services/download-as-csv/{__portalScopeId__}", new { controller = "EntityGrid", action = "DownloadAsCsv" });
			context.MapRoute("PortalDownloadAsExcel", "_services/download-as-excel/{__portalScopeId__}", new { controller = "EntityGrid", action = "DownloadAsExcel" });
			context.MapRoute("PortalExecuteWorkflow", "_services/execute-workflow/{__portalScopeId__}", new { controller = "EntityGrid", action = "ExecuteWorkflow" });
			context.MapRoute("PortalAddNote", "_services/entity-form-addnote/{__portalScopeId__}", new { controller = "EntityNotes", action = "AddNote" });
			context.MapRoute("PortalUpdateNote", "_services/entity-form-updatenote/{__portalScopeId__}", new { controller = "EntityNotes", action = "UpdateNote" });
			context.MapRoute("PortalDeleteNote", "_services/entity-form-deletenote/{__portalScopeId__}", new { controller = "EntityNotes", action = "DeleteNote" });
			context.MapRoute("PortalGetNotes", "_services/entity-notes/{__portalScopeId__}", new { controller = "EntityNotes", action = "GetNotes" });
			context.MapRoute("PortalBadges", "_services/badges/{__portalScopeId__}/{userid}/{type}", new { controller = "Badges", action = "GetBadges" }, new[] { "Adxstudio.Xrm.Cms.Badges.Controllers" });
			context.MapRoute("PortalGetSharePointData", "_services/sharepoint-data.json/{__portalScopeId__}", new { controller = "SharePointGrid", action = "GetSharePointData" });
			context.MapRoute("PortalAddSharePointFiles", "_services/sharepoint-addfiles/{__portalScopeId__}", new { controller = "SharePointGrid", action = "AddSharePointFiles" });
			context.MapRoute("PortalAddSharePointFolder", "_services/sharepoint-addfolder/{__portalScopeId__}", new { controller = "SharePointGrid", action = "AddSharePointFolder" });
			context.MapRoute("PortalDeleteSharePointItem", "_services/sharepoint-deleteitem/{__portalScopeId__}", new { controller = "SharePointGrid", action = "DeleteSharePointItem" });
			context.MapRoute("Default", "_portal/{__portalScopeId__}/{controller}/{action}");
			context.Routes.MapPageRoute("PortalModalFormTemplatePath", "_portal/modal-form-template-path/{__portalScopeId__}", "~/Areas/Portal/Pages/Form.aspx");
			context.Routes.MapPageRoute("PortalQuickFormTemplatePath", "_portal/quickform-template-path/{__portalScopeId__}", "~/Areas/Portal/Pages/QuickForm.aspx");
			
			context.MapRoute("Layout_TokenHtml", "_layout/tokenhtml", new { controller = "Layout", action = "GetAntiForgeryToken" });
			context.MapRoute("Layout_ContextUrlWithLanguage", "_layout/contexturlwithlanguage", new { controller = "Layout", action = "ContextUrlWithLanguage" });
			context.MapRoute("Layout_Header", "_layout/header", new { controller = "Layout", action = "Header" });
			context.MapRoute("Layout_HeaderChildNavbar", "_layout/headerchildnavbar", new { controller = "Layout", action = "HeaderChildNavbar" });
			context.MapRoute("Layout_HeaderPrimaryNavigation", "_layout/headerprimarynavigation", new { controller = "Layout", action = "HeaderPrimaryNavigation" });
			context.MapRoute("Layout_HeaderPrimaryNavigationTabs", "_layout/headerprimarynavigationtabs", new { controller = "Layout", action = "HeaderPrimaryNavigationTabs" });
			context.MapRoute("Layout_HeaderPrimaryNavigationXs", "_layout/headerprimarynavigationxs", new { controller = "Layout", action = "HeaderPrimaryNavigationXs" });
			context.MapRoute("Layout_Footer", "_layout/footer", new { controller = "Layout", action = "Footer" });
			context.MapRoute("Layout_LiquidSubstitution", "_layout/liquidsubstitution", new { controller = "Layout", action = "LiquidSubstitution" });
			context.MapRoute("Layout_RegisterUrl", "_layout/registerurl", new { controller = "Layout", action = "RegisterUrl" });
			context.MapRoute("Layout_SignInLink", "_layout/signinlink", new { controller = "Layout", action = "SignInLink" });
			context.MapRoute("Layout_SignInUrl", "_layout/signinurl", new { controller = "Layout", action = "SignInUrl" });
			context.MapRoute("Layout_SignOutUrl", "_layout/signouturl", new { controller = "Layout", action = "SignOutUrl" });
			
			context.MapRoute("Get_ResourceManager", "_resources/getresourcemanager", new { controller = "Resources", action = "ResourceManager" });
		}
	}
}
