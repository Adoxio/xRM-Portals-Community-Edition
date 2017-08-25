/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

  var ns = XRM.namespace('editable.Entity.handlers');
  var Entity = XRM.editable.Entity;
  var Handler = Entity.Handler;
  var $ = XRM.jQuery;

  XRM.localizations['adx_shortcut.shortname'] = window.ResourceManager['ADX_Shortcut_ShortName'];
  XRM.localizations['adx_shortcut.update.tooltip'] = window.ResourceManager['ADX_Shortcut_Update_Tooltip'];
  XRM.localizations['adx_shortcut.targetinstructions'] = window.ResourceManager['ADX_Shortcut_Target_Instructions'];
  XRM.localizations['entity.create.adx_shortcut.label'] = window.ResourceManager['Entity_Create_ADX_Shortcut_Label'];
  XRM.localizations['entity.create.adx_shortcut.tooltip'] = window.ResourceManager['Entity_Create_ADX_Shortcut_Tooltip'];
  XRM.localizations['entity.delete.adx_shortcut.tooltip'] = window.ResourceManager['Entity_Delete_ADX_Shortcut_Tooltip'];

  var self = ns.adx_shortcut = function(entityContainer, toolbar) {
  };

  self.formPrototype = {
    uri: null,
    urlServiceUri: null,
    urlServiceUriTemplate: null,
    title: null,
    entityName: 'adx_shortcut',
    reload: true,
    fields: [
      { name: 'adx_name', label: window.ResourceManager['Name_Label'], type: 'text', required: true },
      { name: 'adx_title', label: window.ResourceManager['Title_Label'], type: 'text' },
      { name: 'adx_description', label: window.ResourceManager['Description_Label'], type: 'html', ckeditorSettings: { height: 240 } },
      { name: 'targetinstructions', type: 'instructions' },
      { name: 'adx_externalurl', label: window.ResourceManager['Target_URL_Label'], type: 'text'  },
      { name: 'adx_webpageid', label: window.ResourceManager['Target_Web_Page_Label'], type: 'select', excludeEmptyData: true, uri: null, optionEntityName: 'adx_webpage', optionText: 'adx_name', optionValue: 'adx_webpageid', sortby: 'adx_name' },
      { name: 'adx_webfileid', label: window.ResourceManager['Target_Web_File_Label'], type: 'select', excludeEmptyData: true, uri: null, optionEntityName: 'adx_webfile', optionText: 'adx_name', optionValue: 'adx_webfileid', sortby: 'adx_name' },
      { name: 'adx_eventid', label: window.ResourceManager['Target_Event_Label'], type: 'select', excludeEmptyData: true, uri: null, optionEntityName: 'adx_event', optionText: 'adx_name', optionValue: 'adx_eventid', sortby: 'adx_name', expansion: true  },
      { name: 'adx_forumid', label: window.ResourceManager['Target_Forum_Label'], type: 'select', excludeEmptyData: true, uri: null, optionEntityName: 'adx_communityforum', optionText: 'adx_name', optionValue: 'adx_communityforumid', sortby: 'adx_name', expansion: true  },
      { name: 'adx_disabletargetvalidation', label: window.ResourceManager['Disable_Target_Validation_Label'], type: 'checkbox'  },
      { name: 'adx_parentpage_webpageid', label: window.ResourceManager['Parent_Page_Label'], type: 'parent', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_webpage', disableAtRoot: true, defaultToCurrent: true, defaultToRoot: true }
    ],
    layout: {
      cssClass: 'xrm-dialog-expanded',
      full: true,
      columns: [
        { cssClass: 'xrm-dialog-column-main', fields: ['adx_name', 'adx_title', 'adx_description'] },
        { cssClass: 'xrm-dialog-column-side', fields: ['adx_parentpage_webpageid', 'targetinstructions', 'adx_externalurl', 'adx_webpageid', 'adx_webfileid', 'adx_eventid', 'adx_forumid', 'adx_disabletargetvalidation'] }
      ]
    }
  };

  self.getForm = function (entityContainer, options) {
    return Handler.getForm(self.formPrototype, entityContainer, options);
  };

});
