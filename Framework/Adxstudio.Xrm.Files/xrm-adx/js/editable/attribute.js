/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

  var ns = XRM.namespace('editable.Attribute');
  var $ = XRM.jQuery;
  var parseUri = XRM.util.parseUri;

  ns.initialize = function (toolbar) {
    $('.xrm-attribute').each(function () {
      ns.initializeAttribute($(this));
    });
  };

  ns.initializeAttribute = function (attributeContainer) {
    var attributeServiceRef = attributeContainer.children('a.xrm-attribute-ref'),
      attributeServiceUri = attributeContainer.data('editable-url') || attributeServiceRef.attr('href'),
      attributeDisplayName = attributeContainer.data('editable-title') || attributeServiceRef.attr('title') || attributeContainer.data('label') || '',
      createUri = attributeContainer.data('create-url'),
      createInitialData = attributeContainer.data('create-initial') || {},
      createAttribute = attributeContainer.data('create-attribute');

    if (attributeServiceUri) {
      // Apply a special class to empty attributes, so we can make them visible/hoverable, through CSS.
      ns.addClassOnEmptyValue(attributeContainer);

      attributeContainer.editable(attributeServiceUri, {
        loadSuccess: function (attributeData) {
          ns.enterEditMode(attributeContainer, attributeServiceUri, attributeData, attributeDisplayName);
        }
      });

      return;
    }

    if (createUri && createAttribute) {
      // Apply a special class to empty attributes, so we can make them visible/hoverable, through CSS.
      ns.addClassOnEmptyValue(attributeContainer);

      attributeContainer.editable(null, {
        loadSuccess: function () {
          ns.enterCreateMode(attributeContainer, createUri, createInitialData, createAttribute, attributeDisplayName);
        }
      });

      return;
    }
  };

  ns.enterEditMode = function (attributeContainer, attributeServiceUri, attributeData, attributeDisplayName) {
    // If we have no valid attribute data, quit.
    if (!(attributeData && attributeData.d)) {
      return;
    }

    var uriSegments = parseUri(attributeServiceUri);

    if (!uriSegments) {
      return;
    }

    var attributeName = uriSegments.file;
    var entityServiceUri = uriSegments.directory;

    // If we fail to extract the service URI info we need, quit.
    if (!(attributeName && entityServiceUri)) {
      return;
    }

    // Trim the trailing slash from the entity URI.
    entityServiceUri = entityServiceUri.replace(/\/$/, '');

    // Preserve any query string on the entity service URI.
    if (uriSegments.query) {
      entityServiceUri += ('?' + uriSegments.query);
    }

    // Preserve any anchor on the entity service URI.
    if (uriSegments.anchor) {
      entityServiceUri += ('#' + uriSegments.anchor);
    }

    // For example, for the class "xrm-attribute xrm-editable-html foo", this would capture
    // ["xrm-editable-html", "html"]. We want [1] in this case.
    var captures = new RegExp(XRM.editable.editableClassRegexp).exec(attributeContainer.attr('class'));

    // If we fail to extract the editable type identifier we want, quit.
    if (!captures || (captures.length < 2)) {
      return;
    }

    var editableTypeHandler = ns.handlers[captures[1]];

    // If our editable type identifier doesn't correspond to an actual handler function, quit.
    if (!$.isFunction(editableTypeHandler)) {
      return;
    }

    var attributeValue = attributeData.d[attributeName];

    editableTypeHandler(attributeContainer, attributeDisplayName, attributeName, attributeValue, entityServiceUri, function () {
      ns.addClassOnEmptyValue(attributeContainer);
    });
  };

  ns.enterCreateMode = function (attributeContainer, createUri, createInitialData, createAttribute, attributeDisplayName) {
    // For example, for the class "xrm-attribute xrm-editable-html foo", this would capture
    // ["xrm-editable-html", "html"]. We want [1] in this case.
    var captures = new RegExp(XRM.editable.editableClassRegexp).exec(attributeContainer.attr('class'));

    // If we fail to extract the editable type identifier we want, quit.
    if (!captures || (captures.length < 2)) {
      return;
    }

    var creatableTypeHandler = ns.handlers[captures[1] + '_create'];

    // If our editable type identifier doesn't correspond to an actual handler function, quit.
    if (!$.isFunction(creatableTypeHandler)) {
      return;
    }

    creatableTypeHandler(attributeContainer, attributeDisplayName, createAttribute, createInitialData, createUri, function () {
      ns.addClassOnEmptyValue(attributeContainer);
    });
  };
});
