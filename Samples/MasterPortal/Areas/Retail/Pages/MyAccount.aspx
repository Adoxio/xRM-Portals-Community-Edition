<%@ Language="C#" MasterPageFile="~/MasterPages/Profile.master" AutoEventWireup="true" CodeBehind="MyAccount.aspx.cs" Inherits="Site.Areas.Retail.Pages.MyAccount" %>

<asp:Content runat="server" ContentPlaceHolderID="EntityControls">
	<script type="text/javascript">
		function entityFormClientValidate() {
			// Custom client side validation. Method is called by the submit button's onclick event.
			// Must return true or false. Returning false will prevent the form from submitting.
			return true;
		}

		function webFormClientValidate() {
			// Custom client side validation. Method is called by the next/submit button's onclick event.
			// Must return true or false. Returning false will prevent the form from submitting.
			return true;
		}
	</script>
	<adx:EntityForm ID="EntityFormControl" runat="server" FormCssClass="crmEntityFormView readonly centered" PreviousButtonCssClass="btn btn-default" NextButtonCssClass="btn btn-primary" SubmitButtonCssClass="btn btn-primary" ClientIDMode="Static" LanguageCode="<%$ SiteSetting: Language Code, 0 %>" PortalName="<%$ SiteSetting: Language Code %>" />
	<asp:Panel runat="server" ID="Household">
		<fieldset>
			<legend>
				<adx:Snippet SnippetName="My Account Household Form Legend" DefaultText="<%$ ResourceManager:Household_DefaultText %>" EditType="text" runat="server"/>
			</legend>
			<adx:CrmDataSource ID="HouseholdDataSource" runat="server" CrmDataContextName="<%$ SiteSetting: Language Code %>" />
			<adx:CrmEntityFormView ID="HouseholdFormView" runat="server"
				DataSourceID="HouseholdDataSource"
				CssClass="crmEntityFormView readonly"
				EntityName="account"
				FormName="Household Summary Web Form"
				Mode="ReadOnly"
				LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
				ContextName="<%$ SiteSetting: Language Code %>">
			</adx:CrmEntityFormView>
		</fieldset>
		<fieldset>
			<legend>
				<adx:Snippet SnippetName="My Account Household Members Legend" DefaultText="<%$ ResourceManager:Household_Members_DefaultText %>" EditType="text" runat="server"/>
			</legend>
			<adx:EntityList ID="EntityListControl" runat="server" ListCssClass="table table-striped" DefaultEmptyListText="<%$ ResourceManager:No_Items_To_Display %>" ClientIDMode="Static" LanguageCode="<%$ SiteSetting: Language Code, 0 %>" PortalName="<%$ SiteSetting: Language Code %>" />
		</fieldset>
	</asp:Panel>
</asp:Content>