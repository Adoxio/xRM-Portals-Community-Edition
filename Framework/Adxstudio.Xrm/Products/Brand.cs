/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Core.Flighting;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Products
{
	internal class Brand : IBrand
	{
		public Brand(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			entity.AssertEntityName("adx_brand");

			Description = entity.GetAttributeValue<string>("adx_description");
			Id = entity.Id;
			Name = entity.GetAttributeValue<string>("adx_name");

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.ProductBrand, HttpContext.Current, "read_product_brand", 1, entity.ToEntityReference(), "read");
			}
		}

		public string Description { get; private set; }

		public Guid Id { get; private set; }

		public string Name { get; private set; }
	}
}
