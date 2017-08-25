/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function ($) {

	function entityRating(element) {
		this._element = $(element);
		this._urlSave = this._element.attr("data-url-save");
		this._logicalName = this._element.attr("data-logicalname");
		this._id = this._element.attr("data-id");
		this._min = this._element.attr("data-rateit-min");
		this._max = this._element.attr("data-rateit-max");
	}

	$(document).ready(function () {
		$(".rateit").each(function () {
			new entityRating($(this)).initialize();
		});
	});

	entityRating.prototype.initialize = function () {
		var $this = this;
		var $element = $this._element;
		var url = $this._urlSave;
		var logicalName = $this._logicalName;
		var id = $this._id;

		if (typeof url === typeof undefined || url === null || url === "") {
			return;
		}

		if (typeof logicalName === typeof undefined || logicalName === null || logicalName === "") {
			return;
		}

		if (typeof id === typeof undefined || id === null || id === "") {
			return;
		}

		$element.rateit("readonly", false);

		$element.on("rated", function() {
			var rating = $(this).rateit("value");
			$this.saveRating(rating);
		});
	}

	entityRating.prototype.saveRating = function (rating) {
		var $this = this;
		var $element = $this._element;
		var url = $this._urlSave;
		var logicalName = $this._logicalName;
		var id = $this._id;
		var min = $this._min;
		var max = $this._max;

		if (typeof rating === typeof undefined || rating === null || rating === "") {
			$element.rateit("reset");
			return;
		}

		if (typeof url === typeof undefined || url === null || url === "") {
			$element.rateit("reset");
			return;
		}

		if (typeof logicalName === typeof undefined || logicalName === null || logicalName === "") {
			$element.rateit("reset");
			return;
		}

		if (typeof id === typeof undefined || id === null || id === "") {
			$element.rateit("reset");
			return;
		}

		if (typeof min === typeof undefined || min === null || min === "") {
			min = 0;
		} else {
			min = parseInt(min);
			if (isNaN(min)) {
				min = 0;
			}
		}

		if (typeof max === typeof undefined || max === null || max === "") {
			max = 5;
		} else {
			max = parseInt(max);
			if (isNaN(max)) {
				max = 5;
			}
		}

		$element.rateit("readonly", true);

		var data = {};
		var entityReference = {};
		entityReference.LogicalName = logicalName;
		entityReference.Id = id;
		data.entityReference = entityReference;
		data.rating = rating;
		data.min = min;
		data.max = max;
		var jsonData = JSON.stringify(data);

		shell.ajaxSafePost({
			type: "POST",
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			url: url,
			data: jsonData,
			processData: false,
			global: false
		}).done(function (ratingInfo) {
			if (typeof ratingInfo !== typeof undefined && ratingInfo != null && ratingInfo !== "") {
				$element.rateit("value", ratingInfo.AverageRatingRounded);
				var $ratingCount = $element.closest(".rating").find(".rating-count");
				$ratingCount.text(ratingInfo.RatingCount);
			} else {
				$element.rateit("reset");
				return;
			}
		}).fail(function(jqXhr) {
			var contentType = jqXhr.getResponseHeader("content-type") || "";
			var error = contentType.indexOf("json") > -1 ? $.parseJSON(jqXhr.responseText) : { Message: jqXhr.status, InnerError: { Message: jqXhr.statusText } };
			if (typeof console !== typeof undefined && typeof console.log !== typeof undefined) {
				console.log(error);
			}
			$element.rateit("reset");
		}).always(function() {
			$element.rateit("readonly", false);
		});
	}
}(jQuery));
