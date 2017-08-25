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
	
	var isSortingEnabled = $.isFunction($.fn.sortable);
	
	if (!isSortingEnabled) {
		XRM.log('XRM weblinks sorting disabled. Include jQuery UI with Sortable interaction to enable.', 'warn', 'XRM.editable.Entity.handlers.adx_weblinkset');
	}
	
	var self = ns.adx_weblinkset = function(entityContainer, editToolbar) {
		var entityServiceRef = $('a.xrm-entity-ref', entityContainer);
		var entityServiceUri = entityServiceRef.attr('href');

		if (!entityServiceUri) {
			return XRM.log('Unable to get weblink set service URI. Web links will not be editable.', 'warn', 'editable.Entity.handlers.adx_weblinkset');
		}
		
		var weblinksServiceUri = $('a.xrm-entity-adx_weblinkset_weblink-ref', entityContainer).attr('href');
		var saveWebLinksServiceUri = $('a.xrm-entity-adx_weblinkset_weblink-update-ref', entityContainer).attr('href');
		
		if (!weblinksServiceUri) {
			return XRM.log('Unable to get child weblinks service URI. Web links will not be editable.', 'warn', 'editable.Entity.handlers.adx_weblinkset');
		}
		
		self.addClassIfEmpty(entityContainer);

		entityContainer.editable(weblinksServiceUri, {
			loadSuccess: function(entityData) {
				self.renderEditDialog(entityContainer, editToolbar, weblinksServiceUri, saveWebLinksServiceUri, entityServiceRef.attr('title'), entityData);
			}
		});
	}
	
	self.addClassIfEmpty = function(entityContainer) {
		if (entityContainer.children('ul').children('li').length == 0) {
			entityContainer.addClass('xrm-entity-value-empty');
		}
	}
	
	self.formPrototype = {
		title: null,
		entityName: 'adx_weblink',
		fields: [
			{ name: 'adx_name', label: 'Name', type: 'text', required: true },
			{ name: 'adx_pageid', label: 'Page', type: 'select', uri: null, optionEntityName: 'adx_webpage', optionText: 'adx_name', optionValue: 'adx_webpageid', sortby: 'adx_name' },
			{ name: 'adx_externalurl', label: 'External URL', type: 'text' },
			{ name: 'adx_description', label: 'Description', type: 'html' },
			{ name: 'adx_robotsfollowlink', label: 'Robots follow link', type: 'checkbox', value: true },
			{ name: 'adx_openinnewwindow', label: 'Open in new window', type: 'checkbox' }
		]
	};
	
	self.getWeblinkForm = function(title, weblinkData, entityContainer) {
		weblinkData = weblinkData || {};
		
		var form = $.extend(true, {}, self.formPrototype);
		
		form.title = title;
		form.valid = true;
		
		$.each(form.fields, function(i, field) {
			var propertyName = self.getWeblinkPropertyName(field.name);
			
			if (!propertyName) return;
			
			var propertyData = weblinkData[propertyName];
			
			if (typeof (propertyData) === 'undefined') return;
			
			field.value = propertyData;
		});
		
		$.each(form.fields, function() {
			var field = this;
			
			field.label = XRM.localize(form.entityName + '.' + field.name) || field.label || field.name;
			
			if (field.optionEntityName) {
				var data = $('a.xrm-entity-' + field.optionEntityName + '-ref', entityContainer).attr('href');
				
				if (!data) {
					form.valid = false;
				}
				
				field.uri = data;
			}
			
			if (this.type === 'file') {
				var data = $('a.xrm-uri-template.xrm-entity-' + field.name + '-ref', entityContainer).attr('href');
				
				if (!data) {
					form.valid = false;
				}
				
				field.fileUploadUriTemplate = data;
			}
		});
		
		return form;
	}
	
	self.getWeblinkPropertyName = function(propertySchemaName) {
		return Entity.getPropertyName('adx_weblink', propertySchemaName);
	}
	
	self.renderEditDialog = function(entityContainer, editToolbar, weblinksServiceUri, saveWebLinksServiceUri, entityTitle, entityData) {
		var yuiContainer = $('<div />').addClass(yuiSkinClass).appendTo(document.body);
		var dialogContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog xrm-editable-dialog-weblinkset').appendTo(yuiContainer);

		var webLinkData = $.isArray(entityData.d) ? entityData.d : [];

		function completeEdit(dialog) {
			dialog.cancel();
			yuiContainer.remove();
		}

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
						var weblinksContainer = $('.xrm-editable-weblinks', dialog.body);
						self.saveWebLinks(webLinkData, weblinksContainer, saveWebLinksServiceUri, function (successfulOperations, failedOperations) {
							hideProgressPanel();

							if (failedOperations.length < 1) {
								completeEdit(dialog);
							}
							else {
								XRM.ui.showDataServiceError($.map(failedOperations, function(operation) { return operation.xhr; }));
							}

							// Instead of updating the DOM in-place, just refresh the page.
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

		dialog.setHeader(XRM.localize('editable.label.prefix') + (entityTitle || ''));
		dialog.setBody('<ol class="xrm-editable-weblinks"></ol>');
		
		var displayOrderPropertyName = self.getWeblinkPropertyName('adx_displayorder');

		// Sort weblinks by display order and render them.
		webLinkData.sort(function(a, b) {
			var aOrder = a[displayOrderPropertyName], bOrder = b[displayOrderPropertyName];
			if (aOrder < bOrder) return -1;
			if (aOrder == bOrder) return 0;
			return 1;
		});

		var list = $('.xrm-editable-weblinks', dialog.body);
		
		$.each(webLinkData, function(i, weblink) { self.addWebLinkItem(dialog, list, weblink, entityContainer); });
		
		var testForm = self.getWeblinkForm('', {}, entityContainer);
		
		if (testForm.valid) {
			var weblinkCreationLink = $('<a />').attr('title', XRM.localize('entity.create.adx_weblink.tooltip')).addClass('xrm-add').insertAfter(list).append('<span />');
			
			weblinkCreationLink.click(function() {
				var form = self.getWeblinkForm(XRM.localize('entity.create.adx_weblink.tooltip'), {}, entityContainer);
				
				// Show an entity creation dialog for the web link, but override the default save process of
				// the form, so that instead of POSTing to the service on submission, just grab the JSON for
				// the new web link and stuff it into the DOM for later (when the whole set is saved).
				Entity.showEntityDialog(form, {
					save: function(form, data, options) {
						self.addWebLinkItem(dialog, list, data, entityContainer).hide().show('slow');
															
						if ($.isFunction(options.success)) {
							options.success();
						}
						
						dialog.focus();
					},
					cancel: function() {
						dialog.focus();
					}
				});
			});
		}
		
		if (isSortingEnabled) {
			$('.xrm-editable-weblinks', dialog.body).sortable({ handle: '.xrm-drag-handle', opacity: 0.8 });
		}

		dialog.render();
		dialog.show();

		XRM.ui.registerOverlay(dialog);
		dialog.focus();
	}
	
	self.addWebLinkItem = function(dialog, list, weblink, entityContainer) {
		var item = $('<li />').addClass('xrm-editable-weblink').appendTo(list);
		
		// We'll store any JSON data for weblink updates or creates in this element.
		var weblinkUpdateData = $('<input />').attr('type', 'hidden').addClass('xrm-editable-weblink-data').appendTo(item);
		
		if (isSortingEnabled) {
			$('<a />').attr('title', XRM.localize('editable.sortable.tooltip')).addClass('xrm-drag-handle').appendTo(item);
		}
		else {
			$('<span />').addClass('xrm-drag-disabled').appendTo(item);
		}
		
		$('<a />').attr('title', XRM.localize('entity.delete.adx_weblink.tooltip')).addClass('xrm-delete').appendTo(item).click(function() {
			item.addClass('deleted').hide('slow');
		});
		
		var weblinkName = $('<span />').addClass('name').text(weblink[self.getWeblinkPropertyName('adx_name')]);
		
		var testForm = self.getWeblinkForm(XRM.localize('adx_weblink.update.tooltip'), {}, entityContainer);
		
		if (testForm.valid) {
			$('<a />').attr('title', XRM.localize('adx_weblink.update.tooltip')).addClass('xrm-edit').appendTo(item).click(function() {
				
				var currentWeblinkData = weblink;
				
				try {
					var updateJson = weblinkUpdateData.val();
					
					if (updateJson) {
						currentWeblinkData = JSON.parse(updateJson);
					}
				}
				catch (e) {}
			
				Entity.showEntityDialog(self.getWeblinkForm(XRM.localize('adx_weblink.update.tooltip'), currentWeblinkData, entityContainer), {
					save: function(form, data, options) {
						if (!weblinkUpdateData.hasClass('create')) {
							weblinkUpdateData.addClass('update');
						}
						
						var namePropertyName = self.getWeblinkPropertyName('adx_name');
						
						if (namePropertyName && data[namePropertyName]) {
							weblinkName.text(data[namePropertyName]);
						}
						
						weblinkUpdateData.val(JSON.stringify(data));
						
						if ($.isFunction(options.success)) {
							options.success();
						}
						
						dialog.focus();
					},
					cancel: function() {
						dialog.focus();
					}
				});
				
			});
		}
		
		weblinkName.appendTo(item);
		
		if (weblink.__metadata) {
			$('<a />').attr('href', weblink.__metadata.uri).addClass('xrm-entity-ref').appendTo(item).hide();
			
			var weblinkDeleteUriTemplate = $('a.xrm-uri-template.xrm-entity-adx_weblink-delete-ref', entityContainer).attr('href')
			
			if (weblinkDeleteUriTemplate) {
				var weblinkDeleteUri = XRM.util.expandUriTemplate(weblinkDeleteUriTemplate, weblink);
				
				if (weblinkDeleteUri) {
					$('<a />').attr('href', weblinkDeleteUri).addClass('xrm-entity-delete-ref').appendTo(item).hide();
				}
			}
		}
		else {
			weblinkUpdateData.addClass('create').val(JSON.stringify(weblink));
		}
			
		return item;
	}
	
	self.saveWebLinks = function(webLinkData, weblinksContainer, weblinksServiceUri, completeCallback) {
		// Map our weblink data into an object in for which we can look things up by ID.
		var weblinkMap = {};
		
		$.each(webLinkData, function(i, weblink) { weblinkMap[weblink.__metadata.uri] = weblink });
		
		var operations = [];
		
		// Go through the deleted weblinks (the ones that ever existed to begin with, i.e., weren't
		// just added then deleted in the same edit session), and queue up the delete operation.
		$('.xrm-editable-weblink.deleted', weblinksContainer).each(function(i, item) {
			var weblinkDeleteUri = $('.xrm-entity-delete-ref', item).attr('href');
			
			if (!weblinkDeleteUri) return;
							
			operations.push({ uri: weblinkDeleteUri, method: null });
		});
		
		var displayOrderPropertyName = self.getWeblinkPropertyName('adx_displayorder');
		
		// Go through the non-deleted weblinks, and queue up any update or creation operations.
		$('.xrm-editable-weblink:not(.deleted)', weblinksContainer).each(function(i, item) {
			var weblink = weblinkMap[$('.xrm-entity-ref', item).attr('href')];
			var displayOrder = (i + 1);
			var json = $('.xrm-editable-weblink-data', item).val();
			
			// This is a pre-existing weblink. Construct its update data, and queue the update.
			if (weblink) {
				var data = {}, updated = false;
				
				if (json) {
					try {
						data = JSON.parse(json);
						updated = !!data;
					}
					catch (e) {}
				}
				
				if (weblink[displayOrderPropertyName] != displayOrder) {
					data[displayOrderPropertyName] = displayOrder;
					updated = true;
				}
				
				if (updated) {	
					operations.push({ uri: weblink.__metadata.uri, method: 'MERGE', data: data });
				}
			}
			// This is a newly-added weblink. Construct its data, and queue the create operation.
			else {
				try {
					var data = JSON.parse(json);
					data[displayOrderPropertyName] = displayOrder;
					
					// method is null so that our data APIs don't use an HTTP method override--we just
					// want a normal POST.
					operations.push({ uri: weblinksServiceUri, method: null, data: data });
				}
				catch (e) {
					return;
				}
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
				httpMethodOverride: operation.method,
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
	
});
