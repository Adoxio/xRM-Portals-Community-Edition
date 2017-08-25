<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="CreatePortalContact.aspx.cs" Inherits="Site.Areas.AccountManagement.Pages.CreatePortalContact" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content  ContentPlaceHolderID="ContentBottom" ViewStateMode="Enabled" runat="server">
	<adx:Snippet ID="NoParentAccountError" SnippetName="accountmanagement/contact/create/NoParentAccountError" DefaultText="<%$ ResourceManager:Create_Contact_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoContactAccessPermissionsRecordError" SnippetName="accountmanagement/contact/create/NoContactAccessPermissionsRecordError" DefaultText="<%$ ResourceManager:No_Contact_Access_Permissions_Create_Contact_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ContactAccessPermissionsError" SnippetName="accountmanagement/contact/create/ContactAccessPermissionsError" DefaultText="<%$ ResourceManager:Deny_Create_Permission_To_Create_Contact_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoContactAccessPermissionsForParentAccountError" SnippetName="accountmanagement/contact/create/NoContactAccessPermissionsForParentAccountError" DefaultText="<%$ ResourceManager:No_Contact_Access_For_Parent_Account_Permissions_Create_Contact_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:CrmDataSource ID="WebFormDataSource" runat="server" CrmDataContextName="<%$ SiteSetting: Language Code %>" />
	<asp:Panel ID="ContactWebForm" CssClass="crmEntityFormView" runat="server" Visible="true">
		<adx:CrmEntityFormView runat="server" ID="ContactFormView"
			EntityName="contact"
			FormName="Contact Web Form"
			OnItemInserted="OnItemInserted"
			RecommendedFieldsRequired="True"
			ShowUnsupportedFields="False"
			DataSourceID="WebFormDataSource"
			ToolTipEnabled="False"
			Mode="Insert"
			ValidationGroup="CreatePortalContact"
			LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
			ContextName="<%$ SiteSetting: Language Code %>">
			<InsertItemTemplate></InsertItemTemplate>
		</adx:CrmEntityFormView>
		<div class="actions">
			<asp:Button ID="SubmitButton" 
				Text='<%$ Snippet: accountmanagement/contact/create/CreateButtonLabel, Create %>' 
				CssClass="btn btn-primary"
				OnClick="SubmitButton_Click"
				ValidationGroup="CreatePortalContact"
				CausesValidation="true"
				runat="server" />
			<asp:Button ID="InviteAndSave" 
				Text='<%$ Snippet: accountmanagement/contact/create/CreateAndInviteButtonLabel, Create & Send Invitation Email %>' 
				CssClass="btn btn-default"
				OnClick="InviteAndSaveButton_Click" 
				ValidationGroup="CreatePortalContact" 
				CausesValidation="true"
				runat="server" />
		</div>
	</asp:Panel>
</asp:Content>