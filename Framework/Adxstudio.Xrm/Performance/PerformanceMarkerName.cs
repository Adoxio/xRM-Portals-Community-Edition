/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Performance
{
	public static class PerformanceMarkerName
	{
		public const string Services = "Crm.Portals.Services";
		public const string Startup = "Crm.Portals.Startup";
		public const string Metadata = "Crm.Portals.Metadata";
		public const string Cache = "Crm.Portals.Cache";
		public const string LiquidExtension = "Crm.Portals.LiquidExtension";
		public const string SiteMapProvider = "Crm.Portals.SiteMapProvider";
		public const string EntityGridController = "Crm.Portals.EntityGridController";
		public const string EntityPermissionProvider = "Crm.Portals.EntityPermissionProvider";
	}

	public static class PerfMarkerAreaHelper
	{
		//When Logging perf marker, to avoid calling theEnum.ToString() we can just call PerformanceMarkerAreaHelper.AreaEnumToString(theEnum)
		public static string AreaEnumToString(PerformanceMarkerArea e) { return AreaNames[(int)e]; }
		private static readonly string[] AreaNames = Enum.GetNames(typeof(PerformanceMarkerArea));
	}

	/// <summary>
	/// Apart from the UnknownArea, These enum needs to match the string value defined in PerformanceMarkerArea
	/// If a new Area is added, only PerformanceEventSource.PerformanceAggregate needs to be updated.
	/// <seealso cref="PerformanceEventSource.PerformanceAggregate(IPerformanceAggregate)"/>
	/// </summary>
	public enum PerformanceMarkerArea
	{
		Unknown,
		Crm,
		Cms,
		Liquid,
		Security
	}

	public static class PerformanceMarkerTagName
	{
		public const string CreateEntity = "CreateEntity";
		public const string UpdateEntity = "UpdateEntity";
		public const string RetrieveEntity = "RetrieveEntity";
		public const string DeleteEntity = "DeleteEntity";
		public const string ExecuteOrganizationService = "ExecuteOrganizationService";
		public const string AssociateEntity = "AssociateEntity";
		public const string DisassociateEntity = "DisassociateEntity";
		public const string RetrieveEntities = "RetrieveEntities";
		public const string CreateServiceConfiguration = "CreateServiceConfiguration";
		public const string CreateCrmServiceClient = "CreateCrmServiceClient";
		public const string GetToken = "GetToken";
		public const string StartUpConfiguration = "Configuration";
		public const string GetEntityMetadata = "GetEntityMetadata";
		public const string GetEntityPrimaryName = "GetEntityPrimaryName";
		public const string GetSystemFormEntityWithAllLabels = "GetMultipleSystemFormsWithAllLabels";
		public const string GetEntityPrimaryNameWithAttributeLabel = "GetEntityPrimaryNameWithAttributeLabel";
		public const string LiquidSourceParsed = "LiquidSourceParsed";
		public const string RenderLiquid = "RenderLiquid";
		public const string GetParentNodes = "GetParentNodes";
		public const string FindSiteMapNode = "FindSiteMapNode";
		public const string GetChildNodes = "GetChildNodes";
		public const string GetData = "GetData";
		public const string Delete = "Delete";
		public const string BuildEntityPermissionTrees = "BuildEntityPermissionTrees";
		public const string RecordLevelFiltersToFetch = "RecordLevelFiltersToFetch";
		public const string Assert = "Assert";
		public const string GetRolesForUser = "GetRolesForUser";
		public const string DownloadAsCsv = "DownloadAsCsv";
		public const string SerializeQuery = "SerializeQuery";
		public const string CloneResponse = "CloneResponse";
		public const string PersistCachedRequests = "PersistCachedRequests";
		public const string WarmupCache = "WarmupCache";
	}
}
