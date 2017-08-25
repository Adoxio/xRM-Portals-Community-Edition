<%@ Page Language="C#" MasterPageFile="~/Areas/Products/Views/Shared/Products.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Products.ViewModels.ProductViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Register TagPrefix="user" TagName="ReviewTemplate" Src="ReviewTemplate.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="Title"><%: Model.Product.Name %></asp:Content>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Commerce/css/commerce.css") %>" />
	<link rel="stylesheet" href="<%: Url.Content("~/js/prettyPhoto/css/prettyPhoto.css") %>" />
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Products/css/elastislide.css") %>" />
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="Scripts">
	<script type="text/javascript" src="<%: Url.Content("~/Areas/Commerce/js/commerce.js") %>"></script>
	<script type="text/javascript" src="<%: Url.Content("~/js/prettyPhoto/jquery.prettyPhoto.min.js") %>"></script>
	<script type="text/javascript" src="<%: Url.Content("~/Areas/Products/js/modernizr.custom.17475.js") %>"></script>
	<script type="text/javascript" src="<%: Url.Content("~/Areas/Products/js/jquery.elastislide.js") %>"></script>
	<script type="text/javascript" src="<%: Url.Content("~/js/jquery.tmpl.min.js") %>"></script>
	<script type="text/javascript" src="<%: Url.Content("~/js/json2.js") %>"></script>
	<script type="text/javascript" src="<%: Url.Content("~/js/jquery.unobtrusive-ajax.min.js") %>"></script>
	<script type="text/javascript" src="<%: Url.Content("~/js/jquery.blockUI.js") %>"></script>
	<script type="text/javascript" src="<%: Url.Content("~/js/jquery.bootstrap-pagination.js") %>"></script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="PageHeader">
	<ul class="breadcrumb">
		<% foreach (var node in Html.SiteMapPath()) { %>
			<% if (node.Item2 == SiteMapNodeType.Current) { %>
				<li class="active"><%: Model.Product.Name %></li>
			<% } else { %>
				<li>
					<a href="<%: node.Item1.Url %>" title="<%: node.Item1.Title %>"><%: node.Item1.Title %></a>
				</li>
			<% } %>
		<% } %>
	</ul>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="MainContent">
	<script id="reviewTemplate" type="text/html">
		<user:ReviewTemplate runat="server" />
	</script>
	<input type="hidden" id="productid" value="<%= Model.Product.Entity.Id %>"/>
	
	<div class="row">
		<div id="product-image-box" class="col-sm-4 text-center">
			<% if (!string.IsNullOrWhiteSpace(Model.Product.ImageURL)) { %>
				<a id="product-image-link" href="<%: Model.Product.ImageURL %>" class="thumbnail" title="<%: string.Format("{0} image", Model.Product.Name) %>"">
					<img id="product-image" src="<%: Model.Product.ImageThumbnailURL %>" alt="<%= string.Format("{0} image", Model.Product.Name) %>" />
				</a>
			<% } else { %>
				<img src="/image-not-available-300x300.png/" alt="[Product image not available]" class="thumbnail" />
			<% } %>
			<div id="product-image-gallery">
				<% Html.RenderPartial("ProductImageGallery", Model.ImageGalleryNodes); %>
			</div>
		</div>
		<div id="product-info" class="col-sm-8">
			<div id="product-name">
				<h1><%: Model.Product.Name %></h1>
			</div>
			<div id="product-extended-info">
				<small>SKU: <%= Model.Product.SKU %></small>
				<div id="product-rating">
					<div data-rateit-readonly="true" data-rateit-ispreset="true" data-rateit-value="<%= Model.Product.RatingInfo.Average %>" class="rateit"></div>
					<span class="rating-info">
						<% if (Model.Product.RatingInfo.Count > 0) { %>
							<span>(<%= Model.Product.RatingInfo.Count %>
							<% if (Model.Product.RatingInfo.Count == 1) { %>
								&nbsp;Review)</span>
							<% } else { %>
								&nbsp;Reviews)</span>
							<% } %>
							<% if (Model.Product.CurrentUserCanWriteReview && !Model.UserHasReviewed) { %>
								<a id="write-review-link" href="#create-review" title="<%: Html.SnippetLiteral("Product Reviews Create Review Link Text", ResourceManager.GetString("Write_A_Review")) %>"><%: Html.SnippetLiteral("Product Reviews Create Review Link Text", ResourceManager.GetString("Write_A_Review")) %></a>
							<% } %>
						<% } else { %>
							<% if (Model.Product.CurrentUserCanWriteReview && !Model.UserHasReviewed) { %>
								<span id="first-write-review"><%: Html.SnippetLiteral("Product Reviews Be First Text", ResourceManager.GetString("Be_The_First_To")) %> <a id="write-review-link" href="#create-review" title="<%: Html.SnippetLiteral("Product Reviews Create Review Link Text", ResourceManager.GetString("Write_A_Review")) %>"><%: Html.SnippetLiteral("Product Reviews Create Review Link Text", ResourceManager.GetString("Write_A_Review")) %></a></span>
							<% } %>
						<% } %>
					</span>
				</div>
			</div>
			<div id="product-shop" class="well">
				<h2 class="price pull-left"><%= Model.Product.CurrentPrice.ToString("C2") %></h2>
				<div id="add-to-cart-box" class="pull-right">
					<div class="input-group">
						<label for="productQuantity" class="input-group-addon">Qty</label>
						<input id="productQuantity" type="text" class="form-control" value="1" runat="server" ClientIDMode="Static" />
						<div class="input-group-btn">
							<a id="addProductToCart" href="<%: Url.Action("AddProductToCart", "Products", new { productid = Model.Product.Entity.Id, quantity = productQuantity.Value, area = "Products" }) %>" data-action-url="<%: Url.Action("AddProductToCart", "Products", new { productid = Model.Product.Entity.Id }) %>" class="btn btn-primary" title="<%= Html.SnippetLiteral("Shopping Cart/Add To Cart Button Text", ResourceManager.GetString("Add_To_Cart")) %>"><span class="fa fa-shopping-cart" aria-hidden="true"></span> <%= Html.SnippetLiteral("Shopping Cart/Add To Cart Button Text", ResourceManager.GetString("Add_To_Cart")) %></a>
						</div>
					</div>
				</div>
			</div>
			<div id="social">
				<%: Html.HtmlSnippet("Product Social Share Widget Code") %>
			</div>
		</div>
	</div>
	<div id="product-details">
		<ul class="toolbar-nav nav nav-tabs">
			<li class="active">
				<a href="#description" data-toggle="tab" title="<%: Html.SnippetLiteral("Product Description Title Text", ResourceManager.GetString("Description")) %>"><%: Html.SnippetLiteral("Product Description Title Text", ResourceManager.GetString("Description")) %></a>
			</li>
			<li>
				<a href="#specifications" data-toggle="tab" title="<%: Html.SnippetLiteral("Product Specifications Title Text", ResourceManager.GetString("Specifications")) %>"><%: Html.SnippetLiteral("Product Specifications Title Text",ResourceManager.GetString("Specifications")) %></a>
			</li>
			<li>
				<a href="#reviews" data-toggle="tab" title='<%: Html.SnippetLiteral("Product Reviews Title Text", ResourceManager.GetString("Customer_Reviews")) %>'><%: Html.SnippetLiteral("Product Reviews Title Text", ResourceManager.GetString("Customer_Reviews")) %>
					<% if (Model.Product.RatingInfo.Count > 0) { %>
						&nbsp;(<%= Model.Product.RatingInfo.Count %>)
					<% } %>
				</a>
			</li>
			<li id="reviews-sort" class="pull-right dropdown" style="display:none;" data-current-orderby="SubmittedOn DESC">
				<a class="dropdown-toggle" data-toggle="dropdown" href="#" title="Date - Newest First">
					<span class="fa fa-list" aria-hidden="true"></span>
					<span>Date - Newest First</span>
					&nbsp;<b class="caret"></b>
				</a>
				<ul class="dropdown-menu">
					<li><a href="#" data-orderby="SubmittedOn DESC" title="Date - Newest First">Date - Newest First</a></li>
					<li><a href="#" data-orderby="SubmittedOn ASC" title="Date - Oldest First">Date - Oldest First</a></li>
					<li><a href="#" data-orderby="Rating ASC" title="Rating - Low to High">Rating - Low to High</a></li>
					<li><a href="#" data-orderby="Rating DESC" title="Rating - High to Low">Rating - High to Low</a></li>
				</ul>
			</li>
		</ul>
		<div class="tab-content">
			<div class="tab-pane active" id="description">
				<%= Model.Product.Description %>
			</div>
			<div class="tab-pane" id="specifications">
				<%= Model.Product.Specifications %>
			</div>
			<div class="tab-pane" id="reviews">
				<div id="create-review-box">
					<div id="create-review-success-message" class="alert alert-block alert-success" style="display: none;">
						<a href="#" class="close" data-dismiss="alert" title="Close">&times;</a>
						<p><%: Html.SnippetLiteral("Product Review Create Success Message", ResourceManager.GetString("Review_Has_Been_Submitted")) %></p>
					</div>
					<div id="create-review-error-message" class="alert alert-block alert-danger" style="display: none;">
						<a href="#" class="close" data-dismiss="alert" title="Close">&times;</a>
						<p><%: Html.SnippetLiteral("Product Review Create Error Message", ResourceManager.GetString("Problem_Submitting_Your_Review")) %></p>
					</div>
					<% if (Model.Product.CurrentUserCanWriteReview && !Model.UserHasReviewed) { %>
						<a id="write-review" class="btn btn-primary" href="#create-review" title="<%: Html.SnippetLiteral("Product Reviews Create Review Button Text", ResourceManager.GetString("Write_A_Review")) %>"><%: Html.SnippetLiteral("Product Reviews Create Review Button Text", ResourceManager.GetString("Write_A_Review")) %></a>
						<div id="create-review" style="display:none;">
							<% Html.RenderPartial("CreateReview", Model.Product); %>
						</div>
					<% } %>
				</div>
				<div id="reviews-content" style="display:none;"></div>
				<div id="reviews-none" class='alert alert-block alert-info' style="display:none;"><%: Html.SnippetLiteral("Product Reviews No Reviews Text", ResourceManager.GetString("No_Reviews_On_This_Product")) %></div>
				<div id="reviews-pagination" data-pages="1" data-pagesize="<%: Html.SnippetLiteral("Product Reviews Page Size", "10") %>" data-current-page="1"></div>
				<div id="reviews-loading" class="text-center">
					<img src="~/xrm-adx/samples/images/ajax-loader.gif" alt="loading..." />
				</div>
			</div>
		</div>
		<section class="modal" id="report-abuse" data-id="" tabindex="-1" role="dialog" aria-labelledby="report-abuse-modal-label" aria-hidden="true">
			<div class="modal-dialog">
				<div class="modal-content">
					<div class="modal-header">
						<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
						<h1 id="report-abuse-modal-label" class="modal-title h4">
							<%: Html.SnippetLiteral("Product Review Report Abuse Title Text", ResourceManager.GetString("Report_This_Content_As_Inappropriate")) %>
						</h1>
					</div>
					<div class="modal-body">
						<div class="form-group">
							<%: Html.SnippetLiteral("Product Review Report Abuse Message Text", ResourceManager.GetString("Clicking_The_Button_Below")) %>
						</div>
						<div class="form-group">
							<label for="abuse-reason">
								<%: Html.SnippetLiteral("Product Review Report Abuse Reason Label Text", ResourceManager.GetString("Enter_Reason_You_Find_Content_Inappropriate")) %>
							</label>
							<%= Html.TextArea("abuse-reason", string.Empty, new {@class = "form-control", @rows = "2", @maxlength = "300"}) %>
						</div>
					</div>
					<div class="modal-footer">
						<input id="submit-report-abuse" class="btn btn-primary" type="submit" value="<%: Html.SnippetLiteral("Product Review Report Abuse Submit Button Text", ResourceManager.GetString("Report_As_Inappropriate")) %>" />
						<button class="btn btn-default" data-dismiss="modal" aria-hidden="true"><%: Html.SnippetLiteral("Product Review Report Abuse Cancel Button Text", ResourceManager.GetString("Cancel_DefaultText")) %></button>
					</div>
				</div>
			</div>
		</section>
	</div>
	<script type="text/javascript">
		jQuery.fn.restrictNumbers = function () {
			return this.each(function () {
				$(this).keydown(function (e) {
					var key = e.which || e.keyCode;
					if (!e.shiftKey && !e.altKey && !e.ctrlKey &&
						// numbers
						key >= 48 && key <= 57 ||
						// Numeric keypad
						key >= 96 && key <= 105 ||
						// comma, period and minus, . on keypad
						//key == 190 || key == 188 || key == 109 || key == 110 ||
						// Backspace and Tab and Enter
						key == 8 || key == 9 || key == 13 ||
						// Home and End
						key == 35 || key == 36 ||
						// left and right arrows
						key == 37 || key == 39 ||
						// Del and Ins
						key == 46 || key == 45) {
						return true;
					}
					return false;
				});
			});
		};
		
		String.prototype.parseJsonDate = function () {
			var jsonDate = this;
			var offset = new Date().getTimezoneOffset() * 60000;
			var parts = /\/Date\((-?\d+)([+-]\d[2])?(\d[2])?.*/.exec(jsonDate);
			if (parts[2] == undefined)
				parts[2] = 0;
			if (parts[3] == undefined)
				parts[3] = 0;
			return new Date(+parts[1] + offset + parts[2] * 3600000 + parts[3] * 60000);
		};
		
		function reviewFailure() {
			$(document).ready(function() {
				$.unblockUI();
				$("#create-review-error-message").fadeIn();
				return;
			});
		};

		function reviewCreated() {
			$(document).ready(function() {
				$.unblockUI();
				if ($("#create-review .validation-summary-errors").length) {
					$("#create-review .rateit").rateit();
					return;
				}
				$("#create-review-error-message").hide();
				$("#create-review-success-message").fadeIn();
				$("#create-review").slideUp();
				$("#first-write-review").hide();
				$("#write-review-link").hide();
				getReviews();
			});
		};

		function getReviews(page) {
			$(document).ready(function() {
				var productid = $("#productid").val();
				$("#reviews-content").hide();
				$("#reviews-loading").show();
				var pageNumber = $("#reviews-pagination").attr("data-current-page");
				if (pageNumber == null || pageNumber == '') {
					pageNumber = 1;
				}
				page = page || pageNumber;
				var pageSize = $("#reviews-pagination").attr("data-pagesize");
				if (pageSize == null || pageSize == '') {
					pageSize = 10;
				}
				var startRowIndex = (pageSize * page) - pageSize;
				if (startRowIndex < 0) {
					startRowIndex = 0;
				}
				var sortExpression = $("#reviews-sort").attr("data-current-orderby");
				queryReviews(productid, startRowIndex, pageSize, sortExpression,
					function(data) {
						// success
						var $content = $("#reviews-content");
						$content.empty();
						if (data == null || data.length == 0 || data.Reviews == null || data.Reviews.length == 0) {
							$("#reviews-none").clone().appendTo("#reviews-content").show();
							$content.fadeIn();
							return;
						}
						var reviews = data.Reviews;
						$("#reviewTemplate").tmpl(reviews).appendTo("#reviews-content");
						$("#reviews .rateit").rateit();
						$content.fadeIn();
						$("#reviews-sort").fadeIn();
						var $pagination = $("#reviews-pagination");
						$pagination
							.data("pagesize", data.PageSize)
							.data("pages", data.PageCount)
							.data("current-page", data.PageNumber)
							.data("count", data.ItemCount);
						$pagination.pagination({
							total_pages: $pagination.data("pages"),
							current_page: $pagination.data("current-page"),
							callback: function(event, pg) {
								event.preventDefault();
								var $li = $(event.target).closest("li");
								if ($li.not(".disabled").length > 0 && $li.not(".active").length > 0) {
									getReviews(pg);
								}
							}
						});
					},
					function(event, xhr, ajaxOptions, thrownError) {
						// error
					},
					function(event, xhr, ajaxOptions) {
						// complete
						$("#reviews-loading").hide();
					});
			});
		};
		
		function queryReviews(productid, startRowIndex, pageSize, sortExpression, success, error, complete) {
			success = $.isFunction(success) ? success : function () { };
			error = $.isFunction(error) ? error : function () { };
			complete = $.isFunction(complete) ? complete : function () { };
			if (!productid) {
				complete.call(this);
				error.call(this, null, null, null, "productid is null.");
				return;
			}
			startRowIndex = startRowIndex || 0;
			pageSize = pageSize || -1;
			var url = "<%: Url.Action("GetProductReviews", "Products", new { area = "Products" }) %>";
			var data = {};
			data.productid = productid;
			data.sortExpression = sortExpression;
			data.startRowIndex = startRowIndex;
			data.pageSize = pageSize;
			var jsonData = JSON.stringify(data);
			shell.ajaxSafePost({
				type: 'POST',
				dataType: "json",
				contentType: 'application/json',
				url: url,
				data: jsonData,
				global: false,
				success: success,
				error: function (event, xhr, ajaxOptions, thrownError) {
					error.call(this, event, xhr, ajaxOptions, thrownError);
				},
				complete: complete
			});
		};
		
		function reportAbuse(reviewid, remarks, success, error, complete) {
			success = $.isFunction(success) ? success : function () { };
			error = $.isFunction(error) ? error : function () { };
			complete = $.isFunction(complete) ? complete : function () { };
			if (!reviewid) {
				complete.call(this);
				error.call(this, null, null, null, "reviewid is null.");
				return;
			}
			var url = "<%: Url.Action("ReportAbuse", "Review", new { area = "Products" }) %>";
			var data = {};
			data.reviewid = reviewid;
			data.remarks = remarks;
			var jsonData = JSON.stringify(data);
			shell.ajaxSafePost({
				type: 'POST',
				dataType: "json",
				contentType: 'application/json',
				url: url,
				data: jsonData,
				global: false,
				success: success,
				error: function (event, xhr, ajaxOptions, thrownError) {
					error.call(this, event, xhr, ajaxOptions, thrownError);
				},
				complete: complete
			});
		};

		$(document).ready(function () {
			$("#submit-review").click(function () {
				$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
			});
			$(document).on("click", "#cancel-review", function (e) {
				e.preventDefault();
				$("#create-review").slideUp();
				$("#write-review").show();
				$("#first-write-review").show();
				$("#write-review-link").show();
			});
			$("#reviews-sort ul.dropdown-menu li").on("click", function (e) {
				e.preventDefault();
				var $sortLink = $(this).find("a");
				var sortLinkData = $sortLink.attr("data-orderby");
				$("#reviews-sort").attr("data-current-orderby", sortLinkData);
				$("#reviews-sort a.dropdown-toggle span").text($sortLink.text());
				getReviews(1);
			});
			$("#product-image-link").prettyPhoto({
				show_title: false,
				deepLinking: false,
				social_tools: ''
			});
			//$("#gallery .thumbnail").on("click", function(e) {
			//	e.preventDefault();
			//	$("#product-image").attr('src', $(this).find("img").attr("src"));
			//	$("#product-image-link").attr('href', $(this).attr("href"));
			//});
			$("#productQuantity")
				.restrictNumbers()
				.on("change", function () {
					var qty = $(this).val();
					var $target = $("#addProductToCart");
					var path = $target.data("action-url") + "/" + qty.toString();
					$target.attr("href", path);
				});
			$("#write-review-link").on('click', function (e) {
				e.preventDefault();
				$('#product-details a[href="#reviews"]').tab('show');
				$("#create-review").slideDown();
				$("#create-review .rateit").rateit();
				$("#write-review").hide();
				$("#write-review-link").hide();
				$("#first-write-review").hide();
			});
			$("#write-review").on('click', function (e) {
				e.preventDefault();
				$("#create-review").slideDown();
				$("#create-review .rateit").rateit();
				$(this).hide();
			});
			$("#create-review").on('show', function (e) {
				e.preventDefault();
				$("#create-review .rateit").rateit();
			});
			$('a[data-toggle="tab"]').on('click', function(e) {
				if (e.target.hash == "#reviews") {
					getReviews();
				} else {
					$("#reviews-sort").hide();
				}
			});
			$(document).on("click", ".report-abuse", function (e) {
				e.preventDefault();
				var id = $(this).attr("data-id");
				$("#report-abuse").attr("data-id", id);
				$('#report-abuse').modal('show');
			});
			$("#submit-report-abuse").on("click", function (e) {
				e.preventDefault();
				var id = $("#report-abuse").attr("data-id");
				var remarks = $("#abuse-reason").val();
				reportAbuse(id, remarks,
					function(data) {
						// success
					},
					function (event, xhr, ajaxOptions, thrownError) {
						// error
					},
					function (event, xhr, ajaxOptions) {
						// complete
						$("#report-abuse").modal('hide');
					});
			});
		});
	</script>
</asp:Content>
