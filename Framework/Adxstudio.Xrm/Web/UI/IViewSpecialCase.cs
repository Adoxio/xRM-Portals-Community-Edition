/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.UI
{
	/// <summary>
	/// Represents a special case for a <see cref="ViewDataAdapter"/>-driven view, applying
	/// modifications to the view FetchXML if applicable.
	/// </summary>
	internal interface IViewSpecialCase
	{
		bool IsApplicable(IViewConfiguration configuration);

		bool TryApply(IViewConfiguration configuration, IDataAdapterDependencies dependencies, IDictionary<string, string> customParameters, Fetch fetch);
	}

	internal abstract class OpportunityPriceListSpecialCase : IViewSpecialCase
	{
		public abstract bool IsApplicable(IViewConfiguration configuration);

		public abstract bool TryApply(IViewConfiguration configuration, IDataAdapterDependencies dependencies, IDictionary<string, string> customParameters, Fetch fetch);

		protected Guid? GetPriceListId(IDataAdapterDependencies dependencies, Guid opportunityId)
		{
			var serviceContext = dependencies.GetServiceContext();

			var opportunityRetrieveResponse = serviceContext.Execute<RetrieveResponse>(new RetrieveRequest
			{
				Target = new EntityReference("opportunity", opportunityId),
				ColumnSet = new ColumnSet("pricelevelid")
			});

			var opportunityPriceLevel = opportunityRetrieveResponse.Entity.GetAttributeValue<EntityReference>("pricelevelid");

			return opportunityPriceLevel == null
				? null
				: new Guid?(opportunityPriceLevel.Id);
		}
	}
}
