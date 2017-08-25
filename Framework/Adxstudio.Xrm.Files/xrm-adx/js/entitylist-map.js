/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

if (typeof ADX == "undefined" || !ADX) {
	var ADX = {};
}

ADX.entityListMap = (function($) {
	var _export = {};
	var _searchUrl = null;
	var _location = null;
	var _mapElementId = '';
	var _$mapElement = null;
	var _map = null;
	var _mapDirectionsManager = null;
	var _mapLocationQueryRestUrl = '';
	var _mapUserLocation = '';
	var _distanceUnits = '';
	var _mapOptions = null;
	var _mapViewOptions = null;
	var _pushpinSettings = {
		height: 39,
		width: 32,
		url: ''
	};
	var _infoboxSettings = {
		offset: {
			x: 25,
			y: 46
		}
	};

	function createDefaultMapOptions() {
		return {
			credentials: '',
			mapTypeId: Microsoft.Maps.MapTypeId.road,
			center: new Microsoft.Maps.Location(0, 0),
			zoom: 1
		};
	}

	function createDefaultViewOptions() {
		return {
			mapTypeId: Microsoft.Maps.MapTypeId.road,
			center: new Microsoft.Maps.Location(_mapOptions.center.latitude, _mapOptions.center.longitude),
			zoom: _mapOptions.zoom
		};
	}
	
	function initialize(mapElementId, searchQueryUrl, autosize) {
		if (!mapElementId && mapElementId != '') {
			return;
		}
		_mapElementId = mapElementId;
		if (searchQueryUrl != null && searchQueryUrl != '') {
			_searchUrl = searchQueryUrl;
		}
		$(document).ready(function () {
			_$mapElement = $("#" + _mapElementId);
			createStatusOverlay(_$mapElement, _$mapElement.position().left + 40, _$mapElement.position().top + 60);
			var credentials = _$mapElement.attr("data-credentials");
			var restUrl = _$mapElement.attr("data-rest-url");
			var zoom = _$mapElement.attr("data-zoom");
			var latitude = _$mapElement.attr("data-latitude");
			var longitude = _$mapElement.attr("data-longitude");
			var pushpinUrl = _$mapElement.attr("data-pushpin-url");
			var pushpinHeight = _$mapElement.attr("data-pushpin-height");
			var pushpinWidth = _$mapElement.attr("data-pushpin-width");
			var infoboxOffsetX = _$mapElement.attr("data-infobox-offset-x");
			var infoboxOffsetY = _$mapElement.attr("data-infobox-offset-y");
			var searchUrl = _$mapElement.attr("data-search-url");
			var distanceUnits = _$mapElement.attr("data-distance-units");
			resizeMap();
			window.onload = function() {
				_mapOptions = createDefaultMapOptions();
				_mapViewOptions = createDefaultViewOptions();
				if (credentials != null && credentials != '') {
					_mapOptions.credentials = credentials;
				}
				if (restUrl != null && restUrl != '') {
					_mapLocationQueryRestUrl = restUrl;
				}
				if ((latitude != null && latitude != '') && (longitude != null && longitude != '')) {
					_mapOptions.center = new Microsoft.Maps.Location(parseFloat(latitude), parseFloat(longitude));
				}
				if (zoom != null && zoom != '') {
					_mapOptions.zoom = parseInt(zoom);
				}
				if (pushpinUrl != null && pushpinUrl != '') {
					_pushpinSettings.url = pushpinUrl;
				}
				if (pushpinHeight != null && pushpinHeight != '') {
					_pushpinSettings.height = parseInt(pushpinHeight);
				}
				if (pushpinWidth != null && pushpinWidth != '') {
					_pushpinSettings.width = parseInt(pushpinWidth);
				}
				if (infoboxOffsetX != null && infoboxOffsetX != '') {
					_infoboxSettings.offset.x = parseInt(infoboxOffsetX);
				}
				if (infoboxOffsetY != null && infoboxOffsetY != '') {
					_infoboxSettings.offset.y = parseInt(infoboxOffsetY);
				}
				if (searchUrl != null && searchUrl != '') {
					_searchUrl = searchUrl;
				}
				if (distanceUnits != null && distanceUnits != '') {
					_distanceUnits = distanceUnits;
				}
				_location = _mapOptions.center;
				_mapUserLocation = _mapOptions.center.latitude + "," + _mapOptions.center.longitude;
				_map = getMap();
				$("#entity-list-map-search").on("click", function (e) {
					e.preventDefault();
					locationSearch();
				});
				$("#entity-list-map-directions-reverse").on("click", function (e) {
					e.preventDefault();
					var fromLocation = $("#entity-list-map-directions-from").val();
					var toLocation = $("#entity-list-map-directions-to").val();
					$("#entity-list-map-directions-to").val(fromLocation);
					$("#entity-list-map-directions-from").val(toLocation);
					$("#entity-list-map-directions-latitude").val('').removeAttr('value');
					$("#entity-list-map-directions-longitude").val('').removeAttr('value');
				});
				$("#entity-list-map-directions-get").on("click", function (e) {
					e.preventDefault();
					createDirections();
				});
				resetSearchOptions();
				if (autosize) {
					$(window).resize(function () {
						resizeMap();
					});
				}
				search(_location.longitude, _location.latitude);
			}
		});
	}

	function getMap() {
		return new Microsoft.Maps.Map('#' + _mapElementId, _mapOptions);
	}
	
	function resizeMap() {
		_$mapElement.height(getHeight());
		_$mapElement.width(getWidth());
	}

	function getHeight() {
		var height = 240;
		var frameHeight = $(window).height();
		if (frameHeight > 0) {
			height = frameHeight / 2;
		}
		return height;
	}

	function getWidth() {
		var width = 320;
		var frameWidth = $("#" + _mapElementId).parent().width();
		if ((frameWidth) > 0) {
			width = frameWidth - 4;
		}
		return width;
	}

	function reset() {
		resetMapView();
		resetSearchOptions();
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
	}

	function createStatusOverlay(element, left, top) {
		$("<div id='entity-list-map-status' style='display:none; border: 1px solid #333; padding: 5px; background: #fff; position: absolute; left: " + left + "px; top: " + top + "px; z-index: 10000;'><img src='~/xrm-adx/samples/images/loader.gif' alt='Loading' />&nbsp;<span id='entity-list-map-status-text'></span></div>").appendTo(element);
	}
	
	function search(longitude, latitude) {
		$(document).ready(function () {
			var map = null;
			if (_map == null) {
				map = getMap();
			} else {
				map = _map;
			}
			$("#entity-list-map-status-text").html("Searching for Locations...");
			$("#entity-list-map-status").fadeIn();
			map.entities.clear();
			$("#entity-list-map-locations tbody").empty();
			var distance = 0;
			var units = "miles";
			if ($("#entity-list-map-distance").val()) {
				distance = parseInt($("#entity-list-map-distance").val());
			}
			var uom = _$mapElement.attr("data-distance-units");
			if (uom != null && uom != '') {
				units = uom;
			}
			var entityListId = _$mapElement.attr("data-entity-list-id");
			getMapNodes(longitude, latitude, distance, units, entityListId,
				function (data) {
					//error
					if (!data.success && data.error != null) {
						$("#entity-list-map-error").show().find('p').text(data.error);
						return;
					} else {
						$("#entity-list-map-error").hide();
					}
					//success
					var i = 0;
					var locations = [];
					if ($("#entity-list-map-location").val() != null && $("#entity-list-map-location").val() != '') {
						locations.push(_location);
						addPushpin(_map, "<span class='title'><strong>" + $("#entity-list-map-location").val() + "</strong></span>", _location.latitude, _location.longitude, true, '');
					}
					$.each(data, function (index, node) {
						i = i + 1;
						var location = new Microsoft.Maps.Location(node.Latitude, node.Longitude);
						locations.push(location);
						var pushpinContent = "<span class='title'><strong>" + node.Title + "</strong></span>"
							+ "<p>" + node.Description + "</p>";
						var locationContent = "<span class='title'><strong>" + i + ". " + node.Title + "</strong> <span class='uom'>" + node.Distance + " " + units + "</span></span>"
							+ "<p>" + node.Description + "</p>";
						addPushpin(map, pushpinContent, node.Latitude, node.Longitude, true, node.PushpinImageUrl, node.PushpinImageWidth, node.PushpinImageHeight, i);
						addLocationInfo(locationContent, node.Longitude, node.Latitude, node.Location);
						var viewBoundaries = Microsoft.Maps.LocationRect.fromLocations(locations);
						map.setView({ bounds: viewBoundaries });
					});
					$("#entity-list-map-locations").height(_$mapElement.height());
				},
				function () {
					//error
				},
				function () {
					//completed
					$("#entity-list-map-status").fadeOut();
				}
			);
		});
	}

	function locationSearch() {
		if ($("#entity-list-map-location").val().length > 0) {
			//send location query to bing maps REST api
			$.getJSON(_mapLocationQueryRestUrl + "?query=" + $("#entity-list-map-location").val() + "&key=" + _mapOptions.credentials + "&userLocation=" + _mapUserLocation + "&jsonp=?", function (result) {
				if (result.resourceSets[0].estimatedTotal > 0) {
					var loc = result.resourceSets[0].resources[0].point.coordinates;
					var latitude = loc[0];
					var longitude = loc[1];
					_location = new Microsoft.Maps.Location(latitude, longitude);
					var viewBoundaries = Microsoft.Maps.LocationRect.fromLocations(_location);
					_map.setView({ bounds: viewBoundaries });
					search(longitude, latitude);
				}
				else {
					alert(window.ResourceManager['Address_Cannot_Found']);
				}
			});
		}
		else {
			alert(window.ResourceManager['Enter_Address']);
		}
	}

	function getMapNodes(longitude, latitude, distance, units, id, success, error, complete) {
		success = $.isFunction(success) ? success : function () { };
		error = $.isFunction(error) ? error : function () { };
		complete = $.isFunction(complete) ? complete : function () { };
		var url = _searchUrl;
		var data = {};
		data.longitude = longitude;
		data.latitude = latitude;
		data.distance = distance;
		data.units = units;
		data.id = id;
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

	function addPushpin(map, content, latitude, longitude, tether, pushpinImageUrl, pushpinWidth, pushpinHeight, pushpinIndex) {
		var location = new Microsoft.Maps.Location(latitude, longitude);
		var pushpinOptions = {};
		if (pushpinImageUrl != null && pushpinImageUrl != '') {
			pushpinOptions = { icon: pushpinImageUrl, width: pushpinWidth, height: pushpinHeight, text: pushpinIndex.toString(), textOffset: new Microsoft.Maps.Point(-3, 4) };
		} else if (pushpinIndex) {
			pushpinOptions = { text: pushpinIndex.toString(), textOffset: new Microsoft.Maps.Point(0, 4) };
		}
		var pushpin = new Microsoft.Maps.Pushpin(location, pushpinOptions);
		var infoboxContent = "<div class='infobox right'>"
								+ "<div class='infobox-arrow'>"
									+ "<div class='infobox-arrow-inner'></div>"
								+ "</div>"
								+ "<div class='infobox-content'>"
									+ content
								+ "</div>"
							+ "</div>";
		var pushpinInfobox = new Microsoft.Maps.Infobox(pushpin.getLocation(),
		{
			htmlContent: infoboxContent,
			visible: false,
			offset: new Microsoft.Maps.Point(_infoboxSettings.offset.x, _infoboxSettings.offset.y),
			showCloseButton: true
		});
		if (!tether) {
			Microsoft.Maps.Events.addHandler(map, 'viewchange', function (e) { pushpinInfobox.setOptions({ visible: false }); });
		}
		Microsoft.Maps.Events.addHandler(pushpin, 'mouseover', function (e) { pushpinInfobox.setOptions({ visible: true }); });
		Microsoft.Maps.Events.addHandler(pushpin, 'mouseout', function (e) { pushpinInfobox.setOptions({ visible: false }); });
		map.entities.push(pushpin);
		pushpinInfobox.setMap(map);
	}

	function addLocationInfo(content, longitude, latitude, location) {
		var directionsDiv = $("<div></div>");
		var directionsLink = $("<a href='#entity-list-map-directions-dialog' role='modal' data-toggle='modal' class='btn btn-info'> Get Directions </a>");
		directionsDiv.append(directionsLink);
		var newDirectionsDiv = $("<div></div>");
		directionsDiv.append(newDirectionsDiv);
		directionsLink.on("click", function (e) {
			e.preventDefault();
			if (location) {
				$("#entity-list-map-directions-to").val(location);
			}
			if (latitude) {
				$("#entity-list-map-directions-latitude").val(latitude);
			}
			if (longitude) {
				$("#entity-list-map-directions-longitude").val(longitude);
			}
			$("#entity-list-map-directions").appendTo(newDirectionsDiv);
			if ($("#entity-list-map-location").val() != null && $("#entity-list-map-location").val() != '') {
				$("#entity-list-map-directions-from").val($("#entity-list-map-location").val());
			}
			$("#entity-list-map-directions-dialog").modal('show');
			return false;
		});
		var locationContent = $("<tr></tr>");
		var td = $("<td></td>");
		locationContent.append(td);
		td.append(content);
		td.append(directionsDiv);
		$("#entity-list-map-locations tbody").append(locationContent);
	}

	function createDirectionsManager() {
		if (!_mapDirectionsManager) {
			_mapDirectionsManager = new Microsoft.Maps.Directions.DirectionsManager(_map);
		}
		_mapDirectionsManager.clearAll();
		//_directionsErrorEventObj = Microsoft.Maps.Events.addHandler(_mapDirectionsManager, 'directionsError', function (arg) { });
		//_directionsUpdatedEventObj = Microsoft.Maps.Events.addHandler(_mapDirectionsManager, 'directionsUpdated', function () { });
	}

	function createDrivingRoute() {

		if (!_mapDirectionsManager) {
			createDirectionsManager();
		}

		_mapDirectionsManager.clearAll();

		_mapDirectionsManager.setRequestOptions({ routeMode: Microsoft.Maps.Directions.RouteMode.driving });

		var firstWaypoint = new Microsoft.Maps.Directions.Waypoint({ address: $("#entity-list-map-directions-from").val() });
		
		_mapDirectionsManager.addWaypoint(firstWaypoint);
		
		var secondWaypoint = {};
		
		if ($("#entity-list-map-directions-latitude").val()) {
			secondWaypoint = new Microsoft.Maps.Directions.Waypoint({ address: $("#entity-list-map-directions-to").val(), location: new Microsoft.Maps.Location($("#entity-list-map-directions-latitude").val(), $("#entity-list-map-directions-longitude").val()) });
		}
		else {
			secondWaypoint = new Microsoft.Maps.Directions.Waypoint({ address: $("#entity-list-map-directions-to").val() });
		}

		_mapDirectionsManager.addWaypoint(secondWaypoint);
		
		_mapDirectionsManager.setRenderOptions({ itineraryContainer: document.getElementById('entity-list-map-directions') });
		
		_mapDirectionsManager.calculateDirections();
	}

	function createDirections() {
		$('#entity-list-map-directions-dialog').modal('hide');

		if (!_mapDirectionsManager) {
			Microsoft.Maps.loadModule('Microsoft.Maps.Directions', { callback: ADX.entityListMap.createDrivingRoute });
		}
		else {
			createDrivingRoute();
		}
	}

	_export.createDirections = createDirections;
	_export.createDrivingRoute = createDrivingRoute;
	_export.initialize = initialize;
	_export.locationSearch = locationSearch;
	_export.reset = reset;
	_export.search = search;
	
	return _export;

})(jQuery);

ADX.entityListMap.initialize('entity-list-map', null, true);
