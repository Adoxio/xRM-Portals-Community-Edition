<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<script type="text/javascript">
	$.getDocHeight = function () {
		return Math.max(
			$(document).height(),
			$(window).height(),
			/* For opera: */
			document.documentElement.clientHeight
		);
	};
	function onload() {
		if (!JSON || !parent) return;
		var data = {
			msg: 'resize',
			height: $.getDocHeight()
		};
		var message = JSON.stringify(data);
		parent.postMessage(message, '*');
	};
	/* Usage:
		//parent page can attach and event listener to intercept the postMessage and resize the iframe
		function listener(event) {
			if (!event.data || event.data == '') return;
			var data = JSON.parse(event.data);
			if (data.msg != 'resize' && !data.height && !isNaN(data.height)) return;
			$('#myiframe').css('height', data.height + 'px');
		}
		if (window.addEventListener) {
			addEventListener("message", listener, false);
		} else {
			attachEvent("onmessage", listener);
		}
	*/
	$(document).ready(function () {
		onload();
	});
</script>