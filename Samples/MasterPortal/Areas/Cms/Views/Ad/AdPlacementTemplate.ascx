<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>

{% if placement %}
{% assign placement = ads.placements[placement.id] %}
{% if placement.ads.size > 0 %}
{% if random %}
<div class="ad" data-url="{{ placement.random_url | h }}"></div>
{% else %}
<div>
	{% for ad in placement.ads %}
	<!-- ad #{{ forloop.index }} -->
	<div class="ad">
		<% Html.RenderPartial("AdTemplate"); %>
	</div>
	{% endfor %}
</div>
{% endif %}
{% endif %}
{% endif %}