<%@ Page Language="C#" MasterPageFile="~/MasterPages/Default.master" AutoEventWireup="True" CodeBehind="Home.aspx.cs" Inherits="Site.Areas.Customer.Pages.Home" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Register tagPrefix="site" tagName="EventsPanel" src="~/Controls/EventsPanel.ascx" %>
<%@ Register tagPrefix="site" tagName="ForumsPanel" src="~/Controls/ForumsPanel.ascx" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<%: Html.HtmlAttribute("adx_copy", cssClass: "page-copy") %>
	<div class="row">
		<div class="col-md-8">
			<site:ForumsPanel runat="server" />
		</div>
		<div class="col-md-4">
			<site:EventsPanel runat="server" />
		</div>
	</div>
</asp:Content>
