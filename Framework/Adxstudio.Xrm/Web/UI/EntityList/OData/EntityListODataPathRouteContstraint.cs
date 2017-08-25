/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;
using Adxstudio.Xrm.Resources;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;

namespace Adxstudio.Xrm.Web.UI.EntityList.OData
{
	/// <summary>
	/// An implementation of <see cref="T:System.Web.Http.OData.Routing.ODataPathRouteConstraint"/> that only matches OData paths.
	/// </summary>
	/// <remarks>ODataPathRouteConstraint requires an IEdmModel but for us to build our model dynamically at runtime, we require a http request that is not available at the time of route registration and in order to get the current website to filter the entity list records that will be used to build the model we defer to call to match the route. Initially we set it to an empty model and then during the match we build the model the first time.</remarks>
	public class EntityListODataPathRouteConstraint : ODataPathRouteConstraint
	{
		/// <summary>
		/// Gets the EDM model to use for parsing the path.
		/// </summary>
		public new IEdmModel EdmModel { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Web.Http.OData.Routing.ODataPathRouteConstraint"/> class.
		/// </summary>
		/// <param name="pathHandler">The OData path handler to use for parsing.</param><param name="routeName">The name of the route this constraint is associated with.</param><param name="routingConventions">The OData routing conventions to use for selecting the controller name.</param>
		public EntityListODataPathRouteConstraint(IODataPathHandler pathHandler, string routeName,
												IEnumerable<IODataRoutingConvention> routingConventions) : base(pathHandler, CreateEmptyModel(), routeName, routingConventions)
		{
			EdmModel = CreateEmptyModel();
		}

		private static IEdmModel CreateEmptyModel()
		{
			var model = new EdmModel();
			var container = new EdmEntityContainer(string.Empty, string.Empty);
			model.AddElement(container);
			model.SetIsDefaultEntityContainer(container, true);
			return model;
		}

		/// <summary>
		/// Determines whether this instance equals a specified route.
		/// </summary>
		/// 
		/// <returns>
		/// True if this instance equals a specified route; otherwise, false.
		/// </returns>
		/// <param name="request">The request.</param><param name="route">The route to compare.</param><param name="parameterName">The name of the parameter.</param><param name="values">A list of parameter values.</param><param name="routeDirection">The route direction.</param>
		public override bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
		{
			if (request == null)
				throw new ArgumentNullException("request");
			if (values == null)
				throw new ArgumentNullException("values");
			if (routeDirection != HttpRouteDirection.UriResolution)
				return true;
			object obj;
			if (values.TryGetValue(ODataRouteConstants.ODataPath, out obj))
			{
				var entitylistODataFeedDataAdapter = new EntityListODataFeedDataAdapter(new PortalConfigurationDataAdapterDependencies());
				EdmModel = entitylistODataFeedDataAdapter.GetEdmModel();
				
				string odataPath1 = obj as string ?? string.Empty;
				ODataPath odataPath2;
				try
				{
					odataPath2 = PathHandler.Parse(EdmModel, odataPath1);
				}
				catch (ODataException ex)
				{
					throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.NotFound, "The OData path is invalid.", ex));
				}
				if (odataPath2 != null)
				{
					var odataProperties = request.ODataProperties();
					odataProperties.Model = EdmModel;
					odataProperties.PathHandler = PathHandler;
					odataProperties.Path = odataPath2;
					odataProperties.RouteName = RouteName;
					odataProperties.RoutingConventions = RoutingConventions;
					if (!values.ContainsKey(ODataRouteConstants.Controller))
					{
						string str = SelectControllerName(odataPath2, request);
						if (str != null)
							values[ODataRouteConstants.Controller] = str;
					}
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Selects the name of the controller to dispatch the request to.
		/// </summary>
		/// 
		/// <returns>
		/// The name of the controller to dispatch to, or null if one cannot be resolved.
		/// </returns>
		/// <param name="path">The OData path of the request.</param><param name="request">The request.</param>
		protected override string SelectControllerName(ODataPath path, HttpRequestMessage request)
		{
			foreach (IODataRoutingConvention routingConvention in RoutingConventions)
			{
				string str = routingConvention.SelectController(path, request);
				if (str != null)
					return str;
			}
			return null;
		}
	}
}
