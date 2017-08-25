/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.EntityList.OData;
using Microsoft.Data.Edm;

namespace Site.Areas.EntityList.Controllers
{
	public class ODataEntitySetController : ODataController
	{
		public EdmEntityObjectCollection Get()
		{
			var path = Request.ODataProperties().Path;
			var edmType = path.EdmType;
			var collectionType = edmType as IEdmCollectionType;
			
			if (edmType.TypeKind != EdmTypeKind.Collection || collectionType == null)
			{				
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("EdmType.TypeKind is not valid."))); 
           	}

			var entityType = collectionType.ElementType.AsEntity();
			var entitySetName = entityType.EntityDefinition().Name;
			var model = Request.ODataProperties().Model;
			var dataAdapter = new EntityListODataFeedDataAdapter(new PortalConfigurationDataAdapterDependencies());
			var pageSize = dataAdapter.GetPageSize(model, entitySetName);
			var queryContext = new ODataQueryContext(Request.ODataProperties().Model, entityType.Definition);
			var queryOptions = new ODataQueryOptions(queryContext, Request);
			var querySettings = new ODataQuerySettings { PageSize = pageSize };
			
			// http://www.asp.net/web-api/overview/odata-support-in-aspnet-web-api/odata-security-guidance
			var validationSettings = new ODataValidationSettings
			{
				AllowedFunctions = AllowedFunctions.EndsWith | AllowedFunctions.StartsWith | AllowedFunctions.SubstringOf,
				AllowedQueryOptions = AllowedQueryOptions.All & ~AllowedQueryOptions.Expand & ~AllowedQueryOptions.Select & ~AllowedQueryOptions.SkipToken,
				MaxNodeCount = 100,
				MaxTop = pageSize
			};
			
			queryOptions.Validate(validationSettings);

			return dataAdapter.SelectMultiple(model, entitySetName, queryOptions, querySettings, Request);
		}

		public IEdmEntityObject Get([FromODataUri] Guid key)
		{
			var path = Request.ODataProperties().Path;
			var entityType = path.EdmType as IEdmEntityType;
			var entitySetName = entityType == null ? string.Empty : entityType.Name;
			var model = Request.ODataProperties().Model;
			var dataAdapter = new EntityListODataFeedDataAdapter(new PortalConfigurationDataAdapterDependencies());
			var entity = dataAdapter.Select(model, entitySetName, key);

			if (entity == null)
			{
				throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("{0} couldn't be found with key {1}.", entitySetName, key)));
			}

			return entity;
		}
	}
}
