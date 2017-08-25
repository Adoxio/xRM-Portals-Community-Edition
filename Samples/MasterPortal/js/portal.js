/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

var portal = portal || {};

portal.convertAbbrDateTimesToTimeAgo = function ($) {
	$("abbr.timeago").each(function () {
		var $this = $(this),
		  dateTime = Date.parse($this.text()),
		  momentDateTime;

		if (dateTime) {
			momentDateTime = moment(dateTime);
			var dateFormat = dateFormatConverter.convert($this.closest("[data-dateformat]").data("dateformat") || "MMMM d, yyyy", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
			var timeFormat = dateFormatConverter.convert($this.closest("[data-timeformat]").data("timeformat") || "h:mm tt", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
			var datetimeFormat = dateFormatConverter.convert($this.attr('data-format'), dateFormatConverter.dotNet, dateFormatConverter.momentJs) || (dateFormat + ' ' + timeFormat);
			$this.attr("title", momentDateTime.format("YYYY-MM-DDTHH:mm:ss"));
			$this.text(momentDateTime.format(datetimeFormat));
		}
	});

	$("abbr.timeago").timeago();

	$("abbr.posttime").each(function () {
		var $this = $(this),
		  dateTime = Date.parse($this.text()),
		  momentDateTime;

		if (dateTime) {
			momentDateTime = moment(dateTime);
			var dateFormat = dateFormatConverter.convert($this.closest("[data-dateformat]").data("dateformat") || "MMMM d, yyyy", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
			var timeFormat = dateFormatConverter.convert($this.closest("[data-timeformat]").data("timeformat") || "h:mm tt", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
			var datetimeFormat = dateFormatConverter.convert($this.attr('data-format'), dateFormatConverter.dotNet, dateFormatConverter.momentJs) || (dateFormat + ' ' + timeFormat);
			$this.attr("title", momentDateTime.format(datetimeFormat));
			$this.text(momentDateTime.format(datetimeFormat));
		}
	});
};

portal.initializeHtmlEditors = function () {
	var appPath = $('[data-app-path]').data('app-path') || '/';

	$(document).on('focusin', function (e) {
		if ($(e.target).closest(".mce-window").length) {
			e.stopImmediatePropagation();
		}
	});

	$(".html-editors textarea").each(function () {
		CKEDITOR.replace(this, {
			customConfig: '',
			language: $(this).closest('[crm-lang]').attr('crm-lang'),	// ckeditor only supports the CRM languages
			height: 240,
			uiColor: '#EEEEEE',
			contentsCss: [appPath + 'css/bootstrap.min.css', appPath + 'css/ckeditor.css'],
			stylesSet: 'portal',
			format_tags: 'p;h1;h2;h3;h4;h5;h6;pre',
			disableNativeSpellChecker: false,
			toolbarGroups: [
			  { name: 'clipboard', groups: ['clipboard', 'undo'] },
			  { name: 'links', groups: ['links'] },
			  { name: 'editing', groups: ['find', 'selection', 'spellchecker', 'editing'] },
			  { name: 'insert', groups: ['insert'] },
			  { name: 'forms', groups: ['forms'] },
			  { name: 'tools', groups: ['tools'] },
			  { name: 'document', groups: ['document', 'doctools', 'mode'] },
			  '/',
			  { name: 'basicstyles', groups: ['basicstyles', 'cleanup'] },
			  { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi', 'paragraph'] },
			  { name: 'styles', groups: ['styles'] },
			  { name: 'colors', groups: ['colors'] },
			  { name: 'others', groups: ['others'] },
			  { name: 'about', groups: ['about'] }
			],
			removeButtons: 'Save,NewPage,Preview,Print,Templates,SelectAll,Find,Replace,Form,Checkbox,Radio,TextField,Textarea,Select,Button,ImageButton,HiddenField,Anchor,Flash,Smiley,PageBreak,Iframe,ShowBlocks,Font,FontSize,CreateDiv,JustifyLeft,JustifyCenter,JustifyRight,JustifyBlock,BidiRtl,BidiLtr,Language,TextColor,BGColor,About,Subscript,Superscript,Underline',
			on: {
				change: function () {
					this.updateElement();
				}
			}
		});
	});
};

(function ($, XRM) {
	CKEDITOR.stylesSet.add('portal', [
	  { name: window.ResourceManager['CKEditor_Code_Style'], element: 'code' },
	  { name: window.ResourceManager['CKEditor_Code_Block_Style'], element: 'pre', attributes: { 'class': 'linenums prettyprint' } }
	]);

	portal.initializeHtmlEditors();

	$(function () {
		moment.locale($('html').attr('lang'));
		portal.convertAbbrDateTimesToTimeAgo($);

		var facebookSignin = $(".facebook-signin");
		facebookSignin.on("click", function (e) {
			e.preventDefault();
			window.open(facebookSignin.attr("href"), "facebook_auth", "menubar=1,resizable=1,scrollbars=yes,width=800,height=600");
		});

		$(".crmEntityFormView input:not([value])[readonly]:not([placeholder]), .crmEntityFormView input[value=''][readonly]:not([placeholder])").filter(function () {
			var value = $(this).val();
			return value == null || value.length === 0;
		}).each(function () {
			$(this).parent().css("position", "relative");
			$(this).after($("<div>&mdash;</div>").addClass("text-muted").css("position", "absolute").css("top", 4));
		});
		$(".crmEntityFormView select[disabled] option:checked[value='']").closest("select").each(function () {
			$(this).parent().css("position", "relative");
			$(this).after($("<div>&mdash;</div>").addClass("text-muted").css("position", "absolute").css("top", 4));
		});
		$(".crmEntityFormView textarea[readonly]").filter(function () {
			return $(this).val().length === 0;
		}).each(function () {
			$(this).parent().css("position", "relative");
			$(this).after($("<div>&mdash;</div>").addClass("text-muted").css("position", "absolute").css("top", 4));
		});

		// Map dropdowns with .btn-select class to backing field.
		$('.btn-select').each(function () {
			var select = $(this),
				target = $(select.data('target')),
				focus = $(select.data('focus')),
				label = $('.btn .selected', select);

			target.change(function () {
				var changedSelected = $('option:selected', target);
				select.find('.dropdown-menu li.active a').attr("aria-selected", false);
				select.find('.dropdown-menu li.active').removeClass('active');
				label.text(changedSelected.text());
			});

			$('.dropdown-menu li > a', select).click(function () {
				var option = $(this),
					value = option.data('value');
				
				$('.dropdown-menu li a', select).attr("aria-selected", false);
				$('.dropdown-menu li', select).removeClass("active");
				option.parent('li').addClass("active");
				option.attr("aria-selected", true);
				target.val(value);
				label.text(option.text());
				focus.focus();
			});
		});

		// Convert GMT timestamps to client time.
		$('abbr.timestamp').each(function () {
			var $this = $(this),
			  dateTime = Date.parse($this.text()),
			  momentDateTime;

			if (dateTime) {
				momentDateTime = moment(dateTime);
				var dateFormat = dateFormatConverter.convert(element.closest("[data-dateformat]").data("dateformat") || "MMMM d, yyyy", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
				var timeFormat = dateFormatConverter.convert(element.closest("[data-timeformat]").data("timeformat") || "h:mm tt", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
				var datetimeFormat = dateFormatConverter.convert(element.attr('data-format'), dateFormatConverter.dotNet, dateFormatConverter.momentJs) || (dateFormat + ' ' + timeFormat);

				$this.text(momentDateTime.format(datetimeFormat));
				$this.attr("title", momentDateTime.format(datetimeFormat));
			}
		});

		// Format time elements.
		$("time").each(function () {
			var $this = $(this),
			  dateTime = moment($this.attr("datetime"));

			if (dateTime) {
				var dateFormat = dateFormatConverter.convert($this.closest("[data-dateformat]").data("dateformat") || "MMMM d, yyyy", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
				var timeFormat = dateFormatConverter.convert($this.closest("[data-timeformat]").data("timeformat") || "h:mm tt", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
				var datetimeFormat = dateFormat + ' ' + timeFormat;

				$this.text(dateTime.format($this.hasClass("date-only") ? dateFormat : datetimeFormat));
			}
		});

		// Convert GMT date ranges to client time.
		$('.vevent').each(function () {
			var start = $('.dtstart', this),
			  end = $('.dtend', this),
			  startDate = Date.parse(start.text()),
			  endDate = Date.parse(end.text()),
			  dateFormat,
			  timeFormat,
			  datetimeFormat,
			  momentStartDate,
			  momentEndDate;

			if (startDate) {
				momentStartDate = moment(startDate);
				dateFormat = dateFormatConverter.convert(start.closest("[data-dateformat]").data("dateformat") || "MMMM d, yyyy", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
				timeFormat = dateFormatConverter.convert(start.closest("[data-timeformat]").data("timeformat") || "h:mm tt", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
				datetimeFormat = dateFormatConverter.convert(start.attr('data-format'), dateFormatConverter.dotNet, dateFormatConverter.momentJs) || (dateFormat + ' ' + timeFormat);

				start.text(momentStartDate.format(datetimeFormat));
				start.attr('title', momentStartDate.format(datetimeFormat));
			}

			if (endDate) {
				momentEndDate = moment(endDate);
				dateFormat = dateFormatConverter.convert(end.closest("[data-dateformat]").data("dateformat") || "MMMM d, yyyy", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
				timeFormat = dateFormatConverter.convert(end.closest("[data-timeformat]").data("timeformat") || "h:mm tt", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
				datetimeFormat = dateFormatConverter.convert(end.attr('data-format'), dateFormatConverter.dotNet, dateFormatConverter.momentJs) || (dateFormat + ' ' + timeFormat);

				end.text(momentEndDate.format(startDate && momentEndDate.isSame(moment(startDate), 'day') ? timeFormat : datetimeFormat));
				end.attr('title', momentEndDate.format(datetimeFormat));
			}
		});

		// Initialize Bootstrap Carousel for any elements with the .carousel class.
		$('.carousel').carousel();

		// Workaround for jQuery UI and Bootstrap tooltip name conflict
		if ($.ui && $.ui.tooltip) {
			$.widget.bridge('uitooltip', $.ui.tooltip);
		}

		$('.has-tooltip').tooltip();

		prettyPrint();

		// Initialize any shopping cart status displays.
		(function () {
			var shoppingCartStatuses = {};

			$('.shopping-cart-status').each(function () {
				var element = $(this),
				  service = element.attr('data-href'),
				  count = element.find('.count'),
				  countValue = count.find('.value'),
				  serviceQueue;

				if (!service) {
					return;
				}

				serviceQueue = shoppingCartStatuses[service];

				if (!$.isArray(serviceQueue)) {
					serviceQueue = shoppingCartStatuses[service] = [];
				}

				serviceQueue.push(function (data) {
					if (data != null && data.Count && data.Count > 0) {
						countValue.text(data.Count);
						count.addClass('visible');
						element.addClass('visible');
					}
				});
			});

			$.each(shoppingCartStatuses, function (service, queue) {
				$.getJSON(service, function (data) {
					$.each(queue, function (index, fn) {
						fn(data);
					});
				});
			});
		})();

		$('[data-state="sitemap"]').each(function () {
			var $nav = $(this),
			  current = $nav.data('sitemap-current'),
			  ancestor = $nav.data('sitemap-ancestor'),
			  state = $nav.closest('[data-sitemap-state]').data('sitemap-state'),
			  statePath,
			  stateRootKey;

			if (!(state && (current || ancestor))) {
				return;
			}

			statePath = state.split(':');
			stateRootKey = statePath[statePath.length - 1];

			$nav.find('[data-sitemap-node]').each(function () {
				var $node = $(this),
				  key = $node.data('sitemap-node');

				if (!key) {
					return;
				}

				$.each(statePath, function (stateIndex, stateKey) {
					if (stateIndex === 0) {
						if (current && stateKey == key) {
							$node.addClass(current);
						}
					} else {
						if (ancestor && stateKey == key && key != stateRootKey) {
							$node.addClass(ancestor);
						}
					}
				});
			});
		});

		(function () {
			var query = URI ? URI(document.location.href).search(true) || {} : {};

			$('[data-query]').each(function () {
				var $this = $(this),
				  value = query[$this.data('query')];

				if (typeof value === 'undefined') {
					return;
				}

				$this.val(value).change();
			});
		})();
	});

	if (typeof XRM != 'undefined' && XRM) {
		XRM.zindex = 2000;

		(function () {
			var ckeditorConfigs = [XRM.ckeditorSettings, XRM.ckeditorCompactSettings];

			for (var i = 0; i < ckeditorConfigs.length; i++) {
				var config = ckeditorConfigs[i];

				if (!config) continue;

				// Load all page stylesheets into CKEditor, for as close to WYSIWYG as possible.
				var stylesheets = $('head > link[rel="stylesheet"]').map(function (_, e) {
					var href = $(e).attr('href');
					return href.match(/,/) ? null : href;
				}).get();

				stylesheets.push(($('[data-app-path]').data('app-path') || '/') + 'css/ckeditor.css');

				config.contentsCss = stylesheets;
			}

			var styles = CKEDITOR.stylesSet.get('cms');

			if (styles) {
				var newStyles = [
				  { name: window.ResourceManager['CKEditor_Page_Header_Style'], element: 'div', attributes: { 'class': 'page-header' } },
				  { name: window.ResourceManager['CKEditor_Alert_Info_Style'], element: 'div', attributes: { 'class': 'alert alert-info' } },
				  { name: window.ResourceManager['CKEditor_Alert_Success_Style'], element: 'div', attributes: { 'class': 'alert alert-success' } },
				  { name: window.ResourceManager['CKEditor_Alert_Warning_Style'], element: 'div', attributes: { 'class': 'alert alert-warning' } },
				  { name: window.ResourceManager['CKEditor_Alert_Danger_Style'], element: 'div', attributes: { 'class': 'alert alert-danger' } },
				  { name: window.ResourceManager['CKEditor_Well_Style'], element: 'div', attributes: { 'class': 'well' } },
				  { name: window.ResourceManager['CKEditor_Well_Small_Style'], element: 'div', attributes: { 'class': 'well well-sm' } },
				  { name: window.ResourceManager['CKEditor_Well_Large_Style'], element: 'div', attributes: { 'class': 'well well-lg' } },
				  { name: window.ResourceManager['CKEditor_Label_Style'], element: 'span', attributes: { 'class': 'label label-default' } },
				  { name: window.ResourceManager['CKEditor_Label_Info_Style'], element: 'span', attributes: { 'class': 'label label-info' } },
				  { name: window.ResourceManager['CKEditor_Label_Success_Style'], element: 'span', attributes: { 'class': 'label label-success' } },
				  { name: window.ResourceManager['CKEditor_Label_Warning_Style'], element: 'span', attributes: { 'class': 'label label-warning' } },
				  { name: window.ResourceManager['CKEditor_Label_Danger_Style'], element: 'span', attributes: { 'class': 'label label-danger' } }
				];

				for (var i = 0; i < newStyles.length; i++) {
					styles.push(newStyles[i]);
				}
			}

			//temporary fix for bug: http://dev.ckeditor.com/ticket/14591
			$(document).bind('DOMNodeInserted', function () {
				if ($(".cke_dialog_ui_labeled_label").length)
					$(".cke_dialog_ui_labeled_label").attr("title", function () { return $(this).text() });
			});
		})();
	}

	var notification = $.cookie("adx-notification");
	if (typeof (notification) === typeof undefined || notification == null) return;
	displaySuccessAlert(notification, true);
	$.cookie("adx-notification", null);

	function displaySuccessAlert(success, autohide) {
		var $container = $(".notifications");
		if ($container.length == 0) {
			var $pageheader = $(".page-heading");
			if ($pageheader.length == 0) {
				$container = $("<div class='notifications'></div>").prependTo($("#content-container"));
			} else {
				$container = $("<div class='notifications'></div>").appendTo($pageheader);
			}
		}
		$container.find(".notification").slideUp().remove();
		if (typeof success !== typeof undefined && success !== false && success != null && success != '') {
			var $alert = $("<div class='notification alert alert-success success alert-dismissible' role='alert'><button type='button' class='close' data-dismiss='alert' aria-label='Close'><span aria-hidden='true'>&times;</span></button>" + success + "</div>")
                .on('closed.bs.alert', function () {
                	if ($container.find(".notification").length == 0) $container.hide();
                }).prependTo($container);
			$container.show();
			window.scrollTo(0, 0);
			if (autohide) {
				setTimeout(function () {
					$alert.slideUp(100).remove();
					if ($container.find(".notification").length == 0) $container.hide();
				}, 5000);
			}
		}
	}

})(window.jQuery, window.XRM);

$(document).ready(function () {
	// Note: for accessibility, sync focused and selected dropdown item.
	$(".dropdown-menu li a")
		.focus(
			function (e) {
				var $target = $(e.target);
				$target.attr("aria-selected", true);
				$target.parent().toggleClass("active", true);
			});
	// Note: unselect dropdown item that gets unfocused.
	$(".dropdown-menu li a")
	.focusout(
		function (e) {
			var $target = $(e.target);
			$target.attr("aria-selected", false);
			$target.parent().toggleClass("active", false);
		});

	$(".dropdown-submenu").on("click", function (e) {
		// Note: we prevent default button handler of Bootstrap,
		// bacause it closes all other manus except the current one.
		// This is unwanted behavour, bacause we have a nested dropdown menus in the top bar.
		// Default behavour caused parent menu to close when we try to open child menu.

		$(this).toggleClass("open");

		var $searchButton = $(this).find('#search-filter');
		var isExpanded = $searchButton.attr("aria-expanded") === "true";
		$searchButton.attr("aria-expanded", !isExpanded);

		if (!isExpanded) {
			var firstMenuItem = $(this).find(".dropdown-menu li:first-child a");
			firstMenuItem.focus();
		}

		e.stopPropagation();
		e.preventDefault();
	});

	$(".dropdown-menu li a").on("click", function (e) {
		// return focus to dropdown button, when menu item is clicked.
		var dropdownButton = $(this).parent().parent().prev().first();
		dropdownButton.focus();
	});


	// Search button in the top navigation menu is a dropdown that toggles a search form.
	// Search form has a dropdown too -- the Search Filter dropdown.
	// But *Bootstrap* explicitly forbids dropdown logic to be triggered by this combination of elements.
	// See: https://github.com/twbs/bootstrap/blob/v3-dev/js/dropdown.js#L160
	// So, we listen to same event and close `.dropdown-submenu` inside the form.
	$(document).on(
		"click.bs.dropdown.data-api",
		".dropdown form",
		function (e) {
			var $submenu = $(e.target).closest("form").find(".dropdown-submenu");
			$submenu.toggleClass("open", false);

			var $searchButton = $submenu.find("#search-filter");
			$searchButton.attr("aria-expanded", false);
		});
});

$(document).ready(function () {
	$('#profile-dropdown').find('a').each(function () {
		$(this).attr('role', 'button');
	});
	$('li.weblink>a[title="Home"]').attr('aria-label', window.ResourceManager["Home_DefaultText"]);
	$('li.dropdown>a.navbar-icon').attr('title', window.ResourceManager["Search_DefaultText"]);
	$('label.required').next().find('input').each(function () {
		$(this).attr('aria-label', $(this).closest().attr('name'));
		$(this).attr('aria-required', true);
	});
	$('li.dropdown>a.dropdown-toggle').each(function () {
		$(this).attr('aria-label', $(this).attr('title') + window.ResourceManager["DropDown_DefaultText"]);
	});
	$('li.weblink.dropdown>a').each(function () {
		$(this).attr('aria-label', $(this).attr('title') + window.ResourceManager["DropDown_DefaultText"]);
	});
	$('section.modal input[type="file"]').on("keydown", function (event) {
		if (event.shiftKey && event.keyCode == 9) {
			if ($('input[type="file"]').is(":focus")) {
				setTimeout(function () { $('section.modal textarea[name="text"]').focus(); }, 0);
			}
		}
	});
	$('div.list-group>a').each(function () {
		$(this).attr('aria-label', $(this).attr('title'));
	});
});


$(document).ready(function () {
	$('.navbar-nav .weblink.dropdown>a').attr('role', 'button');
	$('.navbar-nav .weblink.dropdown>ul>li>a').attr('role', 'link');
});

$(document).ready(function () {
	$('input#AttachFile').attr('aria-label', window.ResourceManager["FileUpload_Browse_Button_Text"]);
});
$(document).ready(function () {
	var options = $("select>option");
	$.each(options, function (i, op) {
		if ($(op).text() == "") {
			$(op).text($(op).attr("label"))
		}
	});
});

portal.CompositeControl = function (trigger) {
	var self = this;
	self.controlTrigger = trigger;
	self.$popverTriger = null;
	self.contentid = "";
	self.$content = null;
	self.$contentInputs = null;
	self.valueTemplate = "";
	self.isPopoverOpen = false;
	self.isEditable = false;
	self.showOrHidePopover = function() {
		if (self.isPopoverOpen) {
			self.$popverTriger.popover("hide");
		} else {
			self.$popverTriger.popover("show");
		}
	}
	self.updateContent = function() {
		var popeverInputs = $("." + self.contentid).find("input");
		self.$contentInputs.each(function(index, value) {
			var mainInputId = $(value).prop("id");
			popeverInputs.each(function(innerIndex, innnerValue) {
				var $innnerValue = $(innnerValue);
				if ($innnerValue.prop("id") == mainInputId) {
					$(value).attr("value", $innnerValue.val());
				}
			});
		});
		self.updateValue();
	};
	self.updateValue = function () {
		if (!self.isEditable) return;
		var triggerValue = self.valueTemplate.split("{BREAK}");
		self.$contentInputs.each(function (index, value) {
			var inputId = $(value).attr("id");
			var inputVlaue = $('#' + $(value).attr("id")).attr("value");
			for (var i = 0; i < triggerValue.length; i++) {

				triggerValue[i] = triggerValue[i].replace("{" + inputId + "}", inputVlaue).trim();
			}
		});
		self.$popverTriger.attr("value", triggerValue.join("\r\n"));
	};
	self.init = function() {
		self.$popverTriger = $(self.controlTrigger);
		self.contentid = self.$popverTriger.attr("data-content-id");
		self.valueTemplate = self.$popverTriger.attr("data-content-template");
		self.isEditable = self.$popverTriger.attr("data-editable").toLowerCase() === "true";
		self.$popverTriger.click(function() {
			self.showOrHidePopover();
		});
		console.log(self.contentid);
		self.$content = $("#" + self.contentid);
		self.$contentInputs = self.$content.find("input");
		self.$popverTriger.attr("readonly", "true");
		self.updateValue();
		if (self.isEditable) {
			self.$popverTriger.popover({
				trigger: "manual",
				html: true,
				template: '<div class="popover" role="tooltip"><div class="arrow"></div><h3 class="popover-title"></h3><div class="popover-content ' + self.contentid + ' "></div></div>',
				content: function () {
					return self.$content.html();
				},
				placement: function() {
					return "bottom";
				}
			});

			self.$popverTriger.on("inserted.bs.popover", function () {
				var $insertedPopover = $("." + self.contentid);
				$insertedPopover.css("width", "270px");
				$insertedPopover.find("input").removeAttr("name");
				self.isPopoverOpen = true;
			});

			self.$popverTriger.on('hide.bs.popover', function () {
				self.updateContent();
				self.isPopoverOpen = false;
			});

			self.$popverTriger.on("shown.bs.popover",
				function() {
					$("." + self.contentid + " .btn").click(function() {
						self.$popverTriger.popover("hide");
					});
				});
		}

		$('body').on('click', function(e) {
			if (!$(self.$popverTriger).is(e.target)
				&& $('.popover-content').length !== 0
				&& $(e.target).data('toggle') !== 'popover'
				&& $(e.target).parents('[data-toggle="popover"]').length === 0
				&& $(e.target).parents('.popover.in').length === 0) {
				self.$popverTriger.popover("hide");
			}
		});

		$("#" + self.contentid).parent().on("keydown", function (event) {
			//'Enter' key press opens closed composite control and only closes opened control on 'enter' keypress on button
			if (event.keyCode === 13 && (self.isPopoverOpen && event.target.id.indexOf('Button') !== -1 || !self.isPopoverOpen)) {
				self.showOrHidePopover();
			}
			//Hide popover on 'Esc' key press
			if (event.keyCode === 27) {
				self.$popverTriger.popover("hide");
			}
			//Focus is returned to first input field of composite control if user presses "Tab" on the button of composite control 
			if (event.target.id.indexOf('Button') !== -1 && event.keyCode === 9) {
				$(jQuery(this).find('*').filter(':input')[0]).focus();
			}
		});
	};
}

$(document).ready(function () {
	$("*[data-composite-control]")
		.each(function (index, value) {
			var control = new portal.CompositeControl(value);
			control.init();
		});
});

$('body').on("keydown", ".dropdown", function (event) {
	var parentElement = event.srcElement.parentElement.parentElement;
	// Checking that li element is in dropdown menu
	if (event.keyCode === 9 && parentElement !== null && parentElement.className.indexOf("dropdown") !== -1) {
		// Checking if tab was pressed from last element in dropdown
		if ($(parentElement).children().last().attr("class") === "active") {
			$(parentElement).children().first().children().first().focus();
			event.preventDefault();
		}
	}
});

// multilanguage - in dropdown, on item click, update the cookie with the selected item.
$(document).ready(function () {
	try {
		var selectedLanguage = $("html").attr("data-lang");
		if (selectedLanguage) {
			$.cookie("ContextLanguageCode", selectedLanguage, { path: "/" });
		}

		var langOptionsMenu = $(".drop_language").parents().next("ul").last()[0];
		if (langOptionsMenu) {
			var langOptions = $("a", langOptionsMenu);
			$(langOptions).each(function() {
				var code = $(this).attr("data-code");
				if (code) {
					$(this).on("click", function() {
								$.cookie("ContextLanguageCode", code, { path: "/" });
							});
				}
			});
		}
	} catch(ex){
		// do nothing
	}

	return true;
});
