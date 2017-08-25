<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<div class="content-panel panel panel-default">
	<div class="panel-heading">
		<h4>
			<span class="fa fa-search" aria-hidden="true"></span>
			<%: Html.TextSnippet("Idea Search Heading", defaultValue: ResourceManager.GetString("Search_Issues_Defaulttext"), tagName: "span") %>
		</h4>
	</div>
	<div class="panel-body">
		<% using (Html.BeginForm("search", "Issue", FormMethod.Get, new { id = "issues-search-form", @class = "form-search" })) { %>
			<div class="input-group">
				<%= Html.TextBox("q", string.Empty, new { @class = "form-control", placeholder = Html.SnippetLiteral("Issue Search Heading", ResourceManager.GetString("Search_Issues_Defaulttext")) })%>
				<div class="input-group-btn">
					<button type="submit" class="btn btn-default" aria-label="<%: Html.SnippetLiteral("Issue Search Heading", ResourceManager.GetString("Search_Issues_Defaulttext")) %>"><span class="fa fa-search" aria-hidden="true"></span></button>
				</div>
			</div>
		<% } %>
		<script type="text/javascript">
			$(function () {
				$("#issues-search-form").submit(function () {
					if ($("#issues-search-form #q").val().trim().length) {
						return true;
					}
					return false;
				});
			});
		</script>
	</div>
</div>
