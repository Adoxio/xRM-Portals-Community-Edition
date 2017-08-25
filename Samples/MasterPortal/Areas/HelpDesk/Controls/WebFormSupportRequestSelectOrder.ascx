<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WebFormSupportRequestSelectOrder.ascx.cs" Inherits="Site.Areas.HelpDesk.Controls.WebFormSupportRequestSelectOrder" %>

<adx:Snippet runat="server" SnippetName="Help Desk Select Order Message" DefaultText="<%$ ResourceManager:Select_Support_Package_You_Like_To_Purchase %>" Editable="true" EditType="html"/>
<asp:RadioButtonList ID="PlanPackageList" runat="server" CssClass="radio-list" RepeatLayout="Flow">
</asp:RadioButtonList>
<asp:RequiredFieldValidator runat="server"
	ID="PlanPackageListRequiredFieldValidator"
	ControlToValidate="PlanPackageList"
	Display="Static"
	ErrorMessage="<%$ Snippet: Help Desk Select Order Required Text, Select_Support_Package_Error_Message %>"
	CssClass = "help-block error">
</asp:RequiredFieldValidator>