/*	Jquery Plug-in
	Name: Stack Ranking Question
	Description: Creating UI for the stack ranking question on web form tool 
*/
(function ($) {
	$.fn.extend({
		//pass the options variable to the function
		stackRanking: function (options) {
			var defaults = { withPrompt: true };
			var opts = $.extend(defaults, options);
			var confirmed;
			return this.each(function () {
				this.itemNumber = $(this).find("tr").length;
				var theTable = this;
				$(this).find("tr").each(function () {
					$(this).find("div.control").hide();
					//var rowName = $(this).children(".info").attr("id");
					var cells = [];
					for (var i = 1; i <= theTable.itemNumber; i++) {
						cells.push("<div class='sr-cell' col='" + i + "'>" + i + "</div>");
					}
					$(this).find("div.info").after(cells.join(""));
				});
				$(this).find(".sr-cell").click(onClick);
				$(this).addClass("stack-ranking-question");
				loadPreselection(this);
			});
			function loadPreselection(elem) {
				$(elem).find("div.control input").each(function () {
					if ($(this).val() != '') {
						var index = parseInt($(this).val());
						var theCell = $(this).parents("td").first().find(".sr-cell").get(index - 1);
						select(theCell);
					}
				});
			}
			function onClick() {
				confirmed = true;
				removeConflicts(this);
				if (confirmed || !opts.withPrompt) {
					select(this);
					populateValue(this);
				}
				return false;
			}
			function select(elem) {
				$(elem).removeClass("selected-cell");
				$(elem).addClass("selected-cell");

			}
			function removeConflicts(elem) {
				$(elem).parents("table").first().find(".sr-cell[col =" + $(elem).attr("col") + "]").each(function () {
					if ($(this).hasClass("selected-cell")) {
						if (opts.withPrompt) {
							if (confirm("That ranking has already been assigned to another item. \nAre you sure you want to reassign it to this item?")) {
								$(this).removeClass("selected-cell");
								removeValue(this);
								confirmed = true;
							} else
								confirmed = false;
						}
						else {
							$(this).removeClass("selected-cell");
							removeValue(this);
						}
					}
				});
				if (confirmed || !opts.withPrompt) {
					$(elem).parent().find(".sr-cell").each(function () {
						if ($(this).hasClass("selected-cell")) {
							$(this).removeClass("selected-cell");
						}
					});
				}

			}

			function populateValue(elem) {
				$(elem).parent().find("div.control input").val($(elem).html());
			}
			function removeValue(elem) {
				$(elem).parent().find("div.control input").val("");
			}
		}
	});
})(jQuery);