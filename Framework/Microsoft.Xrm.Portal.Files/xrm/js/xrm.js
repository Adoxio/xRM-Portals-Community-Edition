/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


if (typeof XRM == "undefined" || !XRM) {
	var XRM = {};
}

XRM.namespace = function() {
	var a = arguments, o = null, i, j, d;
	for (i = 0; i < a.length; i = i + 1) {
		d = ("" + a[i]).split(".");
		o = XRM;

		// XRM is implied, so it is ignored if it is included.
		for (j = (d[0] == "XRM") ? 1 : 0; j < d.length; j = j + 1) {
			o[d[j]] = o[d[j]] || {};
			o = o[d[j]];
		}
	}

	return o;
};

XRM.log = function(msg, cat, src) {
	try {
		if ((typeof(YAHOO) != 'undefined') && YAHOO && YAHOO.widget && YAHOO.widget.Logger && YAHOO.widget.Logger.log) {
			return YAHOO.widget.Logger.log(msg, cat, src);
		}
		
		if ((typeof(console) == 'undefined') || !console) {
			return false;
		}
		
		var source = src ? ('[' + src + ']') : '';
		
		if (cat == 'warn' && console.warn) {
			console.warn(msg, source);
		}
		else if (cat == 'error' && console.error) {
			console.error(msg, source);
		}
		else if (cat == 'info' && console.info) {
			console.info(msg, source);
		}
		else if (console.log) {
			console.log(msg, source);
		}
		
		return true;
	}
	catch (e) {
		return false;
	}
};

(function() {

	var activations = [];
	var activated = false;

	XRM.onActivate = function(fn) {
		if (activated) {
			fn();
		}
		else {
			activations.push(fn);
		}
	}

	XRM.activate = function(jquery) {
		XRM.jQuery = jquery;
		
		for (var i = 0; i < activations.length; i++) {
			activations[i]();
		}
		
		activated = true;
	}

})();
