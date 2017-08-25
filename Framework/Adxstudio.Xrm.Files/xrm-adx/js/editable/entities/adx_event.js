/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

	var ns = XRM.namespace('editable.Entity.handlers');
	var Entity = XRM.editable.Entity;
	var Handler = Entity.Handler;
	var $ = XRM.jQuery;

	XRM.localizations['adx_event.shortname'] = window.ResourceManager['ADX_Event_ShortName'];
	XRM.localizations['adx_event.update.tooltip'] = window.ResourceManager['ADX_Event_Update_Tooltip'];
	XRM.localizations['entity.create.adx_event.label'] = window.ResourceManager['Entity_Create_ADX_Event_Label'];
	XRM.localizations['entity.create.adx_event.tooltip'] = window.ResourceManager['Entity_Create_ADX_Event_Tooltip'];
	XRM.localizations['entity.delete.adx_event.tooltip'] = window.ResourceManager['Entity_Delete_ADX_Event_Tooltip'];

	var self = ns.adx_event = function (entityContainer, toolbar) {
		var entityUri = $('a.xrm-entity-ref', entityContainer).attr('href');

		// We only have functionality to add to the global edit toolbar, so if the entity is not the
		// "current" request entity, or we don't have the entity URI, quit.
		if (!(entityContainer.hasClass('xrm-entity-current') && entityUri)) {
			return;
		}

		self.renderUpdateButton(entityContainer, toolbar, entityUri);
		self.renderDeleteButton(entityContainer, toolbar, entityUri);
		self.renderCreateButton(entityContainer, toolbar, entityUri);
	};

	self.formPrototype = {
		uri: null,
		urlServiceUri: null,
		urlServiceUriTemplate: null,
		title: null,
		entityName: 'adx_event',
		fields: [
		  { name: 'adx_name', label: window.ResourceManager['Name_Label'], type: 'text', required: true  },
		  { name: 'adx_partialurl', label: window.ResourceManager['Partial_URL_Label'], type: 'text', required: true, slugify: 'adx_name'  },
		  { name: 'adx_pagetemplateid', label: window.ResourceManager['Page_Template_Label'], type: 'select', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_pagetemplate', optionText: 'adx_name', optionValue: 'adx_pagetemplateid', optionDescription: 'adx_description', sortby: 'adx_name', filter: 'adx_event'	  },
		  { name: 'adx_publishingstateid', label: window.ResourceManager['Publishing_State_Label'], type: 'select', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_publishingstate', optionText: 'adx_name', optionValue: 'adx_publishingstateid'  },
		  { name: 'adx_releasedate', label: window.ResourceManager['Release_Date_Label'], type: 'datetime', excludeEmptyData: true  },
		  { name: 'adx_expirationdate', label: window.ResourceManager['Expiration_Date_Label'], type: 'datetime', excludeEmptyData: true  },
		  { name: 'adx_hiddenfromsitemap', label: window.ResourceManager['Hidden_From_Sitemap_Label'], type: 'checkbox'  },
		  { name: 'adx_requiresregistration', label: window.ResourceManager['Requires_Registration_Label'], type: 'checkbox'  },
		  { name: 'adx_description', label: window.ResourceManager['Description_Label'], type: 'html', ckeditorSettings: { height: 80 }  },
		  { name: 'adx_locationname', label: window.ResourceManager['Location_Name_Label'], type: 'text'  },
		  { name: 'adx_summary', label: window.ResourceManager['Summary_Label'], type: 'html', ckeditorSettings: { height: 80 }  },
		  { name: 'adx_content', label: window.ResourceManager['Registration_Content_Label'], type: 'html', ckeditorSettings: { height: 80 }	  },
		  { name: 'adx_parentpageid', label: window.ResourceManager['Parent_Page_Label'], type: 'parent', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_webpage', disableAtRoot: true, defaultToCurrent: true, defaultToRoot: true }
		],
		layout: {
			cssClass: 'xrm-dialog-expanded',
      full: true,
			columns: [
			  { cssClass: 'xrm-dialog-column-main', fields: ['adx_name', 'adx_description', 'adx_summary', 'adx_content'] },
			  { cssClass: 'xrm-dialog-column-side', fields: ['adx_parentpageid', 'adx_partialurl', 'adx_pagetemplateid', 'adx_publishingstateid', 'adx_releasedate', 'adx_expirationdate', 'adx_locationname', 'adx_hiddenfromsitemap', 'adx_requiresregistration'] }
			]
		}
	};

	self.getForm = function (entityContainer, options) {
		return Handler.getForm(self.formPrototype, entityContainer, options);
	};

	self.createOptions = [
	  { entityName: 'adx_eventschedule', relationship: 'adx_event_eventschedule', label: 'entity.create.adx_eventschedule.label', title: 'entity.create.adx_eventschedule.tooltip', redirect: false }
	];

	self.renderCreateButton = function (entityContainer, toolbar, entityUri) {
		Handler.renderCreateButton(self.createOptions, entityContainer, toolbar, entityUri, XRM.localize('entity.create.label'), '');
	};

	self.renderDeleteButton = function (entityContainer, toolbar, entityUri) {
		Handler.renderDeleteButton(entityContainer, toolbar, entityUri, XRM.localize('adx_event.shortname'));
	};

	self.renderUpdateButton = function (entityContainer, toolbar, entityUri) {
		Handler.renderUpdateButton('adx_event', entityContainer, toolbar, entityUri, XRM.localize('entity.update.label'), XRM.localize('adx_event.update.tooltip'));
	};

});
