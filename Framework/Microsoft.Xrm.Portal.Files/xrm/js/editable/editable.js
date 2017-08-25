/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {

	var ns = XRM.namespace('editable');
	var yuiSkinClass = XRM.yuiSkinClass;
	var editableClassRegexp = XRM.editable.editableClassRegexp;
	var Attribute = XRM.editable.Attribute;
	var Entity = XRM.editable.Entity;
	var $ = XRM.jQuery;
	
	$(document).ready(function() {
		XRM.editable.initialize();
	});
	
	ns.initialize = function() {
		var toolbar = ns.toolbar = ns.createToolbar();
		
		ns.initializeToolbar(toolbar);

		Attribute.initialize(toolbar);
		Entity.initialize(toolbar);

		// Only show the global edit toolbar if it contains any functionality.
		if ($('.xrm-editable-toolbar-module', toolbar.body).length > 0) {
			toolbar.show();
			XRM.ui.registerOverlay(toolbar);
			toolbar.focus();
			$('.container-close', toolbar.element).blur();
		}
	}
	
	ns.initializeToolbar = function(toolbar) {}

	ns.createToolbar = function() {
		var yuiContainer = $('<div />').addClass(yuiSkinClass).appendTo(document.body);
		var toolbarContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-toolbar').appendTo(yuiContainer);

		// Create a draggable control panel, that is anchored to the top right (tr) of the viewport.
		var toolbar = new YAHOO.widget.Panel(toolbarContainer.get(0), {
			close: true,
			draggable: true,
			constraintoviewport: true,
			visible: false,
			context: [document.body, 'tr', 'tr', ['windowResize']],
			zindex: XRM.zindex
		});

		// If this toolbar gets closed, remove the editable controls on XRM entities and attributes.
		toolbar.subscribe('hide', function() {
			$('.xrm-attribute, .xrm-entity').noneditable();
			$('.xrm-attribute-value-empty').removeClass('xrm-attribute-value-empty');
		});

		// YUI doesn't create a body element unless we do this.
		toolbar.setBody(' ');
		toolbar.render();

		return toolbar;
	}
	
});
