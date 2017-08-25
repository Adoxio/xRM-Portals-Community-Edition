<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" CodeBehind="ConferenceSpeakers.aspx.cs" Inherits="Site.Areas.Conference.Pages.ConferenceSpeakers" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Notes" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Conference/css/events.css") %>">
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<asp:ListView ID="Speakers" runat="server" OnItemDataBound="Speakers_OnItemDataBound">
		<LayoutTemplate>
			<div class="content-panel panel panel-default">
				<div class="panel-heading">
					<h4>
						<span class="fa fa-microphone" aria-hidden="true"></span>
						<adx:Snippet SnippetName="Speakers Heading" DefaultText="<%$ ResourceManager:Speakers_DefaultText %>" EditType="text" runat="server" />
					</h4>
				</div>
				<ul class="list-group">
					<li id="ItemPlaceholder" runat="server" />
				</ul>
			</div>
		</LayoutTemplate>
		<ItemTemplate>
			<li class="list-group-item clearfix">
				<div class="pull-right">
					<asp:Repeater ID="SpeakerAnnotations" runat="server">
						<ItemTemplate>
							<asp:Image CssClass="thumbnail" ImageUrl='<%# ((IAnnotation)Container.DataItem).FileAttachment.Url %>' runat="server" />
						</ItemTemplate>
					</asp:Repeater>
				</div>
				<crm:CrmEntityDataSource ID="Speaker" DataItem="<%# Container.DataItem %>" runat="server" />
				<h4 class="list-group-item-heading">
					<asp:Panel Visible='<%# ((Entity)Container.DataItem).GetAttributeValue<string>("adx_url") != null %>' runat="server">
						<asp:HyperLink NavigateUrl='<%# ((Entity)Container.DataItem).GetAttributeValue<string>("adx_url") %>' runat="server">
							<adx:Property DataSourceID="Speaker" PropertyName="adx_name" EditType="text" runat="server" />
						</asp:HyperLink>
					</asp:Panel>
					<asp:Panel Visible='<%# ((Entity)Container.DataItem).GetAttributeValue<string>("adx_url") == null %>' runat="server">
						<adx:Property DataSourceID="Speaker" PropertyName="adx_name" EditType="text" runat="server" />
					</asp:Panel>
				</h4>
				<div class="list-group-item-text">
					<adx:Property DataSourceID="Speaker" PropertyName="adx_description" EditType="html" runat="server" />
				</div>
			</li>
		</ItemTemplate>
	</asp:ListView>
</asp:Content>