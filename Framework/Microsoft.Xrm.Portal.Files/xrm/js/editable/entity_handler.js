/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {

	var ns = XRM.namespace('editable.Entity.Handler');
	var Entity = XRM.editable.Entity;
	var $ = XRM.jQuery;
	
	ns.getForm = function(formPrototype, entityContainer, options) {
		var form = $.extend(true, {}, formPrototype);
		
		form.title = options.title;
		form.uri = options.uri;
		form.urlServiceUri = options.urlServiceUri;
		form.urlServiceUriTemplate = options.urlServiceUriTemplate;
		form.urlServiceOperationName = options.urlServiceOperationName;
		
		// The form is only presumed valid if it has a submission URI.
		form.valid = !!form.uri;
		
		$.each(form.fields, function() {
			var field = this;
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
	
	ns.renderCreateButton = function(createOptions, entityContainer, toolbar, entityUri, label, tooltip) {
		var module = $('<div />').addClass('xrm-editable-toolbar-module').appendTo(toolbar.body).get(0);
			
		var menu = [];
		
		$.each(createOptions, function() {
			var option = this;
			var handler = Entity.handlers[option.entityName];
			
			if (!handler) return;
			
			var uri = $('a.xrm-entity-' + option.relationship + '-ref', entityContainer).attr('href');
			
			if (!uri) return;
			
			var urlServiceUriTemplate = $('a.xrm-entity-' + option.entityName + '-url-ref', entityContainer);
			
			var form = handler.getForm(entityContainer, {
				title: XRM.localize(option.title),
				uri: uri,
				urlServiceUriTemplate: option.redirect ? urlServiceUriTemplate.attr('href') : null,
				urlServiceOperationName: option.redirect ? urlServiceUriTemplate.attr('title') : null
			});
			
			if (!(form && form.valid)) return;
			
			menu.push({
				text: XRM.localize(option.label),
				onclick: {
					fn: function() { Entity.showCreationDialog(form) }
				}
			});
		});
		
		if (menu.length > 0) {
			new YAHOO.widget.Button({
				container: module,
				type: 'menu',
				label: label,
				title: tooltip,
				menu: menu
			});
		}
	}
	
	ns.renderDeleteButton = function(entityContainer, toolbar, entityUri, entityDisplayName) {
		var entityDeleteUri = $('a.xrm-entity-delete-ref', entityContainer).attr('href');
		
		if (!entityDeleteUri) return;
		
		var module = $('<div />').addClass('xrm-editable-toolbar-module').appendTo(toolbar.body).get(0);
		
		Entity.createDeleteButton(entityContainer, entityDeleteUri, entityDisplayName, module);
	}
	
	ns.renderUpdateButton = function(entityName, entityContainer, toolbar, entityUri, label, tooltip) {
		var module = $('<div />').addClass('xrm-editable-toolbar-module').appendTo(toolbar.body).get(0);
		
		var handler = Entity.handlers[entityName];
			
		if (!handler) return;
		
		var urlServiceUri = $('a.xrm-entity-url-ref', entityContainer);
		
		var form = handler.getForm(entityContainer, {
			title: tooltip,
			uri: entityUri,
			urlServiceUri: urlServiceUri.attr('href'),
			urlServiceOperationName: urlServiceUri.attr('title')
		});
			
		if (!(form && form.valid)) return;
		
		var button = new YAHOO.widget.Button({
			container: module,
			label: label,
			title: tooltip,
			onclick: {
				fn: function() { Entity.showEditDialog(form) }
			}
		});
	}
	
});
