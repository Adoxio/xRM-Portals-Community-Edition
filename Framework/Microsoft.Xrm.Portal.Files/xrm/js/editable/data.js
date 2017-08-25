/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {
	
	var ns = XRM.namespace('data');
	var $ = XRM.jQuery;
	var dataServicesNS = XRM.namespace('data.services');
	var JSON = YAHOO.lang.JSON;
	
	var acceptHeader = 'application/json, text/javascript';
	
	function createJSONResponseHandler(handler) {
		return function(data, textStatus, xhr) {
			if (!data) {
				handler(null, textStatus, xhr);
				return;
			}
			
			// Get the data as raw text to bypass jQuery 1.4 stricter JSON parsing. WCF Data Services
			// returns invalid JSON pretty regularly. This eval technique is not ideal, obviously, but
			// it's analogous to what jQuery 1.3 did.
			try {
				var evaled = eval("(" + data + ")");
				handler(JSON.parse(JSON.stringify(evaled)), textStatus, xhr);
			}
			catch (e) {
				XRM.log('Error parsing response JSON: ' + e, 'error');
				handler(null, textStatus, xhr);						
			}
		}
	}
	
	ns.getReponseHandler = function(handler) {
		return {
			dataType: 'text',
			handler: $.isFunction(handler) ? createJSONResponseHandler(handler) : null
		}
	}
	
	ns.getJSON = function(uri, options) {
		options = options || {};
		var responseHandler = ns.getReponseHandler(options.success);
		$.ajax({
			beforeSend: function(xhr) {
				xhr.setRequestHeader('Accept', acceptHeader);
			},
			url: uri,
			type: 'GET',
			processData: true,
			dataType: responseHandler.dataType,
			contentType: 'application/json',
			success: responseHandler.handler,
			error: options.error
		});
	}

	ns.postJSON = function(uri, data, options) {
		options = options || {};
		var responseHandler = ns.getReponseHandler(options.success);
		$.ajax({
			beforeSend: function(xhr) {
				xhr.setRequestHeader('Accept', acceptHeader);

				if (options.httpMethodOverride) {
					xhr.setRequestHeader('X-HTTP-Method', options.httpMethodOverride);
				}
			},
			url: uri,
			type: 'POST',
			processData: options.processData || false,
			dataType: responseHandler.dataType,
			contentType: 'application/json',
			data: data ? JSON.stringify(data) : null,
			success: responseHandler.handler,
			error: options.error
		});
	}

	// Given an ADO.NET Data Service entity URI, an attribute name, and an attribute value,
	// update that attribute on the entity.
	dataServicesNS.putAttribute = function(uri, name, value, options) {
		options = options || {};
		options.httpMethodOverride = 'MERGE';
		var data = {};
		data[name] = value;
		ns.postJSON(uri, data, options);
	}
	
});
