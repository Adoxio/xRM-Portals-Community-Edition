/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

  var ns = XRM.namespace('editable.Entity.handlers');
  var Entity = XRM.editable.Entity;
  var Handler = Entity.Handler;
  var $ = XRM.jQuery;

  XRM.localizations['adx_blog.shortname'] = window.ResourceManager['ADX_Blog_ShortName'];
  XRM.localizations['adx_blog.update.tooltip'] = window.ResourceManager['ADX_Blog_Update_Tooltip'];
  XRM.localizations['entity.create.adx_blog.label'] = window.ResourceManager['Entity_Create_ADX_Blog_Label'];
  XRM.localizations['entity.create.adx_blog.tooltip'] = window.ResourceManager['Entity_Create_ADX_Blog_Tooltip'];
  XRM.localizations['entity.delete.adx_blog.tooltip'] = window.ResourceManager['Entity_Delete_ADX_Blog_Tooltip'];

  var self = ns.adx_blog = function (entityContainer, toolbar) {
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
    entityName: 'adx_blog',
    fields: [
      { name: 'adx_name', label: window.ResourceManager['Name_Label'], type: 'text', required: true },
      { name: 'adx_partialurl', label: window.ResourceManager['Partial_URL_Label'], type: 'text', required: true, slugify: 'adx_name' },
      { name: 'adx_summary',label: window.ResourceManager['Summary_Label'] , type: 'html', tinymceSettings: { height: 240 } },
      { name: 'adx_bloghomepagetemplateid', label: window.ResourceManager['Home_Template_Label'] , type: 'select', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_pagetemplate', optionText: 'adx_name', optionValue: 'adx_pagetemplateid', optionDescription: 'adx_description', sortby: 'adx_name', filter: 'adx_blog' },
      { name: 'adx_blogpostpagetemplateid', label: window.ResourceManager['Post_Template_Label'] , type: 'select', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_pagetemplate', optionText: 'adx_name', optionValue: 'adx_pagetemplateid', optionDescription: 'adx_description', sortby: 'adx_name', filter: 'adx_blogpost' },
      { name: 'adx_blogarchivepagetemplateid', label: window.ResourceManager['Archive_Template_Label'] , type: 'select', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_pagetemplate', optionText: 'adx_name', optionValue: 'adx_pagetemplateid', optionDescription: 'adx_description', sortby: 'adx_name', filter: 'adx_webpage' },
      { name: 'adx_commentpolicy', label: window.ResourceManager['Comment_Policy_Label'] , type: 'picklist', required: true },
      { name: 'adx_parentpageid', label: window.ResourceManager['Parent_Page_Label'], type: 'parent', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_webpage', disableAtRoot: true, defaultToCurrent: true, defaultToRoot: true }
    ],
    layout: {
      cssClass: 'xrm-dialog-expanded',
      full: true,
      columns: [
        { cssClass: 'xrm-dialog-column-main', fields: ['adx_name', 'adx_summary'] },
        { cssClass: 'xrm-dialog-column-side', fields: ['adx_parentpageid', 'adx_partialurl', 'adx_bloghomepagetemplateid', 'adx_blogpostpagetemplateid', 'adx_blogarchivepagetemplateid', 'adx_commentpolicy'] }
      ]
    }
  };

  self.getForm = function (entityContainer, options) {
    return Handler.getForm(self.formPrototype, entityContainer, options);
  };

  self.createOptions = [
    { entityName: 'adx_blogpost', relationship: 'adx_blog_blogpost', label: 'entity.create.adx_blogpost.label', title: 'entity.create.adx_blogpost.tooltip', redirect: true }
  ];

  self.renderCreateButton = function (entityContainer, toolbar, entityUri) {
    Handler.renderCreateButton(self.createOptions, entityContainer, toolbar, entityUri, XRM.localize('entity.create.label'), '');
  };

  self.renderDeleteButton = function (entityContainer, toolbar, entityUri) {
    Handler.renderDeleteButton(entityContainer, toolbar, entityUri, XRM.localize('adx_blog.shortname'));
  };

  self.renderUpdateButton = function (entityContainer, toolbar, entityUri) {
    Handler.renderUpdateButton('adx_blog', entityContainer, toolbar, entityUri, XRM.localize('entity.update.label'), XRM.localize('adx_blog.update.tooltip'));
  };

});
