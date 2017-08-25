<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Adxstudio.Xrm.KnowledgeArticles.IKnowledgeArticle>" %>

<fieldset>
    <div class="form-group">
        <div>
            <%= Html.Hidden("maxRating", 5) %>
            <%= Html.Hidden("minRating", 0) %>
			<%= Html.Hidden("ratingUrl", Url.Action("GetArticleRating", "Article", new { id = this.Model.Id })) %>
            <%= Html.Hidden("rating") %>
            <div id="post-rating" class="rateit" data-rateit-resetable="false" data-rateit-step="1" data-rateit-min="0" data-rateit-max="5" data-rateit-backingfld="#rating" onclick="postRating('<%= Url.Action("RatingCreate", "Article",new { id = Model.Id }) %>')"></div>
        </div>
    </div>
</fieldset>

<script type="text/javascript">
	$(function () {
		var ratingUrl = $("#ratingUrl").val();
		$.ajax({
			url: ratingUrl,
			type: 'GET',
			success: function (result) {
				$("#rating").val(result);
			},
			error: function (error) { console.log(error); }
		});

		return false;
	});

	function postRating(url) {
		$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
		var dto = {
			rating: $('#post-rating').rateit('value'),
			maxRating: $('#post-rating').rateit('max'),
			minRating: $('#post-rating').rateit('min')
		}
		shell.ajaxSafePost({
			type: 'POST',
			url: url,
			data: dto,
			success: function (result) {
				$("#article-rating").html(result);
				$('#post-rating').rateit();
				if (((document.getElementById("foundmyanswerbutton") != null) && (document.getElementById("foundmyanswerbutton").style.display != "none")) || ((document.getElementById("foundmyanswerlabel") != null) && (document.getElementById("foundmyanswerlabel").style.display != "none"))) {
					document.getElementById("post-rating").style.marginTop = "13px";
				}
				$.unblockUI();
			},
			error: function () {
				$.unblockUI();
			}
		});
	}
</script>
