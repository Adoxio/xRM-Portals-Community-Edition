/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {

	var ns = XRM.namespace('editable.Entity.handlers');
	var Entity = XRM.editable.Entity;
	var Handler = Entity.Handler;
	var $ = XRM.jQuery;
	
	var self = ns.adx_webfile = function(entityContainer, toolbar) {
		var entityUri = $('a.xrm-entity-ref', entityContainer).attr('href');

		// We only have functionality to add to the global edit toolbar, so if the entity is not the
		// "current" request entity, or we don't have the entity URI, quit.
		if (!(entityContainer.hasClass('xrm-entity-current') && entityUri)) {
			return;
		}

		self.renderUpdateButton(entityContainer, toolbar, entityUri);
		self.renderDeleteButton(entityContainer, toolbar, entityUri);
		self.renderCreateButton(entityContainer, toolbar, entityUri);
	}
	
	self.formPrototype = {
		uri: null,
		urlServiceUri: null,
		urlServiceUriTemplate: null,
		title: null,
		entityName: 'adx_webfile',
		fields: [
			{ name: 'adx_name', label: 'Name', type: 'text', required: true },
			{ name: 'adx_partialurl', label: 'Partial URL', type: 'text', required: true, slugify: 'adx_name' },
			{ name: 'adx_webfile-attachment', label: 'File', type: 'file', required: true, fileUploadUriTemplate: null, copyFilenameTo: 'adx_name', copyFilenameSlugTo: 'adx_partialurl' },
			{ name: 'adx_displaydate', label: 'Display Date', type: 'datetime', excludeEmptyData: true },
			{ name: 'adx_hiddenfromsitemap', label: 'Hidden from Sitemap', type: 'checkbox' }
		]
	};
	
	self.getForm = function(entityContainer, options) {
		return Handler.getForm(self.formPrototype, entityContainer, options);
	}
	
	self.createOptions = [];
	
	self.renderCreateButton = function(entityContainer, toolbar, entityUri) {
		Handler.renderCreateButton(self.createOptions, entityContainer, toolbar, entityUri, XRM.localize('entity.create.label'), '');
	}
	
	self.renderDeleteButton = function(entityContainer, toolbar, entityUri) {
		Handler.renderDeleteButton(entityContainer, toolbar, entityUri, XRM.localize('adx_webfile.shortname'));
	}
	
	self.renderUpdateButton = function(entityContainer, toolbar, entityUri) {
		Handler.renderUpdateButton('adx_webfile', entityContainer, toolbar, entityUri, XRM.localize('entity.update.label'), XRM.localize('adx_webfile.update.tooltip'));
	}
	
});
