<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" CodeBehind="Permits.aspx.cs" Inherits="Site.Areas.Permits.Pages.Permits" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Permits/css/permits.css") %>" />
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" ViewStateMode="Enabled" runat="server">
	<asp:ListView ID="PermitsListView" runat="server">
		<LayoutTemplate>
			<div class="permits row">
				<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
			</div>
		</LayoutTemplate>
		<ItemTemplate>
			<div class="col-xs-6 col-sm-4 col-md-3">
				<a class="well thumbnail permit text-center" href="<%#: Eval("Url") %>" title="<%#: Eval("Title") %>">
					<asp:Image ImageUrl='<%# GetThumbnailUrl(Eval("Entity")) %>' runat="server"/>
					<h6><%#: Eval("Title") %></h6>
				</a>
			</div>
		</ItemTemplate>
	</asp:ListView>
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarTop" runat="server"/>
