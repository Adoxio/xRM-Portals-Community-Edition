<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/Profile.master" CodeBehind="OrderStatus.aspx.cs" Inherits="Site.Areas.Commerce.Pages.OrderStatus" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import namespace="Adxstudio.Xrm" %>
<%@ Import Namespace="Adxstudio.Xrm.Core" %>
<%@ Import Namespace="Microsoft.Xrm.Client" %>
<%@ Import Namespace="Adxstudio.Xrm.Notes" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Microsoft.Xrm.Portal.Core" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/css/webforms.css") %>" />
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Commerce/css/commerce.css") %>" />
</asp:Content>

<asp:Content ContentPlaceHolderID="Breadcrumbs" runat="server">
	<asp:PlaceHolder ID="OrderBreadcrumbs" runat="server">
		<ul class="breadcrumb">
			<% foreach (var node in Html.SiteMapPath()) { %>
				<% if (node.Item2 == SiteMapNodeType.Current) { %>
					<li class="active"><%: OrderToEdit.GetAttributeValue<string>("ordernumber") %></li>
				<% } else { %>
					<li>
						<a href="<%: node.Item1.Url %>" title="<%: node.Item1.Title %>"><%: node.Item1.Title %></a>
					</li>
				<% } %>
			<% } %>
		</ul>
	</asp:PlaceHolder>
	<asp:PlaceHolder ID="PageBreadcrumbs" Visible="False" runat="server">
		<% Html.RenderPartial("Breadcrumbs"); %>
	</asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<crm:CrmEntityDataSource ID="CurrentEntity" DataItem="<%$ CrmSiteMap: Current %>" runat="server" />
	<asp:Panel ID="OrderHeader" CssClass="page-header" runat="server">
		<section class="modal" id="add-note" tabindex="-1" role="dialog" aria-labelledby="add-note-modal-label" aria-hidden="true">
			<div class="modal-dialog">
				<div class="modal-content">
					<div class="modal-header">
						<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
						<h1 id="add-note-modal-label" class="modal-title h4">
							<adx:Snippet  runat="server" SnippetName="Ecommerce/Order/AddNote/ButtonText" DefaultText="<%$ ResourceManager:Add_Note %>" Editable="true" EditType="text"/>
						</h1>
					</div>
					<div class="modal-body form-horizontal">
						<div class="form-group">
							<asp:Label AssociatedControlID="NewNoteText" CssClass="col-sm-3 control-label" runat="server">
								<adx:Snippet runat="server" SnippetName="Ecommerce/Order/AddNote/Text" DefaultText="<%$ ResourceManager:Note_DefaultText %>" />
							</asp:Label>
							<div class="col-sm-9">
								<asp:TextBox runat="server" ID="NewNoteText" TextMode="MultiLine" Rows="6" CssClass="form-control"/>
							</div>
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="NewNoteAttachment" CssClass="col-sm-3 control-label" runat="server">
								<adx:Snippet runat="server" SnippetName="Ecommerce/Order/AddNote/File" DefaultText="<%$ ResourceManager:Attach_A_File_DefaultText %>" />
							</asp:Label>
							<div class="col-sm-9">
								<div class="form-control-static">
									<asp:FileUpload ID="NewNoteAttachment" runat="server"/>
								</div>
							</div>
						</div>
					</div>
					<div class="modal-footer">
						<asp:Button CssClass="btn btn-primary" OnClick="AddNote_Click" Text='<%$ Snippet: Ecommerce/Order/AddNote/ButtonText, Add Note %>' runat="server" />
						<button class="btn btn-default" data-dismiss="modal" aria-hidden="true">
							<adx:Snippet  runat="server" SnippetName="Ecommerce/Order/AddNote/CancelButtonText" DefaultText="<%$ ResourceManager:Cancel_DefaultText %>" Literal="True" EditType="text"/>
						</button>
					</div>
				</div>
			</div>
		</section>
		
		<asp:Panel ID="OrderControls" CssClass="pull-right btn-toolbar" runat="server">
			<asp:Panel ID="AddNote" CssClass="btn-group" runat="server">
				<a href="#add-note" class="btn btn-default" data-toggle="modal" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Add_Note") %>'>
					<span class="fa fa-plus-circle" aria-hidden="true"></span>
					<adx:Snippet runat="server" SnippetName="Ecommerce/Order/AddNote/ButtonText" DefaultText="<%$ ResourceManager:Add_Note %>" Literal="true" EditType="text"/>
				</a>
			</asp:Panel>
		</asp:Panel>
		<h1>
			<adx:Property DataSourceID="CurrentEntity" PropertyName="adx_title,adx_name" EditType="text" runat="server" />
			<small>
				<%: OrderToEdit.GetAttributeValue<string>("ordernumber") %>
			</small>
		</h1>
	</asp:Panel>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ViewStateMode="Enabled" runat="server">
	<asp:Panel ID="GenericError" Visible="False" CssClass="alert alert-block alert-danger" runat="server">
		<adx:Snippet SnippetName="Ecommerce/OrderGenericErrorMessage" DefaultText="<%$ ResourceManager:Error_Viewing_Order_Contact_Support %>" EditType="html" runat="server"/>
	</asp:Panel>
	
	<asp:Panel ID="UpdateSuccessMessage" runat="server" CssClass="alert alert-success alert-block" Visible="False">
		<a class="close" data-dismiss="alert" href="#" title="Close">&times;</a>
		<adx:Snippet runat="server" SnippetName="Ecommerce/OrderUpdateSuccessMessage" DefaultText="<%$ ResourceManager:Order_Updated_Successfully %>" Editable="true" EditType="html" />
	</asp:Panel>

	<asp:Panel ID="OrderInfo" runat="server">
		<div class="commerce-status clearfix">
			<div class="pull-right">
				<span class="<%: "label {0}".FormatWith(GetLabelClassForOrder(OrderToEdit)) %>"><%: XrmContext.GetOptionSetValueLabel("salesorder", "statuscode", OrderToEdit.GetAttributeValue<OptionSetValue>("statuscode").Value) %></span>
			</div>
			<span>
				<adx:Snippet SnippetName="Created Label" DefaultText="<%$ ResourceManager:Created_DefaultText %>" EditType="text" runat="server"/>
				<abbr class="timeago"><%: "{0:r}".FormatWith(OrderToEdit.GetAttributeValue<DateTime>("createdon")) %></abbr>
			</span>
		</div>
		
		<%: Html.HtmlAttribute("adx_copy", cssClass: "page-copy") %>
	</asp:Panel>

	<asp:Panel ID="OrderDetails" CssClass="commerce-details" runat="server" Visible="true">
		<asp:Panel ID="ShoppingCartSummary" CssClass="panel panel-default" runat="server">
			<ul class="list-group">
				<asp:Repeater ID="CartRepeater" runat="server">
					<ItemTemplate>
						<li class="list-group-item clearfix">
							<span class="badge">
								<asp:Label ID="Quantity" runat="server" Text='<%# ((Entity)Container.DataItem).GetAttributeValue<decimal?>("adx_quantity").GetValueOrDefault().ToString("N0") %>' />
								&times;
								<%# ((Entity)Container.DataItem).GetAttributeValue<Money>("adx_quotedprice") == null ? 0.ToString("C2") : ((Entity)Container.DataItem).GetAttributeValue<Money>("adx_quotedprice").Value.ToString("C2") %>
							</span>
							<div class="list-group-item-heading">
								<%# GetCartItemTitle(XrmContext, (Entity)Container.DataItem) %>
							</div>
							<asp:TextBox ReadOnly="True" ID="CartItemID" runat="server" Visible="false" Text='<%# ((Entity)Container.DataItem).GetAttributeValue<Guid>("adx_shoppingcartitemid") %>' />
						</li>
					</ItemTemplate>
				</asp:Repeater>
				<li class="list-group-item list-group-item-success">
					<asp:Label ID="Total" CssClass="badge" runat="server" />
					<adx:Snippet SnippetName="Total" DefaultText="<%$ ResourceManager:Total_DefaultText_Label %>" runat="server" EditType="text" />
				</li>
			</ul>
		</asp:Panel>
		
		<asp:Panel ID="Order" Visible="False" runat="server">
			<div class="well purchase-summary">
				<asp:ListView ID="OrderItems" runat="server">
					<LayoutTemplate>
						<ul class="list-unstyled">
							<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
						</ul>
					</LayoutTemplate>
					<ItemTemplate>
						<li class="line-item">
							<div class="pull-right">
								<asp:Label Visible='<%# ((decimal)Eval("Quantity")) > 1 %>' runat="server" >
									<asp:Label ID="Quantity" runat="server" Text='<%# Eval("Quantity", "{0:N0}") %>' />
									&times;
								</asp:Label>
								<%# Eval("PricePerUnit", "{0:C2}") %>
							</div>
							<div class="column title"><%# Eval("Name") %></div>
						</li>
					</ItemTemplate>
				</asp:ListView>
				<asp:Panel ID="OrderTotalTax" CssClass="total" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalTaxLabel" DefaultText="<%$ ResourceManager:Tax_DefaultText %>" runat="server" EditType="text" />
					<asp:Label ID="OrderTotalTaxAmount" runat="server"/>
				</asp:Panel>
				<asp:Panel ID="OrderTotalShipping" CssClass="total" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalShippingLabel" DefaultText="<%$ ResourceManager:Shipping_DefaultText %>" runat="server" EditType="text" />
					<asp:Label ID="OrderTotalShippingAmount" runat="server"/>
				</asp:Panel>
				<asp:Panel ID="OrderTotalDiscount" CssClass="total discount" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalDiscountLabel" DefaultText="<%$ ResourceManager:Discount_DefaultText %>" runat="server" EditType="text" />
					&minus;<asp:Label ID="OrderTotalDiscountAmount" runat="server"/>
				</asp:Panel>
				<asp:Panel ID="OrderTotal" CssClass="total" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalLabel" DefaultText="<%$ ResourceManager:Total_DefaultText %>" runat="server" EditType="text" />
					<asp:Label ID="OrderTotalAmount" runat="server"/>
				</asp:Panel>
			</div>
		</asp:Panel>

		<asp:Panel ID="Invoice" Visible="False" runat="server">
			<div class="well purchase-summary">
				<asp:ListView ID="InvoiceItems" runat="server">
					<LayoutTemplate>
						<ul class="list-unstyled">
							<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
						</ul>
					</LayoutTemplate>
					<ItemTemplate>
						<li class="line-item">
							<div class="pull-right">
								<asp:Label Visible='<%# ((decimal)Eval("Quantity")) > 1 %>' runat="server" >
									<asp:Label ID="Quantity" runat="server" Text='<%# Eval("Quantity", "{0:N0}") %>' />
									&times;
								</asp:Label>
								<%# Eval("PricePerUnit", "{0:C2}") %>
							</div>
							<div class="column title"><%# Eval("Name") %></div>
						</li>
					</ItemTemplate>
				</asp:ListView>
				<asp:Panel ID="InvoiceTotalTax" CssClass="total" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalTaxLabel" DefaultText="<%$ ResourceManager:Tax_DefaultText %>" runat="server" EditType="text" />
					<asp:Label ID="InvoiceTotalTaxAmount" runat="server"/>
				</asp:Panel>
				<asp:Panel ID="InvoiceTotalShipping" CssClass="total" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalShippingLabel" DefaultText="<%$ ResourceManager:Shipping_DefaultText %>" runat="server" EditType="text" />
					<asp:Label ID="InvoiceTotalShippingAmount" runat="server"/>
				</asp:Panel>
				<asp:Panel ID="InvoiceTotalDiscount" CssClass="total discount" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalDiscountLabel" DefaultText="<%$ ResourceManager:Discount_DefaultText %>" runat="server" EditType="text" />
					&minus;<asp:Label ID="InvoiceTotalDiscountAmount" runat="server"/>
				</asp:Panel>
				<asp:Panel ID="InvoiceTotal" CssClass="total" runat="server">
					<adx:Snippet SnippetName="Ecommerce/Purchase/TotalLabel" DefaultText="<%$ ResourceManager:Total_DefaultText %>" runat="server" EditType="text" />
					<asp:Label ID="InvoiceTotalAmount" runat="server"/>
				</asp:Panel>
			</div>
		</asp:Panel>

		<asp:Panel ID="OrderForm" runat="server">
			<adx:CrmEntityFormView CssClass="crmEntityFormView readonly" runat="server" ID="FormView" EntityName="salesorder" FormName="Order Web Form" OnItemUpdating="OnItemUpdating" OnItemUpdated="OnItemUpdated" ValidationGroup="Profile" RecommendedFieldsRequired="True" ShowUnsupportedFields="False" ToolTipEnabled="False" Mode="ReadOnly"
				SubmitButtonCssClass="btn btn-primary button submit"
				SubmitButtonText='<%$ Snippet: Ecommerce/Order/UpdateOrder, Update Order %>'
				DataBindOnPostBack="True"
				LanguageCode="<%$ SiteSetting: Language Code, 0 %>"
				ContextName="<%$ SiteSetting: Language Code %>">
			</adx:CrmEntityFormView>
		</asp:Panel>
		
		<div class="page-header">
			<h3>
				<adx:Snippet SnippetName="Ecommerce/OrderNotesLabel" DefaultText="<%$ ResourceManager:Order_Notes_DefaultText %>" runat="server" EditType="text" />
			</h3>
		</div>
		
		<asp:ListView ID="NotesList" runat="server">
			<LayoutTemplate>
				<asp:PlaceHolder ID="itemPlaceholder" runat="server"/>
			</LayoutTemplate>
			<ItemTemplate>
				<div class="note">
					<div class="row">
						<div class="col-sm-3 metadata">
							<abbr class="timeago"><%# ((IAnnotation)Container.DataItem).CreatedOn.ToString("r") %></abbr>
						</div>
						<div class="col-sm-9">
							<div class="text">
								<%# AnnotationHelper.FormatNoteText(((IAnnotation)Container.DataItem).NoteText) %>
							</div>
							<asp:Panel Visible='<%# ((IAnnotation)Container.DataItem).FileAttachment != null %>' CssClass="attachment alert alert-block alert-info" runat="server">
								<span class="fa fa-file" aria-hidden="true"></span>
								<asp:HyperLink NavigateUrl='<%#: ((IAnnotation)Container.DataItem).Entity.GetFileAttachmentUrl(Website) %>' Text='<%#: ((IAnnotation)Container.DataItem).FileAttachment != null ? string.Format("{0} ({1:1})", ((IAnnotation)Container.DataItem).FileAttachment.FileName, ((IAnnotation)Container.DataItem).FileAttachment.FileSize) : string.Empty %>' runat="server"/>
							</asp:Panel>
						</div>
					</div>
				</div>
			</ItemTemplate>
		</asp:ListView>
		<asp:Panel ID="AddNoteInline" CssClass="row" runat="server">
			<div class="col-sm-offset-3 col-sm-9">
				<a href="#add-note" class="btn btn-default" data-toggle="modal" title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Add_Note") %>'>
					<span class="fa fa-plus-circle" aria-hidden="true"></span>
					<adx:Snippet runat="server" SnippetName="Ecommerce/Order/AddNote/ButtonText" DefaultText="<%$ ResourceManager:Add_Note %>" Literal="true" EditType="text"/>
				</a>
			</div>
		</asp:Panel>
	</asp:Panel>
</asp:Content>