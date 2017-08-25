<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Search.ascx.cs" Inherits="Site.Areas.KnowledgeBase.Controls.Search" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<div class="content-panel panel panel-default">
	<div class="panel-heading">
		<h4>
			<span class="fa fa-search" aria-hidden="true"></span>
			<%: Html.TextSnippet("Knowledge Base Search Heading", defaultValue: ResourceManager.GetString("Search_Knowledge_Base"), tagName: "span") %>
		</h4>
	</div>
	<div class="panel-body">
		<asp:Panel ID="SearchForm" CssClass="input-group" DefaultButton="SubmitSearch" ViewStateMode="Enabled" runat="server">
			<asp:PlaceHolder ID="Subject" Visible="False" runat="server">
				<div class="btn-group btn-select input-group-btn" data-target="#KnowledgeBaseSubjectFilter" data-focus="#KnowledgeBaseQuery">
					<a href="#" class="btn btn-default dropdown-toggle" data-toggle="dropdown" title="<%: SubjectName %>">
						<span class="selected"><%: SubjectName %></span>
						<span class="caret"></span>
					</a>
					<ul class="dropdown-menu">
						<li>
							<a data-value="<%: SubjectId %>" title="<%: SubjectName %>"><%: SubjectName %></a>
						</li>
						<li>
							<a data-value="" title="<%: Html.SnippetLiteral("Knowledge Base Default Search Filter Text", ResourceManager.GetString("All_Articles")) %>"><%: Html.SnippetLiteral("Knowledge Base Default Search Filter Text", ResourceManager.GetString("All_Articles")) %></a>
						</li>
					</ul>
				</div>
				<asp:DropDownList ID="KnowledgeBaseSubjectFilter" ClientIDMode="Static" CssClass="btn-select" DataTextField="Text" DataValueField="Value" runat="server" />
			</asp:PlaceHolder>
			<asp:TextBox ID="KnowledgeBaseQuery" ClientIDMode="Static" CssClass="form-control" runat="server"/>
			<div class="input-group-btn">
				<asp:LinkButton ID="SubmitSearch" CssClass="btn btn-default" OnClick="SubmitSearch_Click" runat="server">
					<span class="fa fa-search" aria-hidden="true"></span>
				</asp:LinkButton>
			</div>
		</asp:Panel>
	</div>
</div>
