/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Commerce
{
	public class ShoppingCartDataAdapter : IShoppingCartDataAdapter
	{
		public ShoppingCartDataAdapter(IDataAdapterDependencies dependencies, string vistorId = null)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Dependencies = dependencies;
			VistorId = vistorId;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected string VistorId { get; set; }

		public IShoppingCart SelectCart()
		{
			var serviceContext = Dependencies.GetServiceContextForWrite();
			var website = Dependencies.GetWebsite();
			var user = Dependencies.GetPortalUser();

			var entity = SelectCarts(serviceContext, website, user)
				.Where(e => e.GetAttributeValue<bool>("adx_system") == false)
				.OrderByDescending(e => e.GetAttributeValue<DateTime>("createdon"))
				.FirstOrDefault();

			return entity == null
				? null
				: new ShoppingCart(entity, serviceContext);
		}

		public IShoppingCart SelectCart(Guid id)
		{
			var serviceContext = Dependencies.GetServiceContextForWrite();
			var website = Dependencies.GetWebsite();

			var entity = SelectActiveCartsInWebsite(serviceContext, website)
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_shoppingcartid") == id);

			return entity == null
				? null
				: new ShoppingCart(entity, serviceContext);
		}

		public IShoppingCart CreateCart()
		{
			const string nameFormat = "Cart for {0}";
			var serviceContext = Dependencies.GetServiceContextForWrite();
			var website = Dependencies.GetWebsite();
			var user = Dependencies.GetPortalUser();
			Entity cart = null;

			if (user != null)
			{
				cart = new Entity("adx_shoppingcart");
				cart["adx_name"] = string.Format(nameFormat, user.Name);
				cart["adx_contactid"] = user;
				cart["adx_websiteid"] = website;
			}
			else
			{
				cart = new Entity("adx_shoppingcart");
				cart["adx_name"] = string.Format(nameFormat, VistorId);
				cart["adx_visitorid"] = VistorId;
				cart["adx_websiteid"] = website;
			}

			serviceContext.AddObject(cart);
			serviceContext.SaveChanges();

			return new ShoppingCart(cart, serviceContext);
		}

		protected IQueryable<Entity> SelectCarts(OrganizationServiceContext serviceContext, EntityReference website, EntityReference user)
		{
			if (user != null && user.LogicalName == "contact")
			{
				return SelectCartsByContact(serviceContext, website, user);
			}

			if (!string.IsNullOrEmpty(VistorId))
			{
				return SelectCartsByVisitorId(serviceContext, website, VistorId);
			}

			return Enumerable.Empty<Entity>().AsQueryable();
		}

		protected static IQueryable<Entity> SelectActiveCartsInWebsite(OrganizationServiceContext serviceContext, EntityReference website)
		{
			return serviceContext.CreateQuery("adx_shoppingcart")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website)
				.Where(e => e.GetAttributeValue<OptionSetValue>("statecode") != null && e.GetAttributeValue<OptionSetValue>("statecode").Value == 0);
		}

		protected static IQueryable<Entity> SelectCartsByContact(OrganizationServiceContext serviceContext, EntityReference website, EntityReference contact)
		{
			return SelectActiveCartsInWebsite(serviceContext, website)
				.Where(e => e.GetAttributeValue<EntityReference>("adx_contactid") != null && e.GetAttributeValue<EntityReference>("adx_contactid").Equals(contact));
		}

		protected static IQueryable<Entity> SelectCartsByVisitorId(OrganizationServiceContext serviceContext, EntityReference website, string vistorId)
		{
			return SelectActiveCartsInWebsite(serviceContext, website)
				.Where(e => e.GetAttributeValue<string>("adx_visitorid") == vistorId);
		}
	}
}
