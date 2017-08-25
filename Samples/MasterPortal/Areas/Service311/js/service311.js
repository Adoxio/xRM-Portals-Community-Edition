/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

if (typeof ADX == "undefined" || !ADX) {
	var ADX = {};
}
ADX.service311 = (function() {
	var _export = {};

	function checkStatus(inputId, url, key) {
		if (!inputId || !url || !key) {
			return;
		}
		var trackingnumber = $("#" + inputId).val();
		if (!trackingnumber || trackingnumber == '') {
			return;
		}
		var location = url + "?" + key + "=" + trackingnumber;
		window.location.assign(location);
	}

	function disableEnterKey(e) {
		var key;
		if (window.event) {
			key = window.event.keyCode //IE
		} else {
			key = e.which;
		}
		return (key != 13);
	}

	function isMobile() {
		var _isMobile = (/iphone|ipod|ipad|android|ie|iemobile|blackberry|webos/).test
			 (navigator.userAgent.toLowerCase());
		return _isMobile;
	}

	_export.checkStatus = checkStatus;

	_export.disableEnterKey = disableEnterKey;

	_export.isMobile = isMobile;

	return _export;

})();
