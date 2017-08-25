/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {

	var ns = XRM.namespace('ui');
	var Event = YAHOO.util.Event, Dom = YAHOO.util.Dom, $ = XRM.jQuery;
	
	function DateTimePicker(element, id) {
		this._element = $(element);
		this._id = id;
		this._rendered = false;
	}
	
	DateTimePicker.prototype.getSelectedDateTime = function() {
		if (!this._rendered) {
			return null;
		}
		
		var month = this._monthSelect.val();
		var day = this._dateSelect.val();
		var year = this._yearInput.val();
		var time = this._timeSelect.val();
		
		var splitTime = (time || '00:00').split(':');
		
		if (year && month && day && splitTime.length == 2) {
			var datetime = new Date();
			
			datetime.setFullYear(year, month, day);
			
			datetime.setHours(splitTime[0]);
			datetime.setMinutes(splitTime[1]);
			datetime.setSeconds(0, 0);
			
			return datetime;
		}
		
		return null;
	}
	
	var dataServiceDateRegexp = /Date\((\d+)\)/;
	
	function parseDateTime(value) {
		if (typeof(value) === 'string') {
			var standardParse = Date.parse(value);
			
			if (standardParse) {
				return standardParse;
			}
			
			var captures = dataServiceDateRegexp.exec(value);
			
			if (captures && captures.length > 1) {
				var ticks = parseFloat(captures[1]);
				
				if (typeof (ticks) === 'number') {
					return new Date(ticks);
				}
			}
					
			return null;
		}
		else {
			return new Date(value);
		}
	}
	
	function getTimeString(hours, minutes) {
		var h = (hours < 10) ? ('0' + hours) : ('' + hours);
		var m = (minutes < 30) ? '00' : '30';
		return h + ':' + m;
	}
	
	DateTimePicker.prototype.setSelectedDateTime = function(value) {
		var datetime = parseDateTime(value);
		
		if (!this._rendered) {
			this._selectedDateTime = datetime;
			return;
		}
		
		this._yearInput.val(datetime.getFullYear());
		this._monthSelect.val(datetime.getMonth());
		this._dateSelect.val(datetime.getDate());
		this._timeSelect.val(getTimeString(datetime.getHours(), datetime.getMinutes()));
		this._dateButton.set("label", datetime.toLocaleDateString());
	}
		
	DateTimePicker.prototype.render = function() {
		if (this._rendered) {
			return;
		}
		
		var element = this._element, id = this._id;
		
		// Create the DOM required to support our picker.
		var container = $('<div />').addClass('xrm-datetime').attr('id', id).appendTo(element);
		var dateFields = $('<div />').addClass('xrm-date').attr('id', id + '-datefields').appendTo(container);
		var timeFields = $('<div />').addClass('xrm-time').appendTo(container);
		
		function createSelect(name, selectContainer) {
			return $('<select />').attr('id', id + '-' + name).attr('name', id + '-' + name).appendTo(selectContainer);
		}
		
		var time = this._timeSelect = createSelect('time', timeFields);
		
		// Populate time select.
		for (var i = 0; i < 24; i++) {
			var onThehour = getTimeString(i, 0);
			$('<option />').val(onThehour).text(onThehour).appendTo(time);
			var onTheHalfHour = getTimeString(i, 30);
			$('<option />').val(onTheHalfHour).text(onTheHalfHour).appendTo(time);
		}
		
		var month = this._monthSelect = createSelect('month', dateFields).addClass('xrm-date-month').hide();
		
		$('<option />').val('').text('').appendTo(month);
		
		for (var i = 0; i < 12; i++) {
			$('<option />').val(i).text(i).appendTo(month);
		}
		
		var day = this._dateSelect = createSelect('day', dateFields).addClass('xrm-date-day').hide();
		
		$('<option />').val('').text('').appendTo(day);
		
		for (var i = 1; i <= 31; i++) {
			$('<option />').val(i).text(i).appendTo(day);
		}
		
		var year = this._yearInput = $('<input />').addClass('xrm-date-year').hide().attr('type', 'text').attr('id', id + '-year').attr('name', id + '-year').appendTo(dateFields);
		
		var calendarMenu, dateButton;
		
		function onDateButtonClick() {
			var calendarID = id + '-buttoncalendar';
			
			// Create a Calendar instance and render it into the body 
			// element of the Overlay.
			var calendar = new YAHOO.widget.Calendar(calendarID, calendarMenu.body.id);
			
			calendar.render();
			
			// Subscribe to the Calendar instance's "select" event to 
			// update the Button instance's label when the user
			// selects a date.
			calendar.selectEvent.subscribe(function (p_sType, p_aArgs) {
				var aDate, nMonth, nDay, nYear;
				
				if (p_aArgs) {
					aDate = p_aArgs[0][0];
					nMonth = aDate[1];
					nDay = aDate[2];
					nYear = aDate[0];
					
					var selectedDate = new Date();
					selectedDate.setFullYear(nYear, nMonth - 1, nDay);
					dateButton.set("label", selectedDate.toLocaleDateString());
					
					// Sync the Calendar instance's selected date with the date form fields.
					month.val(nMonth - 1);
					day.val(nDay);
					year.val(nYear);
				}
				
				calendarMenu.hide();
			})
			
			// Pressing the Esc key will hide the Calendar Menu and send focus back to 
			// its parent Button.
			Event.on(calendarMenu.element, "keydown", function (p_oEvent) {
				if (Event.getCharCode(p_oEvent) === 27) {
					calendarMenu.hide();
					this.focus();
				}
			}, null, this);
			
			var focusDay = function () {
				var oCalendarTBody = Dom.get(calendarID).tBodies[0],
					aElements = oCalendarTBody.getElementsByTagName("a"),
					oAnchor;
					
				if (aElements.length > 0) {
					Dom.batch(aElements, function (element) {
						if (Dom.hasClass(element.parentNode, "today")) {
							oAnchor = element;
						}
					});
					
					if (!oAnchor) {
						oAnchor = aElements[0];
					}
					
					// Focus the anchor element using a timer since Calendar will try 
					// to set focus to its next button by default.
					YAHOO.lang.later(0, oAnchor, function () {
						try {
							oAnchor.focus();
						}
						catch(e) {}
					});
				}
			};
			
			// Set focus to either the current day, or first day of the month in 
			// the Calendar when it is made visible or the month changes.
			calendarMenu.subscribe("show", focusDay);
			calendar.renderEvent.subscribe(focusDay, calendar, true);
			
			// Give the Calendar an initial focus.
			focusDay.call(calendar);
			
			// Re-align the CalendarMenu to the Button to ensure that it is in the correct
			// position when it is initial made visible.
			calendarMenu.align();
			
			// Unsubscribe from the "click" event so that this code is 
			// only executed once.
			this.unsubscribe("click", onDateButtonClick);
		}
		
		// Create a Overlay instance to house the Calendar instance.
		calendarMenu = new YAHOO.widget.Overlay(id + "-calendarmenu", { visible: false, iframe: true });
		
		// Create a Button instance of type "menu".
		dateButton = new YAHOO.widget.Button({ 
			type: "menu", 
			id: id + "-calendarpicker", 
			label: XRM.localize('datetimepicker.datepicker.label'), 
			menu: calendarMenu, 
			container: dateFields.attr('id') 
		});
		
		dateButton.on("appendTo", function () {
			// Create an empty body element for the Overlay instance in order 
			// to reserve space to render the Calendar instance into.
			calendarMenu.setBody("&#32;");
			calendarMenu.body.id = id + "-calendarcontainer";
		});
		
		// Add a listener for the "click" event.  This listener will be
		// used to defer the creation the Calendar instance until the 
		// first time the Button's Overlay instance is requested to be displayed
		// by the user.
		dateButton.on("click", onDateButtonClick);
		
		this._dateButton = dateButton;
		this._rendered = true;
		
		if (this._selectedDateTime) {
			this.setSelectedDateTime(this._selectedDateTime);
		}
	}
	
	ns.DateTimePicker = DateTimePicker;
		
});
