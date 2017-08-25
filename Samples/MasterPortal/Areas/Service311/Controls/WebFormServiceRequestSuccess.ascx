<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WebFormServiceRequestSuccess.ascx.cs" Inherits="Site.Areas.Service311.Controls.WebFormServiceRequestSuccess" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<asp:Panel ID="PanelSuccess" runat="server" Visible="False" CssClass="row">
	<div class="col-sm-8">
		<div class="alert alert-success">
			<adx:Snippet ID="DefaultSuccessMessageSnippet" runat="server" Visible="False" SnippetName="311 Service Request Default Success Message" EditType="html" Editable="true" DefaultText="<%$ ResourceManager:Service_Request_Default_Success_Message %>" ClientIDMode="Static" />
			<asp:Label ID="CustomSuccessMessage" runat="server" />
		</div>
		<asp:Panel ID="ServiceRequestNumberPanel" runat="server" Visible="False">
			<p>
				<adx:Snippet ID="ServiceRequestNumberLabel" 
					runat="server" 
					SnippetName="311 Service Request Number Label Text" 
					EditType="text" Editable="true" 
					DefaultText="<%$ ResourceManager:Service_Request_Number_DefaultText %>" 
					ClientIDMode="Static" />
				&nbsp;

				<asp:LinkButton ID="LinkToServiceRequest" OnClick="LinkToServiceRequest_Click" ClientIDMode="Static" runat="server"></asp:LinkButton>
				
				&nbsp;
			</p>
		<asp:Panel ID="SLAPanel" CssClass="panel panel-success" runat="server">
			<div class="panel-heading">
				<h3 class="panel-title">Service-Level Agreement Details</h3>
			</div>
			<div class="panel-body">
				<asp:Label ID="SlaResponseTime" Visible="True" runat="server"></asp:Label>
					<br/>
					<asp:Label ID="SlaResolutionTime" Visible="True" runat="server"></asp:Label>
			</div>
		</asp:Panel>

		</asp:Panel>
	</div>
</asp:Panel>