/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

XRM.onActivate(function () {

  var ns = XRM.namespace('util');
  var $ = XRM.jQuery;

  $.extend(true, XRM.localizations, {
  	'entity.create.label': window.ResourceManager['Entity_Create_Label'],
  	'entity.create.adx_webfile.label': window.ResourceManager['Entity_Create_Adx_Webfile_Label'],
  	'entity.create.adx_webfile.tooltip': window.ResourceManager['Entity_Create_Adx_Webfile_Tooltip'],
  	'entity.create.adx_weblink.tooltip': window.ResourceManager['Entity_Create_Adx_Weblink_Tooltip'],
  	'entity.create.adx_webpage.label': window.ResourceManager['Entity_Create_Adx_Webpage_Label'],
  	'entity.create.adx_webpage.tooltip': window.ResourceManager['Entity_Create_Adx_Webpage_Tooltip'],
  	'entity.delete.adx_weblink.tooltip': window.ResourceManager['Entity_Delete_Adx_Weblink_Tooltip'],
  	'entity.update.label': window.ResourceManager['Edit_Label'],
  	'adx_webfile.shortname': window.ResourceManager['Adx_Webfile_Shortname'],
  	'adx_webfile.update.tooltip': window.ResourceManager['Adx_Webfile_Update_Tooltip'],
  	'adx_weblink.update.tooltip': window.ResourceManager['Adx_Weblink_Update_Tooltip'],
  	'adx_webpage.shortname': window.ResourceManager['Adx_Webpage_Shortname'],
  	'adx_webpage.update.tooltip': window.ResourceManager['Adx_Webpage_Update_Tooltip'],
  	'datetimepicker.datepicker.label': window.ResourceManager['Datetimepicker_Datepicker_Label'],
  	'editable.cancel.label': window.ResourceManager['Editable_Cancel_Label'],
  	'editable.delete.label': window.ResourceManager['Editable_Delete_Label'],
  	'editable.delete.tooltip.prefix': window.ResourceManager['Editable_Delete_Tooltip_Prefix'],
  	'editable.label': window.ResourceManager['Edit_Label'],
  	'editable.label.prefix': window.ResourceManager['Edit_Label_Prefix'],
  	'editable.loading': window.ResourceManager['Editable_Loading'],
  	'editable.required.label': window.ResourceManager['Editable_Required_Label'],
  	'editable.save.label': window.ResourceManager['Editable_Save_Label'],
  	'editable.saving': window.ResourceManager['Editable_Saving'],
  	'editable.sortable.tooltip': window.ResourceManager['Editable_Sortable_Tooltip'],
  	'editable.tooltip': window.ResourceManager['Editable_Tooltip'],
  	'error.dataservice.default': window.ResourceManager['Error_Dataservice_Default'],
  	'error.dataservice.loading': window.ResourceManager['Error_Dataservice_Loading'],
  	'error.dataservice.loading.field.prefix': window.ResourceManager['Error_Dataservice_Loading_Field_Prefix'],
  	'confirm.delete.entity.prefix': window.ResourceManager['Confirm_Delete_Entity_Prefix'],
  	'confirm.no': window.ResourceManager['Confirm_No'],
  	'confirm.unsavedchanges': window.ResourceManager['Confirm_Unsavedchanges'],
  	'confirm.yes': window.ResourceManager['Confirm_Yes'],
  	'validation.required.suffix': window.ResourceManager['Validation_Required_Suffix'],
  	'sitemapchildren.header.label': window.ResourceManager['Sitemapchildren_Header_Label'],
  	'sitemarker.warningmessage': window.ResourceManager['SiteMarker_WarningMessage'],
  	'sitemarkers.warningmessage': window.ResourceManager['SiteMarkers_WarningMessage'],
  	'validation.partialurl.suffix': window.ResourceManager['Validation_PartialUrl_Suffix'],
  	'validation.childpage.partialurl.notslash.message': window.ResourceManager['Validation_ChildPage_PartialUrl_NotSlash_Message'],
  	'validation.childpage.partialurl.not.unique': window.ResourceManager['Validation_ChildPage_PartialUrl_NotUnique_Message'],
  	'validation.homepage.partialurl.notslash.message': window.ResourceManager['Validation_HomePage_PartialUrl_NotSlash_Message']
  });

  ns.StringFormatter = (function () {
  	function StringFormatter() {
  	}
  	StringFormatter.format = function (theString) {
  		// theString containing the format items (e.g. "{0}").
  		var args = [];
  		for (var _i = 1; _i < arguments.length; _i++) {
  			args[_i - 1] = arguments[_i];
  		}
  		for (var i = 0; i < args.length; i++) {
  			var regEx = new RegExp("\\{" + (i) + "\\}", "gm");
  			theString = theString.replace(regEx, args[i]);
  		}
  		return theString;
  	};
  	return StringFormatter;
  })();

  ns.expandUriTemplate = function (uriTemplate, data) {
    return uriTemplate.replace(/{([^}]+)}/g, function (match, capture) {
      var names = capture.split('.'),
          currentData = data || {},
          i;
      var step = null;
      for (i = 0; i < names.length; i++) {
        step = currentData[names[i]];
      }

      if (typeof step == 'undefined' || step === null) {
        return '';
      }
      return escape(step);
    });
  };

  ns.updateQueryStringParameter = function (uri, key, value) {
    var re = new RegExp("([?&])" + key + "=.*?(&|$)", "i");
    var separator = uri.indexOf('?') !== -1 ? "&" : "?";
    if (uri.match(re)) {
      return uri.replace(re, '$1' + key + "=" + value + '$2');
    } else {
      return uri + separator + key + "=" + value;
    }
  };

  if (typeof (CKEDITOR) !== 'undefined') {
    CKEDITOR.stylesSet.add('cms', [
      { name: window.ResourceManager['CKEditor_Code_Style'], element: 'code' },
      { name: window.ResourceManager['CKEditor_Code_Block_Style'], element: 'pre', attributes: { 'class': 'linenums prettyprint' } }
    ]);

    (function () {
      var appPath = $('[data-app-path]').data('app-path') || '/',
        externalPluginsPath = appPath + 'xrm-adx/js/ckeditor_plugins/';

      CKEDITOR.plugins.addExternal('xrmsave', externalPluginsPath + 'xrmsave/', 'plugin.js');
      CKEDITOR.plugins.addExternal('ace', externalPluginsPath + 'ace/', 'plugin.js');
      CKEDITOR.plugins.addExternal('select2', externalPluginsPath + 'select2/', 'plugin.js');
      CKEDITOR.plugins.addExternal('cmstemplate', externalPluginsPath + 'cmstemplate/', 'plugin.js');
    })();
  }

  XRM.ckeditorSettings = {
    customConfig: '',
    extraPlugins: 'xrmsave,ace,select2,cmstemplate',
    allowedContent: true,
    entities: false,
    height: 400,
    uiColor: '#EEEEEE',
    stylesSet: 'cms',
    disableNativeSpellChecker: false,
    resize_dir: 'both',
    toolbarGroups: [
      { name: 'document', groups: [ 'document', 'doctools', 'mode' ] },
      { name: 'clipboard', groups: [ 'clipboard', 'undo' ] },
      { name: 'editing', groups: [ 'find', 'selection', 'spellchecker', 'editing' ] },
      { name: 'links', groups: [ 'links' ] },
      { name: 'insert', groups: [ 'insert' ] },
      { name: 'forms', groups: [ 'forms' ] },
      { name: 'tools', groups: [ 'tools' ] },
      '/',
      { name: 'basicstyles', groups: [ 'basicstyles', 'cleanup' ] },
      { name: 'paragraph', groups: [ 'list', 'indent', 'blocks', 'align', 'bidi', 'paragraph' ] },
      { name: 'styles', groups: [ 'styles' ] },
      { name: 'colors', groups: [ 'colors' ] },
      { name: 'others', groups: [ 'others' ] },
      { name: 'about', groups: [ 'about' ] }
    ],
    removeButtons: 'Save,NewPage,Preview,Print,Templates,SelectAll,Form,Checkbox,Radio,TextField,Textarea,Select,Button,ImageButton,HiddenField,Smiley,PageBreak,Font,FontSize,CreateDiv,BidiRtl,BidiLtr,Language,About,Subscript,Superscript,Source,Replace'
  };

  XRM.ckeditorCompactSettings = $.extend(true, {}, XRM.ckeditorSettings);
  XRM.ckeditorCompactSettings.height = 240;
  XRM.ckeditorCompactSettings.resize_enabled = false;
  XRM.ckeditorCompactSettings.removeButtons = 'XrmSave,' + XRM.ckeditorCompactSettings.removeButtons;

  XRM.ui.showDataServiceError = function (xhr, message) {
    var xhrs = $.isArray(xhr) ? xhr : [xhr];
    message = message || XRM.localize('error.dataservice.default');

    $.each(xhrs, function (i, item) {
      try {
        var json = JSON.parse(item.responseText);

        if (json && json.error && json.error.message && json.error.message.value) {
          message += '\n\n' + json.error.message.value;
        }

        if (json && json.error && json.error.innererror) {
          if (json.error.innererror.internalexception && json.error.innererror.internalexception.message) {
            message += '\n\n' + json.error.innererror.internalexception.message;
          }
        }
      } catch (e) {
      }
    });

    XRM.ui.showError(message);
  };

});
