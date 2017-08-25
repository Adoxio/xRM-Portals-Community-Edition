/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

XRM.onActivate(function() {

	var ns = XRM.namespace('editable.Entity.handlers');
	var Entity = XRM.editable.Entity;
	var Handler = Entity.Handler;
	var $ = XRM.jQuery;

	XRM.localizations['adx_pagecomment.shortname'] = window.ResourceManager['ADX_PageComment_ShortName'];
	XRM.localizations['adx_pagecomment.update.tooltip'] = window.ResourceManager['ADX_PageComment_Update_Tooltip'];
	XRM.localizations['entity.delete.adx_pagecomment.tooltip'] = window.ResourceManager['Entity_Delete_ADX_PageComment_Tooltip'];

	var self = ns.feedback = function(entityContainer, toolbar) {
		var entityUri = $('a.xrm-entity-ref', entityContainer).attr('href');
		var entityDeleteUri = $('a.xrm-entity-delete-ref', entityContainer).attr('href');

		if (entityUri) {
			$('.xrm-edit', entityContainer).click(function() {
				var form = self.getForm(entityContainer, {
					title: XRM.localize('adx_pagecomment.update.tooltip'),
					uri: entityUri
				});

				if (!(form && form.valid)) return;

				Entity.showEditDialog(form);
			});
		}

		if (entityDeleteUri) {
			$('.xrm-delete', entityContainer).click(function() {
				Entity.showDeletionDialog(entityContainer, entityDeleteUri, XRM.localize('adx_pagecomment.shortname'));
			});
		}
	};

	self.formPrototype = {
		uri: null,
		urlServiceUri: null,
		urlServiceUriTemplate: null,
		title: null,
		entityName: 'feedback',
		reload: true,
		fields: [
			{ name: 'title', label: window.ResourceManager['Name_Label'], type: 'text', required: true },
			{ name: 'comments', label: window.ResourceManager['Content_Label'], type: 'html', ckeditorSettings: { height: 240 } },
			{ name: 'adx_approved', label: window.ResourceManager['Approved_Label'], type: 'checkbox' }
		],
		layout: {
			cssClass: 'xrm-dialog-wide',
			columns: [
				{ cssClass: 'xrm-dialog-column-main', fields: ['title', 'comments', 'adx_approved'] }
			]
		}
	};

	self.getForm = function (entityContainer, options) {
		return Handler.getForm(self.formPrototype, entityContainer, options);
	};
});
