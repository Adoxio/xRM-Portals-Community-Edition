/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

$(function () {

  $.unblockUI();

	var formActionOriginal = $("#content_form").attr("action");
  var formActionOverride = $('#form-action-override');
  
  if (formActionOverride.length > 0) {
    $("#content_form").attr("action", formActionOverride.val());
    $("#content_form").attr("method", "POST");
  }

	$("#PreviousButton").click(function() {
		$("#content_form").attr("action", formActionOriginal);
		__doPostBack($("#PreviousButton").attr("name"), '');
	});

	$("#PreviousButton").attr("onclick", "");

  if ($('.credit-card-payment').length > 0) {
    $("#PreviousButton").addClass("cancel");

    $.validator.addMethod("validateExpiryMonthYear", function () {
      var expiryMonth = $("#ExpiryMonth").val();
      var expiryYear = $("#ExpiryYear").val();
      return !((expiryMonth == null || expiryMonth.length == 0) || (expiryYear == null || expiryYear.length == 0));
    }, "Month & Year are required.");

    $("#content_form").validate({
      rules: {
        x_first_name: { required: true },
        x_last_name: { required: true },
        x_email: {
          required: true,
          email: true
        },
        x_address: { required: true },
        x_city: { required: true },
        x_zip: { required: true },
        x_state: {
          required: true
        },
        x_country: {
          required: true
        },
        ExpiryMonth: {
          validateExpiryMonthYear: true
        },
        ExpiryYear: {
          validateExpiryMonthYear: true
        },
        x_card_num: {
          required: true,
          digits: true
        },
        x_card_code: {
          required: true,
          minlength: 3,
          digits: true
        }
      },
      errorClass: "help-block error",
      groups: { nameGroup: "ExpiryMonth ExpiryYear" },
      errorPlacement: function (error, element) {
        if (element.attr("name") == "ExpiryMonth" || element.attr("name") == "ExpiryYear") {
          error.insertAfter("#ExpiryYear");
        } else if (element.attr("name") == "x_card_code") {
          error.insertAfter("#cvv-help-link");
        } else {
          error.insertAfter(element);
        }
      },
      highlight: function (label) {
        $(label).closest('.form-group').removeClass('has-success').addClass('has-error');
      },
      success: function (label) {
        $(label).closest('.form-group').removeClass('has-error').addClass('has-success');
      },
      debug: true
    });

    $("#ExpiryMonth").val($("#ExpiryMonthDefault").val());

    $(document).on("change", "#ExpiryMonth", function () {
      updateExpiry();
    });

    populateExpiryYear();

    $("#ExpiryYear").val($("#ExpiryYearDefault").val());

    $(document).on("change", "#ExpiryYear", function () {
      updateExpiry();
    });
  }

  function populateExpiryYear() {
    var now = new Date();
    var currentyear = now.getFullYear();
    for (var i = 0; i < 20; i++) {
      var year = currentyear + i;
      $("#ExpiryYear").append($("<option />").attr("value", year).text(year));
    }
  }

  function updateExpiry() {
    var expdate = $("#x_exp_date");
    var expiryMonth = $("#ExpiryMonth").val();
    var expiryYear = $("#ExpiryYear").val();
    if ((expiryMonth == null || expiryMonth.length == 0) || (expiryYear == null || expiryYear.length == 0)) {
      expdate.val('');
      return;
    }
    var exp = expiryMonth + expiryYear.substr(2);
    expdate.val(exp);
  }
});
