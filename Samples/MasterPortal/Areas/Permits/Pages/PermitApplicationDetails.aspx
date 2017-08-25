<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Profile.master" AutoEventWireup="true" CodeBehind="PermitApplicationDetails.aspx.cs" Inherits="Site.Areas.Permits.Pages.PermitApplicationDetails" %>
<%@ Import Namespace="Adxstudio.Xrm" %>
<%@ Import Namespace="Adxstudio.Xrm.Notes" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Permits/css/permits.css") %>" />
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<asp:Panel ID="PermitHeader" CssClass="page-header" runat="server">
		<section class="modal" id="add-note" tabindex="-1" role="dialog" aria-labelledby="add-note-modal-label" aria-hidden="true">
			<div class="modal-dialog">
				<div class="modal-content">
					<div class="modal-header">
						<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
						<h1 id="add-note-modal-label" class="modal-title h4">
							<adx:Snippet  runat="server" SnippetName="Ecommerce/Order/AddNote/ButtonText" DefaultText="<%$ ResourceManager:Add_Note %>" Editable="true" EditType="text"/>
						</h1>
					</div>
					<div class="modal-body form-horizontal">
						<div class="form-group">
							<asp:Label AssociatedControlID="NewNoteText" CssClass="col-sm-3 control-label" runat="server">
								<adx:Snippet runat="server" SnippetName="Ecommerce/Order/AddNote/Text" DefaultText="<%$ ResourceManager:Note_DefaultText %>" />
							</asp:Label>
							<div class="col-sm-9">
								<asp:TextBox runat="server" ID="NewNoteText" TextMode="MultiLine" Rows="6" CssClass="form-control"/>
							</div>
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="NewNoteAttachment" CssClass="col-sm-3 control-label" runat="server">
								<adx:Snippet runat="server" SnippetName="Ecommerce/Order/AddNote/File" DefaultText="<%$ ResourceManager:Attach_A_File_DefaultText %>" />
							</asp:Label>
							<div class="col-sm-9">
								<div class="form-control-static">
									<asp:FileUpload ID="NewNoteAttachment" runat="server"/>
								</div>
							</div>
						</div>
					</div>
					<div class="modal-footer">
						<asp:Button CssClass="btn btn-primary" OnClick="AddNote_Click" Text='<%$ Snippet: Ecommerce/Order/AddNote/ButtonText, Add Note %>' runat="server" />
						<button class="btn btn-default" data-dismiss="modal" aria-hidden="true">
							<adx:Snippet  runat="server" SnippetName="Ecommerce/Order/AddNote/CancelButtonText" DefaultText="<%$ ResourceManager:Cancel_DefaultText %>" Literal="True" EditType="text"/>
						</button>
					</div>
				</div>
			</div>
		</section>
		
		<asp:Panel ID="PermitControls" CssClass="pull-right btn-toolbar"  runat="server">
			<asp:Panel ID="AddNote" CssClass="btn-group" runat="server">
				<a href="#add-note" class="btn btn-default" data-toggle="modal"  title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Add_Note") %>'>
					<span class="fa fa-plus-circle" aria-hidden="true"></span>
					<adx:Snippet runat="server" SnippetName="Ecommerce/Order/AddNote/ButtonText" DefaultText="<%$ ResourceManager:Add_Note %>" Literal="true" EditType="text"/>
				</a>
			</asp:Panel>
		</asp:Panel>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
		
	</asp:Panel>
</asp:Content>

<asp:Content ID="EntityControls" ContentPlaceHolderID="EntityControls" runat="server" ViewStateMode="Enabled">
	<script type="text/javascript">
		function entityFormClientValidate() {
			// Custom client side validation. Method is called by the submit button's onclick event.
			// Must return true or false. Returning false will prevent the form from submitting.
			return true;
		}
						
		function webFormClientValidate() {
			// Custom client side validation. Method is called by the next/submit button's onclick event.
			// Must return true or false. Returning false will prevent the form from submitting.
			return true;
		}
	</script>
	<adx:WebForm ID="WebFormControl" runat="server" FormCssClass="crmEntityFormView" PreviousButtonCssClass="btn btn-default" NextButtonCssClass="btn btn-primary" SubmitButtonCssClass="btn btn-primary" ClientIDMode="Static" LanguageCode="<%$ SiteSetting: Language Code, 0 %>" PortalName="<%$ SiteSetting: Language Code %>" />
	<asp:Panel CssClass="panel panel-primary" runat="server">
		<div class="panel-heading">
			<h3 class="panel-title">Permit</h3>
		</div>
		<div class="panel-body">
			<adx:EntityForm ID="EntityFormControl" runat="server" FormCssClass="crmEntityFormView serviceheader" PreviousButtonCssClass="btn btn-default" NextButtonCssClass="btn btn-primary" SubmitButtonCssClass="btn btn-primary" ClientIDMode="Static" LanguageCode="<%$ SiteSetting: Language Code, 0 %>" PortalName="<%$ SiteSetting: Language Code %>" />
		</div>
	</asp:Panel>
	<adx:EntityList ID="EntityListControl" runat="server" ListCssClass="table table-striped" DefaultEmptyListText="<%$ ResourceManager:No_Items_To_Display %>" ClientIDMode="Static" LanguageCode="<%$ SiteSetting: Language Code, 0 %>" PortalName="<%$ SiteSetting: Language Code %>" />
	<asp:Panel ID="CrmEntityFormViewPanel"  runat="server"/>
	
	<div class="page-header">
		<h3>
			<adx:Snippet SnippetName="Ecommerce/ServiceRequestNotesLabel" DefaultText="<%$ ResourceManager:Notes_DefaultText %>" runat="server" EditType="text" />
		</h3>
	</div>
		
	<asp:ListView ID="NotesList" runat="server">
		<LayoutTemplate>
			<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
		</LayoutTemplate>
		<ItemTemplate>
			<div class="note">
				<div class="row">
					<div class="col-sm-3 metadata">
						<abbr class="timeago"><%# ((IAnnotation)Container.DataItem).CreatedOn.ToString("r") %></abbr>
					</div>
					<div class="col-sm-9">
						<div class="text">
							<%# AnnotationHelper.FormatNoteText(((IAnnotation)Container.DataItem).NoteText) %>
						</div>
						<asp:Panel Visible='<%# ((IAnnotation)Container.DataItem).FileAttachment != null %>' CssClass="attachment alert alert-block alert-info" runat="server">
							<span class="fa fa-file" aria-hidden="true"></span>
							<asp:HyperLink NavigateUrl='<%#: ((IAnnotation)Container.DataItem).Entity.GetFileAttachmentUrl(Website) %>' Text='<%#: ((IAnnotation)Container.DataItem).FileAttachment != null ? string.Format("{0} ({1:1})", ((IAnnotation)Container.DataItem).FileAttachment.FileName, ((IAnnotation)Container.DataItem).FileAttachment.FileSize) : string.Empty %>' runat="server"/>
						</asp:Panel>
					</div>
				</div>
			</div>
		</ItemTemplate>
	</asp:ListView>
	<asp:Panel ID="AddNoteInline" CssClass="row" runat="server">
		<div class="col-sm-9">
			<a href="#add-note" class="btn btn-default" data-toggle="modal"  title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Add_Note") %>'>
				<span class="fa fa-plus-circle" aria-hidden="true"></span>
				<adx:Snippet runat="server" SnippetName="Ecommerce/Order/AddNote/ButtonText" DefaultText="<%$ ResourceManager:Add_Note %>" Literal="true" EditType="text"/>
			</a>
		</div>
	</asp:Panel>
</asp:Content>