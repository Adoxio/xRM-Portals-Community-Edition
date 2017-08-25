/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

  var ns = XRM.namespace('editable.Entity.handlers');
  var Entity = XRM.editable.Entity;
  var Handler = Entity.Handler;
  var $ = XRM.jQuery;

  XRM.localizations['adx_blogpost.shortname'] = window.ResourceManager['ADX_BlogPost_ShortName'];
  XRM.localizations['adx_blogpost.update.tooltip'] = window.ResourceManager['ADX_BlogPost_Update_Tooltip'];
  XRM.localizations['entity.create.adx_blogpost.label'] = window.ResourceManager['Entity_Create_ADX_BlogPost_Label'];
  XRM.localizations['entity.create.adx_blogpost.tooltip'] = window.ResourceManager['Entity_Create_ADX_BlogPost_Tooltip'];
  XRM.localizations['entity.delete.adx_blogpost.tooltip'] = window.ResourceManager['Entity_Delete_ADX_BlogPost_Tooltip'];

  var self = ns.adx_blogpost = function (entityContainer, toolbar) {
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
    entityName: 'adx_blogpost',
    fields: [
      { name: 'adx_name', label: window.ResourceManager['Title_Label'], type: 'text', required: true      },
      { name: 'adx_date', label: window.ResourceManager['Date_Label'], type: 'datetime', excludeEmptyData: true, required: true, defaultToNow: true  },
      { name: 'adx_partialurl', label: window.ResourceManager['Partial_URL_Label'], type: 'text'    },
      { name: 'adx_summary', label: window.ResourceManager['Summary_Excerpt_Label'], type: 'html', ckeditorSettings: { height: 80 }    },
      { name: 'adx_copy', label: window.ResourceManager['Copy_Label'], type: 'html', ckeditorSettings: { height: 240 }     },
      { name: 'adx_commentpolicy', label: window.ResourceManager['Comment_Policy_Label'], type: 'picklist', required: true    },
      { name: 'adx_published', label: window.ResourceManager['Published_Label'], type: 'checkbox', checkedByDefault: true   },
      { name: 'adx_enableratings', label: window.ResourceManager['Enable_Ratings_Label'], type: 'checkbox', checkedByDefault: false   },
      { name: '__extensions.tags', label: window.ResourceManager['Tags_Label'], type: 'tags'  }
    ],
    layout: {
      cssClass: 'xrm-dialog-expanded',
      full: true,
      columns: [
        { cssClass: 'xrm-dialog-column-main', fields: ['adx_name', 'adx_summary', 'adx_copy'] },
        { cssClass: 'xrm-dialog-column-side', fields: ['adx_date', 'adx_partialurl', '__extensions.tags', 'adx_commentpolicy', 'adx_published', 'adx_enableratings'] }
      ]
    }
  };

  self.getForm = function (entityContainer, options) {
    return Handler.getForm(self.formPrototype, entityContainer, options);
  };

  self.createOptions = [
    { entityName: 'adx_webfile', relationship: 'adx_blogpost_webfile', label: 'entity.create.adx_webfile.label', title: 'entity.create.adx_webfile.tooltip', redirect: false }
  ];

  self.renderCreateButton = function (entityContainer, toolbar, entityUri) {
    Handler.renderCreateButton(self.createOptions, entityContainer, toolbar, entityUri, XRM.localize('entity.create.label'), '');
  };

  self.renderDeleteButton = function (entityContainer, toolbar, entityUri) {
    Handler.renderDeleteButton(entityContainer, toolbar, entityUri, XRM.localize('adx_blogpost.shortname'));
  };

  self.renderUpdateButton = function (entityContainer, toolbar, entityUri) {
    Handler.renderUpdateButton('adx_blogpost', entityContainer, toolbar, entityUri, XRM.localize('entity.update.label'), XRM.localize('adx_blogpost.update.tooltip'));
  };

});
