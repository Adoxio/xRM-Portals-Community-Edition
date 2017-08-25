/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Products
{
	public class SubjectBrandDataAdapter : IBrandDataAdapter
	{
		public SubjectBrandDataAdapter(EntityReference subject, IDataAdapterDependencies dependencies)
		{
			if (subject == null) throw new ArgumentNullException("subject");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Subject = subject;
			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected EntityReference Subject { get; private set; }

		public IBrand SelectBrand(Guid id)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("id={0}: Start", id));

			var serviceContext = Dependencies.GetServiceContext();

			var query = from b in serviceContext.CreateQuery("adx_brand")
				join p in serviceContext.CreateQuery("product") on b.GetAttributeValue<Guid>("adx_brandid") equals p.GetAttributeValue<EntityReference>("adx_brand").Id
				where p.GetAttributeValue<EntityReference>("subjectid") == Subject
				where b.GetAttributeValue<Guid>("adx_brandid") == id
				select b;

			var entity = query.FirstOrDefault();

			if (entity == null)
			{
				return null;
			}

			var brand = new Brand(entity);

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return brand;
		}

		public IEnumerable<IBrand> SelectBrands()
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var serviceContext = Dependencies.GetServiceContext();

			var query = from brand in serviceContext.CreateQuery("adx_brand")
				join product in serviceContext.CreateQuery("product") on brand.GetAttributeValue<Guid>("adx_brandid") equals product.GetAttributeValue<EntityReference>("adx_brand").Id
				where product.GetAttributeValue<EntityReference>("subjectid") == Subject
				orderby brand.GetAttributeValue<string>("adx_name")
				select brand;

			var brands = query.Distinct().ToArray().Select(e => new Brand(e)).ToArray();

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return brands;
		}
	}
}
