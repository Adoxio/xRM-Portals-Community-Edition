/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function ($) {
	$(document).ready(function () {
		var customerField = $("#customerid");
		if (customerField.length > 0 && $("#entitlementid").length > 0 && $("#customerid_name").attr("disabled") != "disabled" && $("#entitlementid").closest("td").is(':visible')) {
			customerField.change(getDefaultEntitlement);
			customerField.change();
		}
	});

	function getDefaultEntitlement() {
		var parameters = [];
		var gridData = $("#entitlementid_lookupmodal").find('.entity-grid');
		var customerid = $("#customerid").val();
		var customertype = $("#customerid_entityname").val();
		var layout = gridData ? gridData.data("view-layouts")[0] : null;
		var url = $("#entitlementid_lookupmodal").find('.entity-lookup').attr('default-entitlements-url');

		if (!layout || !customerid || !customertype) return;

		parameters = [
			{
				Key: "customerid",
				Value: customerid
			},
			{
				Key: "customertype",
				Value: customertype
			}
		];

		var data = {};
		data.layout = layout.Base64SecureConfiguration;
		data.sortExpression = layout.SortExpression;
		data.customParameters = parameters;
		var jsonData = JSON.stringify(data);
		shell.ajaxSafePost({
			type: 'POST',
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			url: url,
			data: jsonData,
			global: false,
			success: function(result) {
				if (result) {
					$("#entitlementid").val(result.id);
					$("#entitlementid_entityname").val(result.entityname);
					$("#entitlementid_name").val(result.name);
				}
			}
		});
	}
}(jQuery));
