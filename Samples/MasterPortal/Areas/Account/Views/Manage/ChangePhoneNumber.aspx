<%@ Page Title="" Language="C#" MasterPageFile="Manage.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.AddPhoneNumberViewModel>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ChangePhoneNumber/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="ProfileNavbar" runat="server">
	<% Html.RenderPartial("ProfileNavbar"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("ChangePhoneNumber", "Manage")) { %>
		<%: Html.AntiForgeryToken() %>
		<div class="form-horizontal">
			<fieldset>
				<legend><%: Html.TextSnippet("Account/ChangePhoneNumber/ChangePhoneNumberFormHeading", defaultValue: ResourceManager.GetString("Change_Mobile_Phone_Defaulttext"), tagName: "span") %></legend>
				<%: Html.ValidationSummary(string.Empty, new {@class = "alert alert-block alert-danger"}) %>
				<div class="form-group">
					<label class="col-sm-2 control-label" for="Number"><%: Html.TextSnippet("Account/ChangePhoneNumber/PhoneNumberLabel", defaultValue: ResourceManager.GetString("Mobile_Phone_Defaulttext"), tagName: "span") %></label>
					<div class="col-sm-10">
						<%: Html.TextBoxFor(model => model.Number, new { @class = "form-control" }) %>
					</div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-2 col-sm-10">
						<% if (ViewBag.ShowRemoveButton) { %>
							<a class="btn btn-default pull-right"data-toggle="modal" data-target="#confirm-remove" href="#" title="<%: Html.SnippetLiteral("Account/ChangePhoneNumber/RemovePhoneNumberButtonText", "Remove") %>"><span class="fa fa-trash-o" aria-hidden="true"></span> <%: Html.SnippetLiteral("Account/ChangePhoneNumber/RemovePhoneNumberButtonText", "Remove") %></a>
							<div class="modal fade" id="confirm-remove" tabindex="-1" role="dialog" aria-labelledby="confirm-remove" aria-hidden="true">
								<div class="modal-dialog">
									<div class="modal-content">
										<div class="modal-header">
											<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
											<h4 class="modal-title" id="confirm-remove"><%: Html.TextSnippet("Account/ChangePhoneNumber/ConfirmRemoveModalHeading", defaultValue: ResourceManager.GetString("Confirm_Remove"), tagName: "span") %></h4>
										</div>
										<div class="modal-body">
											<%: Html.TextSnippet("Account/ChangePhoneNumber/ConfirmRemoveModalBody", defaultValue: ResourceManager.GetString("Remove_Mobile_Phone_Number_Message")) %>
										</div>
										<div class="modal-footer">
											<a class="btn btn-primary" href="<%: Url.Action("RemovePhoneNumber") %>" title="<%: Html.SnippetLiteral("Account/ChangePhoneNumber/RemovePhoneNumberButtonText", ResourceManager.GetString("Remove_DefaultText")) %>"><span class="fa fa-trash-o" aria-hidden="true"></span> <%: Html.SnippetLiteral("Account/ChangePhoneNumber/RemovePhoneNumberButtonText", ResourceManager.GetString("Remove_DefaultText")) %></a>
											<button type="button" class="btn btn-default" data-dismiss="modal"><%: Html.SnippetLiteral("Account/ChangePhoneNumber/ConfirmRemoveCancelButtonText", ResourceManager.GetString("Cancel_DefaultText")) %></button>
										</div>
									</div>
								</div>
							</div>
						<% } %>
						<button type="submit" class="btn btn-primary"><span class="fa fa-mobile" aria-hidden="true"></span> <%: Html.SnippetLiteral("Account/ChangePhoneNumber/ChangePhoneNumberButtonText", ResourceManager.GetString("Change_And_Confirm_Number")) %></button>
					</div>
				</div>
			</fieldset>
		</div>
	<% } %>
</asp:Content>
