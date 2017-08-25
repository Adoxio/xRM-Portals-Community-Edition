/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {

	var ns = XRM.namespace('editable.Entity.handlers');
	var Entity = XRM.editable.Entity;
	var Handler = Entity.Handler;
	var $ = XRM.jQuery;
	
	XRM.localizations['adx_communityforum.shortname'] = window.ResourceManager['ADX_CommunityForum_ShortName'];
	XRM.localizations['adx_communityforum.update.tooltip'] = window.ResourceManager['ADX_CommunityForum_Update_Tooltip'];
	XRM.localizations['entity.create.adx_communityforum.label'] = window.ResourceManager['Entity_Create_ADX_CommunityForum_Label'];
	XRM.localizations['entity.create.adx_communityforum.tooltip'] = window.ResourceManager['Entity_Create_ADX_CommunityForum_Tooltip'];
	XRM.localizations['entity.delete.adx_communityforum.tooltip'] = window.ResourceManager['Entity_Delete_ADX_CommunityForum_Tooltip'];
	
	var self = ns.adx_communityforum = function(entityContainer, toolbar) {
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
		entityName: 'adx_communityforum',
		fields: [
			{  name: 'adx_name', label: window.ResourceManager['Name_Label'], type: 'text', required: true 	},
			{ name: 'adx_parentpageid', label: window.ResourceManager['Parent_Page_Label'], type: 'parent', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_webpage', disableAtRoot: true, defaultToCurrent: true, defaultToRoot: true },
			{  name: 'adx_partialurl', label: window.ResourceManager['Partial_URL_Label'], type: 'text', required: true, slugify: 'adx_name'    },
			{  name: 'adx_description', label: window.ResourceManager['Description_Label'], type: 'text'    },
			{  name: 'adx_forumpagetemplateid', label: window.ResourceManager['Forum_Template_Label'], type: 'select', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_pagetemplate', optionText: 'adx_name', optionValue: 'adx_pagetemplateid', optionDescription: 'adx_description', sortby: 'adx_name', filter: 'adx_communityforum'	},
			{  name: 'adx_threadpagetemplateid', label: window.ResourceManager['Thread_Template_Label'], type: 'select', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_pagetemplate', optionText: 'adx_name', optionValue: 'adx_pagetemplateid', optionDescription: 'adx_description', sortby: 'adx_name', filter: 'adx_communityforumthread'	},
			{  name: 'adx_publishingstateid', label: window.ResourceManager['Publishing_State_Label'], type: 'select', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_publishingstate', optionText: 'adx_name', optionValue: 'adx_publishingstateid' 	},
			{  name: 'adx_hiddenfromsitemap', label: window.ResourceManager['Hidden_From_Sitemap_Label'], type: 'checkbox'	}
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
		Handler.renderDeleteButton(entityContainer, toolbar, entityUri, XRM.localize('adx_communityforum.shortname'));
	}
	
	self.renderUpdateButton = function(entityContainer, toolbar, entityUri) {
		Handler.renderUpdateButton('adx_communityforum', entityContainer, toolbar, entityUri, XRM.localize('entity.update.label'), XRM.localize('adx_communityforum.update.tooltip'));
	}
	
});
