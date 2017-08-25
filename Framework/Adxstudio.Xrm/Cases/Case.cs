/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Core.Flighting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Cases
{
	internal enum IncidentState
	{
		Active = 0,
		Resolved = 1,
		Canceled = 2
	}

	public class Case : ICase
	{
		public Case(Entity incident, EntityMetadata incidentMetadata, string url = null, Entity responsibleContact = null)
		{
			if (incident == null) throw new ArgumentNullException("incident");
			if (incidentMetadata == null) throw new ArgumentNullException("incidentMetadata");
			if (incident.LogicalName != "incident") throw new ArgumentException(string.Format(ResourceManager.GetString("Value_Missing_For_LogicalName"), incident.LogicalName), "incident");

			Entity = incident;
			Url = url;

			EntityReference = incident.ToEntityReference();

			CaseTypeLabel = GetEnumLabel(incident, incidentMetadata, "casetypecode");
			StateLabel = GetEnumLabel(incident, incidentMetadata, "statecode");
			StatusLabel = GetEnumLabel(incident, incidentMetadata, "statuscode");

			Resolution = incident.GetAttributeValue<string>("adx_resolution");
			ResolutionDate = incident.GetAttributeValue<DateTime?>("adx_resolutiondate");

			var statecode = incident.GetAttributeValue<OptionSetValue>("statecode");

			if (statecode != null)
			{
				IsActive = statecode.Value == (int)IncidentState.Active;
				IsCanceled = statecode.Value == (int)IncidentState.Canceled;
				IsResolved = statecode.Value == (int)IncidentState.Resolved;
			}

			Customer = incident.GetAttributeValue<EntityReference>("customerid");

			if (responsibleContact != null)
			{
				ResponsibleContact = responsibleContact.ToEntityReference();
				ResponsibleContactEmailAddress = responsibleContact.GetAttributeValue<string>("emailaddress1");
				ResponsibleContactName = responsibleContact.GetAttributeValue<string>("fullname");
			}

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Case, HttpContext.Current, "read_incident", 1, incident.ToEntityReference(), "read");
			}
		}

		public string CaseTypeLabel { get; private set; }

		public DateTime CreatedOn
		{
			get { return Entity.GetAttributeValue<DateTime?>("createdon").GetValueOrDefault(); }
		}

		public EntityReference Customer { get; private set; }

		public string Description
		{
			get { return Entity.GetAttributeValue<string>("description"); }
		}

		public Entity Entity { get; private set; }

		public EntityReference EntityReference { get; private set; }

		public bool IsActive { get; private set; }

		public bool IsCanceled { get; private set; }

		public bool IsResolved { get; private set; }

		public bool PublishToWeb
		{
			get { return Entity.GetAttributeValue<bool?>("adx_publishtoweb").GetValueOrDefault(); }
		}

		public EntityReference ResponsibleContact { get; private set; }

		public string ResponsibleContactEmailAddress { get; private set; }

		public string ResponsibleContactName { get; private set; }

		public string Resolution { get; private set; }

		public DateTime? ResolutionDate { get; private set; }

		public string StateLabel { get; private set; }

		public string StatusLabel { get; private set; }

		public string TicketNumber
		{
			get { return Entity.GetAttributeValue<string>("ticketnumber"); }
		}

		public string Title
		{
			get { return Entity.GetAttributeValue<string>("title"); }
		}

		public string Url { get; private set; }

		private static string GetEnumLabel(Entity entity, EntityMetadata entityMetadata, string attributeLogicalName)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");

			var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeLogicalName) as EnumAttributeMetadata;

			if (attributeMetadata == null)
			{
				return null;
			}

			var value = entity.GetAttributeValue<OptionSetValue>(attributeLogicalName);

			if (value == null)
			{
				return null;
			}

			var option = attributeMetadata.OptionSet.Options.FirstOrDefault(o => o.Value == value.Value);

			if (option == null)
			{
				return null;
			}

			return option.Label.GetLocalizedLabelString();
		}
	}
}
