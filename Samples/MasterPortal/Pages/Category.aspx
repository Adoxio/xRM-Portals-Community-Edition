<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="Category.aspx.cs" Inherits="Site.Pages.Category" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<div class="pull-right hidden-xs">
			<asp:SiteMapDataSource ID="TopicData" StartFromCurrentNode="True" ShowStartingNode="False" StartingNodeOffset="0" runat="server"/>
			<asp:ListView DataSourceID="TopicData" runat="server">
				<LayoutTemplate>
					<ul class="nav nav-pills">
						<li class="active">
							<asp:HyperLink NavigateUrl='<%$ CrmSiteMap: Current, Return=Url %>' Text='<%$ CrmSiteMap: Current, Eval=Title %>' ToolTip='<%$ CrmSiteMap: Current, Eval=Title %>' runat="server"/>
						</li>
						<li id="itemPlaceholder" runat="server"/>
					</ul>
				</LayoutTemplate>
				<ItemTemplate>
					<li>
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
					<li class="active">
						<asp:HyperLink NavigateUrl='<%$ CrmSiteMap: Current, Return=Url %>' Text='<%$ CrmSiteMap: Current, Eval=Title %>' ToolTip='<%$ CrmSiteMap: Current, Eval=Title %>' runat="server"/>
					</li>
					<li id="itemPlaceholder" runat="server"/>
				</ul>
			</LayoutTemplate>
			<ItemTemplate>
				<li>
					<asp:HyperLink NavigateUrl='<%#: Eval("Url") %>' Text='<%#: Eval("Title") %>' ToolTip='<%#: Eval("Title") %>' runat="server"/>
				</li>
			</ItemTemplate>
		</asp:ListView>
	</div>
</asp:Content>
