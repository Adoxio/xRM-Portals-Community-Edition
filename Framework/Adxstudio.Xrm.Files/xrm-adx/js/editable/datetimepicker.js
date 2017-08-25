/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

	var ns = XRM.namespace('ui');
	var $ = XRM.jQuery;
	var moment = window.moment;
	var dateFormatConverter = window.dateFormatConverter;

	function DateTimePicker(element, id, language) {
		this._element = $(element);
		this._id = id;
		this._language = language;
		this._rendered = false;
		this._dateOnly = false;
		this._dateFormat = dateFormatConverter.convert(this._element.closest('[data-dateformat]').data('dateformat') || 'M/d/yyyy', dateFormatConverter.dotNet, dateFormatConverter.momentJs);
		this._timeFormat = dateFormatConverter.convert(this._element.closest('[data-timeformat]').data('timeformat') || 'h:mm tt', dateFormatConverter.dotNet, dateFormatConverter.momentJs);
		this._dateTimeFormat = this._dateOnly ?
			this._dateFormat :
			(dateFormatConverter.convert(this._element.closest('[data-datetimeformat]').data('datetimeformat'), dateFormatConverter.dotNet, dateFormatConverter.momentJs) ||
			(this._dateFormat + ' ' + this._timeFormat));
		this._dateIcon = this._element.closest('[data-datetimepicker-date-icon]').data('datetimepicker-date-icon') || 'icon-calendar fa fa-calendar';
		this._timeIcon = this._element.closest('[data-datetimepicker-time-icon]').data('datetimepicker-time-icon') || 'icon-time fa fa-clock-o';
	}

	DateTimePicker.prototype.getSelectedDateTime = function () {
		if (!this._rendered) {
			return null;
		}
		var val = $('#' + this._id + ' input', this._element || document).val().trim();
		if (!val) {
			return null;
		}
		var date = this._element.datetimepicker({locale: this._language}).data('DateTimePicker').date().utc().toDate();

		return date;
	};

	DateTimePicker.prototype.render = function (initialValue) {
		if (this._rendered) {
			return;
		}

		var initialDate,
			pickerConfig,
			pickerElement,
			pickerInput,
			pickerCalendar,
			keyMap = {
				'tab': 9,
				9: 'tab',
				'escape': 27,
				27: 'escape',
				'enter': 13,
				13: 'enter'
			};

		if (initialValue) {
			initialDate = moment(initialValue).toDate();
		}

		pickerConfig = {
			defaultDate: initialDate,
			format: this._dateTimeFormat,
			useCurrent: false,
			useStrict: true,
			locale: this._language,
			icons: {
				time: 'icon-time adx-icon adx-icon-clock-o',
				date: 'icon-calendar adx-icon adx-icon-calendar',
				up: 'icon-chevron-up adx-icon adx-icon-chevron-up',
				down: 'icon-chevron-down adx-icon adx-icon-chevron-down'
			},
			keyBinds: {
				enter: function () {
					this.toggle();
					refreshKeyUp(keyMap.enter);
					restoreTabIndexFocus();
				},
				escape: function () {
					this.hide();
					refreshKeyUp(keyMap.escape);
					restoreTabIndexFocus();
				}
			}
		};

		pickerElement = $('<div />')
				.addClass('input-append input-group datetimepicker')
				.attr('id', this._id);

		pickerInput = $('<input />')
				.attr('type', 'text')
				.attr('placeholder', window.ResourceManager["Datetimepicker_Datepicker_Label"])
				.addClass('form-control')
				.appendTo(pickerElement);

		pickerCalendar = $('<span />')
				.attr('tabindex', '0')
				.attr('aria-label', window.ResourceManager['Datetimepicker_Datepicker_Label'])
				.attr('title', window.ResourceManager['Datetimepicker_Datepicker_Label'])
				.addClass('input-group-addon')
				.append($('<span class="fa fa-calendar" />'))
				.appendTo(pickerElement);

		$(this._element).append(pickerElement)

		pickerElement.datetimepicker(pickerConfig);

		this._rendered = true;

		pickerElement.on('keydown', function (e) {
			if (e.keyCode == keyMap.enter) {
				pickerElement.data('DateTimePicker').show();
			}
		});

		pickerCalendar.on('keyup', function (calendarEvent) {
			var code = calendarEvent.keyCode || calendarEvent.which;
			if (code == keyMap.tab) {
				refreshKeyUp(keyMap.tab);
			}
			else if (code == keyMap.escape) {
				refreshKeyUp(keyMap.escape);
			}
		});

		// datetimepicker v.4.17.47 doesn't handle keyup for enter, tab, escape
		// this is to explicitly clear keyState history
		function refreshKeyUp(pressedKey) {
			var keyUpEvent = $.Event('keyup', { which: pressedKey });
			pickerInput.trigger(keyUpEvent);
		};

		// In IE and Edge browsers after selecting a date focus gets set to the page
		// to avoid that we need explicitly set focus to our active control
		function restoreTabIndexFocus() {
			if (pickerCalendar) {
				pickerCalendar.focus();
			}
		}
	};

	ns.DateTimePicker = DateTimePicker;

});
