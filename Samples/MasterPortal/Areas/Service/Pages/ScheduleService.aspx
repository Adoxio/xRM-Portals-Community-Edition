<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/WebForms.master" CodeBehind="ScheduleService.aspx.cs" Inherits="Site.Areas.Service.Pages.ScheduleService" EnableEventValidation="false" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="~/Areas/Service/css/service.css" />
</asp:Content>

<asp:Content runat="server" ViewStateMode="Enabled" ContentPlaceHolderID="ContentBottom">
	<div class="service-schedule">
		<asp:Panel ID="NoServicesMessage" CssClass="alert alert-block alert-info" runat="server" Visible="False">
			<adx:Snippet runat="server" SnippetName="Services/ScheduleService/NoServicesAvailable" DefaultText="<%$ ResourceManager:No_Services_Available_To_schedule %>" EditType="html" />
		</asp:Panel>
		
		<asp:Panel ID="NoTimesMessage" CssClass="alert alert-block alert-danger" runat="server" Visible="False">
			<adx:Snippet runat="server" SnippetName="Services/ScheduleService/NoTimesAvailable" DefaultText="<%$ ResourceManager:No_Appointments_Available_Message %>" EditType="html" />
		</asp:Panel>

		<asp:Panel runat="server" ID="SearchPanel" CssClass="form-horizontal">
			<asp:Label runat="server" ID="ErrorLabel" CssClass="alert alert-danger alert-block" Visible="False" />
			<div class="form-group">
				<asp:Label CssClass="col-sm-3 control-label" AssociatedControlID="ServiceType" Text='<%$ Snippet: Services/ScheduleService/ServiceType, Requested Service Type %>' runat="server"></asp:Label>
				<div class="col-sm-9">
					<asp:DropDownList runat="server" ID="ServiceType" CssClass="form-control" />
				</div>
			</div>
			<div class="form-group">
				<asp:Label CssClass="col-sm-3 control-label" AssociatedControlID="StartDate" Text='<%$ Snippet: Services/ScheduleService/DateRange, Service date range %>' runat="server"></asp:Label>
				<div class="col-sm-9">
					<div class="calendar">
						<asp:Calendar runat="server" ID="StartDate" />
					</div>
					<div class="calendar">
						<asp:Calendar runat="server" ID="EndDate" />
					</div>
				</div>
			</div>
			<div class="form-group">
				<asp:Label CssClass="col-sm-3 control-label" AssociatedControlID="TimeZoneSelection" Text='<%$ Snippet: Services/ScheduleService/TimeZone, Select your time zone %>' runat="server"></asp:Label>
				<div class="col-sm-9">
					<asp:DropDownList EnableViewState="True" runat="server" ID="TimeZoneSelection" CssClass="form-control" />
				</div>
			</div>
			<div class="form-group">
				<asp:Label CssClass="col-sm-3 control-label" AssociatedControlID="StartTime" Text='<%$ Snippet: Services/ScheduleService/TimeOfDay, Time of day %>' runat="server"></asp:Label>
				<div class="col-sm-9">
					<asp:DropDownList runat="server" ID="StartTime" CssClass="form-control" />
				</div>
			</div>
			<div class="form-group">
				<div class="col-sm-offset-3 col-sm-9">
					<asp:DropDownList runat="server" ID="EndTime" CssClass="form-control" />
				</div>
			</div>
			<div class="form-group">
				<div class="col-sm-offset-3 col-sm-9">
					<asp:Button runat="server" OnClick="FindTimes_Click" CssClass="btn btn-primary" Text="<%$ Snippet: Services/ScheduleService/FindAvailableTimes, Find Available Times %>" />
				</div>
			</div>
		</asp:Panel>

		<asp:Panel ID="ResultsDisplay" runat="server" Visible="false">
			<asp:Label runat="server" ID="BookingError" CssClass="alert alert-block alert-danger" Visible="False" />
			<div class="alert alert-block alert-info">
				<adx:Snippet runat="server" SnippetName="Services/ScheduleService/AppointmentTimeLabel" DefaultText="<%$ ResourceManager:Select_Available_Appointment_Times_From_List %>" EditType="html" />
			</div>
			<asp:GridView runat="server" ID="AvailableTimes" CssClass="table table-striped table-hover table-bordered"
				AutoGenerateColumns="false"
				DataKeyNames="AvailableResource, ScheduledStartUniversalTime, ScheduledEndUniversalTime"
				GridLines="None"
				OnRowDataBound="AvailableTimes_RowDataBound"
				OnSelectedIndexChanged="AvailableTimes_SelectedIndexChanged">
				<SelectedRowStyle CssClass="success"></SelectedRowStyle>
				<Columns>
					<asp:TemplateField HeaderText="Scheduled Start">
						<ItemTemplate>
							<%#: Eval("ScheduledStart") %>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Scheduled End">
						<ItemTemplate>
							<%#: Eval("ScheduledEnd") %>
						</ItemTemplate>
					</asp:TemplateField>
				</Columns>
			</asp:GridView>
			<asp:Button runat="server" ID="ScheduleServiceButton" CssClass="btn btn-primary" Text="<%$ Snippet: Services/ScheduleService/ScheduleService %>" OnClick="ScheduleService_Click" />
		</asp:Panel>
	</div>
</asp:Content>