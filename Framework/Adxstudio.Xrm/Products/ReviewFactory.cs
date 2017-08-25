/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Products
{
	public class ReviewFactory
	{
		private readonly EntityReference _portalUser;
		private readonly EntityReference _website;
		private readonly OrganizationServiceContext _serviceContext;

		public ReviewFactory(OrganizationServiceContext serviceContext, EntityReference portalUser, EntityReference website)
		{
			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			_serviceContext = serviceContext;
			_portalUser = portalUser;
			_website = website;
		}

		public IEnumerable<IReview> Create(IEnumerable<Entity> reviewEntities)
		{
			var reviews = reviewEntities.ToArray();

			return reviews.Select(e => new Review(e)).ToArray();
		}
	}
}
