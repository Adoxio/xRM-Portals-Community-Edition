<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebLinkNavigation.master" AutoEventWireup="true" CodeBehind="WebLinkNavigationPage.aspx.cs" Inherits="Site.Pages.WebLinkNavigationPage" %>
<%@ Register src="~/Controls/Comments.ascx" tagname="Comments" tagprefix="site" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<div class="page-metadata clearfix">
		<adx:Snippet SnippetName="Social Share Widget Code Page Bottom" EditType="text" DefaultText="" HtmlTag="Div" runat="server"/>
	</div>
	<%: Html.Rating(Entity != null ? Entity.ToEntityReference() : null, panel: true) %>
	<site:Comments ViewStateMode="Enabled" EnableRatings="False" runat="server" />
</asp:Content>