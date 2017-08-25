<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/WebForms.master" CodeBehind="OpportunityDetails.aspx.cs" Inherits="Site.Areas.Opportunities.Pages.OpportunityDetails" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Opportunities/css/opportunity-details.css") %>">
</asp:Content>

<asp:Content  ContentPlaceHolderID="PageHeader" ViewStateMode="Enabled" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<div class="page-header">
		<div id="opportunity-assigned-to" class="form-inline">
			<div class="input-group">
				<div class="input-group-addon">
					<adx:Snippet runat="server" SnippetName="opportunity-details/label/partner-assigned-to" DefaultText="<%$ ResourceManager:Assigned_To_DefaultText %>" Editable="true" EditType="text" Literal="true" />
				</div>
				<asp:TextBox ID="CurrentlyAssignedToLabel" CssClass="readonly form-control" ReadOnly="True" runat="server"/>
				<asp:DropDownList ID="AssignToList" runat="server" ClientIDMode="Static" CssClass="form-control" />
			</div>
		</div>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
		</h1>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" ViewStateMode="Enabled" runat="server">
	<div id="opportunity-ui" style="display: none;">
		<div class="row">		
			<asp:Panel ID="CrmEntityFormViewsPanel" CssClass="col-md-8" runat="server">
			    <adx:CrmDataSource runat="server" ID="ContactWebFormDataSource"/>
			    <adx:CrmDataSource runat="server" ID="OpportunityDataSource"/>
			    <div id="opportunities-details-tabs" >
					<ul class="toolbar-nav nav nav-tabs">
						<li id="opportunities-details-form-views-tab">
							<a href="#opportunities-details-form-views" data-toggle="tab" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Opportunity_Details_DefaultText") %>'>
							<adx:Snippet runat="server" SnippetName="opportunities-details/label/details-tab" DefaultText="<%$ ResourceManager:Opportunity_Details_DefaultText %>" Editable="true" EditType="text" /></a>
						</li>
						<li id="opportunities-history-tab">
							<a href="#opportunities-history" data-toggle="tab" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Opportunity_History_DefaultText") %>'>
							<adx:Snippet runat="server" SnippetName="opportunities-details/label/history-tab" DefaultText="<%$ ResourceManager:Opportunity_History_DefaultText %>" Editable="true" EditType="text" /></a>
						</li>
						<li id="opportunities-notes-tab">
							<a href="#opportunities-notes" data-toggle="tab" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Opportunity_Notes_DefaultText") %>'>
							<adx:Snippet runat="server" SnippetName="opportunities-details/label/notes-tab" DefaultText="<%$ ResourceManager:Opportunity_Notes_DefaultText %>" Editable="true" EditType="text" /></a>
						</li>
						<li id="opportunities-contacts-tab">
							<a href="#opportunities-contacts" data-toggle="tab" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Opportunity_Contacts_DefaultText") %>'>
							<adx:Snippet runat="server" SnippetName="opportunities-details/label/contacts-tab" DefaultText="<%$ ResourceManager:Opportunity_Contacts_DefaultText %>" Editable="true" EditType="text" /></a>
						</li>
					</ul>
					<div id="opportunities-details-content-pane" class="tab-content">
						<div id="opportunities-details-form-views" class="tab-pane fade">
							<div class="panel panel-default">
								<div class="panel-heading">
									<div class="panel-title">
										<span class="fa fa-edit" aria-hidden="true"></span>
										<adx:Snippet runat="server" SnippetName="opportunities-details/label/company-name" DefaultText="<%$ ResourceManager:Customer_DefaultText %>" Editable="true" EditType="text" />
									</div>
								</div>
								<div class="panel-body">
									<asp:Label ID="CompanyName" runat="server"/>
								</div>
							</div>

							<adx:CrmEntityFormView ID="ContactFormView" runat="server"
								EntityName="contact"
								FormName="Opportunity Contact Details Form"
								Mode="Edit"
								ValidationGroup="UpdateOpportunity"
								OnItemUpdating="ContactUpdating"
								ClientIDMode="Static"
								CssClass="crmEntityFormView"
								LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
								ContextName="<%$ SiteSetting: Language Code %>">
								<UpdateItemTemplate/>
							</adx:CrmEntityFormView>
							
							<div class="crmEntityFormView">
								<table class="tab">
									<tbody>
										<tr>
											<td>
												<table class="section">
													<tbody>
														<tr>
															<td class="cell">
																<div class="info"><label><adx:Snippet runat="server" SnippetName="opportunities-details/label/products" DefaultText="<%$ ResourceManager:Products_DefaultText %>" Editable="true" EditType="text" /></label></div>
																<div class="control"><asp:TextBox ID="Products" runat="server" ReadOnly="true" CssClass="readonly form-control"></asp:TextBox></div>
															</td>
														</tr>
													</tbody>
												</table>
											</td>
										</tr>
									</tbody>
								</table>
							</div>
							
							<adx:CrmEntityFormView ID="OpportunityFormView" runat="server"
								EntityName="opportunity"
								FormName="Opportunity Details Web Form"
								Mode="Edit"
								ValidationGroup="UpdateOpportunity"
								OnItemUpdated="OpportunityUpdated"
								OnItemUpdating="OpportunityUpdating"
								ClientIDMode="Static"
								CssClass="crmEntityFormView"
								LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
								ContextName="<%$ SiteSetting: Language Code %>">
								<UpdateItemTemplate/>
							</adx:CrmEntityFormView>
						</div>

						<div id="opportunities-history" class="tab-pane fade">
							<asp:PlaceHolder ID="OpportunityHistoryPlaceHolder" runat="server"></asp:PlaceHolder>
						</div>

						<div id="opportunities-notes" class="tab-pane fade">
							<asp:TextBox ID="OpportunityNotes" runat="server" TextMode="MultiLine" CssClass="form-control" />
						</div>

						<div id="opportunities-contacts" class="tab-pane fade">
							<asp:PlaceHolder ID="OpportunityContactsPlaceHolder" runat="server"/>
							<div>
								<label>
									<adx:Snippet runat="server" SnippetName="addingcontacts/instructions" DefaultText="<%$ ResourceManager:Add_Existing_Or_Create_New_Contact %>" Editable="true" EditType="text" />
								</label>
								<div class="form-group">
									<asp:CheckBox ID="AddContactCheckBox" CssClass="checkbox" Text="Add an existing contact to this opportunity" TextAlign="Right" ClientIDMode="Static" runat="server" />
								</div>
								<div class="form-group">
									<div class="input-group">
										<asp:DropDownList ID="AddContactList" runat="server" ClientIDMode="Static" CssClass="form-control" />
										<div class="input-group-btn">
											<adx:SiteMarkerLinkButton ID="AddContactButton"  runat="server" SiteMarkerName="Create Customer Contact" CssClass="btn btn-success" >
												<span class="fa fa-plus-circle" aria-hidden="true"></span> Create New
											</adx:SiteMarkerLinkButton>
										</div>
									</div>
								</div>
							</div>
						</div>
					</div>
				</div>
			</asp:Panel>

			<asp:Panel ID="OpportunityStatusPanel" CssClass="col-md-4" runat="server">
				<div id="opportunities-status-tabs">
					<ul class="toolbar-nav nav nav-tabs" >
						<li id="opportunities-details-status-tab">
							<a href="#opportunities-details-status"  title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Opportunity_Status_DefaultText") %>'>
								<adx:Snippet runat="server" SnippetName="opportunitiesdetails/label/status-section" DefaultText="<%$ ResourceManager:Opportunity_Status_DefaultText %>" Editable="true" EditType="text" />
							</a>
						</li>
					</ul>
					<div id="opportunities-details-status">
						<div class="panel panel-default">
							<div class="panel-heading">
								<h4 class="panel-title">
									<asp:Literal Text="<%$ Snippet: current_pipeline_phase, Current Phase: %>" runat="server" />
									<asp:Literal ID="PipelinePhaseText" runat="server"/>
								</h4>
							</div>
							<div class="panel-body">
								<div class="radio">
									<asp:RadioButton ID="UpdatePipelinePhase" Text="Update Pipeline Phase" GroupName="StatusReason" ClientIDMode="Static" runat="server" />
									<asp:DropDownList ID="PipelinePhase" runat="server" ClientIDMode="Static" CssClass="form-control" />
									<asp:TextBox ID="PipelineUpdateDetails" runat="server" TextMode="MultiLine" ClientIDMode="Static" CssClass="form-control" />
								</div>
								<div class="radio">
									<asp:RadioButton ID="WinOpportunity" Text="Declare as Won" GroupName="StatusReason" ClientIDMode="Static" runat="server" />
									<asp:TextBox ID="WonDetails" runat="server" TextMode="MultiLine" ClientIDMode="Static" CssClass="form-control" />
									<div id="won-opportunity-message" title="Declare Opportunity as Won" style="display: none;">
										<adx:Snippet runat="server" SnippetName="opportunities-details/win-opportunity/warning" DefaultText="<%$ ResourceManager:Declare_Opportunity_As_Won %>" Editable="false" />
									</div>
								</div>
								<div class="radio" id="return-parent-div">
									<asp:RadioButton ID="ReturnToNetwork" Text="Return to Network" GroupName="StatusReason" ClientIDMode="Static" runat="server" />
									<crm:CrmMetadataDataSource ID="ReasonForReturnSource" runat="server" AttributeName="adx_reasonforreturn" EntityName="opportunity" CrmDataContextName="<%$ SiteSetting: Language Code %>" />
									<asp:DropDownList ID="ReasonForReturn" runat="server" DataSourceID="ReasonForReturnSource" DataTextField="OptionLabel" DataValueField="OptionValue" ClientIDMode="Static" CssClass="form-control" />
									<div id="return-to-network-message" title="Return Opportunity to Distributor" style="display: none;">
										<adx:Snippet runat="server" SnippetName="opportunities-details/return-to-network/warning" DefaultText="opportunities-details/return-to-network/warning" Editable="false" />
									</div>
								</div>
								<div class="radio">
									<asp:RadioButton ID="CancelOpportunity" Text="Cancel Opportunity" GroupName="StatusReason" ClientIDMode="Static" runat="server" />
									<asp:TextBox ID="CancelDetails" runat="server" TextMode="MultiLine" CssClass="form-control" ClientIDMode="Static" />
									<div id="cancel-opportunity-message" title="This cancels the opportunity." style="display: none;">
										<adx:Snippet runat="server" SnippetName="opportunities-details/win-opportunity/warning" DefaultText="<%$ ResourceManager:Declare_Opportunity_As_Won %>" Editable="false" />
									</div>
								</div>
							</div>
						</div>
					</div>
				</div>
			</asp:Panel>
		</div>

		<asp:Panel ID="ConfirmationMessage" runat="server" Visible="false" CssClass="alert alert-block alert-success opportunity-saved-message">
			<adx:Snippet runat="server" SnippetName="opportunities-details/message/saved-successfully" DefaultText="<%$ ResourceManager:Opportunity_Saved_Successfully_Message %>" Editable="true" EditType="text" />
		</asp:Panel>

		<asp:ValidationSummary runat="server" ValidationGroup="UpdateOpportunity" CssClass="alert alert-block alert-danger" DisplayMode="List" />

		<asp:Panel ID="ErrorMessage" CssClass="alert alert-block alert-danger" runat="server" Visible="false">
			<p>There is a problem with this Opportunity. The information for this opportunity appears to be corrupt or missing.</p>
		</asp:Panel>
		
		<div class="crmEntityFormView">
			<div class="actions">
				<asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="SubmitButton_Click" ValidationGroup="UpdateOpportunity" />
				<asp:Button ID="CancelButton" runat="server" CssClass="btn btn-default" Text="Cancel" OnClick="CancelButton_Click" />
			</div>
		</div>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="Scripts" runat="server">
	<script type="text/javascript">
		$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
		$(function () {
			$("#opportunities-details-tabs").tab();
			$("#opportunities-details-form-views-tab a").tab("show");
			$("#opportunities-status-tabs").tab();
			$("#opportunities-details-status-tab a").tab("show");

			$("#opportunities-details-tabs").show();
			$("#opportunities-status-tabs").show();

			$("#opportunities-history div span.stage-date").each(function () {
				var dateTime = new Date($(this).text());
				$(this).text(dateTime.toString("MMM d, yyyy h:mm tt"));
			});

			$("form").submit(function () {
				if (Page_IsValid) {
					$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
				}
			});

			setTimeout(function () {
				$("div.opportunity-saved-message").hide("fade", {}, 1000);
			}, 5000);

			$("#ReasonForReturn").hide();

			$("#PipelinePhase").hide();
			$("#PipelineUpdateDetails").hide();
			$("#WonDetails").hide();
			$("#CancelDetails").hide();

			$("#UpdatePipelinePhase").click(function () {
				$("#WonDetails").hide();
				$("#CancelDetails").hide();
				$("#ReasonForReturn").hide();

				$("#PipelinePhase").show("slide");
				$("#PipelineUpdateDetails").show("slide");
			});

			$("#WinOpportunity").click(function () {
				$("#ReasonForReturn").hide();
				$("#PipelinePhase").hide();
				$("#PipelineUpdateDetails").hide();
				$("#CancelDetails").hide();

				$("#WonDetails").show("slide");
			});
			$("#CancelOpportunity").click(function () {
				$("#ReasonForReturn").hide();
				$("#PipelinePhase").hide();
				$("#PipelineUpdateDetails").hide();
				$("#WonDetails").hide();

				$("#CancelDetails").show("slide");
			});
			$("#ReturnToNetwork").click(function () {
				$("#PipelinePhase").hide();
				$("#PipelineUpdateDetails").hide();
				$("#WonDetails").hide();
				$("#CancelDetails").hide();

				$("#ReasonForReturn").show("slide");
			});

			$("#opportunities-details-form-views-tab a").click(function (e) {
				e.preventDefault();
				$(this).tab('show');
			});

			$("#opportunities-history-tab a").click(function (e) {
				e.preventDefault();
				$(this).tab('show');
			});

			$("#opportunities-notes-tab a").click(function (e) {
				e.preventDefault();
				$(this).tab('show');
			});

			$("#opportunities-contacts-tab a").click(function (e) {
				e.preventDefault();
				$(this).tab('show');
			});

			$('input[type="submit"]').click(function () {
				$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
			});

			$("#opportunity-ui").show();
			$.unblockUI();
		});

		function clickRadioAndSlide(radio) {
			radio.click();
			var label = $("#" + radio.attr("id") + " + label");
			$("#ReasonForReturn").hide();
			$("#PipelinePhase").hide();
			$("#PipelineUpdateDetails").hide();
			$("#WonDetails").hide();
			$("#CancelDetails").hide();
			label.hide();
			label.show("slide");
		}
	</script>
</asp:Content>