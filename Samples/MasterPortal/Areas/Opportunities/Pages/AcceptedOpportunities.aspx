<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="AcceptedOpportunities.aspx.cs" Inherits="Site.Areas.Opportunities.Pages.AcceptedOpportunities" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Opportunities/css/opportunities.css") %>">
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<div class="pull-right">
			<asp:LinkButton ID="ExportBtn" CssClass="btn btn-default" runat="server" OnClick="ExportButton_Click" >
				<span class="fa fa-list-alt" aria-hidden="true"></span>
				<asp:Literal runat="server" Text="<%$ Snippet: accepted-opportunities/export-to-excel-link, Export to Excel %>"></asp:Literal>
			</asp:LinkButton>
			<adx:SiteMarkerLinkButton ID="CreateButton" runat="server" SiteMarkerName="Create Opportunity" CssClass="btn btn-success" >
				<span class="fa fa-plus-circle" aria-hidden="true"></span>
				Create New
			</adx:SiteMarkerLinkButton>
		</div>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" ViewStateMode="Enabled" runat="server">
	<div class="row">
		<div class="col-sm-3">
			<asp:Panel ID="CustomerFilter" CssClass="input-group gridview-nav" runat="server">
				<asp:Label CssClass="input-group-addon" runat="server">View</asp:Label>
				<asp:DropDownList ID="CustomerDropDown" AutoPostBack="true" CssClass="form-control" runat="server">
				</asp:DropDownList>
			</asp:Panel>
		</div>
		<div class="col-sm-3">
			<asp:Panel ID="StatusFilter" CssClass="input-group gridview-nav" runat="server">
				<asp:Label CssClass="input-group-addon" runat="server">Status</asp:Label>
				<asp:DropDownList ID="StatusDropDown" AutoPostBack="true" CssClass="form-control" runat="server">
					<asp:ListItem>Open</asp:ListItem>
					<asp:ListItem>All</asp:ListItem>
					<asp:ListItem>Won</asp:ListItem>
					<asp:ListItem>Lost</asp:ListItem>
				</asp:DropDownList>
			</asp:Panel>
		</div>
		<div class="col-sm-offset-2 col-sm-4">
			<div id="search" class="input-group gridview-nav">
				<asp:TextBox  title="Search editing text field" ID="SearchText" runat="server" CssClass="form-control text" placeholder="Search" />
				<div class="input-group-btn">
					<asp:LinkButton ID="SearchButton" runat="server" CssClass="btn btn-default button" role="button" ToolTip="Search">
						<span class="fa fa-search" aria-hidden="true"></span>
					</asp:LinkButton>
				</div>
			</div>
		</div>
	</div>

	<adx:Snippet ID="NoOpportunityAccessLabel" CssClass="alert alert-block alert-danger" runat="server" SnippetName="accepted-opportunities/no_access" DefaultText="<%$ ResourceManager:Dont_Have_Opportunity_Permissions %>" Editable="true" EditType="html" />

	<div id="accepted-opportunities">
		<asp:GridView ID="AcceptedOpportunitiesList" runat="server"
			CssClass="table table-striped"
			GridLines="None"
			AlternatingRowStyle-CssClass="alternate-row" 
			AllowSorting="true"
			OnSorting="AcceptedOpportunitiesList_Sorting"
			OnRowDataBound="LeadsList_OnRowDataBound">
			<EmptyDataRowStyle CssClass="empty" />
			<EmptyDataTemplate>
				<adx:Snippet runat="server" SnippetName="accepted-opportunities/list/empty" DefaultText="<%$ ResourceManager:No_Items_To_Display %>" Editable="true" EditType="html" />
			</EmptyDataTemplate>
		</asp:GridView>
	</div>
	<adx:Snippet runat="server" SnippetName="accepted-opportunities/legend" DefaultText="<%$ ResourceManager:Accepted_Opportunities_Legend %>" Editable="true" EditType="html" />
</asp:Content>

<asp:Content ContentPlaceHolderID="Scripts" runat="server">
		<script type="text/javascript">
		$(function () {
			$(".tabular-data tr").not(":has(th)").click(function () {
				window.location.href = $(this).find("a").attr("href");
			});

			$(".tabular-data td.accepted-date").each(function () {
				var dateTime = new Date($(this).text());
				$(this).text(dateTime.toString("yyyy/MM/dd HH:mm"));
			});

			if ($("#search input.text").val().length == 0) {
				$("#icon_clear").hide();
			}

			$("#search input.text").keyup(function () {
				if ($("#search input.text").val().length > 0) {
					$("#icon_clear").fadeIn(300);
				}
				else {
					$("#icon_clear").fadeOut(300);
				}
			});

			$("#icon_clear").click(function () {
				$("#search input.text").val("");
				$("#search input.button").click();
			});

			$("form").submit(function () {
				blockUI();
			});

			$("#filters select").change(function () {
				blockUI();
			});

			$(".tabular-data th a").click(function () {
				blockUI();
			});
		});

		function blockUI() {
			$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
		}
	</script>
</asp:Content>