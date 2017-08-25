<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="ConferenceEvent.aspx.cs" Inherits="Site.Areas.Conference.Pages.ConferenceEvent" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm.Notes" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Events/css/events.css") %>">
	<link rel="profile" href="http://microformats.org/profile/hcalendar">
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<div class="page-header">
		<h1>
			<%: Html.TextAttribute("adx_name") %>
		</h1>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div class="row">
		<div class="col-md-8 vevent">
			<% if (RequestEventOccurrence != null) { %>
				<h3 class="event-occurence">
					<% if (RequestEventOccurrence.IsAllDayEvent) { %>
						<abbr class="dtstart" data-format="MMMM dd, yyyy" title="<%: RequestEventOccurrence.Start.ToString("yyyy-MM-ddTHH:mm:ssZ") %>"><%: RequestEventOccurrence.Start.ToString("r") %></abbr>
					<% } else { %>
						<abbr class="dtstart" title="<%: RequestEventOccurrence.Start.ToString("yyyy-MM-ddTHH:mm:ssZ") %>"><%: RequestEventOccurrence.Start.ToString("r") %></abbr> &ndash; <abbr class="dtend" title="<%: RequestEventOccurrence.End.ToString("yyyy-MM-ddTHH:mm:ssZ") %>"><%: RequestEventOccurrence.End.ToString("r") %></abbr>
					<% } %>
				</h3>
			<% } %>

			<% if (IsRegistered) { %>
				<% if (!string.IsNullOrEmpty(Entity.GetAttributeValue<string>("adx_content"))) { %>
					<div class="alert alert-info alert-block">
						<%: Html.HtmlAttribute("adx_content") %>
					</div>
				<% } else { %>
					<%: Html.HtmlAttribute("adx_content") %>
				<% } %>
			<% } %>
			
			<%: Html.HtmlAttribute("adx_description", cssClass: "page-copy") %>
			
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
		</div>
		<div class="col-md-4">
			<div class="sidebar">
				<% if (RequiresRegistration) { %>
					<div class="section">
					<% if (CanRegister) { %>
						<% if (!IsRegistered) { %>
							<asp:LinkButton ID="Register" CssClass="btn btn-block btn-lg btn-primary" OnClick="Register_Click" runat="server">
								<span class="fa fa-plus-circle" aria-hidden="true"></span>
								<adx:Snippet SnippetName="Event Register Button Text" DefaultText="<%$ ResourceManager:Event_Register_DefaultText %>" Literal="True" runat="server"/>
							</asp:LinkButton>
						<% } else { %>
							<asp:LinkButton ID="Unregister" CssClass="btn btn-block btn-lg btn-danger" OnCommand="Unregister_Click" runat="server">
								<span class="fa fa-minus-circle" aria-hidden="true"></span>
								<adx:Snippet SnippetName="Event Unregister Button Text" DefaultText="<%$ ResourceManager:Unregister_From_Event %>" Literal="True" runat="server"/>
							</asp:LinkButton>
						<% } %>
					<% } else if (Request.IsAuthenticated && !UserIsRegisteredForConference) { %>
						<asp:LinkButton ID="RegisterForConference" CssClass="btn btn-block btn-lg btn-primary" OnClick="RegisterForConference_Click" runat="server">
							<span class="fa fa-edit" aria-hidden="true"></span>
								<adx:Snippet SnippetName="Conference Register Button Text" DefaultText="<%$ ResourceManager:Register_DefaultText %>" Literal="True" runat="server"/>
						</asp:LinkButton>
					<% } else { %>
						<div class="alert alert-info alert-block">
							<div class="pull-right">
								<% Html.RenderPartial("SignInLink"); %>
							</div>
							<adx:Snippet SnippetName="Conference Sign in to Register Button Text" DefaultText="<%$ ResourceManager:SignIn_To_Register_DefaultText %>" EditType="text" runat="server"/>
						</div>
					<% } %>
					</div>
				<% } %>
				<% if (RequestEventOccurrence != null) { %>
					<div class="section">
						<a class="btn btn-block btn-lg btn-default" href="<%: Url.RouteUrl("iCalendar", new {eventScheduleId = RequestEventOccurrence.EventSchedule.Id}) %>" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Add_To_My_Calendar") %>'>
							<span class="fa fa-plus" aria-hidden="true"></span>
							<adx:Snippet SnippetName="Event Export Button Text" DefaultText="<%$ ResourceManager:Add_To_My_Calendar %>" Literal="True" runat="server"/>
						</a>
					</div>
				<% } %>
				<asp:ListView ID="OtherOccurrences" runat="server">
					<LayoutTemplate>
						<div class="content-panel panel panel-default">
							<div class="panel-heading">
								<h4>
									<span class="fa fa-calendar" aria-hidden="true"></span>
									<adx:Snippet SnippetName="Event Upcoming Times Heading" DefaultText="<%$ ResourceManager:UpComing_Times_DefaultText %>" EditType="text" runat="server"/>
								</h4>
							</div>
							<div class="list-group">
								<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
							</div>
						</div>
					</LayoutTemplate>
					<ItemTemplate>
						<a class="list-group-item vevent" href="<%# Eval("Url") %>">
							<asp:PlaceHolder Visible='<%# Eval("IsAllDayEvent") %>' runat="server">
								<abbr class="dtstart" data-format="MMMM dd, yyyy" title="<%# Eval("Start", "{0:yyyy-MM-ddTHH:mm:ssZ}") %>"><%# Eval("Start", "{0:r}") %></abbr>
							</asp:PlaceHolder>
							<asp:PlaceHolder Visible='<%# !(bool)Eval("IsAllDayEvent") %>' runat="server">
								<abbr class="dtstart" title="<%# Eval("Start", "{0:yyyy-MM-ddTHH:mm:ssZ}") %>"><%# Eval("Start", "{0:r}") %></abbr> &ndash; <abbr class="dtend" title="<%# Eval("End", "{0:yyyy-MM-ddTHH:mm:ssZ}") %>"><%# Eval("End", "{0:r}") %></abbr>
							</asp:PlaceHolder>
						</a>
					</ItemTemplate>
				</asp:ListView>
			</div>
		</div>
	</div>
</asp:Content>