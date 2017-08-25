/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


if (window.jQuery) {
	(function ($) {
	    $(document).ready(function () {

	        // Linkify readonly url and email input types.
			$(".entity-form input[type='url'][readonly], .entity-form input[type='url'][disabled]").each(function () {
				var url = null;
				var text = '';
				if ($(this).hasClass("tickersymbol")) {
					url = "http://go.microsoft.com/fwlink?linkid=8506&Symbol=" + encodeURIComponent($(this).val().toString().toUpperCase());
					text = $(this).val();
				} else {
					url = $(this).val();
					text = url;
				}
				if (text == null || text == '' || url == null || url == '') {
					return;
				}
				var $parent = $(this).parent();
				var $container = $("<div></div>").addClass("form-control").attr("readonly", "readonly");
				var $link = $("<a></a>").addClass("text-primary").css("cursor", "pointer").attr("href", url).attr("target", "_blank").text(text);
				$(this).hide();
				$container.append($link).appendTo($parent);
			});
			$(".entity-form input[type='email'][readonly], .entity-form input[type='email'][disabled]").each(function () {
				var email = $(this).val();
				if (email == null || email == '') {
					return;
				}
				var $parent = $(this).parent();
				var $container = $("<div></div>").addClass("form-control").attr("readonly", "readonly");
				var $link = $("<a></a>").addClass("text-primary").css("cursor", "pointer").attr("href", "mailto:" + email).text(email);
				$(this).hide();
				$container.append($link).appendTo($parent);
			});

			// Add "required" attribute anywhere it couldn't be done on the server
			$(".entity-form span[data-required]").each(function () {
				$(this).find("input,select,textarea").prop("required", $(this).data("required"));
			});

			$(".entity-form").on("keypress.entityform", "*", function(e) {
				var keyCode = e.keyCode ? e.keyCode : e.which;
				if (keyCode == '13') {
					e.stopPropagation();
				}
			});

			$("form").find("input[type=submit],button").on("keypress.entityform", function (e) {
				var keyCode = e.keyCode ? e.keyCode : e.which;
				if (keyCode == '13') {
					e.preventDefault();
					e.stopPropagation();
					$(this).trigger("click");
				}
			});
		});
	}(window.jQuery));
}
window.onload = function ()
{
    setfocusOnSuccessMessage();
};
function setfocusOnSuccessMessage()
{
    if ($('#MessageLabel').is(':visible'))
    {
        $('div.status > span.label-default').text($('#casetypecode option[selected="selected"]').text());
        $('#MessageLabel').attr('tabindex', 0);
        $('#MessageLabel').focus();
    }
}
// Simple helper to return the "exMaxLen" attribute for
// the specified field.  Using "getAttribute" won't work
// with Firefox.
function GetMaxLength(targetField)
 {
	 return targetField.exMaxLen;
 }

//
// Limit the text input in the specified field.
//
function LimitInput(targetField, sourceEvent)
{
	var isPermittedKeystroke;
	var enteredKeystroke;
	var maximumFieldLength;
	var currentFieldLength;
	var inputAllowed = true;
	var selectionLength = parseInt(GetSelectionLength(targetField));
	
	if ( GetMaxLength(targetField) != null )
	{
		// Get the current and maximum field length
		currentFieldLength = parseInt(targetField.value.length);
		maximumFieldLength = parseInt(GetMaxLength(targetField));

		// Allow non-printing, arrow and delete keys
		enteredKeystroke = window.event ? sourceEvent.keyCode : sourceEvent.which;
		isPermittedKeystroke = ((enteredKeystroke < 32)
			|| (enteredKeystroke >= 33 && enteredKeystroke <= 40)
				|| (enteredKeystroke == 46));

		// Decide whether the keystroke is allowed to proceed
		if ( !isPermittedKeystroke )
		{
			if ( ( currentFieldLength - selectionLength ) >= maximumFieldLength ) 
			{
				inputAllowed = false;
			}
		}
		
		// Force a trim of the textarea contents if necessary
		if ( currentFieldLength > maximumFieldLength )
		{
			targetField.value = targetField.value.substring(0, maximumFieldLength);
		}
	}
	
	sourceEvent.returnValue = inputAllowed;
	return (inputAllowed);
}

//
// Limit the text input in the specified field.
//
function LimitPaste(targetField, sourceEvent)
{
	var clipboardText;
	var resultantLength;
	var maximumFieldLength;
	var currentFieldLength;
	var pasteAllowed = true;
	var selectionLength = GetSelectionLength(targetField);

	if ( GetMaxLength(targetField) != null )
	{
		// Get the current and maximum field length
		currentFieldLength = parseInt(targetField.value.length);
		maximumFieldLength = parseInt(GetMaxLength(targetField));

		clipboardText = window.clipboardData.getData("Text");
		resultantLength = currentFieldLength + clipboardText.length - selectionLength;
		if ( resultantLength > maximumFieldLength)
		{
			pasteAllowed = false;
		}
	}
	
	sourceEvent.returnValue = pasteAllowed;
	return (pasteAllowed);
}

//
// Returns the number of selected characters in 
// the specified element
//
function GetSelectionLength(targetField)
{
	if ( targetField.selectionStart == undefined )
	{
		return document.selection.createRange().text.length;
	}
	else
	{
		return (targetField.selectionEnd - targetField.selectionStart);
	}
}

// Rounds the value of the specified element to the precision provided
function setPrecision(id, precision) {
	if (!id) {
		return;
	}
	precision = precision || 0;
	var y = document.getElementById(id).value;
	if (y == "" || isNaN(y)) {
		return;
	}
	document.getElementById(id).value = (parseFloat(y)).toFixed(precision);
}

// Opens ticker symbol with web url in a new window
function launchTickerSymbolUrl(symbol) {
	if (symbol == '') {
		return false;
	}
	var url = "http://go.microsoft.com/fwlink?linkid=8506" + "&Symbol=" + encodeURIComponent(symbol.toString().toUpperCase());
	window.open(url, '_blank');
	return false;
}

// Sets the input elements string to uppercase
function uppercaseTickerSymbol(element) {
	if (element.value == '') {
		return;
	}
	element.value = element.value.toUpperCase();
}

// Opens a url in a new window
function launchUrl(url) {
	if (url == '') {
		return false;
	}
	var scheme = getUrlScheme(url);
	var validUrl;
	switch (scheme.toLowerCase()) {
		case "http": case "https": case "ftp": case "ftps": case "onenote": case "tel":
			validUrl = true;
			break;
		default:
			validUrl = false;
			break;
	}
	if (validUrl) {
		window.open(url, '_blank');
		return false;
	}
	return false;
}

// Launch email
function launchEmail(email) {
	if (email == '') {
		return false;
	}
	window.location.href = "mailto:" + email;
	return false;
}

// Returns the scheme of a url such as http, https, ftp etc.
function getUrlScheme(value) {
	var index = value.indexOf("://");
	if (index === -1) {
		return "";
	}
	else {
		return value.substr(0, index);
	}
}

// Ensures the input url has a valid scheme
function validateUrlInput(element, maxLength) {
	var url = element.value;
	element.value = validateUrlProtocol(url, maxLength);
}

// If the url scheme is not valid it prepends a valid scheme to the url.
function validateUrlProtocol(url, maxLength) {
	if (url == '') {
		return url;
	}
	maxLength = maxLength || 100;
	var scheme = getUrlScheme(url);
	switch (scheme.toLowerCase()) {
		case "http": case "https": case "ftp": case "ftps": case "onenote": case "tel": return url;
		case "": return prefixHttp(url, maxLength);
		default:
			alert('Invalid Protocol. Only HTTP, HTTPS, FTP, FTPS, ONENOTE and TEL protocols are allowed in this field.');
			return url;
	}
}

// Returns a url with a valid scheme
function prefixHttp(url, maxlength) {
	url = url.trim();
	if ("http://" != url.substr(0, "http://".length).toLowerCase() && "https://" != url.substr(0, "https://".length).toLowerCase()) url = "http://" + url.substring(0, maxlength - "http://".length);
	return url;
}

// Scroll to element and focus on element.
function scrollToAndFocus(scollToId, focusOnId) {
	if (focusOnId == null || focusOnId.length <= 0) {
		return;
	}
	if (scollToId == null || scollToId.length <= 0) {
		scrollToPosition(focusOnId);
	} else {
		scrollToPosition(scollToId);
	}
	setFocus(focusOnId);
}

// Set focus on the element with the specified id.
function setFocus(id) {
	if (id == null) {
		return;
	}
	var element = document.getElementById(id);
	if (element != null) {
		element.focus();
	}
}

// Scroll to the position of the element with the specified id.
function scrollToPosition(id) {
	if (id == null) {
		return;
	}
	var element = document.getElementById(id);
	var posX = element.offsetLeft;
	var posY = element.offsetTop;
	var parentElement = element.offsetParent;
	while (parentElement != null) {
		posX += parentElement.offsetLeft;
		posY += parentElement.offsetTop;
		parentElement = parentElement.offsetParent;
	}
	window.scrollTo(posX, posY);
}

function disableButtons() {
	var inputs = document.getElementsByTagName("input");
	for (var i = 0, j = inputs.length; i < j; i++) {
		if (inputs[i].type === 'submit' || inputs[i].type === 'button') {
			inputs[i].disabled = true;
		}
	}
}

// Remove whitespace from the ends of a string
String.prototype.trim = function() {
	return this.replace( /^\s+|\s+$/g , "");
};

document.getElementsByClassName = function (cl) {
	var retnode = [];
	var myclass = new RegExp('\\b' + cl + '\\b');
	var elem = this.getElementsByTagName('*');
	for (var i = 0, j = elem.length; i < j; i++) {
		var classes = elem[i].className;
		if (myclass.test(classes)) retnode.push(elem[i]);
	}
	return retnode;
};

// Updates the total of a constant sum composite control
function updateConstantSum(name) {
	var elements = document.getElementsByClassName(name);
	var total = 0;
	for (var i = 0, j = elements.length; i < j; i++) {
		var value = elements[i].value;
		if (!isNaN(value) && value.length > 0) {
			total += parseInt(value);
		}
		if (i == (j - 1)) {
			var totalField = document.getElementById("ConstantSumTotalValue" + name);
			if (totalField != null) {
				totalField.value = total;
			}
		}
	}
};

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
			var message = window.ResourceManager['Click_Stay_To_Save_Your_Changes'];
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


if (window.jQuery) {
	(function ($) {
		if (typeof (Page_ClientValidate) != 'undefined') {
			var originalValidationFunction = Page_ClientValidate;
			if (originalValidationFunction && typeof (originalValidationFunction) == "function") {
				Page_ClientValidate = function() {
					originalValidationFunction.apply(this, arguments);
					if (typeof (Page_IsValid) != 'undefined' && !Page_IsValid) {
						$(".validation-summary").find("a").first().focus();
						return false;
					}
					return true;
				};
			}
		}
	}(window.jQuery));
}
