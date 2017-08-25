<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="CategoryTopic.aspx.cs" Inherits="Site.Pages.CategoryTopic" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<asp:SiteMapDataSource ID="CategoryData" StartFromCurrentNode="True" ShowStartingNode="True" StartingNodeOffset="-1" runat="server"/>
	<asp:SiteMapDataSource ID="TopicData" StartFromCurrentNode="True" ShowStartingNode="False" StartingNodeOffset="-1" runat="server"/>
	<div class="page-header">
		<div class="hidden-xs pull-right">
			<asp:ListView DataSourceID="TopicData" runat="server">
				<LayoutTemplate>
					<ul class="nav nav-pills">
						<asp:ListView DataSourceID="CategoryData" runat="server">
							<LayoutTemplate>
								<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
							</LayoutTemplate>
							<ItemTemplate>
								<li>
									<asp:HyperLink NavigateUrl='<%#: Eval("Url") %>' Text='<%#: Eval("Title") %>' ToolTip='<%#: Eval("Title") %>' runat="server" />
								</li>
							</ItemTemplate>
						</asp:ListView>
						<li id="itemPlaceholder" runat="server"/>
					</ul>
				</LayoutTemplate>
				<ItemTemplate>
					<li class="<%# IsCurrentNode(Container.DataItem) ? "active" : string.Empty %>">
						<asp:HyperLink NavigateUrl='<%#: Eval("Url") %>' Text='<%#: Eval("Title") %>' ToolTip='<%#: Eval("Title") %>' runat="server"/>
					</li>
				</ItemTemplate>
			</asp:ListView>
		</div>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
		<asp:ListView DataSourceID="TopicData" runat="server">
			<LayoutTemplate>
				<ul class="nav nav-pills nav-stacked visible-xs">
					<asp:ListView DataSourceID="CategoryData" runat="server">
						<LayoutTemplate>
							<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
						</LayoutTemplate>
						<ItemTemplate>
							<li>
								<asp:HyperLink NavigateUrl='<%#: Eval("Url") %>' Text='<%#: Eval("Title") %>' ToolTip='<%#: Eval("Title") %>' runat="server" />
							</li>
						</ItemTemplate>
					</asp:ListView>
					<li id="itemPlaceholder" runat="server"/>
				</ul>
			</LayoutTemplate>
			<ItemTemplate>
				<li class="<%# IsCurrentNode(Container.DataItem) ? "active" : string.Empty %>">
					<asp:HyperLink NavigateUrl='<%#: Eval("Url") %>' Text='<%#: Eval("Title") %>' ToolTip='<%#: Eval("Title") %>' runat="server"/>
				</li>
			</ItemTemplate>
		</asp:ListView>
	</div>
</asp:Content>
