/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
	public class EntityView : SavedQueryView
	{
		public string DisplayName { get; set; }

		public EntityView(OrganizationServiceContext serviceContext, string entityLogicalName, string savedQueryName,
			int? languageCode = 0, string displayName = null,
			string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
			: base(serviceContext, entityLogicalName, savedQueryName, languageCode, aliasColumnNameStringFormat)
		{
			DisplayName = displayName;
		}

		public EntityView(OrganizationServiceContext serviceContext, string fetchXmlString, string layoutXmlString,
			string entityLogicalName, string savedQueryName, int? languageCode = 0, string displayName = null,
			string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
			: base(
				serviceContext, fetchXmlString, layoutXmlString, entityLogicalName, savedQueryName, languageCode,
				aliasColumnNameStringFormat)
		{
			DisplayName = displayName;
		}

		public EntityView(OrganizationServiceContext serviceContext, Guid id, int? languageCode = 0, string displayName = null,
			string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
			: base(serviceContext, id, languageCode, aliasColumnNameStringFormat)
		{
			DisplayName = displayName;
		}

		public EntityView(OrganizationServiceContext serviceContext, string fetchXmlString, string layoutXmlString, Guid id,
			int? languageCode = 0, string displayName = null,
			string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
			: base(serviceContext, fetchXmlString, layoutXmlString, id, languageCode, aliasColumnNameStringFormat)
		{
			DisplayName = displayName;
		}

		public EntityView(OrganizationServiceContext serviceContext, string entityLogicalName, int queryType,
			bool isDefault = false, int? languageCode = 0, string displayName = null,
			string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
			: base(serviceContext, entityLogicalName, queryType, isDefault, languageCode, aliasColumnNameStringFormat)
		{
			DisplayName = displayName;
		}

		public EntityView(OrganizationServiceContext serviceContext, Entity savedQuery, int? languageCode = 0,
			string displayName = null, string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
			: base(serviceContext, savedQuery, languageCode, aliasColumnNameStringFormat)
		{
			DisplayName = displayName;
		}

		public EntityView(OrganizationServiceContext serviceContext, Entity savedQuery, string fetchXmlString,
			string layoutXmlString, int? languageCode = 0, string displayName = null,
			string aliasColumnNameStringFormat = DefaultAliasColumnNameStringFormat)
			: base(serviceContext, savedQuery, fetchXmlString, layoutXmlString, languageCode, aliasColumnNameStringFormat)
		{
			DisplayName = displayName;
		}

		public EntityView() { }
	}
}
