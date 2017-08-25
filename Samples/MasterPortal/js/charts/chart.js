/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function ($) {

	function crmChart(element) {
		this._element = $(element);
		this._serviceUrl = this._element.attr("data-serviceurl");
		this._chartId = this._element.attr("data-chartid");
		this._viewId = this._element.attr("data-viewid");
		this._messageAccessDenied = this._element.attr("data-accessdeniedmessage") || window.ResourceManager["Access_Denied_Error"];
		this._messageUnknownError = window.ResourceManager["UnKnown_Error_Occurred"];
		this._messageLoading = this._element.attr("data-loadingmessage") || window.ResourceManager["Visualization_Chart_Loading_Message"];
		this._messageNoData = this._element.attr("data-nodatamessage") || window.ResourceManager["Visualization_Chart_NoData_Message"];
	}

	$(document).ready(function () {
		$(".crm-chart").each(function () {
			new crmChart($(this)).render();
		});
	});

	function displayNoDataMessage($this) {
		var $element = $this._element;
		var message = $this._messageNoData;
		$element.find(".nodata").remove();
		$("<div class='nodata' role='alert'>" + message + "</div>").appendTo($element);
	}

	function displayErrorAlert(error, $element) {
		if (typeof error !== typeof undefined && error !== false && error != null) {
			if (typeof console !== typeof undefined && typeof console.log !== typeof undefined) {
				console.log(error);
			}
			var message;
			if (typeof error.InnerError !== typeof undefined && error.InnerError !== false && error.InnerError != null) {
				message = error.InnerError.Message;
			} else {
				message = error.Message;
			}
			if (message === null || message === "" || message === "error") {
				message = window.ResourceManager["UnKnown_Error_Occurred"];
			}
			$element.find(".notification").slideUp().remove();
			$("<div class='notification alert alert-danger error role='alert'><span class='glyphicon glyphicon-warning-sign' aria-hidden='true'></span> " + message + "</div>").prependTo($element);
		}
	}

	crmChart.prototype.render = function () {
		var $this = this;
		var $element = $this._element;
		var url = $this._serviceUrl;
		var chartId = $this._chartId;
		var viewId = $this._viewId;
		var messageAccessDenied = $this._messageAccessDenied;
		var messageUnknownError = $this._messageUnknownError;
		var messageLoading = $this._messageLoading;

		$element.find(".chart").fadeOut().remove();
		var $loading = $("<div class='chart-loading text-center'><span class='fa fa-spinner fa-spin' aria-hidden='true'></span></div>").append($("<span></span>").text(messageLoading));
		$element.append($loading);

		if (typeof chartId === typeof undefined || chartId === null || chartId === "") {
			var error = { Message: messageUnknownError };
			$loading.hide();
			displayErrorAlert(error, $element);
			return;
		}
		
		var data = {};
		data.chartId = chartId;
		if (typeof viewId !== typeof undefined && viewId !== null && viewId !== "") {
			data.viewId = viewId;
		}
		var builder = undefined;
		$.ajax({
			type: "GET",
			dataType: "json",
			url: url,
			data: data,
			global: false
		}).done(function (chartBuilder) {
			builder = chartBuilder;
			if (chartBuilder == null || chartBuilder.ChartDefinition == null) {
				displayErrorAlert({ Message: messageUnknownError }, $element);
				return;
			}
			if (chartBuilder.EntityPermissionDenied) {
				var error = { Message: messageAccessDenied };
				displayErrorAlert(error, $element);
				return;
			}
			$P_CRM = $;
			if ($element.attr("id") == null) {
				$element.attr("id", chartBuilder.Id);
			}
			Portal.Charting.PortalChartOrchestrator.createChart(
				chartBuilder
			).then(function (chartConfig) {
			  chartConfig.title.text = builder.ChartDefinition.Name;

				$.extend(true, chartConfig, {
					legend: {
						itemStyle: {
							width: '100px',
							textOverflow: 'ellipsis',
							overflow: 'hidden'
						}
					}
				});

				$('#' + $element.attr("id")).highcharts(chartConfig);
				if (builder.Data == null || builder.Data.length === 0) {
					displayNoDataMessage($this);
				}
			}).fail(function (error) {
				displayErrorAlert(error, $element);
			});
		}).fail(function (jqXhr) {
			var contentType = jqXhr.getResponseHeader("content-type") || "";
			var error = contentType.indexOf("json") > -1 ? $.parseJSON(jqXhr.responseText) : { Message: jqXhr.status, InnerError: { Message: jqXhr.statusText } };
			displayErrorAlert(error, $element);
		}).always(function () {
			$loading.hide();
		});
	}

}(jQuery));
