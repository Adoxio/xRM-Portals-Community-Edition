/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

var radcaptcha = {
	onClientLoad: function (o, e) {
		// when the captcha is loaded, refresh the image once because sometimes 
		// it'll have an invalidated img url because of caching

		// retrieve the "Generate New Image" button and click it to get a new image
		var genNewImageBtn = o._element.getElementsByTagName("a");
		if (genNewImageBtn && genNewImageBtn.length > 0) {
			document.location = genNewImageBtn[0].href;
		}
	}
}
