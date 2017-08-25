<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="NewOpportunities.aspx.cs" Inherits="Site.Areas.Opportunities.Pages.NewOpportunities" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Opportunities/css/opportunities.css") %>">
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<div id="new-opportunities">
		<div id="select-all-container" class="btn-group">
			<a href="#" class="btn btn-default" onclick="clearAll(); return false;" title="Clear All">Clear All</a>
			<a href="#" class="btn btn-default" onclick="selectAll('accept'); return false;" title="Accept All">Accept All</a>
			<a href="#" class="btn btn-default" onclick="selectAll('decline'); return false;" title="Decline All">Decline All</a>
		</div>

		<asp:GridView ID="NewOpportunitiesList" runat="server" CssClass="table table-striped" GridLines="None" AlternatingRowStyle-CssClass="alternate-row" AllowSorting="true" OnSorting="NewOpportunitiesList_Sorting" OnRowDataBound="OpportunitiesList_OnRowDataBound" >
			<EmptyDataRowStyle CssClass="empty" />
			<EmptyDataTemplate>
				<adx:Snippet runat="server" SnippetName="new-opportunities/list/empty" DefaultText="<%$ ResourceManager:No_Items_To_Display %>" Editable="true" EditType="html" />
			</EmptyDataTemplate>
		</asp:GridView>

		<div class="form-actions">
			<asp:Button ID="SaveButton" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="SaveButton_Click" />
		</div>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="Scripts" runat="server">
		<script type="text/javascript">
		$(function () {
			$("#new-opportunities .table input").each(function () {
				$(this).click(function () {
					$("#new-opportunities .table input." + $(this).attr("class")).filter(":checked").not(this).removeAttr("checked");
				});
			});

			$("form").submit(function () {
				prepareForPostBack();
			});

			$(".table th a").click(function () {
				prepareForPostBack();
			});

			$(".table tr.saved input").attr("disabled", true);
		});

		function selectAll(type) {
			$("#new-opportunities .table ." + type + " input").each(function () {
				if ($(this).is(":not(:checked)")) {
					$(this).click();
				}
			});
		}

		function clearAll() {
			$("#new-opportunities .table input").each(function () {
				if ($(this).is(":checked")) {
					$(this).click();
				}
			});
		}

		function prepareForPostBack() {
			$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
			$(".table tr.saved input").removeAttr("disabled");
		}
	</script>
</asp:Content>