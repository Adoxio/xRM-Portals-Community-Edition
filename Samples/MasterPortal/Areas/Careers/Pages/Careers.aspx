<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" Inherits="Site.Areas.Careers.Pages.Careers" Codebehind="Careers.aspx.cs" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="alternate" type="application/atom+xml" href="<%: Url.RouteUrl("JobPostingsFeed") %>" title="<%: Html.SnippetLiteral("Careers Subscribe Heading", ResourceManager.GetString("Subscribe_To_Job_Postings")) %>" />
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<div class="pull-right">
			<a class="feed-icon fa fa-rss-square" href="<%: Url.RouteUrl("JobPostingsFeed") %>" title="<%: Html.SnippetLiteral("Careers Subscribe Heading", ResourceManager.GetString("Subscribe_To_Job_Postings")) %>"></a>
		</div>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<asp:ListView ID="JobPostings" runat="server">
		<LayoutTemplate>
			<div class="panel-group" id="job-postings">
				<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
			</div>
		</LayoutTemplate>
		<ItemTemplate>
			<div class="panel panel-default job-posting">
				<div class="panel-heading clearfix">
						<asp:Panel CssClass="closing pull-right" Visible='<%# Eval("ClosingOn") != null %>' runat="server">
							<span class="label label-info">
								<adx:Snippet SnippetName="Job Posting Closing Date Text" DefaultText="<%$ ResourceManager:Closing_DefaultText %>" runat="server" EditType="text" />
								<abbr class="timestamp" data-format="MMMM dd, yyyy"><%#: Eval("ClosingOn", "{0:r}") %></abbr>
							</span>
						</asp:Panel>
					<h4 class="panel-title">
						<a data-toggle="collapse" data-parent="#job-postings" href="#<%#: Eval("Id") %>" title="<%#: Eval("Name") %>">
							<%#: Eval("Name") %>
					 </a>
					</h4>
				</div>
				<div class="panel-collapse collapse" id="<%#: Eval("Id") %>">
					<div class="panel-body">
						<div class="description">
							<%# Eval("Description") %>
						</div>
						<div>
							<crm:CrmHyperLink ID="JobApplication" runat="server" CssClass="btn btn-primary" Text="<%$ Snippet: links/apply, Apply Now %>" SiteMarkerName="Job Application" QueryString='<%# Eval("Id", "jobid={0}") %>'/>
						</div>
					</div>
				</div>
			</div>
		</ItemTemplate>
	</asp:ListView>
</asp:Content>
