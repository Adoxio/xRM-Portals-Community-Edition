/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

XRM.onActivate(function () {
  
  XRM.editable.Entity.getPropertyName = function (entityLogicalName, attributeLogicalName) {
    return attributeLogicalName;
  };

  XRM.util.formatDateForDataService = function (date) {
    return "\/Date(" + date.getTime() + ")\/";
  };
});
