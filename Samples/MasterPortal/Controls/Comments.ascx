<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Comments.ascx.cs" Inherits="Site.Controls.Comments" %>
<%@ Import Namespace="Adxstudio.Xrm.Cms" %>
<%@ Import Namespace="Site.Helpers" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>
<%@ Register src="~/Controls/CommentCreator.ascx" tagname="CommentCreator" tagprefix="crm" %>


<asp:ObjectDataSource ID="CommentDataSource" TypeName="Adxstudio.Xrm.Cms.ICommentDataAdapter" OnObjectCreating="CreateCommentDataAdapter" SelectMethod="SelectComments" SelectCountMethod="SelectCommentCount" runat="server" />
<asp:ListView ID="CommentsView" DataSourceID="CommentDataSource" runat="server">
	<LayoutTemplate>
		<div class="comments">
			<legend>
				<asp:Literal Text='<%$ Snippet: Comments Heading, Comments %>' runat="server"></asp:Literal>
			</legend>
			<ul class="list-unstyled">
				<li id="itemPlaceholder" runat="server" />
			</ul>
		</div>
	</LayoutTemplate>
	<ItemTemplate>
		<li runat="server">
			<div class="row comment <%# ((bool)Eval("IsApproved")) ? "approved" : "unapproved" %>">
				<div class="col-sm-3 comment-metadata">
					<div class="comment-author">
						<asp:HyperLink rel="nofollow" NavigateUrl="<%#: Url.AuthorUrl(Container.DataItem as IComment) %>" Text='<%#: Eval("Author.DisplayName") ?? "" %>' runat="server"></asp:HyperLink>
					</div>
					<abbr class="timeago"><%#: Eval("Date", "{0:r}") %></abbr>
					<asp:Label Visible='<%# !(bool)Eval("IsApproved") %>' CssClass="label label-info" Text='<%$ Snippet: Unapproved Comment Label, Unapproved_Comment_Label %>' runat="server"></asp:Label>
				</div>
				<div class="col-sm-9">
					<div class="comment-controls">
						<asp:Panel Visible='<%# ((bool?)Eval("Editable")).GetValueOrDefault() %>' CssClass='<%# Eval("Entity.LogicalName", "xrm-entity xrm-editable-{0}")%>' runat="server">
							<div class="btn-group">
								<a class="btn btn-default xrm-edit"><span class="fa fa-cog" aria-hidden="true"></span></a>
								<a class="btn btn-default dropdown-toggle" data-toggle="dropdown">
									<span class="caret"></span>
								</a>
								<ul class="dropdown-menu">
									<li>
										<a href="#" class="xrm-edit" title="<%= ResourceManager.GetString("Edit_Label") %>"><span class="fa fa-edit" aria-hidden="true"></span><%= ResourceManager.GetString("Edit_Label") %></a>
									</li>
									<li>
										<a href="#" class="xrm-delete" title="<%= ResourceManager.GetString("Editable_Delete_Label") %>"><span class="fa fa-trash-o" aria-hidden="true"></span><%= ResourceManager.GetString("Editable_Delete_Label") %></a>
									</li>
								</ul>
							</div>
							<asp:HyperLink NavigateUrl='<%#: Eval("EditPath.AbsolutePath") %>' CssClass="xrm-entity-ref" style="display:none;" runat="server"/>
							<asp:HyperLink NavigateUrl='<%#: Eval("DeletePath.AbsolutePath") %>' CssClass="xrm-entity-delete-ref" style="display:none;" runat="server"/>
						</asp:Panel>
					</div>
					<asp:Panel Visible="<%# EnableRatings %>" CssClass="rating" runat="server">
						<asp:Literal Text='<%# Html.Rating(Eval("Entity") != null ? ((Entity)Eval("Entity")).ToEntityReference() : null, Eval("RatingInfo") != null ? (IRatingInfo)Eval("RatingInfo") : null) %>' runat="server"></asp:Literal>
					</asp:Panel>
					<div><%# Eval("Content") %></div>
				</div>
			</div>
		</li>
	</ItemTemplate>
</asp:ListView>

<asp:Panel ID="NewCommentPanel" runat="server">
	<crm:CommentCreator ID="NewCommentCreator" runat="server" />
</asp:Panel>
