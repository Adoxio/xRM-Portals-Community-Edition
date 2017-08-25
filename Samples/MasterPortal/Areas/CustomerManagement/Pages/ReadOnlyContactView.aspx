<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="ReadOnlyContactView.aspx.cs" Inherits="Site.Areas.CustomerManagement.Pages.ReadOnlyContactView" %>

<asp:Content  ContentPlaceHolderID="ContentBottom" runat="server">
	<adx:Snippet ID="RecordNotFoundError" SnippetName="customermanagement/contact/read/RecordNotFoundError" DefaultText="<%$ ResourceManager:Customer_Contact_Not_Found_Invalid_ContactID_Specified %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoChannelPermissionsRecordError" SnippetName="customermanagement/contact/read/NoChannelPermissionsRecordError" DefaultText="<%$ ResourceManager:No_Channel_Permission_Read_Customer_Contacts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ChannelPermissionsError" SnippetName="customermanagement/contact/read/ChannelPermissionsError" DefaultText="<%$ ResourceManager:Deny_Read_View_Customer_Contacts_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoParentAccountError" SnippetName="customermanagement/contact/read/NoParentAccountError" DefaultText="<%$ ResourceManager:Read_Customer_Contacts_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoChannelPermissionsForParentAccountError" SnippetName="customermanagement/contact/read/NoChannelPermissionsForParentAccountError" DefaultText="<%$ ResourceManager:No_Channel_Permissions_For_Parent_Account_Read_Customer_Contacts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ParentAccountClassificationCodeError" SnippetName="customermanagement/contact/read/ParentAccountClassificationCodeError" DefaultText="<%$ ResourceManager:Permission_To_Read_Customer_Contacts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<asp:Panel ID="ContactForm" CssClass="crmEntityFormView" runat="server">
		<adx:CrmEntityFormView runat="server" 
			ID="FormView" EntityName="contact" 
			FormName="Opportunity Contact Details Form" 
			RecommendedFieldsRequired="True" 
			ShowUnsupportedFields="False" 
			ToolTipEnabled="False" 
			Mode="ReadOnly"
			LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
			ContextName="<%$ SiteSetting: Language Code %>">
		</adx:CrmEntityFormView>
	</asp:Panel>
</asp:Content>