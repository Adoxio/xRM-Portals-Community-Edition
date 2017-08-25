/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {

	var ns = XRM.namespace('editable.Entity.handlers');
	var Entity = XRM.editable.Entity;
	var Handler = Entity.Handler;
	var $ = XRM.jQuery;
	
	var self = ns.adx_webpage = function(entityContainer, toolbar) {
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
		entityName: 'adx_webpage',
		fields: [
			{ name: 'adx_name', label: 'Name', type: 'text', required: true },
			{ name: 'adx_partialurl', label: 'Partial URL', type: 'text', required: true, slugify: 'adx_name' },
			{ name: 'adx_pagetemplateid', label: 'Page Template', type: 'select', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_pagetemplate', optionText: 'adx_name', optionValue: 'adx_pagetemplateid', sortby: 'adx_name' },
			{ name: 'adx_displaydate', label: 'Display Date', type: 'datetime', excludeEmptyData: true },
			{ name: 'adx_hiddenfromsitemap', label: 'Hidden from Sitemap', type: 'checkbox' }
		]
	};
	
	self.getForm = function(entityContainer, options) {
		return Handler.getForm(self.formPrototype, entityContainer, options);
	}
	
	self.createOptions = [
		{ entityName: 'adx_webpage', relationship: 'adx_webpage_webpage_Referenced', label: 'entity.create.adx_webpage.label', title: 'entity.create.adx_webpage.tooltip', redirect: true },
		{ entityName: 'adx_webfile', relationship: 'adx_webpage_webfile', label: 'entity.create.adx_webfile.label', title: 'entity.create.adx_webfile.tooltip', redirect: false }
	];
	
	self.renderCreateButton = function(entityContainer, toolbar, entityUri) {
		Handler.renderCreateButton(self.createOptions, entityContainer, toolbar, entityUri, XRM.localize('entity.create.label'), '');
	}
	
	self.renderDeleteButton = function(entityContainer, toolbar, entityUri) {
		Handler.renderDeleteButton(entityContainer, toolbar, entityUri, XRM.localize('adx_webpage.shortname'));
	}
	
	self.renderUpdateButton = function(entityContainer, toolbar, entityUri) {
		Handler.renderUpdateButton('adx_webpage', entityContainer, toolbar, entityUri, XRM.localize('entity.update.label'), XRM.localize('adx_webpage.update.tooltip'));
	}
	
});
