/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


(function() {

	if (typeof YAHOO == "undefined" || !YAHOO || typeof YAHOO.widget == "undefined" || !YAHOO.widget || typeof YAHOO.widget.Calendar == "undefined" || !YAHOO.widget.Calendar) {
		if (typeof console == "undefined" || !console || typeof console.info == "undefined" || !console.info) {
			console.info("'YAHOO.widget.Calendar' is undefined. Include a reference to this YUI 2 component to enable rich date picker functionality with CrmEntityFormView.");
		}
		return;
	}
	
	var dateFormat = function () {
		var	token = /d{1,4}|m{1,4}|yy(?:yy)?|([HhMsTt])\1?|[LloSZ]|"[^"]*"|'[^']*'/g,
			timezone = /\b(?:[PMCEA][SDP]T|(?:Pacific|Mountain|Central|Eastern|Atlantic) (?:Standard|Daylight|Prevailing) Time|(?:GMT|UTC)(?:[-+]\d{4})?)\b/g,
			timezoneClip = /[^-+\dA-Z]/g,
			pad = function (val, len) {
				val = String(val);
				len = len || 2;
				while (val.length < len) val = "0" + val;
				return val;
			};

		// Regexes and supporting functions are cached through closure
		return function (date, mask, utc) {
			var dF = dateFormat;

			// You can't provide utc if you skip other args (use the "UTC:" mask prefix)
			if (arguments.length == 1 && Object.prototype.toString.call(date) == "[object String]" && !/\d/.test(date)) {
				mask = date;
				date = undefined;
			}

			// Passing date through Date applies Date.parse, if necessary
			date = date ? new Date(date) : new Date;
			if (isNaN(date)) return;

			mask = String(dF.masks[mask] || mask || dF.masks["default"]);

			// Allow setting the utc argument via the mask
			if (mask.slice(0, 4) == "UTC:") {
				mask = mask.slice(4);
				utc = true;
			}

			var	_ = utc ? "getUTC" : "get",
				d = date[_ + "Date"](),
				D = date[_ + "Day"](),
				m = date[_ + "Month"](),
				y = date[_ + "FullYear"](),
				H = date[_ + "Hours"](),
				M = date[_ + "Minutes"](),
				s = date[_ + "Seconds"](),
				L = date[_ + "Milliseconds"](),
				o = utc ? 0 : date.getTimezoneOffset(),
				flags = {
					d:    d,
					dd:   pad(d),
					ddd:  dF.i18n.dayNames[D],
					dddd: dF.i18n.dayNames[D + 7],
					m:    m + 1,
					mm:   pad(m + 1),
					mmm:  dF.i18n.monthNames[m],
					mmmm: dF.i18n.monthNames[m + 12],
					yy:   String(y).slice(2),
					yyyy: y,
					h:    H % 12 || 12,
					hh:   pad(H % 12 || 12),
					H:    H,
					HH:   pad(H),
					M:    M,
					MM:   pad(M),
					s:    s,
					ss:   pad(s),
					l:    pad(L, 3),
					L:    pad(L > 99 ? Math.round(L / 10) : L),
					t:    H < 12 ? "a"  : "p",
					tt:   H < 12 ? "am" : "pm",
					T:    H < 12 ? "A"  : "P",
					TT:   H < 12 ? "AM" : "PM",
					Z:    utc ? "UTC" : (String(date).match(timezone) || [""]).pop().replace(timezoneClip, ""),
					o:    (o > 0 ? "-" : "+") + pad(Math.floor(Math.abs(o) / 60) * 100 + Math.abs(o) % 60, 4),
					S:    ["th", "st", "nd", "rd"][d % 10 > 3 ? 0 : (d % 100 - d % 10 != 10) * d % 10]
				};

			return mask.replace(token, function ($0) {
				return $0 in flags ? flags[$0] : $0.slice(1, $0.length - 1);
			});
		};
	}();

	// Some common format strings
	dateFormat.masks = {
		"default":      "ddd mmm dd yyyy HH:MM:ss",
		shortDate:      "m/d/yy",
		mediumDate:     "mmm d, yyyy",
		longDate:       "mmmm d, yyyy",
		fullDate:       "dddd, mmmm d, yyyy",
		shortTime:      "h:MM TT",
		mediumTime:     "h:MM:ss TT",
		longTime:       "h:MM:ss TT Z",
		isoDate:        "yyyy-mm-dd",
		isoTime:        "HH:MM:ss",
		isoDateTime:    "yyyy-mm-dd'T'HH:MM:ss",
		isoUtcDateTime: "UTC:yyyy-mm-dd'T'HH:MM:ss'Z'"
	};

	// Internationalization strings
	dateFormat.i18n = {
		dayNames: [
			"Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat",
			"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
		],
		monthNames: [
			"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
			"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
		]
	};
	
	var Dom = YAHOO.util.Dom, Event = YAHOO.util.Event;
	
	function each(arr, fn) {
		for (var i = 0; i < arr.length; i++) {
			fn(arr[i]);
		}
	}
	
	function getDateTimeValue(cell) {
		
		var format = function(date) {
			return dateFormat(date, "isoUtcDateTime");
		};
				
		var dateInput = Dom.getElementsByClassName('date', 'input', cell)[0];
		
		if (!dateInput || !dateInput.value) {
			return '';
		}
		
		var date = new Date(Date.parse(dateInput.value));
		
		if (!date) {
			return dateInput.value;
		}
		
		var timeInput = Dom.getElementsByClassName('time', 'select', cell)[0];
		
		if (!timeInput || !timeInput.value) {
			return format(date);
		}
		
		var timeParts = timeInput.value.split(':');
		
		if (timeParts.length != 2) {
			return format(date);
		}
		
		date.setHours(timeParts[0]);
		date.setMinutes(timeParts[1]);
		
		return format(date);
	}
	
	YAHOO.util.Event.onDOMReady(function() {
		var cells = Dom.getElementsByClassName('datetime-cell');
		
		each(cells, function(cell) {
			var datetime;

			each(Dom.getElementsByClassName('datetime', 'input', cell), function(e) {
				e.style.display = "none";
				datetime = new Date(e.value);
			});
			
			each(Dom.getElementsByClassName('date', 'input', cell), function(e) {
				e.style.display = '';

				if (!isNaN(datetime)) {
					e.value = datetime.getMonth() + 1 + "/" + datetime.getDate() + "/" + datetime.getFullYear();
				}
				
				var dialogId = Dom.generateId();
				var calendarId = Dom.generateId();
				
				var container = document.createElement('span');
				
				Dom.insertAfter(container, e);
				Dom.addClass(container, 'yui-skin-sam');
				
				var dialog = new YAHOO.widget.Overlay(dialogId, {
					visible: false,
					context: [e.id, 'tl', 'bl', ["beforeShow", "windowResize"]],
					constraintoviewport: true
				});
				
				dialog.setBody('<div id="' + calendarId + '"></div>');
				dialog.render(container);
				
				var calendar = new YAHOO.widget.Calendar(calendarId, { iframe: false });
				
				calendar.render();
				
				calendar._dateSelected = false;
				
				calendar.selectEvent.subscribe(function() {
					var date = calendar.getSelectedDates()[0];
					
					if (date) {
						e.value = dateFormat(date, 'mm/dd/yyyy');
					}
					
					calendar._dateSelected = true;
					e.focus();
					dialog.hide();
				});
				
				Event.on(document, 'click', function(eargs) {
					var target = Event.getTarget(eargs);
					var dialogEl = dialog.element;
					if (target != dialogEl && !Dom.isAncestor(dialogEl, target) && target != e && !Dom.isAncestor(e, target)) {
						dialog.hide();
					}
				});
				
				Event.on(e, 'focus', function() {
					if (!calendar._dateSelected) {
						calendar._dateSelected = false;
						dialog.show();
					}
				});
				
				Event.on(e, 'click', function() {
					dialog.show();
				});
				
				Event.on(e, 'keydown', function() {
					dialog.hide();
				});
			});
			
			each(Dom.getElementsByClassName('time', 'select', cell), function(e) {
				e.style.display = '';

				if (!isNaN(datetime)) {
					var time = datetime.getHours() + ":" + datetime.getMinutes();					
					for (var i=0; i<e.options.length; i++) {
						if (e.options[i].value == time) {
							e.selectedIndex = i;
							break;
						}
					}
				}
			});
			
			var form = Dom.getAncestorByTagName(cell, 'form');
			
			if (!form) return;
			
			Event.addListener(form, 'submit', function() {
				each(Dom.getElementsByClassName('datetime', 'input', cell), function(e) {
					e.value = getDateTimeValue(cell);
				});
			});
		});
	});

})();
