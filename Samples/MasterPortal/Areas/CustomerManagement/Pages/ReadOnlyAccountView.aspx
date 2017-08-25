<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="ReadOnlyAccountView.aspx.cs" Inherits="Site.Areas.CustomerManagement.Pages.ReadOnlyAccountView" %>

<asp:Content  ContentPlaceHolderID="ContentBottom" runat="server">
	<adx:Snippet ID="RecordNotFoundError" SnippetName="customermanagement/account/read/RecordNotFoundError" DefaultText="<%$ ResourceManager:Customer_Account_Not_Found_Invalid_AccountID_Specified %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoChannelPermissionsRecordError" SnippetName="customermanagement/account/read/NoChannelPermissionsRecordError" DefaultText="<%$ ResourceManager:No_Channel_Permission_Read_Customer_Accounts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ChannelPermissionsError" SnippetName="customermanagement/account/read/ChannelPermissionsError" DefaultText="<%$ ResourceManager:Deny_Read_View_Customer_Accounts_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoParentAccountError" SnippetName="customermanagement/account/read/NoParentAccountError" DefaultText="<%$ ResourceManager:Read_Customer_Accounts_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoChannelPermissionsForParentAccountError" SnippetName="customermanagement/account/read/NoChannelPermissionsForParentAccountError" DefaultText="<%$ ResourceManager:No_Channel_Permissions_For_Parent_Account_Read_Customer_Accounts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ParentAccountClassificationCodeError" SnippetName="customermanagement/account/read/ParentAccountClassificationCodeError" DefaultText="<%$ ResourceManager:Permission_To_Read_Customer_Accounts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<asp:Panel ID="AccountForm" CssClass="crmEntityFormView" runat="server">
		<adx:CrmEntityFormView runat="server" 
			ID="FormView" EntityName="account" 
			FormName="Account Read Only Web Form" 
			RecommendedFieldsRequired="True" 
			ShowUnsupportedFields="False" 
			ToolTipEnabled="False" 
			Mode="ReadOnly"
			LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
			ContextName="<%$ SiteSetting: Language Code %>">
		</adx:CrmEntityFormView>
	</asp:Panel>
</asp:Content>