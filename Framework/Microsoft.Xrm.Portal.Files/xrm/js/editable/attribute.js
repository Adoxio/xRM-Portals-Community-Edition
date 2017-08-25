/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {
	
	var ns = XRM.namespace('editable.Attribute');
	var $ = XRM.jQuery;
	
	ns.attributeServiceUriParseRegexp = /^(.+)\/(.+)$/;
	
	if (!ns.handlers) {
		ns.handlers = {};
	}
	
	ns.initialize = function(toolbar) {
		$('.xrm-attribute').each(function() {
			var attributeContainer = $(this);
			var attributeServiceRef = $('a.xrm-attribute-ref', attributeContainer);
			var attributeServiceUri = attributeServiceRef.attr('href');
			var attributeDisplayName = attributeServiceRef.attr('title');

			// If there's no service URI for the attribute, quit.
			if (!attributeServiceUri) {
				return;
			}

			// Apply a special class to empty attributes, so we can make them visible/hoverable, through CSS.
			ns.addClassOnEmptyValue(attributeContainer);

			attributeContainer.editable(attributeServiceUri, {
				loadSuccess: function(attributeData) {
					ns.enterEditMode(attributeContainer, attributeServiceUri, attributeData, attributeDisplayName);
				}
			});
		});
	}
	
	ns.enterEditMode = function(attributeContainer, attributeServiceUri, attributeData, attributeDisplayName) {
		// If we have no valid attribute data, quit.
		if (!(attributeData && attributeData.d)) {
			return;
		}
		
		// For example, this would split the attribute service URI
		// "/Cms.svc/WebPages(guid'8714a5bd-0dfd-dd11-bdf3-0003ff48c0db')/Copy" into
		// ["", "/Cms.svc/WebPages(guid'8714a5bd-0dfd-dd11-bdf3-0003ff48c0db')", "Copy", ""].
		// [1] and [2] are the pieces of information we want (the empty strings are just junk).
		var uriSegments = ns.attributeServiceUriParseRegexp.exec(attributeServiceUri)

		// If we fail to extract the service URI info we need, quit.
		if (!uriSegments || (uriSegments.length < 3)) {
			return;
		}

		var entityServiceUri = uriSegments[1];
		var attributeName = uriSegments[2];

		// If we fail to extract the service URI info we need, quit.
		if (!(entityServiceUri && attributeName)) {
			return;
		}

		// For example, for the class "xrm-attribute xrm-editable-html foo", this would capture
		// ["xrm-editable-html", "html"]. We want [1] in this case.
		var captures = XRM.editable.editableClassRegexp.exec(attributeContainer.attr('class'));

		// If we fail to extract the editable type identifier we want, quit.
		if (!captures || (captures.length < 2)) {
			return;
		}

		var editableTypeHandler = ns.handlers[captures[1]];

		// If our editable type identifier doesn't correspond to an actual handler function, quit.
		if (!$.isFunction(editableTypeHandler)) {
			return;
		}

		var attributeValue = attributeData.d[attributeName];

		editableTypeHandler(attributeContainer, attributeDisplayName, attributeName, attributeValue, entityServiceUri, function() {
			ns.addClassOnEmptyValue(attributeContainer);
		});
	}

	ns.addClassOnEmptyValue = function(attributeContainer) {
		var attributeValue = $('.xrm-attribute-value', attributeContainer);

		if (attributeValue.html() == '') {
			attributeValue.addClass('xrm-attribute-value-empty');
		}
		else {
			attributeValue.removeClass('xrm-attribute-value-empty');
		}
	}
	
});
