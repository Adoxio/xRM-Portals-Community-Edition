<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" ValidateRequest="false" AutoEventWireup="true" CodeBehind="Case.aspx.cs" Inherits="Site.Areas.HelpDesk.Pages.Case" %>
<%@ Import Namespace="Adxstudio.Xrm.Notes" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Site.Helpers" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/HelpDesk/css/helpdesk.css") %>">
</asp:Content>

<asp:Content ContentPlaceHolderID="Breadcrumbs" runat="server">
	<asp:PlaceHolder ID="CaseBreadcrumbs" runat="server">
		<ul class="breadcrumb">
			<% foreach (var node in Html.SiteMapPath()) { %>
				<% if (node.Item2 == SiteMapNodeType.Current) { %>
					<li class="active"><%: CurrentCase.Title %></li>
				<% } else { %>
					<li>
						<a href="<%: node.Item1.Url %>" title="<%: node.Item1.Title %>"><%: node.Item1.Title %></a>
					</li>
				<% } %>
			<% } %>
		</ul>
	</asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<asp:Panel ID="CaseHeader" CssClass="page-header" runat="server">
		<asp:Panel ID="CaseControls" CssClass="pull-right btn-toolbar case-controls" runat="server">
			<asp:Panel ID="ResolveCase" CssClass="btn-group" runat="server">
				<a href="#resolve-case" class="btn btn-success" data-toggle="modal" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Resolve_Case_DefaultText") %>'>
					<adx:Snippet runat="server" SnippetName="cases/editcase/resolvebuttontext" DefaultText="<%$ ResourceManager:Resolve_Case_DefaultText %>" Literal="true" EditType="text"/>
				</a>
			</asp:Panel>
			<asp:Panel ID="CancelCase" CssClass="btn-group" runat="server">
				<a href="#cancel-case" class="btn btn-danger" data-toggle="modal" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Cancel_Case_DefaultText") %>'>
					<adx:Snippet runat="server" SnippetName="cases/editcase/cancelbuttontext" DefaultText="<%$ ResourceManager:Cancel_Case_DefaultText %>" Literal="true" EditType="text"/>
				</a>
			</asp:Panel>
			<asp:Panel ID="AddNote" CssClass="btn-group" runat="server">
				<a href="#add-note" class="btn btn-default" data-toggle="modal" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Add_Note") %>'>
					<span class="fa fa-plus-circle" aria-hidden="true"></span>
					<adx:Snippet runat="server" SnippetName="cases/editcase/addnote/buttontext" DefaultText="<%$ ResourceManager:Add_Note %>" Literal="true" EditType="text"/>
				</a>
			</asp:Panel>
			<asp:Panel ID="ReopenCase" CssClass="btn-group" runat="server">
				<a href="#reopen-case" class="btn btn-default" data-toggle="modal" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Reopen_Case_DefaultText") %>'>
					<span class="fa fa-undo" aria-hidden="true"></span>
					<adx:Snippet runat="server" SnippetName="cases/editcase/reopenbuttontext" DefaultText="<%$ ResourceManager:Reopen_Case_DefaultText %>" Literal="true" EditType="text"/>
				</a>
			</asp:Panel>
		</asp:Panel>
		<h1><%: CurrentCase.Title %> <asp:Label ID="TicketNumber" runat="server"><small class="ticket-number"><%: CurrentCase.TicketNumber %></small></asp:Label></h1>
	</asp:Panel>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server" ViewStateMode="Enabled">
	
	<%: Html.Attribute("adx_copy") %>
		
	<asp:ScriptManagerProxy runat="server">
		<Scripts>
			<asp:ScriptReference Path="~/js/jquery.validate.min.js" />
		</Scripts>
	</asp:ScriptManagerProxy>
	
	<script type="text/javascript">
		$(document).ready(function () {
			$("#content_form").validate({
				errorClass: "help-block error",
				highlight: function (label) {
					$(label).closest('.form-group').removeClass('has-success').addClass('has-error');
				},
				success: function (label) {
					$(label).closest('.form-group').removeClass('has-error').addClass('has-success');
				}
			});
			$(document).on("click", "#ResolveCaseButton", function (e) {
				var isValid = $("#content_form").valid();
				if (!isValid) {
					e.preventDefault();
				}
			});
			$(".crmEntityFormView select[disabled]").replaceWith(function () {
				return $("<span />").append($(this).find("option:selected").text());
			});
		});
	</script>

	<asp:Panel ID="NoCaseAccess" Visible="false" CssClass="alert alert-block alert-danger" runat="server">
		<adx:Snippet runat="server" SnippetName="cases/editcase/nopermissions" DefaultText="<%$ ResourceManager:No_Permission_To_View_This_Case %>" Editable="true" EditType="html"/>
	</asp:Panel>

	<asp:Panel ID="CaseNotFound" Visible="false" CssClass="alert alert-block alert-danger" runat="server">
		<adx:Snippet runat="server" SnippetName="cases/editcase/casenotfound" DefaultText="<%$ ResourceManager:Case_Not_Found %>" Editable="true" EditType="html"/>
	</asp:Panel>

	<asp:Panel ID="CaseData" CssClass="case" runat="server">
		<section class="modal" id="resolve-case" tabindex="-1" aria-labelledby="resolve-case-modal-label" aria-hidden="true">
			<div class="modal-dialog modal-lg">
				<div class="modal-content">
					<div class="modal-header">
						<button title="close" type="button" class="close" data-dismiss="modal" aria-hidden="true" >
                            <span class="weblink-image fa fa-times" aria-hidden="true"></span>
						</button>
						<h1 class="modal-title h4" id="resolve-case-modal-label">
							<adx:Snippet  runat="server" SnippetName="cases/editcase/resolvebuttontext" DefaultText="<%$ ResourceManager:Resolve_Case_DefaultText %>" Editable="true" EditType="text"/>
						</h1>
					</div>
					<div class="modal-body form-horizontal">
						<div class="form-group">
							<crm:CrmMetadataDataSource ID="SatisfactionSource" runat="server" EntityName="incident" AttributeName="customersatisfactioncode" CrmDataContextName="<%$ SiteSetting: Language Code %>" />
							<asp:Label AssociatedControlID="Satisfaction" CssClass="col-sm-3 control-label required" runat="server">
								<adx:Snippet runat="server" SnippetName="cases/editcase/satisfaction" DefaultText="<%$ ResourceManager:Satisfaction_DefaultText %>" />
							</asp:Label>
							<div class="col-sm-9">
								<asp:DropDownList ID="Satisfaction" runat="server"
									CssClass="form-control required"
									DataSourceID="SatisfactionSource"
									DataTextField="OptionLabel"
									DataValueField="OptionValue" />
							</div>
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="Resolution" CssClass="col-sm-3 control-label required" runat="server">
								<adx:Snippet runat="server" SnippetName="cases/editcase/resolution" DefaultText="<%$ ResourceManager:Resolution_DefaultText %>" />
							</asp:Label>
							<div class="col-sm-9">
								<asp:TextBox runat="server" ID="Resolution" CssClass="form-control required" />
							</div>
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="ResolutionDescription" CssClass="col-sm-3 control-label required" runat="server">
								<adx:Snippet runat="server" SnippetName="cases/editcase/resolution/description" DefaultText="<%$ ResourceManager:Description_DefaultText %>" />
							</asp:Label>
							<div class="col-sm-9">
								<asp:TextBox runat="server" ID="ResolutionDescription" TextMode="MultiLine" Rows="6" CssClass="form-control required" Text='<%# CurrentCase.Resolution %>'/>
							</div>
						</div>
					</div>
					<div class="modal-footer">
						<asp:Button ID="ResolveCaseButton" CssClass="btn btn-primary" OnClick="ResolveCase_Click" Text='<%$ Snippet: cases/editcase/resolvebuttontext, Resolve Case %>' ClientIDMode="Static" runat="server" />
						<button class="btn btn-default" data-dismiss="modal" aria-hidden="true">
							<adx:Snippet  runat="server" SnippetName="cases/editcase/resolvecancelbuttontext" DefaultText="<%$ ResourceManager:Cancel_DefaultText %>" Literal="True" EditType="text"/>
						</button>
					</div>
				</div>
			</div>
		</section>
		
		<section class="modal" id="cancel-case" tabindex="-1" aria-labelledby="cancel-case-modal-label" aria-hidden="true">
			<div class="modal-dialog">
				<div class="modal-content">
					<div class="modal-header">
						<button title="close" type="button" class="close" data-dismiss="modal" aria-hidden="true">
                            <span class="weblink-image fa fa-times" aria-hidden="true"></span>
						</button>
						<h1 class="modal-title h4" id="cancel-case-modal-label">
							<adx:Snippet runat="server" SnippetName="cases/editcase/cancelbuttontext" DefaultText="<%$ ResourceManager:Cancel_Case_DefaultText %>" Editable="true" EditType="text"/>
						</h1>
					</div>
					<div class="modal-body">
						<adx:Snippet runat="server" SnippetName="cases/editcase/cancelconfirmation" DefaultText="<%$ ResourceManager:Cancel_Case_Confirmation_Message %>" Editable="true" EditType="html"/>
					</div>
					<div class="modal-footer">
						<asp:Button CssClass="btn btn-primary" OnClick="CancelCase_Click" Text='<%$ Snippet: cases/editcase/cancelconfirmationyes, Cancel this case %>' runat="server" />
						<button class="btn btn-default" data-dismiss="modal" aria-hidden="true">
							<adx:Snippet runat="server" SnippetName="cases/editcase/cancelconfirmationno" DefaultText="<%$ ResourceManager:Cancel_Confirmation_No %>" Literal="true" EditType="text"/>
						</button>
					</div>
				</div>
			</div>
		</section>
		
		<section class="modal" id="reopen-case" tabindex="-1" role="dialog" aria-labelledby="reopen-case-modal-label" aria-hidden="true">
			<div class="modal-dialog">
				<div class="modal-content">
					<div class="modal-header">
						<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
						<h1 class="modal-title h4" id="reopen-case-modal-label">
							<adx:Snippet  runat="server" SnippetName="cases/editcase/reopenbuttontext" DefaultText="<%$ ResourceManager:Reopen_Case_DefaultText %>" Editable="true" EditType="text"/>
						</h1>
					</div>
					<div class="modal-body">
						<adx:Snippet  runat="server" SnippetName="cases/editcase/reopenconfirmation" DefaultText="<%$ ResourceManager:Reopen_Case_Confirmation_Message %>" Editable="true" EditType="html"/>
					</div>
					<div class="modal-footer">
						<asp:Button CssClass="btn btn-primary" OnClick="ReopenCase_Click" Text='<%$ Snippet: cases/editcase/reopenconfirmationyes, Reopen this case %>' runat="server" />
						<button class="btn btn-default" data-dismiss="modal" aria-hidden="true">
							<adx:Snippet  runat="server" SnippetName="cases/editcase/reopenconfirmationno" DefaultText="<%$ ResourceManager:Reopen_Confirmation_No %>" Literal="true" EditType="text"/>
						</button>
					</div>
				</div>
			</div>
		</section>
		
		<section class="modal" id="add-note" tabindex="-1" aria-labelledby="add-note-modal-label" aria-hidden="true">
			<div class="modal-dialog modal-lg">
				<div class="modal-content">
					<div class="modal-header">
						<button type="button" title="close" class="close" data-dismiss="modal" aria-hidden="true">
                            <span class="weblink-image fa fa-times" aria-hidden="true"></span>
						</button>
						<h1 class="modal-title h4" id="add-note-modal-label">
							<adx:Snippet  runat="server" SnippetName="cases/editcase/addnote/buttontext" DefaultText="<%$ ResourceManager:Add_Note %>" Editable="true" EditType="text"/>
						</h1>
					</div>
					<div class="modal-body form-horizontal">
						<div class="form-group">
							<asp:Label AssociatedControlID="NewNoteText" CssClass="col-sm-3 control-label" runat="server">
								<adx:Snippet runat="server" SnippetName="cases/editcase/addnote/text" DefaultText="<%$ ResourceManager:Note_DefaultText %>" />
							</asp:Label>
							<div class="col-sm-9">
								<asp:TextBox runat="server" ID="NewNoteText" TextMode="MultiLine" Rows="6" CssClass="form-control"/>
							</div>
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="NewNoteAttachment" CssClass="col-sm-3 control-label" runat="server">
								<adx:Snippet runat="server" SnippetName="cases/editcase/addnote/file" DefaultText="<%$ ResourceManager:Attach_A_File_DefaultText %>" />
							</asp:Label>
							<div class="col-sm-9">
								<div class="form-control-static">
									<asp:FileUpload ID="NewNoteAttachment" runat="server"/>
								</div>
							</div>
						</div>
					</div>
					<div class="modal-footer">
						<asp:Button CssClass="btn btn-primary" OnClick="AddNote_Click" Text='<%$ Snippet: cases/editcase/addnote/buttontext, Add Note %>' runat="server" />
						<button class="btn btn-default" data-dismiss="modal" aria-hidden="true">
							<adx:Snippet  runat="server" SnippetName="cases/editcase/addnote/cancelbuttontext" DefaultText="<%$ ResourceManager:Cancel_DefaultText %>" Literal="True" EditType="text"/>
						</button>
					</div>
				</div>
			</div>
		</section>
		
		<asp:Panel ID="CaseInfo" CssClass="case-info" runat="server">
			<div class="status pull-right">
				<span class="label label-default <%: string.IsNullOrEmpty(CurrentCase.CaseTypeLabel) ? "hide" : string.Empty %>"><%: CurrentCase.CaseTypeLabel %></span>
				<span class="label <%= CurrentCase.IsActive ? "label-info" : string.Empty %> <%= CurrentCase.IsResolved ? "label-success" : string.Empty %> <%= CurrentCase.IsCanceled ? "label-important" : string.Empty %>"><%: CurrentCase.StateLabel %> &ndash; <%: CurrentCase.StatusLabel %></span>
			</div>
			<div class="opened-by">
				<asp:HyperLink ID="UserAvatar" CssClass="user-avatar" NavigateUrl='<%# Url.AuthorUrl(CurrentCase) %>' ImageUrl='<%# Url.UserImageUrl(CurrentCase) %>' ToolTip='<%# HttpUtility.HtmlEncode(CurrentCase.ResponsibleContactName ?? "") %>' runat="server"/>
				Opened <abbr class="timeago"><%: CurrentCase.CreatedOn.ToString("r") %></abbr>
				<asp:Label ID="UserName" runat="server">
					by <asp:HyperLink NavigateUrl='<%# Url.AuthorUrl(CurrentCase) %>' Text='<%# HttpUtility.HtmlEncode(CurrentCase.ResponsibleContactName ?? "") %>' runat="server"/>
				</asp:Label>
			</div>
		</asp:Panel>
		
		<asp:Panel ID="UpdateSuccessMessage" runat="server" CssClass="alert alert-success alert-block" Visible="False">
			<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a>
			<adx:Snippet runat="server" SnippetName="Case Update Success Text" DefaultText="<%$ ResourceManager:Case_Updated_Successfully %>" Editable="true" EditType="html" />
		</asp:Panel>

		<asp:Panel ID="CaseExtendedInfo" Visible="True" runat="server">
			<asp:Panel ID="PublicForm" CssClass="crmEntityFormView readonly" runat="server" Visible="false">
				<adx:CrmEntityFormView runat="server" ID="PublicFormView"
					EntityName="incident"
					FormName="Public Case Web Form"
					ShowUnsupportedFields="False"
					AutoGenerateSteps="False"
					Mode="ReadOnly"
					DataBindOnPostBack="True"
					LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
					ContextName="<%$ SiteSetting: Language Code %>">
				</adx:CrmEntityFormView>
			</asp:Panel>
			
			<asp:Panel ID="PrivateOpenCaseForm" runat="server" CssClass="crmEntityFormView well" Visible="false">
				<adx:CrmEntityFormView runat="server" ID="PrivateOpenCaseFormView"
					EntityName="incident" FormName="Private Open Case Web Form"
					RecommendedFieldsRequired="True"
					ShowUnsupportedFields="False"
					ToolTipEnabled="False" Mode="Edit"
					AutoGenerateSteps="False"
					ValidationGroup="PrivateOpenCase"
					ValidationSummaryCssClass="alert alert-danger alert-block"
					SubmitButtonCssClass="btn btn-primary button submit"
					SubmitButtonText="Update"
					OnItemUpdated="OnItemUpdated"
					DataBindOnPostBack="True"
					LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
					ContextName="<%$ SiteSetting: Language Code %>">
				</adx:CrmEntityFormView>
			</asp:Panel>
			
			<asp:Panel ID="PrivateClosedCaseForm" CssClass="crmEntityFormView readonly" runat="server" Visible="false">
				<adx:CrmEntityFormView runat="server" ID="PrivateClosedCaseFormView"
					EntityName="incident" FormName="Private Closed Case Web Form"
					ShowUnsupportedFields="False"
					AutoGenerateSteps="False"
					Mode="ReadOnly"
					DataBindOnPostBack="True"
					LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
					ContextName="<%$ SiteSetting: Language Code %>">
				</adx:CrmEntityFormView>
			</asp:Panel>
		</asp:Panel>
		
		<asp:Panel ID="Notes" CssClass="notes" runat="server">
			<div class="page-header">
				<h3>
					<adx:Snippet runat="server" SnippetName="Case Notes Header" DefaultText="<%$ ResourceManager:Notes_DefaultText %>" Editable="true" EditType="html"/>
				</h3>
			</div>
			<asp:ObjectDataSource ID="NoteDataSource" TypeName="Adxstudio.Xrm.Cases.ICaseDataAdapter" OnObjectCreating="GetCurrentCaseDataAdapter" SelectMethod="SelectNotes" runat="server" />
			<asp:ListView DataSourceID="NoteDataSource" runat="server">
				<LayoutTemplate>
					<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
				</LayoutTemplate>
				<ItemTemplate>
					<div class="note" runat="server">
						<div class="row">
							<div class="col-sm-3 metadata">
								<p><abbr class="timeago"><%#: Eval("CreatedOn", "{0:r}") %></abbr></p>
							</div>
							<div class="col-sm-9">
								<div class="text">
									<%#: AnnotationHelper.FormatNoteText(Eval("NoteText") as string) %>
								</div>
								<asp:Panel Visible='<%# Eval("FileAttachment") != null %>' CssClass="attachment alert alert-block alert-info" runat="server">
									<span class="fa fa-file" aria-hidden="true"></span>
									<asp:HyperLink NavigateUrl='<%#: Eval("FileAttachment.Url") %>' Text='<%# HttpUtility.HtmlEncode(string.Format("{0} ({1:1})", Eval("FileAttachment.FileName"), Eval("FileAttachment.FileSize"))) %>' runat="server"/>
								</asp:Panel>
							</div>
						</div>
					</div>
				</ItemTemplate>
				<EmptyDataTemplate>
					<div class="alert alert-block alert-info">
						<adx:Snippet runat="server" SnippetName="No Case Notes Message" DefaultText="<p><%$ ResourceManager:No_Case_Notes_Message %></p>" Editable="true" EditType="html"/>
					</div>
				</EmptyDataTemplate>
			</asp:ListView>
			<asp:Panel ID="AddNoteInline" CssClass="row" runat="server">
				<div class="col-sm-offset-3 col-sm-9">
					<a href="#add-note" class="btn btn-default" data-toggle="modal" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Add_Note") %>'>
						<span class="fa fa-plus-circle" aria-hidden="true"></span>
						<adx:Snippet runat="server" SnippetName="cases/editcase/addnote/buttontext" DefaultText="<%$ ResourceManager:Add_Note %>" Literal="true" EditType="text"/>
					</a>
				</div>
			</asp:Panel>
		</asp:Panel>
	</asp:Panel>
    <script>
        $(document).ready(function () {
            $('#ContentContainer_MainContent_PrivateOpenCaseFormView_customerid_name').removeAttr('disabled');
            $('td.clearfix').find('.info.required').siblings('div.control').find('p').removeAttr('disabled');
            $('td.clearfix').find('.info.required').siblings('div.control').find('p').attr('tabindex', 0)
            $('td.clearfix').find('.info.required').siblings('div.control').find('p').focusin(function () {
                $(this).attr('aria-label',$('#description_label').text()+$('td.clearfix').find('.info.required').siblings('div.control').find('p').text());
            });
        });
  </script>

</asp:Content>