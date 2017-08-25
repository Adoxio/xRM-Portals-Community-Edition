<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" CodeBehind="Subject.aspx.cs" Inherits="Site.Areas.KnowledgeBase.Pages.Subject" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Import namespace="Adxstudio.Xrm" %>
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Register src="../Controls/Search.ascx" tagname="Search" tagprefix="adx" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/KnowledgeBase/css/knowledgebase.css") %>">
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarTop" runat="server"/>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<adx:Search runat="server" />
	
	<% var childNodes = Html.SiteMapChildNodes().ToArray(); %>
	<% if (childNodes.Any()) { %>
		<div class="kb-subjects">
			<div class="page-header">
				<h3><%: Html.TextSnippet("Knowledge Base Subjects Heading", defaultValue: ResourceManager.GetString("Subjects_Defaulttext"), tagName: "span") %></h3>
			</div>
			<% foreach (var batch in childNodes.Batch(2)) { %>
				<div class="row">
					<% foreach (var child in batch) { %>
						<div class="col-sm-6">
							<h4><a href="<%: child.Url %>" title="<%: child.Title %>"><%: child.Title %></a></h4>
						</div>
					<% } %>
				</div>
			<% } %>
		</div>
	<% } %>

	<asp:Panel ID="SubjectSearch" CssClass="kb-search-results search-results" Visible="False" runat="server">
		<adx:SearchDataSource ID="SearchData" Query="<%$ SiteSetting: knowledgebase/subject/query %>" LogicalNames="kbarticle" OnSelected="SearchData_OnSelected" runat="server">
			<SelectParameters>
				<asp:QueryStringParameter Name="PageNumber" QueryStringField="page" />
				<asp:Parameter Name="PageSize" DefaultValue="10" />
			</SelectParameters>
		</adx:SearchDataSource>
		
		<asp:Repeater DataMember="Info" DataSourceID="SearchData" runat="server">
			<ItemTemplate>
				<asp:Panel Visible='<%# ((int)Eval("Count")) > 0 %>' runat="server">
					<div class="page-header">
						<h3><%# string.Format(ResourceManager.GetString("Search_Results_Format_String"), Eval("FirstResultNumber"), Eval("LastResultNumber"), Eval("ApproximateTotalHits")) %></h3>						
					</div>
					
					<asp:ListView DataSourceID="SearchData" ID="SearchResults" OnDataBound="SearchResults_DataBound" runat="server">
						<LayoutTemplate>
							<ul>
								<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
							</ul>
						</LayoutTemplate>
						<ItemTemplate>
							<li runat="server">
								<h4 class="title">
									<asp:HyperLink Text='<%#: Eval("Title") %>' NavigateUrl='<%#: Eval("Url") %>' ToolTip='<%#: Eval("Title") %>' runat="server" />
								</h4>
							</li>
						</ItemTemplate>
					</asp:ListView>
					
					<adx:UnorderedListDataPager ID="SearchResultPager" CssClass="pagination" PagedControlID="SearchResults" QueryStringField="page" PageSize="10" runat="server">
						<Fields>
							<adx:ListItemNextPreviousPagerField ShowNextPageButton="false" ShowFirstPageButton="True" FirstPageText="&laquo;" PreviousPageText="&lsaquo;" />
							<adx:ListItemNumericPagerField ButtonCount="10" PreviousPageText="&hellip;" NextPageText="&hellip;" />
							<adx:ListItemNextPreviousPagerField ShowPreviousPageButton="false" ShowLastPageButton="True" LastPageText="&raquo;" NextPageText="&rsaquo;" />
						</Fields>
					</adx:UnorderedListDataPager>
				</asp:Panel>
				<asp:Panel Visible='<%# ((int)Eval("Count")) == 0 %>' runat="server">
					<div class="alert alert-block alert-info">
						<%: Html.HtmlSnippet("Knowledge Base Subject No Articles", defaultValue: ResourceManager.GetString("No_Published_Knowledge_Base_Articles_For_This_Subject")) %>
					</div>
				</asp:Panel>
			</ItemTemplate>
		</asp:Repeater>
	</asp:Panel>
	
	<adx:SearchDataSource ID="MostPopularArticlesSearchData" LogicalNames="kbarticle" OnSelected="SearchData_OnSelected" runat="server">
		<SelectParameters>
			<asp:Parameter Name="PageSize" DefaultValue="10" />
		</SelectParameters>
	</adx:SearchDataSource>
		
	<asp:ListView ID="MostPopularArticles" DataSourceID="MostPopularArticlesSearchData" runat="server">
		<LayoutTemplate>
			<div class="kb-popular content-panel panel panel-default">
				<div class="panel-heading">
					<h4>
						<span class="fa fa-star-o" aria-hidden="true"></span>
						<adx:Snippet SnippetName="Knowledge Base Most Popular Articles" DefaultText="<%$ ResourceManager:Most_Popular_Articles_DefaultText %>" EditType="text" runat="server"/>
					</h4>
				</div>
				<div class="list-group">
					<asp:PlaceHolder ID="itemPlaceHolder" runat="server"/>
				</div>
			</div>
		</LayoutTemplate>
		<ItemTemplate>
			<asp:HyperLink CssClass="list-group-item" Text='<%#: Eval("Title") %>' NavigateUrl='<%#: Eval("Url") %>' ToolTip='<%#: Eval("Title") %>' runat="server" />
		</ItemTemplate>
	</asp:ListView>
</asp:Content>