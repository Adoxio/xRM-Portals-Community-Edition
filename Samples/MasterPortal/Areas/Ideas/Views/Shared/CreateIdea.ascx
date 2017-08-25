<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Adxstudio.Xrm.Ideas.IIdeaForum>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<% using (Html.BeginForm("Create", "Idea", new { id = Model.Id }, FormMethod.Post, new{id="create-idea-id", @class = "form-horizontal html-editors"})) { %>
	<fieldset>
		<legend><%: Html.SnippetLiteral("Idea New Suggestion Label", ResourceManager.GetString("Suggest_A_New_Idea"))%></legend>
		<%= Html.ValidationSummary(string.Empty, new {@class = "alert alert-block alert-danger"})%>
		<% if (!Request.IsAuthenticated) { %>
			<div class="form-group">
				<label class="col-sm-3 control-label required" for="authorName">Your Name</label>
				<div class="col-sm-9">
					<%= Html.TextBox("authorName", string.Empty, new { @class = "form-control" })%>
				</div>
			</div>
			<div class="form-group">
				<label class="col-sm-3 control-label required" for="authorEmail">E-mail</label>
				<div class="col-sm-9">
					<%= Html.TextBox("authorEmail", string.Empty, new { @class = "form-control" })%>
				</div>
			</div>
		<% } %>
		<div class="form-group">
			<label class="col-sm-3 control-label required" for="title"><%: ResourceManager.GetString("Idea_Label") %></label>
			<div class="col-sm-9">
				<%= Html.TextBox("title", string.Empty, new { @class = "form-control" })%>
			</div>
		</div>
		<div class="form-group">
			<label class="col-sm-3 control-label" for="title"><%: ResourceManager.GetString("Description") %></label>
			<div class="col-sm-9">
				<%= Html.TextArea("copy", string.Empty, new { @class = "form-control" })%>
			</div>
		</div>
		<div class="form-group">
			<div class="col-sm-offset-3 col-sm-9">
				<input id="submit-idea" class="btn btn-primary" type="submit" value="<%: Html.SnippetLiteral("Idea Submit Label", ResourceManager.GetString("Submit_Idea"))%>" />
			</div>
		</div>
	</fieldset>
<% } %>
<script type="text/javascript">
	$(function() {
		$("#submit-idea").click(function() {
		    $.blockUI({ message: null, overlayCSS: { opacity: .3 } });
		    shell.ajaxSafePost({
		        type: "POST",
		        success: function (result) {
		            $("#create-idea").html(result);
		            ideaCreated();
		        }
		    }, $("#create-idea-id"));
		    return false;
		});
	});

	function ideaCreated() {
		if ($("#create-idea .validation-summary-errors").length) {
			portal.initializeHtmlEditors();
			prettyPrint();
			$.unblockUI();
			return;
		}
		window.location.href = '<%= Url.RouteUrl("IdeasFilter", new { ideaForumPartialUrl = Model.PartialUrl, filter = "new" }) %>';
	}
</script>
