<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="EditCustomerAccount.aspx.cs" Inherits="Site.Areas.CustomerManagement.Pages.EditCustomerAccount" %>

<asp:Content  ContentPlaceHolderID="ContentBottom" ViewStateMode="Enabled" runat="server">
	<adx:Snippet ID="RecordNotFoundError" SnippetName="customermanagement/account/edit/RecordNotFoundError" DefaultText="<%$ ResourceManager:Customer_Account_Not_Found_Invalid_AccountID_Specified %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoChannelPermissionsRecordError" SnippetName="customermanagement/account/edit/NoChannelPermissionsRecordError" DefaultText="<%$ ResourceManager:No_Channel_Permission_Edit_Customer_Accounts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ChannelPermissionsError" SnippetName="customermanagement/account/edit/ChannelPermissionsError" DefaultText="<%$ ResourceManager:Deny_Read_Edit_Customer_Accounts_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoParentAccountError" SnippetName="customermanagement/account/edit/NoParentAccountError" DefaultText="<%$ ResourceManager:Edit_Customer_Accounts_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoChannelPermissionsForParentAccountError" SnippetName="customermanagement/account/edit/NoChannelPermissionsForParentAccountError" DefaultText="<%$ ResourceManager:No_Channel_Permissions_For_Parent_Account_Edit_Customer_Accounts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ParentAccountClassificationCodeError" SnippetName="customermanagement/account/edit/ParentAccountClassificationCodeError" DefaultText="<%$ ResourceManager:Permission_To_Edit_Customer_Accounts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="SuccessMessage" SnippetName="customermanagement/account/edit/SuccessMessage" DefaultText="<%$ ResourceManager:Account_Information_Updated_Success_Message %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-success" runat="server"/>
	<asp:Panel ID="EditAccountForm" CssClass="panel panel-default" runat="server">
		<div class="panel-body">
			<asp:Panel ID="AccountForm" CssClass="crmEntityFormView" runat="server" Visible="true">
				<adx:CrmEntityFormView runat="server" 
					ID="FormView" 
					EntityName="account" 
					FormName="Account Web Form" 
					OnItemUpdating="OnItemUpdating" 
					OnItemUpdated="OnItemUpdated" 
					ValidationGroup="UpdateAccount" 
					RecommendedFieldsRequired="True" 
					ShowUnsupportedFields="False" 
					ToolTipEnabled="False" 
					Mode="Edit"
					LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
					ContextName="<%$ SiteSetting: Language Code %>">
					<UpdateItemTemplate>
					</UpdateItemTemplate>
				</adx:CrmEntityFormView>
				<div class="well">
					<adx:Snippet ID="NoContactsExistWarningMessage" SnippetName="customermanagement/account/edit/CreateContactMessage" DefaultText="<%$ ResourceManager:No_Contacts_Exist_Create_Appropriate_Contact %>" Editable="true" EditType="html"  Visible="false" CssClass="alert alert-block alert-warning" runat="server"/>
					<asp:Label ID="PrimaryContactLabel" AssociatedControlID="PrimaryContactList" runat="server">
						<adx:Snippet runat="server" SnippetName="customermanagement/account/edit/PrimaryContactFieldLabel" DefaultText="<%$ ResourceManager:Set_Primary_Contact_Message %>" />
					</asp:Label>
					<asp:DropDownList ID="PrimaryContactList" runat="server" ClientIDMode="Static" CssClass="form-control" />
				</div>
				<div class="actions">
					<asp:Button ID="UpdateAccountButton" 
						Text='<%$ Snippet: customermanagement/account/edit/AccountUpdateButtonLabel, Update %>' 
						CssClass="btn btn-primary" 
						OnClick="UpdateAccountButton_Click" 
						ValidationGroup="UpdateAccount" 
						runat="server" />
				</div>
			</asp:Panel>
		</div>
	</asp:Panel>
	<asp:Panel ID="ContactsList" CssClass="panel panel-default" runat="server">
		<div class="panel-heading">
			<div class="pull-right">
				<adx:SiteMarkerLinkButton ID="CreateContactButton" SiteMarkerName="Create Customer Contact" runat="server" CssClass="btn btn-success btn-xs">
					<span class="fa fa-plus-circle" aria-hidden="true"></span>
					<adx:Snippet runat="server" SnippetName="customermanagement/account/edit/CreateContactButtonLabel" DefaultText="<%$ ResourceManager:Create_New_DefaultText %>" Editable="true" EditType="text" />
				</adx:SiteMarkerLinkButton>
			</div>
			<h4 class="panel-title">
				<span class="fa fa-user" aria-hidden="true"></span>
				<adx:Snippet runat="server" SnippetName="customermanagement/account/edit/ContactsListTitle" DefaultText="<%$ ResourceManager:Account_Contacts %>" Editable="true" EditType="text" />
			</h4>
		</div>
		<div class="panel-body">
			<div id="account-contacts">
				<asp:GridView ID="AccountContactsList" runat="server" CssClass="table table-striped" GridLines="None"  AlternatingRowStyle-CssClass="alternate-row" AllowSorting="true" OnSorting="AccountContactsList_Sorting" OnRowDataBound="AccountContactsList_OnRowDataBound" >
					<EmptyDataRowStyle CssClass="empty" />
					<EmptyDataTemplate>
						<adx:Snippet runat="server" SnippetName="customermanagement/account/edit/ContactsListEmptyText" DefaultText="<%$ ResourceManager:No_Contact_Records_To_Display %>" Editable="true" EditType="html" />
					</EmptyDataTemplate>
				</asp:GridView>
			</div>
		</div>
	</asp:Panel>
</asp:Content>

<asp:Content ContentPlaceHolderID="Scripts" runat="server">
	<script type="text/javascript">
		$(function () {
			$("form").submit(function () {
				if (Page_IsValid) {
					$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
				}
			});
		});
	</script>
</asp:Content>