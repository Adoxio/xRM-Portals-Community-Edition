<%@ Control Language="C#" Inherits="ViewUserControl<Adxstudio.Xrm.Cms.IAdPlacement>" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Liquid" %>
<%
	if (Model == null) return;

	var portalLiquidContext = new PortalLiquidContext(Html,
		ViewData[PortalExtensions.PortalViewContextKey] as IPortalViewContext);

	var drop = new AdPlacementDrop(portalLiquidContext, Model);

	var alias = ViewData["alias"] as string;
	var variables = new Dictionary<string, object>
	{
		{string.IsNullOrEmpty(alias) ? "placement" : alias, drop}
	};

	var showcopy = (ViewData["showcopy"] as bool?).GetValueOrDefault(true);
	variables["show_copy"] = showcopy;

	var random = (ViewData["random"] as bool?).GetValueOrDefault(true);
	variables["random"] = random;

	if (Model.WebTemplate == null)
	{
		Html.RenderLiquid(Html.Partial("AdPlacementTemplate").ToHtmlString(), variables);
	}
	else
	{
		Html.RenderWebTemplate(Model.WebTemplate, variables);
	}
%>