/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Portal.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Web.Mvc;
	using System.Web.Security;
	using Adxstudio.Xrm;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Json.JsonConverter;
	using Adxstudio.Xrm.Metadata;
	using Adxstudio.Xrm.Performance;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Web.Mvc;
	using Adxstudio.Xrm.Web.UI;
	using Adxstudio.Xrm.Web.UI.CrmEntityListView;
	using Adxstudio.Xrm.Web.UI.JsonConfiguration;
	using DocumentFormat.OpenXml;
	using DocumentFormat.OpenXml.Packaging;
	using DocumentFormat.OpenXml.Spreadsheet;
	using Microsoft.Crm.Sdk.Messages;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Messages;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Microsoft.Xrm.Sdk.Query;
	using Newtonsoft.Json;
	using Site.Areas.Portal.ViewModels;
	using Filter = Adxstudio.Xrm.Services.Query.Filter;

	public class EntityGridController : Controller
	{
		private const int DefaultPageSize = 10;
		private const int DefaultMaxPageSize = 50;

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult GetSubgridData(string base64SecureConfiguration, string sortExpression, string search, int page,
			int pageSize = DefaultPageSize)
		{
			return GetData(ConvertSecureStringToViewConfiguration(base64SecureConfiguration), sortExpression, search, null, null, page, pageSize);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult GetGridData(string base64SecureConfiguration, string sortExpression, string search, string filter,
			string metaFilter, int page, int pageSize = DefaultPageSize)
		{
			return GetData(ConvertSecureStringToViewConfiguration(base64SecureConfiguration), sortExpression, search, filter, metaFilter, page, pageSize);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult GetLookupGridData(string base64SecureConfiguration, string sortExpression, string search, string filter,
			string metaFilter, int page, int pageSize = DefaultPageSize, bool applyRelatedRecordFilter = false,
			string filterRelationshipName = null, string filterEntityName = null, string filterAttributeName = null, string filterValue = null, IDictionary<string, string> customParameters = null, string entityId = null, string entityName = null)
		{
			Guid? filterGuidValue = null;
			Guid entityGuid;
			if (applyRelatedRecordFilter)
			{
				if (string.IsNullOrWhiteSpace(filterValue) && !string.IsNullOrWhiteSpace(entityId) && Guid.TryParse(entityId, out entityGuid) && !string.IsNullOrWhiteSpace(entityName))
				{
					filterValue = GetDependentFilterAttributeValue(entityGuid, entityName, filterAttributeName);
				}
				if (!string.IsNullOrWhiteSpace(filterValue))
				{
					Guid guidValue;
					if (Guid.TryParse(filterValue, out guidValue))
					{
						filterGuidValue = guidValue;
					}
				}
			}
			return GetData(ConvertSecureStringToViewConfiguration(base64SecureConfiguration), sortExpression, search, filter, metaFilter, page, pageSize, true, applyRelatedRecordFilter,
				filterRelationshipName, filterEntityName, filterAttributeName, filterGuidValue, customParameters: customParameters);
		}

		private string GetDependentFilterAttributeValue(Guid entityId, string entityName, string filterAttributeName)
		{
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var serviceContext = dataAdapterDependencies.GetServiceContext();
			var entityRetrieveResponse = (RetrieveResponse)serviceContext.Execute(new RetrieveRequest() { ColumnSet = new ColumnSet(new string[] { filterAttributeName }), Target = new EntityReference(entityName, entityId) });
			
			if (null != entityRetrieveResponse && null != entityRetrieveResponse.Entity)
			{
				var filterEntityReference = entityRetrieveResponse.Entity.GetAttributeValue<EntityReference>(filterAttributeName);

				if (null != filterEntityReference)
				{
					return filterEntityReference.Id.ToString();
				}
			}
			return null;
		}

		private ViewConfiguration ConvertSecureStringToViewConfiguration(string base64SecureConfiguration)
		{
			var secureConfigurationByteArray = Convert.FromBase64String(base64SecureConfiguration);
			var unprotectedByteArray = MachineKey.Unprotect(secureConfigurationByteArray, "Secure View Configuration");
			if (unprotectedByteArray == null)
			{
				return null;
			}
			var configurationJson = Encoding.UTF8.GetString(unprotectedByteArray);
			var viewConfiguration = JsonConvert.DeserializeObject<ViewConfiguration>(configurationJson, new JsonSerializerSettings { ContractResolver = JsonConfigurationContractResolver.Instance, Converters = new List<JsonConverter> { new UrlBuilderConverter() } });
			return viewConfiguration;
		}

		private ActionResult GetData(ViewConfiguration viewConfiguration, string sortExpression, string search, string filter,
			string metaFilter, int page, int pageSize = DefaultPageSize, bool applyRecordLevelFilters = true,
			bool applyRelatedRecordFilter = false, string filterRelationshipName = null, string filterEntityName = null,
			string filterAttributeName = null, Guid? filterValue = null, bool overrideMaxPageSize = false, IDictionary<string, string> customParameters = null)
		{
			PaginatedGridData data;
			//Search criteria with length 4000+ causes Generic SQL error Bug#371907
			const int maxSearchLength = 3999;
			var searchCriteria = search?.Length > maxSearchLength ? search.Substring(0, maxSearchLength) : search;

			using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.EntityGridController, PerformanceMarkerArea.Crm, PerformanceMarkerTagName.GetData))
			{
				if (viewConfiguration == null)
				{
					return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Invalid_Request"));
				}

				if (pageSize < 0)
				{
					pageSize = DefaultPageSize;
				}

				if (pageSize > DefaultMaxPageSize && !overrideMaxPageSize)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(
						"pageSize={0} is greater than the allowed maximum page size of {1}. Page size has been constrained to {1}.",
						pageSize, DefaultMaxPageSize));
					pageSize = DefaultMaxPageSize;
				}

				var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: viewConfiguration.PortalName);
				var website = HttpContext.GetWebsite();

				var viewDataAdapter = SetViewDataAdapter(viewConfiguration, sortExpression, searchCriteria, filter, metaFilter, page,
					applyRecordLevelFilters, applyRelatedRecordFilter, filterRelationshipName, filterEntityName,
					filterAttributeName, filterValue, customParameters, dataAdapterDependencies, website);

				var result = viewDataAdapter.FetchEntities();


				//If current page doesn't contain any records, but records exist in general, get those records from previous page for further rendering them.
				if (!result.Records.Any() && result.TotalRecordCount > 0)
				{
					viewDataAdapter = SetViewDataAdapter(viewConfiguration, sortExpression, searchCriteria, filter, metaFilter,
						page - 1, applyRecordLevelFilters, applyRelatedRecordFilter, filterRelationshipName, filterEntityName,
						filterAttributeName, filterValue, customParameters, dataAdapterDependencies, website);

					result = viewDataAdapter.FetchEntities();
				}
				
				if (result.EntityPermissionDenied)
				{
					var permissionResult = new EntityPermissionResult(true);

					return Json(permissionResult);
				}


				var serviceContext = dataAdapterDependencies.GetServiceContext();
				var organizationMoneyFormatInfo = new OrganizationMoneyFormatInfo(dataAdapterDependencies);
				var crmLcid = HttpContext.GetCrmLcid();

				EntityRecord[] records;

				if (viewConfiguration.EnableEntityPermissions && AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled && viewConfiguration.EntityName != "entitlement")
				{
					var crmEntityPermissionProvider = new CrmEntityPermissionProvider();
					records = result.Records.Select(e => new EntityRecord(e, serviceContext, crmEntityPermissionProvider, viewDataAdapter.EntityMetadata, true, organizationMoneyFormatInfo: organizationMoneyFormatInfo, crmLcid: crmLcid)).ToArray();
				}
				else
				{
					records = result.Records.Select(e => new EntityRecord(e, viewDataAdapter.EntityMetadata, serviceContext, organizationMoneyFormatInfo, crmLcid)).ToArray();
				}

				records = FilterWebsiteRelatedRecords(records, dataAdapterDependencies.GetWebsite());
				var totalRecordCount = result.TotalRecordCount;

				var disabledActionLinks = new List<DisabledItemActionLink>();

				// Disable Create Related Record Action Links based on Filter Criteria.
				disabledActionLinks.AddRange(DisableActionLinksBasedOnFilterCriteria(serviceContext, viewDataAdapter.EntityMetadata,
					viewConfiguration.CreateRelatedRecordActionLinks, records));

				// Disable Item Action Links based on Filter Criteria.
				disabledActionLinks.AddRange(DisableActionLinksBasedOnFilterCriteria(serviceContext, viewDataAdapter.EntityMetadata,
					viewConfiguration.ItemActionLinks, records));

				data = new PaginatedGridData(records, totalRecordCount, page, pageSize, disabledActionLinks)
				{
					CreateActionMetadata = GetCreationActionMetadata(viewConfiguration, dataAdapterDependencies),
					MoreRecords = result.MoreRecords.GetValueOrDefault() || (totalRecordCount > (page * pageSize))
				};
			}

			var json = Json(data);
			json.MaxJsonLength = int.MaxValue;

			return json;
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult Delete(EntityReference entityReference)
		{
			string portalName = null;
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var languageCodeSetting = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, "Language Code");

			if (!string.IsNullOrWhiteSpace(languageCodeSetting))
			{
				int languageCode;
				if (int.TryParse(languageCodeSetting, out languageCode))
				{
					portalName = languageCode.ToString(CultureInfo.InvariantCulture);
				}
			}

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: portalName);
			var serviceContext = dataAdapterDependencies.GetServiceContext();
			var entityPermissionProvider = new CrmEntityPermissionProvider();

			if (!entityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var entityMetadata = serviceContext.GetEntityMetadata(entityReference.LogicalName, EntityFilters.All);
			var primaryKeyName = entityMetadata.PrimaryIdAttribute;
			var entity =
				serviceContext.CreateQuery(entityReference.LogicalName)
					.First(e => e.GetAttributeValue<Guid>(primaryKeyName) == entityReference.Id);
			var test = entityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Delete, entity);

			if (test)
			{
				using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.EntityGridController, PerformanceMarkerArea.Crm, PerformanceMarkerTagName.Delete))
				{
					serviceContext.DeleteObject(entity);
					serviceContext.SaveChanges();
				}
			}
			else
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("No_Permissions_To_Delete_This_Record"));
			}

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult Associate(AssociateRequest request)
		{
			string portalName = null;
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var languageCodeSetting = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, "Language Code");

			if (!string.IsNullOrWhiteSpace(languageCodeSetting))
			{
				int languageCode;
				if (int.TryParse(languageCodeSetting, out languageCode))
				{
					portalName = languageCode.ToString(CultureInfo.InvariantCulture);
				}
			}

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: portalName);
			var serviceContext = dataAdapterDependencies.GetServiceContext();
			var entityPermissionProvider = new CrmEntityPermissionProvider();

			if (!entityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var relatedEntities = request.RelatedEntities
				.Where(e => entityPermissionProvider.TryAssertAssociation(serviceContext, request.Target, request.Relationship, e))
				.ToArray();

			if (!relatedEntities.Any())
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Missing_Permissions_For_Operation_Exception"));
			}

			relatedEntities = FilterAlreadyAssociated(serviceContext, request.Relationship, request.Target, relatedEntities);

			var filtered = new AssociateRequest
			{
				Target = request.Target,
				Relationship = request.Relationship,
				RelatedEntities = new EntityReferenceCollection(relatedEntities)
			};

			serviceContext.Execute(filtered);
			
			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult Disassociate(DisassociateRequest request)
		{
			string portalName = null;
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var languageCodeSetting = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, "Language Code");

			if (!string.IsNullOrWhiteSpace(languageCodeSetting))
			{
				int languageCode;
				if (int.TryParse(languageCodeSetting, out languageCode))
				{
					portalName = languageCode.ToString(CultureInfo.InvariantCulture);
				}
			}

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: portalName);
			var serviceContext = dataAdapterDependencies.GetServiceContext();
			var entityPermissionProvider = new CrmEntityPermissionProvider();

			if (!entityPermissionProvider.PermissionsExist)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Entity_Permissions_Have_Not_Been_Defined_Message"));
			}

			var relatedEntities =
				request.RelatedEntities.Where(
					related => entityPermissionProvider.TryAssertAssociation(serviceContext, request.Target, request.Relationship, related)).ToList();

			if (relatedEntities.Any())
			{
				var filtered = new DisassociateRequest { Target = request.Target, Relationship = request.Relationship, RelatedEntities = new EntityReferenceCollection(relatedEntities) };

				serviceContext.Execute(filtered);
			}
			else
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Missing_Permissions_For_Operation_Exception"));
			}

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult DownloadAsCsv(string viewName, IEnumerable<LayoutColumn> columns, string base64SecureConfiguration, string sortExpression, string search, string filter,
			string metaFilter, int page = 1, int pageSize = DefaultPageSize)
		{
			var viewConfiguration = ConvertSecureStringToViewConfiguration(base64SecureConfiguration);
			using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.EntityGridController, PerformanceMarkerArea.Crm, PerformanceMarkerTagName.DownloadAsCsv))
			{
				if (viewConfiguration == null)
				{
					return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Invalid_Request"));
				}

				var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: viewConfiguration.PortalName);
		
				// override the page parameters
				page = 1;
				pageSize = new SettingDataAdapter(dataAdapterDependencies, HttpContext.GetWebsite())
					.GetIntegerValue("Grid/Download/MaximumResults")
					.GetValueOrDefault(Fetch.MaximumPageSize);
				viewConfiguration.PageSize = pageSize;
			
				var json = GetData(viewConfiguration, sortExpression, search, filter, metaFilter, page, pageSize, true, false, null, null, null, null, true) as JsonResult;

				if (json == null)
				{
					return new HttpStatusCodeResult(HttpStatusCode.NoContent);
				}

				if (json.Data is EntityPermissionResult)
				{
					return json;
				}

				var data = json.Data as PaginatedGridData;

				if (data == null)
				{
					return new HttpStatusCodeResult(HttpStatusCode.NoContent);
				}

				var csv = new StringBuilder();

				var dataColumns = columns.Where(col => col.LogicalName != "col-action").ToArray();

				foreach (var column in dataColumns)
				{
					csv.Append(EncodeCommaSeperatedValue(column.Name));
				}

				csv.AppendLine();

				foreach (var record in data.Records)
				{
					foreach (var column in dataColumns)
					{
						var attribute = record.Attributes.FirstOrDefault(a => a.Name == column.LogicalName);

						if (attribute == null) continue;

						csv.Append(EncodeCommaSeperatedValue(attribute.DisplayValue as string));
					}

					csv.AppendLine();
				}

				var filename = new string(viewName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());

				var sessionKey = "{0:s}|{1}.csv".FormatWith(DateTime.UtcNow, filename);

				Session[sessionKey] = csv.ToString();

				return Json(new { success = true, sessionKey }, JsonRequestBehavior.AllowGet);
			}
		}

		[ActionName("DownloadAsCsv")]
		[HttpGet]
		public ActionResult GetCsvFile(string key)
		{
			var csv = Session[key] as string;

			if (string.IsNullOrEmpty(csv))
			{
				return new HttpStatusCodeResult(HttpStatusCode.NoContent);
			}

			Session[key] = null;

			return File(new UTF8Encoding().GetBytes(csv), "text/csv", key.Substring(key.IndexOf('|') + 1));
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult DownloadAsExcel(string viewName, IEnumerable<LayoutColumn> columns, string base64SecureConfiguration, string sortExpression, string search, string filter,
			string metaFilter, int page = 1, int pageSize = DefaultPageSize, int timezoneOffset = 0)
		{
			var viewConfiguration = ConvertSecureStringToViewConfiguration(base64SecureConfiguration);

			if (viewConfiguration == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ResourceManager.GetString("Invalid_Request"));
			}

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: viewConfiguration.PortalName);

			// override the page parameters
			page = 1;
			pageSize = new SettingDataAdapter(dataAdapterDependencies, HttpContext.GetWebsite())
				.GetIntegerValue("Grid/Download/MaximumResults")
				.GetValueOrDefault(Fetch.MaximumPageSize);
			viewConfiguration.PageSize = pageSize;
			
			var json = GetData(viewConfiguration, sortExpression, search, filter, metaFilter, page, pageSize, true, false, null, null, null, null, true) as JsonResult;

			if (json == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.NoContent);
			}

			if (json.Data is EntityPermissionResult)
			{
				return json;
			}

			var data = json.Data as PaginatedGridData;

			if (data == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.NoContent);
			}

			var stream = new MemoryStream();

			var spreadsheet = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);

			var workbookPart = spreadsheet.AddWorkbookPart();
			workbookPart.Workbook = new Workbook();

			var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
			var sheet = new Sheet { Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = viewName.Truncate(30) };

			var sheets = new Sheets();
			sheets.Append(sheet);

			var sheetData = new SheetData();

			var rowIndex = 1;
			var columnIndex = 1;

			var firstRow = new Row { RowIndex = (uint)rowIndex };

			var dataColumns = columns.Where(col => col.LogicalName != "col-action").ToArray();

			foreach (var column in dataColumns)
			{
				var cell = new Cell { CellReference = CreateCellReference(columnIndex) + rowIndex, DataType = CellValues.InlineString };

				var inlineString = new InlineString { Text = new Text { Text = column.Name } };

				cell.AppendChild(inlineString);

				firstRow.AppendChild(cell);

				columnIndex++;
			}

			sheetData.Append(firstRow);

			foreach (var record in data.Records)
			{
				var row = new Row { RowIndex = (uint)++rowIndex };

				columnIndex = 0;

				foreach (var column in dataColumns)
				{
					columnIndex++;

					var attribute = record.Attributes.FirstOrDefault(a => a.Name == column.LogicalName);

					if (attribute == null) continue;

					var isDateTime = attribute.AttributeMetadata.AttributeType == AttributeTypeCode.DateTime;

					var cell = new Cell { CellReference = CreateCellReference(columnIndex) + rowIndex, DataType = CellValues.InlineString };

					var inlineString = new InlineString { Text = new Text { Text = isDateTime ? this.GetFormattedDateTime(attribute, timezoneOffset) : attribute.DisplayValue as string } };

					cell.AppendChild(inlineString);

					row.AppendChild(cell);
				}

				sheetData.Append(row);
			}

			worksheetPart.Worksheet = new Worksheet(sheetData);

			spreadsheet.WorkbookPart.Workbook.AppendChild(sheets);

			workbookPart.Workbook.Save();

			spreadsheet.Close();

			var filename = new string(viewName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());

			var sessionKey = "{0:s}|{1}.xlsx".FormatWith(DateTime.UtcNow, filename);

			stream.Position = 0; // Reset the stream to the beginning and save to session.

			Session[sessionKey] = stream;

			return Json(new { success = true, sessionKey }, JsonRequestBehavior.AllowGet);
		}

		private string GetFormattedDateTime(EntityRecordAttribute attribute, int timezoneOffset)
		{
			if (attribute.Value == null)
			{
				return string.Empty;
			}

			var dateTimeDisplayValue = Convert.ToDateTime(attribute.Value);
			
			// Adjust TimeZone
			if (attribute.DateTimeBehavior == DateTimeBehavior.UserLocal)
			{
				dateTimeDisplayValue = dateTimeDisplayValue.AddMinutes(-1 * timezoneOffset);
			}

			// Get format
			string format = attribute.DateTimeFormat == DateTimeFormat.DateAndTime.ToString()
				? this.DateTimeFormatSetting()
				: this.DateFormatSetting();

			return dateTimeDisplayValue.ToString(format);
		}

		private string DateFormatSetting()
		{
			const string dateSetting = "DateTime/DateFormat";
			string setting = null;

			try
			{
				setting = this.HttpContext.GetSiteSetting(dateSetting);
			}
			catch (Exception e)
			{
				WebEventSource.Log.GenericWarningException(e, $"error finding setting: {dateSetting}");
			}

			return setting ?? CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
		}

		private string DateTimeFormatSetting()
		{
			const string dateTimeSetting = "DateTime/DateTimeFormat";
			string setting = null;

			try
			{
				setting = this.HttpContext.GetSiteSetting(dateTimeSetting);
			}
			catch (Exception e)
			{
				WebEventSource.Log.GenericWarningException(e, $"error finding setting: {dateTimeSetting}");
			}

			return setting ?? this.DateFormatSetting() + " " + this.TimeFormatSetting();
		}

		private string TimeFormatSetting()
		{
			const string timeSetting = "DateTime/TimeFormat";
			string setting = null;

			try
			{
				setting = this.HttpContext.GetSiteSetting(timeSetting);
			}
			catch (Exception e)
			{
				WebEventSource.Log.GenericWarningException(e, $"error finding setting: {timeSetting}");
			}

			return setting ?? CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
		}

		[ActionName("DownloadAsExcel")]
		[HttpGet]
		public ActionResult GetExcelFile(string key)
		{
			using (var stream = Session[key] as MemoryStream)
			{
				if (stream == null)
				{
					return new HttpStatusCodeResult(HttpStatusCode.NoContent);
				}

				Session[key] = null;

				return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", key.Substring(key.IndexOf('|') + 1));
			}
		}

		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult ExecuteWorkflow(EntityReference workflow, EntityReference entity)
		{
			string portalName = null;
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var languageCodeSetting = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, "Language Code");

			if (!string.IsNullOrWhiteSpace(languageCodeSetting))
			{
				int languageCode;
				if (int.TryParse(languageCodeSetting, out languageCode))
				{
					portalName = languageCode.ToString(CultureInfo.InvariantCulture);
				}
			}

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: portalName);
			var serviceContext = dataAdapterDependencies.GetServiceContext();

			var request = new ExecuteWorkflowRequest
			{
				WorkflowId = workflow.Id,
				EntityId = entity.Id
			};

			serviceContext.Execute(request);

			serviceContext.TryRemoveFromCache(entity);

			return new HttpStatusCodeResult(HttpStatusCode.NoContent);
		}

		private string CreateCellReference(int column)
		{
			// A, B, C...Z, AA, AB...ZY, ZZ, AAA, AAB...
			const char firstRef = 'A';
			const uint firstIndex = (uint)firstRef;

			var result = string.Empty;

			while (column > 0)
			{
				var mod = (column - 1) % 26;
				result = (char)(firstIndex + mod) + result;
				column = (column - mod) / 26;
			}

			return result;
		}

		private static string EncodeCommaSeperatedValue(string value)
		{
			return !string.IsNullOrEmpty(value)
				? string.Format(@"""{0}"",", value.Replace(@"""", @""""""))
				: ",";
		}

		private static CreateActionMetadata GetCreationActionMetadata(ViewConfiguration viewConfiguration, IDataAdapterDependencies dataAdapterDependencies)
		{
			if (string.IsNullOrEmpty(viewConfiguration.SubgridFormEntityLogicalName)
				|| viewConfiguration.SubgridFormEntityId == Guid.Empty)
			{
				return CreateActionMetadata.Default;
			}

			if (string.Equals(viewConfiguration.SubgridFormEntityLogicalName, "opportunity", StringComparison.InvariantCultureIgnoreCase)
				&& string.Equals(viewConfiguration.EntityName, "opportunityproduct", StringComparison.InvariantCultureIgnoreCase)
				&& string.Equals(viewConfiguration.ViewRelationshipName, "product_opportunities", StringComparison.InvariantCultureIgnoreCase))
			{
				return GetOpportunityProductCreateActionMetadata(new EntityReference("opportunity", viewConfiguration.SubgridFormEntityId), dataAdapterDependencies);
			}

			return CreateActionMetadata.Default;
		}

		private static CreateActionMetadata GetOpportunityProductCreateActionMetadata(EntityReference opportunity, IDataAdapterDependencies dataAdapterDependencies)
		{
			var serviceContext = dataAdapterDependencies.GetServiceContext();

			var response = (RetrieveResponse)serviceContext.Execute(new RetrieveRequest
			{
				Target = opportunity,
				ColumnSet = new ColumnSet("pricelevelid")
			});

			return new CreateActionMetadata(
				response.Entity.GetAttributeValue<EntityReference>("pricelevelid") == null,
				ResourceManager.GetString("Opportunity_Product_Price_List_Required"));
		}

		private static ViewDataAdapter SetViewDataAdapter(ViewConfiguration viewConfiguration, string sortExpression, string search,
			string filter, string metaFilter, int page, bool applyRecordLevelFilters, bool applyRelatedRecordFilter,
			string filterRelationshipName, string filterEntityName, string filterAttributeName, Guid? filterValue,
			IDictionary<string, string> customParameters, PortalConfigurationDataAdapterDependencies dataAdapterDependencies, CrmWebsite website)
		{
			var viewDataAdapter = applyRelatedRecordFilter &&
								  (!string.IsNullOrWhiteSpace(filterRelationshipName) &&
								   !string.IsNullOrWhiteSpace(filterEntityName))
				? new ViewDataAdapter(viewConfiguration, dataAdapterDependencies, filterRelationshipName,
					filterEntityName,
					filterAttributeName, filterValue ?? Guid.Empty, page, search, sortExpression, filter, metaFilter,
					applyRecordLevelFilters, customParameters: customParameters)
				: new ViewDataAdapter(viewConfiguration, dataAdapterDependencies, page, search, sortExpression, filter,
					metaFilter,
					applyRecordLevelFilters, customParameters: customParameters);

			var siteSettings = new SettingDataAdapter(dataAdapterDependencies, website);
			var multiQueryEntities = (siteSettings.GetValue("Grid/DoQueryPerRecordLevelFilter/Entities") ?? string.Empty)
				.Split(',')
				.ToLookup(e => e, StringComparer.OrdinalIgnoreCase);

			viewDataAdapter.DoQueryPerRecordLevelFilter = multiQueryEntities.Contains(viewConfiguration.EntityName);

			return viewDataAdapter;
		}

		private static IEnumerable<DisabledItemActionLink> DisableActionLinksBasedOnFilterCriteria(OrganizationServiceContext context, EntityMetadata entityMetadata, IEnumerable<ViewActionLink> links, EntityRecord[] records)
		{
			var disabledLinks = new List<DisabledItemActionLink>();

			if (context == null)
			{
				return disabledLinks;
			}

			foreach (var link in links)
			{
				disabledLinks.AddRange(AddActionLinkToDisabledActionLinksList(context, entityMetadata, link, records));
			}

			return disabledLinks;
		}

		private static IEnumerable<DisabledItemActionLink> AddActionLinkToDisabledActionLinksList(OrganizationServiceContext context, EntityMetadata entityMetadata, ViewActionLink link, EntityRecord[] records)
		{
			var disabledLinks = new List<DisabledItemActionLink>();

			if (string.IsNullOrEmpty(link.FilterCriteria) || records == null || !records.Any())
			{
				return disabledLinks;
			}

			// Get Entity Record IDs for filter.
			var ids = records.Select(record => record.Id).Cast<object>().ToList();

			// The condition for the filter on primary key
			var primaryAttributeCondition = new Condition
			{
				Attribute = entityMetadata.PrimaryIdAttribute,
				Operator = ConditionOperator.In,
				Values = ids
			};

			// Primary key filter
			var primaryAttributeFilter = new Filter
			{
				Conditions = new[] { primaryAttributeCondition },
				Type = LogicalOperator.And
			};

			Fetch fetch = null;
			try
			{
				fetch = Fetch.Parse(link.FilterCriteria);
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.Message);
				return disabledLinks;
			}

			if (fetch.Entity == null)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Fetch XML query is not valid. Entity can't be Null.");
				return disabledLinks;
			}
			// Set number of fields to fetch to 0.
			fetch.Entity.Attributes = FetchAttribute.None;

			if (fetch.Entity.Filters == null)
			{
				fetch.Entity.Filters = new List<Filter>();
			}
			// Add primary key filter
			fetch.Entity.Filters.Add(primaryAttributeFilter);

			RetrieveMultipleResponse response;
			try
			{
				response = (RetrieveMultipleResponse)context.Execute(new RetrieveMultipleRequest { Query = fetch.ToFetchExpression() });
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.Message);
				return disabledLinks;
			}

			if (response == null)
			{
				return disabledLinks;
			}

			disabledLinks.AddRange(from record in records
				where response.EntityCollection.Entities.All(entity => entity.Id != record.Id)
				select new DisabledItemActionLink(record.Id, link.FilterCriteriaId));

			return disabledLinks;
		}

		/// <summary>
		/// Filter website related records (for adx_portallanguage only).
		/// </summary>
		/// <param name="records">
		/// The records.
		/// </param>
		/// <param name="website">
		/// The website.
		/// </param>
		/// <returns>
		/// The <see cref="EntityRecord[]"/>.
		/// </returns>
		private static EntityRecord[] FilterWebsiteRelatedRecords(EntityRecord[] records, EntityReference website)
		{
			if (website == null || records.All(r => r.EntityName != "adx_portallanguage"))
			{
				return records;
			}

			return records
				.Where(r => r.Attributes.Any(a => a.Name.Contains("adx_websiteid") && ((EntityReference)a.Value).Id == website.Id))
				.ToArray();
		}

		private static EntityReference[] FilterAlreadyAssociated(OrganizationServiceContext serviceContext, Relationship relationship, EntityReference target, EntityReference[] relatedEntities)
		{
			var metadataResponse = (RetrieveRelationshipResponse)serviceContext.Execute(new RetrieveRelationshipRequest
			{
				Name = relationship.SchemaName
			});

			var manyToManyMetadata = metadataResponse.RelationshipMetadata as ManyToManyRelationshipMetadata;

			if (manyToManyMetadata == null)
			{
				return relatedEntities;
			}

			string targetIntersectAttribute;
			string relatedEntityLogicalName;
			string relatedEntityIntersectAttribute;

			if (string.Equals(manyToManyMetadata.Entity1LogicalName, target.LogicalName, StringComparison.Ordinal))
			{
				targetIntersectAttribute = manyToManyMetadata.Entity1IntersectAttribute;
				relatedEntityLogicalName = manyToManyMetadata.Entity2LogicalName;
				relatedEntityIntersectAttribute = manyToManyMetadata.Entity2IntersectAttribute;
			}
			else
			{
				targetIntersectAttribute = manyToManyMetadata.Entity2IntersectAttribute;
				relatedEntityLogicalName = manyToManyMetadata.Entity1LogicalName;
				relatedEntityIntersectAttribute = manyToManyMetadata.Entity1IntersectAttribute;
			}

			var result = serviceContext.RetrieveMultiple(new QueryExpression(manyToManyMetadata.IntersectEntityName)
			{
				NoLock = true,
				ColumnSet = new ColumnSet(relatedEntityIntersectAttribute),
				Criteria =
				{
					Filters =
					{
						new FilterExpression(LogicalOperator.And)
						{
							Conditions =
							{
								new ConditionExpression(targetIntersectAttribute, ConditionOperator.Equal, target.Id),
								new ConditionExpression(relatedEntityIntersectAttribute, ConditionOperator.In, relatedEntities.Select(e => e.Id).ToArray())
							},
						}
					}
				}
			});

			var alreadyAssociated = result.Entities
				.Select(e => new EntityReference(relatedEntityLogicalName, e.GetAttributeValue<Guid>(relatedEntityIntersectAttribute)));

			return relatedEntities.Except(alreadyAssociated).ToArray();
		}
	}
}
