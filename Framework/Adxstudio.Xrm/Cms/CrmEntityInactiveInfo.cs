/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	internal class CrmEntityInactiveInfo
	{
		protected static readonly IDictionary<string, CrmEntityInactiveInfo> InfoByLogicalName = new Dictionary<string, CrmEntityInactiveInfo>
		{
			{ "adx_webfile", new CrmEntityInactiveInfo("adx_webfile", "statecode", 1, "statuscode", 2) },
			{ "adx_weblink", new CrmEntityInactiveInfo("adx_weblink", "statecode", 1, "statuscode", 2) },
			{ "adx_webpage", new CrmEntityInactiveInfo("adx_webpage", "statecode", 1, "statuscode", 2) },
			{ "adx_ad", new CrmEntityInactiveInfo("adx_ad", "statecode", 1, "statuscode", 2) },
		};

		public CrmEntityInactiveInfo(string entityName, string statePropertyName, int inactiveState, string statusPropertyName, int inactiveStatus)
		{
			EntityName = entityName;
			StatePropertyName = statePropertyName;
			InactiveState = inactiveState;
			InactiveStatus = inactiveStatus;
			StatusPropertyName = statusPropertyName;
		}

		public string EntityName { get; private set; }

		public int InactiveState { get; private set; }

		public int InactiveStatus { get; private set; }

		public string StatePropertyName { get; private set; }

		public string StatusPropertyName { get; private set; }

		public bool IsInactive(Entity entity)
		{
			entity.AssertEntityName(EntityName);

			var status = entity.GetAttributeValue<OptionSetValue>(StatusPropertyName);

			return status != null && status.Value == InactiveStatus;
		}

		public static bool TryGetInfo(string entityName, out CrmEntityInactiveInfo info)
		{
			return InfoByLogicalName.TryGetValue(entityName, out info);
		}
	}
}
