<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<%: Html.WebLinksDropdowns("Primary Navigation", "weblinks", "nav nav-tabs", "active", "active", clientSiteMapState: true) %>