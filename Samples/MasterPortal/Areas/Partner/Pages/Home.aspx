<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="Site.Areas.Partner.Pages.Home" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Partner/css/partner.css") %>">
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentHeader" runat="server"/>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<%: Html.HtmlAttribute("adx_copy", cssClass: "page-copy") %>
		
	<asp:Panel ID="PartnerHomePanel" runat="server">
		<div class="row">
			<div class="col-sm-8">
				<div class="content-panel panel panel-default">
					<div class="panel-heading">
						<h4>
							<span class="fa fa-exclamation-circle" aria-hidden="true"></span>
							<asp:Literal Text="<%$ Snippet: home/alerts/label, Alerts %>" runat="server" />
						</h4>
					</div>
					<div class="panel-body">
						<adx:Snippet runat="server" SnippetName="home/alerts/legend" DefaultText="home/alerts/legend" CssClass="content-caption" Editable="true" EditType="html"/>
						<asp:GridView ID="Alerts" runat="server" CssClass="table table-striped" GridLines="None" AlternatingRowStyle-CssClass="alternate-row" OnRowDataBound="Alerts_OnRowDataBound">
							<EmptyDataRowStyle CssClass="empty" />
							<EmptyDataTemplate>
								<adx:Snippet runat="server" SnippetName="home/alerts/empty" DefaultText="<%$ ResourceManager:No_Alerts_Currently %>" Editable="true" EditType="html" />
							</EmptyDataTemplate>
						</asp:GridView>
					</div>
				</div>
			</div>
			<div class="col-sm-4">
				<div class="content-panel panel panel-default">
					<div class="panel-heading">
						<div class="pull-right">
							<crm:CrmHyperLink runat="server" SiteMarkerName="New Opportunities" CssClass="btn btn-default btn-xs"><span class="fa fa-external-link" aria-hidden="true"></span></crm:CrmHyperLink>
						</div>
						<h4>
							<crm:CrmHyperLink runat="server" SiteMarkerName="New Opportunities" >
								<span class="fa fa-asterisk" aria-hidden="true"></span>
								<adx:Snippet runat="server" SnippetName="home/activities/label/new-opportunities" DefaultText="<%$ ResourceManager:New_Opportunities_DefaultText %>" Editable="true" EditType="text" />
							</crm:CrmHyperLink>
						</h4>
					</div>
					<ul class="list-group">
						<li class="list-group-item">
							<asp:Label ID="NewOpportunityCount" CssClass="badge" runat="server"/>
							<adx:Snippet runat="server" SnippetName="home/activities/label/new-opportunities-count" DefaultText="<%$ ResourceManager:Opportunities_Awaiting_Acceptance %>" Editable="true" EditType="text" />
						</li>
						<li class="list-group-item">
							<asp:Label ID="NewOpportunityValue" CssClass="badge" runat="server"/>
							<adx:Snippet runat="server" SnippetName="home/activities/label/new-opportunities-value" DefaultText="<%$ ResourceManager:Value_Of_New_Opportunities %>" Editable="true" EditType="text" />
						</li>
					</ul>
				</div>
				<div class="content-panel panel panel-default">
					<div class="panel-heading">
						<div class="pull-right">
							<crm:CrmHyperLink runat="server" SiteMarkerName="Accepted Opportunities" CssClass="btn btn-default btn-xs"><span class="fa fa-external-link" aria-hidden="true"></span></crm:CrmHyperLink>
						</div>
						<h4>
							<crm:CrmHyperLink runat="server" SiteMarkerName="Accepted Opportunities" >
								<span class="fa fa-money" aria-hidden="true"></span>
								<adx:Snippet runat="server" SnippetName="home/activities/label/accepted-opportunities" DefaultText="<%$ ResourceManager:Active_Opportunities %>" Editable="true" EditType="text" />
							</crm:CrmHyperLink>
						</h4>
					</div>
					<ul class="list-group">
						<li class="list-group-item">
							<asp:Label ID="AcceptedOpportunityCount" CssClass="badge" runat="server"/>
							<adx:Snippet runat="server" SnippetName="home/activities/label/accepted-opportunities-count" DefaultText="<%$ ResourceManager:Current_Opportunities_DefaultText %>" Editable="true" EditType="text" />
						</li>
						<li class="list-group-item">
							<asp:Label ID="AcceptedOpportunityValue" CssClass="badge" runat="server"/>
							<adx:Snippet runat="server" SnippetName="home/activities/label/accepted-opportunities-value" DefaultText="<%$ ResourceManager:Value_Of_Active_Opportunities %>" Editable="true" EditType="text" />
						</li>
					</ul>
				</div>
			</div>
		</div>
	</asp:Panel>
</asp:Content>