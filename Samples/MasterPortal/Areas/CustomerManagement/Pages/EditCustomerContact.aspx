<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="EditCustomerContact.aspx.cs" Inherits="Site.Areas.CustomerManagement.Pages.EditCustomerContact" %>

<asp:Content  ContentPlaceHolderID="ContentBottom" ViewStateMode="Enabled" runat="server">
	<adx:Snippet ID="RecordNotFoundError" SnippetName="customermanagement/contact/edit/RecordNotFoundError" DefaultText="<%$ ResourceManager:Customer_Contact_Not_Found_Invalid_CustomerID_Specified %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoChannelPermissionsRecordError" SnippetName="customermanagement/contact/edit/NoChannelPermissionsRecordError" DefaultText="<%$ ResourceManager:No_Channel_Permission_Edit_Customer_Contacts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ChannelPermissionsError" SnippetName="customermanagement/contact/edit/ChannelPermissionsError" DefaultText="<%$ ResourceManager:Deny_Read_Edit_Customer_Contacts_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoParentAccountError" SnippetName="customermanagement/contact/edit/NoParentAccountError" DefaultText="<%$ ResourceManager:Edit_Customer_Contacts_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoChannelPermissionsForParentAccountError" SnippetName="customermanagement/contact/edit/NoChannelPermissionsForParentAccountError" DefaultText="<%$ ResourceManager:No_Channel_Permissions_For_Parent_Account_Edit_Customer_Contacts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ParentAccountClassificationCodeError" SnippetName="customermanagement/contact/edit/ParentAccountClassificationCodeError" DefaultText="<%$ ResourceManager:Permission_To_Edit_Customer_Contacts_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="SuccessMessage" SnippetName="customermanagement/contact/edit/SuccessMessage" DefaultText="<%$ ResourceManager:Contact_Information_Updated_Successfully_Message %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-success" runat="server"/>
	<asp:Panel ID="EditContactForm" CssClass="crmEntityFormView" runat="server">
		<adx:CrmEntityFormView runat="server" 
			ID="ContactFormView" 
			EntityName="contact" 
			FormName="Contact Web Form" 
			OnItemUpdating="OnItemUpdating" 
			OnItemUpdated="OnItemUpdated" 
			ValidationGroup="UpdateContact" 
			RecommendedFieldsRequired="True" 
			ShowUnsupportedFields="False" 
			ToolTipEnabled="False"
			Mode="Edit"
			LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
			ContextName="<%$ SiteSetting: Language Code %>">
			<UpdateItemTemplate></UpdateItemTemplate>
		</adx:CrmEntityFormView>
		<div class="actions">
			<asp:Button ID="UpdateContactButton" Text='<%$ Snippet: customermanagement/contact/edit/UpdateContactButtonLabel, Update %>' CssClass="btn btn-primary" OnClick="UpdateContactButton_Click" CausesValidation="true" ValidationGroup="UpdateContact" runat="server" />
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