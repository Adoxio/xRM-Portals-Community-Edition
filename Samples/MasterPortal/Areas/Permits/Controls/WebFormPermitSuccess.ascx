<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WebFormPermitSuccess.ascx.cs" Inherits="Site.Areas.Permits.Controls.WebFormPermitSuccess" %>

<asp:Panel ID="PanelSuccess" runat="server" Visible="False">
	<div class="alert alert-block alert-success">
		<adx:Snippet ID="DefaultSuccessMessageSnippet" runat="server" Visible="False" SnippetName="Permit Default Success Message" EditType="html" Editable="true" DefaultText="<%$ ResourceManager:Permit_Default_Success_Message %>" ClientIDMode="Static" />
		<asp:Label ID="CustomSuccessMessage" runat="server" />
	</div>
	<asp:Panel ID="PermitNumberPanel" runat="server" Visible="False" CssClass="alert alert-block alert-info">
		<p>
			<adx:Snippet ID="PermitNumberLabel" runat="server" SnippetName="Permit Number Label Text" EditType="text" Editable="true" DefaultText="<%$ ResourceManager:Permit_Number_DefaultText %>" ClientIDMode="Static" />&nbsp;<strong><asp:Label ID="PermitNumber" runat="server" ClientIDMode="Static"/></strong>
		</p>
	</asp:Panel>
</asp:Panel>