<%@ Control Language="C#" Inherits="ViewUserControl<Adxstudio.Xrm.Cms.IPollPlacement>" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Liquid" %>
<%
	if (Model == null) return;

	var portalLiquidContext = new PortalLiquidContext(Html,
		ViewData[PortalExtensions.PortalViewContextKey] as IPortalViewContext);

	var drop = new PollPlacementDrop(portalLiquidContext, Model);

	var alias = ViewData["alias"] as string;
	var variables = new Dictionary<string, object>
	{
		{string.IsNullOrEmpty(alias) ? "placement" : alias, drop}
	};

	var random = (ViewData["random"] as bool?).GetValueOrDefault(true);
	variables["random"] = random;

	if (Model.WebTemplate == null)
	{
		Html.RenderLiquid(Html.Partial("PollPlacementTemplate"), variables);
	}
	else
	{
		Html.RenderWebTemplate(Model.WebTemplate, variables);
	}
%>