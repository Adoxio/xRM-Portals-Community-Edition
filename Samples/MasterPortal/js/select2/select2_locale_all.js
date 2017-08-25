
(function ($) {
    "use strict";

    $.fn.select2.locales['all'] = {
        formatMatches: function (matches) { if (matches === 1) { return window.ResourceManager["Select2_One_Result_Available"]; } return matches + window.ResourceManager["Select2_Many_Results_Available"]; },
        formatNoMatches: function () { return window.ResourceManager["Select2_No_Matches_Found"]; },
        formatInputTooShort: function (input, min) { var n = min - input.length; return "Please enter " + n + " or more character" + (n == 1 ? "" : "s"); },
        formatInputTooLong: function (input, max) { var n = input.length - max; return "Please delete " + n + " character" + (n == 1 ? "" : "s"); },
        formatSelectionTooBig: function (limit) { return "You can only select " + limit + " item" + (limit == 1 ? "" : "s"); },
        formatLoadMore: function (pageNumber) { return window.ResourceManager["Select2_Loading_More"]; },
        formatSearching: function () { return window.ResourceManager["Select2_Searching"]; }
    };

    $.extend($.fn.select2.defaults, $.fn.select2.locales['all']);
})(jQuery);