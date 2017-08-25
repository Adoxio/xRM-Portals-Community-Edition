<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="FullPage.aspx.cs" Inherits="Site.Pages.FullPage" ValidateRequest="false" EnableEventValidation="false" %>
<%@ Register src="~/Controls/ChildNavigation.ascx" tagname="ChildNavigation" tagprefix="site" %>
<%@ Register src="~/Controls/Comments.ascx" tagname="Comments" tagprefix="site" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<div class="page-metadata clearfix">
		<adx:Snippet SnippetName="Social Share Widget Code Page Bottom" EditType="text" DefaultText="" HtmlTag="Div" runat="server"/>
	</div>
	<site:ChildNavigation ShowDescriptions="true" runat="server" />
	<site:Comments ViewStateMode="Enabled" EnableRatings="False" runat="server" />
</asp:Content>
