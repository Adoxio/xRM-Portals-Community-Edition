<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<div class="review clearfix" data-item-id="${Id}">
	<div class="rateit" data-rateit-value="${Rating}" data-rateit-ispreset="true" data-rateit-readonly="true"></div>
	<h4 class="title">
		${Title}
	</h4>
	<div>
		<small><strong><%: Html.SnippetLiteral("Product Review Date Label", ResourceManager.GetString("Date_Posted")) %></strong> <abbr class="timeago">${jQuery.timeago(SubmittedOn.parseJsonDate())}</abbr></small>
	</div>
	<div>
		<small><strong><%: Html.SnippetLiteral("Product Review Reviewer Name Label", ResourceManager.GetString("Submitted_By")) %></strong> <span class="text-info">${ReviewerName}</span></small>
	</div>
	<div>
		{{if ReviewerLocation}}
			<small><strong><%: Html.SnippetLiteral("Product Review Reviewer Location Label", "Location:") %></strong> ${ReviewerLocation}</small>
		{{/if}}
	</div>
	<div class="content">
		{{html Content}}
	</div>
	<h5 class="recommend">
		{{if Recommend}}
			<span class="text-success">
				<span class='fa fa-check-circle-o' aria-hidden='true'></span> <%: Html.SnippetLiteral("Product Review Recommend Yes Text", ResourceManager.GetString("I_Would_Recommend_To_A_Friend")) %>
			</span>
		{{else}}
			<span class="text-warning">
				<span class='fa fa-ban' aria-hidden='true'></span> <%: Html.SnippetLiteral("Product Review Recommend No Text", ResourceManager.GetString("I_Would_Not_Recommend_To_A_Friend")) %>
			</span>
		{{/if}}
	</h5>
	<div class="review-actions pull-right">
		<a class="report-abuse" href="#" data-id="${Id}" title="<%: Html.SnippetLiteral("Product Review Report Abuse Link Text", ResourceManager.GetString("Report_Abuse")) %>"><%: Html.SnippetLiteral("Product Review Report Abuse Link Text", ResourceManager.GetString("Report_Abuse")) %></a>
	</div>
</div>
