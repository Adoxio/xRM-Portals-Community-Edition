/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function() {

	function bustFrame() {
		if (top != self) {
			top.location.replace(self.location.href);
		}
	}

	window.onload = bustFrame;

})();
