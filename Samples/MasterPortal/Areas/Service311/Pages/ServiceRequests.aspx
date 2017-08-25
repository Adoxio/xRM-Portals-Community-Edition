<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" CodeBehind="ServiceRequests.aspx.cs" Inherits="Site.Areas.Service311.Pages.ServiceRequests" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>
<%@ Import Namespace="Site.Areas.Service311" %>

<asp:Content runat="server" ContentPlaceHolderID="Head">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Service311/css/311.css") %>" />
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<div id="service-requests" class="content-panel panel panel-default">
		<div class="panel-heading">
			<h4>
				<span class="fa fa-edit" aria-hidden="true"></span>
				<adx:Snippet runat="server" SnippetName="311 Service Requests List Title Text" EditType="text" Editable="true" DefaultText="<%$ ResourceManager:Submit_New_Service_Request_DefaultText %>" ClientIDMode="Static" />
			</h4>
		</div>
		<asp:ListView runat="server" ID="ServiceRequestTypesListView">
			<LayoutTemplate>
				<ul class="list-group">
					<asp:PlaceHolder runat="server" ID="ItemPlaceHolder"></asp:PlaceHolder>
				</ul>
			</LayoutTemplate>
			<ItemTemplate>
				<li class="list-group-item">
					<div class="media">
						<div class="pull-left">
							<%# ServiceRequestHelpers.BuildServiceRequestTypeThumbnailImageTag(ServiceContext, Container.DataItem as Entity, "media-object") %>
						</div>
						<div class="media-body">
							<h4>
								<crm:CrmHyperLink SiteMarkerName="New Service Request" QueryString='<%# string.Format("typeid={0}", ((Entity)Container.DataItem).GetAttributeValue<Guid>("adx_servicerequesttypeid")) %>' Text='<%# ((Entity)Container.DataItem).GetAttributeValue<string>("adx_name") %>' runat="server"></crm:CrmHyperLink>
							</h4>
							<p><%# ((Entity)Container.DataItem).GetAttributeValue<string>("adx_description") %></p>
						</div>
					</div>
				</li>
			</ItemTemplate>
		</asp:ListView>
	</div>
	<adx:Snippet runat="server" SnippetName="311 Service Requests Types Not Listed Message" EditType="html" Editable="true" DefaultText="" />
</asp:Content>