/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Profile;
using System.Web.Routing;
using System.Web.Security;
using Adxstudio.Xrm;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Commerce;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.Commerce
{
	public class CommerceAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Commerce"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute("ShoppingCartStatus", "_services/commerce/{__portalScopeId__}/shopping-cart/status", new
			{
				controller = "ShoppingCart",
				action = "Status"
			});

			context.Routes.Add("PaymentHandler", new Route("{area}/commerce/payment", null, new RouteValueDictionary(new { area = "_services" }), new PaymentRouteHandler()));

			var portalAreaRegistrationState = context.State as IPortalAreaRegistrationState;

			if (portalAreaRegistrationState != null)
			{
				portalAreaRegistrationState.Profile_MigrateAnonymous += Profile_MigrateAnonymous;
			}
		}

		protected static string PriceListName
		{
			get
			{
				var portal = PortalCrmConfigurationManager.CreatePortalContext();
				var context = portal.ServiceContext;
				return (portal.User != null) ? context.GetPriceListNameForParentAccount(portal.User) : "Web";
			}
		}

		protected static void Profile_MigrateAnonymous(object sender, ProfileMigrateEventArgs e)
		{
			// transfer the anonyous shopping cart items to the authenticated shopping cart

			var visitorId = e.AnonymousID;
			var portal = PortalCrmConfigurationManager.CreatePortalContext();

			using (var context = PortalCrmConfigurationManager.CreateServiceContext())
			{
				if (!AdxstudioCrmConfigurationManager.TryAssertSolutionName(PortalSolutions.SolutionNames.CommerceSolutionName))
				{
                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Execution aborted. {0} has not been imported.", PortalSolutions.SolutionNames.CommerceSolutionName));
					return;
				}
				
				var website = context.CreateQuery("adx_website").First(ws => ws.GetAttributeValue<Guid>("adx_websiteid") == portal.Website.Id);
				//var visitorBaseCart = context.GetCartsForVisitor(visitorId, website).FirstOrDefault() as Adx_shoppingcart;
				var visitorCart = context.GetCartsForVisitor(visitorId, website).FirstOrDefault();

				if (visitorCart != null)
				{
					var contactCartBase = context.GetCartsForContact(portal.User, website).FirstOrDefault();

					var contactCart = contactCartBase != null ? new ShoppingCart((contactCartBase), context) : null;

					if (contactCart != null)
					{
						// merge the anonymous cart with the authenticated cart

						foreach (var item in visitorCart.GetRelatedEntities(context, new Relationship("adx_shoppingcart_shoppingcartitem")))
						{
							if (item.GetAttributeValue<EntityReference>("adx_productid") == null)
							{
								continue;
							}

							contactCart.AddProductToCart(item.GetAttributeValue<EntityReference>("adx_productid").Id, PriceListName, (int)item.GetAttributeValue<decimal>("adx_quantity"));
						}

						if (!context.IsAttached(visitorCart))
						{
							context.Attach(visitorCart);
						}

						context.DeleteObject(visitorCart);
					}
					else
					{
						// transfer the cart directly

						const string nameFormat = "Cart for {0}";

						var contact = portal.User;

						if (contact != null)
						{
							visitorCart.SetAttributeValue("adx_name", string.Format(nameFormat, contact.GetAttributeValue<string>("fullname")));
						}

						visitorCart.SetAttributeValue("adx_visitorid", string.Empty);
						visitorCart.SetAttributeValue("adx_contactid", portal.User.ToEntityReference());

						if (!context.IsAttached(visitorCart))
						{
							context.Attach(visitorCart);
						}

						context.UpdateObject(visitorCart);
					}

					if (!context.IsAttached(visitorCart))
					{
						context.Attach(visitorCart);
					}

					context.SaveChanges();
				}
			}

			AnonymousIdentificationModule.ClearAnonymousIdentifier();
		}
	}
}
