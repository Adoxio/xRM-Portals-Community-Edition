<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EventsPanel.ascx.cs" Inherits="Site.Controls.EventsPanel" %>

<asp:ListView ID="UpcomingEvents" runat="server">
	<LayoutTemplate>
		<div class="content-panel panel panel-default">
			<div class="panel-heading">
				<asp:HyperLink CssClass="pull-right" NavigateUrl='<%$ CrmSiteMap: SiteMarker=Events, Return=Url %>' Text='<%$ Snippet: Home All Events Link Text, All Events %>' ToolTip='<%$ Snippet: Home All Events Link Text, All Events %>' runat="server" />
				<h4>
					<span class="fa fa-calendar" aria-hidden="true"></span>
					<adx:Snippet SnippetName="Home Upcoming Events Heading" DefaultText="<%$ ResourceManager:Events_DefaultText %>" EditType="text" runat="server" />
				</h4>
			</div>
			<ul class="list-group">
				<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
			</ul>
		</div>
	</LayoutTemplate>
	<ItemTemplate>
		<li class="vevent list-group-item">
			<crm:CrmEntityDataSource ID="Event" DataItem='<%# Eval("Event") %>' runat="server" />
			<h4 class="list-group-item-heading">
				<a class="url summary" href="<%#: Eval("Url") %>"><adx:Property DataSourceID="Event" PropertyName="adx_name" Literal="True" runat="server"/></a>
			</h4>
			<div class="content-metadata">
				<abbr class="dtstart" title="<%#: Eval("Start", "{0:yyyy-MM-ddTHH:mm:ssZ}") %>"><%#: Eval("Start", "{0:r}") %></abbr>&ndash;<abbr class="dtend" title="<%#: Eval("End", "{0:yyyy-MM-ddTHH:mm:ssZ}") %>"><%#: Eval("End", "{0:r}") %></abbr>
			</div>
			<div class="list-group-item-text">
				<adx:Property DataSourceID="Event" PropertyName="adx_summary" EditType="html" runat="server"/>
			</div>
		</li>
	</ItemTemplate>
</asp:ListView>