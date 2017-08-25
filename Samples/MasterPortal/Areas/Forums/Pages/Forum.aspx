<%@ Page Language="C#" MasterPageFile="../MasterPages/Forums.master" AutoEventWireup="true" ValidateRequest="false" CodeBehind="Forum.aspx.cs" Inherits="Site.Areas.Forums.Pages.Forum" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm.Forums" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Site.Areas.Forums" %>
<%@ Import Namespace="Site.Helpers" %>

<asp:Content ContentPlaceHolderID="Breadcrumbs" runat="server">
	<% Html.RenderPartial("ForumBreadcrumbs"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header forums-page-header">
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_name" EditType="text" runat="server" />
			<small>
				<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_description" EditType="text" runat="server" />
			</small>
		</h1>
		<asp:Panel ID="ForumControls" CssClass="forum-controls pull-right" runat="server">
			<a class="btn btn-primary" href="#new" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Create_New_Thread_DefaultText") %>'>
				<span class="fa fa-plus-circle" aria-hidden="true"></span>
				<adx:Snippet SnippetName="Forum Thread Create Heading" DefaultText="<%$ ResourceManager:Create_New_Thread_DefaultText %>" Literal="True" runat="server"/>
			</a>
		</asp:Panel>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<asp:ObjectDataSource ID="ForumAccouncementDataSource" TypeName="Adxstudio.Xrm.Forums.IForumDataAdapter" OnObjectCreating="CreateForumDataAdapter" SelectMethod="SelectAnnouncements" runat="server" />
	<asp:ListView DataSourceID="ForumAccouncementDataSource" runat="server">
		<LayoutTemplate>
			<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
		</LayoutTemplate>
		<ItemTemplate>
			<crm:CrmEntityDataSource ID="Announcement" DataItem='<%# Eval("Entity") %>' runat="server"/>
			<div class="alert alert-block alert-info">
				<h4>
					<adx:Property DataSourceID="Announcement" PropertyName="adx_name" EditType="text" runat="server"/>
				</h4>
				<adx:Property DataSourceID="Announcement" PropertyName="adx_content" EditType="html" runat="server"/>
			</div>
		</ItemTemplate>
	</asp:ListView>

	<asp:ObjectDataSource ID="ForumThreadDataSource" TypeName="Adxstudio.Xrm.Forums.IForumThreadAggregationDataAdapter" OnObjectCreating="CreateForumDataAdapter" SelectMethod="SelectThreads" SelectCountMethod="SelectThreadCount" EnablePaging="True" runat="server" />
	<asp:ListView ID="ForumThreads" DataSourceID="ForumThreadDataSource" OnDataBound="ForumThreads_DataBound" runat="server">
		<LayoutTemplate>
			<table class="table forums forum-threads table-striped table-fluid">
				<thead>
					<tr>
						<th class="labels"></th>
						<th class="name">
							<adx:Snippet SnippetName="Forum Thread Name Heading" DefaultText="<%$ ResourceManager:Thread_DefaultText %>" EditType="text" runat="server"/>
						</th>
						<th class="author">
							<adx:Snippet SnippetName="Forum Thread Author Heading" DefaultText="<%$ ResourceManager:Author_DefaultText %>" EditType="text" runat="server"/>
						</th>
						<th class="last-post">
							<adx:Snippet SnippetName="Forum Thread Last Post Heading" DefaultText="<%$ ResourceManager:Last_Post_DefaultText %>" EditType="text" runat="server"/>
						</th>
						<th class="count">
							<adx:Snippet SnippetName="Forum Thread Reply Count Heading" DefaultText="<%$ ResourceManager:Replies_DefaultText %>" EditType="text" runat="server"/>
						</th>
					</tr>
				</thead>
				<tbody>
					<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
				</tbody>
			</table>
			
			<adx:UnorderedListDataPager ID="ForumThreadsPager" CssClass="pagination" PagedControlID="ForumThreads" QueryStringField="page" PageSize='<%$ SiteSetting: Forums/ThreadsPerPage, 20 %>' runat="server">
				<Fields>
					<adx:ListItemNextPreviousPagerField ShowNextPageButton="false" ShowFirstPageButton="True" FirstPageText="&laquo;" PreviousPageText="&lsaquo;" />
					<adx:ListItemNumericPagerField ButtonCount="10" PreviousPageText="&hellip;" NextPageText="&hellip;" />
					<adx:ListItemNextPreviousPagerField ShowPreviousPageButton="false" ShowLastPageButton="True" LastPageText="&raquo;" NextPageText="&rsaquo;" />
				</Fields>
			</adx:UnorderedListDataPager>
		</LayoutTemplate>
		<ItemTemplate>
			<tr>
				<td class="labels">
					<asp:Label CssClass="label label-default" Visible='<%# (bool)Eval("IsSticky") &&  (bool)Eval("Locked")%>' runat="server"><span class="fa fa-lock" aria-hidden="true"></span> <%=ResourceManager.GetString("Sticky_Label")%></asp:Label>
					<asp:Label CssClass="label label-default" Visible='<%# (bool)Eval("IsSticky") && (!(bool)Eval("Locked")) %>' Text="<%$ ResourceManager:Sticky_Label%>" runat="server"/>
					<asp:Label CssClass="fa fa-lock" Visible='<%# (bool)Eval("Locked") &&  (!(bool)Eval("IsSticky"))%>' ToolTip='<%$ Snippet: forums/thread/locked, Thread_Locked %>' runat="server" />
					<asp:Label CssClass="fa fa-check" Visible='<%# Eval("IsAnswered") %>' ToolTip='<%$ Snippet: Forum Thread Is Answered ToolTip, Forum_Thread_Is_Answered %>' runat="server"></asp:Label>
					<asp:Label CssClass="fa fa-question-circle" Visible='<%# (bool)Eval("ThreadType.RequiresAnswer") && (!(bool)Eval("IsAnswered")) && (!(bool)Eval("Locked"))%>' ToolTip='<%$ Snippet: Forum Thread Requires Answer ToolTip, Forum_Thread_Requires_Answer %>' runat="server"/>
				</td>
				<td class="name">
					<h4><asp:HyperLink NavigateUrl='<%#: Eval("Url") %>' Text='<%#: Eval("Name") ?? "" %>' ToolTip='<%# Eval("Name")%>' runat="server"/></h4>
				</td>
				<td class="author">
					<a class="author-link" href='<%# Url.AuthorUrl(Eval("Author") as IForumAuthor) %>' title="<%#: Eval("Author.DisplayName") ?? "" %>">
						<%#: Eval("Author.DisplayName") ?? "" %>
					</a>
					<div class="badges" style="display:block;">
						<div data-badge="true" data-uri="<%# Url.RouteUrl("PortalBadges", new { __portalScopeId__ = Website.Id, userId = Eval("Author.EntityReference.Id"), type = "basic-badges" }) %>"></div>
					</div>
				</td>
				<td class="last-post">
					<div class="media">
						<div class="media-left">
							<asp:HyperLink CssClass="author-link" NavigateUrl='<%# Url.AuthorUrl(Eval("LatestPost.Author") as IForumAuthor) %>' ToolTip='<%# Eval("LatestPost.Author.DisplayName") %>' aria-label='<%#: string.Format(ResourceManager.GetString("User_Avatar_Label"), Eval("LatestPost.Author.DisplayName")) %>' runat="server">
								<asp:Image CssClass="author-img" ImageUrl='<%# Url.UserImageUrl(Eval("LatestPost.Author") as IForumAuthor, 40) %>' AlternateText='<%# Eval("LatestPost.Author.DisplayName") %>' runat="server"/>
							</asp:HyperLink>
						</div>
						<div class="media-body">
							<div class="last-post-info small">
								<asp:HyperLink CssClass="author-link" NavigateUrl='<%#: Url.AuthorUrl(Eval("LatestPost.Author") as IForumAuthor) %>' Text='<%#: Eval("LatestPost.Author.DisplayName") as string ?? "" %>' ToolTip='<%#: Eval("LatestPost.Author.DisplayName") %>' runat="server" />
								<div class="postedon">
									<abbr class="timeago">
										<%#: ForumHelpers.PostedOn(Eval("LatestPost") as IForumPostInfo, "r") %>
									</abbr>
								</div>
								<div class="badges" style="display:block;">
									<div data-badge="true" data-uri="<%# Url.RouteUrl("PortalBadges", new { __portalScopeId__ = Website.Id, userId = Eval("LatestPost.Author.EntityReference.Id"), type = "basic-badges" }) %>"></div>
								</div>
							</div>
							<asp:HyperLink CssClass="last-post-link" NavigateUrl='<%#: Eval("LatestPostUrl") %>' Visible='<%# Eval("LatestPost") != null && Eval("LatestPostUrl") != null %>' ToolTip="<%$ ResourceManager:Last_Post_In_Thread %>" runat="server">
								<span class="fa fa-arrow-circle-o-right" aria-hidden="true"></span>
							</asp:HyperLink>
						</div>
					</div>
				</td>
				<td class="count"><%#: Eval("ReplyCount") %></td>
			</tr>
		</ItemTemplate>
	</asp:ListView>
	
	<asp:Panel ID="ForumThreadCreateForm" CssClass="form-horizontal form-forum-thread html-editors" ViewStateMode="Enabled" runat="server">
		<fieldset id="new">
			<legend>
				<adx:Snippet SnippetName="Forum Thread Create Heading" DefaultText="<%$ ResourceManager:Create_New_Thread_DefaultText %>" EditType="text" runat="server"/>
			</legend>
			<adx:Snippet SnippetName="Forum Thread Create Instructions" EditType="html" runat="server"/>
			<asp:ValidationSummary CssClass="alert alert-danger alert-block" ValidationGroup="NewThread" runat="server" />
			<div class="form-group">
				<asp:Label AssociatedControlID="NewForumThreadName" CssClass="col-sm-2 control-label required" runat="server">
					<%: Html.SnippetLiteral("Forum Thread Create Name Label", ResourceManager.GetString("Thread_Title")) %>
				</asp:Label>
				<div class="col-sm-10">
					<asp:TextBox ID="NewForumThreadName" ValidationGroup="NewThread" MaxLength="95" CssClass="form-control" runat="server" ToolTip="<%$ ResourceManager:Required_Field_Text %>"/>
					<asp:RequiredFieldValidator CssClass="validator" ValidationGroup="NewThread" ControlToValidate="NewForumThreadName" ErrorMessage="<%$ ResourceManager:Thread_Title_Required_Field %>" Text="*" EnableClientScript="False" runat="server"/>
				</div>
			</div>
			<div class="form-group">
				<asp:Label AssociatedControlID="NewForumThreadType" CssClass="col-sm-2 control-label required" runat="server">
					<%: Html.SnippetLiteral("Forum Thread Create Type Label", ResourceManager.GetString("Thread_Type_Label")) %>
				</asp:Label>
				<div class="col-sm-10">
					<asp:ObjectDataSource ID="ForumThreadTypeDataSource" TypeName="Adxstudio.Xrm.Forums.IForumDataAdapter" OnObjectCreating="CreateForumDataAdapter" SelectMethod="SelectThreadTypeListItems" runat="server" />
					<asp:DropDownList DataSourceID="ForumThreadTypeDataSource" ValidationGroup="NewThread" ID="NewForumThreadType" DataTextField="Text" DataValueField="Value" CssClass="form-control" runat="server" ToolTip="<%$ ResourceManager:Required_Field_Text %>"/>
					<asp:RequiredFieldValidator CssClass="validator" ValidationGroup="NewThread" ControlToValidate="NewForumThreadType" ErrorMessage="<%$ ResourceManager:Thread_Type_Required_Field %>" Text="*" EnableClientScript="False" runat="server"/>
				</div>
			</div>
			<div class="form-group">
				<asp:Label AssociatedControlID="NewForumThreadContent" CssClass="col-sm-2 control-label required" runat="server">
					<%: Html.SnippetLiteral("Forum Thread Create Content Label", ResourceManager.GetString("Content_Label")) %>
				</asp:Label>
				<div class="col-sm-10">
					<asp:TextBox ID="NewForumThreadContent" ValidationGroup="NewThread" TextMode="MultiLine" CssClass="form-control" runat="server"/>
					<asp:RequiredFieldValidator CssClass="validator" ValidationGroup="NewThread" ControlToValidate="NewForumThreadContent" ErrorMessage="<%$ ResourceManager:Content_Required_Field_Validation_Message %>" Text="*" EnableClientScript="False" runat="server"/>
					<asp:CustomValidator CssClass="validator" ValidationGroup="NewThread" ControlToValidate="NewForumThreadContent" OnServerValidate="ValidatePostContentLength" ErrorMessage='<%$ Snippet: forums/threads/maxlengthvalidation, Post_Exceeds_Maximum_Length %>' Text="*" runat="server"/>
				</div>
	<script type="text/javascript">
		CKEDITOR.on('instanceCreated', function (event) {
			var editor = event.editor;
			editor.on('instanceReady', function (e) {
				e.editor.container.$.firstChild.innerText = window.ResourceManager['CkEditor_iFrame_Title'];
				$((e.editor.container.$)).find(".cke_inner .cke_contents .cke_wysiwyg_frame").attr("title", window.ResourceManager['CkEditor_iFrame_Title']);
				$((e.editor.container.$)).find(".cke_inner .cke_contents .cke_wysiwyg_frame").contents().find("html").find("head").find("title").attr('data-cke-title', window.ResourceManager['CkEditor_iFrame_Title']).text(window.ResourceManager['CkEditor_iFrame_Title']);
			});
		});
	</script> 
			</div>
			<div class="form-group">
				<asp:Label AssociatedControlID="NewForumThreadAttachment" CssClass="col-sm-2 control-label" runat="server">
					<%: Html.SnippetLiteral("Forum Thread Create File Attachment Label", ResourceManager.GetString("AttachFiles")) %>
				</asp:Label>
				<div class="col-sm-10">
					<div class="form-control-static">
						<asp:FileUpload ID="NewForumThreadAttachment" ValidationGroup="NewThread" AllowMultiple="True" runat="server"/>
						<asp:CustomValidator CssClass="validator" ValidationGroup="NewThread" ControlToValidate="NewForumThreadAttachment" OnServerValidate="ValidateFileUpload" ErrorMessage='<%$ Snippet: forums/threads/filetypevalidation, File_Type_Not_Supported %>' Text="*" runat="server"/>
						<asp:CustomValidator ID="FileUploadSizeValidator" CssClass="validator" ValidationGroup="NewThread" ControlToValidate="NewForumThreadAttachment" OnServerValidate="ValidateFileUploadSize" Text="*" runat="server"/>
					</div>
				</div>
			</div>
			<div class="form-group">
				<div class="col-sm-offset-2 col-sm-10">
					<div class="checkbox">
						<label>
							<asp:CheckBox ID="NewForumThreadSubscribe" Checked="True" runat="server"/>
							<adx:Snippet SnippetName="Forum Thread Create Subscribe Label" DefaultText="<%$ ResourceManager:Subscribe_To_This_Thread %>" EditType="text" runat="server"/>
						</label>
					</div>
				</div>
			</div>
			<div class="form-group">
				<div class="col-sm-offset-2 col-sm-10">
					<asp:Button CssClass="btn btn-primary" OnClick="CreateThread_Click" Text='<%$ Snippet: Forum Thread Create Button Text, Create_This_Thread %>' ValidationGroup="NewThread" runat="server"/>
				</div>
			</div>
		</fieldset>
	</asp:Panel>
	
	<adx:Snippet ID="AnonymousMessage" SnippetName="Forum Post Anonymous Message" EditType="html" runat="server"/>
	
	<script type="text/javascript">
		$(function () {
			$('input[type="submit"]').click(function () {
				$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
			});
		});
	</script>

</asp:Content>
