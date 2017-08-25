/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function() {
	var orgSvcUrlPathPattern = /\/XRMServices\/2011\/Organization.svc.*/i;
	var aspxPathPattern = /\/[\w]+\.aspx.*/i;

	var selectorPanel = $();
	var helpBlock = $();
	var connectionButton = $();
	var submitButton = $();

	function isUrl(url) {
		var pattern = /(https?:\/\/([-\w\.]+)+(:\d+)?(\/([-A-Za-z0-9_\\$\\.\\+\\!\\*\\(\\),;:@&=\\?\/~\\#\\%]*(\?\S+)?)?)?)/;
		return (url || "").match(pattern);
	}

	function orgSvcUrlChange() {
		var $orgSvcUrl = $("#OrganizationServiceUrl");
		var url = $orgSvcUrl.val();
		var chopped = (url || "").replace(orgSvcUrlPathPattern, "").replace(aspxPathPattern, "");

		if (isUrl(chopped)) {
			$orgSvcUrl.val(chopped);
			var headers = {};
			headers['__RequestVerificationToken'] = $("input[name='__RequestVerificationToken']").val();;
			var dataUrl = $orgSvcUrl.attr("data-url");
			if (dataUrl) {
				hideError();
				$.ajax({
					type: "POST",
					url: dataUrl,
					data: { url: chopped },
					success: null,
					headers: headers,
					dataType: "json"
				}).fail(onOrgSvcUrlFailed).then(onOrgSvcUrlDone).then(reset).always(onEndAjax);
			}
		} else {
			reset();
		}
	}

	function authenticationTypeToPlaceholder(authenticationType) {
		if (authenticationType == "ActiveDirectory") return "contoso\\username";
		if (authenticationType == "Federation") return "username@contoso.com";
		if (authenticationType == "LiveId") return "username@live.com";
		if (authenticationType == "OnlineFederation") return "username@contoso.onmicrosoft.com";
		return null;
	}

	function onOrgSvcUrlDone(data, textStatus, jqXHR) {
		console.log(data);

		if (data) {
			var placeholder = authenticationTypeToPlaceholder(data.authenticationType);
			$("#Username").attr("placeholder", placeholder);
			$("#Website").empty();
		} else {
			reset();
		}
	}

	function onOrgSvcUrlFailed(jqXHR, textStatus, errorThrown) {
		onFailed(jqXHR);
	}

	function credentialChange() {
		var orgSvcUrl = $("#OrganizationServiceUrl").val();
		var $username = $("#Username");
		var $password = $("#Password");
		var username = $username.val();
		var password = $password.val();
		var antiForgeryToken = $("input[name='__RequestVerificationToken']").val();
		var headers = {};
		headers['__RequestVerificationToken'] = antiForgeryToken;
		if (orgSvcUrl && username && password) {
			var dataUrl = $password.attr("data-url");
			if (dataUrl) {
				onBeginAjax();
				$.ajax({
					type: "POST",
					url: dataUrl,
					data: { url: orgSvcUrl, username: username, password: password },
					success: null,
					headers: headers,
					dataType: "json"
				}).fail(onCredentialFailed).then(onCredentialDone).always(onEndAjax);
			}
		} else {
			reset();
		}
	}

	function onCredentialDone(data, textStatus, jqXHR) {
		console.log(data);

		if (data) {
			helpBlock.hide();
			$("#Website").val(null);
			$("#Website").empty();
			for (var key in data) {
				var website = data[key];
				var option = $("<option/>").attr("value", website.Id).text(website.Name).prop("selected", website.Binding);
				$("#Website").append(option);
				if (website.Binding) {
					$("#Website").prop("disabled", true);
					helpBlock.show();
				}
			}
			selectorPanel.show();
			submitButton.show();
			connectionButton.hide();
		} else {
			reset();
		}

		return data;
	}

	function onCredentialFailed(jqXHR, textStatus, errorThrown) {
		onFailed(jqXHR);
	}

	function onFailed(jqXHR) {
		var contentType = jqXHR.getResponseHeader("content-type");
		var error = contentType.indexOf('json') > -1
			? $.parseJSON(jqXHR.responseText)
			: { errorMessage: "[" + jqXHR.status + ": " + jqXHR.statusText + "] An error was encountered. Refresh this page and try again." };

		if (error) {
			console.log(error);

			if (error.key) {
				$("#" + error.key).parents(".form-group").addClass("has-error");
			}

			if (error.key == "Password") {
				$("#Username").parents(".form-group").addClass("has-error");
			}

			if (error.errorMessage) {
				if (error.key) {
					$("#errors-list").children("[data-key='" + error.key + "']").remove();
				}
				$("#errors-list").append($("<li/>").attr("data-key", error.key).text(error.errorMessage));
				$("#errors-list").parents(".alert").removeClass('hide');
			}
		}
		reset();
	}

	function onBeginAjax() {
		hideError();
		connectionButton.prop("disabled", true);
		$(".fa-spin").removeClass("hidden");
	}

	function onEndAjax() {
		connectionButton.prop("disabled", false);
		$(".fa-spin").addClass("hidden");
	}

	function hideError() {
		$("#OrganizationServiceUrl").closest(".form-group").removeClass("has-error");
		$("#Username").closest(".form-group").removeClass("has-error");
		$("#Password").closest(".form-group").removeClass("has-error");
		$("#Website").closest(".form-group").removeClass("has-error");

		$("#errors-list").closest(".alert").addClass('hide');
		$("#errors-list").empty();
	}

	function reset() {
		selectorPanel.hide();
		helpBlock.hide();
		connectionButton.show();
		submitButton.hide();
	}

	function submit(e) {
		onBeginAjax();
		$(this).find(".apply-label").text("Please wait...");
	}

	function init() {
		selectorPanel = $("#Website").closest(".form-group");
		helpBlock = selectorPanel.find(".help-block");
		connectionButton = $("#checkConnection");
		submitButton = $("#submit");
		reset();

		$("#OrganizationServiceUrl").change(orgSvcUrlChange);
		$("#Username").change(reset);
		$("#Password").change(reset);
		connectionButton.on("click", credentialChange);
		submitButton.on("click", submit);

		$(".service-url-popover-link").popover({
			html: true,
			trigger: "focus",
			placement: "top",
			content: function () {
				return $(".service-url-popover-content").html();
			}
		});
	}

	$(init);
}());
