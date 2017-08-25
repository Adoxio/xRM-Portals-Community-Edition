<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" CodeBehind="ChildEvents.aspx.cs" Inherits="Site.Areas.Events.Pages.ChildEvents" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Register TagPrefix="site" TagName="ChildNavigation" Src="~/Controls/ChildNavigation.ascx" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Events/css/events.css") %>">
	<link rel="profile" href="http://microformats.org/profile/hcalendar">
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<div class="events">
		<ul class="toolbar-nav nav nav-tabs">
			<li class="active">
				<a href="#upcoming" data-toggle="tab" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("UpComing_DefaultText") %>'>
					<adx:Snippet SnippetName="Event Upcoming Tab Text" DefaultText="<%$ ResourceManager:UpComing_DefaultText %>" Literal="True" runat="server"/>
				</a>
			</li>
			<li>
				<a href="#past" data-toggle="tab" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Past_DefaultText") %>'>
					<adx:Snippet SnippetName="Event Past Tab Text" DefaultText="<%$ ResourceManager:Past_DefaultText %>" Literal="True" runat="server"/>
				</a>
			</li>
		</ul>
		<div class="tab-content">
			<div class="tab-pane active" id="upcoming">
				<asp:ListView ID="UpcomingEvents" runat="server">
					<LayoutTemplate>
						<ul class="list-unstyled">
							<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
						</ul>
					</LayoutTemplate>
					<ItemTemplate>
						<li>
							<crm:CrmEntityDataSource ID="Event" DataItem='<%# Eval("Event") %>' runat="server" />
							<span class="icon fa fa-calendar" aria-hidden="true"></span>
							<div class="vevent">
								<h3>
									<a class="url summary" href="<%#: Eval("Url") %>"><adx:Property DataSourceID="Event" PropertyName="adx_name" Literal="True" runat="server"/></a>
								</h3>
								<p>
									<abbr class="dtstart" title="<%#: Eval("Start", "{0:yyyy-MM-ddTHH:mm:ssZ}") %>"><%#: Eval("Start", "{0:r}") %></abbr>
									&ndash;
									<abbr class="dtend" title="<%#: Eval("End", "{0:yyyy-MM-ddTHH:mm:ssZ}") %>"><%#: Eval("End", "{0:r}") %></abbr>
								</p>
								<adx:Property DataSourceID="Event" PropertyName="adx_summary" EditType="html" runat="server"/>
							</div>
						</li>
					</ItemTemplate>
				</asp:ListView>
			</div>
			<div class="tab-pane" id="past">
				<asp:ListView ID="PastEvents" runat="server">
					<LayoutTemplate>
						<ul class="list-unstyled">
							<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
						</ul>
					</LayoutTemplate>
					<ItemTemplate>
						<li>
							<crm:CrmEntityDataSource ID="Event" DataItem='<%# Eval("Event") %>' runat="server" />
							<span class="icon fa fa-calendar" aria-hidden="true"></span>
							<div class="vevent">
								<h3>
									<a class="url summary" href="<%#: Eval("Url") %>"><adx:Property DataSourceID="Event" PropertyName="adx_name" Literal="True" runat="server"/></a>
								</h3>
								<p>
									<abbr class="dtstart" title="<%#: Eval("Start", "{0:yyyy-MM-ddTHH:mm:ssZ}") %>"><%#: Eval("Start", "{0:r}") %></abbr>&ndash;<abbr class="dtend" title="<%#: Eval("End", "{0:yyyy-MM-ddTHH:mm:ssZ}") %>"><%#: Eval("End", "{0:r}") %></abbr>
								</p>
								<adx:Property DataSourceID="Event" PropertyName="adx_summary" EditType="html" runat="server"/>
							</div>
						</li>
					</ItemTemplate>
				</asp:ListView>
			</div>
		</div>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarTop" runat="server">
	<site:ChildNavigation Exclude="adx_event" runat="server"/>
</asp:Content>