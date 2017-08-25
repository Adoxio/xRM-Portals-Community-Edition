<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SessionScheduleDetails.ascx.cs" Inherits="Site.Areas.Events.Controls.SessionScheduleDetails" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<asp:ListView ID="ScheduleListView" runat="server" OnItemCommand="ScheduleListView_ItemCommand" OnItemDataBound="ScheduleListView_ItemDataBound">
	<LayoutTemplate>
		<div class="content-panel panel panel-default">
			<ul class="list-group">
				<asp:PlaceHolder ID="ItemPlaceholder" runat="server" />
			</ul>
		</div>
	</LayoutTemplate>
	<ItemTemplate>
		<li class="list-group-item clearfix">
			<asp:Panel CssClass="pull-right" Visible="<%# Request.IsAuthenticated && Attendee != null %>" runat="server">
				<asp:Button ID="FeedbackButton" runat="server" CssClass="btn btn-default btn-sm" Text="Feedback" CommandName="Feedback" CommandArgument='<%# ((Entity)Container.DataItem).GetAttributeValue<Guid>("adx_eventscheduleid") %>'/>
				<asp:Button ID="ScheduleButton" runat="server" CssClass="btn btn-primary btn-sm" Text='<%# CheckIfScheduledForCurrentUser((Entity)Container.DataItem, Portal.User == null ? Guid.Empty : Portal.User.Id) ? "Remove From Schedule" : "Add To Schedule" %>' CommandName='<%# CheckIfScheduledForCurrentUser((Entity)Container.DataItem, Portal.User == null ? Guid.Empty : Portal.User.Id) ? "RemoveFromSchedule" : "AddToSchedule" %>' CommandArgument='<%# ((Entity)Container.DataItem).GetAttributeValue<Guid>("adx_eventscheduleid") %>' />
			</asp:Panel>
			<crm:CrmEntityDataSource ID="SessionEvent" runat="server" />
			<crm:CrmEntityDataSource ID="SessionEventLocation" runat="server" />
			<h4 visible="<%# ShowSessionTitle %>" class="list-group-item-heading" runat="server">
				<adx:Property ID="EventName" DataSourceID="SessionEvent" PropertyName="adx_name" EditType="text" runat="server" />
			</h4>
			<div class="content-metadata">
				<crm:DateTimeLiteral Value='<%# ((Entity)Container.DataItem).GetAttributeValue<DateTime?>("adx_starttime") %>' Format="D" runat="server" OutputTimeZoneLabel="false" /> <crm:DateTimeLiteral Value='<%# ((Entity)Container.DataItem).GetAttributeValue<DateTime?>("adx_starttime") %>' Format="hh:mm tt" OutputTimeZoneLabel="false" runat="server" /> &ndash; <crm:DateTimeLiteral Value='<%# ((Entity)Container.DataItem).GetAttributeValue<DateTime?>("adx_endtime") %>' Format="hh:mm tt" runat="server" />
			</div>
			<div class="list-group-item-text">
				<adx:Property ID="EventLocationName" DataSourceID="SessionEventLocation" PropertyName="adx_name" Format="Location: {0}" Literal="true" runat="server" />
			</div>
		</li>
	</ItemTemplate>
</asp:ListView>
