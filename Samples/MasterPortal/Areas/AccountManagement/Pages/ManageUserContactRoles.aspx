<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="ManageUserContactRoles.aspx.cs" Inherits="Site.Areas.AccountManagement.Pages.ManageUserContactRoles" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="~/Areas/AccountManagement/css/account-management.css">
</asp:Content>

<asp:Content  ContentPlaceHolderID="PageHeader" ViewStateMode="Enabled" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" /> &ndash; <em class="text-muted"><%= ContactToEdit != null ? ContactToEdit.GetAttributeValue<string>("fullname") : string.Empty %></em>
		</h1>
	</div>
</asp:Content>

<asp:Content  ContentPlaceHolderID="ContentBottom" ViewStateMode="Enabled" runat="server">
	<adx:Snippet ID="RecordNotFoundError" SnippetName="customermanagement/contact/editpermissions/RecordNotFoundError" DefaultText="<%$ ResourceManager:Contact_Not_Found_Invalid_ContactID_Specified_Exception %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="OpportunityPermissionsRecordForContactEditNotFoundError" SnippetName="accountmanagement/contact/editpermissions/OpportunityPermissionsRecordForContactEditNotFoundError" DefaultText="<%$ ResourceManager:Contact_Not_Have_Opportunity_Permission_Record %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="ChannelPermissionsRecordForContactEditNotFoundError" SnippetName="accountmanagement/contact/editpermissions/ChannelPermissionsRecordForContactEditNotFoundError" DefaultText="<%$ ResourceManager:Contact_Not_Have_Channel_Permission_Record %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="PermissionsRecordsForContactEditNotFoundError" SnippetName="accountmanagement/contact/editpermissions/PermissionsRecordsForContactEditNotFoundError" DefaultText="<%$ ResourceManager:Contact_Not_Have_Channel_Opportunity_Permission_Record %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoAccountAccessPermissionsRecordError" SnippetName="accountmanagement/contact/editpermissions/NoAccountAccessPermissionsRecordError" DefaultText="<%$ ResourceManager:Permission_To_Manage_Contact_Permissions_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoAccountAccessPermissionsForParentAccountError" SnippetName="accountmanagement/contact/editpermissions/NoAccountAccessPermissionsForParentAccountError" DefaultText="<%$ ResourceManager:No_Permission_For_Parent_Account_Manage_Contact_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoParentAccountError" SnippetName="accountmanagement/contact/editpermissions/NoParentAccountError" DefaultText="<%$ ResourceManager:Permission_For_Edit_Contact_Permissions_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-danger" runat="server"/>
	<adx:Snippet ID="NoContactAccessPermissionsRecordError" SnippetName="accountmanagement/contact/editpermissions/NoContactAccessPermissionsRecordError" DefaultText="<%$ ResourceManager:No_Contact_Access_Permissions_Edit_Contact_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-warning" runat="server"/>
	<adx:Snippet ID="ContactAccessPermissionsError" SnippetName="accountmanagement/contact/editpermissions/ContactAccessPermissionsError" DefaultText="<%$ ResourceManager:Deny_Write_Permission_To_Edit_Contact_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-warning" runat="server"/>
	<adx:Snippet ID="NoContactAccessPermissionsForParentAccountError" SnippetName="accountmanagement/contact/editpermissions/NoContactAccessPermissionsForParentAccountError" DefaultText="<%$ ResourceManager:No_Contact_Access_For_Parent_Account_Permissions_Edit_Contact_Permission_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-warning" runat="server"/>	
	<adx:Snippet ID="AccountAccessManagePermissionsDeniedError" SnippetName="accountmanagement/contact/editpermissions/AccountAccessManagePermissionsDeniedError" DefaultText="<%$ ResourceManager:Account_Access_Manage_Permissions_Denied_Error %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-warning" runat="server"/>
	<adx:Snippet ID="NoPermissionsError" SnippetName="accountmanagement/contact/editpermissions/NoPermissionsError" DefaultText="<%$ ResourceManager:Permission_To_Edit_ContactChannel_And_Opportunity_Permissions_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-warning" runat="server"/>
	<adx:Snippet ID="UpdateSuccessMessage" SnippetName="accountmanagement/contact/editpermissions/UpdateSuccessMessage" DefaultText="<%$ ResourceManager:Permissions_Updated_Successfully_Message %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-success" runat="server" />
	<asp:Panel ID="ManagePermissions" CssClass="crmEntityFormView" runat="server">
		<asp:Panel ID="OpportunityPermissions" runat="server">
			<adx:CrmEntityFormView runat="server" ID="OpportunityPermissionsFormView"
				EntityName="adx_opportunitypermissions"
				FormName="Permissions Management Web Form"
				OnItemUpdating="OppPermissionsUpdating"
				OnItemUpdated="OppPermissionsUpdated" 
				ValidationGroup="Profile"
				RecommendedFieldsRequired="True"
				ShowUnsupportedFields="False"
				ToolTipEnabled="False"
				Mode="Edit"
				LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
				ContextName="<%$ SiteSetting: Language Code %>">
				<UpdateItemTemplate>
				</UpdateItemTemplate>
			</adx:CrmEntityFormView>
			<table class="tab section" id="opportunity-permissions">
				<tr>
					<td colspan="1" rowspan="1" class="cell checkbox-cell">
						<div class="info">
							<label for="OppCreateCheckBox">Create</label>
						</div>
						<div class="control">
							<span class="checkbox">
								<asp:CheckBox ID="OppCreateCheckBox" ClientIDMode="Static" runat="server" /> 
							</span>
						</div>
					</td>
					<td colspan="1" rowspan="1" class="cell checkbox-cell">
						<div class="info">
							<label for="OppDeleteCheckBox">Delete</label>
						</div>
						<div class="control">
							<span class="checkbox">
								<asp:CheckBox ID="OppDeleteCheckBox" ClientIDMode="Static" runat="server" />
							</span>
						</div>
					</td>
				</tr>
				<tr>
					<td colspan="1" rowspan="1" class="cell checkbox-cell">
						<div class="info">
							<label for="OppAcceptDeclineCheckBox">Accept/Decline</label>
						</div>
						<div class="control">
							<span class="checkbox">
								<asp:CheckBox ID="OppAcceptDeclineCheckBox" ClientIDMode="Static" runat="server" /> 
							</span>
						</div>
					</td>
					<td colspan="1" rowspan="1" class="cell checkbox-cell">
						<div class="info">
							<label for="OppAssignCheckBox">Assign</label>
						</div>
						<div class="control">
							<span class="checkbox">
								<asp:CheckBox ID="OppAssignCheckBox" ClientIDMode="Static" runat="server" />
							</span>
						</div>
					</td>
				</tr>
			</table>
		</asp:Panel>
		<asp:Panel ID="ChannelPermissions" runat="server">
			<adx:CrmEntityFormView runat="server" ID="ChannelPermissionsFormView"
				EntityName="adx_channelpermissions" 
				FormName="Permissions Management Web Form"
				OnItemUpdating="ChannelPermissionsUpdating"
				OnItemUpdated="ChannelPermissionsUpdated"
				ValidationGroup="ManagePermissions"
				RecommendedFieldsRequired="True"
				ShowUnsupportedFields="False"
				ToolTipEnabled="False"
				Mode="Edit"
				LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
				ContextName="<%$ SiteSetting: Language Code %>">
				<UpdateItemTemplate>
				</UpdateItemTemplate>
			</adx:CrmEntityFormView>
			<table class="tab section" id="channel-permissions">
				<tr>
					<td colspan="1" rowspan="1" class="cell checkbox-cell">
						<div class="info">
							<label for="ChannelWriteCheckBox">Write</label>
						</div>
						<div class="control">
							<span class="checkbox">
								<asp:CheckBox ID="ChannelWriteCheckBox" ClientIDMode="Static" runat="server" /> 
							</span>
						</div>
					</td>
					<td colspan="1" rowspan="1" class="cell checkbox-cell">
						<div class="info">
							<label for="ChannelCreateCheckBox">Create</label>
						</div>
						<div class="control">
							<span class="checkbox">
								<asp:CheckBox ID="ChannelCreateCheckBox" ClientIDMode="Static" runat="server" /> 
							</span>
						</div>
					</td>
				</tr>
			</table>
		</asp:Panel>
		<adx:Snippet ID="NoChannelPermissionsRecordWarning" SnippetName="accountmanagement/contact/editpermissions/NoChannelPermissionsRecordWarning" DefaultText="<%$ ResourceManager:No_Channel_Permission_Edit_Contact_Channel_Permissions_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-warning" runat="server"/>
		<adx:Snippet ID="NoOpportunityPermissionsRecordWarning" SnippetName="accountmanagement/contact/editpermissions/NoOpportunityPermissionsRecordWarning" DefaultText="<%$ ResourceManager:Permission_To_Edit_Contact_Opportunity_Permissions_Denied %>" Editable="true" EditType="html" Visible="false" CssClass="alert alert-block alert-warning" runat="server"/>
		<div class="actions">
			<asp:Button ID="SubmitButton" Text='<%$ Snippet: accountmanagement/contact/editpermissions/UpdateButtonLabel, Update %>' CssClass="btn btn-primary" OnClick="SubmitButton_Click" ValidationGroup="ManagePermissions" runat="server" />
		</div>
	</asp:Panel>
</asp:Content>