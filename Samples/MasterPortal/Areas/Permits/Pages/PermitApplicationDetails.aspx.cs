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
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Text;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Site.Pages;
using OrganizationServiceContextExtensions = Microsoft.Xrm.Portal.Cms.OrganizationServiceContextExtensions;

namespace Site.Areas.Permits.Pages
{
	public partial class PermitApplicationDetails : PortalPage
	{
		public Entity Permit { get; set; }

		public string RegardingContactFieldName { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			var reference = Entity.GetAttributeValue<EntityReference>("adx_entityform");

			var entityFormRecord = XrmContext.CreateQuery("adx_entityform").FirstOrDefault(ef => ef.GetAttributeValue<Guid>("adx_entityformid") == reference.Id);

			if (entityFormRecord != null)
			{
				var recordEntityLogicalName = entityFormRecord.GetAttributeValue<string>("adx_entityname");

				Guid recordId;

				if (Guid.TryParse(Request["id"], out recordId))
				{

					var metadataRequest = new RetrieveEntityRequest
					{
						LogicalName = recordEntityLogicalName,
						EntityFilters = EntityFilters.Attributes
					};

					var metadataResponse = (RetrieveEntityResponse)XrmContext.Execute(metadataRequest);

					var primaryFieldLogicalName = metadataResponse.EntityMetadata.PrimaryIdAttribute;

					var permitRecord = XrmContext.CreateQuery(recordEntityLogicalName).FirstOrDefault(r => r.GetAttributeValue<Guid>(primaryFieldLogicalName) == recordId);

					var permitTypeReference = permitRecord.GetAttributeValue<EntityReference>("adx_permittype");

					var permitType =
						XrmContext.CreateQuery("adx_permittype").FirstOrDefault(
							srt => srt.GetAttributeValue<Guid>("adx_permittypeid") == permitTypeReference.Id);

					var entityName = permitType.GetAttributeValue<string>("adx_entityname");

					RegardingContactFieldName = permitType.GetAttributeValue<string>("adx_regardingcontactfieldname");

					var trueMetadataRequest = new RetrieveEntityRequest
					{
						LogicalName = entityName,
						EntityFilters = EntityFilters.Attributes
					};

					var trueMetadataResponse = (RetrieveEntityResponse)XrmContext.Execute(trueMetadataRequest);

					var primaryFieldName = trueMetadataResponse.EntityMetadata.PrimaryIdAttribute;

					var entityId = permitRecord.GetAttributeValue<string>("adx_entityid");

					var trueRecordId = Guid.Parse(entityId);

					var trueRecord = XrmContext.CreateQuery(entityName).FirstOrDefault(r => r.GetAttributeValue<Guid>(primaryFieldName) == trueRecordId);

					Permit = trueRecord;

					var permitDataSource = CreateDataSource("PermitDataSource", entityName, primaryFieldName, trueRecordId);

					var permitFormView = new CrmEntityFormView() { FormName = "Details Form", Mode = FormViewMode.Edit, EntityName = entityName, CssClass = "crmEntityFormView", AutoGenerateSteps = false };

					var languageCodeSetting = OrganizationServiceContextExtensions.GetSiteSettingValueByName(ServiceContext, Portal.Website, "Language Code");
					if (!string.IsNullOrWhiteSpace(languageCodeSetting))
					{
						int languageCode;
						if (int.TryParse(languageCodeSetting, out languageCode))
						{
							permitFormView.LanguageCode = languageCode;
							permitFormView.ContextName = languageCode.ToString(CultureInfo.InvariantCulture);
							permitDataSource.CrmDataContextName = languageCode.ToString(CultureInfo.InvariantCulture);
						}
					}

					CrmEntityFormViewPanel.Controls.Add(permitFormView);

					permitFormView.DataSourceID = permitDataSource.ID;

					var regardingContact = Permit.GetAttributeValue<EntityReference>(RegardingContactFieldName);

					if (regardingContact == null || Contact == null || regardingContact.Id != Contact.Id)
					{
						PermitControls.Enabled = false;
						PermitControls.Visible = false;
						AddNoteInline.Visible = false;
						AddNoteInline.Enabled = false;
					}
					else
					{
						var dataAdapterDependencies =
							new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: PortalName);
						var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
						var annotations = dataAdapter.GetAnnotations(Permit.ToEntityReference(),
							new List<Order> { new Order("createdon") }, respectPermissions: false);

						NotesList.DataSource = annotations;
						NotesList.DataBind();
					}
				}
			}
		}

		private CrmDataSource CreateDataSource(string dataSourceId, string entityName, string entityIdAttribute, Guid? entityId)
		{
			var formViewDataSource = new CrmDataSource
			{
				ID = dataSourceId,
				FetchXml = string.Format(@"<fetch mapping='logical'> <entity name='{0}'> <all-attributes /> <filter type='and'> <condition attribute = '{1}' operator='eq' value='{{{2}}}'/> </filter> </entity> </fetch>",
					entityName,
					entityIdAttribute,
					entityId)
			};

			CrmEntityFormViewPanel.Controls.Add(formViewDataSource);

			return formViewDataSource;
		}

		protected static IHtmlString FormatNote(object text)
		{
			return text == null ? null : new SimpleHtmlFormatter().Format(text.ToString().Replace("*WEB* ", string.Empty));
		}

		protected void AddNote_Click(object sender, EventArgs e)
		{
			var regardingContact = Permit.GetAttributeValue<EntityReference>(RegardingContactFieldName);

			if (regardingContact == null || Contact == null || regardingContact.Id != Contact.Id)
			{
				throw new InvalidOperationException("Unable to retrieve the order.");
			}

			if (!string.IsNullOrEmpty(NewNoteText.Text) || (NewNoteAttachment.PostedFile != null && NewNoteAttachment.PostedFile.ContentLength > 0))
			{
				var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(
					requestContext: Request.RequestContext, portalName: PortalName);

				var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
				
				var annotation = new Annotation
				{
					NoteText = string.Format("{0}{1}", AnnotationHelper.WebAnnotationPrefix, NewNoteText.Text),
					Subject = AnnotationHelper.BuildNoteSubject(dataAdapterDependencies),
					Regarding = Permit.ToEntityReference()
				};
				if (NewNoteAttachment.PostedFile != null && NewNoteAttachment.PostedFile.ContentLength > 0)
				{
					annotation.FileAttachment = AnnotationDataAdapter.CreateFileAttachment(new HttpPostedFileWrapper(NewNoteAttachment.PostedFile));
				}
				dataAdapter.CreateAnnotation(annotation);
			}

			Response.Redirect(Request.Url.PathAndQuery);
		}
	}
}
