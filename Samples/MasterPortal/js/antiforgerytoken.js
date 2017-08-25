/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// shell to add tokens.
(function (shell, $) {

    function ajaxRetry(options, retryCount) {
        // retry ajax with retryCount as no of retry attempts.
        var retryAjax = $.Deferred();
        (function ajaxInternal() {
            $.ajax(options).done(retryAjax.resolve).fail(function () {
                console.log("AjaxRetry attempt :".concat(retryCount));
                retryCount--;
                if (retryCount > 0) {
                    ajaxInternal();
                } else {
                    retryAjax.rejectWith(this, arguments); // pass the last failed ajax args and this
                }
            });
        })();

        return retryAjax.promise();
    };

    function getTokenDeferred() {
        // makes ajax call if required to get the csrf token other wise its from the page.
        var tokenAjax = $.Deferred();
        var token = $("#antiforgerytoken input[name=\"__RequestVerificationToken\"]").val();
        var ajaxtokenurl = $("#antiforgerytoken").attr("data-url");
        if (!token) {
            ajaxRetry({
                type: "GET",
                url: ajaxtokenurl,
                cache: false
            }, 3).done(
                function (tokenfield) {
                    $("#antiforgerytoken").html(tokenfield);
                    tokenAjax.resolve($(tokenfield).val());
                }).fail(function (xhr) {
                    if (xhr && xhr.responseText) {
                        var errorDetails = "GetAntiForgeryToken failed".concat("Details: ", xhr.responseText);
                        console.log(errorDetails);
                    }
                    tokenAjax.rejectWith(this, arguments);
                });
        } else {
            tokenAjax.resolve(token);
        }
        return tokenAjax.promise();
    };

    function refreshToken() {
        $("input[name=\"__RequestVerificationToken\"]").each(function () {
        var token = this;
        var ajaxtokenurl = $("#antiforgerytoken").attr("data-url");
        ajaxRetry({
            type: "GET",
            url: ajaxtokenurl,
            cache: false
        }, 3).done(
            function (tokenfield) {
                $(token).replaceWith(tokenfield);
            }).fail(function (xhr) {
                if (xhr && xhr.responseText) {
                    var errorDetails = "GetAntiForgeryToken failed".concat("Details: ", xhr.responseText);
                    console.log(errorDetails);
                }
            });
        });
    };

    function updateOptions(token, ajaxOptions, ajaxForm) {
        // updates ajax options to insert token on header or form data.
        if (ajaxOptions.mimeType && ajaxOptions.mimeType === "multipart/form-data") { // add token on formdata for form-data type
            if (!ajaxOptions.data) {
                ajaxOptions.data = new FormData();
            }
            ajaxOptions.data.append("__RequestVerificationToken", token);
        } else {
            if (ajaxForm) {
                // add form token for form.plugin 
                $("<input>").attr("name", "__RequestVerificationToken").attr("type", "hidden").appendTo(ajaxForm).val(token);
            } else {
                // add headers for ajax
                if (!ajaxOptions.headers) {
                    var headers = {};
                    headers["__RequestVerificationToken"] = token;
                    $.extend(ajaxOptions, {
                        headers: headers
                    });
                } else {
                    ajaxOptions.headers["__RequestVerificationToken"] = token;
                }
            }
        };
    };

    function ajaxSafePost(options, form) {
        /// <summary>wrapper function on $.ajax on post & form.ajaxSubmit which csrf token to header or on form.</summary>
        /// <param name="options" type="object">ajax options.</param>
        /// <param name="form" type="object">jQuery.Form plugin, dom element representing the form object.</param>
        /// <returns type="$.Deferred"> returns a promise which is resolved/rejected based on the result from the API $.ajax or form.ajaxSubmit </returns>

        var deferredAjax = $.Deferred();

        getTokenDeferred().done(function (token) {
            updateOptions(token, options, form); //update the ajax options and add header or on form.
            if (form) {
                form.ajaxSubmit(options); //form submit for form.plugin
            } else {
                $.ajax(options).done(deferredAjax.resolve).fail(deferredAjax.reject); //ajax
            }
        }).fail(function () {
            deferredAjax.rejectWith(this, arguments); // on token failure pass the token ajax and args
        });

        return deferredAjax.promise();
    };

    shell.ajaxSafePost = ajaxSafePost;
    shell.getTokenDeferred = getTokenDeferred;
    shell.refreshToken = refreshToken;

})(window.shell = window.shell || {}, jQuery)
