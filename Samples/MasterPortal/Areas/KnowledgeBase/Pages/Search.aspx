<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="Site.Areas.KnowledgeBase.Pages.Search" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="System.Web.Security.AntiXss" %>
<%@ Register src="../Controls/Search.ascx" tagname="Search" tagprefix="adx" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>
<%@ Import Namespace="Adxstudio.Xrm.Web" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/KnowledgeBase/css/knowledgebase.css") %>">
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarTop" runat="server"/>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<adx:Search runat="server" />
	
	<div class="kb-search-results search-results">
		<adx:SearchDataSource ID="SearchData" LogicalNames="kbarticle" OnSelected="SearchData_OnSelected" runat="server">
			<SelectParameters>
				<asp:QueryStringParameter Name="Query" QueryStringField="kbquery" />
				<asp:QueryStringParameter Name="PageNumber" QueryStringField="page" />
				<asp:Parameter Name="PageSize" DefaultValue="10" />
			</SelectParameters>
		</adx:SearchDataSource>
		
		<asp:Repeater DataMember="Info" DataSourceID="SearchData" runat="server">
			<ItemTemplate>
				<asp:Panel Visible='<%# ((int)Eval("Count")) > 0 %>' runat="server">
					<div class="page-header">
						<h3><%# string.Format(ResourceManager.GetString("Search_Results_Format_String"), Eval("FirstResultNumber"), Eval("LastResultNumber"), Eval("ApproximateTotalHits")) %><em class="querytext"><%# HttpUtility.HtmlEncode((string)(Eval("[Query]") ?? string.Empty)) %></em></h3>
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
								<p class="fragment"><%# Eval(SafeHtml.SafeHtmSanitizer.GetSafeHtml("Fragment")) %></p>
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
					<h3><%# ResourceManager.GetString("Search_No_Results_Found") %> <em class="querytext"><%# HttpUtility.HtmlEncode((string)(Eval("[Query]") ?? string.Empty)) %></em></h3>
				</asp:Panel>
			</ItemTemplate>
		</asp:Repeater>
	</div>
</asp:Content>
