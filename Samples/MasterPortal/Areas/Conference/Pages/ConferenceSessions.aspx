<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" CodeBehind="ConferenceSessions.aspx.cs" Inherits="Site.Areas.Conference.Pages.ConferenceSessions" %>
<%@ Register TagPrefix="site" TagName="ChildNavigation" Src="~/Controls/ChildNavigation.ascx" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Conference/css/events.css") %>">
	<link rel="profile" href="http://microformats.org/profile/hcalendar">
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<div class="events">
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
							<abbr class="dtstart" title="<%#: Eval("Start", "{0:yyyy-MM-ddTHH:mm:ssZ}") %>">
								<crm:DateTimeLiteral Value='<%# Eval("Start") %>' Format="D" runat="server" OutputTimeZoneLabel="false" /> 
								<crm:DateTimeLiteral Value='<%# Eval("Start") %>' Format="hh:mm tt" OutputTimeZoneLabel="false" runat="server" />
							</abbr>
							&ndash;
							<abbr class="dtend" title="<%#: Eval("End", "{0:yyyy-MM-ddTHH:mm:ssZ}") %>">
								<crm:DateTimeLiteral  Value='<%# Eval("End") %>' Format="hh:mm tt" runat="server" />
							</abbr>
						</p>
						<adx:Property DataSourceID="Event" PropertyName="adx_summary" EditType="html" runat="server"/>
					</div>
				</li>
			</ItemTemplate>
		</asp:ListView>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarTop" runat="server">
	<site:ChildNavigation Exclude="adx_event" runat="server"/>
</asp:Content>
