/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

  var ns = XRM.namespace('editable.Attribute.handlers');
  var Attribute = XRM.namespace('editable.Attribute');
  var $ = XRM.jQuery;
  var yuiSkinClass = XRM.yuiSkinClass;
  var JSON = YAHOO.lang.JSON;

  XRM.localizations['filebrowser.header.label'] = window.ResourceManager['FileBrowser_Header_Label'];

  function renderHtmlEditor(attributeContainer, attributeDisplayName, attributeName, attributeValue, editCompleteCallback, save) {
    var yuiContainer,
      panelContainer,
      editPanel,
      textarea,
      ckeditorSettings,
      cmsTemplateUrl,
      cmsTemplateRenderUrl,
      fileBrowserServiceUri,
      fileBrowserDialogUri,
      editor,
      resizeEditPanel,
      liquid,
      encoded,
      language;
    
    yuiContainer = $('<div />').addClass(yuiSkinClass).appendTo(document.body);
    panelContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog xrm-editable-dialog-html').appendTo(yuiContainer);
    language = attributeContainer.attr('data-languageContext');

    editPanel = new YAHOO.widget.Panel(panelContainer.get(0), {
      close: true,
      draggable: true,
      constraintoviewport: true,
      visible: false,
      zindex: XRM.zindex,
      xy: YAHOO.util.Dom.getXY(attributeContainer.get(0))
    });

    var headerText = XRM.localize('editable.label.prefix') + attributeDisplayName;
    if (language) {
    	headerText += window.ResourceManager['Language_Label_With_Language'].replace('{0}', language);
    }
    editPanel.setHeader(headerText);
    editPanel.setBody(' ');
    
    // Create the textarea that will host our content, and that CKEditor will latch on to.
    textarea = $('<textarea />').text(attributeValue || '').appendTo(editPanel.body);

    ckeditorSettings = $.extend(true, {}, XRM.ckeditorSettings);

    // ckeditor only supports the CRM languages
    ckeditorSettings.language = $('html').attr('crm-lang');

    cmsTemplateUrl = attributeContainer.data('cmstemplate-url');

    if (cmsTemplateUrl) {
    	ckeditorSettings.cmstemplates = cmsTemplateUrl;
    }

    // Set the textarea to the approximate height of CKEditor (content height + approximate toolbar height), purely
    // so that the editor dialog's auto-centering/sizing logic will take into account the editors eventual height.
    // The textarea will be hidden, so its height otherwise doesn't matter.
    if (ckeditorSettings.height) {
      textarea.height((parseInt(ckeditorSettings.height) || 0) + 100);
    }

    cmsTemplateRenderUrl = attributeContainer.data('cmstemplate-render-url');
    encoded = attributeContainer.attr('data-encoded') == 'true';
    liquid = attributeContainer.attr('data-liquid') == 'true';

    fileBrowserServiceUri = attributeContainer.data('filebrowser-url') || $('a.xrm-filebrowser-ref', attributeContainer).attr('href');
    fileBrowserDialogUri = attributeContainer.data('filebrowser-dialog-url') || $('a.xrm-filebrowser-dialog-ref', attributeContainer).attr('href');

    if (fileBrowserServiceUri && fileBrowserDialogUri) {
      ckeditorSettings.filebrowserBrowseUrl = XRM.util.updateQueryStringParameter(fileBrowserDialogUri, 'url', encodeURIComponent(fileBrowserServiceUri));
    }

    editPanel.render();

    editor = CKEDITOR.replace(textarea.get(0), ckeditorSettings);

    resizeEditPanel = function () {
      editPanel.cfg.setProperty('width', $(editor.container.$).width() + 'px');
    };

    editor.on('instanceReady', function (evt) {
      evt.editor.focus();
    });

    editor.on('focus', function () {
      editPanel.focus();
    });

    editor.on('resize', resizeEditPanel);

    editPanel.beforeHideEvent.subscribe(function () {
      if (editor.checkDirty()) {
        if (!confirm(XRM.localize('confirm.unsavedchanges'))) {
          return false;
        }
      }
    });
    
    editPanel.hideEvent.subscribe(function () {
      editor.destroy();
      yuiContainer.remove();
    });

    // CRTL + s to save
    editor.setKeystroke(CKEDITOR.CTRL + 83, 'xrmsave');

    editor.on('save', function (evt) {
      var editor = evt.editor,
        content = editor.getData();

      editor.setReadOnly(true);

      function complete() {
        editor.setReadOnly(false);
        editPanel.hide();

        if ($.isFunction(editCompleteCallback)) {
          editCompleteCallback();
        }
      }

      if (!editor.checkDirty()) {
        complete();
      }

      save(content, {
        success: function () {
          editor.resetDirty();

          function notNullorWhitespace(text) {
            return text && text.length > 0 && /\S/.test(text);
          }

          function setHtml(html) {
            if (encoded) {
              $('.xrm-attribute-value', attributeContainer).text(html);
            } else {
              $('.xrm-attribute-value', attributeContainer).html(html);
            }

            if (notNullorWhitespace(html)) {
              attributeContainer.removeClass('no-value');
            } else {
              attributeContainer.addClass('no-value');
            }
          }

          if (liquid && cmsTemplateRenderUrl) {
              shell.ajaxSafePost({
                method: 'POST',
                url: cmsTemplateRenderUrl,
                contentType: "application/json",
                data: JSON.stringify({ source: content })
              })
              .done(function (data) {
                setHtml(data);
              })
              .fail(function () {
                setHtml(content);
              })
              .always(function () {
                complete();
              });
          } else {
            setHtml(content);
            complete();
          }
        },
        error: function (xhr) {
          editor.setReadOnly(false);
          XRM.ui.showDataServiceError(xhr);
        }
      });
    });

    editPanel.show();
    XRM.ui.registerOverlay(editPanel);
    editPanel.focus();
  }

  ns.html = function (attributeContainer, attributeDisplayName, attributeName, attributeValue, entityServiceUri, editCompleteCallback) {
    if (typeof (CKEDITOR) === 'undefined') {
      ns.textarea(attributeContainer, attributeDisplayName, attributeName, attributeValue, entityServiceUri, editCompleteCallback);
    } else {
      renderHtmlEditor(attributeContainer, attributeDisplayName, attributeName, attributeValue, editCompleteCallback, function (content, options) {
        XRM.data.services.putAttribute(entityServiceUri, attributeName, content, {
          success: options.success,
          error: options.error
        });
      });
    }
  };

  ns.html_create = function (attributeContainer, attributeDisplayName, attributeName, createInitialData, createUri, editCompleteCallback) {
    if (typeof (CKEDITOR) === 'undefined') {
      ns.textarea_create(attributeContainer, attributeDisplayName, createUri, createInitialData, createAttribute, editCompleteCallback);
    } else {
      renderHtmlEditor(attributeContainer, attributeDisplayName, attributeName, attributeContainer.data('default') || '', editCompleteCallback, function (content, options) {
        var createData = createInitialData || {};

        createData[attributeName] = content;

        XRM.data.postJSON(createUri, createData, {
          processData: true,
          success: function (data) {
            var serviceUriTemplate = attributeContainer.data('editable-uritemplate');

            attributeContainer.noneditable();

            if (serviceUriTemplate && data && data.d) {
              attributeContainer.removeAttr('data-create-url').data('create-url', null);
              attributeContainer.attr('data-editable-url', XRM.util.expandUriTemplate(serviceUriTemplate, data.d));
              Attribute.initializeAttribute(attributeContainer);
            }

            if ($.isFunction(options.success)) {
              options.success(data);
            }
          },
          error: options.error
        });
      });
    }
  };

});
