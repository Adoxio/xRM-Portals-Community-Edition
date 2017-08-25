/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function ($, opener) {
  function getQuery() {
    var parts = document.location.search.replace(/(^\?)/, '').split("&"),
      query = {};

    $.each(parts, function () {
      var pair = this.split('='), key = pair[0], value = pair[1];

      if (key) {
        query[key] = decodeURIComponent(value || '');
      }
    });

    return query;
  };

  var query = getQuery(),
    serviceUri = query['url'],
    ckEditorFuncNum = query['CKEditorFuncNum'],
    selector = '#filebrowser';

  if (!serviceUri) {
    return;
  }

  $(function () {
    var url = opener.$("#antiforgerytoken").attr("data-url");
	$.ajax({
	    url: url
	}).done(function (data) {
		if (data) {
			var tokenElement = $(data).filter("input[name=__RequestVerificationToken]");
			if (tokenElement) {
				$("meta[name=csrf-token]").attr("content", tokenElement.val());
			}
		}
	});

    $(selector).elfinder({
      url: serviceUri,
      rememberLastDir: false,
      editorCallback: function (selected) {
        opener.CKEDITOR.tools.callFunction(ckEditorFuncNum, selected);
        window.close();
      },
        lang: 'all',
      closeOnEditorCallback: false
    });
  });

  function resize() {
    var documentHeight = $(window).height();
    var toolbarHeight = $('.el-finder-toolbar').outerHeight();
    var statusbarHeight = $('.el-finder-statusbar').outerHeight();
    var workzoneHeight = $('.el-finder-workzone').outerHeight();
    var navPaneHeight = $('.el-finder-nav').height();
    var cwdPaneHeight = $('.el-finder-cwd').height();
    var workzoneExtra = workzoneHeight - (navPaneHeight > cwdPaneHeight ? navPaneHeight : cwdPaneHeight);

    $('.el-finder-nav, .el-finder-cwd').height((documentHeight - (toolbarHeight + statusbarHeight) - workzoneExtra));
  }

  $(window).load(resize).resize(resize);

}(jQuery, window.opener))
