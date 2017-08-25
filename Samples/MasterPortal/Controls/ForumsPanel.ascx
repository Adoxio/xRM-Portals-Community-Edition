<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ForumsPanel.ascx.cs" Inherits="Site.Controls.ForumsPanel" %>

<asp:ObjectDataSource ID="ForumsDataSource" TypeName="Adxstudio.Xrm.Forums.IForumAggregationDataAdapter" OnObjectCreating="CreateForumDataAdapter" SelectMethod="SelectForums" runat="server" />
<asp:ListView DataSourceID="ForumsDataSource" runat="server">
	<LayoutTemplate>
		<div class="content-panel panel panel-default">
			<div class="panel-heading">
				<h4>
					<span class="fa fa-comments" aria-hidden="true"></span>
					<adx:Snippet SnippetName="Home Forum Activity Heading" DefaultText="<%$ ResourceManager:Forums_DefaultText %>" EditType="text" runat="server" ToolTip="<%$ ResourceManager:Forums_DefaultText %>" />
				</h4>
			</div>
			<ul class="list-group">
				<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
			</ul>
		</div>
	</LayoutTemplate>
	<ItemTemplate>
		<li class="list-group-item">
			<div class="row">
				<div class="col-sm-6">
					<h4 class="list-group-item-heading">
						<asp:HyperLink NavigateUrl='<%#: Eval("Url") %>' Text='<%#: Eval("Name") %>' ToolTip='<%#: Eval("Name") %>' runat="server"/>
					</h4>
					<div class="list-group-item-text content-metadata"><%# Eval("Description") %></div>
				</div>
				<div class="col-sm-3 content-metadata"><%#: Eval("ThreadCount") %> threads</div>
				<div class="col-sm-3 content-metadata"><%#: Eval("PostCount") %> posts</div>
			</div>
		</li>
	</ItemTemplate>
</asp:ListView>