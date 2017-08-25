/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


$(document).ready(function () {
    if ($('[data-badge]'))
    {
        var items = {}, base, key;
        $('[data-badge]').each(function (i, e) {
            key = $(e).attr("data-uri");
            if (!items[key]) {
                items[key] = Array();
            }
            items[key].push($(e));
        });

        $.each(items, function (i, e) {
            if (e[0].attr('data-uri'))
            {
                $.ajax({
                    type: 'GET',
                    url: e[0].attr('data-uri'),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'html'
                }).done(function (msg) {

                    $.each(e, function (idx, badge) {
                        $(badge).html(msg);
                        $(badge).find('[data-toggle="popover"]').popover({
                            html: true, template: '<div class="popover" role="tooltip"><div class="arrow"></div><h3 class="popover-title"></h3><div class="popover-content" style="padding-bottom:3px;"></div></div>'
                        });
                    });
                });
            }
            
        });
    }
});
