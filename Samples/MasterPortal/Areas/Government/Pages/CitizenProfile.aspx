<%@ Page Language="C#" MasterPageFile="~/MasterPages/Profile.master" AutoEventWireup="True" CodeBehind="CitizenProfile.aspx.cs" Inherits="Site.Areas.Government.Pages.CitizenProfile" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<asp:Content ContentPlaceHolderID="ContentBottom" ViewStateMode="Enabled" runat="server">
	<asp:Panel ID="ConfirmationMessage" runat="server" CssClass="alert alert-success alert-block" Visible="False">
		<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a>
		<adx:Snippet runat="server" SnippetName="Profile Update Success Text" DefaultText="<%$ ResourceManager:Profile_Updated_Successfully %>" Editable="true" EditType="html" />
	</asp:Panel>
		
	<asp:Panel ID="MissingFieldsMessage" runat="server" CssClass="alert alert-danger alert-block" Visible="False">
		<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a>
		<adx:Snippet runat="server" SnippetName="Force Sign-Up Profile Explanation" DefaultText="<%$ ResourceManager:Force_SignUp_Profile_Explanation %>" Editable="true" EditType="html" />
	</asp:Panel>

	<asp:Panel ID="ProfileAlertInstructions" runat="server" CssClass="alert alert-warning alert-block" Visible="False">
		<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a>
		<asp:Label runat="server" Text='<%$ Context: Property=User, Attribute=adx_profilealertinstructions %>' />
	</asp:Panel>

	<fieldset>
		<legend>
			<adx:Snippet SnippetName="Profile Form Legend" DefaultText="<%$ ResourceManager:Your_Information %>" EditType="text" runat="server"/>
		</legend>

		<adx:CrmDataSource ID="ProfileDataSource" runat="server" CrmDataContextName="<%$ SiteSetting: Language Code %>" />
		<adx:CrmEntityFormView ID="ProfileFormView" runat="server"
			DataSourceID="ProfileDataSource"
			CssClass="crmEntityFormView"
			EntityName="contact"
			FormName="Citizen Respond Profile"
			OnItemUpdated="OnItemUpdated"
			OnItemUpdating="OnItemUpdating"
			ValidationGroup="Profile"
			ValidationSummaryCssClass="alert alert-danger alert-block"
			RecommendedFieldsRequired="True"
			ShowUnsupportedFields="False"
			ToolTipEnabled="False"
			ClientIDMode="Static"
			Mode="Edit"
			LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
			ContextName="<%$ SiteSetting: Language Code %>">
			<UpdateItemTemplate>
			</UpdateItemTemplate>
		</adx:CrmEntityFormView>
	</fieldset>
		
	<asp:Panel ID="MarketingOptionsPanel" Visible="false" runat="server">
		<fieldset>
			<legend>
				<adx:Snippet runat="server" SnippetName="Profile/MarketingPref" DefaultText="<%$ ResourceManager:How_May_We_Contact_You %>" Editable="True" EditType="text" />
			</legend>
			<div class="form-horizontal">
				<div class="form-group">
					<div class="col-sm-12">
						<div class="checkbox">
							<label>
								<asp:CheckBox runat="server" ID="marketEmail" />
								<adx:Snippet runat="server" SnippetName="Profile/MarketEmail" DefaultText="<%$ ResourceManager:Email_DefaultText %>" Editable="True" EditType="text" />
							</label>
						</div>
						<div class="checkbox">
							<label>
								<asp:CheckBox runat="server" ID="marketFax" />
								<adx:Snippet runat="server" SnippetName="Profile/MarketFax" DefaultText="<%$ ResourceManager:Fax_DefaultText %>" Editable="True" EditType="text" />
							</label>
						</div>
						<div class="checkbox">
							<label>
								<asp:CheckBox runat="server" ID="marketPhone" />
								<adx:Snippet runat="server" SnippetName="Profile/MarketPhone" DefaultText="<%$ ResourceManager:Phone_DefaultText %>" Editable="True" EditType="text" />
							</label>
						</div>
						<div class="checkbox">
							<label>
								<asp:CheckBox runat="server" ID="marketMail" />
								<adx:Snippet runat="server" SnippetName="Profile/MarketMail" DefaultText="<%$ ResourceManager:Mail_DefaultText %>" Editable="True" EditType="text" />
							</label>
						</div>
					</div>
				</div>
			</div>
		</fieldset>
	</asp:Panel>

	<asp:Panel ID="MarketingLists" runat="server">
		<fieldset>
			<legend>
				<adx:Snippet runat="server" SnippetName="Profile Marketing Lists Title Text" DefaultText="<%$ ResourceManager:Subscribe_To_Following_Email_Lists %>" Editable="true" EditType="text" />
			</legend>
			<div class="form-horizontal">
				<div class="form-group">
					<div class="col-sm-12">
						<asp:Listview ID="MarketingListsListView" runat="server">
							<LayoutTemplate>
								<ul class="list-unstyled">
									<asp:PlaceHolder ID="ItemPlaceholder" runat="server" />
								</ul>
							</LayoutTemplate>
							<ItemTemplate>
								<li>
									<div class="checkbox">
										<label>
											<asp:CheckBox ID="ListCheckbox" runat="server" Checked='<%# IsListChecked(Container.DataItem) %>'/>
											<asp:HiddenField ID="ListID" Value='<%# ((Entity)Container.DataItem).GetAttributeValue<Guid>("listid") %>' runat="server" />
											<%# ((Entity)Container.DataItem).GetAttributeValue<string>("listname") %> &ndash; <%# ((Entity)Container.DataItem).GetAttributeValue<string>("purpose") %>
										</label>
									</div>
								</li>
							</ItemTemplate>
						</asp:Listview>
					</div>
				</div>
			</div>
		</fieldset>
	</asp:Panel>

	<div class="form-actions">
		<asp:Button ID="SubmitButton" Text='<%$ Snippet: Profile Submit Button Text, Update %>' CssClass="btn btn-primary" OnClick="SubmitButton_Click" ValidationGroup="Profile" runat="server" />
	</div>
</asp:Content>