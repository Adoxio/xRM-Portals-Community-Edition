/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

  var ns = XRM.namespace('editable.Entity.handlers');
  var Entity = XRM.editable.Entity;
  var $ = XRM.jQuery;
  var yuiSkinClass = XRM.yuiSkinClass;
  var JSON = YAHOO.lang.JSON;
  var hiddenFromSiteMapClass = 'xrm-editable-sitemapchild-hidden';

  XRM.localizations['editable.deletemultiple.tooltip.prefix'] = window.ResourceManager['Editable_DeleteMultiple_Tooltip_Prefix'];
  XRM.localizations['editable.deletemultiple.tooltip.suffix.singular'] = window.ResourceManager['Editable_DeleteMultiple_Tooltip_Suffix_Singular'];
  XRM.localizations['editable.deletemultiple.tooltip.suffix.plural'] = window.ResourceManager['Editable_DeleteMultiple_Tooltip_Suffix_Plural'];
  XRM.localizations['confirm.deletemultiple.entity'] = window.ResourceManager['Confirm_DeleteMultiple_Entity'];
  XRM.localizations['sitemapchildren.update.tooltip'] = window.ResourceManager['SitemapChildren_Update_Tooltip'];
  XRM.localizations['sitemapchildren.hiddenfromsitemap.tooltip'] = window.ResourceManager['SitemapChildren_HiddenFrom_Sitemap_Tooltip'];

  var isSortingEnabled = $.isFunction($.fn.sortable);

  if (!isSortingEnabled) {
    XRM.log('XRM weblinks sorting disabled. Include jQuery UI with Sortable interaction to enable.', 'warn', 'XRM.editable.Entity.handlers.sitemapchildren');
  }

  var self = ns.sitemapchildren = function (entityContainer, editToolbar) {
    var entityServiceRef = $('a.xrm-entity-ref-sitemapchildren', entityContainer);
    var entityServiceUri = entityServiceRef.attr('href');
    var entityTitle = entityServiceRef.attr('title');

    if (!entityServiceUri) {
      XRM.log('Unable to get site map children service URI. Child view will not be editable.', 'warn', 'editable.Entity.handlers.sitemapchildren');
      return;
    }

    if (entityContainer.hasClass('xrm-entity-current')) {
      var module = $('<div />').addClass('xrm-editable-toolbar-module').appendTo(editToolbar.body).get(0);
      var button = new YAHOO.widget.Button({
        container: module,
        label: window.ResourceManager['Cms_Sitemapchildren_Update_Label'],
        title: XRM.localize('sitemapchildren.update.tooltip'),
        onclick: {
          fn: function () {
            var hideProgressPanel = XRM.ui.showProgressPanel(XRM.localize('editable.loading'), {
              close: false,
              draggable: false,
              zindex: XRM.zindex,
              visible: false,
              context: [editToolbar.element, 'tr', 'tl', [], [-10, 0]],
              constraintoviewport: true
            });

            XRM.data.getJSON(entityServiceUri, {
              success: function (data, textStatus) {
                hideProgressPanel();

                self.renderEditDialog(entityContainer, entityServiceUri, entityTitle, data, true, editToolbar);
              },
              error: function (xhr) {
                hideProgressPanel();

                XRM.ui.showDataServiceError(xhr, XRM.localize('error.dataservice.loading'));
              }
            });
          }
        }
      });
    }
    else {
      entityContainer.editable(entityServiceUri, {
        loadSuccess: function (entityData) {
          self.renderEditDialog(entityContainer, entityServiceUri, entityTitle, entityData, false);
        }
      });
    }
  };

  self.renderEditDialog = function (entityContainer, entityServiceUri, entityTitle, entityData, global, editToolbar) {
    var yuiContainer = $('<div />').addClass(yuiSkinClass).appendTo(document.body);
    var dialogContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog xrm-editable-dialog-sitemapchildren').appendTo(yuiContainer);

    var childData = $.isArray(entityData.d) ? entityData.d : [];

    function completeEdit(dialog) {
      dialog.cancel();
      yuiContainer.remove();
    }

    var list = $('<ol />').addClass('xrm-editable-sitemapchildren');

    var dialogOptions = {
      visible: false,
      zindex: XRM.zindex,
      constraintoviewport: true,
      buttons: [{
        text: XRM.localize('editable.save.label'),
        handler: function () {
          var dialog = this;
          var hideProgressPanel = XRM.ui.showProgressPanelAtXY(XRM.localize('editable.saving'), YAHOO.util.Dom.getXY(dialog.id));

          self.saveChildren(childData, list, {
            save: function () {
              hideProgressPanel();
              completeEdit(dialog);
              document.location = document.location;
            },
            cancel: function () {
              hideProgressPanel();
            }
          });
        },
        isDefault: true
      }, {
        text: XRM.localize('editable.cancel.label'),
        handler: function () { completeEdit(this); }
      }
      ]
    };

    if (global && editToolbar) {
      // If the interface is global (i.e., spawned from the global edit toolbar), align it to the left of the toolbar.
      dialogOptions.context = [editToolbar.element, 'tr', 'tl', [], [-10, 0]];
    }
    else {
      dialogOptions.xy = YAHOO.util.Dom.getXY(entityContainer.get(0));
    }

    var dialog = new YAHOO.widget.Dialog(dialogContainer.get(0), dialogOptions);

    dialog.setHeader(XRM.localize('sitemapchildren.header.label') + (entityTitle ? (' (' + entityTitle + ')') : ''));
    dialog.setBody(' ');

    list.appendTo(dialog.body);

    $.each(childData, function (_, child) { self.addChildItem(dialog, list, child, entityContainer); });

    if (isSortingEnabled) {
      list.sortable({ handle: '.xrm-drag-handle', opacity: 0.8 });
    }

    dialog.render();
    if (dialog._aButtons != null) {
        if (dialog._aButtons.length == 2) {
            $('#' + dialog._aButtons[0]._button.id).attr("title", XRM.localize('editable.save.label'));
            $('#' + dialog._aButtons[1]._button.id).attr("title", XRM.localize('editable.cancel.label'));
        }
    }
    dialog.show();
    $(".container-close").attr('title', window.ResourceManager["Close_DefaultText"]);
    XRM.ui.registerOverlay(dialog);
    dialog.focus();
  };

  self.addChildItem = function (dialog, list, child, entityContainer) {
    var item = $('<li />').addClass('xrm-editable-sitemapchild').appendTo(list);

    if (child.HiddenFromSiteMap) {
      item.addClass(hiddenFromSiteMapClass);
    }

    // We'll store any JSON data for updates or creates in this element.
    var childUpdateData = $('<input />').attr('type', 'hidden').addClass('xrm-editable-sitemapchild-data').appendTo(item);

    var entityUri = self.getChildEntityUri(child, entityContainer);
    var isUpdatable = child.HasPermission && !!entityUri;
    var isOrderable = isUpdatable && child.DisplayOrderPropertyName;
    var entityDeleteUri = self.getChildEntityDeleteUri(child, entityContainer);
    var isDeletable = child.HasPermission && !!entityDeleteUri;

    if (isSortingEnabled && isOrderable) {
      $('<a />').attr('title', XRM.localize('editable.sortable.tooltip')).addClass('xrm-drag-handle').appendTo(item);
    }
    else {
      $('<span />').addClass('xrm-drag-disabled').appendTo(item);
    }

    if (isDeletable) {
      $('<a />').attr('title', XRM.localize('entity.delete.' + child.LogicalName + '.tooltip') || 'Delete this item').addClass('xrm-delete').appendTo(item).click(function () {
        item.addClass('deleted').hide('slow');
      });
    }

    if (isUpdatable) {
      var testForm = self.getChildEntityForm(child.LogicalName, entityUri, child, entityContainer, false);

      if (testForm && testForm.valid) {
        $('<a />').attr('title', XRM.localize(child.LogicalName + '.update.tooltip') || 'Edit this item').addClass('xrm-edit').appendTo(item).click(function () {
          var currentChildData = null;

          try {
            var updateJson = childUpdateData.val();

            if (updateJson) {
              currentChildData = JSON.parse(updateJson);
            }
          }
          catch (e) {
            currentChildData = null;
          }

          var showDialog = currentChildData ? Entity.showEntityDialog : Entity.showEditDialog;

          showDialog(self.getChildEntityForm(child.LogicalName, entityUri, currentChildData || {}, entityContainer, false, child), {
            save: function (form, data, options) {
              self.saveChild(item, childUpdateData, form, data, options);
              dialog.focus();
            },
            cancel: function () {
              dialog.focus();
            }
          });
        });
      }
    }

    $('<a />').attr('title', XRM.localize('entity.hiddenfromsitemap.' + child.LogicalName + '.tooltip') || XRM.localize('sitemapchildren.hiddenfromsitemap.tooltip') || '').addClass('xrm-hidden').appendTo(item);

    $('<span />').addClass('name').text(child.Title || '').appendTo(item);

    if (isUpdatable) {
      $('<a />').attr('href', entityUri).addClass('xrm-entity-ref').appendTo(item).hide();
    }

    if (isDeletable) {
      $('<a />').attr('href', entityDeleteUri).addClass('xrm-entity-delete-ref').appendTo(item).hide();
    }

    $('<span />').addClass('xrm-entity-id').text(child.Id || '').appendTo(item).hide();
  };

  self.saveChild = function (childUIItem, childUpdateData, form, data, options) {
    childUpdateData.val(JSON.stringify(data));

    // Find all valid file upload fields.
    var fileUploads = $.map(form.fields, function (field) {
      return (field.type === 'file' && (field.fileUploadUri || field.fileUploadUriTemplate))
        ? new Entity.Form.fieldTypes.file(form.entityName, field)
        : null;
    });

    // Store them in the child DOM for upload during the save process.
    $.each(fileUploads, function (_, fileUpload) {
      var fileElement = $('#' + fileUpload.id);
      if (fileElement.val()) {
        var updateID = fileUpload.id + '-update';
        $('#' + updateID).remove();
        fileElement.hide().appendTo(childUIItem).addClass('xrm-file-update').attr('id', updateID).data('xrm-field', fileUpload);
      }
    });

    // If the title of the entity has been updated, try to update the item in the child list
    // accordingly. Since we can't really know how the sitemapnode Title is computed on the
    // server, we'll just make a best-effort guess based on how our entity schemas normally
    // work.
    var updatedName = null;

    $.each(['adx_title', 'adx_name'], function (_, possibleNameAttribute) {
      var propertyName = Entity.getPropertyName(form.entityName, possibleNameAttribute);

      if (!propertyName) return true;

      updatedName = data[propertyName];

      if (updatedName) return false;
    });

    var hiddenFromSiteMapPropertyName = Entity.getPropertyName(form.entityName, 'adx_hiddenfromsitemap');

    if (hiddenFromSiteMapPropertyName) {
      if (data[hiddenFromSiteMapPropertyName] === false && childUIItem.hasClass(hiddenFromSiteMapClass)) {
        childUIItem.removeClass(hiddenFromSiteMapClass);
      }

      if (data[hiddenFromSiteMapPropertyName] === true && !childUIItem.hasClass(hiddenFromSiteMapClass)) {
        childUIItem.addClass(hiddenFromSiteMapClass);
      }
    }

    if (updatedName) {
      $('.name', childUIItem).text(updatedName);
    }

    if ($.isFunction(options.success)) {
      options.success();
    }
  };

  self.getChildEntityForm = function (childEntityName, childEntityUri, childData, entityContainer, forCreate) {
    childData = childData || {};

    var handler = ns[childEntityName];

    if (!(handler && handler.formPrototype)) {
      return null;
    }

    var form = $.extend(true, {}, handler.formPrototype);

    form.uri = childEntityUri;
    form.title = XRM.localize(childEntityName + '.update.tooltip');
    form.valid = !!form.uri;

    if (arguments.length > 4) {
        var child = arguments[5];
        if (!$.isEmptyObject(child) && $.type(form.current) === "undefined") {
            var logicalName = child.LogicalName;
            var id = child.Id;
            form.current = { LogicalName: logicalName, Id: id };
            if (!$.type(arguments[6]) !== "undefined" && !$.isEmptyObject(childData) ) {
                form.current.optionsEdit =  true;
            }
            
        }
    }

    $.each(form.fields, function (i, field) {
      var propertyName = Entity.getPropertyName(form.entityName, field.name);

      if (!propertyName) {
        return;
      }

      var propertyData = childData[propertyName];

      if (typeof (propertyData) === 'undefined') {
        return;
      }

      field.value = propertyData;
    });

    var fieldsMissingServiceUri = [];

    $.each(form.fields, function (i, field) {
      if (!forCreate && field.labelOnUpdate) {
        field.label = XRM.localize(form.entityName + '.' + field.name + '.onupdate') || field.labelOnUpdate || field.label || field.name;
      }
      else {
        field.label = XRM.localize(form.entityName + '.' + field.name) || field.label || field.name;
      }

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

      if (!forCreate && field.requiredOnUpdate === false) {
        field.required = false;
      }
    });

    $.each(fieldsMissingServiceUri, function () {
      var field = this;

      form.fields.splice(form.fields.indexOf(field), 1);
    });

    return form;
  };

  self.getChildEntityUri = function (child, entityContainer) {
    var entityUriTemplate = $('.xrm-uri-template.xrm-entity-' + child.LogicalName + '-ref', entityContainer).attr('href');

    if (!entityUriTemplate) {
      return null;
    }

    return XRM.util.expandUriTemplate(entityUriTemplate, child);
  };

  self.getChildEntityDeleteUri = function (child, entityContainer) {
    var entityUriTemplate = $('.xrm-uri-template.xrm-entity-' + child.LogicalName + '-delete-ref', entityContainer).attr('href');

    if (!entityUriTemplate) {
      return null;
    }

    return XRM.util.expandUriTemplate(entityUriTemplate, child);
  };

  self.saveChildren = function (childData, childContainer, options) {
    var onSave = $.isFunction(options.save) ? options.save : function () { },
        onCancel = $.isFunction(options.cancel) ? options.cancel : function () { },
        childMap = {},
        deletes = [],
        operations = [];

    $.each(childData, function (_, child) {
      if (child.Id) {
        childMap[child.Id] = child;
      }
    });

    // Queue any delete operations.
    $('.xrm-editable-sitemapchild.deleted', childContainer).each(function (index, item) {
      var deleteUri = $('.xrm-entity-delete-ref', item).attr('href');

      if (!deleteUri) {
        return;
      }

      deletes.push({ name: $('.name', item).text() });

      operations.push(function () {
        var deferredOperation = $.Deferred();

        XRM.data.postJSON(deleteUri, null, {
          success: function (data, textStatus) {
            deferredOperation.resolve();
          },
          error: function (xhr) {
            deferredOperation.resolve();
          }
        });

        return deferredOperation.promise();
      });
    });

    // Queue any update operations.
    $('.xrm-editable-sitemapchild:not(.deleted)', childContainer).each(function (index, item) {
      var entityUri = $('.xrm-entity-ref', item).attr('href');

      if (!entityUri) {
        return;
      }

      var child = childMap[$('.xrm-entity-id', item).text()];

      if (!child) {
        return;
      }

      var displayOrder = (index + 1),
          data = {},
          isUpdated = false;

      var updateJson = $('.xrm-editable-sitemapchild-data', item).val();

      if (updateJson) {
        try {
          data = JSON.parse(updateJson);
          isUpdated = !!data;
        }
        catch (e) { }
      }

      // If the child is orderable and its display order has changed, update the display order.
      if (child.DisplayOrderPropertyName && child.DisplayOrder != displayOrder) {
        data[child.DisplayOrderPropertyName] = displayOrder;
        isUpdated = true;
      }

      if (isUpdated) {
        operations.push(function () {
          var deferredOperation = $.Deferred();

          Entity.saveEditDialog({ uri: entityUri }, data, {
            disableFileUploads: true,
            disableProgress: true,
            success: function () {
              deferredOperation.resolve();
            },
            error: function () {
              deferredOperation.resolve();
            }
          });

          return deferredOperation.promise();
        });
      }

      $('.xrm-file-update', item).each(function () {
        var fileUpdate = $(this);
        var field = fileUpdate.data('xrm-field');

        if (!field) {
          return;
        }

        fileUpdate.attr('id', field.id);

        operations.push(function () {
          var deferredOperation = $.Deferred();

          field.upload({ d: child }, function () {
            deferredOperation.resolve();
          });

          return deferredOperation.promise();
        });
      });
    });

    function doSave() {
      $.when.apply($, $.map(operations, function (operation) { return operation(); }))
        .then(onSave);
    }

    if (deletes.length < 1) {
      doSave();
      return;
    }

    self.showDeletionConfirmationDialog(deletes, doSave, function () { onCancel(); });
  };

  self.showDeletionConfirmationDialog = function (deletions, onYes, onNo) {
    var dialogContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog').appendTo(document.body);

    function closeDialog(dialog) {
      dialog.cancel();
      dialogContainer.remove();
      $(document.body).removeClass(yuiSkinClass);
    }

    function handleYes() {
      closeDialog(this);
      onYes(deletions);
    }

    function handleNo() {
      closeDialog(this);
      onNo(deletions);
    }

    var dialog = new YAHOO.widget.SimpleDialog(dialogContainer.get(0), {
      fixedcenter: true,
      visible: false,
      draggable: false,
      close: false,
      modal: true,
      icon: YAHOO.widget.SimpleDialog.ICON_WARN,
      constraintoviewport: true,
      buttons: [{ text: XRM.localize('confirm.yes'), handler: handleYes, isDefault: true }, { text: XRM.localize('confirm.no'), handler: handleNo }]
    });

    dialog.setHeader(' ');
    dialog.setBody(' ');

    var header = XRM.localize('editable.deletemultiple.tooltip.prefix') +
      deletions.length +
      XRM.localize('editable.deletemultiple.tooltip.suffix.' + (deletions.length > 1 ? 'plural' : 'singular'));

    $('<span />').text(header).appendTo(dialog.header);
    $('<p />').text(XRM.localize('confirm.deletemultiple.entity')).appendTo(dialog.body);
    var names = $('<ul />').addClass('xrm-editable-sitemapchildren-deletions').appendTo(dialog.body);

    $.each(deletions, function (_, deletion) {
      $('<li />').text(deletion.name).appendTo(names);
    });

    $(document.body).addClass(yuiSkinClass);

    dialog.render();
    dialog.show();
    XRM.ui.registerOverlay(dialog);
    dialog.focus();
  };
});
