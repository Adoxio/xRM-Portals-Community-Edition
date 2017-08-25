/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

/*
# Case Deflection widget

<div class="case-deflection">
	<input type="text" class="form-control subject case-deflection" data-container=".case-deflection" data-target="#case-deflection-topics" data-template="#case-deflection-results" data-itemtemplate="#case-deflection-results" data-pagesize="5" data-logicalnames="adx_issue,adx_webpage,adx_communityforumthread,adx_communityforumpost,adx_blogpost,kbarticle" data-query="" data-filter="" data-noresultstext="No information matching your query was found." placeholder="e.g. User login is failing" />
	<ul id="case-deflection-topics" class="list-group"></ul>
	<a class="btn btn-default search-more pull-right"><span class='fa fa-plus'></span> Show More...</a>
</div>

{% raw %}
<script id="case-deflection-results" type="text/x-handlebars-template">
	{{# each items}}
	<li class="list-group-item">
		<h4 class="list-group-item-heading">
			<a href="{{ url }}">{{ title }}</a>
		</h4>
		<p class="list-group-item-text search-results fragment">
			{{{ fragment }}}
		</p>
		<div class="content-metadata">
			<a href="{{ url }}">{{ absoluteUrl }}</a>
		</div>
	</li>
	{{/each}}
</script>
{% endraw %}

For usage on an Entity Form, create an Entity Form Metadata record for a single line of text attribute and set the 'CSS Class' field value to 'case-deflection'. You must include a Handlebars template in embedded in the page such as in a web template to be used to render the results.

{% raw %}
<script id="case-deflection-container" type="text/x-handlebars-template">
	{{#if items}}
	<div class="panel panel-default">
		<div class="panel-heading">
			<div class="panel-title">{% endraw %}{{ resx['Case_Deflection_Suggested_Topics'] }}{% raw %}</div>
		</div>
		<ul id="case-deflection-topics" class="list-group">
		</ul>
		<div class="panel-footer clearfix">
			<button type="button" class="btn btn-default search-more pull-right"><span class='fa fa-plus'></span> {% endraw %}{{ resx['CustomerService_Support_ShowMore'] }}{% raw %}</button>
			<a href="#" class="btn btn-success pull-left deflect"><span class='fa fa-check'></span> {% endraw %}{{ resx['Found_My_Answer'] }}{% raw %}</a>
		</div>
	</div>
	{{/if}}
</script>
<script id="case-deflection-results" type="text/x-handlebars-template">
	{{# each items}}
	<li class="list-group-item">
		<h4 class="list-group-item-heading"><a href="{{ url }}">{{ title }}</a></h4>
		<p class="list-group-item-text search-results fragment">{{{ fragment }}}</p>
		<div>
			{{#label entityLogicalName 'adx_communityforum'}}
			<span class='label label-info'>{% endraw %}{{ resx['Forums_Label']}}{% raw %}</span>
			{{/label}}
			{{#label entityLogicalName 'adx_communityforumthread'}}
			<span class='label label-info'>{% endraw %}{{ resx['Forums_Label'] }}{% raw %}</span>
			{{/label}}
			{{#label entityLogicalName 'adx_communityforumpost'}}
			<span class='label label-info'>{% endraw %}{{ resx['Forums_Label'] }}{% raw %}</span>
			{{/label}}
			{{#label entityLogicalName 'adx_event'}}
			<span class='label label-info'>{% endraw %} {{ resx['Events_Label'] }} {% raw %}</span>
			{{/label}}
			{{#label entityLogicalName 'adx_eventschedule'}}
			<span class='label label-info'>{% endraw %} {{ resx['Events_Label'] }} {% raw %}</span>
			{{/label}}
			{{#label entityLogicalName 'adx_issue'}}
			<span class='label label-danger'>{% endraw %} {{ resx['Issues_Label'] }} {% raw %}</span>
			{{/label}}
			{{#label entityLogicalName 'incident'}}
			<span class='label label-success'>{% endraw %} {{ resx['Resolved_Cases_Label'] }} {% raw %}</span>
			{{/label}}
			{{#label entityLogicalName 'kbarticle'}}
			<span class='label label-primary'>{% endraw %} {{ resx['Knowledge_Base_Label'] }} {% raw %}</span>
			{{/label}}
			{{#label entityLogicalName 'knowledgearticle'}}
			<span class='label label-primary'>{% endraw %} {{ resx['Knowledge_Base_Label']}} {% raw %}</span>
			{{/label}}
		</div> 
	</li>
	{{/each}}
</script>
{% endraw %}

*/

(function ($, handlebars) {
	'use strict';

	handlebars.registerHelper('urlquerystringcheck', function (url, options) {
	    var parts = (url || '').split('?');

	    if (parts.length <= 1) {
	        return options.fn(this);
	    }
	    else { return options.inverse(this); }
	});

	var requiredInputLength = 4;

	handlebars.registerHelper('label', function (a, b, options) {
		if (a === b)
			return options.fn(this);
		else
			return options.inverse(this);
	});

	function caseDeflection(element) {
		this._element = $(element);
		this._url = this._element.data('url') || $("body").data("case-deflection-url");
		this._container = this._element.data('container');
		this._target = this._element.data('target');
		this._itemTarget = this._element.data('itemtarget');
		this._template = this._element.data('template');
		this._itemTemplate = this._element.data('itemtemplate');
		this._logicalNames = this._element.closest("[data-case-deflection-logicalnames]").data("case-deflection-logicalnames");
		this._filter = this._element.closest("[data-case-deflection-filter]").data("case-deflection-filter");
		this._query = this._element.closest("[data-case-deflection-query]").data("case-deflection-query");
		this._noResultsText = this._element.data('noresultstext');
		this._$container = null;
		this._compiledTemplate = null;
		this._compiledItemTemplate = null;

		if (typeof this._query === typeof undefined || this._query == null || this._query === '') {
			this._query = "(+(@subject))";
		}
	}

	$(document).ready(function () {
		$("input.case-deflection").each(function () {
			new caseDeflection($(this)).init();
		});
	});

	caseDeflection.prototype.init = function() {
		var $this = this,
			$element = $this._element,
			$container;

		if ($this._element.length === 0) {
			return;
		}

		if (!$this._url) {
			return;
		}

		if (!$this._container) {
			$container = $element.parent();
		} else {
			$container = $element.parents($this._container);
		}

		$this._$container = $container;

		if (!$this._template) {
			if ($("#case-deflection-container").length === 0) {
				return;
			}
			$this._compiledTemplate = handlebars.compile($("#case-deflection-container").html());
		} else {
			if ($($this._template).length === 0) {
				return;
			}
			$this._compiledTemplate = handlebars.compile($($this._template).html());
		}

		if (!$this._itemTemplate) {
			if ($("#case-deflection-results").length === 0) {
				return;
			}
			$this._compiledItemTemplate = handlebars.compile($("#case-deflection-results").html());
		} else {
			if ($($this._itemTemplate).length === 0) {
				return;
			}
			$this._compiledItemTemplate = handlebars.compile($($this._itemTemplate).html());
		}

		$element.off("keyup.adx.case-deflection").on("keyup.adx.case-deflection", _.debounce(function (e) {
			e.stopPropagation();
			$this.clearResults();
			$this.getResults();
		}, 800));

		var $clear = $container.find(".search-clear"),
			$apply = $container.find(".search-apply");

		$clear.off("click.adx.case-deflection").on("click.adx.case-deflection", function(e) {
			e.preventDefault();
			$this.clearSearch();
			$this.clearResults();
		});

		$apply.off("click.adx.case-deflection").on("click.adx.case-deflection", function(e) {
			e.preventDefault();
			$this.clearResults();
			$this.getResults();
		});
	}
	
	caseDeflection.prototype.clearSearch = function () {
		var $this = this,
			$element = $this._element;
		$element.val('');
	}

	caseDeflection.prototype.clearResults = function () {
		var $this = this,
			$element = $this._element,
			$container = $this._$container;
		var $more = $container.find(".search-more");
		if (!$this._target) {
			$element.parent().find(".results").animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide'}, 'normal', 'linear', function () { $(this).empty();});
		} else {
			$more.parent(".panel-footer").animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide' }, 'fast', 'linear');
			$($this._target).animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide' }, 'normal', 'linear', function () { $(this).empty(); });
		}
		$more.hide();
		$element.data("page", null);
	}

	caseDeflection.prototype.getResults = function () {
		var $this = this;
		$this.getSearchResults();
	}

	caseDeflection.prototype.getMoreResults = function () {
		var $this = this,
			$element = $this._element,
			page = $element.data("page");
		if (page != null) {
			page = page + 1;
		}
		$this.getSearchResults(page);
	}

	caseDeflection.prototype.getSearchResults = function (page) {
		var $this = this;
		var $element = $this._element;
		var $container = $this._$container;
		var value = $element.val();
		var $more = $container.find(".search-more"),
			$target,
			$itemTarget;

		if (!(value)) {
			if (!$this._target) {
				$element.parent().find(".results").stop(true, true).animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide' }, 'normal', 'linear', function () { $(this).empty(); });
			} else {
				$more.parent(".panel-footer").stop(true, true).animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide' }, 'normal', 'linear');
				$($this._target).stop(true, true).animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide' }, 'normal', 'linear', function () { $(this).empty(); });
			}
			$more.hide();
			$element.data("page", null);
			return;
		}
		
		if (value.length < requiredInputLength) return;

		$more.prop('disabled', true);
		$more.append("<span class='fa fa-spinner fa-spin'></span>");

		if (!$this._target) {
			if (!page) {
				$more.parent(".panel-footer").stop(true, true).animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide' }, 'normal', 'linear');
				$more.hide();
				if ($element.parent().find(".results").length === 1) {
					$target = $element.parent().find(".results");
					$target.stop(true, true).animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide' }, 'fast', 'linear', function () {
						$(this).empty();
					});
				} else {
					$target = $("<div class='results pull-left' style='margin-top: 20px; width: 100%;'></div>");
					$target.hide();
					$element.parent().append($target);
				}
			} else {
				$target = $element.parent().find(".results");
			}
		} else {
			if (!page) {
				$target = $($this._target);
				$more.parent(".panel-footer").stop(true, true).animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide' }, 'normal', 'linear');
				$more.hide();
				$target.stop(true, true).animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide' }, 'fast', 'linear', function () {
					$(this).empty();
				});
			} else {
				$target = $($this._target);
			}
		}

		var $loading = $container.find(".case-deflection-loading");

		if ($loading.length === 0) {
			$loading = $("<div class='case-deflection-loading' style='text-align:center;margin-bottom:10px;margin-top:10px;width:100%;'><span class='fa fa-spinner fa-spin' aria-hidden='true'></span></div>").hide();
			if (!$this._target) {
				$loading.addClass("pull-left");
				$target.before($loading);
			} else {
				$target.after($loading);
			}
		}

		if (!page && $this._target) {
			$loading.css("margin-top", 0);
		} else {
			$loading.css("margin-top", "10px");
		}

		if (!page) {
			$loading.animate({ opacity: 'show', margin: 'show', padding: 'show', height: 'show' }, 'normal', 'linear');
		}

		var pageNumber = page || 1;
		var pageSize = $element.closest("[data-case-deflection-pagesize]").data("case-deflection-pagesize");
		if (pageSize == null || pageSize === '') {
			pageSize = 5;
		}

		var data = {};
		data.parameters = { "subject": value };
		data.query = $this._query;
		data.logicalNames = $this._logicalNames;
		data.filter = $this._filter;
		data.pageNumber = pageNumber;
		data.pageSize = pageSize;
		var jsonData = JSON.stringify(data);

		shell.ajaxSafePost({
			type: 'POST',
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			url: $this._url,
			data: jsonData,
			global: false
		}).done(function (result) {
			if (result == null || result.itemCount === 0) {
				if (!page) {
					if (!$this._target) {
						$more.parent(".panel-footer").animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide' }, 'fast', 'linear');
					} else {
						if (typeof $this._noResultsText != typeof undefined && $this._noResultsText != null && $this._noResultsText !== '') {
							var $noresultstext = $("<li class='list-group-item noresults'></li>").text($this._noResultsText);
							$target.html($noresultstext);
							$target.stop(true, true).animate({ opacity: 'show', margin: 'show', padding: 'show', height: 'show' }, 'normal', 'linear');
						}
					}
				}
				$more.hide();
				$more.prop('disabled', false);
				$more.find(".fa-spinner").remove();
				return;
			}
			// adding the search query to result which is used to append to the url of the entity.
			result.caseTitle = value;

			if (!page) {
				$target.html($this._compiledTemplate(result));
			}

			if (!$this._itemTarget) {
				$itemTarget = $container.find("#case-deflection-topics");
			} else {
				$itemTarget = $($this._itemTarget);
			}

			if ($itemTarget.length === 0) {
				$itemTarget = $target;
			}

			if (page) {
				$itemTarget.append($this._compiledItemTemplate(result));
			} else {
				$itemTarget.html($this._compiledItemTemplate(result));
				$target.stop(true, true).animate({ opacity: 'show', margin: 'show', padding: 'show', height: 'show' }, 'normal', 'linear');
			}

			$more = $container.find(".search-more");

			if (result.pageCount <= 1 || result.pageNumber === result.pageCount) {
				if ($this._target) {
					$more.parent(".panel-footer").stop(true, true).animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide' }, 'normal', 'linear');
				}
				$more.hide();
			} else {
				$element.data("page", result.pageNumber);
				$more.off("click.adx.case-deflection").on("click.adx.case-deflection", function (e) {
					e.preventDefault();
					$this.getMoreResults();
				});
				$more.show();
				$more.parent(".panel-footer").stop(true, true).animate({ opacity: 'show', margin: 'show', padding: 'show', height: 'show' }, 'normal', 'linear');
			}

			$more.prop('disabled', false);
			$more.find(".fa-spinner").remove();

			$container.find(".deflect").off("click.adx.case-deflection").on("click.adx.case-deflection", function(e) {
				e.preventDefault();
				if (window.parent) {
					window.parent.location.replace("/");
				} else {
					window.location.replace("/");
				}
			});

			if (!page) {
				$('html, body').animate({
					scrollTop: (0)
				}, 200);
			}
		}).always(function() {
			$loading.stop(true, true).animate({ opacity: 'hide', margin: 'hide', padding: 'hide', height: 'hide' }, 'fast', 'linear');
		});
	}
}(jQuery, Handlebars));
