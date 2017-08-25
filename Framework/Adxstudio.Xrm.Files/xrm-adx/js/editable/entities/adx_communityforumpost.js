/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

  var ns = XRM.namespace('editable.Entity.handlers');
  var Entity = XRM.editable.Entity;
  var Handler = Entity.Handler;
  var $ = XRM.jQuery;

  XRM.localizations['adx_communityforumpost.shortname'] = window.ResourceManager['ADX_CommunityForumPost_ShortName'];
  XRM.localizations['adx_communityforumpost.update.tooltip'] = window.ResourceManager['ADX_CommunityForumPost_Update_Tooltip'];
  XRM.localizations['entity.delete.adx_communityforumpost.tooltip'] = window.ResourceManager['Entity_Delete_ADX_CommunityForumPost_Tooltip'];

  var self = ns.adx_communityforumpost = function (entityContainer, toolbar) {
    var entityUri = $('a.xrm-entity-ref', entityContainer).attr('href');
    var entityDeleteUri = $('a.xrm-entity-delete-ref', entityContainer).attr('href');

    if (entityUri) {
      $('.xrm-edit', entityContainer).click(function () {
        var form = self.getForm(entityContainer, {
          title: XRM.localize('adx_communityforumpost.update.tooltip'),
          uri: entityUri
        });

        if (!(form && form.valid)) return;

        Entity.showEditDialog(form);
      });
    }

    if (entityDeleteUri) {
    	$('.xrm-delete', entityContainer).click(function () {
    		var $this = $(this);
    		Entity.showDeletionDialog(entityContainer, entityDeleteUri, XRM.localize('adx_communityforumpost.shortname'), $this.data('warning'));
    	});
    }
  };

  self.formPrototype = {
    uri: null,
    urlServiceUri: null,
    urlServiceUriTemplate: null,
    title: null,
    entityName: 'adx_communityforumpost',
    reload: true,
    fields: [
      {  name: 'adx_name', label: window.ResourceManager['Name_Label'], type: 'text', required: true  },
      {  name: 'adx_content', label: window.ResourceManager['Content_Label'], type: 'html', ckeditorSettings: { height: 240 }   },
      {  name: 'adx_isanswer', label: window.ResourceManager['Answer_Label'], type: 'checkbox'   }
    ],
    layout: {
      cssClass: 'xrm-dialog-wide',
      columns: [
        { cssClass: 'xrm-dialog-column-main', fields: ['adx_name', 'adx_content', 'adx_isanswer'] }
      ]
    }
  };

  self.getForm = function (entityContainer, options) {
    return Handler.getForm(self.formPrototype, entityContainer, options);
  };
});
