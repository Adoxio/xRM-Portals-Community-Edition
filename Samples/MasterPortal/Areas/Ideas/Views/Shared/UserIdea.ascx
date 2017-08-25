<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Adxstudio.Xrm.Ideas.IIdea>" %>

<h3><%: Html.ActionLink(Model.Title, "Ideas", "Ideas", new { ideaForumPartialUrl = Model.IdeaForumPartialUrl, ideaPartialUrl = Model.PartialUrl }, new {title=Model.Title }) %></h3>
<p class="user-idea-details">
	<% Html.RenderPartial("IdeaStatus", Model); %>
	&middot; <small> From: </small>
	<%: Html.ActionLink(Model.IdeaForumTitle, "Ideas", "Ideas", new { ideaForumPartialUrl = Model.IdeaForumPartialUrl }, new {title=Model.IdeaForumTitle }) %>
</p>
