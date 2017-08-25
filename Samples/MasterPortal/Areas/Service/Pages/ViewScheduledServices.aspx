<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/Profile.master" CodeBehind="ViewScheduledServices.aspx.cs" Inherits="Site.Areas.Service.Pages.ViewScheduledServices" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="~/Areas/Service/css/service.css" />
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<div class="pull-right">
			<crm:CrmHyperLink runat="server" SiteMarkerName="BookService" CssClass="btn btn-primary">
				<span class="fa fa-plus-circle" aria-hidden="true"></span>
				<adx:Snippet runat="server" SnippetName="Services/ScheduleService/NewService" Literal="True" DefaultText="<%$ ResourceManager:Schedule_New_Service %>" />
			</crm:CrmHyperLink>
		</div>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<div class="service-schedule">
		<asp:GridView runat="server" ID="BookedAppointments" CssClass="table table-striped"
			AutoGenerateColumns="false"
			GridLines="None"
			OnRowCommand="BookedAppointments_OnRowCommand" ViewStateMode="Enabled">
			<Columns>
				<asp:TemplateField HeaderText="Scheduled Start">
					<ItemTemplate>
						<%# DateTime.Parse(Eval("scheduledstart").ToString()).ToString("ddd, MMM d, yyyy h:mm tt")%>
					</ItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="Scheduled End">
					<ItemTemplate>
						<%# DateTime.Parse(Eval("scheduledend").ToString()).ToString("ddd, MMM d, yyyy h:mm tt")%>
					</ItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="Service Type">
					<ItemTemplate>
						<%#: Eval("servicetype")%>
					</ItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="Date Booked">
					<ItemTemplate>
						<%# DateTime.Parse(Eval("dateBooked").ToString()).ToString("ddd, MMM d, yyyy h:mm tt")%>
					</ItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="Cancel Service">
					<ItemTemplate>
						<asp:Button runat="server" CssClass="btn btn-danger" CommandName="Cancel" CommandArgument='<%# Eval("serviceId") %>' Text="Cancel" />
					</ItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="">
					<ItemTemplate>
						<asp:HyperLink CssClass="btn btn-default" NavigateUrl='<%# Url.RouteUrl("ServiceAppointmentiCalendar", new { id = Eval("serviceId")}) %>' runat="server">
							<span class="fa fa-plus" aria-hidden="true"></span>
							<adx:Snippet runat="server" Literal="True" SnippetName="Service/ExportLink" DefaultText="<%$ ResourceManager:Add_To_Calendar %>"/>
						</asp:HyperLink>
					</ItemTemplate>
				</asp:TemplateField>
			</Columns>
		</asp:GridView>
	</div>
</asp:Content>