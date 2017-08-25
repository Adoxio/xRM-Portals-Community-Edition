<%@ Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" CodeBehind="ServiceRequest.aspx.cs" Inherits="Site.Areas.Service311.Pages.ServiceRequest" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<asp:Content runat="server" ContentPlaceHolderID="Head">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Service311/css/311.css") %>" />
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<h1>
			<asp:Image ID="Thumbnail" runat="server"/> <adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server" ViewStateMode="Enabled">
	<asp:ListView ID="LatestArticlesList" runat="server">
		<LayoutTemplate>
			<div class="content-panel panel panel-default">
				<div class="panel-heading">
					<h4>
						<span class="fa fa-file-text-o" aria-hidden="true"></span>
						<adx:Snippet runat="server" SnippetName="311 Service Request Latest Knowledge Base Articles Title Text" DefaultText="<%$ ResourceManager:Latest_Knowledge_Base_Articles %>" />
					</h4>
				</div>
				<div class="list-group">
					<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
				</div>
			</div>
		</LayoutTemplate>
		<ItemTemplate>
			<asp:HyperLink CssClass="list-group-item" NavigateUrl='<%# GetKbArticleUrl(Container.DataItem as Entity) %>' Text='<%# HtmlEncode(((Entity)Container.DataItem).GetAttributeValue<string>("title")) %>' runat="server" />
		</ItemTemplate>
	</asp:ListView>
	<asp:HyperLink runat="server" ID="CreateRequestLink" CssClass="btn btn-success btn-lg">
		<span class="fa fa-plus" aria-hidden="true"></span>&nbsp;<%: Html.SnippetLiteral("311 Service Request Create Button Text", ResourceManager.GetString("Create_A_New")) %>&nbsp;<%: Html.AttributeLiteral("adx_title") %>
	</asp:HyperLink>
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarTop" runat="server">
	
</asp:Content>
