<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<div class="footer">
	<div class="container">
		<%: Html.HtmlSnippet("Facebook/Footer") %>
	</div>
</div>
<div id="fb-root"></div>
<script src="//connect.facebook.net/en_US/all.js"></script>
<script>
	FB.init({
		appId: '<%: Html.Setting("Authentication/OpenAuth/Facebook/AppId") %>',
		status: true, // check login status
		cookie: true, // enable cookies to allow the server to access the session
		xfbml: true // parse XFBML
	});

	(function ($) {

		function setSize() {
			FB.Canvas.setSize({ height: $('body').height() });
		}

		setSize();
		setInterval(setSize, 100);

	})(window.jQuery);
</script>
<style type="text/css">
	html,
	body {
		overflow: hidden;
	}
</style>
