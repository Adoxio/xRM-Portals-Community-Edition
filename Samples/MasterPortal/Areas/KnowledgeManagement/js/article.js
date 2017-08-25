$(document).ready(function () {
	function showMore(context,e) {
		e.stopPropagation();
		e.preventDefault();
		var elementId = $("#" + context.id).attr("data-parent");
		var items = elementId === "#RelatedArticles" ? $(elementId).find("a.list-group-item") : $(elementId).find("li.list-group-item");
		items.show();
		$("#" + context.id).hide();
		var $nextItemId = "#" + context.nextElementSibling.id;
		$($nextItemId).show();
		$($nextItemId).css("display", "block");
		$($nextItemId).focus();
	}

	function showLess(context, e) {
		e.stopPropagation();
		e.preventDefault();
		var elementId = $("#" + context.id).attr("data-parent");
		var items = elementId === "#RelatedArticles" ? $(elementId).find("a.list-group-item") : $(elementId).find("li.list-group-item");
		var itemsToHide = items.filter(function (index) {
			return index > 4;
		});
		itemsToHide.hide();
		$("#" + context.id).hide();
		var $prevItemId = "#" + context.previousElementSibling.id;
		$($prevItemId).show();
		$($prevItemId).focus();
	}

	$("#showMoreNotesButton").click(function (e) {
		showMore(this, e);
	});

	$("#showLessNotesButton").click(function (e) {
		showLess(this, e);
	});

	$("#showMoreArticleButton").click(function(e) {
		showMore(this, e);
	});

	$("#showLessArticleButton").click(function(e) {
		showLess(this, e);
	});
});