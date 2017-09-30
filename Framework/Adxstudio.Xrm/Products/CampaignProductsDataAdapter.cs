/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Provides data operations on a set of products for a given campaign.
	/// </summary>
	public class CampaignProductsDataAdapter : IProductAggregationDataAdapter
	{
		internal enum ProductStateCode
		{
			Active = 0,
			Inactive = 1
		}

		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="campaign">Campaign Entity Reference</param>
		/// <param name="dependencies">Data Adapter Dependencies</param>
		public CampaignProductsDataAdapter(EntityReference campaign, IDataAdapterDependencies dependencies)
		{
			if (campaign == null) throw new ArgumentNullException("campaign");
			if (campaign.LogicalName != "campaign") throw new ArgumentException(string.Format(ResourceManager.GetString("Value_Missing_For_LogicalName"), campaign.LogicalName), "campaign");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Campaign = campaign;
			Dependencies = dependencies;
		}

		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="campaign">Campaign Entity</param>
		/// <param name="dependencies">Data Adapter Dependencies</param>
		public CampaignProductsDataAdapter(Entity campaign, IDataAdapterDependencies dependencies) : this(campaign.ToEntityReference(), dependencies) { }

		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="campaign">Campaign Entity Reference</param>
		/// <param name="portalName">Portal Name</param>
		public CampaignProductsDataAdapter(EntityReference campaign, string portalName = null) : this(campaign, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="campaign">Campaign Entity</param>
		/// <param name="portalName">Portal Name</param>
		public CampaignProductsDataAdapter(Entity campaign, string portalName = null) : this(campaign, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected IDataAdapterDependencies Dependencies { get; private set; }
		
		protected EntityReference Campaign { get; set; }

		public virtual IEnumerable<IProduct> SelectProducts()
		{
			return SelectProducts(0, 1);
		}

		public virtual IEnumerable<IProduct> SelectProducts(int startRowIndex, int maximumRows)
		{
			if (startRowIndex < 0)
			{
                throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IProduct[] { };
			}

			var serviceContext = Dependencies.GetServiceContext();

			var campaign = serviceContext.CreateQuery("campaign").FirstOrDefault(c => c.GetAttributeValue<Guid>("campaignid") == Campaign.Id &&
				c.GetAttributeValue<OptionSetValue>("statecode") != null && c.GetAttributeValue<OptionSetValue>("statecode").Value == (int)ProductDataAdapter.StateCode.Active);

			if (campaign == null)
			{
				return new IProduct[] { };
			}

			var query = campaign.GetRelatedEntities(serviceContext, "campaignproduct_association")
							.Where(p => p.GetAttributeValue<OptionSetValue>("statecode") != null && p.GetAttributeValue<OptionSetValue>("statecode").Value == (int)ProductDataAdapter.StateCode.Active);
			
			if (startRowIndex > 0)
			{
				query = query.Skip(startRowIndex);
			}

			if (maximumRows > 0)
			{
				query = query.Take(maximumRows);
			}
			
			return new ProductFactory(serviceContext, Dependencies.GetPortalUser(), Dependencies.GetWebsite()).Create(query);
		}

		/// <summary>
		/// Retrieve the product count
		/// </summary>
		/// <returns>Number of products</returns>
		public virtual int SelectProductCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			return serviceContext.FetchCampaignProductCount(Campaign.Id);
		}
	}
}
