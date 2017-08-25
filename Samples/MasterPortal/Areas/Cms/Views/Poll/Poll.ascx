<%@ Control Language="C#" Inherits="ViewUserControl<Adxstudio.Xrm.Cms.IPoll>" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Liquid" %>
<%
	if (Model == null) return;

	var portalLiquidContext = new PortalLiquidContext(Html,
		ViewData[PortalExtensions.PortalViewContextKey] as IPortalViewContext);

	var alias = ViewData["alias"] as string;
	var variables = new Dictionary<string, object>
	{
		{string.IsNullOrEmpty(alias) ? "poll" : alias, new PollDrop(portalLiquidContext, Model)}
	};

	if (Model.WebTemplate == null)
	{
		Html.RenderLiquid(Html.Partial("PollTemplate"), variables);
	}
	else
	{
		Html.RenderWebTemplate(Model.WebTemplate, variables);
	}
%>