/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function ($) {

	function quickform(element) {
		this._element = $(element);
		this._lookupElement = $("#" + this._element.data("lookup-element"));
		this._lookupEntityNameElement = $("#" + this._element.data("lookup-element") + "_entityname");
		this._path = this._element.data("path");
		this._controlId = this._element.data("controlid");
		this._formName = this._element.data("formname");
	}

	$(document).ready(function () {
		$("iframe.quickform").each(function () {
			new quickform($(this)).bindEvents();
		});
	});

	quickform.prototype.bindEvents = function () {
		var $this = this;
		var $element = $this._element;
		var $lookup = $this._lookupElement;
		var $lookupEntityName = $this._lookupEntityNameElement;
		$element.on("load.quickform", function() {
			$this.resize();
		});
		$lookup.on("change.quickform", function() {
			var recordId = $(this).val();
			if (recordId == null || recordId == '') {
				$this.clear();
			} else {
				var entityName = $lookupEntityName.val();
				$this.reload(recordId, entityName);
			}
		});
	}

	quickform.prototype.reload = function (recordId, entityName, entityPrimaryKeyName) {
		var $this = this;
		var $element = $this._element;
		var formName = $this._formName;
		var controlId = $this._controlId;
		var path = $this._path;
		if (typeof (entityPrimaryKeyName) == "undefined") entityPrimaryKeyName = "";
		var src = path + "?entityid=" + recordId + "&entityname=" + entityName + "&entityprimarykeyname=" + entityPrimaryKeyName + "&formname=" + formName + "&controlid=" + controlId;
		$element.attr("src", src);
	}

	quickform.prototype.clear = function () {
		var $this = this;
		var $element = $this._element;
		$element.attr("src", "about:blank");
		$element.css("height", 0);
	}

	quickform.prototype.resize = function() {
		var $this = this;
		var $element = $this._element;
		$element.css("height", 0);
		$element.css("height", ($element.contents().outerHeight() + 4) + 'px');
	}

}(jQuery));
