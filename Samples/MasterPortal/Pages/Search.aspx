<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="~/MasterPages/WebForms.master" ValidateRequest="false" CodeBehind="Search.aspx.cs" Inherits="Site.Pages.Search" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="System.Web.Security.AntiXss"%>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>
<%@ Import Namespace="Adxstudio.Xrm.Web" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Liquid" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server"/>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

<% if (FacetedSearchEnabled && FacetedSearchTemplate!=null)
{ %>
	<%
		Html.RenderWebTemplate(FacetedSearchTemplate);
		SearchResults.Visible = false;
	%>

<% } else { %>

	<div class="search-results"  runat="server" id="SearchResults">
		<adx:SearchDataSource ID="SearchData" Query='<%$ SiteSetting: search/query %>' OnSelected="SearchData_OnSelected" runat="server">
			<SelectParameters>
				<asp:QueryStringParameter Name="Query" QueryStringField="q"  />
				<asp:QueryStringParameter Name="LogicalNames" QueryStringField="logicalNames" />
				<asp:QueryStringParameter Name="Filter" QueryStringField="filter" />
				<asp:QueryStringParameter Name="PageNumber" QueryStringField="page" />
				<asp:Parameter Name="PageSize" DefaultValue="10" />
			</SelectParameters>
		</adx:SearchDataSource>
		
		<asp:Repeater DataMember="Info" DataSourceID="SearchData" runat="server">
			<ItemTemplate>
				<asp:Panel Visible='<%# ((int)Eval("Count")) > 0 %>' runat="server">
					<h2><%# string.Format(ResourceManager.GetString("Search_Results_Format_String"), Eval("FirstResultNumber"), Eval("LastResultNumber"), Eval("ApproximateTotalHits")) %><em class="querytext"><%#  HttpUtility.HtmlEncode((string)(Eval("[Query]") ?? string.Empty)) %></em></h2>					
					<asp:ListView DataSourceID="SearchData" ID="SearchResults" runat="server">
						<LayoutTemplate>
							<ul>
								<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
							</ul>
						</LayoutTemplate>
						<ItemTemplate>
							<li runat="server">
								<h3><asp:HyperLink Text='<%#: Eval("Title") %>' NavigateUrl='<%#: Eval("Url") %>' ToolTip='<%#: Eval("Title") %>' runat="server" /></h3>
								<p class="fragment"><%# Eval(SafeHtml.SafeHtmSanitizer.GetSafeHtml("Fragment")) %></p>
								<div>
									<asp:Label Visible='<%# (string)Eval("EntityLogicalName") == "adx_communityforum" || (string)Eval("EntityLogicalName") == "adx_communityforumthread" || (string)Eval("EntityLogicalName") == "adx_communityforumpost" %>' CssClass="label label-default" Text="<%$ ResourceManager:Forums_Label %>" runat="server"/>
									<asp:Label Visible='<%# (string)Eval("EntityLogicalName") == "adx_blog" || (string)Eval("EntityLogicalName") == "adx_blogpost" || (string)Eval("EntityLogicalName") == "adx_blogpostcomment" %>' CssClass="label label-info" Text="<%$ ResourceManager:Blogs_Label %>" runat="server"/>
									<asp:Label Visible='<%# (string)Eval("EntityLogicalName") == "adx_event" || (string)Eval("EntityLogicalName") == "adx_eventschedule" %>' CssClass="label label-info" Text="<%$ ResourceManager:Events_Label %>" runat="server"/>
									<asp:Label Visible='<%# (string)Eval("EntityLogicalName") == "adx_idea" || (string)Eval("EntityLogicalName") == "adx_ideaforum" %>' CssClass="label label-success" Text="<%$ ResourceManager:Ideas_Label %>" runat="server"/>
									<asp:Label Visible='<%# (string)Eval("EntityLogicalName") == "adx_issue" %>' CssClass="label label-danger" Text="<%$ ResourceManager:Issues_Label %>" runat="server"/>
									<asp:Label Visible='<%# (string)Eval("EntityLogicalName") == "incident" %>' CssClass="label label-warning" Text="<%$ ResourceManager:Resolved_Cases_Label %>" runat="server"/>
									<asp:Label Visible='<%# (string)Eval("EntityLogicalName") == "kbarticle" %>' CssClass="label label-info" Text="<%$ ResourceManager:Knowledge_Base_Label %>" runat="server"/>
									<asp:Label Visible='<%# (string)Eval("EntityLogicalName") == "knowledgearticle" %>' CssClass="label label-primary" Text="<%$ ResourceManager:Knowledge_Base_Label %>" runat="server"/>
								</div>
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
				<asp:Panel Visible='<%# ((int)Eval("Count")) == 0 && ((int)Eval("PageNumber")) == 1 %>' runat="server">
					<h2><%# ResourceManager.GetString("Search_No_Results_Found") %><em class="querytext"><%#  HttpUtility.HtmlEncode((string)(Eval("[Query]") ?? string.Empty)) %></em></h2>
				</asp:Panel>
			</ItemTemplate>
		</asp:Repeater>
	</div>
<% } %>
</asp:Content>

<asp:Content ContentPlaceHolderID="Scripts" runat="server">
	<% if (FacetedSearchEnabled && FacetedSearchTemplate!=null)
		{ %>
	<script type="text/javascript">
		$(document).ready(function () {
			if (typeof (FacetedSearch) == typeof (Function)) {
				var facetedSearchObj = new FacetedSearch();
				facetedSearchObj.init();
			}
		});
	</script>
	<% } %>
</asp:Content>
