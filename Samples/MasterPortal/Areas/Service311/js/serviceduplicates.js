/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

$(document).ready(function () {
	$('#PanelResolveDuplicates thead tr').prepend($('<th/>').append($('<span class="fa fa-pencil" aria-hidden="true"></span>')));

	$("#CurrentList").children("div").children(".entity-grid").on("loaded", function() {
		$(this).children(".view-grid").find("tbody").on("click", "tr", function () {
			var $tr = $(this);
			if ($tr.hasClass("selected")) {
				var id = $tr.attr("data-id");
				$('#SelectedServiceRequestId').val(id);
				$('#NextButton').attr("disabled", false);
				$("#DuplicateList").children("div").children(".entity-grid").children(".view-grid").find("tbody tr.selected").each(function() {
					$(this).find("td:first").empty();
					$(this).removeClass("selected").removeClass("info");
				});
			} else {
				if ($("#DuplicateList").children("div").children(".entity-grid").children(".view-grid").find("tbody tr.selected").length == 0) {
					$('#NextButton').attr("disabled", true);
				}
			}
		});
	});


	$("#DuplicateList").children("div").children(".entity-grid").on("loaded", function() {
		$(this).children(".view-grid").find("tbody").on("click", "tr", function() {
			var $tr = $(this);
			if ($tr.hasClass("selected")) {
				var id = $tr.attr("data-id");
				$('#SelectedServiceRequestId').val(id);
				$('#NextButton').attr("disabled", false);
				$("#CurrentList").children("div").children(".entity-grid").children(".view-grid").find("tbody tr.selected").each(function () {
					$(this).find("td:first").empty();
					$(this).removeClass("selected").removeClass("info");
				});
			} else {
				if ($("#CurrentList").children("div").children(".entity-grid").children(".view-grid").find("tbody tr.selected").length == 0) {
					$('#NextButton').attr("disabled", true);
				}
			}
		});
	});

	$('#NextButton').attr("disabled", true);

	$('#confirmSubmitContinue').click(function() {
		$('#confirmSubmit').attr("data-continue", true);
		$('#NextButton').click();
	});
});
