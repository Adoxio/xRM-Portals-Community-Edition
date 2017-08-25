<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" CodeBehind="Sponsors.aspx.cs" Inherits="Site.Areas.Conference.Pages.Sponsors" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<crm:CrmMetadataDataSource ID="SponsorCategories" runat="server" AttributeName="adx_sponsorshipcategory" EntityName="adx_eventsponsor" CrmDataContextName="<%$ SiteSetting: Language Code %>" />
	<asp:Repeater DataSourceID="SponsorCategories" runat="server">
		<ItemTemplate>
			<div class="content-panel panel panel-default">
				<div class="panel-heading">
					<h4>
						<span class="fa fa-trophy" aria-hidden="true"></span>
						<%#: Eval("OptionLabel") %>
					</h4>
				</div>
				<ul class="list-group">
					<asp:Repeater DataSource='<%# GetSponsorsByCategory(Eval("OptionValue") as int?) %>' runat="server">
						<ItemTemplate>
							<li class="list-group-item">
								<crm:CrmEntityDataSource ID="Sponsor" DataItem='<%# Container.DataItem %>' runat="server" />
								<h4 class="list-group-item-heading">
									<asp:HyperLink NavigateUrl='<%# ((Entity)Container.DataItem).GetAttributeValue<string>("adx_url") %>' runat="server">
										<adx:Property DataSourceID="Sponsor" PropertyName="adx_name" EditType="text" runat="server" />
									</asp:HyperLink>
								</h4>
								<div class="list-group-item-text">
									<adx:Property DataSourceID="Sponsor" PropertyName="adx_description" EditType="html" runat="server" />
								</div>
							</li>
						</ItemTemplate>
					</asp:Repeater>
				</ul>
			</div>
		</ItemTemplate>
	</asp:Repeater>
</asp:Content>
