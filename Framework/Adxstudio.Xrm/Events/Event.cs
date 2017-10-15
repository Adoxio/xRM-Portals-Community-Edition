/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Core.Flighting;

namespace Adxstudio.Xrm.Events
{
	internal class Event : IEvent
	{
		public Event(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			Entity = entity;

			Name = entity.GetAttributeValue<string>("adx_name");
			Description = entity.GetAttributeValue<string>("adx_description");

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Event, HttpContext.Current, "read_event", 1, entity.ToEntityReference(), "read");
			}
		}

		public string Description { get; private set; }

		public Entity Entity { get; private set; }

		public string Name { get; private set; }


	}
}
