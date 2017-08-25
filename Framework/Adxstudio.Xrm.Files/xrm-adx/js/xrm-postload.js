/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


(function() {

  // Restore the native console, if previously overridden.
  if ((typeof(console) == 'undefined') || !console) {
    return;
  }

  if (XRM && XRM.__consoleInfo) {
    console.info = XRM.__consoleInfo;
  }

  if (XRM && XRM.__consoleLog) {
    console.log = XRM.__consoleLog;
  }

  if (XRM && XRM.__consoleWarn) {
    console.warn = XRM.__consoleWarn;
  }

})();
