/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


(function ($, handlebars) {
	var keyCode =
	{
		spacebar: 32,
		enter: 13,
		up: 38,
		down: 40,
		isFiredSpaceOrEnterEvent: function (e) {
			return e.keyCode === keyCode.spacebar || e.charCode === keyCode.spacebar || e.keyCode === keyCode.enter;
		},
		isFiredSpaceEvent: function (e) {
			return e.keyCode === keyCode.spacebar || e.charCode === keyCode.spacebar;
		}
	};

	var searchResultTags =
	[
		{
			entityLogicalNames: ["adx_communityforum", "adx_communityforumthread", "adx_communityforumpost"],
			cssClass: "label label-default",
			label: ResourceManager.Forums_Label
		},
		{
			entityLogicalNames: ["adx_blog", "adx_blogpost", "adx_blogpostcomment"],
			cssClass: "label label-info",
			label: ResourceManager.Blogs_Label
		},
		{
			entityLogicalNames: ["adx_event", "adx_eventschedule"],
			cssClass: "label label-info",
			label: ResourceManager.Events_Label
		},
		{
			entityLogicalNames: ["adx_idea", "adx_ideaforum"],
			cssClass: "label label-success",
			label: ResourceManager.Ideas_Label
		},
		{
			entityLogicalNames: ["adx_issue"],
			cssClass: "label label-danger",
			label: ResourceManager.Issues_Label
		},
		{
			entityLogicalNames: ["incident"],
			cssClass: "label label-warning",
			label: ResourceManager.Resolved_Cases_Label
		},
		{
			entityLogicalNames: ["kbarticle"],
			cssClass: "label label-info",
			label: ResourceManager.Knowledge_Base_Label
		},
		{
			entityLogicalNames: ["knowledgearticle"],
			cssClass: "label label-primary",
			label: ResourceManager.Knowledge_Base_Label
		}
	];

	handlebars.registerHelper("ifvalue", function (conditional, options) {
		if (options.hash.value === conditional) {
			return options.fn(this);
		} else {
			return options.inverse(this);
		}
	});

	handlebars.registerHelper("stringFormat", function (format) {
		var result =
			// skip last argument: `options` object,
			// skip first argument: format string
			_.rest(_.initial(arguments))
			.reduce(
				function (acc, value, i) {
					return acc.replace("{" + i + "}", value);
				},
				format);

		return result;
	});

	function getUrlParameterByName(name) {
		var url = window.location.href;
		name = name.replace(/[\[\]]/g, "\\$&");
		var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
			results = regex.exec(url);
		if (!results) return null;
		if (!results[2]) return '';
		var val = decodeURIComponent(results[2].replace(/\+/g, " "));

		var result = handlebars.Utils.escapeExpression(val);
		return result;
	}

	function addClickEventForElement(element, handler) {
		element.click(function (e) {
			e.preventDefault();
			e.stopPropagation();
			handler(this);
		});
		element.keypress(function (e) {
			if (keyCode.isFiredSpaceOrEnterEvent(e)) {
				e.preventDefault();
				e.stopPropagation();
				handler(this);
			}
		});
	}

	// # Base component of facetedSearch
	function FacetedSearchComponent(templateItem, insertItem, searchControlData, initialState) {
		var $this = this;

		$this.$templateItem = $(templateItem);
		$this.$insertItem = $(insertItem);
		$this.searchControlData = searchControlData;
		$this.initialState = initialState;
	}

	FacetedSearchComponent.prototype.render = function () {
		var $this = this;
		var compiledFacetViewTemplate = handlebars.compile($this.$templateItem.html());
		$this.$insertItem.html(compiledFacetViewTemplate($this.searchControlData));
		if ($this.applyInitialState != null && $this.initialState != null) {
			$this.applyInitialState();
		}
		if ($this.addOnClickCallback != null) {
			$this.addOnClickCallback();
		}
	}

	// Apply data transforms that sets "All" value as selected if there's no other value selected.
	function _setAllAsActiveTransform(facetViews) {
		if (!facetViews) {
			return;
		}

		facetViews.forEach(function (facet) {
			var activeConstraints = _(facet.facetData).where({ "active": true });
			var hasActive = !!activeConstraints && (activeConstraints.length > 0);

			var isDateFacet = facet.facetName === "modifiedon.date" && !!facet.facetData[0];
			var isRecordType = facet.facetName == "_logicalname" && !!facet.facetData[0];

			if (isDateFacet || isRecordType) {
				// Date and RecordType Facets have an "All" option in data, while other Facets have it in template.

				var noActive = !activeConstraints || activeConstraints.length === 0;
				var otherActive = !!activeConstraints && activeConstraints.length > 1;
				var onlyAllIsActive = !!facet.facetData[0].active && !otherActive;

				facet.facetData[0].active = noActive || onlyAllIsActive;
				facet.facetData[0].first = true;
			} else {
				facet.noActive = !hasActive;
			}
		});
	}

	//Apply data transforms that Rating Facet template expects.
	function _ratingFacetTransforms(facetViews) {
		if (!facetViews) {
			return;
		}

		var templateData = [
			{ skipStars: true },
			{ filledStars: [1, 2, 3, 4], emptyStars: [5], ratingLabel: ResourceManager.Facet_Rating_Four_And_Up },
			{ filledStars: [1, 2, 3], emptyStars: [4, 5], ratingLabel: ResourceManager.Facet_Rating_Three_And_Up },
			{ filledStars: [1, 2], emptyStars: [3, 4, 5], ratingLabel: ResourceManager.Facet_Rating_Two_And_Up },
			{ filledStars: [1], emptyStars: [2, 3, 4, 5], ratingLabel: ResourceManager.Facet_Rating_One_And_Up }
		];

		var facet = _(facetViews).findWhere({ "facetName": "rating" });

		if (!facet) {
			return;
		}

		// Contract with server: data comes in this order: [1+, 2+, 3+, 4+ ].
		// We re-order data for the template.
		facet.facetData = [
			{},// Empty object for All constraint
			facet.facetData[3],
			facet.facetData[2],
			facet.facetData[1],
			facet.facetData[0]
		];

		// set attributes for template from `templateData`.
		templateData.forEach(function (x, index) {
			facet.facetData[index] =
				_.assign(
					facet.facetData[index],
					templateData[index]);
		});
	}

	// # Facet Component
	function FacetComponent(templateItem, insertItem, searchControlData, initialState) {
		var $this = this;
		FacetedSearchComponent.call($this, templateItem, insertItem, searchControlData, initialState);
		$this.calculateSelectedData();

		var facetViews = searchControlData.facetViews;

		_ratingFacetTransforms(facetViews);
		_setAllAsActiveTransform(facetViews);

	};

	FacetComponent.prototype = Object.create(FacetedSearchComponent.prototype);

	FacetComponent.prototype.calculateSelectedData = function () {
		var $this = this;
		if ($this.searchControlData == null || $this.searchControlData.facetViews == null || $this.initialState == null) {
			return;
		}
		$this.initialState.forEach(function (initialFacet) {
			initialFacet.Constraints.forEach(function (activeFacetContraint) {
				var activeFacetName = initialFacet.FacetName;
				$this.searchControlData.facetViews.forEach(function (facetView) {
					facetView.facetData.forEach(function (contraint) {
						if (contraint.name == activeFacetContraint && facetView.facetName == activeFacetName) {
							contraint.active = true;
						}
					});
				});
			});
		});
	}

	FacetComponent.prototype.processFacetConstraintClick = function (elementThis, $this) {
		var $controlItem = $(elementThis);
		var $parents = $controlItem.parents(".facet-view");
		var hasMultiple = $parents.hasClass("facet-view-multiple-select");

		var isAllButton = $controlItem.data("control-value") === "";

		if (!hasMultiple || isAllButton) {
			$parents.find(".active").removeClass("active");
		}

		$controlItem.toggleClass("active");
		var elementValue = $controlItem.attr("data-control-value");
		var elementName = $controlItem.attr("data-facet");
		var focusedElementSelector = "[data-control-value='" + elementValue + "'][data-facet='" + elementName + "']";
		$this.callback({ "facetConstraints": $this.getSelected() }, focusedElementSelector);
	};

	FacetComponent.prototype.addOnClickCallback = function () {
		var $this = this;

		addClickEventForElement($this.$insertItem.find(".control-item"), function (element) { $this.processFacetConstraintClick(element, $this); });

		$this.$insertItem.find(".control-item").keydown(function (event) {
			var $element = $(this);
			var $elementToChange;
			if (event.keyCode === keyCode.up) {
				$elementToChange = $element.prev();
			}
			if (event.keyCode === keyCode.down) {
				$elementToChange = $element.next();
			}
			if ($elementToChange != null && $elementToChange.hasClass("control-item")) {
				$elementToChange.focus();
				return false;
			}
		});
		
		//Add Show More and Show Less buttons
		addClickEventForElement($this.$insertItem.find(".show-more"), function (element) { $(element).parents(".facet-view").removeClass("short-list"); });
		addClickEventForElement($this.$insertItem.find(".show-less"), function (element) { $(element).parents(".facet-view").addClass("short-list"); });
		//Hide show more if no needs
		if ($this.$insertItem.find(".control-item:hidden").length === 0) {
			$this.$insertItem.find(".show-more").hide();
		};

		//Get localized names for active elements
		addClickEventForElement($this.$insertItem.find(".facet-view-multiple-select .show-more"), FacetComponent.prototype.showLocalizedProductLabels);
		FacetComponent.prototype.showLocalizedProductLabels($this.$insertItem.find(".facet-view-multiple-select .show-more"));
	};

	FacetComponent.prototype.showLocalizedProductLabels = function (element) {
		//look for not-localized and visible constraints
		//hide all constraints without localized labels
		var entityGuids = [];
		var $activeFacet = $(element).parents(".facet-view");
		var $elementsWithoutDisplayName = $activeFacet.find(" .control-item[data-control-display-name='']:visible");
		$elementsWithoutDisplayName.each(function() {
			var elementValue = $(this).data("control-value");
			entityGuids.push(elementValue);
			$(this).addClass("hidden");
		});
		if (entityGuids.length === 0) {
			return;
		}
		//freeze buttons before getting results
		$activeFacet.find(".btn").each(function () { $(this).attr("disabled", "true"); });

		//get localized labels for constraints
		shell.ajaxSafePost({
			url: $("body").data("case-deflection-url") + "/GetLocalizedLabels",
			type: "POST",
			dataType: "json",
			data: {
				localizedLabelEntityName: "product",
				localizedLabelField: "name",
				entityGuids: entityGuids
			}
		}).done(function (localizedLabels) {
			// map localized label to contraints without display name
			$elementsWithoutDisplayName.each(function() {
				var $elementToModify = $(this);
				var elementValue = $(this).data("control-value");
				var localizedLabel = localizedLabels[elementValue];
				if (localizedLabel != null) {
					$elementToModify.attr("aria-label", localizedLabel + $elementToModify.attr("aria-label"));
					$elementToModify.attr("data-control-display-name", localizedLabel);
					var label = $elementToModify.find("label");
					label.html(label.html() + localizedLabel);
					$elementToModify.removeClass("hidden");
				}
			});
		}).always(function() {
			//enable show more and show less buttons
			$activeFacet.find(".btn").each(function () { $(this).removeAttr("disabled"); });
		});
	}

	FacetComponent.prototype.getSelected = function () {
		var $this = this;

		var selectedElements = [];
		$this.$insertItem.find(".control-item.active").each(function (i, activeElement) {
			var elementValue = $(activeElement).attr("data-control-value");
			var elementName = $(activeElement).attr("data-facet");

			if (elementValue != null && elementValue != "" && elementName !== "" && elementValue !== "[* TO *]") {
				selectedElements.push({
					"FacetName": elementName, "Constraints": [elementValue]
				});
			}
		});
		return selectedElements;
	}


	// # Search Pagination Component

	// Template needs to be able to display short list of pages to the left and
	// to the right of the current page. This returns a list of pages to display.
	var _unwrappedPages = function (pageCount, pageNumber) {
		var MAX_UNWRAPPED_PAGES = 5;

		var unwrappedPagesCount = Math.max(1, MAX_UNWRAPPED_PAGES - 1);

		var minFrom = pageNumber - Math.floor(unwrappedPagesCount / 2);
		var maxTo = pageNumber + Math.ceil(unwrappedPagesCount / 2);


		var safeFrom = Math.max(minFrom, 1);
		var safeTo = Math.min(maxTo, pageCount);

		var overstepFrom = safeFrom - minFrom;
		var overstepTo = maxTo - safeTo;

		var unwrapFrom = Math.max(safeFrom - overstepTo, 1);
		var unwrapTo = Math.min(safeTo + overstepFrom, pageCount);

		var result = _.range(unwrapFrom, unwrapTo + 1);

		return result;
	};

	function SearchPaginationComponent(templateItem, insertItem, searchControlData, initialState) {
		var $this = this;

		FacetedSearchComponent.call($this, templateItem, insertItem, searchControlData, initialState);

		$this.data = searchControlData;

		$this.data.needsPagination = $this.data.pageCount > 1;

		$this.data.previousPage = $this.data.pageNumber - 1;
		$this.data.nextPage = $this.data.pageNumber + 1;

		$this.data.pageLinks = _unwrappedPages($this.data.pageCount, $this.data.pageNumber);
	};

	SearchPaginationComponent.prototype = Object.create(FacetedSearchComponent.prototype);

	SearchPaginationComponent.prototype.addOnClickCallback = function () {
		var $this = this;

		var clickablePageNumbers = _.union($this.data.pageLinks, [1, $this.data.pageCount]);

		clickablePageNumbers.forEach(function (pageId) {
			addClickEventForElement($this.$insertItem.find(".js-go-to-" + pageId), function () { $this.callback({ "pageNumber": pageId }) });
		});
	}

	SearchPaginationComponent.prototype.getSelected = function () {
		var $this = this;
		return $this._state || $this.data.pageNumber;
	}



	// # Sort Component
	function SortComponent(templateItem, insertItem, searchControlData, initialState) {
		var $this = this;

		FacetedSearchComponent.call($this, templateItem, insertItem, searchControlData, initialState);
	};

	SortComponent.prototype = Object.create(FacetedSearchComponent.prototype);

	SortComponent.prototype.addOnClickCallback = function () {
		var $this = this;
		if ($this.callback != null) {
			$this.$insertItem.find("select").change(function () {
				$this.callback({ "sortOptions": $this.getSelected() });
			});

			//expand drop down by space
			$this.$insertItem.find("select").keydown(function (e) {
				if (keyCode.isFiredSpaceEvent(e)) {
					$(this).focus();
					$(this).find("option").trigger("click");
				}
			});
		}
	}

	SortComponent.prototype.getSelected = function () {
		var $this = this;
		return $this.$insertItem.find(":selected").val();
	}

	SortComponent.prototype.applyInitialState = function () {
		var $this = this;
		$this.$insertItem.find("select").val($this.initialState);
	}

	// SearchResults Component with reset button
	function SearchResultsComponent(templateItem, insertItem, searchControlData, initialState) {
		var $this = this;
		searchControlData.isResetVisible = initialState != null && initialState.length > 0;
		FacetedSearchComponent.call($this, templateItem, insertItem, searchControlData, initialState);
	};

	SearchResultsComponent.prototype = Object.create(FacetedSearchComponent.prototype);

	SearchResultsComponent.prototype.addOnClickCallback = function () {
		var $this = this;
		addClickEventForElement($this.$insertItem.find(".facet-clear-all"), function () { $this.callback({ "facetConstraints": null }); });
	}


	function facetedSearch() {
		var $this = this;
		$this._element = $(".handlebars-search-container");
		$this._searchComponents = [];
		$this._state = {
			pageNumber: 1,
			url: $("body").data("case-deflection-url"),
			query: $this._element.data("query"),
			facetsOrder: $this._element.data("facets-order"),
			logicalNames: getUrlParameterByName("logicalNames"),
			filter: getUrlParameterByName("filter"),
			searchTerm: getUrlParameterByName("q")
		};
	}

	var _reorderSearchData = function (searchData, facetOrder) {
		var reorderedViews =
			facetOrder
			.map(function (facetName) {
				return _(searchData.facetViews).findWhere({ "facetName": facetName });
			})
			.filter(function (x) {
				// filter out empty values
				return x;
			});

		searchData.facetViews = reorderedViews;
	};

	facetedSearch.prototype.init = function () {
		this.updateState();
	};

	facetedSearch.prototype.initSearchComponents = function () {

		var compiledBodyTemplate = handlebars.compile($("#facets-view-body-container").html());
		$(".search-body-container").html(compiledBodyTemplate(this.searchData));

		// Note: jQuery's `map` function returns custom object, not array. So we have to use underscore `map`.
		var facetOrder = _($(".js-facet-order-definition").children()).map(function (x) {
			return $(x).text();
		});

		if (!_.isEmpty(facetOrder)) {
			_reorderSearchData(this.searchData, facetOrder);
		}

		var facetControl = new FacetComponent("#facets-view-results", ".facets", this.searchData, this._state.facetConstraints);
		facetControl.callback = this.setState.bind(this);
		this._searchComponents.push(facetControl);

		var sortComponent = new SortComponent("#search-order-select", ".search-order", this.searchData, this._state.sortOptions);
		sortComponent.callback = this.setState.bind(this);
		this._searchComponents.push(sortComponent);

		var paginationComponent = new SearchPaginationComponent("#facets-view-pagination", ".search-pagination", this.searchData);
		paginationComponent.callback = this.setState.bind(this);
		this._searchComponents.push(paginationComponent);

		var searchResultsComponent = new SearchResultsComponent("#search-view-results", ".search-results", this.searchData, this._state.facetConstraints);
		searchResultsComponent.callback = this.setState.bind(this);
		this._searchComponents.push(searchResultsComponent);
	};

	facetedSearch.prototype.breadCrumbs = function (searchTerm) {
		$("ul.breadcrumb > li.active").removeClass("active");
		$("#searchterm").remove();
		var searchTermString = searchTerm || "";
		var searchTermElement =
			$("<li id='searchterm' >")
			.html("<span class='searchterm'>" + searchTermString + "</span>")
			.addClass("active");
		$("ul.breadcrumb li:last").after(searchTermElement);
	};

	facetedSearch.prototype.render = function () {
		this._searchComponents.forEach(function (controlElement) {
			controlElement.render();
		});
	};

	facetedSearch.prototype.mapResponseData = function (jsonResult) {
		var $this = this;
		jsonResult.firstResultNumber = (jsonResult.pageNumber - 1) * (jsonResult.pageSize) + 1;
		jsonResult.lastResultNumber = Math.min((jsonResult.pageNumber) * (jsonResult.pageSize), jsonResult.itemCount);
		jsonResult.items.forEach(function (item) {
			item.tags =
				searchResultTags
				.filter(function (x) {
					return _.contains(x.entityLogicalNames, item.entityLogicalName);
				});
		});
		jsonResult.query = $this._state.searchTerm;
		return jsonResult;
	}

	facetedSearch.prototype.setState = function (updatedStateElements, focusedElementSelector) {
		var $this = this;

		var hasFacetChanged = !!updatedStateElements.facetConstraints;

		if (hasFacetChanged) {
			updatedStateElements.pageNumber = 1;
		}

		$this._state = _.assign($this._state, updatedStateElements);

		$this.updateState(focusedElementSelector);
	}



	facetedSearch.prototype.updateState = function (focusedElementSelector) {
		var $this = this;

		$this._element.find(".js-search-body").hide();
		$this._element.find(".loader").show();

		var data = {};
		data.query = $this._state.query;
		data.parameters = { "Query": $this._state.searchTerm };
		data.logicalNames = $this._state.logicalNames;
		data.facetConstraints = $this._state.facetConstraints;
		data.sortingOption = $this._state.sortOptions;
		data.pageNumber = $this._state.pageNumber;
		data.filter = $this._state.filter;
		var jsonData = JSON.stringify(data);
		shell.ajaxSafePost({
			type: "POST",
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			url: $this._state.url,
			data: jsonData,
			global: false
		}).done(function (resultData) {
			$this.breadCrumbs($this._state.searchTerm);
			$this.searchData = $this.mapResponseData(resultData);
			$this.initSearchComponents();
			$this.render();
			$this._element.find(".js-search-body").show();
			if (focusedElementSelector != null) {
				if ($(focusedElementSelector).find("input").length > 0) {
					$(focusedElementSelector).find("input").focus();
				} else {
					$(focusedElementSelector).focus();
				}
			}
		}).always(function (jqXHR, textStatus) {
			$this._element.find(".loader").hide();
		});
	};
	window.FacetedSearch = facetedSearch;

}(jQuery, Handlebars));
