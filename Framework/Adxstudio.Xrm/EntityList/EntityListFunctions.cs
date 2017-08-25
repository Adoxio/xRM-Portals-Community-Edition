/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.EntityList
{
	internal class EntityListFunctions
	{
		/// <summary>
		/// Retrieve the Standard Name for the specified time zone code from the timezonedefinition table.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="timeZoneCode"></param>
		/// <returns></returns>
		internal static string GetTimeZoneStandardName(OrganizationServiceContext serviceContext, int timeZoneCode)
		{
			var timeZoneDefinition = serviceContext.CreateQuery("timezonedefinition")
				.Where(e => e.GetAttributeValue<int?>("timezonecode") == timeZoneCode)
				.Select(e => new { StandardName = e.GetAttributeValue<string>("standardname") })
				.ToArray()
				.FirstOrDefault();

			if (timeZoneDefinition == null || string.IsNullOrEmpty(timeZoneDefinition.StandardName))
			{
				return null;
			}

			return timeZoneDefinition.StandardName;
		}

		/// <summary>
		/// Gets the URL for the specified web page.
		/// </summary>
		/// <param name="webpage"></param>
		/// <returns></returns>
		internal static UrlBuilder GetUrlForWebPage(OrganizationServiceContext serviceContext, EntityReference webpage, string portalName)
		{
			var page = serviceContext.RetrieveSingle(webpage.LogicalName,
				FetchAttribute.None,
				new Condition("adx_webpageid", ConditionOperator.Equal, webpage.Id));

			if (page == null) return null;

			var path = serviceContext.GetUrl(page);

			return path == null ? null : new UrlBuilder(path);
		}

		/// <summary>
		/// Generates a URL to a controller action
		/// </summary>
		internal static string BuildControllerActionUrl(string actionName, string controllerName, object routeValues)
		{
			var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);

			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper) ?? new RouteData();

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			return urlHelper.Action(actionName, controllerName, routeValues);
		}
	}
}
