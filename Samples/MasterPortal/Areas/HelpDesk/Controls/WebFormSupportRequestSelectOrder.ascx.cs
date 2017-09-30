/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Commerce;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Site.Controls;

namespace Site.Areas.HelpDesk.Controls
{
	public partial class WebFormSupportRequestSelectOrder : WebFormPortalUserControl
	{
		private const string DefaultPriceListName = "Web";

		protected string VisitorID
		{
			get { return Context.Profile.UserName; }
		}

		public string PriceListName
		{
			get
			{
				var priceListName = DefaultPriceListName;

				if (Contact != null)
				{
					var accountPriceListName = ServiceContext.GetPriceListNameForParentAccount(Contact);

					if (string.IsNullOrWhiteSpace(accountPriceListName))
					{
						var websitePriceListName = ServiceContext.GetDefaultPriceListName(Website);

						if (!string.IsNullOrWhiteSpace(websitePriceListName))
						{
							priceListName = websitePriceListName;
						}
					}
					else
					{
						priceListName = accountPriceListName;
					}

				}

				return priceListName;
			}
		}

		private Guid _sessionId;

		public Guid SessionId
		{
			get
			{
				if (!Guid.TryParse(Request["sessionid"], out _sessionId))
				{
					return Guid.Empty;
				}
				return _sessionId;
			}
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			PlanPackageList.ValidationGroup = PlanPackageListRequiredFieldValidator.ValidationGroup = ValidationGroup;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				PopulatePlanPackageList();
			}
		}

		private void PopulatePlanPackageList()
		{
			PlanPackageList.Items.Clear();

			var supportRequest = XrmContext.CreateQuery("adx_supportrequest")
				.FirstOrDefault(sr => sr.GetAttributeValue<Guid>("adx_supportrequestid") == CurrentStepEntityID);
			if (supportRequest == null)
			{
				throw new ApplicationException(string.Format("Couldn't find support request record with id equal to {0}.", CurrentStepEntityID));
			}
			var supportRequestProductReference = supportRequest.GetAttributeValue<EntityReference>("adx_product");
			var parentProduct = supportRequestProductReference == null ? null : XrmContext.CreateQuery("product").FirstOrDefault(pp => pp.GetAttributeValue<Guid>("productid") == supportRequestProductReference.Id);
			IQueryable<Entity> supportProducts;
			if (parentProduct != null)
			{
				supportProducts = from product in XrmContext.CreateQuery("product")
								join supportedset in XrmContext.CreateQuery("adx_supportedproduct_productplan") on product.GetAttributeValue<Guid>("productid") equals supportedset.GetAttributeValue<Guid>("productidone")
								join supportedProduct in XrmContext.CreateQuery("product") on supportedset.GetAttributeValue<Guid>("productidtwo") equals supportedProduct.GetAttributeValue<Guid>("productid")
								where product.GetAttributeValue<OptionSetValue>("statecode") != null && product.GetAttributeValue<OptionSetValue>("statecode").Value == 0
								where product.GetAttributeValue<Guid>("productid") == parentProduct.GetAttributeValue<Guid>("productid")
								where supportedProduct.GetAttributeValue<OptionSetValue>("producttypecode") != null && supportedProduct.GetAttributeValue<OptionSetValue>("producttypecode").Value == (int)ProductTypeCode.SupportPlan
								select supportedProduct;
			}
			else
			{
				supportProducts = XrmContext.CreateQuery("product").Where(p => p.GetAttributeValue<OptionSetValue>("statecode") != null && p.GetAttributeValue<OptionSetValue>("statecode").Value == 0 && p.GetAttributeValue<OptionSetValue>("producttypecode") != null && p.GetAttributeValue<OptionSetValue>("producttypecode").Value == (int)ProductTypeCode.SupportPlan);
			}
			
			foreach (var supportProduct in supportProducts)
			{
				var supportUnit =
					XrmContext.CreateQuery("uomschedule").FirstOrDefault(unit => unit.GetAttributeValue<Guid>("uomscheduleid") == (supportProduct.GetAttributeValue<EntityReference>("defaultuomscheduleid") == null ? Guid.Empty : supportProduct.GetAttributeValue<EntityReference>("defaultuomscheduleid").Id));

				var baseUom = XrmContext.CreateQuery("uom").FirstOrDefault(baseuom => baseuom.GetAttributeValue<string>("name") == supportUnit.GetAttributeValue<string>("baseuomname"));

				var uoms = XrmContext.CreateQuery("uom").Where(uom => uom.GetAttributeValue<Guid>("baseuom") == baseUom.Id);

				foreach (var u in uoms)
				{
					var amount = new Money(0);
					var priceListItem = XrmContext.GetPriceListItemByPriceListNameAndUom(supportProduct.GetAttributeValue<Guid>("productid"), u.Id, PriceListName);
					
					if (priceListItem != null)
					{
						amount = priceListItem.GetAttributeValue<Money>("amount");
					}

					PlanPackageList.Items.Add(new ListItem(
												string.Format("{0} - {1} - {2}", supportProduct.GetAttributeValue<string>("name"), u.GetAttributeValue<string>("name"), amount.Value.ToString("c0")),
												string.Format("{0}&{1}", supportProduct.GetAttributeValue<Guid>("productid"), u.GetAttributeValue<Guid>("uomid"))));
				}
			}
		}

		protected override void OnSubmit(object sender, Adxstudio.Xrm.Web.UI.WebControls.WebFormSubmitEventArgs e)
		{
			AddToCart();

			base.OnSubmit(sender, e);
		}

		protected void AddToCart()
		{
			const string nameFormat = "Case Order for {0}";

			var cart = new Entity("adx_shoppingcart");

			cart.SetAttributeValue("adx_websiteid", Website.ToEntityReference());
			cart.SetAttributeValue("adx_system", true);

			if (Contact == null)
			{
				cart.SetAttributeValue("adx_name", string.Format(nameFormat, VisitorID));
				cart.SetAttributeValue("adx_visitorid", VisitorID);
			}
			else
			{
				cart.SetAttributeValue("adx_name", string.Format(nameFormat, Contact.GetAttributeValue<string>("fullname")));
				cart.SetAttributeValue("adx_contactid", Contact.ToEntityReference());
			}

			XrmContext.AddObject(cart);
			XrmContext.SaveChanges();

			// Choose Parent Product

			var selectedValue = PlanPackageList.SelectedValue;
			var guids = selectedValue.Split('&');
			var supportProductId = new Guid(guids[0]);
			var uomId = new Guid(guids[1]);

			var caseProduct = XrmContext.CreateQuery("product")
				.FirstOrDefault(p => p.GetAttributeValue<Guid>("productid") == supportProductId);

			var myUom = XrmContext.CreateQuery("uom")
				.FirstOrDefault(uom => uom.GetAttributeValue<Guid>("uomid") == uomId);
			
			var productId = caseProduct != null ? caseProduct.Id : Guid.Empty;

			cart = XrmContext.CreateQuery("adx_shoppingcart")
				.FirstOrDefault(sc => sc.GetAttributeValue<Guid>("adx_shoppingcartid") == cart.Id);

			var myCart = cart == null ? null : new ShoppingCart(cart, XrmContext);

			if (myCart == null)
			{
				throw new ApplicationException("Unable to retrieve the case purchase shopping cart.");
			}

			myCart.AddProductToCart(productId, myUom, PriceListName);

			var total = myCart.GetCartTotal();

			if (total < 0)
			{
				throw new ApplicationException("Case purchase shopping cart cannot have a sub-zero total value.");
			}

			AddCartToSupportRequest(myCart);
		}

		private void AddCartToSupportRequest(ShoppingCart myCart)
		{
			var context = new CrmOrganizationServiceContext(new CrmConnection("Xrm"));

			var supportRequest = new Entity("adx_supportrequest") { Id = CurrentStepEntityID };

			supportRequest.Attributes["adx_shoppingcartid"] = myCart.Entity.ToEntityReference();

			context.Attach(supportRequest);

			context.UpdateObject(supportRequest);

			context.SaveChanges();
		}
	}
}
