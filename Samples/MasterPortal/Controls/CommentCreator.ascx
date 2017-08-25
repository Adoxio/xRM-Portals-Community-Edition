<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommentCreator.ascx.cs" Inherits="Site.Controls.CommentCreator" %>

<div class="post-comment-new-form">
	<fieldset>
		<legend>
			<asp:Literal Text='<%$ Snippet: New Comment Heading, Comments_Post_A_Comment %>' runat="server"></asp:Literal>
		</legend>
		<asp:ValidationSummary CssClass="alert alert-danger alert-block" ValidationGroup="NewComment" runat="server" />
		<asp:Panel ID="NewCommentAuthorInfoPanel" CssClass="author" runat="server" >
			<div class="form-group">
				<asp:Label CssClass="control-label required" Text='<%$ Snippet: New Comment Author Name Label, Name_DefaultText %>' AssociatedControlID="CommentAuthorName" runat="server" />
				<div>
					<asp:TextBox ID="CommentAuthorName" ValidationGroup="NewComment" CssClass="form-control" runat="server"></asp:TextBox>
				</div>
				<asp:RequiredFieldValidator Display="None" ControlToValidate="CommentAuthorName" ValidationGroup="NewComment" ErrorMessage="<%$ ResourceManager:Name_Required_Field_Validation_Message %>" Text="*" runat="server"></asp:RequiredFieldValidator>
			</div>
			<div class="form-group">
				<asp:Label CssClass="control-label" Text='<%$ Snippet: New Comment Author Email Label, Email_DefaultText %>' AssociatedControlID="CommentAuthorEmail" runat="server" />
				<div>
					<asp:TextBox ID="CommentAuthorEmail" ValidationGroup="NewComment" CssClass="form-control" runat="server"></asp:TextBox>
				</div>
			</div>
			<div class="form-group">
				<asp:Label CssClass="control-label" Text='<%$ Snippet: New Comment Author URL Label, URL_Default_Text %>' AssociatedControlID="CommentAuthorURL" runat="server" />
				<div>
					<asp:TextBox ID="CommentAuthorUrl" ValidationGroup="NewComment" CssClass="form-control" runat="server"></asp:TextBox>
				</div>
			</div>
		</asp:Panel>
		<adx:CrmDataSource ID="NewCommentDataSource" runat="server" CrmDataContextName="<%$ SiteSetting: Language Code %>" />
		<adx:CommentCreatorFormView ID="NewCommentFormView" OnPreRender="NewCommentFormView_OnPreRender" DataSourceID="NewCommentDataSource"
			CssClass="crmEntityFormView html-editors" Mode="Insert" AutoGenerateSteps="False" OnItemInserting="NewComment_OnItemInserting"
			OnItemInserted="NewComment_OnItemInserted" FormName="New Comment Form" ValidationGroup="NewComment" runat="server">
			<InsertItemTemplate>
				<div class="form-actions">
					<asp:Button CommandName="Insert" ValidationGroup="NewComment" Text='<%$ Snippet: New Comment Submit Button Text, Comments_Post_This_Comment %>' CssClass="btn btn-primary" runat="server"/>
				</div>
			</InsertItemTemplate>
		</adx:CommentCreatorFormView>
	</fieldset>
</div>