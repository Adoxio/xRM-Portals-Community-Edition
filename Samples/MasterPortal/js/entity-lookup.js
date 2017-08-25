/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function ($) {

	function entityLookup(element) {
		this._element = $(element);
		this._target = this._element.data("target") || {};
		this._url = this._element.attr("data-url");
		this._lookupDataFieldName = this._element.attr("data-lookup-datafieldname");
		this._lookupReferenceEntityFormId = this._element.attr("data-lookup-reference_entityformid");
		this._languageCode = this._element.attr("data-languageCode");
	}

	$(document).ready(function () {
		$(".entity-lookup").each(function () {
			new entityLookup($(this)).render();
		});
	});

	entityLookup.prototype.render = function () {
		var $this = this;
		var $element = $this._element;
		var $grid = $element.find(".entity-grid");
		var $button = $element.find(".modal-footer button.primary");
		var $modal = $element.find(".modal-lookup");
		var $lookupnewModal = $element.find(".modal-lookup-create-form");
		var lookupReferenceEntityFormId = this._lookupReferenceEntityFormId;
		var languageCode = this._languageCode;

		$element.find(".modal-footer .remove-value")
			.on("click", function (e) {
				e.preventDefault();
				$this.clearFieldValue();
			});
		
		$button.on("click", function () {
			$modal.find(".alert-danger.error").remove();
			$this.setFieldValue();
		});

		$("#" + $this._lookupDataFieldName + "_name").on("keydown", function (event) {
			if (event.keyCode === 8 || event.keyCode === 46) { // Backspace or Delete
				$this.clearFieldValue();
				event.stopPropagation();
				event.preventDefault();
			}
		});

		$element.closest("td.lookup.form-control-cell").find(".clearlookupfield").on("click", function (e) {
			e.preventDefault();
			$this.clearFieldValue();
		}).toggle(!!$("#" + $this._lookupDataFieldName).val());

		$element.closest("td.lookup.form-control-cell").find(".launchentitylookup").on("click", function (e) {
			e.preventDefault();
			$element.find(".view-pagination").data("pages", 1).data("current-page", 1);
			$modal.modal();
		});

		$element.closest("td.lookup.form-control-cell").find(".new-value").on("click", function (e) {
			e.preventDefault();
			var $iframe = $lookupnewModal.find("iframe");
			var page = $iframe.data("page");
			if (!lookupReferenceEntityFormId) {
				return;
			}
			var src = page + "?lookup=true&entityformid=" + lookupReferenceEntityFormId + "&languagecode=" + languageCode;

			$iframe.attr("src", src);
			$iframe.on("load", function () {
				$lookupnewModal.find(".form-loading").hide();
				$lookupnewModal.find("iframe").contents().find("#EntityFormControl").show();
			});
			$lookupnewModal.find(".form-loading").show();
			$lookupnewModal.find("iframe").contents().find("#EntityFormControl").hide();
			$lookupnewModal.modal();

			window.addEventListener('message', function (e) {
				var origin, message;

				if (!window.location.origin) {
					origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : ''); // IE does not set origin
				} else {
					origin = window.location.origin;
				}

				if (e.origin != origin) {
					return;
				}

				try {
					message = $.parseJSON(e.data);
				} catch (jsonParseError) {
					return;
				}

				if (message.type === 'Success') {
					e.preventDefault();
					$element.children(".modal-form").modal('hide');
					$grid.data('selected', message.id);
					$grid.trigger("refresh");
				}

				e.target.removeEventListener(e.type, arguments.callee);
			});
		});

		$lookupnewModal.on('show.bs.modal', function () {
			$lookupnewModal.attr("aria-hidden", "false");
		});

		$lookupnewModal.on('hidden.bs.modal', function () {
			$lookupnewModal.attr("aria-hidden", "true");
			$element.closest("td.lookup.form-control-cell").find(".new-value").trigger("focus");
		});
		
		$modal.on('show.bs.modal', function () {
			$modal.attr("aria-hidden", "false");
			$grid.data("selected", $('#' + $this._lookupDataFieldName).val());
			$grid.trigger("refresh");
		});

		$modal.on('hidden.bs.modal', function () {
			$modal.attr("aria-hidden", "true");
			$modal.find(".alert-danger.error").remove();
			$grid.children(".view-toolbar").find(".view-search input.query").val('');
			$grid.children(".view-grid").find("tr.selected").each(function () {
				$(this).find("td:first").empty();
				$(this).removeClass("selected").removeClass("info");
			});
			$grid.children(".view-toolbar").find(".toggle-related-filter").addClass("active");
		});
	}

	entityLookup.prototype.setFieldValue = function () {
		var $this = this;
		var $element = $this._element;
		var $button = $element.find(".modal-footer button.primary");
		var $modal = $element.find(".modal-lookup");
		var $grid = $element.find(".entity-grid");
		var lookupDataFieldName = $this._lookupDataFieldName;

		var $field = $("#" + lookupDataFieldName);

		if ($field.length == 0) {
			return;
		}

		var $nameField = $("#" + lookupDataFieldName + "_name");

		if ($nameField.length == 0) {
			return;
		}

		var $lookupTargetField = $("#" + lookupDataFieldName + "_entityname");

		if ($lookupTargetField.length == 0) {
			return;
		}

		var items = $.map($grid.children(".view-grid").find("tr.selected"), function (e) {
			return { LogicalName: $(e).attr("data-entity"), Id: $(e).attr("data-id"), Name: $(e).attr("data-name") }
		});

		if (items.length <= 0) {
			return;
		}

		$button.attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");

		$nameField.val(items[0].Name);

		$lookupTargetField.val(items[0].LogicalName);

		$field.val(items[0].Id).trigger("change");

		$modal.modal("hide");

		$element.closest("td.lookup.form-control-cell").find(".clearlookupfield").show();

		$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();

		if (setIsDirty && $.isFunction(setIsDirty)) {
			setIsDirty($field.attr("id"));
		}

		$nameField.trigger("focus");
	}

	entityLookup.prototype.clearFieldValue = function () {
		var $this = this;
		var $element = $this._element;
		var $button = $element.find(".modal-footer button.primary");
		var $modal = $element.find(".modal-lookup");
		var lookupDataFieldName = $this._lookupDataFieldName;
		var $field = $("#" + lookupDataFieldName);

		if ($field.length == 0) {
			return;
		}

		var $nameField = $("#" + lookupDataFieldName + "_name");

		if ($nameField.length == 0) {
			return;
		}
		
		var $lookupTargetField = $("#" + lookupDataFieldName + "_entityname");

		if ($lookupTargetField.length == 0) {
			return;
		}

		$button.attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");

		$nameField.val('');

		$lookupTargetField.val('');

		$field.val('').trigger("change");

		$modal.modal("hide");

		$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();

		$element.closest("td.lookup.form-control-cell").find(".clearlookupfield").hide();

		if (setIsDirty && $.isFunction(setIsDirty)) {
			setIsDirty($field.attr("id"));
		}

		$nameField.trigger("focus");
	}

}(jQuery));
