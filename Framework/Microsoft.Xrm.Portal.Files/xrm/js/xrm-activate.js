/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


(function() {

	// This code will validate script dependencies, and only activate XRM inline editing
	// if all dependency checks are satisfied.
	
	var logName = 'XRM.activator';
	
	if (typeof jQuery == "undefined" || !jQuery) {
		XRM.log('jQuery (1.4.2) is required by this libary, and has not been loaded. XRM features will not be activated.', 'warn', logName);
		return;
	}
	
	if (jQuery.fn.jquery !== '1.4.2') {
		XRM.log('This library has been tested with jQuery 1.4.2. You have loaded jQuery ' + jQuery.fn.jquery + '. Features of this libary may not work as designed.', 'warn', logName);
	}
	
	if (typeof YAHOO == "undefined" || !YAHOO) {
		XRM.log('YUI (2.7.0-2.8.0) is required by this libary, and has not been loaded.  XRM features will not be activated.', 'warn', logName);
		return;
	}
	
	XRM.activate(jQuery);
	
})();
