<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebFormsContent.master" AutoEventWireup="true" CodeBehind="EventFeedback.aspx.cs" Inherits="Site.Areas.Events.Pages.EventFeedback" %>
<%@ OutputCache CacheProfile="User" %>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<asp:Panel ID="ThankYouPanel" runat="server" CssClass="alert alert-block alert-success" Visible="false">
		<adx:Snippet runat="server" SnippetName="SessionFeedback/ThankyouMessage" DefaultText="<%$ ResourceManager:Thank_You_For_Feedback %>" EditType="html" />
	</asp:Panel>

	<asp:Panel ID="SessionScheduleNotFoundMessage" CssClass="alert alert-block alert-danger" Visible="false" runat="server">
		<adx:Snippet runat="server" SnippetName="sessionFeedback/NotFound" DefaultText="<%$ ResourceManager:Requested_Session_Schedule_Not_Found %>" EditType="html" />
	</asp:Panel>
	
	<div class="page-header vevent">
		<h3>
			<asp:Label runat="server" ID="EventName" CssClass="summary" />
			<small>
				<abbr class="dtstart"><asp:Label runat="server" ID="EventDate" /></abbr>
			</small>
		</h3>
	</div>

	<asp:Panel ID="FeedbackFormPanel" runat="server">
		<adx:CrmDataSource ID="WebFormDataSource" runat="server" CrmDataContextName="<%$ SiteSetting: Language Code %>" />
		<adx:CrmEntityFormView runat="server" CssClass="crmEntityFormView"
			DataSourceID="WebFormDataSource" 
			FormName="Web Form"
			EntityName="adx_eventsessionfeedback" 
			OnItemInserting="OnItemInserting"
			OnItemInserted="OnItemInserted"
			LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
			ContextName="<%$ SiteSetting: Language Code %>" />
	</asp:Panel>
</asp:Content>