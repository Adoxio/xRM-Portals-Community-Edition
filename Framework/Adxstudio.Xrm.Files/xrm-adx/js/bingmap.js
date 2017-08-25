/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

var _map, loc, _pin;

function createMap() {
	var bingMapsCredentials = $("#bingmapscredentials").val();
	_map = new Microsoft.Maps.Map("#adx-bing-map", { credentials: bingMapsCredentials });
}

function onGeolocationPositionReady(position) {
	var location = new Microsoft.Maps.Location(position.coords.latitude, position.coords.longitude);
	_map.setView({ zoom: 18, center: location });

	var latitudeFieldName = $("#geolocation_latitudefieldname").val();
	var longitudeFieldName = $("#geolocation_longitudefieldname").val();

	var latitudeValue = modifyCoordinate(position.coords.latitude);
	var longitudeValue = modifyCoordinate(position.coords.longitude);

	$("#" + latitudeFieldName).val(latitudeValue);
	$("#" + longitudeFieldName).val(longitudeValue);

	_pin.setLocation(location);

	initPin(latitudeFieldName, longitudeFieldName);

	findLocationByPoint();
}

function initPin(latitudeFieldName, longitudeFieldName) {
	var addressLineFieldName = $("#geolocation_addresslinefieldname").val();

	var dragstartDetails = function (e) {
	};

	var dragendDetails = function (e) {
		var latitudeValue = modifyCoordinate(e.location.latitude);
		var longitudeValue = modifyCoordinate(e.location.longitude);

		$("#" + latitudeFieldName).val(latitudeValue);
		$("#" + longitudeFieldName).val(longitudeValue);
		findLocationByPoint();
	};

	var readonly = $("#" + addressLineFieldName).is('[readonly]') || $("#" + addressLineFieldName).is('[disabled]');

	if (!readonly) { // don't allow pin to be moved if the address is readonly.
		_pin.setOptions({ draggable: true });
		Microsoft.Maps.Events.addHandler(_pin, 'dragstart', dragstartDetails);
		Microsoft.Maps.Events.addHandler(_pin, 'dragend', dragendDetails);
	} else {
		_pin.setOptions({ draggable: false });
	}

	_map.entities.push(_pin);
}

function onGeolocationPositionError(err) {
	switch (err.code) {
		case 0:
			$("#adx-bing-map-info").append("<div class='alert alert-block alert-warning'>Failed to determine geolocation. An unknown error occurred.</div>");
			break;
		case 1:
			$("#adx-bing-map-info").append("<div class='alert alert-block alert-warning'>Geolocation access denied by user.</div>");
			break;
		case 2:
			$("#adx-bing-map-info").append("<div class='alert alert-block alert-warning'>Failed to determine geolocation. Location data unavailable.</div>");
			break;
		case 3:
			$("#adx-bing-map-info").append("<div class='alert alert-block alert-warning'>Failed to determine geolocation. Location request timed out.</div>");
			break;
	}
}

function findLocationByQuery() {

	var addressLineFieldName = $("#geolocation_addresslinefieldname").val();
	var cityFieldName = $("#geolocation_cityfieldname").val();
	var countyFieldName = $("#geolocation_countyfieldname").val();
	var stateFieldName = $("#geolocation_statefieldname").val();
	var countryFieldName = $("#geolocation_countryfieldname").val();
	var postalCodeFieldName = $("#geolocation_portalcodefieldname").val();

	var query = '';
	var addressline = $("#" + addressLineFieldName).val();
	var city = $("#" + cityFieldName).val();
	var county = $("#" + countyFieldName).val();
	var state = $("#" + stateFieldName).val();
	var country = $("#" + countryFieldName).val();
	var postalcode = $("#" + postalCodeFieldName).val();

	if (city == '' && state == '') {
		return;
	}

	if (addressline && addressline != '') {
		query = addressline;
	}
	if (city && city != '') {
		if (query.length > 0) {
			query = query + ',';
		}
		query = query + city;
	}
	if (state && state != '') {
		if (query.length > 0) {
			query = query + ',';
		}
		query = query + state;
	}
	if (county && county != '') {
		if (query.length > 0) {
			query = query + ',';
		}
		query = query + county;
	}
	if (country && country != '') {
		if (query.length > 0) {
			query = query + ',';
		}
		query = query + country;
	}
	if (postalcode && postalcode != '') {
		if (query.length > 0) {
			query = query + ',';
		}
		query = query + postalcode;
	}
	if (query.length == 0) {
		return;
	}
	query = encodeURIComponent(query);
	getLocationByQuery(query);
}

function findLocationByPoint() {
	var point = '';
	var pushpin = _map.entities.get(0);
	if (pushpin instanceof Microsoft.Maps.Pushpin) {
		var location = pushpin.getLocation();
		point = location.latitude + ',' + location.longitude;
	}
	if (point.length == 0) {
		return;
	}

	getLocationByPoint(point);
}

function getCurrentCultureQuery() {
	var culture = $("html").data("lang");
	return culture
		? "&culture=" + culture
		: "";
}

function getLocationByQuery(query) {
	var bingMapsCredentials = $("#bingmapscredentials").val();
	var bingmapsRestUrl = $("#bingmapsresturl").val();
	var cultureQueryAddition = getCurrentCultureQuery();

	var searchRequest = bingmapsRestUrl + '?q=' + query + '&output=json&jsonp=findLocationByQueryCallback&key=' + bingMapsCredentials + cultureQueryAddition;

	var mapscript = document.createElement('script');
	mapscript.type = 'text/javascript';
	mapscript.src = searchRequest;
	document.getElementById('adx-bing-map').appendChild(mapscript);
}

function getLocationByPoint(point) {
	var bingMapsCredentials = $("#bingmapscredentials").val();
	var bingmapsRestUrl = $("#bingmapsresturl").val();
	var cultureQueryAddition = getCurrentCultureQuery();

	var searchRequest = bingmapsRestUrl + point + '?includeNeighborhood=1&output=json&jsonp=findLocationByPointCallback&key=' + bingMapsCredentials + cultureQueryAddition;

	var mapscript = document.createElement('script');
	mapscript.type = 'text/javascript';
	mapscript.src = searchRequest;
	document.getElementById('adx-bing-map').appendChild(mapscript);
}

function findLocationByQueryCallback(result) {
	var addressLineFieldName = $("#geolocation_addresslinefieldname").val();
	var cityFieldName = $("#geolocation_cityfieldname").val();
	var countyFieldName = $("#geolocation_countyfieldname").val();
	var stateFieldName = $("#geolocation_statefieldname").val();
	var countryFieldName = $("#geolocation_countryfieldname").val();
	var postalCodeFieldName = $("#geolocation_portalcodefieldname").val();
	var neighbourhoodFieldName = $("#geolocation_neighbourhoodfieldname").val();

	var formattedLocationFieldName = $("#geolocation_formattedlocationfieldname").val();

	var latitudeFieldName = $("#geolocation_latitudefieldname").val();
	var longitudeFieldName = $("#geolocation_longitudefieldname").val();

	var alert = $("#adx-bing-map-info").children(".alert").hide();
	if (!alert.length) {
		alert = $("<div class='alert alert-block'></div>").appendTo("#adx-bing-map-info").hide();
	}

	var icon = alert.children(".fa").remove();
	if (!icon.length) {
		icon = $("<span class='fa'></span>");
	}

	if (result && result.resourceSets && result.resourceSets.length > 0 && result.resourceSets[0].resources && result.resourceSets[0].resources.length > 0) {

		var bbox = result.resourceSets[0].resources[0].bbox;
		var viewBoundaries = Microsoft.Maps.LocationRect.fromLocations(new Microsoft.Maps.Location(bbox[0], bbox[1]), new Microsoft.Maps.Location(bbox[2], bbox[3]));
		_map.setView({ bounds: viewBoundaries });
		var location = new Microsoft.Maps.Location(result.resourceSets[0].resources[0].point.coordinates[0], result.resourceSets[0].resources[0].point.coordinates[1]);

		if (_map.entities.indexOf(_pin) === -1) {
			initPin(latitudeFieldName, longitudeFieldName);
		}

		_pin.setLocation(location);

		$("#" + addressLineFieldName).val(result.resourceSets[0].resources[0].address.addressLine);
		$("#" + cityFieldName).val(result.resourceSets[0].resources[0].address.locality);
		$("#" + stateFieldName).val(result.resourceSets[0].resources[0].address.adminDistrict);
		$("#" + countyFieldName).val(result.resourceSets[0].resources[0].address.adminDistrict2);
		$("#" + countryFieldName).val(result.resourceSets[0].resources[0].address.countryRegion);
		$("#" + postalCodeFieldName).val(result.resourceSets[0].resources[0].address.postalCode);
		$("#" + neighbourhoodFieldName).val(result.resourceSets[0].resources[0].address.neighborhood);

		var latitudeValue = modifyCoordinate(result.resourceSets[0].resources[0].point.coordinates[0]);
		var longitudeValue = modifyCoordinate(result.resourceSets[0].resources[0].point.coordinates[1]);

		$("#" + latitudeFieldName).val(latitudeValue);
		$("#" + longitudeFieldName).val(longitudeValue);

		$("#" + formattedLocationFieldName).val(result.resourceSets[0].resources[0].address.formattedAddress);

		alert.hide();
	} else {
		$("#" + latitudeFieldName).val("");
		$("#" + longitudeFieldName).val("");

		$("#" + formattedLocationFieldName).val("");

		alert.html("").append(icon).append(" ");
		icon.removeClass("fa-exclamation-triangle").addClass("fa-exclamation-circle");
		if (typeof (response) == 'undefined' || response == null) {
			icon.addClass("fa-exclamation-triangle").removeClass("fa-exclamation-circle");
			alert.append("This address could not be found.").addClass("alert-warning").removeClass("alert-danger").show();
		} else if (typeof (response) != 'undefined' && response && result && result.errorDetails) {
			alert.append(response.errorDetails[0]).addClass("alert-danger").removeClass("alert-warning").show();
		} else {
			alert.append("An unknown error has occurred.").addClass("alert-danger").removeClass("alert-warning").show();
		}
	}
}

function findLocationByPointCallback(result) {
	
	var addressLineFieldName = $("#geolocation_addresslinefieldname").val();
	var cityFieldName = $("#geolocation_cityfieldname").val();
	var countyFieldName = $("#geolocation_countyfieldname").val();
	var stateFieldName = $("#geolocation_statefieldname").val();
	var countryFieldName = $("#geolocation_countryfieldname").val();
	var postalCodeFieldName = $("#geolocation_portalcodefieldname").val();
	var neighbourhoodFieldName = $("#geolocation_neighbourhoodfieldname").val();

	var formattedLocationFieldName = $("#geolocation_formattedlocationfieldname").val();

	var alert = $("#adx-bing-map-info").children(".alert").hide();
	if (!alert.length) {
		alert = $("<div class='alert alert-block'></div>").appendTo("#adx-bing-map-info").hide();
	}

	var icon = alert.children(".fa").remove();
	if (!icon.length) {
		icon = $("<span class='fa'></span>");
	}

	if (result && result.resourceSets && result.resourceSets.length > 0 && result.resourceSets[0].resources && result.resourceSets[0].resources.length > 0) {
		var bbox = result.resourceSets[0].resources[0].bbox;
		var viewBoundaries = Microsoft.Maps.LocationRect.fromLocations(new Microsoft.Maps.Location(bbox[0], bbox[1]), new Microsoft.Maps.Location(bbox[2], bbox[3]));
		_map.setView({ bounds: viewBoundaries });
		
		$("#" + addressLineFieldName).val(result.resourceSets[0].resources[0].address.addressLine);
		$("#" + cityFieldName).val(result.resourceSets[0].resources[0].address.locality);
		$("#" + stateFieldName).val(result.resourceSets[0].resources[0].address.adminDistrict);
		$("#" + countyFieldName).val(result.resourceSets[0].resources[0].address.adminDistrict2);
		$("#" + countryFieldName).val(result.resourceSets[0].resources[0].address.countryRegion);
		$("#" + postalCodeFieldName).val(result.resourceSets[0].resources[0].address.postalCode);
		$("#" + neighbourhoodFieldName).val(result.resourceSets[0].resources[0].address.neighborhood);
		$("#" + formattedLocationFieldName).val(result.resourceSets[0].resources[0].address.formattedAddress);
	} else {
		$("#" + addressLineFieldName).val('');
		$("#" + cityFieldName).val('');
		$("#" + stateFieldName).val('');
		$("#" + countyFieldName).val('');
		$("#" + countryFieldName).val('');
		$("#" + postalCodeFieldName).val('');
		$("#" + neighbourhoodFieldName).val('');

		$("#" + formattedLocationFieldName).val("");

		alert.html("").append(icon).append(" ");
		icon.removeClass("fa-exclamation-triangle").addClass("fa-exclamation-circle");
		if (typeof (response) == 'undefined' || response == null) {
			icon.addClass("fa-exclamation-triangle").removeClass("fa-exclamation-circle");
			alert.append("No valid location could be found at these coordinates.").addClass("alert-warning").removeClass("alert-danger").show();
		} else if (typeof (response) != 'undefined' && response && result && result.errorDetails) {
			alert.append(response.errorDetails[0]).addClass("alert-danger").removeClass("alert-warning").show();
		} else {
			alert.append("An unknown error has occurred.").addClass("alert-danger").removeClass("alert-warning").show();
		}
	}
}

function modifyCoordinate(value) {
	var latitudeFieldName = $("#geolocation_latitudefieldname").val();
	var decimalSeparator = $("#" + latitudeFieldName).data("numberdecimalseparator");
	var val = value.toString();
	
	if (val === "") {
		return "";
	}

	if (decimalSeparator) {
		return val.replace(".", decimalSeparator);
	}

	return val;
}

$(document).ready(function() {
	var $table = $("table[data-name='section_map']");

	if ($table.length == 0) {
		return;
	}
	
	var formattedLocationFieldName = $("#geolocation_formattedlocationfieldname").val();
	
	var latitudeFieldName = $("#geolocation_latitudefieldname").val();
	var longitudeFieldName = $("#geolocation_longitudefieldname").val();

	var addressLineFieldName = $("#geolocation_addresslinefieldname").val();
	var cityFieldName = $("#geolocation_cityfieldname").val();
	var countyFieldName = $("#geolocation_countyfieldname").val();
	var stateFieldName = $("#geolocation_statefieldname").val();
	var countryFieldName = $("#geolocation_countryfieldname").val();
	var postalCodeFieldName = $("#geolocation_portalcodefieldname").val();


	$("#" + formattedLocationFieldName).parent().prepend("<div id='adx-bing-map' style='position: relative; width: 100%;  height: 400px; border: 0;'></div><div id='adx-bing-map-info'></div>");

	$("#" + formattedLocationFieldName).hide();

	$("#" + latitudeFieldName).prop("readonly", true);

	$("#" + longitudeFieldName).prop("readonly", true);

	window.onload = function () {
		loc = new Microsoft.Maps.Location(0, 0);
		_pin = new Microsoft.Maps.Pushpin(loc, { draggable: true });

		createMap();

		var latitude = modifyCoordinate($("#" + latitudeFieldName).val());
		var longitude = modifyCoordinate($("#" + longitudeFieldName).val());

		if (latitude == null || latitude.length == 0 || longitude == null || longitude.length == 0) {

			var readonly = $("#" + addressLineFieldName).is('[readonly]') || $("#" + addressLineFieldName).is('[disabled]');

			if (!readonly) {
				if (!navigator.geolocation) {
					$("#adx-bing-map-info").append("<div class='alert alert-block alert-warning'>Your browser does not support geolocation.</div>");
				} else {
					navigator.geolocation.getCurrentPosition(onGeolocationPositionReady, onGeolocationPositionError);
				}
			}
		} else {
			var position = {
				coords: {
					latitude: latitude,
					longitude: longitude,
				}
			};
			onGeolocationPositionReady(position);
		}

		$(document).on("change", "#" + [addressLineFieldName, cityFieldName, countyFieldName, stateFieldName, countryFieldName, postalCodeFieldName].join(",#"), function () {
			findLocationByQuery();
		});
	}
});
