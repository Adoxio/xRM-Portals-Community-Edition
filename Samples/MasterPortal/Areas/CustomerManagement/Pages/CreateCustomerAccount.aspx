<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="CreateCustomerAccount.aspx.cs" Inherits="Site.Areas.CustomerManagement.Pages.CreateCustomerAccount" %>

<asp:Content ContentPlaceHolderID="ContentBottom" ViewStateMode="Enabled" runat="server">
	<adx:Snippet ID="NoChannelPermissionsRecordError" SnippetName="customermanagement/account/create/NoChannelPermissionsRecordError" DefaultText="<%$ ResourceManager:No_Channel_Permissions_Create_Customer_Account_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ChannelPermissionsError" SnippetName="customermanagement/account/create/ChannelPermissionsError" DefaultText="<%$ ResourceManager:Channel_Permissions_Deny_Manage_Customer_Accounts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoParentAccountError" SnippetName="customermanagement/account/create/NoParentAccountError" DefaultText="<%$ ResourceManager:Create_Customer_Accounts_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoChannelPermissionsForParentAccountError" SnippetName="customermanagement/account/create/NoChannelPermissionsForParentAccountError" DefaultText="<%$ ResourceManager:No_Channel_Permissions_For_Parent_Account_Create_Customer_Account_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ParentAccountClassificationCodeError" SnippetName="customermanagement/account/create/ParentAccountClassificationCodeError" DefaultText="<%$ ResourceManager:Permission_To_Create_Customer_Accounts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<asp:Panel ID="CreateAccountForm" CssClass="crmEntityFormView" runat="server">
		<adx:CrmDataSource ID="WebFormDataSource" runat="server" CrmDataContextName="<%$ SiteSetting: Language Code %>" />
		<adx:CrmEntityFormView runat="server" ID="AccountFormView" EntityName="account" FormName="Account Web Form" ValidationGroup="CreateAccount" OnItemInserting="OnItemInserting" OnItemInserted="OnItemInserted" RecommendedFieldsRequired="True" ShowUnsupportedFields="False" DataSourceID="WebFormDataSource" ToolTipEnabled="False" Mode="Insert" LanguageCode="<%$ SiteSetting: Language Code, 0 %>" ContextName="<%$ SiteSetting: Language Code %>">
			<InsertItemTemplate>
			</InsertItemTemplate>
		</adx:CrmEntityFormView>
		<div class="actions">
			<asp:Button ID="CreateAccountButton" 
				Text='<%$ Snippet: customermanagement/account/create/CreateAccountButtonLabel, Create %>' 
				CssClass="btn btn-primary"
				CausesValidation="true"
				ValidationGroup="CreateAccount" 
				OnClick="CreateAccountButton_Click" 
				runat="server" />
		</div>
	</asp:Panel>
</asp:Content>