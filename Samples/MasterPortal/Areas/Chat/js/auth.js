/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function (auth, $) {
    "use strict";

    var callback = null;

    auth.getAuthenticationToken = function (callbackFn) {
        callback = callbackFn;

        $.ajax({
            type: 'GET',
            url: '/_services/auth/token',
            cache: false,
            success: handleGetAuthenticationTokenSuccess,
            error: function() { callback(null) }
        });
    }

    function handleGetAuthenticationTokenSuccess(data, status, jqXHR) {
        var jsonResult = JSON.parse(jqXHR.getResponseHeader('X-Responded-JSON'));
        if (jsonResult && jsonResult.status === 401) {
            var wasLoginForced = Number($.cookie("chatForcedLogin")) === 1;
            if (!wasLoginForced) {
                $.cookie("chatForcedLogin", 1);
                // If the user is not logged in and no token has not been returned, then force the user to log in.
                redirectToLogin();
            } else {
                $.cookie("chatForcedLogin", 0);
                callback("","Login Required");
            }
        } else {
            // Pass the token to the callback function from the chat widget.
            callback(data);
        }
    }

    function redirectToLogin() {
        var redirectUrl = window.location;
        var loginUrl = window.location.origin + '/SignIn?returnUrl=' + encodeURIComponent(redirectUrl);
        window.location = loginUrl;
    }
}(window.auth || (window.auth = {}), window.jQuery));
