<%@ Page Title="" Language="C#" MasterPageFile="Account.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ConfirmEmail/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div class="form-horizontal">
		<fieldset>
			<legend><%: Html.TextSnippet("Account/ConfirmEmail/ConfirmEmailFormHeading", defaultValue: ResourceManager.GetString("Confirm_Email_Defaulttext"), tagName: "span") %></legend>
			<% Html.RenderPartial("ProfileMessage", Request["Message"] ?? string.Empty); %>
		</fieldset>
	</div>
</asp:Content>
