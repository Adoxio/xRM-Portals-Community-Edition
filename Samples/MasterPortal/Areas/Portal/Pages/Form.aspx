<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Default.master" AutoEventWireup="true" CodeBehind="Form.aspx.cs" Inherits="Site.Areas.Portal.Pages.Form" %>
<asp:Content ContentPlaceHolderID="Header" runat="server" />
<asp:Content ContentPlaceHolderID="HeaderNavbar" runat="server" />
<asp:Content ContentPlaceHolderID="MainContent" runat="server" ViewStateMode="Enabled">
	<form id="content_form" runat="server">
		<asp:ScriptManager runat="server">
			<Scripts>
				<asp:ScriptReference Path="~/js/jquery.blockUI.js" />
			</Scripts>
		</asp:ScriptManager>
		<script type="text/javascript">
			function entityFormClientValidate() {
				// Custom client side validation. Method is called by the submit button's onclick event.
				// Must return true or false. Returning false will prevent the form from submitting.
				return true;
			}
			
		</script>
		<adx:EntityForm ID="EntityFormControl" runat="server" FormCssClass="crmEntityFormView" PreviousButtonCssClass="btn btn-default" NextButtonCssClass="btn btn-primary" SubmitButtonCssClass="btn btn-primary" OnItemSaved="OnItemSaved" ClientIDMode="Static" LanguageCode="<%$ SiteSetting: Language Code, 0 %>" PortalName="<%$ SiteSetting: Language Code %>" />
	</form>
</asp:Content>
<asp:Content ID="ChatWidgetContent" ContentPlaceHolderID="ChatWidgetContainer" Runat="Server"/>
<asp:Content ContentPlaceHolderID="Footer" runat="server" />