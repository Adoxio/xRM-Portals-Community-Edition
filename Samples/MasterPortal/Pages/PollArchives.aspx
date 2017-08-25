<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" Inherits="Site.Pages.PollArchives" Codebehind="PollArchives.aspx.cs" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<asp:ListView ID="PollsArchiveListView" runat="server" OnItemDataBound="PollsArchiveListView_ItemDataBound">
		<LayoutTemplate>
				<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
		</LayoutTemplate>
		<ItemTemplate>
			<div class="poll content-panel panel panel-default">
				<div class="panel-heading">
					<div class="panel-title poll-question"><%# ((Entity)Container.DataItem).GetAttributeValue<string>("adx_question") %></div>
				</div>
				<div class="panel-body">
					<asp:ListView ID="PollResponsesListView" runat="server">
						<LayoutTemplate>
							<div class="poll-results">
								<asp:PlaceHolder ID="itemPlaceholder" runat="server" />
							</div>
						</LayoutTemplate>
						<ItemTemplate>
							<div class="poll-result">
								<asp:Label CssClass="poll-option" runat="server">
									<%#: Eval("Response") %> (<%#: Eval("Count") ?? 0 %>)
								</asp:Label>
								<div class="progress">
										<div class="progress-bar" style="<%#: Eval("Percentage", "width: {0}%") %>"><%#: Eval("Percentage", "{0}%") %></div>
								</div>
							</div>
						</ItemTemplate>
						<EmptyDataTemplate>
							<adx:Snippet runat="server" SnippetName="polls/archives/emptyresponsemessage" DefaultText="<%$ ResourceManager:No_Responses_To_This_Poll %>" EditType="html"/>
						</EmptyDataTemplate>
					</asp:ListView>
					<div>
						<adx:Snippet runat="server" CssClass="TotalLabel" SnippetName="polls/archives/totalslabel" DefaultText="<%$ ResourceManager:Total_Votes_DefaultText %>"/>
						<asp:Label ID="Total" runat="server"/>
					</div>
				</div>
			</div>
		</ItemTemplate>
		<EmptyDataTemplate>
			<adx:Snippet runat="server" SnippetName="polls/archives/emptyarchivesmessage" DefaultText="<%$ ResourceManager:No_Archived_Polls_Currently %>" EditType="html"/>
		</EmptyDataTemplate>
	</asp:ListView>
</asp:Content>