/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;
using Adxstudio.Xrm.Mapping;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Site.Areas.Service311.Controls
{
	public partial class WebFormServiceRequestResolveDuplicates : WebFormUserControl
	{
		private enum DuplicateStatus
		{
			Potential = 756150000,
			Confirmed = 756150001,
		};

		private enum DuplicateDistanceUnit
		{
			Miles = 756150000,
			Kilometers = 756150001,
		};

		private bool _isUnique;

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Page.IsPostBack) return;

			if (PreviousStepEntityID == Guid.Empty)
			{
				throw new NullReferenceException("The ID of the previous web form step's created entity is null.");
			}

			var context = PortalCrmConfigurationManager.CreateServiceContext();
			var type = GetServiceRequestType(context);
			var duplicateDetectionView = type.GetAttributeValue<string>("adx_duplicateview");
			Guid viewId;

			if (!string.IsNullOrWhiteSpace(duplicateDetectionView) && Guid.TryParse(duplicateDetectionView, out viewId))
			{
				RegisterClientSideDependencies(this);

				var latitudeFieldName = type.GetAttributeValue<string>("adx_latitudefieldname");
				var longitudeFieldName = type.GetAttributeValue<string>("adx_longitudefieldname");

				var duplicateDistance = Convert.ToDouble(type.GetAttributeValue("adx_duplicatedistance"));
				var distanceUnit = type.GetAttributeValue<OptionSetValue>("adx_duplicatedistanceunit");
				var unit = distanceUnit != null ? (DuplicateDistanceUnit)distanceUnit.Value : DuplicateDistanceUnit.Miles;
				var distance = unit == DuplicateDistanceUnit.Miles ? (1.60934 * duplicateDistance) : duplicateDistance;

				var entity = GetPreviousStepEntity(context);
				var latitude = entity.GetAttributeValue(latitudeFieldName);
				var longitude = entity.GetAttributeValue(longitudeFieldName);

				if (latitude != null && longitude != null)
				{
					CurrentServiceRequestId.Value = PreviousStepEntityID.ToString();
					RenderDuplicatesList(context, viewId, latitudeFieldName, longitudeFieldName, Convert.ToDouble(latitude), Convert.ToDouble(longitude), distance);
					RenderCurrentList(context, viewId);

					return;
				}
			}

			_isUnique = true;
			MoveToNextStep(PreviousStepEntityID);
		}

		private Entity GetServiceRequestType(OrganizationServiceContext context)
		{
			var type = context.CreateQuery("adx_servicerequesttype").FirstOrDefault(s => s.GetAttributeValue<string>("adx_entityname") == PreviousStepEntityLogicalName);

			if (type == null)
			{
				throw new NullReferenceException(string.Format("The {0} record couldn't be found with the entity name of {1}.", "adx_servicerequesttype", PreviousStepEntityLogicalName));
			}

			return type;
		}

		private Entity GetPreviousStepEntity(OrganizationServiceContext context)
		{
			var entity = context.CreateQuery(CurrentStepEntityLogicalName).FirstOrDefault(o => o.GetAttributeValue<Guid>(PreviousStepEntityPrimaryKeyLogicalName) == PreviousStepEntityID);

			if (entity == null)
			{
				throw new NullReferenceException(string.Format("The {0} record with primary key {1} equal to {2} couldn't be found.", PreviousStepEntityLogicalName, PreviousStepEntityPrimaryKeyLogicalName, PreviousStepEntityID));
			}

			return entity;
		}

		private void RenderCurrentList(OrganizationServiceContext context, Guid viewId)
		{
			var savedQueryView = new SavedQueryView(context, viewId, LanguageCode);

			savedQueryView.FetchXml.Element("entity").Add(new XElement("filter", new XAttribute("type", "and"),
				new XElement("condition",
					new XAttribute("attribute", PreviousStepEntityPrimaryKeyLogicalName),
					new XAttribute("operator", "eq"),
					new XAttribute("value", PreviousStepEntityID))));

			var viewConfiguration = new ViewConfiguration(savedQueryView)
			{
				DataPagerEnabled = false,
				FetchXml = savedQueryView.FetchXml.ToString(),
				LanguageCode = LanguageCode,
				PortalName = PortalName
			};

			var crmEntityListView = new CrmEntityListView
			{
				ID = "CurrentList",
				LanguageCode = LanguageCode,
				PortalName = PortalName,
				ViewConfigurations = new List<ViewConfiguration> { viewConfiguration },
				ListCssClass = "table table-striped",
				SelectMode = EntityGridExtensions.GridSelectMode.Single
			};

			CurrentListPlaceholder.Controls.Add(crmEntityListView);
		}

		private void RenderDuplicatesList(OrganizationServiceContext context, Guid viewId, string latitudeFieldName, string longitudeFieldName, double latitude, double longitude, double distance)
		{
			var savedQueryView = new SavedQueryView(context, viewId, LanguageCode);

			var angularDistance = distance / GeoHelpers.EarthRadiusInKilometers;
			var originLatitudeRadians = GeoHelpers.DegreesToRadians(latitude);
			var originLongitudeRadians = GeoHelpers.DegreesToRadians(longitude);
			var minLatitudeRadians = originLatitudeRadians - angularDistance;
			var maxLatitudeRadians = originLatitudeRadians + angularDistance;
			var deltaLongitude = Math.Asin(Math.Sin(angularDistance) / Math.Cos(originLatitudeRadians));
			var minLongitudeRadians = originLongitudeRadians - deltaLongitude;
			var maxLongitudeRadians = originLongitudeRadians + deltaLongitude;
			var minLatitude = GeoHelpers.RadiansToDegrees(minLatitudeRadians);
			var maxLatitude = GeoHelpers.RadiansToDegrees(maxLatitudeRadians);
			var minLongitude = GeoHelpers.RadiansToDegrees(minLongitudeRadians);
			var maxLongitude = GeoHelpers.RadiansToDegrees(maxLongitudeRadians);

			var minLatitudeCondition = new XElement("condition",
				new XAttribute("attribute", latitudeFieldName),
				new XAttribute("operator", "ge"),
				new XAttribute("value", minLatitude));

			var maxLatitudeCondition = new XElement("condition",
				new XAttribute("attribute", latitudeFieldName),
				new XAttribute("operator", "le"),
				new XAttribute("value", maxLatitude));

			var minLongitudeCondition = new XElement("condition",
				new XAttribute("attribute", longitudeFieldName),
				new XAttribute("operator", "ge"),
				new XAttribute("value", minLongitude));

			var maxLongitudeCondition = new XElement("condition",
				new XAttribute("attribute", longitudeFieldName),
				new XAttribute("operator", "le"),
				new XAttribute("value", maxLongitude));

			var notCurrentServiceRequest = new XElement("condition",
				new XAttribute("attribute", PreviousStepEntityPrimaryKeyLogicalName),
				new XAttribute("operator", "ne"),
				new XAttribute("value", PreviousStepEntityID));

			savedQueryView.FetchXml.Element("entity").Add(new XElement("filter", new XAttribute("type", "and"), notCurrentServiceRequest,
				minLatitudeCondition, maxLatitudeCondition, minLongitudeCondition, maxLongitudeCondition));

			var viewConfiguration = new ViewConfiguration(savedQueryView)
			{
				DataPagerEnabled = false,
				FetchXml = savedQueryView.FetchXml.ToString(),
				LanguageCode = LanguageCode,
				PortalName = PortalName
			};

			var response = context.Execute(new RetrieveMultipleRequest { Query = new FetchExpression(viewConfiguration.FetchXml.ToString()) }) as RetrieveMultipleResponse;

			if (!response.EntityCollection.Entities.Any())
			{
				_isUnique = true;
				MoveToNextStep(PreviousStepEntityID);
				return;
			}

			var crmEntityListView = new CrmEntityListView
			{
				ID = "DuplicateList",
				LanguageCode = LanguageCode,
				PortalName = PortalName,
				ViewConfigurations = new List<ViewConfiguration> { viewConfiguration },
				ListCssClass = "table table-striped",
				SelectMode = EntityGridExtensions.GridSelectMode.Single
			};

			DuplicateListPlaceholder.Controls.Add(crmEntityListView);
		}

		protected override void OnSubmit(object sender, WebFormSubmitEventArgs e)
		{
			var serviceRequestId = SelectedServiceRequestId.Value;

			if (!string.IsNullOrWhiteSpace(serviceRequestId))
			{
				var entityId = new Guid(serviceRequestId);

				var context = PortalCrmConfigurationManager.CreateServiceContext();
				var type = GetServiceRequestType(context);

				var duplicateStatusFieldName = type.GetAttributeValue<string>("adx_duplicatestatusfieldname");
				var duplicateParentFieldName = type.GetAttributeValue<string>("adx_duplicateparentfieldname");

				if (!string.IsNullOrWhiteSpace(duplicateStatusFieldName) || !string.IsNullOrWhiteSpace(duplicateParentFieldName))
				{
					var duplicateStatus = entityId == PreviousStepEntityID ? DuplicateStatus.Potential : DuplicateStatus.Confirmed;
					var duplicateParent = entityId == PreviousStepEntityID ? null : new EntityReference(PreviousStepEntityLogicalName, entityId);

					var entity = GetPreviousStepEntity(context);

					if (!string.IsNullOrWhiteSpace(duplicateStatusFieldName))
					{
						entity.SetAttributeValue(duplicateStatusFieldName, new OptionSetValue((int)duplicateStatus));
					}

					if (!string.IsNullOrWhiteSpace(duplicateParentFieldName))
					{
						entity.SetAttributeValue(duplicateParentFieldName, duplicateParent);
					}

					context.UpdateObject(entity);
					context.SaveChanges();
				}

				e.EntityID = entityId;

				return;
			}

			if (!_isUnique)
			{
				e.Cancel = true;
			}
		}

		private string[] ScriptIncludes
		{
			get { return new[] { "~/Areas/Service311/js/serviceduplicates.js" }; }
		}

		private void RegisterClientSideDependencies(Control control)
		{
			foreach (var script in ScriptIncludes)
			{
				if (string.IsNullOrWhiteSpace(script))
				{
					continue;
				}

				var scriptManager = ScriptManager.GetCurrent(control.Page);

				if (scriptManager == null)
				{
					continue;
				}

				var absolutePath = script.StartsWith("http", true, CultureInfo.InvariantCulture) ? script : VirtualPathUtility.ToAbsolute(script);

				scriptManager.Scripts.Add(new ScriptReference(absolutePath));
			}
		}
	}
}
