/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

jQuery(function () {
  jQuery('input:checkbox[id$=SameAsBilling]').change(function () {
    if (jQuery(this).is(':checked')) {
      jQuery('input:text[id$=address1]').val(jQuery('input:text[id$=address1_line1]').val());
      jQuery('input:text[id$=city]').val(jQuery('input:text[id$=address1_city]').val());
      jQuery('input:text[id$=country]').val(jQuery('input:text[id$=address1_country]').val());
      jQuery('input:text[id$=state]').val(jQuery('input:text[id$=address1_province]').val());
      jQuery('input:text[id$=zip]').val(jQuery('input:text[id$=address1_postalcode]').val());
    }
    else {
      jQuery('input:text[id$=address1]').val('');
      jQuery('input:text[id$=city]').val('');
      jQuery('input:text[id$=country]').val('');
      jQuery('input:text[id$=state]').val('');
      jQuery('input:text[id$=zip]').val('');
    }
  });
});
