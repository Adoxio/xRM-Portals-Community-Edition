<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/WebFormsContent.master" CodeBehind="ContactUs.aspx.cs" Inherits="Site.Areas.ContactUs.Pages.ContactUs" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Register TagPrefix="site" Namespace="Site.Controls" Assembly="Site" %>

<asp:Content  ContentPlaceHolderID="MainContent" runat="server">
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
		<crm:CrmDataSource ID="WebFormDataSource" runat="server" />
		<adx:CrmEntityFormView runat="server" ID="FormView" DataSourceID="WebFormDataSource" 
			CssClass="crmEntityFormView" 
			FormName="Web Form" 
			EntityName="lead" 
			OnItemInserted="OnItemInserted" 
			ValidationGroup="ContactUs"
			ValidationSummaryCssClass="alert alert-danger alert-block"
			LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
			ContextName="<%$ SiteSetting: Language Code %>">
		</adx:CrmEntityFormView>

	<asp:Panel ID="ConfirmationMessage" runat="server" Visible="false">
		<adx:Snippet runat="server" SnippetName="ContactUs/ConfirmationMsg" DefaultText="<%$ ResourceManager:Thank_You_DefaultText %>" EditType="html" />
	</asp:Panel>
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarTop" runat="server">
	<div class="content-panel panel panel-default">
		<div class="panel-heading">
			<h4>
				<%: Html.TextSnippet("contact-us/instructions", defaultValue: ResourceManager.GetString("Instructions_Defaulttext"), tagName: "span") %>
			</h4>
		</div>
		<div class="panel-body">
			<%: Html.HtmlAttribute("adx_copy") %>
		</div>
	</div>
	<site:ChildNavigation runat="server"/>
</asp:Content>