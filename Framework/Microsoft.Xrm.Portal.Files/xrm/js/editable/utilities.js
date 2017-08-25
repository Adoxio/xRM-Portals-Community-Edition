/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {

	var ns = XRM.namespace('util');
	var $ = XRM.jQuery;
	
	ns.expandUriTemplate = function(uriTemplate, data) {
		return uriTemplate.replace(/{([^}]+)}/g, function(match, capture) {
			return data[capture];
		});
	}
	
	ns.formatDateForDataService = function(date) {
		// Maybe add some better logic here later, but Astoria doesn't like the result
		// of IE's toUTCString, while it does for other browsers. Turns out the only
		// difference is that IE puts 'UTC' for the timezone, while the others put 'GMT'.
		// Hack to fix the IE value.
		return date.toUTCString().replace(/UTC/, 'GMT');
	}
	
	var uriParser = {
		pattern: /^(?:([^:\/?#]+):)?(?:\/\/((?:(([^:@]*)(?::([^:@]*))?)?@)?([^:\/?#]*)(?::(\d*))?))?((((?:[^?#\/]*\/)*)([^?#]*))(?:\?([^#]*))?(?:#(.*))?)/,
		key: ["source", "protocol", "authority", "userInfo", "user", "password", "host", "port", "relative", "path", "directory", "file", "query", "anchor"]
	}
	
	ns.parseUri = function(str) {
		var matches = uriParser.pattern.exec(str),
		    key = uriParser.key,
		    uri = {},
		    i = key.length;
		
		while (i--) uri[key[i]] = matches[i] || "";
		
		return uri;
	}
	
	ns.isSameDomain = function(location, url) {
		var parsedUri = ns.parseUri(url);
		
		return parsedUri.host == '' || parsedUri.host == location.host;
	}
	
	// Transforms a string value into a similar string usable as a URL slug/partial URL path.
	ns.slugify = function(value) {
		return value.
			replace(/^\s+/, ''). // Strip leading whitespace.
			replace(/\s+$/, ''). // Strip trailing whitespace.
			toLowerCase().
			replace(/[^-a-z0-9~\s\.:;+=_\/]/g, ''). // Strip chars that are not alphanumeric, or within a certain set of allowed punctuation.
			replace(/[\s:;=+]+/g, '-'). // Replace whitespace and certain punctuation with a hyphen.
			replace(/[\.]+/g, '.'); // Replace runs of multiple periods with a single period.
	}
	
	$.fn.extend({
		syncSlugify: function(source) {
			var slugify = XRM.util.slugify;
			source = $(source);
			var target = this;
			var oldValue = source.val();
			source.keyup(function() {
				var currentValue = source.val();
				if (slugify(oldValue) == target.val()) {
					target.val(slugify(currentValue));
				}
				oldValue = currentValue;
			});
		}
	});
	
	XRM.localizations = {
		'entity.create.label': 'New',
		'entity.create.adx_webfile.label': 'Child file',
		'entity.create.adx_webfile.tooltip': 'Create a new child file',
		'entity.create.adx_weblink.tooltip': 'Add a new link',
		'entity.create.adx_webpage.label': 'Child page',
		'entity.create.adx_webpage.tooltip': 'Create a new child page',
		'entity.delete.adx_weblink.tooltip': 'Delete this item',
		'entity.update.label': 'Edit',
		'adx_webfile.shortname': 'file',
		'adx_webfile.update.tooltip': 'Edit this file',
		'adx_weblink.update.tooltip': 'Edit this link',
		'adx_webpage.shortname': 'page',
		'adx_webpage.update.tooltip': 'Edit this page',
		'datetimepicker.datepicker.label': 'Choose a Date',
		'editable.cancel.label': 'Cancel',
		'editable.delete.label': 'Delete',
		'editable.delete.tooltip.prefix': 'Delete this ',
		'editable.label': 'Edit',
		'editable.label.prefix': 'Edit ',
		'editable.loading': 'Loading...',
		'editable.required.label': '(required)',
		'editable.save.label': 'Save',
		'editable.saving': 'Saving...',
		'editable.sortable.tooltip': 'Move this item',
		'editable.tooltip': 'Edit this content',
		'error.dataservice.default': 'An error has occured while saving your content. Your changes have not been saved.',
		'error.dataservice.loading': 'An error has occured while loading content required for this feature.',
		'error.dataservice.loading.field.prefix': 'Unable to load ',
		'confirm.delete.entity.prefix': 'Are you sure you want to delete this ',
		'confirm.no': 'No',
		'confirm.unsavedchanges': "You have unsaved changes. Proceed?",
		'confirm.yes': 'Yes',
		'validation.required.suffix': ' is a required field.',
		'sitemapchildren.header.label': 'Edit children'
	};

	XRM.localize = function(key) {
		return XRM.localizations[key];
	}

	XRM.yuiSkinClass = 'yui-skin-sam';
	XRM.zindex = 4;

	XRM.tinymceSettings = {
		theme: 'advanced',
		plugins: 'style,safari,advimage,inlinepopups,table,contextmenu,paste,media,searchreplace,directionality,fullscreen',
		mode: 'none',
		height: '340',
		theme_advanced_resizing: true,
		theme_advanced_resize_horizontal: false,
		theme_advanced_resizing_use_cookie: false,
		theme_advanced_statusbar_location: "bottom",
		theme_advanced_toolbar_location: "top",
		theme_advanced_toolbar_align: "left",
		theme_advanced_buttons1: "save,cancel,fullscreen,|,bold,italic,underline,strikethrough,|,justifyleft,justifycenter,justifyright,justifyfull,|,ltr,rtl,|,styleprops,formatselect,help",
		theme_advanced_buttons2: "cut,copy,paste,pastetext,pasteword,|,search,replace,|,bullist,numlist,|,outdent,indent,blockquote,|,undo,redo,|,link,unlink,anchor,image,media,cleanup,code",
		theme_advanced_buttons3_add_before: "tablecontrols,separator",
		width: "100%",
		convert_urls: false,
		forced_root_block: 'p',
		invalid_elements: "html,head,meta,title,body",
		remove_linebreaks: false,
		apply_source_formatting: true
	};

	XRM.tinymceCompactSettings = $.extend(true, {}, XRM.tinymceSettings);
	XRM.tinymceCompactSettings.height = null;
	XRM.tinymceCompactSettings.theme_advanced_toolbar_location = "external";
	XRM.tinymceCompactSettings.theme_advanced_statusbar_location = "none";

	XRM.namespace('editable').editableClassRegexp = /xrm-editable-(\S+)/i;
	
});
