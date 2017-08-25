<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HelpDesk.aspx.cs" Inherits="Site.Areas.HelpDesk.Pages.HelpDesk" MasterPageFile="~/MasterPages/WebForms.master" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Register src="~/Controls/ChildNavigation.ascx" tagname="ChildNavigation" tagprefix="site" %>
<%@ Register src="../Controls/CaseDeflection.ascx" tagname="CaseDeflection" tagprefix="adx" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/HelpDesk/css/helpdesk.css") %>">
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server" ViewStateMode="Enabled">
	<adx:CaseDeflection ID="CaseDeflection" runat="server"/>
	
	<div class="cases">
		<asp:PlaceHolder ID="NoCaseAccessWarning" Visible="False" runat="server">
			<asp:LoginView runat="server">
				<AnonymousTemplate>
					<div class="alert alert-block alert-info">
						<div class="pull-right">
							<% Html.RenderPartial("SignInLink"); %>
						</div>
						<adx:Snippet runat="server" SnippetName="cases/access/signin" DefaultText="<%$ ResourceManager:SignIn_To_View_Your_Cases %>" Editable="true" EditType="html"/>
					</div>
				</AnonymousTemplate>
				<LoggedInTemplate>
					<div class="alert alert-block alert-warning">
						<adx:Snippet runat="server" SnippetName="cases/access/nopermissions" DefaultText="<%$ ResourceManager:No_Permission_To_View_Cases %>" Editable="true" EditType="html"/>
					</div>
				</LoggedInTemplate>
			</asp:LoginView>
		</asp:PlaceHolder>

		<asp:Panel ID="CaseControls" CssClass="controls" runat="server">
			<asp:Button ID="CreateCase" CssClass="btn btn-primary gridview-nav create-case" Text='<%$ Snippet: cases/createbuttontext, Open a New Support Request %>' OnClick="CreateCase_Click" runat="server"/>
			<asp:Panel ID="CaseFilters" CssClass="row" runat="server">
				<div class="col-sm-3">
					<div class="input-group gridview-nav">
						<div class="input-group-btn">
							<button title="Filter cases using options" class="btn btn-default" type="button">
							    <span class="fa fa-filter" aria-hidden="true"></span>
							</button>
						</div>
						<asp:DropDownList ID="CustomerFilter" CssClass="form-control" AutoPostBack="true" runat="server"/>
					</div>
				</div>
				<div class="col-sm-3 gridview-nav">
					<asp:DropDownList ID="StatusDropDown" CssClass="form-control" AutoPostBack="true" runat="server">
						<asp:ListItem>Active</asp:ListItem>
						<asp:ListItem>Closed</asp:ListItem>
					</asp:DropDownList>
				</div>
			</asp:Panel>
		</asp:Panel>
		<div class="table-responsive">
			<asp:GridView ID="CaseList" runat="server" CssClass="table table-striped" GridLines="None" AlternatingRowStyle-CssClass="alternate-row" OnRowDataBound="CaseList_OnRowDataBound" >
				<EmptyDataTemplate>
					<adx:Snippet  runat="server" SnippetName="cases/view/empty" DefaultText="<%$ ResourceManager:No_Cases_For_selected_Filter %>" Editable="true" EditType="html"/>
				</EmptyDataTemplate>
			</asp:GridView>
		</div>
	</div>
	
	<site:ChildNavigation runat="server" />
    <script>
        $(document).ready(function () {
            $('#ContentContainer_ContentBottom_CaseDeflection_Subject').attr('aria-label', $('span.xrm-attribute-value').eq(1).text())
            $('#ContentContainer_ContentBottom_CaseDeflection_SearchButton').attr('aria-label', 'search button')
            $('#ContentContainer_ContentBottom_CaseList th').attr('tabindex', 0)
            });
  </script>

</asp:Content>