/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

  var ns = XRM.namespace('editable.Entity.handlers');
  var Entity = XRM.editable.Entity;
  var Handler = Entity.Handler;
  var $ = XRM.jQuery;

  XRM.localizations['adx_communityforumthread.shortname'] = window.ResourceManager['ADX_Community_Forum_Thread_ShortName'];
  XRM.localizations['adx_communityforumthread.update.tooltip'] = window.ResourceManager['ADX_CommunityForumThread_Update_Tooltip'];
  XRM.localizations['entity.delete.adx_communityforumthread.tooltip'] = window.ResourceManager['Entity_Delete_ADX_CommunityForumThread_Tooltip'];

  var self = ns.adx_communityforumthread = function (entityContainer, toolbar) {
    var entityUri = $('a.xrm-entity-ref', entityContainer).attr('href');

    // We only have functionality to add to the global edit toolbar, so if the entity is not the
    // "current" request entity, or we don't have the entity URI, quit.
    if (!(entityContainer.hasClass('xrm-entity-current') && entityUri)) {
      return;
    }

    self.renderUpdateButton(entityContainer, toolbar, entityUri);
    self.renderDeleteButton(entityContainer, toolbar, entityUri);
  };

  self.formPrototype = {
    uri: null,
    urlServiceUri: null,
    urlServiceUriTemplate: null,
    title: null,
    entityName: 'adx_communityforumthread',
    fields: [
      { name: 'adx_name', label: window.ResourceManager['Name_Label'], type: 'text', required: true   },
      { name: 'adx_typeid', label: window.ResourceManager['Thread_Type_Label'], type: 'select', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_forumthreadtype', optionText: 'adx_name', optionValue: 'adx_forumthreadtypeid'    },
      { name: '__extensions.tags', label: window.ResourceManager['Tags_Label'], type: 'tags'   },
      { name: 'adx_sticky', label: window.ResourceManager['Sticky_Label'], type: 'checkbox'   },
      { name: 'adx_isanswered', label: window.ResourceManager['Answered_Label'], type: 'checkbox' },
      { name: 'adx_locked', label: window.ResourceManager['Locked_Label'], type: 'checkbox'  }
    ]
  };

  self.getForm = function (entityContainer, options) {
    return Handler.getForm(self.formPrototype, entityContainer, options);
  };

  self.createOptions = [];

  self.renderDeleteButton = function (entityContainer, toolbar, entityUri) {
    Handler.renderDeleteButton(entityContainer, toolbar, entityUri, XRM.localize('adx_communityforumthread.shortname'));
  };

  self.renderUpdateButton = function (entityContainer, toolbar, entityUri) {
    Handler.renderUpdateButton('adx_communityforumthread', entityContainer, toolbar, entityUri, XRM.localize('entity.update.label'), XRM.localize('adx_communityforumthread.update.tooltip'));
  };

});
