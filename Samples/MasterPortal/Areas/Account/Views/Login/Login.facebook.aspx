<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %> 
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<!DOCTYPE html>
<html lang="<%: Html.Setting("Html/LanguageCode", "en") %>">
	<head runat="server">
		<meta charset="utf-8" />
		<meta name="viewport" content="width=device-width, initial-scale=1.0" />
		<meta http-equiv="X-UA-Compatible" content="IE=edge" />
		<title><%: Html.AttributeLiteral("adx_title") ?? Html.AttributeLiteral("adx_name") %><%= Html.SnippetLiteral("Browser Title Suffix") %></title>
		<%= Html.SnippetLiteral("Head/Fonts") %>
		<%: Html.ContentStyles(only: new Dictionary<string, string>
			{
				{"bootstrap.min.css", Url.Content("~/css/bootstrap.min.css")}
			}) %>
		<%: Styles.Render("~/css/default.bundle.css") %>
		<!-- HTML5 shim, for IE6-8 support of HTML elements -->
		<!--[if lt IE 9]>
			<script src="//html5shim.googlecode.com/svn/trunk/html5.js"></script>
		<![endif]-->
		<% Html.RenderPartialFromSetting("Head/Template"); %>
		<%: Html.ContentStyles(except: new [] { "bootstrap.min.css" }) %>
	</head>
	<body>
		<% if (Request.IsAuthenticated && !string.IsNullOrWhiteSpace((string)ViewBag.ReturnUrl)) { %>
			<script type="text/javascript">
				window.location.href = "<%: (string)ViewBag.ReturnUrl %>";
			</script>
		<% } %>
		<% Html.RenderPartialFromSetting("Header/Template", "HeaderNavbar"); %>
		<%: Scripts.Render("~/js/default.preform.bundle.js") %>
		<div class="container">
			<%: Html.HtmlSnippet("Facebook/SignIn/Prompt", defaultValue: @"<div class=""alert alert-block alert-info""><p>You must be signed in into Facebook to use this application.</p></div>") %>
		</div>
		<% Html.RenderPartialFromSetting("Footer/Template", "FooterMenuWithCopy"); %>
		<%: Html.EntityEditingMetadata() %>
		<%: Html.EditingStyles(new []
			{
				"~/xrm-adx/css/yui-skin-sam-2.9.0/skin.css"
			}) %>
		<%: Html.EditingScripts(dependencyScriptPaths: new []
			{
				"~/xrm-adx/js/yui-2.9.0-combo.min.js",
				"~/xrm-adx/js/jquery-ui-1.10.0.min.js",
			}, extensionScriptPaths: new string[] {}) %>
		<%: LocalizedScripts.Render("~/js/default.bundle.js") %>
		<%= Html.SnippetLiteral("Tracking Code") %>
	</body>
</html>
<!-- Generated at <%: DateTime.UtcNow %> -->
<!-- Page OK -->
