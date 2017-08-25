<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<% Html.RenderWebTemplate(Html.Website().GetAttribute("adx_footerwebtemplateid").Value as EntityReference,
	fallback: () => Html.RenderPartialFromSetting("Footer/Template", "FooterMenuWithCopy")); %>