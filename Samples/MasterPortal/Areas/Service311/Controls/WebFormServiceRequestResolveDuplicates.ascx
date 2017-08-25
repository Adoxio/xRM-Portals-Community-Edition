<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WebFormServiceRequestResolveDuplicates.ascx.cs" Inherits="Site.Areas.Service311.Controls.WebFormServiceRequestResolveDuplicates" %>
<asp:Panel ID="PanelResolveDuplicates" runat="server" CssClass="row">
	<input type="hidden" ID="CurrentServiceRequestId" runat="server"/>
	<input type="hidden" ID="SelectedServiceRequestId" runat="server"/>
	<div class="col-sm-12">
		<div class="alert alert-warning">
			<adx:Snippet runat="server" SnippetName="311 Service Request Resolve Duplicates Summary" DefaultText="<%$ ResourceManager:Service_Request_Resolve_Duplicates_Summary_Message %>" />
		</div>
		<div class="panel panel-default">
			<div class="panel-heading"><adx:Snippet runat="server" SnippetName="311 Service Request Resolve Duplicates Continue Existing" DefaultText="<%$ ResourceManager:Continue_With_Existing_Service_Request %>" /></div>
			<div class="panel-body">
				<asp:PlaceHolder ID="DuplicateListPlaceholder" runat="server"></asp:PlaceHolder>
			</div>
		</div>
		<div class="panel panel-default">
			<div class="panel-heading"><adx:Snippet runat="server" SnippetName="311 Service Request Resolve Duplicates Continue My" DefaultText="<%$ ResourceManager:Continue_With_My_Service_Request %>" /></div>
			<div class="panel-body">
				<asp:PlaceHolder ID="CurrentListPlaceholder" runat="server"></asp:PlaceHolder>
			</div>
		</div>
	</div>
</asp:Panel>

<section class="modal fade" id="confirmSubmit" tabindex="-1" role="dialog" aria-labelledby="confirmSubmitLabel" aria-hidden="true">
	<div class="modal-dialog">
		<div class="modal-content">
			<div class="modal-header">
				<h1 class="modal-title h4" id="confirmSubmitLabel"><adx:Snippet runat="server" SnippetName="311 Service Request Resolve Duplicates Confirmation Header" DefaultText="<%$ ResourceManager:Confirmation_DefaultText %>" /></h1>
			</div>
			<div class="modal-body" id="continueExisting">
				<adx:Snippet runat="server" SnippetName="311 Service Request Resolve Duplicates Continue Existing Message" DefaultText="<%$ ResourceManager:Continue_With_Selected_Service_Request_Error %>" />
			</div>
			<div class="modal-body" id="continueNew">
				<adx:Snippet runat="server" SnippetName="311 Service Request Resolve Duplicates Continue New Message" DefaultText="<%$ ResourceManager:Ignore_Existing_And_Submit_My_Service_Request %>" />
			</div>
			<div class="modal-footer">
				<button type="button" class="btn btn-default" data-dismiss="modal"><adx:Snippet runat="server" SnippetName="311 Service Request Resolve Duplicates Confirmation Cancel" DefaultText="<%$ ResourceManager:Cancel_DefaultText %>" /></button>
				<button type="button" class="btn btn-primary" data-dismiss="modal" id="confirmSubmitContinue"><adx:Snippet runat="server" SnippetName="311 Service Request Resolve Duplicates Confirmation Continue" DefaultText="<%$ ResourceManager:Continue_DefaultText %>" /></button>
			</div>
		</div>
	</div>
</section>

<script type="text/javascript">
	function webFormClientValidate() {
		if ($('#confirmSubmit').attr("data-continue")) return true;

		if ($('#CurrentServiceRequestId').val() == $('#SelectedServiceRequestId').val()) {
			$('#continueExisting').hide();
			$('#continueNew').show();
		} else {
			$('#continueExisting').show();
			$('#continueNew').hide();
		}

		$('#confirmSubmit').modal();
		return false;
	}
</script>