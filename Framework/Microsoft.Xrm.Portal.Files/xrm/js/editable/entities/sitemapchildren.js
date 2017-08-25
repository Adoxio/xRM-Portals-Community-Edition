/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {

	var ns = XRM.namespace('editable.Entity.handlers');
	var Entity = XRM.editable.Entity;
	var $ = XRM.jQuery;
	var yuiSkinClass = XRM.yuiSkinClass;
	var JSON = YAHOO.lang.JSON;
	
	ns["sitemapchildren"] = function(entityContainer, editToolbar) {
		var entityServiceRef = $('a.xrm-entity-ref-sitemapchildren', entityContainer);
		var entityServiceUri = entityServiceRef.attr('href');
		var entityTitle = entityServiceRef.attr('title');

		if (!entityServiceUri) {
			return XRM.log('Unable to get site map children service URI. Child view will not be editable.', 'warn', 'editable.Entity.handlers.sitemapchildren');
		}
		
		entityContainer.editable(entityServiceUri, {
			loadSuccess: function(entityData) {
				renderEditDialog(entityContainer, entityServiceUri, entityTitle, entityData);
			}
		});
	}
	
	function renderEditDialog(entityContainer, entityServiceUri, entityTitle, entityData) {
		var yuiContainer = $('<div />').addClass(yuiSkinClass).appendTo(document.body);
		var dialogContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog xrm-editable-dialog-sitemapchildren').appendTo(yuiContainer);

		var childData = $.isArray(entityData.d) ? entityData.d : [];

		function completeEdit(dialog) {
			dialog.cancel();
			yuiContainer.remove();
		}
		
		var list = $('<ol />').addClass('xrm-editable-sitemapchildren');
		
		var dialog = new YAHOO.widget.Dialog(dialogContainer.get(0), {
			visible: false,
			constraintoviewport: true,
			zindex: XRM.zindex,
			xy: YAHOO.util.Dom.getXY(entityContainer.get(0)),
			buttons: [
				{
					text: XRM.localize('editable.save.label'),
					handler: function() {
						var dialog = this;
						var hideProgressPanel = XRM.ui.showProgressPanelAtXY(XRM.localize('editable.saving'), YAHOO.util.Dom.getXY(dialog.id));
						
						saveChildren(childData, list, function(successfulUpdates, failedUpdates) {
							hideProgressPanel();
							completeEdit(dialog);
							document.location = document.location;
						});
					},
					isDefault: true
				},
				{
					text: XRM.localize('editable.cancel.label'),
					handler: function() { completeEdit(this); }
				}
			]
		});

		dialog.setHeader(XRM.localize('sitemapchildren.header.label') + (entityTitle ? (' (' + entityTitle + ')') : ''));
		dialog.setBody(' ');

		list.appendTo(dialog.body);
		
		$.each(childData, function(_, child) {
			var item = $('<li />').addClass('xrm-editable-sitemapchild').appendTo(list);
			$('<a />').attr('title', XRM.localize('editable.sortable.tooltip')).addClass('xrm-drag-handle').appendTo(item);
			$('<span />').addClass('name').html(child.Title || '').appendTo(item);
			
			if (updatableChild(child)) {
				item.addClass('updatable');
				$('<a />').attr('href', child.EntityUri).addClass('xrm-entity-ref').appendTo(item).hide();
			}
		});

		list.sortable({ handle: '.xrm-drag-handle', opacity: 0.8 });

		dialog.render();
		dialog.show();

		XRM.ui.registerOverlay(dialog);
		dialog.focus();
	}
	
	function saveChildren(childData, childContainer, completeCallback) {
		var childMap = {};
		
		$.each(childData, function(_, child) {
			if (updatableChild(child)) {
				childMap[child.EntityUri] = child;
			}
		});
		
		var operations = [];
		
		// Gather the updatable children, and queue updates for their orders, if necessary.
		$('.xrm-editable-sitemapchild.updatable', childContainer).each(function(index, item) {
			var entityUri = $('.xrm-entity-ref', item).attr('href');
			
			if (!entityUri) {
				return;
			}
			
			var child = childMap[entityUri];
			
			if (!child) {
				return;
			}
			
			var displayOrder = (index + 1);
			
			// If the display order has changed, queue the update.
			if (child.DisplayOrder != displayOrder && child.DisplayOrderPropertyName) {
				var data = {};
				data[child.DisplayOrderPropertyName] = displayOrder;
				operations.push({ uri: entityUri, data: data });
			}
		});
		
		var successfulOperations = [], failedOperations = [];

		// Signal aggregate save completion, providing both successful and failed operation info.
		function saveComplete() {
			if ($.isFunction(completeCallback)) {
				completeCallback(successfulOperations, failedOperations);
			}
		}

		// Signal that one operation has completed.
		function operationComplete() {
			// If all operations are now completed (successfully or not), signal save completion.
			if ((successfulOperations.length + failedOperations.length) == operations.length) {
				saveComplete();
			}
		}

		// If there are no updates, just signal completion and return.
		if (operations.length < 1) {
			saveComplete();
			return;
		}

		// Post any operations.
		$.each(operations, function(i, operation) {
			XRM.data.postJSON(operation.uri, operation.data, {
				httpMethodOverride: 'MERGE',
				success: function(data, textStatus) {
					successfulOperations.push({ operation: operation });
					operationComplete();
				},
				error: function(xhr) {
					failedOperations.push({ operation: operation, xhr: xhr });
					operationComplete();
				}
			});
		});
	}
	
	function updatableChild(child) {
		return child && child.EntityUri && child.DisplayOrderPropertyName;
	}
	
});
