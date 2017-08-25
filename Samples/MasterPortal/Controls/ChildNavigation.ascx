<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChildNavigation.ascx.cs" Inherits="Site.Controls.ChildNavigation" %>

<% if (ShowChildren && Children.Any()) {%>
	<div class="child-navigation content-panel panel panel-default">
		<div class="panel-heading">
			<h4>
				<span class="fa fa-folder-open" aria-hidden="true"></span>
				<adx:Snippet SnippetName="Page Children Heading" DefaultText="<%$ ResourceManager:In_This_Section_DefaultText %>" EditType="text" runat="server"/>
			</h4>
		</div>
		<% if (ShowDescriptions) { %>
			<ul class="list-group">
			<% foreach (var node in Children) { %>
				<li class="list-group-item">
					<h4 class="list-group-item-heading">
						<a href="<%: node.Url %>" title="<%: node.Title %>"><%: node.Title %></a>
					</h4>
					<div class="list-group-item-text">
						<%= GetDescription(node) %>
					</div>
				</li>
			<% } %>
		</ul>
		<% } else { %>
			<div class="list-group">
				<% foreach (var node in Children) { %>
					<a href="<%: node.Url %>" class="list-group-item" title="<%: node.Title %>"><%: node.Title %></a>
				<% } %>
			</div>
		<% } %>
	</div>
<% } %>

<% if (ShowShortcuts && Shortcuts.Any()) {%>
	<div class="child-navigation content-panel panel panel-default">
		<div class="panel-heading">
			<h4>
				<span class="fa fa-share-square-o" aria-hidden="true"></span>
				<adx:Snippet SnippetName="Page Related Heading" DefaultText="<%$ ResourceManager:Related_Topics_DefaultText %>" EditType="text" runat="server"/>
			</h4>
		</div>
		<% if (ShowDescriptions) { %>
			<ul class="list-group">
			<% foreach (var node in Shortcuts) { %>
				<li class="list-group-item">
					<h4 class="list-group-item-heading">
						<a href="<%: node.Url %>" title="<%: node.Title %>"><%: node.Title %></a>
					</h4>
					<div class="list-group-item-text">
						<%= GetDescription(node) %>
					</div>
				</li>
			<% } %>
		</ul>
		<% } else { %>
			<div class="list-group">
				<% foreach (var node in Shortcuts) { %>
					<a href="<%: node.Url %>" class="list-group-item" title="<%: node.Title %>"><%: node.Title %></a>
				<% } %>
			</div>
		<% } %>
	</div>
<% } %>