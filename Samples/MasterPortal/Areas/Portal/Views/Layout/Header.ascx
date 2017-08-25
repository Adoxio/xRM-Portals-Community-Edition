<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<% Html.RenderWebTemplate(Html.Website().GetAttribute("adx_headerwebtemplateid").Value as EntityReference,
	fallback: () => Html.RenderPartialFromSetting("Header/Template", "HeaderNavbar")); %>