<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="Directory.aspx.cs" Inherits="Site.Pages.Directory" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<%: Html.HtmlAttribute("adx_copy", cssClass: "page-copy") %>
	<div class="directory row">
		<div class="col-md-4">
			<asp:SiteMapDataSource ID="ChildDataSource" StartFromCurrentNode="true" ShowStartingNode="False" StartingNodeOffset="0" runat="server"/>
			<asp:ListView DataSourceID="ChildDataSource" runat="server">
				<LayoutTemplate>
					<div class="panel panel-default">
						<div class="subnav list-group">
							<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
						</div>
					</div>
				</LayoutTemplate>
				<ItemTemplate>
					<a class="list-group-item" href="<%#: Eval("Entity.Id", "#{0}") %>" title="<%#: Eval("Title") %>"><span class="fa fa-chevron-right" aria-hidden="true"></span> <%#: Eval("Title") %></a>
				</ItemTemplate>
			</asp:ListView>
		</div>
		<div class="col-md-8">
			<asp:ListView DataSourceID="ChildDataSource" runat="server">
				<LayoutTemplate>
					<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
				</LayoutTemplate>
				<ItemTemplate>
					<section id="<%#: Eval("Entity.Id", "{0}") %>">
						<div class="page-header">
							<h2>
								<asp:HyperLink NavigateUrl='<%#: Eval("Url") %>' Text='<%#: Eval("Title") %>' ToolTip='<%#: Eval("Title") %>' runat="server"/>
							</h2>
						</div>
						<asp:Panel Visible='<%# IsWebPage(Eval("Entity")) %>' runat="server">
							<crm:CrmEntityDataSource ID="SectionEntity" DataItem='<%# Eval("Entity") %>' runat="server"/>
							<adx:Property DataSourceID="SectionEntity" PropertyName='<%# IsWebPage(Eval("Entity")) ? "adx_summary" : null %>' EditType="html" runat="server" />
						</asp:Panel>
						<asp:SiteMapDataSource ID="SubChildDataSource" StartingNodeUrl='<%# Eval("Url") %>' ShowStartingNode="False" StartingNodeOffset="0" runat="server"/>
						<asp:ListView DataSourceID="SubChildDataSource" runat="server">
							<LayoutTemplate>
								<div class="panel panel-default">
									<div class="list-group">
										<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
									</div>
								</div>
							</LayoutTemplate>
							<ItemTemplate>
								<asp:HyperLink CssClass="list-group-item" NavigateUrl='<%#: Eval("Url") %>' Text='<%#: Eval("Title") %>' ToolTip='<%#: Eval("Title") %>' runat="server"/>
							</ItemTemplate>
						</asp:ListView>
					</section>
				</ItemTemplate>
			</asp:ListView>
		</div>
	</div>
</asp:Content>
