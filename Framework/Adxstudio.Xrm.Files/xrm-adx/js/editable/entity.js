/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

  var ns = XRM.namespace('editable.Entity');
  var $ = XRM.jQuery;
  var yuiSkinClass = XRM.yuiSkinClass;
  var JSON = YAHOO.lang.JSON;

  XRM.util.slugify = function (value) {
	var encodedValue = encodeURI(value.replace('%', ''));//encode string and strip %
	var cleanedEncodedValue = encodedValue.replace(/[^\.\w-\d%]/g, ''); //remove all restricted symbols
	var decodedValue = decodeURI(cleanedEncodedValue);
	return decodedValue.
	  replace(/^\s+/, ''). // Strip leading whitespace.
	  replace(/\s+$/, ''). // Strip trailing whitespace.
	  toLowerCase().
	  replace(/[\s:;=+]+/g, '-'). // Replace whitespace and certain punctuation with a hyphen.
	  replace(/[\.]+/g, '.'); // Replace runs of multiple periods with a single period.
  }

  XRM.editable.editableClassRegexp = /xrm-editable-(\S+)/gi;

  ns.initialize = function (toolbar) {
	$('.xrm-entity').each(function () {
	  var entityContainer = $(this),
		  entityContainerClass = entityContainer.attr('class'),
		  editableClassRegexp = new RegExp(XRM.editable.editableClassRegexp),
		  match = null;

	  // For example, for the class "xrm-entity xrm-editable-webpage foo", this would capture
	  // ["xrm-editable-webpage", "webpage"]. We want [1] in this case.
	  while (match = editableClassRegexp.exec(entityContainerClass)) {
		// If we fail to extract the editable type identifier we want, quit.
		if (!match || (match.length < 2)) {
		  continue;
		}

		var editableTypeHandler = ns.handlers[match[1]];

		// If our editable type identifier doesn't correspond to an actual handler function, quit.
		if (!$.isFunction(editableTypeHandler)) {
		  continue;
		}

		editableTypeHandler(entityContainer, toolbar);
	  }
	});
  };

  ns.deleteEntity = function (entityContainer, entityDeleteUri) {
	XRM.data.postJSON(entityDeleteUri, {}, {
	  success: function () {
		var parentUrl = $(".xrm-adx-entity-parent-url-ref", entityContainer).attr('href');

		if (parentUrl) {
		  document.location = parentUrl;
		}
		else if (entityContainer.hasClass('xrm-entity-current')) {
		  document.location = '/';
		}
		else if ($.isFunction(document.location.reload)) {
		  document.location.reload(true);
		}
		else {
		  document.location = document.location;
		}
	  },
	  error: function (xhr) {
		XRM.ui.showDataServiceError(xhr);
	  }
	});
  };

  ns.showCreationDialog = function (form, options) {
	options = options || {};

	options.create = true;

	if (!$.isFunction(options.save)) {
	  options.save = saveCreationDialog;
	}

	$.each(form.fields, function (fieldIndex, field) {
	  if (field.defaultToRoot && form.rootWebpage) {
		field.value = form.rootWebpage;
	  }
	  else if (field.defaultToCurrent && form.current) {
		field.value = form.current;
	  }

	  if (field.defaultToCurrentLang && form.currentLanguage && form.currentLanguage.Name && form.currentLanguage.Id)
		field.value = form.currentLanguage;

	});

	ns.showEntityDialog(form, options);
  };

  var saveCreationDialog = ns.saveCreationDialog = function (form, data, options) {
	options.processData = true;
	saveEntityDialog(form, data, options);
  };

  var saveEntityDialog = ns.saveEntityDialog = function (form, data, options) {
	var hideProgressPanel,
	  error,
	  postSaveOperations,
	  postSaveCreateOperations,
	  postSaveFieldOperations;

	if (!form.uri) {
	  XRM.log('"form.uri" must be defined.', 'error', 'XRM.editable.Entity.saveEntityDialog');
	  return;
	}

	options = options || {};

	if (!$.isFunction(options.success)) {
	  options.success = function () { };
	}

	hideProgressPanel = options.disableProgress
	  ? function () { }
	  : XRM.ui.showModalProgressPanel(XRM.localize('editable.saving'));

	error = $.isFunction(options.error)
	  ? function (xhr, errorType, ex) {
		hideProgressPanel();
		options.error(xhr, errorType, ex);
	  }
	  : function (xhr, errorType, ex) {
		hideProgressPanel();
		XRM.log(errorType + ':' + ex, 'error');
		XRM.ui.showDataServiceError(xhr);
	  };

	postSaveCreateOperations = $.map(data['__new__'] || [], function (create) {
	  if (!create.data) {
		return null;
	  }

	  return function (postSaveData) {
		var deferredOperation = $.Deferred(),
		  uri = postSaveData && postSaveData.d && create.uriTemplate
			? XRM.util.expandUriTemplate(create.uriTemplate, postSaveData.d)
			: create.uri;

		if (uri) {
		  saveCreationDialog({ uri: uri }, create.data, {
			disableProgress: true,
			success: function () {
			  deferredOperation.resolve();
			},
			error: function () {
			  deferredOperation.resolve();
			}
		  });
		} else {
		  deferredOperation.resolve();
		}

		return deferredOperation.promise();
	  };
	});

	delete data['__new__'];

	postSaveFieldOperations = $.map(form.fields || [], function (field) {
	  if (!options.disableFileUploads && field.type === 'file' && (field.fileUploadUri || field.fileUploadUriTemplate)) {
		return function (postSaveData) {
		  var deferredOperation = $.Deferred();

		  new ns.Form.fieldTypes['file'](form.entityName, field).upload(postSaveData, function () {
			console.log('resolved post-save file upload');
			deferredOperation.resolve();
		  });

		  return deferredOperation.promise();
		};
	  }

	  return null;
	});

	postSaveOperations = (postSaveCreateOperations || []).concat(postSaveFieldOperations || []);

	// Post our data to the creation URI. (Browser security demands that this be same-domain).
	// (See comment block on showEntityDialog for further explanation of form.uri, but this
	// value will be loaded originally from DOM metadata output by framework webcontrols.)
	XRM.data.postJSON(form.uri, data, {
	  processData: options.processData,
	  httpMethodOverride: options.httpMethodOverride,
	  success: function (postSaveData) {
		function done() {
		  hideProgressPanel();
		  options.success();
		}

		function tryRedirectToEntity() {
		  // Get the location of the new entity with a service call.
		  var urlServiceUri = null;

		  if (form.urlServiceUri) {
			urlServiceUri = form.urlServiceUri;
		  }
		  else if (form.urlServiceUriTemplate && postSaveData && postSaveData.d) {
			urlServiceUri = XRM.util.expandUriTemplate(form.urlServiceUriTemplate, postSaveData.d);
		  }

		  var urlServiceOperationName = form.urlServiceOperationName;

		  if (urlServiceUri === null || (!urlServiceOperationName)) {
			done();
			return;
		  }

		  // Go to the data service to retrieve the URL of the newly-edited/created entity (the edit
		  // may have changed its URL). The browser dictates that this AJAX request can be only made
		  // same-domain, and we'll also validate that the URL returned from the service is same-domain
		  // before we redirect to it.	
		  XRM.data.getJSON(urlServiceUri, {
			success: function (urlData) {
			  if (urlData && urlData.d && urlData.d[urlServiceOperationName]) {
				var url = urlData.d[urlServiceOperationName];
				if (url) {
				  if (XRM.util.isSameDomain(document.location, url)) {
					document.location.href = options.preserveQueryString && url.indexOf('?') < 0 ? (url + document.location.search) : url;
				  }
				  else {
					XRM.log('Returned redirect URL "' + url + '" is not equal to current document.location.host "' + document.location.host + '". Skipping redirect.', 'error');
				  }
				}
			  }
			  done();
			},
			error: function () {
			  done();
			}
		  });
		}

		$.when.apply($, $.map(postSaveOperations, function (operation) { return operation(postSaveData); }))
		  .then(function () {
			// If the reload option is set on the form definition, just reload the page.
			if (form.reload) {
			  done();
			  document.location.href = document.location.href.split("#")[0];
			} else {
			  tryRedirectToEntity();
			}
		  });
	  },
	  error: error
	});
  };

  ns.showEditDialog = function (form, options) {
	if (!form.uri) {
	  return XRM.log('"form.uri" must be defined.', 'error', 'XRM.editable.Entity.showEditDialog');
	}

	options = options || {};

	options.edit = true;

	  if (!$.isFunction(options.save)) {
	  options.save = saveEditDialog;
	}

	var hideProgressPanel = XRM.ui.showModalProgressPanel('Loading...');

	function loadError(xhr, error, ex) {
	  hideProgressPanel();
	  XRM.log(error + ': ' + ex, 'error', 'XRM.editable.Entity.showEditDialog');
	  XRM.ui.showDataServiceError(xhr, XRM.localize('error.dataservice.loading'));
	}

	// Load the entity data, and use it to populate the field values of the form. This is an AJAX
	// request, and so must be same-domain.
	// (See comment block on showEntityDialog for explanation of form.uri. form.uri will
	// have been originally loaded from metadata in the DOM, rendered by framework webcontrols.)
	XRM.data.getJSON(form.uri, {
	  success: function (data, textStatus) {
		hideProgressPanel();

		if (!(data && data.d)) {
		  XRM.ui.showError(XRM.localize('error.dataservice.loading'));
		  return;
		}

		$.each(form.fields, function (fieldIndex, field) {
		  var current = data.d;
		  var propertyName = ns.getPropertyName(form.entityName, field.name);

		  if (field.create && field.create.uriTemplate) {
			field.create.uri = XRM.util.expandUriTemplate(field.create.uriTemplate, data.d);
		  }

		  if (!propertyName) {
			return;
		  }

		  var propertyNameParts = propertyName.split('.');

		  for (var i = 0; i < propertyNameParts.length; i++) {
			current = current[propertyNameParts[i]];
			if (typeof (current) === 'undefined') {
			  break;
			}
		  }

		  var propertyData = current;

		  if (typeof (propertyData) === 'undefined') {
			return;
		  }

		  field.value = propertyData;

		  if (field.uriTemplate) {
			field.uri = field.type === 'parent'
			  ? XRM.util.expandUriTemplate(field.uriTemplate, data.d)
			  : XRM.util.expandUriTemplate(field.uriTemplate, propertyData);
		  }
		});

		ns.showEntityDialog(form, options);
	  },
	  error: loadError
	});
  };

  ns.showDeletionDialog = function (entityContainer, entityUri, entityDisplayName, warningText) {
	var dialogContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog').appendTo(document.body);

	function closeDialog(dialog) {
		dialog.cancel();
		dialogContainer.remove();
		$(document.body).removeClass(yuiSkinClass);
	}

	function handleYes() {
		ns.deleteEntity(entityContainer, entityUri);
		closeDialog(this);
	}

	function handleNo() {
		closeDialog(this);
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

	// In all current usages, entityDisplayName is a string literal comes from the XRM.localizations
	// dictionary. For example, "adx_webpage.shortname" => "page". In this example, as well, jQuery's
	// text() function will do HTML escaping.
	$('<span />').text(XRM.localize('editable.delete.tooltip.prefix') + entityDisplayName + '?').appendTo(dialog.header);
	$('<p />').text(XRM.localize('confirm.delete.entity.prefix') + entityDisplayName + '?').appendTo(dialog.body);

	var siteMarkerWarning = ns.getSiteMarkerWarning(entityContainer, entityDisplayName);

	if (siteMarkerWarning) {
		$('<p />').text(siteMarkerWarning).appendTo(dialog.body);
	}

	if (warningText) {
		$('<p />').text(warningText).appendTo(dialog.body);
	}

	$(document.body).addClass(yuiSkinClass);

	dialog.render();
	dialog.show();
	XRM.ui.registerOverlay(dialog);
	dialog.focus();
  }

  ns.getSiteMarkerWarning = function (entityContainer, entityDisplayName) {
	  // Gather the site markers associated with this entity, and warn the user about these
	  // existing associations.
	  var siteMarkers = [];

	  $('.xrm-entity-adx_webpage_sitemarker', entityContainer).each(function () {
		  siteMarkers.push('"' + $(this).attr('title') + '"');
	  });

	  if (siteMarkers.length < 1) {
		  return null;
	  }

	  // In all current usages, entityDisplayName is a string literal comes from the XRM.localizations
	  // dictionary. For example, "adx_webpage.shortname" => "page". This string will also be passed
	  // through jQuery's text() function before being rendered, which does HTML escaping (see line 100-103).
	  if (siteMarkers.length = 1) {
		  return XRM.util.StringFormatter.format(XRM.localize('sitemarker.warningmessage'), entityDisplayName, siteMarkers.join(', '));
	  }
	  return XRM.util.StringFormatter.format(XRM.localize('sitemarkers.warningmessage'), entityDisplayName, siteMarkers.join(', '));
  }

  var saveEditDialog = ns.saveEditDialog = function (form, data, options) {
	options.httpMethodOverride = 'MERGE';
	options.processData = false;
	options.preserveQueryString = true;
	saveEntityDialog(form, data, options);
  };

  ns.showEntityDialog = function (form, options) {
	options = options || {};

	if (!form) {
	  XRM.log('"form" cannot be null.', 'error', 'XRM.editable.Entity.showEntityDialog');
	  return;
	}

	var layout = form.layout;

	if (!layout) {
	  layout = {
		columns: [
		  { fields: $.map(form.fields, function (field) { return field.name; }) }
		]
	  };
	}

	// Create a DOM container for our dialog.
	var dialogContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog xrm-editable-dialog-create').appendTo(document.body);

	dialogContainer.addClass('xrm-editable-dialog-' + form.entityName);

	if (layout.cssClass) {
	  dialogContainer.addClass(layout.cssClass);
	}

	if (layout.full) {
	  dialogContainer.addClass('xrm-editable-dialog-full');
	}

	// Map array of form field definition objects to actual form field type handler objects (see comment block on showEntityDialog).
	var fields = $.map(form.fields, function (field) {
	  if (field.disableAtRoot && form.root && options.edit) {
		return null;
	  }

	  var type = ns.Form.fieldTypes[field.type];
	  return type ? new type(form.entityName, field) : null;
	});

	function validatePartialUrlForUniqueness(parentPageId, partialUrl, addValidationError, deferred) {
		var getUrl = getChildrenUrl();

		//Try get parent page's children and validate partial url
		XRM.data.getJSON(getUrl, {
			success: function (data, textStatus) {
				if (!(data && data.d)) {
					addValidationError(XRM.localize('error.dataservice.loading'));
					return;
				}
				var childrenWithSameUrl = data.d.filter(function (e) { return e.Url && e.Url === partialUrl });
				// if found children with same url
				if (childrenWithSameUrl.length) {
					// if edit mode
					if (options.edit || form.current.optionsEdit) {
						// trying to find child with same id
						var current = childrenWithSameUrl.filter(function(e) {
							return e.Id === form.current.Id;
						});
						// if found -> partial url not changed -> allow to change
						if (current.length) {
							deferred.resolve(); //resolving deferred as allowed to change
							return;
						}
					}
					addValidationError(XRM.localize('validation.childpage.partialurl.not.unique'));
					return;
				}
				//resolve if parent page does not contain child with same partial url
				deferred.resolve();
			},
			error: function (xhr) {
				addValidationError(XRM.localize('error.dataservice.loading'));
			}
		});

		// helper function to extract website id from form.uri 
		function getChildrenUrl() {
			var regex = /\/_services\/portal\/[A-Za-z0-9-]*/g;
			var portalPath = regex.exec(form.uri)[0]; // `/_services/portal/{__portalguid__}`
			return [portalPath, 'adx_webpage', parentPageId, '__children'].join('/');
		}
	}

	function closeDialog(d) {
	  d.cancel();
	}

	function cancel() {
	  this.cancel();
	}

	function save() {
	  var d = this;
	  var deferred = $.Deferred(); //performing validation using deferred object
	  var uniqValidator; // partial url validator (defined if partial url attribute exists)
	  var hideProgressPanel; // hide progress panel function
	  // Remove any previous validation error messages.			
	  $('.xrm-dialog-validation-summary .xrm-dialog-validation-message', d.body).remove();

	  function addValidationError(message) {
		// Add message through jQuery.text(), which will HTML escape. Message will also generally come 
		// from a combination of a field label (which is either hard-coded in the formPrototype of an
		// entity handler--entities/adx_webpage.js, for example--or retrieved from XRM.localizations) 
		// and a validation suffix retrieved from XRM.localizations.
	  	$('<span />').addClass('xrm-dialog-validation-message').text(message).appendTo($('.xrm-dialog-validation-summary', d.body));
	  	if (deferred.state() === 'pending') {
	  		deferred.reject(); // operation failed - rejecting
	  	}
	  }

	  $.each(fields, function (i, field) {
		  if (!field.validate(d.body)) {
			  addValidationError(XRM.util.StringFormatter.format(XRM.localize('validation.required.suffix'), field.label));
		  }
		  else {
		  	if (field.name == "adx_partialurl") {
		  		var value = $('#' + this.id).val();

			  	var parentPage = _.find(fields, function (f) { return f.name === "adx_parentpageid"; });
			  	var parentPageValue = parentPage ? $('#' + parentPage.id).val() : "" ;
			  	var entitiName = field._entityName;

				if (value === "/" && parentPageValue.length > 0 ) {
					addValidationError(XRM.util.StringFormatter.format(XRM.localize('validation.childpage.partialurl.notslash.message'), field.label));
				}

				if (value !== "/" || parentPage) { // ignore home page validation, check: !(value === "/" && !parentPage)
				  var encodedValue = encodeURI(value.replace('%', ''));//encode string and strip %
				  var cleanedEncodedValue = encodedValue.replace(/[^\.\w-_\d%]/g, ''); //remove all restricted symbols
				  var decodedValue = decodeURI(cleanedEncodedValue);

				  if (decodedValue.length < value.length) {
					addValidationError(XRM.util.StringFormatter.format(XRM.localize('validation.childpage.partialurl.notslash.message'), field.label));
				  }
				}

				if (value !== "/" && !parentPageValue.length && entitiName === "adx_webpage") {
			  		addValidationError(XRM.util.StringFormatter.format(XRM.localize('validation.homepage.partialurl.notslash.message'), field.label));
				}

				if (field.validatePartialUrl(value))
					addValidationError(XRM.util.StringFormatter.format(XRM.localize('validation.partialurl.suffix'), field.label));

		  		//bind all arguments to validate function if other checks succeeded
				if (parentPage && deferred.state() === 'pending') {
					uniqValidator = validatePartialUrlForUniqueness.bind(null, parentPageValue, value, addValidationError, deferred);
				}
			}
		  }
	  });
	  
	  // if not in rejected state and hasn't partial url field
	  if (deferred.state() !== 'rejected' && !uniqValidator) {
	  	deferred.resolve();
	  }
	  // if should validate partial url
	  else if (uniqValidator) {
	  	hideProgressPanel = XRM.ui.showModalProgressPanel(XRM.localize('editable.loading'));
	  	uniqValidator();
	  }

	  deferred.promise().then(function () {
	  	var data = {};

	  	$.each(fields, function (i, field) {
	  		field.appendData(data, d.body);
	  		var formFields = $.grep(form.fields, function (e) { return e.name == field.name; });
	  		if (formFields.length > 0 && formFields[0].defaultToCurrentLang && formFields[0].value != null)
	  			data[field.name] = { Id: formFields[0].value.Id, LogicalName: formFields[0].value.LogicalName };
	  	});

	  	options.save(form, data, {
	  		success: function () {
	  			closeDialog(d);
	  		}
	  	});
	  });

	  //hide progress bar if created
	  deferred.always(function () {
	  	if (hideProgressPanel) {
	  	  hideProgressPanel();
	  	}
	  });
	}

	var dialog = new YAHOO.widget.Dialog(dialogContainer.get(0), {
	  visible: false,
	  constraintoviewport: true,
	  fixedcenter: 'contained',
	  zindex: XRM.zindex,
	  modal: false,
	  buttons: [
		{ text: XRM.localize('editable.save.label'), handler: save, isDefault: true },
		{ text: XRM.localize('editable.cancel.label'), handler: cancel }
	  ]
	});

	var backdrop = $('<div />').addClass('xrm-modal-backdrop').css('z-index', XRM.zindex).hide().appendTo(document.body);

	var stackedDialog = $(document.body).hasClass(yuiSkinClass);

	dialog.subscribe('cancel', function () {
	  dialogContainer.remove();
	  if (!stackedDialog) {
		$(document.body).removeClass(yuiSkinClass);
	  }
	  backdrop.remove();

	  if ($.isFunction(options.cancel)) {
		options.cancel();
	  }
	});

	dialog.setHeader(' ');

	// form.title comes from form definition object provided as parameter, originally found in XRM.localizations.
	// See comment block on showEntityDialog for more. jQuery.text() will also perform HTML escaping here.
	$('<span />').text(options.edit ? (form.editTitle || form.title) : form.title).appendTo(dialog.header);

	dialog.setBody(' ');

	function renderColumns(columns, container) {
	  $.each(columns, function (columnIndex, column) {
		var columnContainer = $('<div />').addClass('xrm-dialog-column').appendTo(container);

		if (column.cssClass) {
		  columnContainer.addClass(column.cssClass);
		}

		$.each(column.fields, function (fieldIndex, fieldName) {
		  var matchingFields = $.grep(fields, function (e) { return e.name == fieldName; });

		  if (!(matchingFields.length > 0)) {
			return;
		  }

		  var field = matchingFields[0];

		  if (!field) {
			return;
		  }

		  var section = $('<div />').addClass('xrm-dialog-section').appendTo(columnContainer);
		  var label = $('<label />').attr('for', field.id).text(field.label + (field.required ? ' ' + XRM.localize('editable.required.label') : '')).appendTo(section);

		  field.render(section, label);
		});
	  });
	}

	if (layout.tabs) {
	  var tabContainer = $('<div>').attr('role', 'tabpanel').appendTo(dialog.body),
		tabNav = $('<ul>').addClass('xrm-tabs').attr('role', 'tablist').appendTo(tabContainer),
		tabContent = $('<div>').addClass('xrm-tabcontent').appendTo(tabContainer);

	  $.each(layout.tabs, function (tabIndex, tab) {
		var isFirstTab = tabIndex == 0,
		  tabId = 'xrm-editable-tab-' + form.entityName + '-' + tab.id,
		  tabItem = $('<li>').attr('role', 'presentation').appendTo(tabNav),
		  tabPanel = $('<div>').addClass('xrm-tabpanel').attr('id', tabId).attr('role', 'tabpanel').appendTo(tabContent),
		  tabLink;

		if (isFirstTab) {
		  tabItem.addClass('active');
		  tabPanel.addClass('active');
		}

		tabPanel.attr('aria-expanded', isFirstTab);

		tabLink = $('<a>')
		  .attr('href', '#' + tabId)
		  .attr('title', tab.tooltip || tab.label)
		  .attr('role', 'tab')
		  .attr('aria-controls', tabId)
		  .attr('aria-expanded', isFirstTab)
		  .text(tab.label)
		  .appendTo(tabItem)
		  .click(function (e) {
			e.preventDefault();

			var $this = $(this),
			  $ul = $this.closest('ul'),
			  $li = $this.parent('li'),
			  $target = $($this.attr('href'));

			if ($li.hasClass('active')) {
			  return;
			}

			$ul.find('> .active').removeClass('active').attr('aria-expanded', false);
			$li.addClass('active');
			$this.attr('aria-expanded', true);

			$target.parent().find('> .active').removeClass('active').attr('aria-expanded', false);
			$target.addClass('active').attr('aria-expanded', true);
		  });

		if (tab.icon) {
		  tabLink.prepend('&nbsp;');
		  $('<span>').addClass(tab.icon).attr('aria-hidden', true).prependTo(tabLink);
		}

		renderColumns(tab.columns || [], tabPanel);
	  });
	} else {
	  renderColumns(layout.columns, dialog.body);
	}

	$('<div />').addClass("xrm-dialog-validation-summary").appendTo(dialog.body);

	// Set up any sync-slugification specified by any fields, only if that field has no existing value.
	$.each(fields, function (index, field) {
	  if (field.slugify && !$("#" + field.id, dialog.body).val()) {
		var targets = $.grep(fields, function (f) { return f.name == field.slugify; });

		for (var i = 0; i < targets.length; i++) {
		  $('#' + field.id, dialog.body).syncSlugify($('#' + targets[i].id, dialog.body));
		}
	  }
	});

	var hideProgressPanel = XRM.ui.showModalProgressPanel(XRM.localize('editable.loading'));

	function showDialog() {
	  if (layout.full) {
		dialogContainer.width($(window).width() - 100);
	  }

	  dialog.render();
	  $(".container-close").attr('title', window.ResourceManager["Close_DefaultText"]);
	  hideProgressPanel();
	  backdrop.show();

	  // Add this class to the document body, so that YUI modal dialog effects work
	  // properly. We'll clean up this class when our dialog closes.
	  $(document.body).addClass(yuiSkinClass);

	  $.each(fields, function (i, field) { field.show(dialog); });

	  if (layout.full) {
		$(dialog.body).height($(window).height() - 180);
	  }

	  dialog.show();
	  XRM.ui.registerOverlay(dialog);
	  dialog.focus();
	  backdrop.css('z-index', $(dialog.element).css('z-index') - 1);
	  $('input:first', dialog.body).focus();
	  $('.xrm-editable-dialog .ft .button-group button').first().attr('title', XRM.localize('editable.save.label'));
	  $('.xrm-editable-dialog .ft .button-group button').last().attr('title', XRM.localize('editable.cancel.label'));
	}

	// Load any remote data required by any select fields.
	var fieldsToLoad = $.grep(fields, function (field) { return field.requiresLoading; });

	if (fieldsToLoad.length < 1) {
	  showDialog();
	  return;
	}

	var completedLoads = [];

	function loadComplete(field) {
	  completedLoads.push(field);

	  if (completedLoads.length == fieldsToLoad.length) {
		showDialog();
	  }
	}

	function loadError(field, xhr) {
	  hideProgressPanel();
	  closeDialog(dialog);
	  XRM.ui.showDataServiceError(xhr, XRM.localize('error.dataservice.loading'));
	}

	function dataError(message) {
	  hideProgressPanel();
	  closeDialog(dialog);
	  XRM.ui.showError(message);
	}

	$.each(fieldsToLoad, function (i, field) {
	  XRM.data.getJSON(field.uri, {
		success: function (data, textStatus) {
		  if (!(data && data.d)) {
			dataError(XRM.localize('error.dataservice.loading.field.prefix') + field.label + '.');
			return;
		  }

		  field.load(data, dialog.body);

		  loadComplete(field);
		},
		error: function (xhr) {
		  loadError(field, xhr);
		}
	  });
	});
  };

  ns.configureDateTimeField = function (field, entityContainer) {
	if (field.type !== 'datetime') {
	  return;
	}

	var dateTimeFormat = entityContainer.data('cultureinfo-datetimeformat');

	if (dateTimeFormat) {
	  try {
		field.dateTimeFormat = typeof (dateTimeFormat) === 'string' ? JSON.parse(dateTimeFormat) : dateTimeFormat;
	  } catch (e) {
		field.dateTimeFormat = null;
	  }
	}
  };

  // This part copied from buildin js file
  var nsData = XRM.namespace('data');
  XRM.data.postJSON = function (uri, data, options) {
	  var acceptHeader = 'application/json, text/javascript';
	  options = options || {};
	  var responseHandler = nsData.getReponseHandler(options.success);
		  shell.ajaxSafePost({
		  beforeSend: function (xhr) {
			  xhr.setRequestHeader('Accept', acceptHeader);

			  if (options.httpMethodOverride) {
				  xhr.setRequestHeader('X-HTTP-Method', options.httpMethodOverride);
			  }
		  },
		  url: uri,
		  type: 'POST',
		  processData: options.processData || false,
		  dataType: responseHandler.dataType,
		  contentType: 'application/json',
		  data: data ? JSON.stringify(data) : null,
		  success: responseHandler.handler,
		  error: options.error
	  });
  };
});
