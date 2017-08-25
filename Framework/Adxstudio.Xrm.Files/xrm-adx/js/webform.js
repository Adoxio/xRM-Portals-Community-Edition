/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

$(document).ready(function() {
	$(".stack-rank-cell").parents("table.section").each(function() {
		if (!$(this).hasClass("stack-rank")) {
			$(this).stackRanking();
		}
	});
});

function disableButtons() {
	var inputs = document.getElementsByTagName("input");
	for (var i = 0, j = inputs.length; i < j; i++) {
		if (inputs[i].type === 'submit' || inputs[i].type === 'button') {
			inputs[i].disabled = true;
		}
	}
}

// Adds class name "dirty"
function setIsDirty(id) {
	if (!id) {
		return;
	}
	var className = "dirty";
	var element = document.getElementById(id);
	if (element == null) {
		return;
	}
	if (!element.className.match(new RegExp('(\\s|^)' + className + '(\\s|$)'))) {
		element.className += " " + className;
	}
};

// Returns true if is dirty (i.e. an input has a class name 'dirty'. Otherwise returns false.
function isDirty() {
	var elements = document.getElementsByClassName("dirty");
	if (elements.length > 0) {
		return true;
	} else {
		return false;
	}
};

function clearIsDirty() {
	var elements = document.getElementsByClassName("dirty");
	for (var i = 0, j = elements.length; i < j; i++) {
		var className = "dirty";
		var reg = new RegExp('(\\s|^)' + className + '(\\s|$)');
		elements[i].className = elements[i].className.replace(reg, '');
	}
}

window.onbeforeunload = confirmExit;

function confirmExit() {
	var confirm = false;
	var confirmOnExit = document.getElementById("confirmOnExit");
	if (confirmOnExit != null) {
		if (confirmOnExit.value != null) {
			if (confirmOnExit.value == 'true') {
				confirm = true;
			}
		}
	}
	if (confirm) {
		// check to see if any changes to the data entry fields have been made
		if (isDirty()) {
			var message = "You have attempted to leave or refresh this page. Your changes have not been saved. To stay on the page to save your changes, click Stay.";
			var element = document.getElementById("confirmOnExitMessage");
			if (element != null) {
				if (element.value != null && element.value != '') {
					message = element.value;
				}
			}
			return message;
		}
		// no changes - return nothing
	}
};

// Remove whitespace from the ends of a string
String.prototype.trim = function () {
	return this.replace(/^\s+|\s+$/g, "");
};

document.getElementsByClassName = function (cl) {
	var retnode = [];
	var myclass = new RegExp('(\\b(?!-))' + cl + '(\\b(?!-))');
	var elem = this.getElementsByTagName('*');
	for (var i = 0, j = elem.length; i < j; i++) {
		var classes = elem[i].className;
		if (myclass.test(classes)) retnode.push(elem[i]);
	}
	return retnode;
};
