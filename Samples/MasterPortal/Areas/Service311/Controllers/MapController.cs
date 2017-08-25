/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.Service311.Controllers
{
	public class MapController : Controller
	{
		public class MapNode
		{
			public enum NodeType
			{
				ServiceRequest = 1,
				Alert = 2,
			}

			public MapNode(NodeType type, string serviceRequestNumber, string title, string location, string description, int status, int priority, DateTime? incidentDate, DateTime? scheduledDate, DateTime? closedDate, decimal latitude, decimal longitude, string pushpinImageUrl, string checkStatusUrl)
			{
				ItemNodeType = type;

				ServiceRequestNumber = serviceRequestNumber;

				Title = title;

				Location = location;

				Description = description;

				var context = PortalCrmConfigurationManager.CreateServiceContext();

				Status = context.GetOptionSetValueLabel("adx_servicerequest", "adx_servicestatus", status);

				StatusId = status;

				Priority = context.GetOptionSetValueLabel("adx_servicerequest", "adx_priority", priority);

				PriorityId = priority;

				IncidentDate = incidentDate;

				ScheduledDate = scheduledDate;

				ClosedDate = closedDate;

				Latitude = latitude;

				Longitude = longitude;

				PushpinImageUrl = pushpinImageUrl;

				CheckStatusUrl = checkStatusUrl;
			}

			public MapNode(NodeType type, string title, string location, string description, DateTime? scheduledStartDate, DateTime? scheduledEndDate, decimal latitude, decimal longitude, string pushpinImageUrl)
			{
				ItemNodeType = type;

				Title = title;

				Location = location;

				Description = description;

				ScheduledStartDate = scheduledStartDate;

				ScheduledEndDate = scheduledEndDate;

				Latitude = latitude;

				Longitude = longitude;

				PushpinImageUrl = pushpinImageUrl;
			}

			public NodeType ItemNodeType { get; set; }
			public string ServiceRequestNumber { get; set; }
			public string Title { get; set; }
			public string Description { get; set; }
			public string Location { get; set; }
			public string Status { get; set; }
			public int? StatusId { get; set; }
			public string Priority { get; set; }
			public int? PriorityId { get; set; }
			public DateTime? IncidentDate { get; set; }
			public DateTime? ScheduledDate { get; set; }
			public DateTime? ScheduledStartDate { get; set; }
			public DateTime? ScheduledEndDate { get; set; }
			public DateTime? ClosedDate { get; set; }
			public decimal Latitude { get; set; }
			public decimal Longitude { get; set; }
			public string PushpinImageUrl { get; set; }
			public string CheckStatusUrl { get; set; }
		}

		// POST: /Service311/Map/

		[AcceptVerbs(HttpVerbs.Post)]
		[AjaxValidateAntiForgeryToken, SuppressMessage("ASP.NET.MVC.Security", "CA5332:MarkVerbHandlersWithValidateAntiforgeryToken", Justification = "Handled with the custom attribute AjaxValidateAntiForgeryToken")]
		public ActionResult Search(int dateFilterCode, string dateFrom, string dateTo, int statusFilterCode, int priorityFilterCode, string[] types, bool includeAlerts)
		{
			dateFilterCode = (dateFilterCode >= 0) ? dateFilterCode : 0;
			var status = (statusFilterCode > 0) ? statusFilterCode : 999;
			var priority = (priorityFilterCode >= 0) ? priorityFilterCode : 0;
			DateTime fromDate;
			DateTime.TryParse(dateFrom, out fromDate);
			DateTime toDate;
			DateTime.TryParse(dateTo, out toDate);
			var typesGuids = types == null || types.Length < 1 ? null : Array.ConvertAll(types, Guid.Parse);
			var context = PortalCrmConfigurationManager.CreateServiceContext();

			var serviceRequests = context.CreateQuery("adx_servicerequest").Where(s => s.GetAttributeValue<Decimal?>("adx_latitude") != null && s.GetAttributeValue<Decimal?>("adx_longitude") != null)
				.FilterServiceRequestsByPriority(priority)
				.FilterServiceRequestsByStatus(status)
				.FilterServiceRequestsByDate(dateFilterCode, fromDate, toDate.AddDays(1))
				.FilterServiceRequestsByType(typesGuids)
				.ToList();

			var serviceRequestMapNodes = new List<MapNode>();

			if (serviceRequests.Any())
			{
				serviceRequestMapNodes =
					serviceRequests.Select(
						s =>
							new MapNode(
								MapNode.NodeType.ServiceRequest,
								s.GetAttributeValue<string>("adx_servicerequestnumber"),
								s.GetAttributeValue<string>("adx_name"),
								s.GetAttributeValue<string>("adx_location"),
								string.Empty,
								s.GetAttributeValue<OptionSetValue>("adx_servicestatus") == null ? 0 : s.GetAttributeValue<OptionSetValue>("adx_servicestatus").Value,
								s.GetAttributeValue<OptionSetValue>("adx_priority") == null ? 0 : s.GetAttributeValue<OptionSetValue>("adx_priority").Value,
								s.GetAttributeValue<DateTime>("adx_incidentdate"),
								s.GetAttributeValue<DateTime>("adx_scheduleddate"),
								s.GetAttributeValue<DateTime>("adx_closeddate"),
								s.GetAttributeValue<Decimal?>("adx_latitude").GetValueOrDefault(0),
								s.GetAttributeValue<Decimal?>("adx_longitude").GetValueOrDefault(0),
								ServiceRequestHelpers.GetPushpinImageUrl(context, s),
								ServiceRequestHelpers.GetCheckStatusUrl(s))).ToList();
			}

			var alertMapNodes = new List<MapNode>();

			if (includeAlerts)
			{
				var alerts = context.CreateQuery("adx_311alert").Where(a =>
					a.GetAttributeValue<Decimal?>("adx_latitude") != null && a.GetAttributeValue<Decimal?>("adx_longitude") != null && a.GetAttributeValue<bool?>("adx_publishtoweb").GetValueOrDefault(false) == true)
					.FilterAlertsByDate(dateFilterCode, fromDate, toDate.AddDays(1)).ToList();

				if (alerts.Any())
				{
					var alertIconImageUrl = ServiceRequestHelpers.GetAlertPushpinImageUrl();
					alertMapNodes = alerts.Select(a => new MapNode(MapNode.NodeType.Alert, a.GetAttributeValue<string>("adx_name"), a.GetAttributeValue<string>("adx_address1_line1"), a.GetAttributeValue<string>("adx_description"), a.GetAttributeValue<DateTime?>("adx_scheduledstartdate"), a.GetAttributeValue<DateTime?>("adx_scheduledenddate"), a.GetAttributeValue<Decimal?>("adx_latitude") ?? 0, a.GetAttributeValue<Decimal?>("adx_longitude") ?? 0, alertIconImageUrl)).ToList();
				}
			}

			var mapNodes = serviceRequestMapNodes.Union(alertMapNodes).ToList();

			var json = Json(mapNodes);

			return json;
		}
	}

	public static class XrmQueryExtensions
	{
		public static IQueryable<Entity> FilterAlertsByDate(this IQueryable<Entity> query, int dateFilterCode, DateTime dateFrom, DateTime dateTo)
		{
			switch (dateFilterCode)
			{
				case 0: // filter last 7 days
					return query.Where(a => a.GetAttributeValue<DateTime?>("adx_scheduledstartdate").GetValueOrDefault() > DateTime.Now.AddDays(-7));
				case 1: // filter last 30 days
					return query.Where(a => a.GetAttributeValue<DateTime?>("adx_scheduledstartdate").GetValueOrDefault() > DateTime.Now.AddDays(-30));
				case 2: // filter last 12 months
					return query.Where(a => a.GetAttributeValue<DateTime?>("adx_scheduledstartdate").GetValueOrDefault() > DateTime.Now.AddMonths(-12));
				case 3: // filter by date range
					return query.Where(a => a.GetAttributeValue<DateTime?>("adx_scheduledstartdate").GetValueOrDefault() >= dateFrom && a.GetAttributeValue<DateTime?>("adx_scheduledstartdate").GetValueOrDefault() <= dateTo);
				default:
					return query;
			}
		}

		public static IQueryable<Entity> FilterServiceRequestsByDate(this IQueryable<Entity> query, int dateFilterCode, DateTime dateFrom, DateTime dateTo)
		{
			switch (dateFilterCode)
			{
				case 0: // filter last 7 days
					return query.Where(s => s.GetAttributeValue<DateTime?>("adx_incidentdate").GetValueOrDefault() > DateTime.Now.AddDays(-7));
				case 1: // filter last 30 days
					return query.Where(s => s.GetAttributeValue<DateTime?>("adx_incidentdate").GetValueOrDefault() > DateTime.Now.AddDays(-30));
				case 2: // filter last 12 months
					return query.Where(s => s.GetAttributeValue<DateTime?>("adx_incidentdate").GetValueOrDefault() > DateTime.Now.AddMonths(-12));
				case 3: // filter by date range
					return query.Where(s => s.GetAttributeValue<DateTime?>("adx_incidentdate").GetValueOrDefault() >= dateFrom && s.GetAttributeValue<DateTime?>("adx_incidentdate").GetValueOrDefault() <= dateTo);
				default:
					return query;
			}
		}

		public static IQueryable<Entity> FilterServiceRequestsByPriority(this IQueryable<Entity> query, int priority)
		{
			return priority != 0 ? query.Where(s => s.GetAttributeValue<OptionSetValue>("adx_priority") != null && s.GetAttributeValue<OptionSetValue>("adx_priority").Value == priority) : query;
		}

		public static IQueryable<Entity> FilterServiceRequestsByStatus(this IQueryable<Entity> query, int value)
		{
			return value != 999 ? query.Where(s => s.GetAttributeValue<OptionSetValue>("adx_servicestatus") != null && s.GetAttributeValue<OptionSetValue>("adx_servicestatus").Value == value) : query;
		}

		public static IQueryable<Entity> FilterServiceRequestsByType(this IQueryable<Entity> query, IEnumerable<Guid> types)
		{
			if (types == null || types.Contains(Guid.Empty))
			{
				return query;
			}
			return query.Where(ContainsPropertyValueEqual<Entity>("adx_servicerequesttype", types));
		}

		private static Expression<Func<TParameter, bool>> ContainsPropertyValueEqual<TParameter>(string crmPropertyName, IEnumerable<Guid> values)
		{
			var parameterType = typeof(TParameter);

			var parameter = Expression.Parameter(parameterType, parameterType.Name.ToLowerInvariant());

			var expression = ContainsPropertyValueEqual(crmPropertyName, values, parameter);

			return Expression.Lambda<Func<TParameter, bool>>(expression, parameter);
		}

		private static Expression ContainsPropertyValueEqual(string crmPropertyName, IEnumerable<Guid> values, ParameterExpression parameter)
		{
			var left = PropertyValueEqual(parameter, crmPropertyName, values.First());

			return ContainsPropertyValueEqual(crmPropertyName, values.Skip(1), parameter, left);
		}

		private static Expression ContainsPropertyValueEqual(string crmPropertyName, IEnumerable<Guid> values, ParameterExpression parameter, Expression expression)
		{
			if (!values.Any())
			{
				return expression;
			}

			var orElse = Expression.OrElse(expression, PropertyValueEqual(parameter, crmPropertyName, values.First()));

			return ContainsPropertyValueEqual(crmPropertyName, values.Skip(1), parameter, orElse);
		}

		private static Expression PropertyValueEqual(Expression parameter, string crmPropertyName, Guid value)
		{
			var methodCall = Expression.Call(parameter, "GetAttributeValue", new[] { typeof(Guid) }, Expression.Constant(crmPropertyName));

			return Expression.Equal(methodCall, Expression.Constant(value));
		}
	}
}
