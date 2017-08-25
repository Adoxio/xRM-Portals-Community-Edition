/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


+function ($, URI) {
  'use strict';

  $(document).on('click.adx.serialized-query.data-api', '[data-serialized-query]', submitSerializedQuery);

  $(document).ready(function () {
    $('[data-serialized-query]').each(initializeRadios);
  });

  function submitSerializedQuery(e) {
    var $this = $(this),
      key = $this.data("serialized-query"),
      target = $this.data("target"),
      $entitylist = $this.closest(".entitylist"),
      $entitygrid = $entitylist.find(".entity-grid").filter(":first"),
      value,
      uri;
    if ($entitygrid.length == 1 && target) {
        e.preventDefault();
        var metaFilter = $(target).find('input,select').serialize();
        $entitygrid.trigger("metafilter", metaFilter);
    } else if (target && key) {
      value = $(target).find('input,select').serialize();
      uri = new URI();
      uri.setSearch(key, value);
      window.location.href = uri.toString();
    }
  };

  function initializeRadios() {
    var target = $(this).data("target");

    if (target) {
      $(target).find('input:radio').click(function () {
        var $this = $(this);

        if ($this.data("checked")) {
          $this.data("checked", null);
          $this.prop("checked", false);
        } else {
          $(target).find("input:radio[name=" + $this.prop("name") + "]").data("checked", null);
          $this.data("checked", true);
        }
      });
    }
  }

}(jQuery, URI);
