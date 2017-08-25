<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Site.Areas.Products.ViewModels.ProductImageGalleryViewModel>" %>
<% if (Model.ImageGalleryNodes.Count > 0) { %>
	<div id="gallery">
		<ul id="carousel" class="elastislide-list">
			<% foreach (var node in Model.ImageGalleryNodes) { %>
				<li data-preview="<%= node.ThumbnailImageURL %>" data-full="<%= node.ImageURL %>">
					<a href="#" title="<%= node.Title %>">
						<img class="thumbnail" src="<%= node.ThumbnailImageURL %>" alt="<%= node.Title %>" />
					</a>
				</li>
			<% } %>
		</ul>
	</div>
<% } %>
<script type="text/javascript">
(function($) {
	$(document).ready(function() {
		var current = 0,
		$preview = $('#product-image'),
		$previewLink = $("#product-image-link"),
		$carouselEl = $('#carousel'),
		$carouselItems = $carouselEl.children(),
		carousel = $carouselEl.elastislide({
			current: current,
			minItems: 3,
			onClick: function(el, pos, evt) {
				changeImage(el, pos);
				evt.preventDefault();
			},
			onReady: function() {
				changeImage($carouselItems.eq(current), current);
			}
		});

		function changeImage(el, pos) {
			$preview.attr('src', el.data('preview'));
			$previewLink.attr('href', el.data('full'));
			$carouselItems.removeClass('current-img');
			el.addClass('current-img');
			carousel.setCurrent(pos);
		}
	});
}(jQuery));
</script>