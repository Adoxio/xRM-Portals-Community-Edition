<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<% Html.RenderLiquid((string)ViewBag.LiquidSource ?? string.Empty, "Substitution string", Html.ViewContext.Writer); %>