/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

if (typeof ADX == "undefined" || !ADX) {
	var ADX = {};
}
ADX.serviceMap = (function ($) {
	var _export = {};
	var _elementId = '';
	var _map = null;
	var _mapOptions = {};
	var _infobox = null;
	var _searchActionURL = "";

	_mapOptions = { credentials: ADX.settings.mapSettings.key, mapTypeId: Microsoft.Maps.MapTypeId.road, center: new Microsoft.Maps.Location(ADX.settings.mapSettings.latitude, ADX.settings.mapSettings.longitude), zoom: ADX.settings.mapSettings.zoom, width: getWidth(), height: getHeight() };
	//_mapOptions = { credentials: ADX.settings.mapSettings.key, mapTypeId: Microsoft.Maps.MapTypeId.road, center: new Microsoft.Maps.Location(ADX.settings.mapSettings.latitude, ADX.settings.mapSettings.longitude), zoom: ADX.settings.mapSettings.zoom, width: ADX.settings.mapSettings.width, height: ADX.settings.mapSettings.height };
	var _mapViewOptions = { mapTypeId: Microsoft.Maps.MapTypeId.road, center: new Microsoft.Maps.Location(ADX.settings.mapSettings.latitude, ADX.settings.mapSettings.longitude), zoom: ADX.settings.mapSettings.zoom };
	var _pushpinSettings = ADX.settings.mapSettings.pushpin;
	var _infoboxSettings = ADX.settings.mapSettings.infobox;
	var _dateFormat = "m/d/yyyy";
	var _statusColorsString = ADX.settings.serviceRequestSettings.statusColors.toString().replace(/\s/g, '');
	var _priorityColorsString = ADX.settings.serviceRequestSettings.priorityColors.toString().replace(/\s/g, '');
	var _statusColors = _statusColorsString.split(";");
	var _statusColorsArray = new Array();
	for (var i = 0; i < _statusColors.length; i++) {
		// split each string into key/value pair
		var statusItem = _statusColors[i].toString().split(":");
		_statusColorsArray[statusItem[0]] = statusItem[1];
	}
	var _priorityColors = _priorityColorsString.split(";");
	var _priorityColorsArray = new Array();
	for (var j = 0; j < _priorityColors.length; j++) {
		// split each string into key/value pair
		var priorityItem = _priorityColors[j].toString().split(":");
		_priorityColorsArray[priorityItem[0]] = priorityItem[1];
	}

	function initialize(elementId, searchActionUrl, autosize) {
		if (!elementId && elementId != '') {
			log('Map elementId was not provided. Map initialization failed.', 'error', 'ADX.serviceMap.initialize');
			return;
		}
		
		if (!ADX.settings.mapSettings.key && !(ADX.settings.mapSettings.key === "")) {

			throw Error;
		}

		_searchActionURL = searchActionUrl;
		$(document).ready(function () {
		  createStatusOverlay($("#" + elementId), $("#" + elementId).position().left + 40, $("#" + elementId).position().top + 60);
			$("#dateFrom").datepicker();
			$("#dateTo").datepicker();
			$("#dates").change(function () {
				if ($(this).val() == 3) {
					$("#datesFilter").show();
				} else {
					$("#datesFilter").hide();
				}
			});
			_elementId = elementId;
			_map = getMap();
			resetSearchOptions();
			if (autosize) {
				resizeMap();
				$(window).resize(function () {
					resizeMap();
				});
			}
			mapIt();
		});
	}

	function resizeMap() {
		var map = null;
		if (_map == null) {
			map = getMap();
		} else {
			map = _map;
		}
		var width = getWidth();
		var height = getHeight();
		_mapOptions.width = width;
		_mapOptions.height = height;
		map.setOptions({ height: height, width: width });
	}

	function getHeight() {
	  var minHeight = 240;
	  var setHeight = ADX.settings.mapSettings.height || minHeight;

	  return setHeight < minHeight ? minHeight : setHeight;
	}

	function getWidth() {
		var width = 320;
		var frameWidth = $("#" + _elementId).parent().width();
		if ((frameWidth) > 0) {
			width = frameWidth - 4;
		}
		return width;
	}

	function createStatusOverlay(element, left, top) {
		$("<div id='mapStatus' style='display:none; border: 1px solid #333; padding: 5px; background: #fff; position: absolute; left: " + left + "px; top: " + top + "px; z-index: 10000;'><img src='~/xrm-adx/samples/images/loader.gif' alt='Loading' />&nbsp;<span id='statusText'></span></div>").appendTo(element);
	}

	function reset() {
		resetMapView();
		resetSearchOptions();
	  mapIt();
	}

	function resetMapView() {
		var map = null;
		if (_map == null) {
			map = getMap();
		} else {
			map = _map;
		}
		map.entities.clear();
		map.setView(_mapViewOptions);
	}

	function resetSearchOptions() {
		// clear the inputs
		$("#location-query").val('');
		$("#dateFrom").val('');
		$("#dateTo").val('');
		$("#datesFilter").hide();
		$("#searchOptions #dates").removeAttr("selected");
		$("#searchOptions #status").removeAttr("selected");
		$("#searchOptions #priority").removeAttr("selected");
		$("#searchOptions #types").removeAttr("selected");
		// set the defaults
		$("#searchOptions #dates option[value='0']").prop("selected", true);
		$("#searchOptions #status option[value='0']").prop("selected", true);
		$("#searchOptions #priority option[value='0']").prop("selected", true);
		$("#searchOptions #types option[value='00000000-0000-0000-0000-000000000000']").prop("selected", true);
		$("#adx-map-show-alerts").prop('checked', true);
	}

	function getMap() {
		var map = new Microsoft.Maps.Map(document.getElementById(_elementId), _mapOptions);

		_infobox = new Microsoft.Maps.Infobox(new Microsoft.Maps.Location(0, 0), { visible: false, offset: new Microsoft.Maps.Point(_infoboxSettings.offset.x, _infoboxSettings.offset.y) });
		Microsoft.Maps.Events.addHandler(map, 'viewchange', function (e) { _infobox.setOptions({ visible: false }); });
		Microsoft.Maps.Events.addHandler(map, 'click', function (e) { _infobox.setOptions({ visible: false }); });

		return map;
	}

	function log(msg, cat, src) {
		try {
			if ((typeof (YAHOO) != 'undefined') && YAHOO && YAHOO.widget && YAHOO.widget.Logger && YAHOO.widget.Logger.log) {
				return YAHOO.widget.Logger.log(msg, cat, src);
			}

			if ((typeof (console) == 'undefined') || !console) {
				return false;
			}

			var source = src ? ('[' + src + ']') : '';

			if (cat == 'warn' && console.warn) {
				console.warn(msg, source);
			}
			else if (cat == 'error' && console.error) {
				console.error(msg, source);
			}
			else if (cat == 'info' && console.info) {
				console.info(msg, source);
			}
			else if (console.log) {
				console.log(msg, source);
			}

			return true;
		}
		catch (e) {
			return false;
		}
	}

	function addPushpin(pushpinLayer, content, latitude, longitude, tether, pushpinImageUrl) {
		var location = new Microsoft.Maps.Location(latitude, longitude);
		var pushpinOptions = {};
		if (pushpinImageUrl != '') {
			pushpinOptions = { icon: pushpinImageUrl, width: _pushpinSettings.width, height: _pushpinSettings.height };
		}
		var pushpin = new Microsoft.Maps.Pushpin(location, pushpinOptions);
		pushpin.InfoboxContent = "<div class='infobox right'>"
								+ "<div class='infobox-arrow'>"
									+ "<div class='infobox-arrow-inner'></div>"
								+ "</div>"
								+ "<div class='infobox-content'>"
									+ content
								+ "</div>"
							+ "</div>";
		
		Microsoft.Maps.Events.addHandler(pushpin, 'click', displayInfobox);
		Microsoft.Maps.Events.addHandler(pushpin, 'mouseover', displayInfobox);
		pushpinLayer.push(pushpin);
	}

	function displayInfobox(e) {
		if (e.targetType == 'pushpin') {
			_infobox.setLocation(e.target.getLocation());
			_infobox.setOptions({ visible: true, htmlContent: e.target.InfoboxContent });
		}
	}

	function mapIt() {
		$(document).ready(function () {
			var map = null;
			if (_map == null) {
				map = getMap();
			} else {
				map = _map;
			}

			var locationQuery = $("#location-query").val();
			if (locationQuery != '') {
				//send location query to bing maps REST api
				$.getJSON(ADX.settings.mapSettings.restServiceUrl
					+ "?q=" + encodeURIComponent(locationQuery)
					+ "&maxRes=1"
					+ "&ul=" + ADX.settings.mapSettings.latitude + "," + ADX.settings.mapSettings.longitude
					+ "&key=" + ADX.settings.mapSettings.key
					+ "&jsonp=?", function(result) {
						if (result.resourceSets[0].estimatedTotal > 0) {
							var bbox = result.resourceSets[0].resources[0].bbox;
							var viewBoundaries = Microsoft.Maps.LocationRect.fromEdges(bbox[0], bbox[1], bbox[2], bbox[3]);
							map.setView({ bounds: viewBoundaries});
						} else {
							alert("sorry that address cannot be found");
						}
					});
			}

			var dateFilterCode = parseInt($("#searchOptions #dates option:selected").val());
			var dateFrom = $("#dateFrom").val();
			var dateTo = $("#dateTo").val();
			if ($("#searchOptions #dates option[value='3']").is(":checked")) {
				if ((!dateFrom || dateFrom == '') || (!dateTo || dateTo == '')) {
					return;
				}
			}
			var statusFilterCode = parseInt($("#searchOptions #status option:selected").val());
			var priorityFilterCode = parseInt($("#searchOptions #priority option:selected").val());
			var types = new Array();
			$("#searchOptions #types option:selected").each(function (index) {
				types[index] = $(this).val();
			});
			$("#statusText").html("Searching for Service Requests...");
			$("#mapStatus").fadeIn();
			map.entities.clear();

			var pushpinLayer = new Microsoft.Maps.EntityCollection();
			map.entities.push(pushpinLayer);

			var infoboxLayer = new Microsoft.Maps.EntityCollection();
			map.entities.push(infoboxLayer);
			infoboxLayer.push(_infobox);

			var includeAlerts = false;
			if ($("#adx-map-show-alerts").prop('checked') == true) {
				includeAlerts = true;
			}
			queryServiceRequestsMapEntities(dateFilterCode, dateFrom, dateTo, statusFilterCode, priorityFilterCode, types, includeAlerts,
				function (data) {
					//success
					//var locations = new Array();
					$.each(data, function (index, entity) {
						//var location = new Microsoft.Maps.Location(entity.Latitude, entity.Longitude);
						//locations.push(location);

						var content = "";

						if (entity.Title != null && entity.Title != '') {
							if (entity.CheckStatusUrl != null && entity.CheckStatusUrl != '') {
								content = "<a href='" + entity.CheckStatusUrl + "' class='title'>" + entity.Title + "</a>";
							} else {
								content = "<span class='title'>" + entity.Title + "</span>";
							}
						}
						if (entity.Description != null && entity.Description != '') {
							content = content + "<p>" + entity.Description + "</p>";
						}
						if (entity.Location != null && entity.Location != '') {
							content = content + "<p>" + entity.Location + "</p>";
						}

						switch (entity.ItemNodeType) {
							case 1:
								content = content
									+ "<p><strong>Service Request #:</strong> " + entity.ServiceRequestNumber + "</p>"
									+ "<p><strong>Status:</strong> <span class='status' style='background:" + getStatusColor(entity.StatusId) + ";'>" + entity.Status + "</span></p>"
									+ "<p><strong>Priority:</strong> <span class='priority' style='color:" + getPriorityColor(entity.PriorityId) + ";'>" + entity.Priority + "</span></p>"
									+ "<p><strong>Incident Date:</strong> " + formatJsonDate(entity.IncidentDate, _dateFormat) + "</p>";
								var scheduledDateString = "";
								var closedDateString = "";
								try {
									scheduledDateString = "<p><strong>Scheduled Date:</strong> " + formatJsonDate(entity.ScheduledDate, _dateFormat) + "</p>";
								} catch (e) { }
								try {
									closedDateString = "<p><strong>Closed Date:</strong> " + formatJsonDate(entity.ClosedDate, _dateFormat) + "</p>";
								} catch (e) { }
								content += scheduledDateString + closedDateString;
								break;
							case 2:
								var scheduledStartDateString = "";
								var scheduledEndDateString = "";
								try {
									scheduledStartDateString = "<p><strong>Scheduled Start Date:</strong> " + formatJsonDate(entity.ScheduledStartDate, _dateFormat) + "</p>";
								} catch (e) { }
								try {
									scheduledEndDateString = "<p><strong>Scheduled End Date:</strong> " + formatJsonDate(entity.ScheduledEndDate, _dateFormat) + "</p>";
								} catch (e) { }
								content = content + scheduledStartDateString + scheduledEndDateString;
						}
						addPushpin(pushpinLayer, content, entity.Latitude, entity.Longitude, true, entity.PushpinImageUrl);
					});
					//if (locations.length > 0) {
					//		map.setView({ bounds: Microsoft.Maps.LocationRect.fromLocations(locations) });
					//}
				},
				function () {
					//error
				},
				function () {
					//completed
					$("#mapStatus").fadeOut();
				}
			);
		});
	}

	function queryServiceRequestsMapEntities(dateFilterCode, dateFrom, dateTo, statusFilterCode, priorityFilterCode, types, includeAlerts, success, error, complete) {
		success = $.isFunction(success) ? success : function () { };
		error = $.isFunction(error) ? error : function () { };
		complete = $.isFunction(complete) ? complete : function () { };
		var url = _searchActionURL;
		var data = {};
		data.dateFilterCode = dateFilterCode;
		data.dateFrom = dateFrom;
		data.dateTo = dateTo;
		data.statusFilterCode = statusFilterCode;
		data.priorityFilterCode = priorityFilterCode;
		data.types = types;
		data.includeAlerts = includeAlerts;
		var jsonData = JSON.stringify(data);
		shell.ajaxSafePost({
			type: 'POST',
			dataType: "json",
			contentType: 'application/json',
			url: url,
			data: jsonData,
			global: false,
			success: success,
			error: function (event, xhr, ajaxOptions, thrownError) {
				//errorHandler(event, xhr, ajaxOptions, thrownError);
				error.call(this, event, xhr, ajaxOptions, thrownError);
			},
			complete: complete
		});
	}

	function getStatusColor(id) {
		if (!id || id == '') {
			return '';
		}
		var color = _statusColorsArray[id];
		if (color == undefined) {
			return '';
		}
		return color;
	}

	function getPriorityColor(id) {
		if (!id || id == '') {
			return '';
		}
		var color = _priorityColorsArray[id];
		if (color == undefined) {
			return '';
		}
		return color;
	}

	function formatJsonDate(dateString, format) {
		if (!dateString) {
			return '';
		}
		dateString = dateString.dateFromJSON();
		var date = new Date(dateString);
		return date.format(format);
	}

	String.prototype.dateFromJSON = function () {
		return eval(this.replace(/\/Date\((.*?)\)\//gi, "new Date($1)"));
	};

	_export.initialize = initialize;

	_export.getMap = getMap;

	_export.mapIt = mapIt;

	_export.resetSearchOptions = resetSearchOptions;

	_export.resetMapView = resetMapView;

	_export.reset = reset;

	return _export;

})(jQuery);
