<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Site.Areas.Issues.ViewModels.IssueViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ Import Namespace="Site.Helpers" %>

<% if (!Model.CurrentUserHasAlert) { %>
    <a class="btn btn-default btn-lg btn-block" onclick="issueTracking('<%= Url.Action("AlertCreate", "Issue", new { id = Model.Issue.Id }) %>')"><span class="fa fa-eye" aria-hidden="true"></span>   <%= Html.SnippetLiteral("Issue Alert Create Label", ResourceManager.GetString("Track"))%></a>
<% } else { %>
    <a class="btn btn-danger btn-lg btn-block" onclick="issueTracking('<%= Url.Action("AlertRemove", "Issue", new { id = Model.Issue.Id }) %>')"><span class="fa fa-eye-slash" aria-hidden="true"></span>   <%= Html.SnippetLiteral("Issue Alert Remove Label", ResourceManager.GetString("Stop_Tracking"))%></a>
<% } %>
<script type="text/javascript">
	$(function () {
		$("div#issue-tracking > a").click(function () {
			$(this).block({ message: null, overlayCSS: { opacity: .3} });
		});
	});

	function issueTracking(url) {
	    shell.ajaxSafePost({
	        type: 'POST',
	        url: url,
	        success: function (result) {
	            $("#issue-tracking").html(result);
	        }
	    });
	}
</script>
