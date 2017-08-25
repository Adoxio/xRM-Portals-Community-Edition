/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

$(function () {
    setTimeout(function () {
        $('span.cke_button_icon').removeAttr('style');
        $('span.cke_button_icon').css('overflow', 'hidden');
        $('span.cke_button_icon.cke_button__bold_icon').addClass('hc_button__bold_icon');
        $('span.cke_button_icon.cke_button__cut_icon').addClass('hc_button__cut_icon');
        $('span.cke_button_icon.cke_button__copy_icon').addClass('hc_button__copy_icon');
        $('span.cke_button_icon.cke_button__paste_icon').addClass('hc_button__paste_icon');
        $('span.cke_button_icon.cke_button__pastetext_icon').addClass('hc_button__pastetext_icon');
        $('span.cke_button_icon.cke_button__pastefromword_icon').addClass('hc_button__pastefromword_icon');
        $('span.cke_button_icon.cke_button__undo_icon').addClass('hc_button__undo_icon');
        $('span.cke_button_icon.cke_button__redo_icon').addClass('hc_button__redo_icon');
        $('span.cke_button_icon.cke_button__link_icon').addClass('hc_button__link_icon');
        $('span.cke_button_icon.cke_button__unlink_icon').addClass('hc_button__unlink_icon');
        $('span.cke_button_icon.cke_button__image_icon').addClass('hc_button__image_icon');
        $('span.cke_button_icon.cke_button__table_icon').addClass('hc_button__table_icon');
        $('span.cke_button_icon.cke_button__horizontalrule_icon').addClass('hc_button__horizontalrule_icon');
        $('span.cke_button_icon.cke_button__specialchar_icon').addClass('hc_button__specialchar_icon');
        $('span.cke_button_icon.cke_button__maximize_icon').addClass('hc_button__maximize_icon');
        $('span.cke_button_icon.cke_button__source_icon').addClass('hc_button__source_icon');
        $('span.cke_button_icon.cke_button__removeformat_icon').addClass('hc_button__removeformat_icon')
        $('span.cke_button_icon.cke_button__strike_icon').addClass('hc_button__strike_icon');
        $('span.cke_button_icon.cke_button__italic_icon').addClass('hc_button__italic_icon');
        $('span.cke_button_icon.cke_button__numberedlist_icon').addClass('hc_button__numberedlist_icon');
        $('span.cke_button_icon.cke_button__blockquote_icon').addClass('hc_button__blockquote_icon');
        $('span.cke_button_icon.cke_button__indent_icon').addClass('hc_button__indent_icon');
        $('span.cke_button_icon.cke_button__outdent_icon').addClass('hc_button__outdent_icon');
        $('span.cke_button_icon.cke_button__bulletedlist_icon').addClass('hc_button__bulletedlist_icon');
    }, 2000);

});
