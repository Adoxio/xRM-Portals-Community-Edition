/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {
	
	var ns = XRM.namespace('editable.Entity');
	var $ = XRM.jQuery;
	var editableClassRegexp = XRM.editable.editableClassRegexp;
	var yuiSkinClass = XRM.yuiSkinClass;
	var JSON = YAHOO.lang.JSON;
	
	if (!ns.handlers) {
		ns.handlers = {};
	}
	
	ns.initialize = function(toolbar) {
		$('.xrm-entity').each(function() {
			var entityContainer = $(this);

			// For example, for the class "xrm-entity xrm-editable-webpage foo", this would capture
			// ["xrm-editable-webpage", "webpage"]. We want [1] in this case.
			var captures = editableClassRegexp.exec(entityContainer.attr('class'));

			// If we fail to extract the editable type identifier we want, quit.
			if (!captures || (captures.length < 2)) {
				return;
			}

			var editableTypeHandler = ns.handlers[captures[1]];

			// If our editable type identifier doesn't correspond to an actual handler function, quit.
			if (!$.isFunction(editableTypeHandler)) {
				return;
			}

			editableTypeHandler(entityContainer, toolbar);
		});
	}
	
	// In all current usages, entityDisplayName is a string literal comes from the XRM.localizations
	// dictionary. For example, "adx_webpage.shortname" => "page".
	ns.createDeleteButton = function(entityContainer, entityDeleteUri, entityDisplayName, container) {
		return new YAHOO.widget.Button({
			container: container,
			label: XRM.localize('editable.delete.label'),
			title: XRM.localize('editable.delete.tooltip.prefix') + entityDisplayName,
			onclick: { fn: function() { ns.showDeletionDialog(entityContainer, entityDeleteUri, entityDisplayName); } }
		});
	}
	
	ns.deleteEntity = function(entityContainer, entityDeleteUri) {
		XRM.data.postJSON(entityDeleteUri, {}, {
			success: function () {
				var redirectUrl = $(".xrm-entity-parent-url-ref", entityContainer).attr('href');
				document.location = redirectUrl && XRM.util.isSameDomain(redirectUrl) ? redirectUrl : '/'
			},
			error: function(xhr) {
				XRM.ui.showDataServiceError(xhr);
			}
		});
	}
	
	ns.showDeletionDialog = function(entityContainer, entityUri, entityDisplayName) {
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
			buttons: [{ text: XRM.localize('confirm.yes'), handler: handleYes, isDefault: true }, { text: XRM.localize('confirm.no'), handler: handleNo}]
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
		
		$(document.body).addClass(yuiSkinClass);

		dialog.render();
		dialog.show();
		XRM.ui.registerOverlay(dialog);
		dialog.focus();
	}
	
	ns.getSiteMarkerWarning = function(entityContainer, entityDisplayName) {
		// Gather the site markers associated with this entity, and warn the user about these
		// existing associations.
		var siteMarkers = [];
		
		$('.xrm-entity-adx_webpage_sitemarker', entityContainer).each(function() {
			siteMarkers.push('"' + $(this).attr('title') + '"');
		});
		
		if (siteMarkers.length < 1) {
			return null;
		}
		
		// In all current usages, entityDisplayName is a string literal comes from the XRM.localizations
		// dictionary. For example, "adx_webpage.shortname" => "page". This string will also be passed
		// through jQuery's text() function before being rendered, which does HTML escaping (see line 100-103).
		return "This " +
			entityDisplayName +
			" is associated with the sitemarker" +
			((siteMarkers.length > 1) ? "s" : "") +
			" " +
			siteMarkers.join(', ') +
			". Site functionality may depend on this association.";
	}
	
	// The form parameter of this funtion is an object that forms a "specification" for an editing dialog.
	// This object must conform to a specific structure (specifies a form title, the service URI of the
	// entity being edited, etc.), and also specifies the form fields to be rendered, and their properties.
	// For example (also see entities/adx_webpage.js for another example):
	//
	// var webPageForm = {
	//  uri: "/Services/Cms.svc/adx_webpages(guid'00000000-0000-0000-0000-000000000000')", // Often populated from metadata embedded in the DOM, rendered by framework webcontrols (for example, Microsoft.Xrm.Portals.Web.UI.WebControls.Property).
	//  urlServiceUri: "/Services/Cms.svc/GetEntityUrl?entitySet='adx_webpages'&entityID=guid'00000000-0000-0000-0000-000000000000'", // Usually populated from metadata rendered to the DOM.
	//  urlServiceUriTemplate: null,
	//  title: "Edit this page", // Usually populated from a lookup to XRM.localizations.
	//  entityName: 'adx_webpage',
	//  fields: [
	//   { name: 'adx_name', label: 'Name', type: 'text', required: true },
	//   { name: 'adx_partialurl', label: 'Partial URL', type: 'text', required: true, slugify: 'adx_name' },
	//   { name: 'adx_pagetemplateid', label: 'Page Template', type: 'select', excludeEmptyData: true, required: true, uri: "/Services/Cms.svc/adx_pagetemplates", optionEntityName: 'adx_pagetemplate', optionText: 'adx_name', optionValue: 'adx_pagetemplateid', sortby: 'adx_name' },
	//   { name: 'adx_displaydate', label: 'Display Date', type: 'datetime', excludeEmptyData: true },
	//   { name: 'adx_hiddenfromsitemap', label: 'Hidden from Sitemap', type: 'checkbox' }
	//  ]
	// };
	//
	// The "fields" array here dictates the form fields that will be rendered for this dialog (and whether they are required fields, etc.).
	// The "type" property of a field (e.g., "text") will lookup a class in XRM.editable.Entity.Form.fieldTypes (e.g., fieldTypes["text"]).
	// These classes are then responsible for the actual rendering/UI of each supported field type. The definition of these types can be
	// found in entity_form.js. 
	ns.showEntityDialog = function(form, options) {
		options = options || {};
		
		if (!form) {
			return XRM.log('"form" cannot be null.', 'error', 'XRM.editable.Entity.showCreationDialog');
		}
		
		// Create a DOM container for our dialog.
		var dialogContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog xrm-editable-dialog-create').appendTo(document.body);
		
		// Map array of form field definition objects to actual form field type handler objects (see comment block on showEntityDialog).
		var fields = $.map(form.fields, function(field) {
			var type = ns.Form.fieldTypes[field.type];
			return type ? new type(form.entityName, field) : null;
		});

		function closeDialog(dialog) {
			dialog.cancel();
		}

		function cancel() {
			this.cancel();
		}

		function save() {
			var dialog = this;
			
			// Remove any previous validation error messages.			
			$('.xrm-dialog-validation-summary .xrm-dialog-validation-message', dialog.body).remove();
			
			// Do field validation.
			var valid = true;

			function addValidationError(message) {
				valid = false;
				// Add message through jQuery.text(), which will HTML escape. Message will also generally come 
				// from a combination of a field label (which is either hard-coded in the formPrototype of an
				// entity handler--entities/adx_webpage.js, for example--or retrieved from XRM.localizations) 
				// and a validation suffix retrieved from XRM.localizations.
				$('<span />').addClass('xrm-dialog-validation-message').text(message).appendTo($('.xrm-dialog-validation-summary', dialog.body));
			}

			$.each(fields, function(i, field) {
				if (!field.validate(dialog.body)) {
					addValidationError(field.label + XRM.localize('validation.required.suffix'));
				}
			});

			// Invalid input; stop the save process.
			if (!valid) {
				return;
			}
			
			var data = {};
			
			$.each(fields, function(i, field) { field.appendData(data, dialog.body); });
			
			options.save(form, data, {
				success: function() {
					closeDialog(dialog);
				}
			});
		}

		var dialog = new YAHOO.widget.Dialog(dialogContainer.get(0), {
			visible: false,
			constraintoviewport: true,
			fixedcenter: true,
			zindex: XRM.zindex,
			modal: true,
			buttons: [{ text: XRM.localize('editable.save.label'), handler: save, isDefault: true }, { text: XRM.localize('editable.cancel.label'), handler: cancel }]
		});

		dialog.subscribe('cancel', function () {
			dialogContainer.remove();
			$(document.body).removeClass(yuiSkinClass);

			if ($.isFunction(options.cancel)) {
				options.cancel();
			}
		});

		dialog.setHeader(' ');
		
		// form.title comes from form definition object provided as parameter, originally found in XRM.localizations.
		// See comment block on showEntityDialog for more. jQuery.text() will also perform HTML escaping here.
		$('<span />').text(form.title).appendTo(dialog.header);
		
		dialog.setBody(' ');

		// Generate the DOM for our form fields.
		$.each(fields, function(i, field) {
			var section = $('<div />').addClass('xrm-dialog-section').appendTo(dialog.body);
			var label = $('<label />').attr('for', field.id).text(field.label + (field.required ? ' ' + XRM.localize('editable.required.label') : '')).appendTo(section);
			
			field.render(section, label);
		});

		$('<div />').addClass("xrm-dialog-validation-summary").appendTo(dialog.body);

		// Set up any sync-slugification specified by any fields.
		$.each(fields, function(i, field) {
			if (field.slugify) {
				var targets = $.grep(fields, function(f) { return f.name == field.slugify });
				
				for (var i = 0; i < targets.length; i++) {
					$('#' + field.id, dialog.body).syncSlugify($('#' + targets[i].id, dialog.body));
				}
			}
		});

		var hideProgressPanel = XRM.ui.showModalProgressPanel(XRM.localize('editable.loading'));

		function showDialog() {
			dialog.render();
			hideProgressPanel();

			// Add this class to the document body, so that YUI modal dialog effects work
			// properly. We'll clean up this class when our dialog closes.
			$(document.body).addClass(yuiSkinClass);
			
			$.each(fields, function(i, field) { field.show(dialog) });
			
			dialog.show();
			XRM.ui.registerOverlay(dialog);
			dialog.focus();
			$('input:first', dialog.body).focus();
		}
		
		// Load any remote data required by any select fields.
		var fieldsToLoad = $.grep(fields, function(field) { return field.requiresLoading });
		
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

		$.each(fieldsToLoad, function(i, field) {
			XRM.data.getJSON(field.uri, {
				success: function(data, textStatus) {
					if (!(data && data.d)) {
						dataError(XRM.localize('error.dataservice.loading.field.prefix') + field.label + '.');
						return;
					}

					field.load(data, dialog.body);

					loadComplete(field);
				},
				error: function(xhr) {
					loadError(field, xhr);
				}
			});
		});
	}
	
	ns.showCreationDialog = function(form, options) {
		options = options || {};
		
		if (!$.isFunction(options.save)) {
			options.save = saveCreationDialog;
		}
		
		ns.showEntityDialog(form, options);
	}
	
	function saveCreationDialog(form, data, options) {
		options.processData = true;
		saveEntityDialog(form, data, options);
	}
	
	function saveEntityDialog(form, data, options) {
		if (!form.uri) {
			return XRM.log('"form.uri" must be defined.', 'error', 'XRM.editable.Entity.saveEntityDialog');
		}
		
		options = options || {};
		
		if (!$.isFunction(options.success)) {
			options.success = function() {};
		}
		
		var hideProgressPanel = XRM.ui.showModalProgressPanel(XRM.localize('editable.saving'));

		// Post our data to the creation URI. (Browser security demands that this be same-domain).
		// (See comment block on showEntityDialog for further explanation of form.uri, but this
		// value will be loaded originally from DOM metadata output by framework webcontrols.)
		XRM.data.postJSON(form.uri, data, {
			processData: options.processData,
			httpMethodOverride: options.httpMethodOverride,
			success: function(data, textStatus) {
				function done() {
					hideProgressPanel();
					options.success();
				}
				
				// If the reload option is set on the form definition, just reload the page.
				if (form.reload) {
					done();
					document.location = document.location;
					return;
				}
				
				function tryRedirectToEntity() {
					// Get the location of the new entity with a service call.
					var urlServiceUri = null;
					
					if (form.urlServiceUri) {
						urlServiceUri = form.urlServiceUri;
					}
					else if (form.urlServiceUriTemplate && data && data.d) {
						urlServiceUri = XRM.util.expandUriTemplate(form.urlServiceUriTemplate, data.d);
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
						success: function(urlData, textStatus) {
							if (urlData && urlData.d && urlData.d[urlServiceOperationName]) {
								var url = urlData.d[urlServiceOperationName];
								if (url) {
									if (XRM.util.isSameDomain(document.location, url)) {
										document.location = url;
									}
									else {
										XRM.log('Returned redirect URL "' + url + '" is not equal to current document.location.host "' + document.location.host + '". Skipping redirect.', 'error');
									}
								}
							}
							done();
						},
						error: function() { done(); }
					});
				}
				
				// Find all valid file upload fields.
				var fileUploads = $.map(form.fields, function(field) {
					return (field.type === 'file' && (field.fileUploadUri || field.fileUploadUriTemplate))
						? new ns.Form.fieldTypes.file(form.entityName, field)
						: null;
				});
							
				// No file uploads; try redirect and close dialog.
				if (fileUploads.length < 1) {
					tryRedirectToEntity();
					return;
				}
				
				var completedFileUploads = [];
				
				// Called to signal a completed file upload.
				function fileUploadComplete(field) {
					completedFileUploads.push(field);
					
					// If all file uploads are complete, try redirect, and close the dialog.
					if (completedFileUploads.length >= fileUploads.length) {
						tryRedirectToEntity();
					}
				}
				
				// Do any file uploads.
				$.each(fileUploads, function(i, field) {
					field.upload(data, function() { fileUploadComplete(field) });
				});
			},
			error: function(xhr, errorType, ex) {
				hideProgressPanel();
				XRM.log(errorType + ':' + ex, 'error');
				XRM.ui.showDataServiceError(xhr);
			}
		});
	}
	
	ns.showEditDialog = function(form, options) {
		if (!form.uri) {
			return XRM.log('"form.uri" must be defined.', 'error', 'XRM.editable.Entity.showEditDialog');
		}
		
		options = options || {};
		
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
			success: function(data, textStatus) {
				hideProgressPanel();
				
				if (!(data && data.d)) {
					XRM.ui.showError(XRM.localize('error.dataservice.loading'));
					return;
				}
				
				$.each(form.fields, function(i, field) {
					var propertyName = ns.getPropertyName(form.entityName, field.name);
					
					if (!propertyName) {
						return;
					}
					
					var propertyData = data.d[propertyName];
					
					if (typeof (propertyData) === 'undefined') {
						return;
					}
					
					field.value = propertyData;
				});
				
				ns.showEntityDialog(form, options);
			},
			error: loadError
		});
	}
	
	function saveEditDialog(form, data, options) {
		options.httpMethodOverride = 'MERGE';
		options.processData = false;
		saveEntityDialog(form, data, options);
	}
	
	var schemaMap = {};
	
	function loadSchemaMap() {
		$('.xrm-entity-schema-map').each(function() {
			var map = $(this);
			var entitySchemaName = map.attr('title');
			
			if (!entitySchemaName) {
				return;
			}
			
			var mapData = null;
			
			try {
				mapData = JSON.parse(map.text() || '[]');
			}
			catch (e) {
				XRM.log('Error loading XRM schema map data for entity "' + entitySchemaName + '": ' + e, 'error', 'XRM.editable.Entity.loadSchemaMap');
				mapData = []
			}
			
			var entityMap = {};
			
			$.each(mapData, function() { entityMap[this['Key']] = this['Value']; });
			
			schemaMap[entitySchemaName] = entityMap;
		});
	}
	
	$(document).ready(function() {
		loadSchemaMap();
	});
	
	ns.getPropertyName = function(entitySchemaName, propertySchemaName) {
		return schemaMap[entitySchemaName][propertySchemaName];
	}
	
});
