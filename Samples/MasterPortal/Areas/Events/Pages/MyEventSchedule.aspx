<%@ Page Language="C#" MasterPageFile="~/MasterPages/Profile.master" AutoEventWireup="true" CodeBehind="MyEventSchedule.aspx.cs" Inherits="Site.Areas.Events.Pages.MyEventSchedule" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Register TagPrefix="adx" TagName="SessionScheduleDetails" Src="~/Areas/Events/Controls/SessionScheduleDetails.ascx" %>

<asp:Content ContentPlaceHolderID="ContentBottom" ViewStateMode="Enabled" EnableViewState="True" runat="server">
	<adx:SessionScheduleDetails ID="SessionSchedule" runat="server" ShowSessionTitle="true" />

	<asp:Panel ID="EmptyPanel" CssClass="alert alert-info alert-block clearfix" runat="server">
		<div class="pull-right">
			<crm:CrmHyperLink CssClass="btn btn-info btn-xs" SiteMarkerName="Events" runat="server">
				<span class="fa fa-share" aria-hidden="true"></span>
				<%: Html.SnippetLiteral("My Event Schedule View Events Text", ResourceManager.GetString("View_Upcoming_Events")) %>
			</crm:CrmHyperLink>
		</div>
		<adx:Snippet SnippetName="My Event Schedule Empty Message" DefaultText="<%$ ResourceManager:You_Are_Not_Registered_For_Events_Message %>" EditType="html" runat="server"/>
	</asp:Panel>
</asp:Content>
