/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

XRM.onActivate(function () {

  var ns = XRM.namespace('editable.Entity.Handler');
  var Entity = XRM.editable.Entity;
  var $ = XRM.jQuery;
  var JSON = YAHOO.lang.JSON;

  ns.getForm = function (formPrototype, entityContainer, options) {
    var form = $.extend(true, {}, formPrototype);

    form.title = options.title;
    form.editTitle = XRM.ui.getEditTooltip(entityContainer);
    form.uri = options.uri;
    form.urlServiceUri = options.urlServiceUri;
    form.urlServiceUriTemplate = options.urlServiceUriTemplate;
    form.urlServiceOperationName = options.urlServiceOperationName;
    form.root = entityContainer.attr('data-root') == 'true';
    form.currentLanguage = {};

    if (entityContainer.attr('data-languagename') !== 'undefined')
      form.currentLanguage.Name = entityContainer.attr('data-languagename');

    if (entityContainer.attr('data-languageid') !== 'undefined')
      form.currentLanguage.Id = entityContainer.attr('data-languageid');

    form.currentLanguage.LogicalName = "adx_websitelanguage";
    form.currentLanguage.__metadata = { type: "Microsoft.Xrm.Sdk.EntityReference" };

    var logicalName = entityContainer.attr('data-logicalname');
    var id = entityContainer.attr('data-id');
    var rootwebpageid = entityContainer.attr('data-rootwebpageid');

    if (logicalName && id) {
      form.current = { LogicalName: logicalName, Id: id };
    }

    if (logicalName && rootwebpageid) {
      form.rootWebpage = { LogicalName: logicalName, Id: rootwebpageid};
    }


    // The form is only presumed valid if it has a submission URI.
    form.valid = !!form.uri;

    var fieldsMissingServiceUri = [];

    $.each(form.fields, function () {
      var field = this;

      if (field.type === 'select') {
        var optionServiceUri = $('a.xrm-entity-' + field.optionEntityName + '-ref', entityContainer).attr('href');

        if (!optionServiceUri) {
          if (field.expansion) {
            fieldsMissingServiceUri.push(field);
          }
          else {
            form.valid = false;
          }
        }

        field.uri = field.filter
          ? XRM.util.updateQueryStringParameter(optionServiceUri, 'filter', field.filter)
          : optionServiceUri;

        field.uriTemplate = $('a.xrm-entity-edit' + field.optionEntityName + '-ref', entityContainer).attr('href');

        if (field.create && field.create.relationship) {
          field.create.uriTemplate = $('a.xrm-uri-template.xrm-entity-' + field.create.relationship + '-ref', entityContainer).attr('href');
          field.create.entityContainer = entityContainer;
        }
      }

      Entity.configureDateTimeField(field, entityContainer);

      if (field.type === 'file') {
        var fileUploadUriTemplate = $('a.xrm-uri-template.xrm-entity-' + field.name + '-ref', entityContainer).attr('href');

        if (!fileUploadUriTemplate) {
          form.valid = false;
        }

        field.fileUploadUriTemplate = fileUploadUriTemplate;
      }

      if (field.type === 'html') {
        field.cmsTemplateUrl = entityContainer.data('cmstemplate-url');
        field.fileBrowserServiceUri = entityContainer.data('filebrowser-url') || $('a.xrm-filebrowser-ref', entityContainer).attr('href');
        field.fileBrowserDialogUri = entityContainer.data('filebrowser-dialog-url') || $('a.xrm-filebrowser-dialog-ref', entityContainer).attr('href');
      }

      if (field.type === 'iframe') {
        field.src = field.src || (entityContainer.closest('[data-xrm-base]').attr('data-xrm-base') || '') + field.xrmsrc;
      }

      if (field.type === 'parent') {
        field.uri = entityContainer.attr('data-parentoptions');
        field.uriTemplate = entityContainer.attr('data-parentoptions-uritemplate');
      }
    });

    $.each(fieldsMissingServiceUri, function () {
      var field = this;

      form.fields.splice($.inArray(field, form.fields), 1);
    });

    return form;
  };
});
