/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Adxstudio.Xrm;
using Adxstudio.Xrm.Commerce;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Text;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Site.Pages;
using PortalConfigurationDataAdapterDependencies = Adxstudio.Xrm.Cms.PortalConfigurationDataAdapterDependencies;

namespace Site.Areas.Commerce.Pages
{
	public partial class QuoteStatus : PortalPage
	{
		private CommerceQuote _quote;

		protected CommerceQuote QuoteToEdit
		{
			get
			{
				if (_quote != null)
				{
					return _quote;
				}

				Guid quoteId;

				if (!Guid.TryParse(Request["QuoteID"], out quoteId))
				{
					return null;
				}

				var myQuote = XrmContext.CreateQuery("quote").FirstOrDefault(c => c.GetAttributeValue<Guid>("quoteid") == quoteId);

				_quote = (myQuote != null) ? new CommerceQuote(myQuote, XrmContext) : null;

				return _quote;
			}
		}

		protected string QuoteStatusLabel
		{
			get { return XrmContext.GetOptionSetValueLabel("quote", "statuscode", QuoteToEdit.Entity.GetAttributeValue<OptionSetValue>("statuscode")); }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			var quoteToEditEntity = QuoteToEdit != null ? QuoteToEdit.Entity : null;

			if (quoteToEditEntity == null || (quoteToEditEntity.GetAttributeValue<EntityReference>("customerid") != null && !quoteToEditEntity.GetAttributeValue<EntityReference>("customerid").Equals(Contact.ToEntityReference())))
			{
				PageBreadcrumbs.Visible = true;
				GenericError.Visible = true;
				QuoteHeader.Visible = false;
				QuoteDetails.Visible = false;
				QuoteInfo.Visible = false;
				QuoteBreadcrumbs.Visible = false;
				QuoteHeader.Visible = false;

				return;
			}
			
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: PortalName);
			var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
			var annotations = dataAdapter.GetAnnotations(QuoteToEdit.Entity.ToEntityReference(),
				new List<Order> { new Order("createdon") }, respectPermissions: false);

			NotesList.DataSource = annotations;
			NotesList.DataBind();

			var quoteState = quoteToEditEntity.GetAttributeValue<OptionSetValue>("statecode");

			ConvertToOrder.Visible = quoteState != null
				&& (quoteState.Value == (int)Enums.QuoteState.Active || quoteState.Value == (int)Enums.QuoteState.Won);

			var formViewDataSource = new CrmDataSource { ID = "WebFormDataSource", CrmDataContextName = FormView.ContextName };

			var fetchXml = string.Format("<fetch mapping='logical'><entity name='{0}'><all-attributes /><filter type='and'><condition attribute = '{1}' operator='eq' value='{{{2}}}'/></filter></entity></fetch>", "quote", "quoteid", QuoteToEdit.Id);

			formViewDataSource.FetchXml = fetchXml;

			QuoteForm.Controls.Add(formViewDataSource);

			FormView.DataSourceID = "WebFormDataSource";

			var baseCartReference = quoteToEditEntity.GetAttributeValue<EntityReference>("adx_shoppingcartid");

			var baseCart = (baseCartReference != null) ? ServiceContext.CreateQuery("adx_shoppingcart").FirstOrDefault(sc => sc.GetAttributeValue<Guid>("adx_shoppingcartid") == baseCartReference.Id)
				: null;

			var cartRecord = baseCart == null ? null : new ShoppingCart(baseCart, XrmContext);

			if (cartRecord == null)
			{
				ShoppingCartSummary.Visible = false;

				return;
			}

			var cartItems = cartRecord.GetCartItems().Select(sci => sci.Entity);

			if (!cartItems.Any())
			{
				ShoppingCartSummary.Visible = false;

				return;
			}

			CartRepeater.DataSource = cartItems;
			CartRepeater.DataBind();

			Total.Text = cartRecord.GetCartTotal().ToString("C2");
		}

		protected string GetCartItemTitle(OrganizationServiceContext context, Entity item)
		{
			var product = item.GetRelatedEntity(context, new Relationship("adx_product_shoppingcartitem"));

			return product.GetAttributeValue<string>("name");
		}

		protected void OnItemUpdating(object sender, CrmEntityFormViewUpdatingEventArgs e)
		{
		}

		protected void OnItemUpdated(object sender, CrmEntityFormViewUpdatedEventArgs e)
		{
			UpdateSuccessMessage.Visible = true;
		}

		protected void AddNote_Click(object sender, EventArgs e)
		{
			if (QuoteToEdit == null || (QuoteToEdit.Entity.GetAttributeValue<EntityReference>("customerid") != null && !QuoteToEdit.Entity.GetAttributeValue<EntityReference>("customerid").Equals(Contact.ToEntityReference())))
			{
				throw new InvalidOperationException("Unable to retrieve the quote.");
			}

			if (!string.IsNullOrEmpty(NewNoteText.Text) || (NewNoteAttachment.PostedFile != null && NewNoteAttachment.PostedFile.ContentLength > 0))
			{
				var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(
					requestContext: Request.RequestContext, portalName: PortalName);

				var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
				
				var annotation = new Annotation
				{
					NoteText = string.Format("{0}{1}", AnnotationHelper.WebAnnotationPrefix, NewNoteText.Text),
					Subject = AnnotationHelper.BuildNoteSubject(dataAdapterDependencies),
					Regarding = QuoteToEdit.Entity.ToEntityReference()
				};
				if (NewNoteAttachment.PostedFile != null && NewNoteAttachment.PostedFile.ContentLength > 0)
				{
					annotation.FileAttachment = AnnotationDataAdapter.CreateFileAttachment(new HttpPostedFileWrapper(NewNoteAttachment.PostedFile));
				}
				dataAdapter.CreateAnnotation(annotation);
			}

			Response.Redirect(Request.Url.PathAndQuery);
		}

		protected void ConvertToOrder_Click(object sender, EventArgs args)
		{
			var quoteToEditEntity = QuoteToEdit != null ? QuoteToEdit.Entity : null;

			if (quoteToEditEntity == null || (quoteToEditEntity.GetAttributeValue<EntityReference>("customerid") != null && !quoteToEditEntity.GetAttributeValue<EntityReference>("customerid").Equals(Contact.ToEntityReference())))
			{
				throw new InvalidOperationException("Unable to retrieve the quote.");
			}

			Entity order;
			
			try
			{
				order = QuoteToEdit.CreateOrder();

				if (order == null)
				{
					ConvertToOrderError.Visible = true;

					return;
				}
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());

                ConvertToOrderError.Visible = true;

				return;
			}

			var orderid = order.GetAttributeValue<Guid>("salesorderid");

			if (!ServiceContext.IsAttached(Website))
			{
				ServiceContext.Attach(Website);
			}

			var page = ServiceContext.GetPageBySiteMarkerName(Website, "View Order") ?? ServiceContext.GetPageBySiteMarkerName(Website, "Order Status");

			if (page == null)
			{
				return;
			}

			var url = new UrlBuilder(ServiceContext.GetUrl(page));

			url.QueryString.Set("OrderID", orderid.ToString());

			Response.Redirect(url.PathWithQueryString);
		}

		protected string GetLabelClassForQuote(CommerceQuote quote)
		{
			var statusCodeOption = quote.Entity.GetAttributeValue<OptionSetValue>("statuscode");

			if (statusCodeOption == null)
			{
				return null;
			}

			switch (statusCodeOption.Value)
			{
				case (int)Enums.QuoteStatusCode.Canceled:
				case (int)Enums.QuoteStatusCode.Lost:
					return "label-danger";
				case (int)Enums.QuoteStatusCode.Draft:
				case (int)Enums.QuoteStatusCode.New:
				case (int)Enums.QuoteStatusCode.Open:
					return "label-info";
				case (int)Enums.QuoteStatusCode.Won:
					return "label-success";
				case (int)Enums.QuoteStatusCode.Revised:
					return "label-warning";
				default:
					return "label-default";
			}
		}
	}
}
