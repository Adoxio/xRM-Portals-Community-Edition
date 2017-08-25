<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
{% if placement %}
{% assign placement = polls.placements[placement.id] %}
{% if placement.polls.size > 0 %}
<div class="content-panel panel panel-default">
	<div class="panel-heading">
		{% assign sm = sitemarkers["Poll Archives"] %}
		{% if sm %}
		<a href="{{ sm.url | h}}" class="pull-right">{{ snippets["polls/archiveslabel"] | default: resx.Poll_Archives_Heading | h }}</a>
		{% endif %}
		<h4>
			<span class="fa fa-question-circle" aria-hidden="true"></span>
			{% editable snippets "polls/title" type: 'text', escape: true, default: resx.Poll_DefaultText, tag: 'span' %}
		</h4>
	</div>
	{% if random %}
	<div class="panel-body poll random" data-url="{{ placement.random_url | h }}" data-submit-url="{{ placement.submit_url | h }}"></div>
	{% else %}
	{% for poll in placement.polls %}
	{% unless forloop.index0 == 0 %}<hr style="margin:0"/>{% endunless %}
	<div class="panel-body poll" data-url="{{ poll.poll_url | h }}" data-submit-url="{{ poll.submit_url | h }}">
	</div>
	{% endfor %}
	{% endif %}
</div>
{% endif %}
{% endif %}