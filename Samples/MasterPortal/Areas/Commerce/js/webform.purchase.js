/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


$(function () {

  $.unblockUI();
  
  function getRules() {
    var rules = {};

    $('input.required').each(function() {
      rules[$(this).attr('id')] = { required: true };
    });

    return rules;
  }
  
  $("#content_form").validate({
    rules: getRules(),
    errorClass: "help-block error",
    errorPlacement: function (error, element) {
      error.insertAfter(element);
    },
    highlight: function (label) {
      $(label).closest('.form-group').removeClass('has-success').addClass('has-error');
    },
    success: function (label) {
      $(label).closest('.form-group').removeClass('has-error').addClass('has-success');
    },
    debug: true
  });

});
