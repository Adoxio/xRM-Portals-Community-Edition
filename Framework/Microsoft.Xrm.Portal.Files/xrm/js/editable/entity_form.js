/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

	var ns = XRM.namespace('editable.Entity.Form');
	var $ = XRM.jQuery;

	var initializing = false, fnTest = /xyz/.test(function () { xyz; }) ? /\b_super\b/ : /.*/;

	// The base Class implementation (does nothing)
	var Class = function () { };

	// Create a new Class that inherits from this class
	Class.extend = function (prop) {
		var _super = this.prototype;

		// Instantiate a base class (but only create the instance,
		// don't run the init constructor)
		initializing = true;
		var prototype = new this();
		initializing = false;

		// Copy the properties over onto the new prototype
		for (var name in prop) {
			// Check if we're overwriting an existing function
			prototype[name] = typeof prop[name] == "function" &&
				typeof _super[name] == "function" && fnTest.test(prop[name]) ?
				(function (name, fn) {
					return function () {
						var tmp = this._super;

						// Add a new ._super() method that is the same method
						// but on the super-class
						this._super = _super[name];

						// The method only need to be bound temporarily, so we
						// remove it when we're done executing
						var ret = fn.apply(this, arguments);
						this._super = tmp;

						return ret;
					};
				})(name, prop[name]) :
				prop[name];
		}

		// The dummy class constructor
		function Class() {
			// All construction is actually done in the init method
			if (!initializing && this.init)
				this.init.apply(this, arguments);
		}

		// Populate our constructed prototype object
		Class.prototype = prototype;

		// Enforce the constructor to be what we expect
		Class.constructor = Class;

		// And make this class extendable
		Class.extend = arguments.callee;

		return Class;
	};

	var FieldType = ns.FieldType = Class.extend({
		init: function (entityName, data) {
			this.id = data.id || this._getId(data.name);
			this.label = XRM.localize(entityName + '.' + data.name) || data.label || data.name;
			this.name = data.name;
			this.propertyName = XRM.editable.Entity.getPropertyName(entityName, data.name);
			this.required = data.required;
			this.requiresLoading = !!data.uri;
			this.slugify = data.slugify;
			this.uri = data.uri;

			this._data = data;
			this._entityName = entityName;
			this._initialValue = data.value;
		},

		appendData: function (data, container) {
			var value = this.getValue(container);

			if (this._data.excludeEmptyData && ((typeof (value) === 'undefined') || (value === null) || (value === ''))) {
				return;
			}

			data[this.propertyName] = value;

			if (this._data.mirror) {
				data[XRM.editable.Entity.getPropertyName(this._entityName, this._data.mirror)] = value;
			}

			return data;
		},

		getValue: function (container) {
			return $('#' + this.id, container || document).val();
		},

		load: function (data, container) { },

		render: function (container, label) { },

		show: function (dialog) { },

		validate: function (container) {
			return !!(!this.required || this.getValue(container));
		},

		_getId: function (fieldName) {
			return 'xrm-editable-' + fieldName;
		}
	});

	var types = ns.fieldTypes = {};

	types.text = FieldType.extend({
		render: function (container, label) {
			var textbox = $('<input />').attr('type', 'text').attr('id', this.id).attr('name', this.id).addClass('xrm-text').appendTo(container);

			if (typeof (this._initialValue) !== 'undefined' && this._initialValue !== null) {
				textbox.val(this._initialValue || '');
			}
		}
	});

	types.html = FieldType.extend({
		getValue: function (container) {
			return this._editor ? this._editor.getContent() : $('#' + this.id, container || document).val(); ;
		},

		render: function (container, label) {
			var textbox = $('<textarea />').attr('id', this.id).attr('name', this.id).addClass('xrm-html').appendTo(container);

			if (typeof (this._initialValue) !== 'undefined' && this._initialValue !== null) {
				textbox.val(this._initialValue || '');
			}

			if (typeof (tinymce) !== 'undefined') {
				this._editor = new tinymce.Editor(textbox.attr('id'), XRM.tinymceCompactSettings);
			}
			else {
				this._editor = null;
			}
		},

		show: function (dialog) {
			var editor = this._editor;
			if (editor) {
				editor.onClick.add(function () {
					if (dialog && dialog.focus) dialog.focus();
				});
				editor.render();
			}
		}
	});

	types.checkbox = FieldType.extend({
		getValue: function (container) {
			return $('#' + this.id, container || document).is(':checked');
		},

		render: function (container, label) {
			label.addClass('xrm-checkbox');
			var checkbox = $('<input />').attr('type', 'checkbox').attr('id', this.id).attr('name', this.id).prependTo(label);

			if (typeof (this._initialValue) !== 'undefined' && this._initialValue !== null) {
				checkbox.attr('checked', this._initialValue);
			}
		}
	});

	types.select = FieldType.extend({
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

			var isSelected = (field.value != null && typeof (field.value) !== 'undefined')
				? function (item) { return item[valuePropertyName] === (field.value.Id || field.value); }
				: function (item) { return !!item[isDefaultPropertyName]; };

			// Populate select with retrieved items.
			$.each(items, function (i, item) {
				$('<option />').val(item[valuePropertyName]).text(item[textPropertyName]).attr('selected', isSelected(item)).appendTo(element);
			});
		},

		appendData: function (data, container) {
			var value = this.getValue(container);

			if ((typeof (value) === 'undefined') || (value === null) || (value === '')) {
				if (!this._data.excludeEmptyData) {
					data[this.propertyName] = null;
				}

				return;
			}

			data[this.propertyName] = { "Id": value, "LogicalName": this._data.optionEntityName };

			return data;
		},

		render: function (container, label) {
			var select = $('<select />').attr('id', this.id).attr('name', this.id).appendTo(container);

			if (!this.required) {
				$('<option />').val('').text('').attr('selected', 'selected').appendTo(select);
			}
		}
	});

	types.datetime = FieldType.extend({
		getValue: function (container) {
			if (!this._picker) {
				return null;
			}
			var datetime = this._picker.getSelectedDateTime();
			return datetime ? XRM.util.formatDateForDataService(datetime) : null;
		},

		render: function (container, label) {
			var picker = new XRM.ui.DateTimePicker(container, this.id);
			picker.render();

			if (typeof (this._initialValue) !== 'undefined' && this._initialValue !== null) {
				picker.setSelectedDateTime(this._initialValue);
			}

			this._picker = picker;
		}
	});

	types.file = FieldType.extend({
		appendData: function (data, container) { },

		upload: function (data, onComplete) {
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
				.hide()
				.appendTo(document.body);

			$('#' + this.id).wrap(form);

			YAHOO.util.Connect.setForm(form.attr('id'), true, true);

			YAHOO.util.Connect.asyncRequest('POST', uploadUri, {
				upload: function (response) {
					form.remove();
					onComplete(this);
				}
			});
		},

		render: function (container, label) {
			var fileInput = $('<input />').attr('type', 'file').attr('id', this.id).attr('name', this.id).addClass('xrm-file').appendTo(container);

			var self = this;
			var field = this._data;

			fileInput.change(function () {
				if (field.copyFilenameTo) {
					var copyTo = $('#' + self._getId(field.copyFilenameTo));

					if (!copyTo.val()) {
						copyTo.val(self._getFilenameFromFileInput(fileInput));
					}
				}

				if (field.copyFilenameSlugTo) {
					var copyTo = $('#' + self._getId(field.copyFilenameSlugTo));

					if (!copyTo.val()) {
						copyTo.val(XRM.util.slugify(self._getFilenameFromFileInput(fileInput)));
					}
				}
			});
		},

		_getFilenameFromFileInput: function (fileInput) {
			var rawValue = fileInput.val();

			if (!rawValue) {
				return '';
			}

			var parts = rawValue.split('\\');

			return parts[parts.length - 1];
		}
	});

});
