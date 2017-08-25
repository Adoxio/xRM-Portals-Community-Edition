<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="ManageCustomerAccounts.aspx.cs" Inherits="Site.Areas.CustomerManagement.Pages.ManageCustomerAccounts" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<div class="pull-right">
			<adx:SiteMarkerLinkButton ID="CreateButton" runat="server" SiteMarkerName="Create Customer Account" CssClass="btn btn-success" Visible="False">
				<span class="fa fa-plus-circle" aria-hidden="true"></span>
				<adx:Snippet runat="server" SnippetName="customermanagement/account/list/CreateButtonLabel" DefaultText="<%$ ResourceManager:Create_New_DefaultText %>" Editable="true" EditType="text"/>
			</adx:SiteMarkerLinkButton>
		</div>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
	</div>
</asp:Content>

<asp:Content  ContentPlaceHolderID="ContentBottom" ViewStateMode="Enabled" runat="server">
	<adx:Snippet ID="NoChannelPermissionsRecordError" SnippetName="customermanagement/account/list/NoChannelPermissionsRecordError" DefaultText="<%$ ResourceManager:No_Channel_Permission_Manage_Customer_Accounts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ChannelPermissionsError" SnippetName="customermanagement/account/list/ChannelPermissionsError" DefaultText="<%$ ResourceManager:Deny_Read_Write_Manage_Customer_Accounts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoParentAccountError" SnippetName="customermanagement/account/list/NoParentAccountError" DefaultText="<%$ ResourceManager:Manage_Customer_Accounts_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ParentAccountClassificationCodeError" SnippetName="customermanagement/account/list/ParentAccountClassificationCodeError" DefaultText="<p><%$ ResourceManager:Permission_To_Manage_Customer_Accounts_Denied %></p><p><%$ ResourceManager:Account_Classification_Code_Must_Set_To_Partner_Message %></p>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<asp:Panel ID="AccountsList" runat="server">
		<div id="customer-accounts-list">
			<asp:GridView ID="CustomerAccountsList" runat="server" CssClass="table table-striped" GridLines="None" AlternatingRowStyle-CssClass="alternate-row" AllowSorting="true" OnSorting="CustomerAccountsList_Sorting" OnRowDataBound="CustomerAccountsList_OnRowDataBound" >
				<EmptyDataRowStyle CssClass="empty" />
				<EmptyDataTemplate>
					<adx:Snippet runat="server" SnippetName="customer-accounts/list/empty" DefaultText="<%$ ResourceManager:No_Items_To_Display %>" Editable="true" EditType="html" />
				</EmptyDataTemplate>
			</asp:GridView>
		</div>
	</asp:Panel>
</asp:Content>

<asp:Content ContentPlaceHolderID="Scripts" runat="server">
	<script type="text/javascript">
		$(function () {
			$(".table tr").not(":has(th)").click(function () {
				window.location.href = $(this).find("a").attr("href");
			});
			$("form").submit(function () {
				blockUI();
			});
			$(".table th a").click(function () {
				blockUI();
			});
		});
		function blockUI() {
			$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
		}
	</script>
</asp:Content>