<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="Site.Areas.Conference.Pages.Home" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Register tagPrefix="site" tagName="NewsPanel" src="~/Controls/NewsPanel.ascx" %>

<asp:Content ContentPlaceHolderID="ContentHeader" runat="server"/>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<%: Html.HtmlAttribute("adx_copy", cssClass: "page-copy") %>
	<div class="row">
		<div class="col-md-4">
			<div class="content-panel panel panel-default">
				<div class="panel-heading">
					<h4>
						<span class="fa fa-users" aria-hidden="true"></span>
						<%: Html.AttributeLiteral(PortalConferenceViewEntity, "adx_name") %>
					</h4>
				</div>
				<div class="panel-body">
					<%: Html.HtmlAttribute(PortalConferenceViewEntity, "adx_summary", cssClass: "content-caption") %>
					<% if (Request.IsAuthenticated)
					   { %>
						<% if (!UserIsRegisteredForConference)
						   { %>
							<asp:LinkButton ID="Register" CssClass="btn btn-lg btn-block btn-primary" OnClick="Register_Click" runat="server">
								<span class="fa fa-edit" aria-hidden="true"></span>
								<adx:Snippet SnippetName="Conference Register Button Text" DefaultText="<%$ ResourceManager:Register_DefaultText %>" Literal="True" runat="server"/>
							</asp:LinkButton>
						<% }
						   else
						   { %>
							<adx:Snippet SnippetName="Conference Registered Text" CssClass="alert alert-block alert-info" DefaultText="<%$ ResourceManager:You_Registered_For_This_Conference_Message %>" EditType="Html" runat="server"/>
						<% } %>
					<% }
					   else
					   { %>
						<div class="alert alert-info alert-block">
							<div class="pull-right">
								<% Html.RenderPartial("SignInLink"); %>
							</div>
							<adx:Snippet SnippetName="Conference Sign in to Register Button Text" DefaultText="<%$ ResourceManager:SignIn_To_Register_DefaultText %>" EditType="text" runat="server"/>
						</div>
					<% } %>
				</div>
			</div>
		</div>
		<div class="col-md-4">
			<site:NewsPanel runat="server" />
		</div>
		<div class="col-md-4">
			<% Html.RenderAction("PollPlacement", "Poll", new { Area = "Cms", id = "Home", __portalScopeId__ = Website.Id }); %>
		</div>
	</div>
</asp:Content>