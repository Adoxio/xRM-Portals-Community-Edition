/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
#if AUTHORIZENET
using AuthorizeNet;
#endif
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Commerce
{
	public class AuthorizeNetHelper 
	{

		public AuthorizeNetHelper(IPortalContext xrm) 
		{

		}

#if AUTHORIZENET
		public static bool IsSIMValid(SIMResponse response, IPortalContext portal)
		{

			var context = portal.ServiceContext;

			var apiLogin = context.GetSiteSettingValueByName(portal.Website, "Ecommerce/Authorize.Net/ApiLogin");

			var merchantHash = context.GetSiteSettingValueByName(portal.Website, "Ecommerce/Authorize.Net/MerchantHash");

			//first order of business - validate that it was Auth.net that posted this using the
			//MD5 hash that was passed back to us
			var isValid = response.Validate(merchantHash, apiLogin);

			return isValid;
		}
#endif

		public static Entity GetSupportPlan(IPortalContext portal, Entity account, OrganizationServiceContext context, Entity contact, Entity supportRequest)
		{
			var shoppingCart =
				context.CreateQuery("adx_shoppingcart").FirstOrDefault(sc => sc.GetAttributeValue<Guid>("adx_shoppingcartid")
					== supportRequest.GetAttributeValue<EntityReference>("adx_shoppingcartid").Id);

			var myCart = new ShoppingCart(shoppingCart, context);

			var total = myCart.GetCartTotal();

			//lookup shopping cart item(s)?
			//for now we only care about ONE shopping cart item, since this is an "instant purchase" and there is no cart experience.
			var shoppingCartItem =
				context.CreateQuery("adx_shoppingcartitem").FirstOrDefault(sci => sci.GetAttributeValue<EntityReference>("adx_shoppingcartid").Id ==
					shoppingCart.GetAttributeValue<Guid>("adx_shoppingcartid"));

			var uom = context.CreateQuery("uom").FirstOrDefault(
				u => u.GetAttributeValue<Guid>("uomid") == shoppingCartItem.GetAttributeValue<EntityReference>("adx_uomid").Id);

			var supportRequestProduct = supportRequest.GetAttributeValue<EntityReference>("adx_product");

			Entity product = null;

			if (supportRequestProduct != null)
			{
				product = context.CreateQuery("product").FirstOrDefault(p => p.GetAttributeValue<Guid>("productid") == supportRequestProduct.Id);	
			}
			
			var supportPlan = CreateSupportPlan(total, portal, context, uom, contact, product, account);

			return supportPlan;
		}

		public static Entity CreateSupportPlan(decimal total, IPortalContext portal, OrganizationServiceContext context,
			Entity uom, Entity contact, Entity product, Entity account)
		{
			var supportPlanId = Guid.NewGuid();

			var supportPlan = new Entity("adx_supportplan") { Id = supportPlanId };

			supportPlan.Attributes["adx_supportplanid"] = supportPlanId;

			var siteSettingStringFormat = context.GetSiteSettingValueByName(portal.Website, "HelpDesk/SupportPlanNameFormat");
			var contactName = string.Empty;
			var accountName = string.Empty;

			if (account != null)
			{
				supportPlan.Attributes["adx_customer"] = account.ToEntityReference();
				supportPlan.Attributes["adx_billtocustomer"] = account.ToEntityReference();
				accountName = account.GetAttributeValue<string>("name");
			}
			else if (contact != null)
			{
				supportPlan.Attributes["adx_customercontact"] = contact.ToEntityReference();
				contactName = contact.GetAttributeValue<string>("fullname");
			}

			supportPlan.Attributes["adx_name"] = !string.IsNullOrWhiteSpace(siteSettingStringFormat)
															? string.Format(siteSettingStringFormat,
																			accountName,
																			contactName,
																			DateTime.UtcNow)
															: string.Format(ResourceManager.GetString("Support_Plan_For_Purchased"),
																			accountName,
																			contactName,
																			DateTime.UtcNow);

			supportPlan.Attributes["adx_startdate"] = DateTime.UtcNow;

			supportPlan.Attributes["adx_enddate"] = DateTime.UtcNow.AddYears(1);

			if (product != null)
			{
				supportPlan.Attributes["adx_product"] = product.ToEntityReference();
			}

			supportPlan.Attributes["adx_totalprice"] = new Money(total);

			if (uom != null)
			{
				supportPlan.Attributes["adx_allotmentsused"] = 0;
				supportPlan.Attributes["adx_allotmentsissued"] = (int)uom.GetAttributeValue<decimal>("quantity");
				supportPlan.Attributes["adx_allotmentsremaining"] = (int)uom.GetAttributeValue<decimal>("quantity");
			}

			try
			{
				context.AddObject(supportPlan);

				context.SaveChanges();
			}
			catch
			{

			}

			supportPlan = context.CreateQuery("adx_supportplan")
				.FirstOrDefault(sr => sr.GetAttributeValue<Guid>("adx_supportplanid") == supportPlanId);
			return supportPlan;
		}

		public static void UpdateSupportRequest(out Entity supportRequest, Entity supportPlan, Guid supportRequestId,
			OrganizationServiceContext context)
		{

			supportRequest = context.CreateQuery("adx_supportrequest")
				.FirstOrDefault(sr => sr.GetAttributeValue<Guid>("adx_supportrequestid") == supportRequestId);

			supportRequest.Attributes["adx_supportplan"] = supportPlan.ToEntityReference();

			context.UpdateObject(supportRequest);

			context.SaveChanges();
		}

		public static bool TryCreateOrder(NameValueCollection values, 
			IPortalContext xrm, 
			Entity account, 
			string tombstoneEntityLogicalName = null, 
			string tombstoneEntityPrimaryKeyName = null)
		{
			var dict = ToDictionary(values);

			var order = CreateOrder(dict, xrm, account, tombstoneEntityLogicalName, tombstoneEntityPrimaryKeyName);

			return order != null && order.Entity != null;
		}

		public static CommerceOrder CreateOrder(Dictionary<string, string> values, 
			IPortalContext xrm, 
			Entity account, 
			string tombstoneEntityLogicalName = null, 
			string tombstoneEntityPrimaryKeyName = null)
		{
			var newOrder = new CommerceOrder(values, xrm, "Authorize.Net", GetCreateInvoiceSettingValue(xrm), account, tombstoneEntityLogicalName, tombstoneEntityPrimaryKeyName);

			return newOrder;
		}


		public static Dictionary<string, string> ToDictionary(NameValueCollection source)
		{
			return source.Cast<string>().Select(s => new { Key = s, Value = source[s] }).ToDictionary(p => p.Key, p => p.Value);
		}

		public static bool GetCreateInvoiceSettingValue(IPortalContext xrm)
		{
			var website = xrm.Website;

			var createInvoiceSetting = xrm.ServiceContext.CreateQuery("adx_sitesetting")
				.Where(ss => ss.GetAttributeValue<EntityReference>("adx_websiteid").Id == website.GetAttributeValue<Guid>("adx_websiteid"))
				.FirstOrDefault(purl => purl.GetAttributeValue<string>("adx_name") == "Ecommerce/CreateInvoiceOnVerification");

			if (createInvoiceSetting == null)
			{
				return true;
			}

			return bool.Parse(createInvoiceSetting.GetAttributeValue<string>("adx_value"));
		}
	}
}
