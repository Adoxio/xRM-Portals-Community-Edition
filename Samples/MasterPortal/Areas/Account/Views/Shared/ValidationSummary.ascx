<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ModelErrorCollection>" %>

<% if (Model.Any()) { %>
	<div class="validation-summary-errors alert alert-block alert-danger" data-valmsg-summary="true">
		<ul>
			<% foreach (var error in Model) { %>				<li><%: error.ErrorMessage %></li>			<% } %>		</ul>
	</div>
<% } else { %>
	<div class="validation-summary-valid alert alert-block alert-danger" data-valmsg-summary="true"><ul><li class="hidden"></li></ul></div>
<% } %>
