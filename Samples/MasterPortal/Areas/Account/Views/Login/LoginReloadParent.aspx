<%@ Page Title="Reloading parent window..." Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<script type="text/javascript">
	window.opener.location.href = window.opener.location.href;

	if (window.opener.progressWindow) {
		window.opener.progressWindow.close();
	}

	window.close();
</script>
