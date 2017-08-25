/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {

	var ns = XRM.namespace('ui');
	var yuiSkinClass = XRM.yuiSkinClass;
	var overlayManager = new YAHOO.widget.OverlayManager();
	var JSON = YAHOO.lang.JSON;
	var $ = XRM.jQuery;
	
	ns.registerOverlay = function(overlay) {
		overlayManager.register(overlay);
	}
	
	ns.showProgressPanel = function(message, options) {
		// Create a root container so we can apply our YUI skin class.
		var yuiContainer = $('<div />').addClass(yuiSkinClass).appendTo(document.body);

		// Create the actual container element for the panel, with our desired classes.
		var panelContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog xrm-editable-wait').appendTo(yuiContainer);

		var panel = new YAHOO.widget.Panel(panelContainer.get(0), options);

		panel.setHeader(message);
		panel.setBody(' ');
		panel.render();
		panel.show();

		ns.registerOverlay(panel);
		panel.focus();

		// Return a funtion that will cleanup/remove the panel.
		return function() {
			panel.hide();
			yuiContainer.remove();
		}
	}

	ns.showProgressPanelAtXY = function(message, xy) {
		return ns.showProgressPanel(message, {
			close: false,
			draggable: false,
			zindex: XRM.zindex,
			visible: false,
			xy: xy,
			constraintoviewport: true
		});
	}

	ns.showModalProgressPanel = function(message) {
		var body = $(document.body);

		var hadClass = body.hasClass(yuiSkinClass);

		if (!hadClass) {
			body.addClass(yuiSkinClass);
		}

		var hideProgressPanel = ns.showProgressPanel(message, {
			close: false,
			draggable: false,
			zindex: XRM.zindex,
			visible: false,
			fixedcenter: true,
			modal: true
		});

		return function() {
			hideProgressPanel();

			if (!hadClass) {
				body.removeClass(yuiSkinClass);
			}
		}
	}
	
	ns.showError = function(message) {
		alert(message);
	}

	ns.showDataServiceError = function(xhr, message) {
		var xhrs = $.isArray(xhr) ? xhr : [xhr];
		message = message || XRM.localize('error.dataservice.default');

		$.each(xhrs, function(i, item) {
			// Try get an ADO.NET DataServicesException message out of the response JSON, and
			// append it to the error message.
			try {
				var errorJSON = JSON.parse(item.responseText);

				if (errorJSON && errorJSON.error && errorJSON.error.message && errorJSON.error.message.value) {
					message += '\n\n' + errorJSON.error.message.value;
				}
			}
			catch (e) { }
		});

		ns.showError(message);
	}
	
	ns.getEditLabel = function(containerElement) {
		return XRM.localize('editable.label');
	}
	
	ns.getEditTooltip = function(containerElement) {
		return XRM.localize('editable.tooltip')
	}
	
	$.fn.extend({
		editable: function(serviceUri, options) {
			var options = options || {};
			var container = this;
			var containerElement = container.get(0);

			var yuiContainer = $('<span />').addClass(yuiSkinClass).appendTo(container);
			var panelContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-controls').appendTo(yuiContainer);

			var editPanel = new YAHOO.widget.Panel(panelContainer.get(0), {
				close: false,
				draggable: false,
				constraintoviewport: true,
				visible: false,
				zindex: XRM.zindex
			});

			editPanel.setBody('<a class="xrm-editable-edit"></a>');
			editPanel.render();
			
			$('.xrm-editable-edit', editPanel.body).attr('title', XRM.ui.getEditTooltip(containerElement)).text(XRM.ui.getEditLabel(containerElement)).click(function() {
				var hideProgressPanel = XRM.ui.showProgressPanelAtXY(XRM.localize('editable.loading'), YAHOO.util.Dom.getXY(containerElement));

				// Retrieve the latest value data for the attribute, as JSON, and enter edit mode
				// when (if) it returns successfully.
				XRM.data.getJSON(serviceUri, {
					success: function(data, textStatus) {
						hideProgressPanel();

						if ($.isFunction(options.loadSuccess)) {
							options.loadSuccess(data);
						}
					},
					error: function(xhr) {
						hideProgressPanel();

						if ($.isFunction(options.loadError)) {
							options.loadError(xhr);
						}
						else {
							XRM.ui.showDataServiceError(xhr);
						}
					}
				});
			});
			
			var timeoutID;

			container.hover(
				function() {
					if (timeoutID) {
						clearTimeout(timeoutID);
					}
					container.addClass('xrm-editable-hover');
					editPanel.cfg.setProperty('xy', YAHOO.util.Dom.getXY(containerElement));
					editPanel.show();
				},
				function() {
					timeoutID = setTimeout(function() {
						editPanel.hide();
						container.removeClass('xrm-editable-hover');
					}, 800);
				}
			);
		},

		noneditable: function() {
			this.unbind('mouseenter').unbind('mouseleave');
		}
	});
	
});
