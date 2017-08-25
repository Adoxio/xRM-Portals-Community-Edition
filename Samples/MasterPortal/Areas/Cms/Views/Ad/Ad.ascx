<%@ Control Language="C#" Inherits="ViewUserControl<Adxstudio.Xrm.Cms.IAd>" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Liquid" %>
<%
	if (Model == null) return;

	var portalLiquidContext = new PortalLiquidContext(Html,
		ViewData[PortalExtensions.PortalViewContextKey] as IPortalViewContext);

	var variables = new Dictionary<string, object>();

	var alias = ViewData["alias"] as string;
	if (string.IsNullOrEmpty(alias))
	{
		alias = "ad";
	}
	variables[alias] = new AdDrop(portalLiquidContext, Model);

	var showcopy = (ViewData["showcopy"] as bool?).GetValueOrDefault(true);
	variables["show_copy"] = showcopy;

	if (Model.WebTemplate == null)
	{
		Html.RenderLiquid(Html.Partial("AdTemplate").ToHtmlString(), variables);
	}
	else
	{
		Html.RenderWebTemplate(Model.WebTemplate, variables);
	}
%>