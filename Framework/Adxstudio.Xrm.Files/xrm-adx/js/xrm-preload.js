/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


if (typeof XRM == "undefined" || !XRM) {
	var XRM = {};
}

(function() {

  // This code is meant to be loaded prior to the Microsoft.Xrm.Portal.Files scripts. Its current
  // single purpose is to patch console logging functions to filter out certain messages that are
  // no longer useful to users, but that we cannot change.

  if ((typeof(console) == 'undefined') || !console) {
    return;
  }

  function isActivatorMessage(args) {
    if (args.length < 2) {
      return false;
    }

    var source = args[1];

    return source && source.match && source.match(/XRM\.activator/);
  }

  XRM.__consoleInfo = (function (original) {
    if (!original) {
      return null;
    }

    console.info = function () {
      if (!isActivatorMessage(arguments)) {
        original.apply(console, arguments);
      }
    }

    return original;
  })(console.info);

  XRM.__consoleLog = (function (original) {
    if (!original) {
      return null;
    }

    console.log = function () {
      if (!isActivatorMessage(arguments)) {
        original.apply(console, arguments);
      }
    }

    return original;
  })(console.log);

  XRM.__consoleWarn = (function (original) {
    if (!original) {
      return null;
    }

    console.warn = function () {
      if (!isActivatorMessage(arguments)) {
        original.apply(console, arguments);
      }
    }

    return original;
  })(console.warn);

})();
