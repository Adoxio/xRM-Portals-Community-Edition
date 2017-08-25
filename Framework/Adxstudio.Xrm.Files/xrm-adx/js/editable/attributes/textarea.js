/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {

  var ns = XRM.namespace('editable.Attribute.handlers');
  var Attribute = XRM.namespace('editable.Attribute');
  var $ = XRM.jQuery;
  var yuiSkinClass = XRM.yuiSkinClass;
  var JSON = YAHOO.lang.JSON;

  function renderTextEditor(attributeContainer, attributeDisplayName, attributeName, attributeValue, editCompleteCallback, dialogClass, save) {
    var yuiContainer,
      dialogContainer,
      editDialog,
      cmsTemplateRenderUrl,
      liquid,
      encoded,
      language;

    // Build the DOM necessary to support our UI.
    yuiContainer = $('<div />').addClass(yuiSkinClass).appendTo(document.body);
    dialogContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog').addClass(dialogClass).appendTo(yuiContainer);

    cmsTemplateRenderUrl = attributeContainer.data('cmstemplate-render-url');
    encoded = attributeContainer.attr('data-encoded') == 'true';
    liquid = attributeContainer.attr('data-liquid') == 'true';
    language = attributeContainer.attr('data-languageContext');

    function completeEdit(dialog) {
      dialog.cancel();
      yuiContainer.remove();

      if ($.isFunction(editCompleteCallback)) {
        editCompleteCallback();
      }
    }

    function handleCancel(dialog) {
      completeEdit(dialog);
    }

    function handleSave(dialog) {
      var dialogInput = $('.xrm-text', dialog.body);
      var dialogInputValue = dialogInput.val();
      var dialogFooter = $(dialog.footer);

      // If the attribute value has been changed, persist the new value.
      if (dialogInputValue != attributeValue) {
        dialogFooter.hide();
        dialogInput.hide();
        dialogContainer.addClass('xrm-editable-wait');
        save(dialogInputValue, {
          success: function () {
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
              $.ajax({
                method: 'POST',
                url: cmsTemplateRenderUrl,
                contentType: "application/json",
                data: JSON.stringify({ source: dialogInputValue })
              })
                .done(function (data) {
                  setHtml(data);
                })
                .fail(function () {
                  setHtml(dialogInputValue);
                })
                .always(function () {
                  completeEdit(dialog);
                });
            } else {
              setHtml(dialogInputValue);
              completeEdit(dialog);
            }
          },
          error: function (xhr) {
            dialogContainer.removeClass('xrm-editable-wait');
            dialogFooter.show();
            dialogInput.show();
            XRM.ui.showDataServiceError(xhr);
          }
        });
      }
        // Otherwise, just dismiss the edit dialog without doing anything.
      else {
        completeEdit(dialog);
      }
    }

    // Create our modal editing dialog.
    editDialog = new YAHOO.widget.Dialog(dialogContainer.get(0), {
      visible: false,
      constraintoviewport: true,
      zindex: XRM.zindex,
      xy: YAHOO.util.Dom.getXY(attributeContainer.get(0)),
      buttons: [
        { text: XRM.localize('editable.save.label'), handler: function () { handleSave(this); }, isDefault: true },
        { text: XRM.localize('editable.cancel.label'), handler: function () { handleCancel(this); } }
      ]
    });

    var headerText = XRM.localize('editable.label.prefix') + attributeDisplayName;
    if (language) {
    	headerText += window.ResourceManager['Language_Label_With_Language'].replace('{0}', language);
    }

    editDialog.setHeader(headerText);
    editDialog.setBody(' ');

    $('<textarea />').addClass('xrm-text').val(attributeValue || '').appendTo(editDialog.body);

    // Add ctrl+s shortcut for saving content.
    $('.xrm-text', editDialog.body).keypress(function (e) {
      if (!(e.which == ('s').charCodeAt(0) && e.ctrlKey)) {
        return true;
      }
      handleSave(editDialog);
      return false;
    });

    editDialog.render();
    editDialog.show();

    XRM.ui.registerOverlay(editDialog);
    editDialog.focus();

    $('.xrm-text', editDialog.body).focus();
  }

  function createTextareaAttributeHandler(dialogClass) {
    return function (attributeContainer, attributeDisplayName, attributeName, attributeValue, entityServiceUri, editCompleteCallback) {
      renderTextEditor(attributeContainer, attributeDisplayName, attributeName, attributeValue, editCompleteCallback, dialogClass, function (content, options) {
        XRM.data.services.putAttribute(entityServiceUri, attributeName, content, {
          success: options.success,
          error: options.error
        });
      });
    }
  }

  function createTextareaAttributeCreateHandler(dialogClass) {
    return function (attributeContainer, attributeDisplayName, attributeName, createInitialData, createUri, editCompleteCallback) {
      renderTextEditor(attributeContainer, attributeDisplayName, attributeName, attributeContainer.data('default') || '', editCompleteCallback, dialogClass, function (content, options) {
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
  }

  ns.textarea = createTextareaAttributeHandler('xrm-editable-dialog-html-fallback');
  ns.textarea_create = createTextareaAttributeCreateHandler('xrm-editable-dialog-html-fallback');
  ns.text = createTextareaAttributeHandler('xrm-editable-dialog-textarea');
  ns.text_create = createTextareaAttributeCreateHandler('xrm-editable-dialog-textarea');

});
