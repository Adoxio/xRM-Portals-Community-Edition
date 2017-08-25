/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

  var ns = XRM.namespace('editable.Entity.Form');
  var $ = XRM.jQuery;
  var JSON = YAHOO.lang.JSON;
  var Entity = XRM.editable.Entity;

  if (!(ns.fieldTypes && ns.FieldType)) {
	XRM.log('Unable to extend XRM.editable.Entity.Form.fieldTypes.', 'warn');
  }

  XRM.localizations['entity.form.select.new'] = window.ResourceManager['Entity_Form_Select_New'];

  // We need to hot-patch YAHOO.util.Connect.createFrame, as it uses a browser detection
  // strategy that only cares about IE 9, and doesn't consider IE 10. Their non-forward-
  // looking browser detection breaks our file upload fields in IE 10.
  function _patchedCreateFrame (secureUri) {
	// IE does not allow the setting of id and name attributes as object
	// properties via createElement().  A different iframe creation
	// pattern is required for IE.
	var frameId = 'yuiIO' + this._transaction_id,
		// NOTE: This line was updated from YUI 2.9.0, where it previously tested '=== 9'.
		// We need the same behaviour for IE 10 as well.
		ie9 = (YAHOO.env.ua.ie >= 9) ? true : false,
		io;

	if (YAHOO.env.ua.ie && !ie9) {
	  io = document.createElement('<iframe id="' + frameId + '" name="' + frameId + '" />');

	  // IE will throw a security exception in an SSL environment if the
	  // iframe source is undefined.
	  if (typeof secureUri == 'boolean') {
		io.src = 'javascript:false';
	  }
	}
	else {
	  io = document.createElement('iframe');
	  io.id = frameId;
	  io.name = frameId;
	}

	io.style.position = 'absolute';
	io.style.top = '-1000px';
	io.style.left = '-1000px';

	document.body.appendChild(io);
	YAHOO.log('File upload iframe created. Id is:' + frameId, 'info', 'Connection');
  };

  ns.fieldTypes.text = ns.fieldTypes.text.extend({
	render: function (container, label) {
	  var field = this._data;
	  var attrs = { };
	  var textbox = $('<input type="text" class="xrm-text" />').appendTo(container);
	  if (field["disabled"]) {
		textbox.attr('readonly', 'readonly');
		textbox.addClass('xrm-text-disabled');
	  }

	  attrs = { id: this.id, name: this.id };
	
	  if (field.maxlength) {
		  attrs["maxlength"] = field.maxlength;
	  }
	  textbox.attr(attrs);
	  var formatter = field.formatter;
	  if (!formatter || "function" !== typeof formatter) {
		formatter = function(val) {
		  return val || "";
		};
	  }
	  if (typeof (this._initialValue) !== 'undefined' && this._initialValue !== null) {
		textbox.val(formatter(this._initialValue));
	  }

	},
	validate: function (container) {
		return !!(!this.required || this.getValue(container).trim());
	},
	validatePartialUrl: function (value) {
		var reg = /^(http|https|ftp)/i;
		return (reg.test(value));
	}
  });

  ns.fieldTypes.file = ns.fieldTypes.file.extend({
	upload: function (data, onComplete) {
	  var originalCreateFrame = YAHOO.util.Connect.createFrame;
	  YAHOO.util.Connect.createFrame = _patchedCreateFrame;
	  var field = this._data,
			 uploadUri = null,
			 parsedUri,
			 message,
			 form;

	  if (field.fileUploadUri) {
		  uploadUri = field.fileUploadUri;
	  }
	  else if (field.fileUploadUriTemplate && data && data.d) {
		  uploadUri = XRM.util.expandUriTemplate(field.fileUploadUriTemplate, data.d);
	  }

	  if (uploadUri === null) {
		  return;
	  }

	  if (!XRM.util.isSameDomain(document.location, uploadUri)) {
		  message = 'Host of upload URI "' + uploadUri + '" is not equal to current document.location.host "' + document.location.host + '". File upload failed.';
		  XRM.log(message, 'error');
		  alert(message);
		  onComplete(this);
		  return;
	  }

	  form = $('<form />')
		  .attr('id', this.id + '-form')
		  .attr('action', uploadUri)
		  .attr('enctype', "multipart/form-data")
		  .attr('method', 'post')
		  .hide();

	  var _this = this;

	  shell.getTokenDeferred().done(function (token) {

		  // add as hidden field
		  $("<input>").attr("name", "__RequestVerificationToken").attr("type", "hidden").appendTo(form).val(token);

		  //append form to body.
		  form.appendTo(document.body);

		  $('#' + _this.id).wrap(form);

		  YAHOO.util.Connect.setForm(form.attr('id'), true, true);

		  YAHOO.util.Connect.asyncRequest('POST', uploadUri, {
			  upload: function (response) {
				  form.remove();
				  onComplete(_this);
			  }
		  });
		  YAHOO.util.Connect.createFrame = originalCreateFrame;
	  });
	}
  });

  ns.fieldTypes.picklist = ns.FieldType.extend({
	appendData: function (data, container) {
	  if (this._disabled) {
		return data;
	  }

	  return this._super(data, container);
	},

	getValue: function (container) {
	  var value = $('#' + this.id, container || document).val();
	  return value === '' ? null : value;
	},

	render: function (container, label) {
	  var select = $('<select />').attr('id', this.id).attr('name', this.id).appendTo(container);

	  if (!this.required) {
		$('<option />').val('').html('&nbsp;').prop('selected', true).appendTo(select);
	  }

	  var picklistSelector = this._entityName + '.' + this.name;

	  var json = $('.xrm-entity-picklist[title="' + picklistSelector + '"]').text();
	  var data;

	  try {
		data = JSON.parse(json);
	  }
	  catch (e) {
		XRM.log('Unable to load picklist ' + picklistSelector + '. ' + e, 'warn');
		this._disabled = true;
		container.hide();
		return;
	  }

	  if (!$.isArray(data)) {
		XRM.log('Unable to load picklist ' + picklistSelector + '. JSON is not an array.', 'warn');
		this._disabled = true;
		container.hide();
		return;
	  }

	  for (var i = 0; i < data.length; i++) {
		var option = data[i];
		$('<option />').val(option.Key).text(option.Value).prop('selected', this._initialValue == option.Key).appendTo(select);
	  }
	},

	validate: function (container) {
	  return this._disabled || this._super(container);
	}
  });

  ns.fieldTypes.datetime = ns.fieldTypes.datetime.extend({
	appendData: function (data, container) {

	  data[this.propertyName] = this._picker.getSelectedDateTime();

	  return data;
	},

	init: function (entityName, data) {
	  this._super(entityName, data);

	  if (data.defaultToNow && !this._initialValue) {
		this._initialValue = "Date(" + new Date().getTime() + ")";
	  }

	  this._language = $('html').attr('lang');
	},
	render: function (container, label) {
	  var picker = new XRM.ui.DateTimePicker(container, this.id, this._language);
	  
	  picker.render(this._initialValue);

	  this._picker = picker;
	}
  });

  ns.fieldTypes.select = ns.fieldTypes.select.extend({
	appendData: function (data, container) {
	  var element = $('#' + this.id, container || document),
		selected = element.find(":selected"),
		newData,
		newUri,
		newUriTemplate;

	  if (selected.val() != 'new') {
		return this._super(data, container);
	  }

	  newData = selected.data('new');
	  newUri = selected.data('uri');
	  newUriTemplate = selected.data('uri-template');

	  if (newData && newUriTemplate) {
		if (!$.isArray(data['__new__'])) {
		  data['__new__'] = [];
		}

		data['__new__'].push({
		  uri: newUri,
		  uriTemplate: newUriTemplate,
		  data: newData
		});
	  }

	  return data;
	},

	load: function (data, container) {
	  var element = $('#' + this.id, container || document);

	  var field = this._data;

	  function getOptionPropertyName(propertySchemaName) {
		return field.optionEntityName ? XRM.editable.Entity.getPropertyName(field.optionEntityName, propertySchemaName) : propertySchemaName;
	  }

	  var items = $.isArray(data.d) ? data.d : [];

	  // Sort elements if specified.
	  if (field.sortby) {
		var sortPropertyName = getOptionPropertyName(field.sortby);

		items.sort(function (a, b) {
		  var aOrder = a[sortPropertyName], bOrder = b[sortPropertyName];
		  if (aOrder < bOrder) return -1;
		  if (aOrder == bOrder) return 0;
		  return 1;
		});
	  }

	  var textPropertyName = getOptionPropertyName(field.optionText);
	  var valuePropertyName = getOptionPropertyName(field.optionValue);
	  var isDefaultPropertyName = getOptionPropertyName('adx_isdefault');
	  var descriptionPropertyName = field.optionDescription ? getOptionPropertyName(field.optionDescription) : null;

	  var isSelected = (field.value != null && typeof (field.value) !== 'undefined')
		? function (item) { return item[valuePropertyName] === (field.value.Id || field.value); }
		: function (item) { return !!item[isDefaultPropertyName]; };

	  // Populate select with retrieved items.
	  $.each(items, function (i, item) {
		var option = $('<option />').val(item[valuePropertyName]).text(item[textPropertyName]).prop('selected', isSelected(item)).appendTo(element);

		if (descriptionPropertyName) {
		  option.attr('data-description', item[descriptionPropertyName] || '');
		}
	  });

	  if ($.fn.select2) {
		element.css('display', 'block').css('width', '100%');
		element.select2({
		  allowClear: true,
		  dropdownCssClass: "xrm-editable-select2" + (descriptionPropertyName ? ' xrm-editable-select2-with-descriptions' : ''),
		  minimumResultsForSearch: 5,
		  formatResult: function (item) {
			if (!descriptionPropertyName) {
			  return item.text;
			}

			var option = $(item.element);

			return $('<div>').append($('<div>').addClass('name').text(item.text)).append($('<small>').addClass('description').text(option.data('description')));
		  }
		});
	  }
	},

	render: function (container, label) {
	  var selectContainer = $('<div>').addClass('xrm-select').appendTo(container);
	  var selectElementContainer = $('<div>').addClass('xrm-select-control').appendTo(selectContainer);

	  var select = $('<select />').attr('id', this.id).attr('name', this.id).appendTo(selectElementContainer);

	  if (!this.required) {
		$('<option />').val('').html('&nbsp;').prop('selected', true).appendTo(select);
	  }

	  this.renderCreate(selectContainer, select);
	},

	renderCreate: function (container, element) {
	  var field = this._data,
		create = field.create || {},
		handler,
		createForm,
		title,
		textPropertyName;

	  if (!(create.uriTemplate && create.entityContainer)) {
		return;
	  }

	  handler = Entity.handlers[field.optionEntityName];

	  if (!(handler && $.isFunction(handler.getForm))) {
		return;
	  }

	  title = XRM.localize(create.title) || "New";

	  createForm = handler.getForm(create.entityContainer, {
		title: title,
		uri: create.uriTemplate
	  });

	  if (!(createForm && createForm.valid)) {
		return;
	  }

	  function getOptionPropertyName(propertySchemaName) {
		return field.optionEntityName ? XRM.editable.Entity.getPropertyName(field.optionEntityName, propertySchemaName) : propertySchemaName;
	  }

	  textPropertyName = getOptionPropertyName(field.optionText);

	  container.addClass('xrm-select-with-new');

	  var buttonContainer = $('<div>').addClass('xrm-new').appendTo(container);

	  $('<button>').attr('type', 'button').attr('title', title).text(XRM.localize('') || '+ New').appendTo(buttonContainer).click(function () {
		Entity.showEntityDialog(createForm, {
		  save: function (form, data, options) {
			element.children('option[value=new]').remove();

			$('<option />')
			  .val('new')
			  .text(data[textPropertyName])
			  .prop('selected', true)
			  .data('new', data)
			  .data('uri', create.uri)
			  .data('uri-template', create.uriTemplate)
			  .appendTo(element);
			
			if ($.fn.select2) {
			  $(element).select2('val', 'new');
			}

			options.success();
		  }
		});
	  });
	}
  });

  ns.fieldTypes.parent = ns.FieldType.extend({
	init: function (entityName, data) {
	  this._super(entityName, data);
	  this.requiresLoading = false;
	},

	appendData: function (data, container) {
	  var $input = $('#' + this.id, container || document);

	  if ($input.prop('disabled')) {
		return data;
	  }

	  var value = this.getValue(container);

	  if ((typeof (value) === 'undefined') || (value === null) || (value === '')) {
		if (!this._data.excludeEmptyData) {
		  data[this.propertyName] = null;
		}

		return data;
	  }

	  data[this.propertyName] = { "Id": value, "LogicalName": this._data.optionEntityName };

	  return data;
	},

	render: function (container, label) {
	  var field = this._data,
		selectContainer = $('<div>').addClass('xrm-select').appendTo(container),
		selectElementContainer = $('<div>').addClass('xrm-select-control').appendTo(selectContainer),
		$input = $('<input />').attr('type', 'hidden').attr('id', this.id).attr('name', this.id).appendTo(selectElementContainer),
		initialValue = this._initialValue;

	if (this.required && !(initialValue && initialValue.LogicalName && initialValue.LogicalName == field.optionEntityName)) {
		$input.prop('disabled', true);
		container.hide();
		return;
	  }

	  $input.css('width', '100%');

	  if (initialValue && initialValue.Id) {
		$input.val(initialValue.Id);
	  }

	  if (!$.fn.select2) {
		XRM.log('Select2 plugin not found. Disabling parent selector.', 'warn');
		container.hide();
		return;
	  }

	  $input.select2({
		allowClear: !this.required,
		placeholder: this.required ? null : (XRM.localize(field.placeholder || field.label) || field.label),
		dropdownCssClass: "xrm-editable-select2 xrm-editable-select2-with-descriptions",
		minimumInputLength: 0,
		ajax: {
		  url: field.uri,
		  dataType: 'json',
		  quietMillis: 250,
		  data: function (term, page) {
			return {
			  search: term,
			  page: page
			};
		  },
		  results: function (data, page) {
			return { results: data.d, more: data.more };
		  },
		  cache: true
		},
		id: function (item) {
		  return item.Id;
		},
		initSelection: function (element, callback) {
		  if (initialValue && initialValue.Name) {
			callback(initialValue);
			return;
		  }

		  if (initialValue && initialValue.Id) {
			$.ajax(
			  XRM.util.updateQueryStringParameter(field.uri, 'selectedId', initialValue.Id),
			  {
				dataType: 'json'
			  })
			  .done(function (data) {
				if (data && data.d && data.d[0]) {
				  callback(data.d[0]);
				}
			  });
		  }
		},
		formatResult: function (item) {
		  var path = item.Path || [],
			breadcrumbs = '';

		  for (var i = 1; i < path.length; i++) {
			breadcrumbs += '/ ' + path[i] + ' ';
		  }

		  if (item.Name) {
			breadcrumbs += '/ ' + (path.length < 1 ? '' : item.Name);
		  }
		  
		  return $('<div>').append($('<div>').addClass('name').text(item.Name || item.Id)).append($('<small>').addClass('description path').text(breadcrumbs));
		},
		formatSelection: function (item) {
		  return item.Name || item.Id;
		}
	  });
	},

	validate: function (container) {
	  var $input = $('#' + this.id, container || document);
	  return $input.prop('disabled') || this._super(container);
	}
  });

  ns.fieldTypes.instructions = ns.FieldType.extend({
	appendData: function (data, container) {
	  return data;
	},
	getValue: function (container) {
	  return null;
	},
	render: function (container, label) {
	  $('<input />').attr('type', 'hidden').attr('id', this.id).attr('name', this.id).appendTo(container);
	}
  });

  ns.fieldTypes.textarea = ns.FieldType.extend({
	render: function (container, label) {
	  var field = this._data,
		textbox = $('<textarea />').attr('id', this.id).attr('name', this.id).addClass('xrm-textarea').appendTo(container);

	  if (typeof (this._initialValue) !== 'undefined' && this._initialValue !== null) {
		textbox.val(this._initialValue || '');
	  }

	  if (field.maxlength) {
		textbox.attr('maxlength', field.maxlength);
	  }

	  if (field.height) {
		textbox.height(field.height);
	  }
	}
  });

  ns.fieldTypes.integer = ns.fieldTypes.text.extend({
	appendData: function (data, container) {
	  var value = this.getValue(container);

	  data[this.name] = value;

	  return data;
	},

	getValue: function (container) {
	  var value = parseInt($('#' + this.id, container || document).val());
	  return isNaN(value) ? null : value;
	}
  });

  ns.fieldTypes.html = ns.FieldType.extend({
	getValue: function (container) {
	  return this._editor ? this._editor.getData() : $('#' + this.id, container || document).val();
	},

	render: function (container, label) {
	  var ckeditorSettings,
		height,
		textbox = $('<textarea />').attr('id', this.id).attr('name', this.id).addClass('xrm-html').appendTo(container);

	  if (typeof (this._initialValue) !== 'undefined' && this._initialValue !== null) {
		textbox.val(this._initialValue || '');
	  }

	  if (typeof (CKEDITOR) !== 'undefined') {
		ckeditorSettings = $.extend(true, {}, XRM.ckeditorCompactSettings || {});

		// ckeditor only supports the CRM languages
		ckeditorSettings.language = $('html').attr('crm-lang');

		if (this._data.ckeditorSettings) {
		  ckeditorSettings = $.extend(true, ckeditorSettings, this._data.ckeditorSettings);
		}

		if (ckeditorSettings.height) {
		  height = parseInt(ckeditorSettings.height);

		  if (height) {
			textbox.height(height + 100);
		  }
		}

		if (this._data.cmsTemplateUrl) {
			ckeditorSettings.cmstemplates = this._data.cmsTemplateUrl;
		}

		if (this._data.fileBrowserServiceUri && this._data.fileBrowserDialogUri) {
		  ckeditorSettings.filebrowserBrowseUrl = XRM.util.updateQueryStringParameter(this._data.fileBrowserDialogUri, 'url', encodeURIComponent(this._data.fileBrowserServiceUri));
		}

		this._editorElement = textbox.get(0);
		this._editorSettings = ckeditorSettings;
	  }
	  else {
		this._editor = null;
	  }
	},

	show: function (dialog) {
	  var editorSettings = this._editorSettings || {}

	  if (this._editorElement) {
		this._editor = CKEDITOR.replace(this._editorElement, editorSettings);

		this._editor.on('focus', function () {
		  if (dialog && dialog.focus) dialog.focus();
		});
	  }
	}
  });

  ns.fieldTypes.iframe = ns.FieldType.extend({
	appendData: function (data, container) {
	  var iframe = $('#' + this.id).get(0),
		iframeWindow,
		api;

	  if (!iframe) {
		return data;
	  }

	  iframeWindow = iframe.contentWindow;

	  if (!iframeWindow) {
		return data;
	  }

	  api = iframeWindow.xrm_iframe;

	  if (!api) {
		return data;
	  }

	  data[this.propertyName] = api.getValue();

	  return data;
	},

	getValue: function (container) {
	  var iframe = $('#' + this.id).get(0),
		iframeWindow,
		api;

	  if (!iframe) {
		return null;
	  }

	  iframeWindow = iframe.contentWindow;

	  if (!iframeWindow) {
		return null;
	  }

	  api = iframeWindow.xrm_iframe;

	  if (!api) {
		return null;
	  }

	  return api.getValue();
	},

	render: function (container, label) {
	  var field = this._data,
		initialValue = this._initialValue,
		$iframe = $('<iframe />').attr('id', this.id).attr('src', field.src).addClass('xrm-iframe').appendTo(container);

	  $iframe.on('load', function () {
		var api = $iframe.get(0).contentWindow.xrm_iframe;

		if (!api) {
		  return;
		}

		if (initialValue !== 'undefined' && initialValue !== null) {
		  api.setValue(initialValue);
		}
	  });

	  if (field.height) {
		$iframe.height(field.height);
	  }
	}
  });

  ns.fieldTypes.customrender = ns.FieldType.extend({
	render: function (container, label) {
	  var field = this._data;
	  if (field && field.render && typeof (field.render) === "function") {
		field.render(container, label);
	  }
	}
  });

  ns.fieldTypes.checkbox = ns.fieldTypes.checkbox.extend({
	render: function (container, label) {
	  label.addClass('xrm-checkbox');
	  var checkbox = $('<input />').attr('type', 'checkbox').attr('id', this.id).attr('name', this.id).prependTo(label);

	  if (typeof (this._initialValue) !== 'undefined' && this._initialValue !== null) {
		checkbox.prop('checked', this._initialValue);
	  }
	  else if (this._data.checkedByDefault) {
		checkbox.prop('checked', true);
	  }
	}
  });

  ns.fieldTypes.tags = ns.FieldType.extend({
	appendData: function (data, container) {
	  data[this.name] = this.getValue(container);
	  return data;
	},

	getValue: function (container) {
	  return $.map($('#' + this.id, container).adxtags('tags'), function (tag) { return tag.value; });
	},

	render: function (container, label) {
	  var json = $('.xrm-entity-tags[title="' + this._entityName + '"]').text();
	  var data = JSON.parse(json);
	  var availableTags = data.tags;

	  if (!$.isArray(availableTags)) {
		XRM.log('Unable to load tags for ' + this._entityName + '.', 'warn');
		return;
	  }

	  this._availableTags = availableTags;
	  this._element = $('<ul />').attr('id', this.id).attr('name', this.id).appendTo(container);
	},
	
	show: function (dialog) {
	  if (!(this._availableTags && this._element)) {
		return;
	  }
	  
	  this._element.adxtags({
		tagSource: this._availableTags,
		initialTags: $.isArray(this._initialValue) ? this._initialValue : [],
		triggerKeys: ['enter', 'comma'],
		highlightOnExistColor: '#426FD9'
	  });
	},

	_getId: function (fieldName) {
	  return 'xrm-editable-' + (fieldName || '').replace(/\./, '-');
	}
  });

  (function ($) {
	$.widget("ui.adxtags", {

	  // default options
	  options: {
		tagSource: [],
		triggerKeys: ['enter', 'space', 'comma', 'tab'],
		initialTags: [],
		minLength: 1,
		select: false,
		allowNewTags: true,
		caseSensitive: false,
		highlightOnExistColor: '#0F0',
		emptySearch: true, // empty search on focus
		tagsChanged: function (tagValue, action, element) { ; }
	  },

	  _splitAt: /\ |,/g,
	  _existingAtIndex: 0,
	  _pasteMetaKeyPressed: false,
	  _keys: {
		backspace: [8],
		enter: [13],
		space: [32],
		comma: [44, 188],
		tab: [9]
	  },

	  //initialization function
	  _create: function () {

		var self = this;
		this.tagsArray = [];
		this.timer = null;

		//add class "tagit" for theming
		this.element.addClass("tagit");

		//add any initial tags added through html to the array
		this.element.children('li').each(function () {
		  var tagValue = $(this).attr('tagValue');
		  self.options.initialTags.push(
					tagValue ? { label: $(this).text(), value: tagValue} : $(this).text()
				);
		});

		//setup split according to the trigger keys
		self._splitAt = null;
		if ($.inArray('space', self.options.triggerKeys) > 0 && $.inArray('comma', self.options.triggerKeys) > 0)
		  self._splitAt = /\ |,/g;
		else if ($.inArray('space', self.options.triggerKeys) > 0)
		  self._splitAt = /\ /g;
		else if ($.inArray('comma', self.options.triggerKeys) > 0)
		  self._splitAt = /,/g;

		//add the html input
		this.element.html('<li class="tagit-new"><input class="tagit-input" type="text" /></li>');

		this.input = this.element.find(".tagit-input");

		//setup click handler
		$(this.element).click(function (e) {
		  if ($(e.target).hasClass('tagit-close')) {
			// Removes a tag when the little 'x' is clicked.
			var parent = $(e.target).parent();
			parent.remove();
			var tagValue = parent.attr('tagValue');
			if (tagValue) {
			  self._popTag(null, tagValue);
			} else {
			  var text = parent.text();
			  self._popTag(text.substr(0, text.length - 1));
			}
		  }
		  else {
			self.input.focus();
			if (self.options.emptySearch && $(e.target).hasClass('tagit-input') && self.input.val() == '' && self.input.autocomplete != undefined) {
			  self.input.autocomplete('search');
			}
		  }
		});

		//setup autocomplete handler
		var os = this.options.select;
		this.options.appendTo = this.element;
		this.options.source = this.options.tagSource;
		this.options.select = function (event, ui) {
		  clearTimeout(self.timer);
		  if (ui.item.label === undefined)
			self._addTag(ui.item.value);
		  else
			self._addTag(ui.item.label, ui.item.value);
		  return false;
		}
		var inputBox = this.input;
		this.options.focus = function (event, ui) {
		  if (ui.item.label !== undefined && /^key/.test(event.originalEvent.originalEvent.type)) {
			inputBox.val(ui.item.label);
			inputBox.attr('tagValue', ui.item.value);
			return false;
		  }
		}
		this.input.autocomplete(this.options);
		this.options.select = os;

		//setup keydown handler
		this.input.keydown(function (e) {
		  var lastLi = self.element.children(".tagit-choice:last");
		  if (e.which == self._keys.backspace)
			return self._backspace(lastLi);

		  if (self._isInitKey(e.which)) {
			e.preventDefault();
			if (!self.options.allowNewTags || (self.options.maxTags !== undefined && self.tagsArray.length == self.options.maxTags)) {
			  self.input.val("");
			}
			else if (self.options.allowNewTags && $(this).val().length >= self.options.minLength) {
			  self._addTag($(this).val());
			}
		  }

		  if (self.options.maxLength !== undefined && self.input.val().length == self.options.maxLength) {
			e.preventDefault();
		  }

		  if (lastLi.hasClass('selected'))
			lastLi.removeClass('selected');

		  _pasteMetaKeyPressed = e.metaKey;
		  self.lastKey = e.which;
		});

		this.input.keyup(function (e) {

		  if (_pasteMetaKeyPressed && (e.which == 91 || e.which == 86))
			$(this).blur();

		  // timeout for the fast copy pasters
		  window.setTimeout(function () { _pasteMetaKeyPressed = e.metaKey; }, 250);
		});

		//setup blur handler
		this.input.blur(function (e) {
		  self.currentLabel = $(this).val();
		  self.currentValue = $(this).attr('tagValue');
		  if (self.options.allowNewTags) {
			self.timer = setTimeout(function () {
			  self._addTag(self.currentLabel, self.currentValue);
			  self.currentValue = '';
			  self.currentLabel = '';
			}, 400);
		  }
		  $(this).val('').removeAttr('tagValue');
		  return false;
		});

		//define missing trim function for strings
		String.prototype.trim = function () {
		  return this.replace(/^\s+|\s+$/g, "");
		};

		if (this.options.select) {
		  this.element.after('<select class="tagit-hiddenSelect" name="' + this.element.attr('name') + '" multiple="multiple"></select>');
		  this.select = this.element.next('.tagit-hiddenSelect');
		}
		this._initialTags();

	  },

	  _popSelect: function (label, value) {
		this.select.children('option[value="' + (value === undefined ? label : value) + '"]').remove();
		this.select.change();
	  }
		,

	  _addSelect: function (label, value) {
		var opt = $('<option>').attr({
		  'selected': 'selected',
		  'value': (value === undefined ? label : value)
		}).text(label);
		this.select.append(opt);
		this.select.change();
	  }
		,

	  _popTag: function (label, value) {
		if (label === undefined) {
		  label = this.tagsArray.pop();
		  if (typeof (label) == 'object') {
			value = label.value;
			label = label.label;
		  }
		} else {
		  var index;
		  if (value === undefined) {
			index = -1;
			$.each(this.tagsArray, function (tagIndex, tag) {
			  if (tag && tag.label == label) {
				index = tagIndex;
				return false;
			  }
			});
			index = (index == -1 ? this.tagsArray.length - 1 : index);
		  } else {
			index = this.tagsArray.length - 1;
			for (var i in this.tagsArray) {
			  if (this.tagsArray[i].value == value) {
				index = i;
				break;
			  }
			}
		  }
		  this.tagsArray.splice(index, 1);
		}
		if (this.options.select)
		  this._popSelect(label, value);
		if (this.options.tagsChanged)
		  this.options.tagsChanged(value || label, 'popped', null);
	  }
		,

	  _addTag: function (label, value) {
		this.input.val("");

		if (this._splitAt && label.search(this._splitAt) > 0) {
		  var result = label.split(this._splitAt);
		  for (var i = 0; i < result.length; i++)
			this._addTag(result[i], value);
		  return;
		}

		label = label.replace(/,+$/, "");
		label = label.trim();

		if (label == "")
		  return false;

		if (this._exists(label, value)) {
		  this._highlightExisting();
		  return false;
		}

		var tag = "";
		tag = $('<li class="tagit-choice"'
				+ (value !== undefined ? ' tagValue="' + value + '"' : '')
				+ '>' + label + '<a class="tagit-close">x</a></li>');
		tag.insertBefore(this.input.parent());
		this.input.val("");
		this.tagsArray.push(value === undefined ? { label: label, value: label} : { label: label, value: value });
		if (this.options.select)
		  this._addSelect(label, value);
		if (this.options.tagsChanged)
		  this.options.tagsChanged(label, 'added', tag);
		return true;
	  }
		,

	  _exists: function (label, value) {
		if (this.tagsArray.length == 0)
		  return false;

		if (value === undefined) {
		  this._existingAtIndex = 0;

		  for (var ind in this.tagsArray) {
			var _label = (typeof this.tagsArray[ind] == "string") ? this.tagsArray[ind] : this.tagsArray[ind].label;

			if (this._lowerIfCaseInsensitive(label) == this._lowerIfCaseInsensitive(_label))
			  return true;
			this._existingAtIndex++;
		  }
		} else {
		  this._existingAtIndex = 0;
		  for (var ind in this.tagsArray) {
			if (this._lowerIfCaseInsensitive(value) === this._lowerIfCaseInsensitive(this.tagsArray[ind].value))
			  return true;
			this._existingAtIndex++;
		  }
		}
		this._existingAtIndex = -1;
		return false;
	  }
		,

	  _highlightExisting: function () {
		if (this.options.highlightOnExistColor === undefined)
		  return;
		var duplicate = $($(this.element).children(".tagit-choice")[this._existingAtIndex]);
		duplicate.stop();

		var beforeFont = duplicate.css('color');
		duplicate.animate({ color: this.options.highlightOnExistColor }, 100).animate({ 'color': beforeFont }, 800);
	  }
		,

	  _isInitKey: function (keyCode) {
		var keyName = "";
		for (var key in this._keys)
		  if ($.inArray(keyCode, this._keys[key]) != -1)
			keyName = key;

		if ($.inArray(keyName, this.options.triggerKeys) != -1)
		  return true;
		return false;
	  }
		,

	  _removeTag: function () {
		this._popTag();
		this.element.children(".tagit-choice:last").remove();
	  }
		,

	  _backspace: function (li) {
		if (this.input.val() == "") {
		  // When backspace is pressed, the last tag is deleted.
		  if (this.lastKey == this._keys.backspace) {
			this._popTag();
			li.remove();
			this.lastKey = null;
		  } else {
			li.addClass('selected');
			this.lastKey = this._keys.backspace;
		  }
		}
		return true;
	  }
		,

	  _initialTags: function () {
		var input = this;
		var _temp;
		if (this.options.tagsChanged)
		  _temp = this.options.tagsChanged;
		this.options.tagsChanged = null;

		if (this.options.initialTags.length != 0) {
		  $(this.options.initialTags).each(function (i, element) {
			if (typeof (element) == "object")
			  input._addTag(element.label, element.value);
			else
			  input._addTag(element);
		  });
		}
		this.options.tagsChanged = _temp;
	  }
		,

	  _lowerIfCaseInsensitive: function (inp) {

		if (inp === undefined || typeof (inp) != typeof ("a"))
		  return inp;

		if (this.options.caseSensitive)
		  return inp;

		return inp.toLowerCase();

	  }
		,
	  tags: function () {
		return this.tagsArray;
	  }
		,

	  destroy: function () {
		$.Widget.prototype.destroy.apply(this, arguments); // default destroy
		this.tagsArray = [];
	  }
		,

	  reset: function () {
		this.element.find(".tagit-choice").remove();
		this.tagsArray = [];
		if (this.options.select) {
		  this.select.children().remove();
		  this.select.change();
		}
		this._initialTags();
		if (this.options.tagsChanged)
		  this.options.tagsChanged(null, 'reseted', null);
	  }
		,

	  fill: function (tags) {
		this.element.find(".tagit-choice").remove();
		this.tagsArray = [];
		if (tags !== undefined) {
		  this.options.initialTags = tags;
		}
		if (this.options.select) {
		  this.select.children().remove();
		  this.select.change();
		}
		this._initialTags();
	  }
		,

	  add: function (label, value) {
		label = label.replace(/,+$/, "");

		if (this._splitAt && label.search(this._splitAt) > 0) {
		  var result = label.split(this._splitAt);
		  for (var i = 0; i < result.length; i++)
			this.add(result[i], value);
		  return;
		}

		label = label.trim();
		if (label == "" || this._exists(label, value))
		  return false;

		var tag = "";
		tag = $('<li class="tagit-choice"'
				+ (value !== undefined ? ' tagValue="' + value + '"' : '')
				+ '>' + label + '<a class="tagit-close">x</a></li>');
		tag.insertBefore(this.input.parent());
		this.tagsArray.push(value === undefined ? label : { label: label, value: value });
		if (this.options.select)
		  this._addSelect(label, value);
		if (this.options.tagsChanged)
		  this.options.tagsChanged(label, 'added', tag);

		return true;
	  }
	});
  })($);

});
