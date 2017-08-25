<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" CodeBehind="ServiceRequestStatus.aspx.cs" Inherits="Site.Areas.Service311.Pages.ServiceRequestStatus" %>

<asp:Content runat="server" ContentPlaceHolderID="Head">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Service311/css/311.css") %>" />
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<div id="service-request-status">
		<asp:Panel runat="server" ID="ErrorPanel" CssClass="alert alert-block alert-danger">
			<adx:Snippet ID="ErrorMessage" runat="server" SnippetName="311 Service Request Status Error Message" EditType="html" Editable="true" DefaultText="<%$ ResourceManager:Service_Request_Not_Found_Error %>" ClientIDMode="Static" />
		</asp:Panel>
	</div>
</asp:Content>