<%@ Page Language="C#" MasterPageFile="~/MasterPages/Default.master" AutoEventWireup="true" CodeBehind="QuickForm.aspx.cs" Inherits="Site.Areas.Portal.Pages.QuickForm" %>
<asp:Content ContentPlaceHolderID="Header" runat="server" />
<asp:Content ContentPlaceHolderID="HeaderNavbar" runat="server" />
<asp:Content ContentPlaceHolderID="ContentContainer" runat="server" ViewStateMode="Enabled">
	<div id="content-container" class="container quickform">
		<div id="content">
			<form id="content_form" runat="server">
				<asp:ScriptManager runat="server">
				</asp:ScriptManager>
				<asp:Panel ID="FormPanel" runat="server"></asp:Panel>
			</form>
		</div>
	</div>
</asp:Content>
<asp:Content ID="ChatWidgetContent" ContentPlaceHolderID="ChatWidgetContainer" Runat="Server"/>
<asp:Content ContentPlaceHolderID="Footer" runat="server" />