/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

  var ns = XRM.namespace('editable.Entity.handlers');
  var Entity = XRM.editable.Entity;
  var Handler = Entity.Handler;
  var $ = XRM.jQuery;
  var yuiSkinClass = XRM.yuiSkinClass;
  var JSON = YAHOO.lang.JSON;
  
  XRM.localizations['entity.create.adx_weblinkset.tooltip'] = window.ResourceManager['Entity_Create_ADX_Weblinkset_Tooltip'];
  XRM.localizations['adx_weblink.displaypagechildlinks.tooltip'] = window.ResourceManager['ADX_Weblink_DisplayPage_Childlinks_Tooltip'];

  var isSortingEnabled = $.isFunction($.fn.sortable);

  if (!isSortingEnabled) {
    XRM.log('XRM weblinks sorting disabled. Include jQuery UI with Sortable interaction to enable.', 'warn', 'XRM.editable.Entity.handlers.adx_weblinkset');
  }

  var self = ns.adx_weblinkset = function(entityContainer, editToolbar) {
    var entityServiceRef = $('a.xrm-entity-ref', entityContainer);
    var entityServiceUri = entityServiceRef.attr('href');

    if (!entityServiceUri) {
      return XRM.log('Unable to get weblink set service URI. Web links will not be editable.', 'warn', 'editable.Entity.handlers.adx_weblinkset');
    }

    var weblinksServiceUri = $('a.xrm-entity-adx_weblinkset_weblink-ref', entityContainer).attr('href');
    var saveWebLinksServiceUri = $('a.xrm-entity-adx_weblinkset_weblink-update-ref', entityContainer).attr('href');

    if (!weblinksServiceUri) {
      return XRM.log('Unable to get child weblinks service URI. Web links will not be editable.', 'warn', 'editable.Entity.handlers.adx_weblinkset');
    }

    self.addClassIfEmpty(entityContainer);

    entityContainer.editable(weblinksServiceUri, {
      loadSuccess: function(entityData) {
        self.renderEditDialog(entityContainer, editToolbar, weblinksServiceUri, saveWebLinksServiceUri, entityServiceRef.attr('title'), entityData);
      }
    });
  };

  self.addClassIfEmpty = function(entityContainer) {
    if (entityContainer.children('ul, .weblinks').children('li, .weblink').length == 0) {
      entityContainer.addClass('xrm-entity-value-empty');
    }
  };

  self.setformPrototype = {
    uri: null,
    urlServiceUri: null,
    urlServiceUriTemplate: null,
    title: null,
    entityName: 'adx_weblinkset',
    fields: [
      { name: 'adx_name', label: window.ResourceManager['Name_Label'], type: 'text', required: true  }
    ]
  };

  self.getForm = function (entityContainer, options) {
    return Handler.getForm(self.setformPrototype, entityContainer, options);
  };

  self.formPrototype = {
    title: null,
    entityName: 'adx_weblink',
    fields: [
      { name: 'adx_name', label: window.ResourceManager['Name_Label'], type: 'text', required: true  },
      { name: 'adx_pageid', label: window.ResourceManager['Page_Label'], type: 'select', excludeEmptyData: false, uri: null, optionEntityName: 'adx_webpage', optionText: 'adx_name', optionValue: 'adx_webpageid', sortby: 'adx_name' },
      { name: 'adx_externalurl', label: window.ResourceManager['External_URL_Label'], type: 'text' },
      { name: 'adx_description', label: window.ResourceManager['Description_Label'], type: 'html', ckeditorSettings: { height: 240 }  },
      { name: 'adx_publishingstateid', label: window.ResourceManager['Publishing_State_Label'], type: 'select', excludeEmptyData: true, required: true, uri: null, optionEntityName: 'adx_publishingstate', optionText: 'adx_name', optionValue: 'adx_publishingstateid', sortby: 'adx_name'  },
      { name: 'adx_imageurl', label: window.ResourceManager['Image_URL_Label'], type: 'text'   },
      { name: 'adx_imageheight', label: window.ResourceManager['Image_Height_Label'], type: 'integer'  },
      { name: 'adx_imagewidth', label: window.ResourceManager['Image_Width_Label'], type: 'integer'  },
      { name: 'adx_imagealttext', label: window.ResourceManager['Image_Alternate_Text_Label'], type: 'text'  },
      { name: 'adx_robotsfollowlink', label: window.ResourceManager['Robots_Follow_Link_Label'], type: 'checkbox', value: true   },
      { name: 'adx_openinnewwindow', label: window.ResourceManager['Open_In_New_Window_Label'], type: 'checkbox'    },
      { name: 'adx_disablepagevalidation', label: window.ResourceManager['Disable_Page_Validation_Label'], type: 'checkbox'   },
      { name: 'adx_displayimageonly', label: window.ResourceManager['Display_Image_Only_Label'], type: 'checkbox'  },
      { name: 'adx_displaypagechildlinks', label: window.ResourceManager['Display_Page_Child_Links_Label'], type: 'checkbox' }
    ],
    layout: {
      cssClass: 'xrm-dialog-expanded',
      full: true,
      columns: [
        { cssClass: 'xrm-dialog-column-main', fields: ['adx_name', 'adx_pageid', 'adx_externalurl', 'adx_description'] },
        { cssClass: 'xrm-dialog-column-side', fields: ['adx_publishingstateid', 'adx_imageurl', 'adx_imageheight', 'adx_imagewidth', 'adx_imagealttext', 'adx_displayimageonly', 'adx_robotsfollowlink', 'adx_openinnewwindow', 'adx_disablepagevalidation', 'adx_displaypagechildlinks'] }
      ]
    }
  };

  self.getWeblinkForm = function(title, weblinkData, entityContainer) {
    weblinkData = weblinkData || {};

    var form = $.extend(true, {}, self.formPrototype);

    form.title = title;
    form.valid = true;

    $.each(form.fields, function(i, field) {
      var propertyName = self.getWeblinkPropertyName(field.name);

      if (!propertyName) return;

      var propertyData = weblinkData[propertyName];

      if (typeof(propertyData) === 'undefined') return;

      field.value = propertyData;
    });

    $.each(form.fields, function() {
      var field = this, data;

      field.label = XRM.localize(form.entityName + '.' + field.name) || field.label || field.name;

      if (field.type === 'select') {
        data = $('a.xrm-entity-' + field.optionEntityName + '-ref', entityContainer).attr('href');

        if (!data) {
          form.valid = false;
        }

        field.uri = data;
      }

      if (this.type === 'file') {
        data = $('a.xrm-uri-template.xrm-entity-' + field.name + '-ref', entityContainer).attr('href');

        if (!data) {
          form.valid = false;
        }

        field.fileUploadUriTemplate = data;
      }
    });
    
    $.each(form.fields, function () {
      var field = this;

      Entity.configureDateTimeField(field, entityContainer);

      if (field.type === 'html') {
        field.cmsTemplateUrl = entityContainer.data('cmstemplate-url');
        field.fileBrowserServiceUri = entityContainer.data('filebrowser-url') || $('a.xrm-filebrowser-ref', entityContainer).attr('href');
        field.fileBrowserDialogUri = entityContainer.data('filebrowser-dialog-url') || $('a.xrm-filebrowser-dialog-ref', entityContainer).attr('href');
      }

      if (field.type === 'iframe') {
        field.src = field.src || (entityContainer.closest('[data-xrm-base]').attr('data-xrm-base') || '') + field.xrmsrc;
      }
    });

    return form;
  };

  self.getWeblinkPropertyName = function(propertySchemaName) {
    return Entity.getPropertyName('adx_weblink', propertySchemaName);
  };

  self.renderEditDialog = function(entityContainer, editToolbar, weblinksServiceUri, saveWebLinksServiceUri, entityTitle, entityData) {
    var yuiContainer = $('<div />').addClass(yuiSkinClass).appendTo(document.body);
    var dialogContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog xrm-editable-dialog-weblinkset').appendTo(yuiContainer);

    var webLinkData = $.isArray(entityData.d) ? entityData.d : [];

    function completeEdit(dialog) {
      dialog.cancel();
      yuiContainer.remove();
    }

    var dialog = new YAHOO.widget.Dialog(dialogContainer.get(0), {
      visible: false,
      constraintoviewport: true,
      zindex: XRM.zindex,
      xy: YAHOO.util.Dom.getXY(entityContainer.get(0)),
      buttons: [
        {
          text: XRM.localize('editable.save.label'),
          handler: function() {
            var dialog = this;
            var hideProgressPanel = XRM.ui.showProgressPanelAtXY(XRM.localize('editable.saving'), YAHOO.util.Dom.getXY(dialog.id));
            var weblinksContainer = $('.xrm-editable-weblinks', dialog.body);
            self.saveWebLinks(webLinkData, weblinksContainer, saveWebLinksServiceUri, function(successfulOperations, failedOperations) {
              hideProgressPanel();

              if (failedOperations.length < 1) {
                completeEdit(dialog);
              } else {
                XRM.ui.showDataServiceError($.map(failedOperations, function(operation) { return operation.xhr; }));
              }

              // Instead of updating the DOM in-place, just refresh the page.
              document.location = document.location;
            });
          },
          isDefault: true
        },
        {
          text: XRM.localize('editable.cancel.label'),
          handler: function() { completeEdit(this); }
        }
      ]
    });

    dialog.setHeader(XRM.localize('editable.label.prefix') + (entityTitle || ''));
    dialog.setBody('<ol class="xrm-editable-weblinks xrm-editable-weblinks-sortable"></ol>');

    var displayOrderPropertyName = self.getWeblinkPropertyName('adx_displayorder');

    // Sort weblinks by display order and render them.
    webLinkData.sort(function(a, b) {
      var aOrder = a[displayOrderPropertyName], bOrder = b[displayOrderPropertyName];
      if (aOrder < bOrder) return -1;
      if (aOrder == bOrder) return 0;
      return 1;
    });

    var list = $('.xrm-editable-weblinks', dialog.body);
    
    var parentPropertyName = self.getWeblinkPropertyName('adx_parentweblinkid');
    
    var rootWebLinks = $.grep(webLinkData, function(e) {
      return !(e[parentPropertyName]);
    });

    $.each(rootWebLinks, function(index, weblink) {
      self.addWebLinkItem(dialog, list, weblink, entityContainer, webLinkData);
    });

    var testForm = self.getWeblinkForm('', {}, entityContainer);

    if (testForm.valid) {
      
      var getId = (function () {
        var id = 1;
        return function () {
          return id++;
        };
      })();
      
      var weblinkCreationLink = $('<a />').attr('title', XRM.localize('entity.create.adx_weblink.tooltip')).addClass('xrm-add').insertAfter(list).append('<span />');
      
      weblinkCreationLink.click(function() {
        var form = self.getWeblinkForm(XRM.localize('entity.create.adx_weblink.tooltip'), {}, entityContainer);

        // Show an entity creation dialog for the web link, but override the default save process of
        // the form, so that instead of POSTing to the service on submission, just grab the JSON for
        // the new web link and stuff it into the DOM for later (when the whole set is saved).
        Entity.showEntityDialog(form, {
          save: function(form, data, options) {
            self.addWebLinkItem(dialog, list, data, entityContainer).addClass('created').attr('data-created-id', getId()).hide().show('slow');

            if ($.isFunction(options.success)) {
              options.success();
            }

            dialog.focus();
          },
          cancel: function() {
            dialog.focus();
          }
        });
      });
    }
    
    var maxDepth = parseInt(entityContainer.attr('data-weblinks-maxdepth')) || 1;
    
    if (isSortingEnabled) {
      list.sortable({
        connectWith: '.xrm-editable-weblinks-sortable',
        handle: '.xrm-drag-handle',
        opacity: 0.8,
        placeholder: 'xrm-drag-placeholder',
        start: function(event, ui) {
          $('.xrm-editable-weblink:not(.xrm-displaypagechildlinks) .xrm-editable-child-weblinks:empty', dialog.body).each(function () {
            var itemContainer = $(this);
            var depth = itemContainer.parentsUntil('.xrm-editable-weblinks', '.xrm-editable-child-weblinks').length + 1;

            if (maxDepth < 1 || depth < maxDepth) {
              itemContainer.addClass('xrm-drag-target');
            }
          });
        },
        stop: function(event, ui) {
          $('.xrm-editable-child-weblinks', dialog.body).removeClass('xrm-drag-target');
        }
      });
    }

    dialog.render();
    dialog.show();

    XRM.ui.registerOverlay(dialog);
    dialog.focus();
  };

  self.addWebLinkItem = function (dialog, list, weblink, entityContainer, weblinkData) {
    var item = $('<li />').addClass('xrm-editable-weblink').appendTo(list);
    var inner = $('<div />').addClass('xrm-editable-weblink-inner').appendTo(item);
    var displayPageChildLinksPropertyName = self.getWeblinkPropertyName('adx_displaypagechildlinks');
    
    // We'll store any JSON data for weblink updates or creates in this element.
    var weblinkUpdateData = $('<input />').attr('type', 'hidden').addClass('xrm-editable-weblink-data').appendTo(item);

    if (isSortingEnabled) {
      $('<a />').attr('title', XRM.localize('editable.sortable.tooltip')).addClass('xrm-drag-handle').appendTo(inner);
    } else {
      $('<span />').addClass('xrm-drag-disabled').appendTo(item);
    }
    
    $('<a />').attr('title', XRM.localize('entity.delete.adx_weblink.tooltip')).addClass('xrm-delete').appendTo(inner).click(function() {
      item.addClass('deleted').hide('slow');
      item.find('.xrm-editable-weblink').addClass('deleted');
    });
    
    var weblinkName = $('<span />').addClass('name').text(weblink[self.getWeblinkPropertyName('adx_name')]);

    var testForm = self.getWeblinkForm(XRM.localize('adx_weblink.update.tooltip'), {}, entityContainer);

    if (testForm.valid) {
      $('<a />').attr('title', XRM.localize('adx_weblink.update.tooltip')).addClass('xrm-edit').appendTo(inner).click(function() {

        var currentWeblinkData = weblink;

        try {
          var updateJson = weblinkUpdateData.val();

          if (updateJson) {
            currentWeblinkData = JSON.parse(updateJson);
          }
        } catch(e) {
        }

        Entity.showEntityDialog(self.getWeblinkForm(XRM.localize('adx_weblink.update.tooltip'), currentWeblinkData, entityContainer), {
          save: function(form, data, options) {
            if (!weblinkUpdateData.hasClass('create')) {
              weblinkUpdateData.addClass('update');
            }
            
            if (displayPageChildLinksPropertyName && data[displayPageChildLinksPropertyName]) {
              item.addClass('xrm-displaypagechildlinks');
              if (inner.find('.xrm-displaypagechildlinks-icon').length < 1) {
                $('<span />').attr('title', XRM.localize('adx_weblink.displaypagechildlinks.tooltip')).addClass('xrm-displaypagechildlinks-icon').appendTo(inner);
              }
            } else {
              item.removeClass('xrm-displaypagechildlinks');
              inner.find('.xrm-displaypagechildlinks-icon').remove();
            }

            var namePropertyName = self.getWeblinkPropertyName('adx_name');

            if (namePropertyName && data[namePropertyName]) {
              weblinkName.text(data[namePropertyName]);
            }

            weblinkUpdateData.val(JSON.stringify(data));

            if ($.isFunction(options.success)) {
              options.success();
            }

            dialog.focus();
          },
          cancel: function() {
            dialog.focus();
          }
        });

      });
    }

    weblinkName.appendTo(inner);
    
    if (displayPageChildLinksPropertyName && weblink[displayPageChildLinksPropertyName]) {
      item.addClass('xrm-displaypagechildlinks');
      $('<span />').attr('title', XRM.localize('adx_weblink.displaypagechildlinks.tooltip')).addClass('xrm-displaypagechildlinks-icon').appendTo(inner);
    }

    if (weblink.__metadata) {
      $('<a />').attr('href', weblink.__metadata.uri).addClass('xrm-entity-ref').appendTo(item).hide();

      var weblinkDeleteUriTemplate = $('a.xrm-uri-template.xrm-entity-adx_weblink-delete-ref', entityContainer).attr('href');

      if (weblinkDeleteUriTemplate) {
        var weblinkDeleteUri = XRM.util.expandUriTemplate(weblinkDeleteUriTemplate, weblink);

        if (weblinkDeleteUri) {
          $('<a />').attr('href', weblinkDeleteUri).addClass('xrm-entity-delete-ref').appendTo(item).hide();
        }
      }
    } else {
      weblinkUpdateData.addClass('create').val(JSON.stringify(weblink));
    }

    var parentPropertyName = self.getWeblinkPropertyName('adx_parentweblinkid');

    var childLinks = $.grep(weblinkData || [], function (e) {
      var parent = e[parentPropertyName];
      return parent && parent.Id === weblink.Id;
    });
    
    var childLinkList = $('<ul />').addClass('xrm-editable-child-weblinks').addClass('xrm-editable-weblinks-sortable').appendTo(item);
    
    $.each(childLinks, function(index, child) {
      self.addWebLinkItem(dialog, childLinkList, child, entityContainer, weblinkData);
    });
    
    var maxDepth = parseInt(entityContainer.attr('data-weblinks-maxdepth')) || 1;

    if (isSortingEnabled) {
      childLinkList.sortable({
        connectWith: '.xrm-editable-weblinks-sortable',
        handle: '.xrm-drag-handle',
        opacity: 0.8,
        placeholder: 'xrm-drag-placeholder',
        start: function(event, ui) {
          $('.xrm-editable-weblink:not(.xrm-displaypagechildlinks) .xrm-editable-child-weblinks:empty', dialog.body).each(function() {
            var itemContainer = $(this);
            var depth = itemContainer.parentsUntil('.xrm-editable-weblinks', '.xrm-editable-child-weblinks').length + 1;

            if (maxDepth < 1 || depth < maxDepth) {
              itemContainer.addClass('xrm-drag-target');
            }
          });
        },
        stop: function(event, ui) {
          $('.xrm-editable-child-weblinks', dialog.body).removeClass('xrm-drag-target');
        }
      });
    }

    return item;
  };

  self.saveWebLinks = function(webLinkData, weblinksContainer, weblinksServiceUri, completeCallback) {
    var weblinkMap = {},
        displayOrderPropertyName = self.getWeblinkPropertyName('adx_displayorder'),
        parentPropertyName = self.getWeblinkPropertyName('adx_parentweblinkid'),
        operations = [],
        successfulOperations = [],
        failedOperations = [],
        totalOperations = 0;
    
    function saveComplete() {
      if ($.isFunction(completeCallback)) {
        completeCallback(successfulOperations, failedOperations);
      }
    }
    
    // Signal that an operation has completed. If all operations have been completed, signal save completion.
    function operationComplete() {
      if ((successfulOperations.length + failedOperations.length) >= totalOperations) {
        saveComplete();
      }
    }
    
    // Add an operation to the queue, which is segregated by weblink depth.
    function addOperation(depth, operation) {
      if (!$.isArray(operations[depth])) {
        operations[depth] = [];
      }

      operations[depth].push(operation);
    }

    // Map our weblink data into an object in for which we can look things up by ID.
    $.each(webLinkData, function(i, weblink) { weblinkMap[weblink.__metadata.uri] = weblink; });

    // Go through the deleted weblinks (the ones that ever existed to begin with, i.e., weren't
    // just added then deleted in the same edit session), and queue up the delete operation.
    $('.xrm-editable-weblink.deleted', weblinksContainer).each(function(_, item) {
      var weblinkDeleteUri = $(item).children('.xrm-entity-delete-ref').attr('href');

      if (!weblinkDeleteUri) return;

      addOperation(0, { uri: weblinkDeleteUri, method: null });
    });

    // Go through the non-deleted weblinks, and queue up any update or creation operations.
    $.each($.map($('.xrm-editable-weblink:not(.deleted)', weblinksContainer).get(), function(item, itemIndex) {
        return self.getWebLinkOperation($(item), itemIndex, weblinkMap, weblinksServiceUri, parentPropertyName, displayOrderPropertyName);
      }),
      function (_, operation) {
        addOperation(operation.depth, operation);
      });
    
    // Add up the total number of queued operations.
    $.each(operations, function (i, e) {
      if ($.isArray(e)) {
        totalOperations = totalOperations + e.length;
      }
    });
    
    if (totalOperations < 1) {
      saveComplete();
      return;
    }
    
    self.saveWebLinksAtDepth(0, operations, successfulOperations, failedOperations, operationComplete);
  };

  self.getWebLinkOperation = function(itemContainer, itemIndex, weblinkMap, weblinksServiceUri, parentPropertyName, displayOrderPropertyName) {
    var weblink = weblinkMap[itemContainer.children('.xrm-entity-ref').attr('href')],
        displayOrder = (itemIndex + 1),
        json = itemContainer.children('.xrm-editable-weblink-data').val(),
        parentWeblinkItem = itemContainer.parent().parent('.xrm-editable-weblink').eq(0),
        parentWeblink = parentWeblinkItem ? weblinkMap[parentWeblinkItem.children('.xrm-entity-ref').attr('href')] : null,
        depth = itemContainer.parentsUntil('.xrm-editable-weblinks', '.xrm-editable-child-weblinks').length;

    return weblink
      ? self.getWebLinkUpdateOperation(weblink, json, depth, parentWeblinkItem, parentWeblink, parentPropertyName, displayOrder, displayOrderPropertyName)
      : self.getWebLinkCreateOperation(itemContainer, weblinksServiceUri, json, depth, parentWeblinkItem, parentWeblink, parentPropertyName, displayOrder, displayOrderPropertyName);
  };
  
  self.getWebLinkCreateOperation = function (itemContainer, weblinksServiceUri, json, depth, parentWeblinkItem, parentWeblink, parentPropertyName, displayOrder, displayOrderPropertyName) {
    var data;
    
    try {
      data = JSON.parse(json);
    } catch(e) {
      return null;
    }
    
    if (!data) {
      return null;
    }
    
    data[displayOrderPropertyName] = displayOrder;

    // If the weblink has a parent, add that to the data.
    if (parentWeblink) {
      data[parentPropertyName] = {
        LogicalName: parentWeblink.LogicalName,
        Id: parentWeblink.Id
      };
    }

    // method is null so that our data APIs don't use an HTTP method override--we just
    // want a normal POST.
    return {
      uri: weblinksServiceUri,
      method: null,
      data: data,
      depth: depth,
      creationId: itemContainer.attr('data-created-id'),
      parentPropertyName: parentPropertyName,
      parentCreationId: parentWeblinkItem ? parentWeblinkItem.attr('data-created-id') : null
    };
  };

  self.getWebLinkUpdateOperation = function (weblink, json, depth, parentWeblinkItem, parentWeblink, parentPropertyName, displayOrder, displayOrderPropertyName) {
    var data = {},
        parentProperty = weblink[parentPropertyName],
        updated = false;

    if (json) {
      try {
        data = JSON.parse(json);
        updated = !!data;
      } catch (e) {
      }
    }

    if (weblink[displayOrderPropertyName] != displayOrder) {
      data[displayOrderPropertyName] = displayOrder;
      updated = true;
    }

    // If the weblink has a parent and it was updated, add that to the update.
    if (parentWeblink && (!parentProperty || parentProperty.Id !== parentWeblink.Id)) {
      data[parentPropertyName] = {
        LogicalName: parentWeblink.LogicalName,
        Id: parentWeblink.Id
      };
      updated = true;
    }
    // The parent of this weblink is a newly-created weblink, we'll have to set this relationship
    // later, after the parent has been created (so we can get its ID).
    else if (parentWeblinkItem && parentWeblinkItem.hasClass('created')) {
      updated = true;
    }
    // If the weblink had a parent relationship, but no longer does, nullify that lookup.
    else if (weblink[parentPropertyName] && !(parentWeblink || (parentWeblinkItem && parentWeblinkItem.hasClass('created')))) {
      data[parentPropertyName] = null;
      updated = true;
    }
    
    if (!updated) {
      return null;
    }

    return {
      uri: weblink.__metadata.uri,
      method: 'MERGE',
      data: data,
      depth: depth,
      parentPropertyName: parentPropertyName,
      parentCreationId: parentWeblinkItem ? parentWeblinkItem.attr('data-created-id') : null
    };
  };

  self.saveWebLinksAtDepth = function (depth, operations, successfulOperations, failedOperations, operationsComplete) {
    // End depth recursion if we've gone past the end of the depth queue.
    if (depth >= operations.length) {
      operationsComplete();
      return;
    }
    
    var depthOperations = operations[depth],
        depthOperationsCount = $.isArray(depthOperations) ? depthOperations.length : 0,
        completedOperationsCount = 0;
    
    // Signal that all operations for this depth are complete. Trigger the save operations for the next depth.
    function depthOperationsComplete() {
      operationsComplete();
      self.saveWebLinksAtDepth(depth + 1, operations, successfulOperations, failedOperations, operationsComplete);
    }
    
    // Signal that a single operation at this depth is complete. If all are complete, signal that.
    function operationComplete() {
      completedOperationsCount++;
      if (completedOperationsCount >= depthOperationsCount) {
        depthOperationsComplete();
      }
    }
    
    if (depthOperationsCount < 1) {
      depthOperationsComplete();
      return;
    }
    
    $.each(depthOperations, function (_, operation) {
      
      // If the create operation needs a parent ID from previous create operations, set that here.
      if (operation.parentCreationId && operation.parentPropertyName) {
        var parentMatches = $.grep(successfulOperations, function (completed) {
          return completed.operation.creationId === operation.parentCreationId;
        });

        if (parentMatches.length > 0) {
          var parentMatch = parentMatches[0];

          if (parentMatch && parentMatch.data && parentMatch.data.d) {
            operation.data[operation.parentPropertyName] = {
              LogicalName: parentMatch.data.d.LogicalName,
              Id: parentMatch.data.d.Id
            };
          }
        }
      }
        
      XRM.data.postJSON(operation.uri, operation.data, {
        httpMethodOverride: operation.method,
        success: function (data) {
          successfulOperations.push({ operation: operation, data: data });
          operationComplete();
        },
        error: function (xhr) {
          failedOperations.push({ operation: operation, xhr: xhr });
          operationComplete();
        }
      });
    });
  };

});
