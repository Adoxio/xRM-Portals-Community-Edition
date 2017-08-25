<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Adxstudio.Xrm.Products.IProduct>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<% using (Html.BeginForm("CreateReview", "Products", new { productid = Model.Entity.Id }, FormMethod.Post, new {id="create-review-id",  @class = "form-horizontal" }))
{ %>
	<fieldset>
		<legend>
			<%: Html.SnippetLiteral("Product Review Create Title", ResourceManager.GetString("Submit_A_Review")) %>
		</legend>
		<%= Html.ValidationSummary(string.Empty, new {@class = "alert alert-block alert-danger"}) %>
		<% if (!Request.IsAuthenticated)
		{ %>
			<div class="form-group">
				<label class="col-sm-3 control-label required" for="reviewerName"><%: Html.SnippetLiteral("Product Create Review Reviewer Name Label", ResourceManager.GetString("Nickname")) %></label>
				<div class="col-sm-9">
					<%= Html.TextBox("reviewerName", string.Empty, new {@maxlength = "100", @class = "form-control"}) %>
				</div>
			</div>
			<div class="form-group">
				<label class="col-sm-3 control-label required" for="reviewerEmail"><%: Html.SnippetLiteral("Product Create Review Reviewer Email Label", ResourceManager.GetString("E-mail")) %></label>
				<div class="col-sm-9">
					<%= Html.TextBox("reviewerEmail", string.Empty, new {@maxlength = "200", @class = "form-control"}) %>
					<span class="help-inline"> (This will not be displayed on the review)</span>
				</div>
			</div>
		<% } else { %>
			<div class="form-group">
				<label class="col-sm-3 control-label required" for="reviewerName"><%: Html.SnippetLiteral("Product Create Review Reviewer Name Label", ResourceManager.GetString("Nickname")) %></label>
				<div class="col-sm-9">
					<%= Html.TextBox("reviewerName", Html.AttributeLiteral(Html.PortalUser(), "nickname"), new {@maxlength = "100", @class = "form-control"}) %>
				</div>
			</div>
		<% } %>
		<div class="form-group">
			<label class="col-sm-3 control-label" for="reviewerLocation"><%: Html.SnippetLiteral("Product Create Review Reviewer Location Label", ResourceManager.GetString("Location")) %></label>
			<div class="col-sm-9">
				<%= Html.TextBox("reviewerLocation", string.Empty, new {@maxlength = "100", @class = "form-control"}) %>
			</div>
		</div>
		<div class="form-group">
			<label class="col-sm-3 control-label required" for="rating"><%: Html.SnippetLiteral("Product Create Review Rating Label", ResourceManager.GetString("Rating_DefaultText")) %></label>
			<div class="col-sm-9">
				<%= Html.Hidden("maximumRatingValue", 5) %>
				<%= Html.Hidden("rating", 0) %>
				<div class="rateit" data-rateit-resetable="false" data-rateit-step="1" data-rateit-min="0" data-rateit-max="5" data-rateit-backingfld="#rating"></div>
			</div>
		</div>
		<div class="form-group">
			<div class="col-sm-offset-3 col-sm-9">
				<p>
					<%: Html.SnippetLiteral("Product Review Recommend Text", ResourceManager.GetString("Recommend_To_A_Friend")) %>
				</p>
				<div class="radio">
					<label>
						<%= Html.RadioButton("recommend", true, true) %> Yes
					</label>
				</div>
				<div class="radio">
					<label>
						<%= Html.RadioButton("recommend", false, false) %> No
					</label>
				</div>
			</div>
		</div>
		<div class="form-group">
			<label class="col-sm-3 control-label required" for="title"><%: Html.SnippetLiteral("Product Create Review Title Label", ResourceManager.GetString("Title_Label")) %></label>
			<div class="col-sm-9">
				<%= Html.TextBox("title", string.Empty, new {@maxlength = "150", @class = "form-control"}) %>
			</div>
		</div>
		<div class="form-group">
			<label class="col-sm-3 control-label" for="content"><%: Html.SnippetLiteral("Product Create Review Content Label", ResourceManager.GetString("Review")) %></label>
			<div class="col-sm-9">
				<%= Html.TextArea("content", string.Empty, new {@rows = "10", @maxlength = "10000", @class = "form-control"}) %>
			</div>
		</div>
		<div class="form-group">
			<div class="col-sm-offset-3 col-sm-9">
				<input id="submit-review" class="btn btn-primary" type="submit" value="<%: Html.SnippetLiteral("Product Review Submit Button Text", ResourceManager.GetString("Submit_Review")) %>" />
				<a id="cancel-review" href="#cancel" class="cancel btn btn-default" title="<%: Html.SnippetLiteral("Product Review Cancel Button Text", ResourceManager.GetString("Cancel_DefaultText")) %>"><%: Html.SnippetLiteral("Product Review Cancel Button Text", ResourceManager.GetString("Cancel_DefaultText")) %></a>
			</div>
		</div>
	</fieldset>
<% } %>
	<script type="text/javascript">
	    $(function () {
	        $("#submit-review").click(function () {
	            shell.ajaxSafePost({
	                type: "POST",
	                success: function (result) {
	                    $("#create-review").html(result);
	                    reviewCreated();
	                }
	            }, $("#create-review-id")).fail(reviewFailure);

	            return false;
	        });
	    });
     </script>
