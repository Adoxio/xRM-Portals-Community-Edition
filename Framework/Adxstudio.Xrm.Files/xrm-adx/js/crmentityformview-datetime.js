/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function($) {

	if (!($)) {
		return;
	}

	$(function() {
		var moment = window.moment;
		var dateFormatConverter = window.dateFormatConverter;

		if (!(moment && $.isFunction($.fn.datetimepicker))) {
			return;
		}

		$('input[data-ui="datetimepicker"]').each(function(_, e) {
			var $e = $(e),
				readonly = $e.is('[readonly]'),
				disabled = $e.is('[disabled]'),
				pickerDisabled = $e.closest('[data-datetimepicker-disabled]').data('datetimepicker-disabled'),
				pickerElement,
				pickerInput,
			    pickerCalendar,
				picker,
				pickerConfig,
				roundTripUserLocalFormat = 'YYYY-MM-DDTHH:mm:ss.0000000\\Z',
				roundTripFormat = 'YYYY-MM-DDTHH:mm:ss.0000000',
				dateOnly = $e.data('type') === 'date',
				dateFormat = dateFormatConverter.convert($e.closest('[data-dateformat]').data('dateformat') || 'M/d/yyyy', dateFormatConverter.dotNet, dateFormatConverter.momentJs),
				timeFormat = dateFormatConverter.convert($e.closest('[data-timeformat]').data('timeformat') || 'h:mm tt', dateFormatConverter.dotNet, dateFormatConverter.momentJs),
				dateTimeFormat = dateOnly ? dateFormat : (dateFormatConverter.convert($e.closest('[data-datetimeformat]').data('datetimeformat'), dateFormatConverter.dotNet, dateFormatConverter.momentJs) || (dateFormat + ' ' + timeFormat)),
				language = $e.closest('[lang]').attr('lang'),
				dateIcon = $e.closest('[data-datetimepicker-date-icon]').data('datetimepicker-date-icon') || 'icon-calendar fa fa-calendar',
				timeIcon = $e.closest('[data-datetimepicker-time-icon]').data('datetimepicker-time-icon') || 'icon-time fa fa-clock-o',
				loadEvent,
				changeEvent,
				lastGoodValue = "",
				isUTC = ($e.closest('[data-behavior]').data('behavior') == "UserLocal"),
			    keyMap = {
			        'tab': 9,
			        9: 'tab',
			        'escape': 27,
			        27: 'escape',
			        'enter': 13,
			        13: 'enter'
			    };

			if (pickerDisabled) {
				return;
			}

			pickerConfig = {
                format: dateTimeFormat,
				useCurrent: false,
				useStrict: true,
				locale: language,
				icons: {
					time: 'icon-time fa fa-clock-o',
					date: 'icon-calendar fa fa-calendar',
					up: 'icon-chevron-up fa fa-chevron-up',
					down: 'icon-chevron-down fa fa-chevron-down'
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

			loadEvent = $.Event('adx:datetimepicker:init', {
				element: e,
				config: pickerConfig
			});

			$e.trigger(loadEvent);

			if (loadEvent.isDefaultPrevented()) {
				return;
			}

			$e.hide();

			$("<span>").addClass("sr-only").attr("id", e.id + "_description")
				.text('Date/time format "' + dateTimeFormat + "'")
				.insertBefore(e);

			pickerElement = $('<div />')
				.addClass('input-append input-group datetimepicker')
				.insertAfter($e);

			pickerInput = $('<input />')
				.attr('type', 'text')
				.attr('data-date-format', dateTimeFormat)
				.attr("aria-describedby", e.id + "_description")
				.attr("aria-labelledby", e.id + "_label")
				.addClass('form-control')
				.appendTo(pickerElement);

			pickerCalendar = $('<span />')
				.attr('tabindex', '0')
				.attr("aria-label", window.ResourceManager['Datetimepicker_Datepicker_Label'])
				.attr("title", window.ResourceManager['Datetimepicker_Datepicker_Label'])
				.addClass('input-group-addon')
				.append($('<span />').attr('data-date-icon', dateIcon).attr('data-time-icon', timeIcon).addClass(dateIcon))
				.appendTo(pickerElement);

			pickerElement.datetimepicker(loadEvent.config);

			picker = pickerElement.data('DateTimePicker');

			var currentValue = isUTC ? moment.utc($e.val()) : moment($e.val());

			if (currentValue.isValid()) {
				picker.date(currentValue.toDate());
			}

			if (readonly) {
				pickerElement.find('input').attr('readonly', 'readonly').addClass('readonly');
			}

			if (disabled) {
				pickerElement.find('input').attr('disabled', 'disabled').addClass('aspNetDisabled');
			}

			if (readonly || disabled) {
				pickerElement.removeClass('input-append').removeClass('input-group').find('.input-group-addon').hide();

				return;
			}

			pickerElement.on('dp.change', function(ev) {
				if (ev.date) {
					var localDate = moment(ev.date);

					// if format doesn't contatin seconds - reset them to 0
					if (!loadEvent.config.format || loadEvent.config.format.toLowerCase().indexOf('s') < 0) {
						localDate = localDate.seconds(0);
					}

					// if format doesn't contatin hours - time is not supported
					// reset hours, minutes, seconds to 0
					if (!loadEvent.config.format || loadEvent.config.format.toLowerCase().indexOf('h') < 0) {
						localDate = localDate.hours(0).minutes(0).seconds(0);
					}

					changeEvent = $.Event('adx:datetimepicker:change', {
						element: e,
						date: localDate.toDate()
					});

					$e.trigger(changeEvent);

					if (changeEvent.isDefaultPrevented()) {
						return;
					}

					$e.val(changeEvent.date ? (isUTC ? moment(changeEvent.date).utc().format(roundTripUserLocalFormat) : moment(changeEvent.date).format(roundTripFormat)) : '');
				} else {
					changeEvent = $.Event('adx:datetimepicker:change', {
						element: e,
						date: null
					});

					$e.trigger(changeEvent);

					if (changeEvent.isDefaultPrevented()) {
						return;
					}

					$e.val(changeEvent.date ? (isUTC ? moment(changeEvent.date).utc().format(roundTripUserLocalFormat) : moment(changeEvent.date).format(roundTripFormat)) : '');
				}
			}).on("change", function() {
				var val = pickerInput.val().trim();
				if (val) {
					var d = moment(val, dateTimeFormat, true);
					if (!d.isValid()) {
						if (lastGoodValue) {
							d = moment(lastGoodValue, dateTimeFormat, true);
							$e.val(isUTC ? d.utc().format(roundTripUserLocalFormat) : d.format(roundTripFormat));
							pickerInput.val(d.local().format(dateTimeFormat));
						} else {
							$e.val('');
							pickerInput.val('');
						}
					} else {
						lastGoodValue = val;
					}
				} else {
					$e.val('');
					pickerInput.val('');
					lastGoodValue = '';
				}
			}).on('keydown', function (e) {
				if (e.keyCode == keyMap.enter) {
					pickerElement.data('DateTimePicker').show();
				}
			});

		    
		    pickerCalendar.on('keyup', function(calendarEvent){
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

			// In IE and Microsoft Edge browsers after selecting a date focus gets set to the page
			// to avoid that we need explicitly set focus to our active control
			function restoreTabIndexFocus() {
				if (pickerCalendar) {
					pickerCalendar.focus();
				}
			}
		});
	});

})(window.jQuery);
