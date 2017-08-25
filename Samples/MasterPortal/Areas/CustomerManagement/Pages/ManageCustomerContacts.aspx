<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="ManageCustomerContacts.aspx.cs" Inherits="Site.Areas.CustomerManagement.Pages.ManageCustomerContacts" %>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<div class="pull-right">
			<adx:SiteMarkerLinkButton ID="CreateButton" runat="server" SiteMarkerName="Create Customer Contact" CssClass="btn btn-success">
				<span class="fa fa-plus-circle" aria-hidden="true"></span>
				<adx:Snippet runat="server" SnippetName="customermanagement/contact/list/CreateButtonLabel" DefaultText="<%$ ResourceManager:Create_New_DefaultText %>" Editable="true" EditType="text"/>
			</adx:SiteMarkerLinkButton>
		</div>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" ViewStateMode="Enabled" runat="server">
	<adx:Snippet ID="NoChannelPermissionsRecordError" SnippetName="customermanagement/contact/list/NoChannelPermissionsRecordError" DefaultText="<%$ ResourceManager:No_Channel_Permission_Manage_Customer_Contacts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ChannelPermissionsError" SnippetName="customermanagement/contact/list/ChannelPermissionsError" DefaultText="<%$ ResourceManager:Deny_Read_Write_Manage_Customer_Contacts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoParentAccountError" SnippetName="customermanagement/contact/list/NoParentAccountError" DefaultText="<%$ ResourceManager:Manage_Customer_Contacts_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ParentAccountClassificationCodeError" SnippetName="customermanagement/contact/list/ParentAccountClassificationCodeError" DefaultText="<%$ ResourceManager:Permission_To_Manage_Customer_Contacts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<asp:Panel ID="ContactsList" runat="server">
		<div id="customer-contacts-list">
			<asp:Panel ID="Filters" CssClass="row" runat="server">
				<div class="col-sm-3">
					<div class="input-group gridview-nav">
						<div class="input-group-addon">
							<asp:Label ID="ViewLabel" runat="server">View</asp:Label>
						</div>
						<asp:DropDownList ID="CustomerFilter" AutoPostBack="true" runat="server" CssClass="form-control" />
					</div>
				</div>
			</asp:Panel>
			<asp:GridView ID="CustomerContactsList" runat="server" CssClass="table table-striped" GridLines="None"  AlternatingRowStyle-CssClass="alternate-row" AllowSorting="true" OnSorting="CustomerContactsList_Sorting" OnRowDataBound="CustomerContactsList_OnRowDataBound" >
				<EmptyDataRowStyle CssClass="empty" />
				<EmptyDataTemplate>
					<adx:Snippet runat="server" SnippetName="customer-contacts/list/empty" DefaultText="<%$ ResourceManager:No_Items_To_Display %>" Editable="true" EditType="html" />
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
			$.blockUI({ message: null, overlayCSS: { opacity: .3} });
		}
	</script>
</asp:Content>