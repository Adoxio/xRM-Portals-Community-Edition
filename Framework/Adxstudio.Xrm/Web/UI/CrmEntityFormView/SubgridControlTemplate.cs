/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.HtmlControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.EntityList;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Adxstudio.Xrm.Web.UI.WebForms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Filter = Adxstudio.Xrm.Services.Query.Filter;
using GridMetadata = Adxstudio.Xrm.Web.UI.JsonConfiguration.GridMetadata;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a subgrid.
	/// </summary>
	public class SubgridControlTemplate : SubgridCellTemplate
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="metadata"></param>
		/// <param name="contextName"></param>
		/// <param name="bindings"></param>
		public SubgridControlTemplate(FormXmlCellMetadata metadata, string contextName, IDictionary<string, CellBinding> bindings)
			: base(metadata)
		{
			ContextName = contextName;
			Bindings = bindings;
		}

		/// <summary>
		/// Name of the context the portal binds to
		/// </summary>
		protected string ContextName { get; set; }

		/// <summary>
		/// <see cref="EntityMetadata"/>
		/// </summary>
		protected EntityMetadata EntityMetadata { get; set; }

		/// <summary>
		/// Dictionary of the cell bindings
		/// </summary>
		protected IDictionary<string, CellBinding> Bindings { get; private set; }

		/// <summary>
		/// Control instantiation
		/// </summary>
		/// <param name="container"></param>
		protected override void InstantiateControlIn(HtmlControl container)
		{
			if (string.IsNullOrWhiteSpace(Metadata.ViewID)) return;

			var context = CrmConfigurationManager.CreateContext(ContextName);
			var portal = PortalCrmConfigurationManager.CreatePortalContext(ContextName);
			var user = portal == null ? null : portal.User;

			EntityMetadata = GetEntityMetadata(context);

			var viewId = new Guid(Metadata.ViewID);

			if (viewId == Guid.Empty) return;

			var viewGuids = new[] { viewId };

			if (Metadata.ViewEnableViewPicker && !string.IsNullOrWhiteSpace(Metadata.ViewIds))
			{
				var viewids = Metadata.ViewIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (viewids.Length >= 1)
				{
					viewGuids = Array.ConvertAll(viewids, Guid.Parse).AsEnumerable().OrderBy(o => o != viewId).ThenBy(o => o).ToArray();
				}
			}

			Bindings[Metadata.ControlID + "CrmEntityId"] = new CellBinding
			{
				Get = () => null,
				Set = obj =>
					{
						var id = obj.ToString();
						Guid entityId;

						if (!Guid.TryParse(id, out entityId)) return;

						var subgridHtml = BuildGrid(container, context, viewGuids, viewId, entityId, user);
						var subgrid = new HtmlGenericControl("div") { ID = Metadata.ControlID, InnerHtml = subgridHtml.ToString() };
						subgrid.Attributes.Add("class", "subgrid");
						container.Controls.Add(subgrid);
					}
			};

			if (!container.Page.IsPostBack) return;

			// On PostBack no databinding occurs so get the id from the viewstate stored on the CrmEntityFormView control.
			var crmEntityId = Metadata.FormView.CrmEntityId;

			if (crmEntityId == null) return;

			var gridHtml = BuildGrid(container, context, viewGuids, viewId, (Guid)crmEntityId, user);
			var subgridControl = new HtmlGenericControl("div") { ID = Metadata.ControlID, InnerHtml = gridHtml.ToString() };
			subgridControl.Attributes.Add("class", "subgrid");
			container.Controls.Add(subgridControl);
		}

		/// <summary>
		/// Creates the HTML to render a subgrid listing of records defined by the associated view
		/// </summary>
		protected virtual IHtmlString BuildGrid(HtmlControl container, OrganizationServiceContext context, Guid[] viewGuids,
			Guid viewId, Guid entityId, Entity user)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(Metadata.FormView.ContextName);
			var html = Mvc.Html.EntityExtensions.GetHtmlHelper(Metadata.FormView.ContextName, container.Page.Request.RequestContext, container.Page.Response);
			var source = new EntityReference(Metadata.TargetEntityName, entityId);
			var relationship = new Relationship(Metadata.ViewRelationshipName);

			var metadataManyToMany = EntityMetadata.ManyToManyRelationships.FirstOrDefault(r => r.SchemaName == Metadata.ViewRelationshipName);
			var relationshipManyToOne = EntityMetadata.ManyToOneRelationships.FirstOrDefault(r => r.SchemaName == Metadata.ViewRelationshipName);
			var relationshipOneToMany = EntityMetadata.OneToManyRelationships.FirstOrDefault(r => r.SchemaName == Metadata.ViewRelationshipName);

			if (relationshipManyToOne != null && relationshipOneToMany != null) // self referential
			{
				relationship.PrimaryEntityRole = EntityRole.Referenced;
			}
			else if (metadataManyToMany != null && metadataManyToMany.Entity1LogicalName == metadataManyToMany.Entity2LogicalName)
			{
				relationship.PrimaryEntityRole = EntityRole.Referencing;
			}

			var settings = Metadata.SubgridSettings;

			var viewConfigurations = new List<ViewConfiguration>();

			var cssClass = (settings != null) ? settings.CssClass : null;
			var gridCssClass = (settings != null) ? settings.GridCssClass : null;
			var gridColumnWidthStyle = (settings != null && settings.GridColumnWidthStyle != null) ? settings.GridColumnWidthStyle.GetValueOrDefault(EntityGridExtensions.GridColumnWidthStyle.Percent) : EntityGridExtensions.GridColumnWidthStyle.Percent;

			var accessDeniedMessage = (settings != null) ? Localization.GetLocalizedString(settings.AccessDeniedMessage, Metadata.LanguageCode) : null;
			var errorMessage = (settings != null) ? Localization.GetLocalizedString(settings.ErrorMessage, Metadata.LanguageCode) : null;
			var loadingMessage = (settings != null) ? Localization.GetLocalizedString(settings.LoadingMessage, Metadata.LanguageCode) : null;
			var emptyMessage = (settings != null) ? Localization.GetLocalizedString(settings.EmptyMessage, Metadata.LanguageCode) : null;

			var modalLookup = (settings != null) ? settings.LookupDialog : null;

			var modalLookupSelectedRecordsTitle = modalLookup == null ? null : Localization.GetLocalizedString(modalLookup.SelectRecordsTitle, Metadata.LanguageCode);
			var modalLookupCssClass = modalLookup == null ? null : modalLookup.CssClass;
			var modalLookupTitle = modalLookup == null ? null : Localization.GetLocalizedString(modalLookup.Title, Metadata.LanguageCode);
			var modalLookupTitleCssClass = modalLookup == null ? null : modalLookup.TitleCssClass;
			var modalLookupPrimaryButtonText = modalLookup == null ? null : Localization.GetLocalizedString(modalLookup.PrimaryButtonText, Metadata.LanguageCode);
			var modalLookupDismissButtonSrText = modalLookup == null ? null : Localization.GetLocalizedString(modalLookup.DismissButtonSrText, Metadata.LanguageCode);
			var modalLookupCloseButtonText = modalLookup == null ? null : Localization.GetLocalizedString(modalLookup.CloseButtonText, Metadata.LanguageCode);
			var modalLookupPrimaryButtonCssClass = modalLookup == null ? null : modalLookup.PrimaryButtonCssClass;
			var modalLookupCloseButtonCssClass = modalLookup == null ? null : modalLookup.CloseButtonCssClass;
			var modalLookupGridContainerCssClass = modalLookup == null ? null : modalLookup.GridSettings == null ? null : modalLookup.GridSettings.CssClass;
			var modalLookupGridCssClass = modalLookup == null ? null : modalLookup.GridSettings == null ? null : modalLookup.GridSettings.GridCssClass;
			var modalLookupGridLoadingMessage = modalLookup == null ? null : modalLookup.GridSettings == null ? null : Localization.GetLocalizedString(modalLookup.GridSettings.LoadingMessage, Metadata.LanguageCode);
			var modalLookupGridErrorMessage = modalLookup == null ? null : modalLookup.GridSettings == null ? null : Localization.GetLocalizedString(modalLookup.GridSettings.ErrorMessage, Metadata.LanguageCode);
			var modalLookupGridAccessDeniedMessage = modalLookup == null ? null : modalLookup.GridSettings == null ? null : Localization.GetLocalizedString(modalLookup.GridSettings.AccessDeniedMessage, Metadata.LanguageCode);
			var modalLookupGridEmptyMessage = modalLookup == null ? null : modalLookup.GridSettings == null ? null : Localization.GetLocalizedString(modalLookup.GridSettings.EmptyMessage, Metadata.LanguageCode);
			var modalLookupDefaultErrorMessage = modalLookup == null ? null : Localization.GetLocalizedString(modalLookup.DefaultErrorMessage, Metadata.LanguageCode);

			var modalDetailsForm = (settings != null) ? settings.DetailsFormDialog : null;
			var modalDetailsFormCssClass = modalDetailsForm == null ? null : modalDetailsForm.CssClass;
			var modalDetailsFormTitle = modalDetailsForm == null ? null : Localization.GetLocalizedString(modalDetailsForm.Title, Metadata.LanguageCode);
			var modalDetailsFormTitleCssClass = modalDetailsForm == null ? null : modalDetailsForm.TitleCssClass;
			var modalDetailsFormLoadingMessage = modalDetailsForm == null ? null : Localization.GetLocalizedString(modalDetailsForm.LoadingMessage, Metadata.LanguageCode);
			var modalDetailsFormDismissButtonSrText = modalDetailsForm == null ? null : Localization.GetLocalizedString(modalDetailsForm.DismissButtonSrText, Metadata.LanguageCode);
			var modalDetailsFormSize = (modalDetailsForm != null && modalDetailsForm.Size != null) ? modalDetailsForm.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Large) : BootstrapExtensions.BootstrapModalSize.Large;

			var modalEditForm = (settings != null) ? settings.EditFormDialog : null;
			var modalEditFormCssClass = modalEditForm == null ? null : modalEditForm.CssClass;
			var modalEditFormTitle = modalEditForm == null ? null : Localization.GetLocalizedString(modalEditForm.Title, Metadata.LanguageCode);
			var modalEditFormTitleCssClass = modalEditForm == null ? null : modalEditForm.TitleCssClass;
			var modalEditFormLoadingMessage = modalEditForm == null ? null : Localization.GetLocalizedString(modalEditForm.LoadingMessage, Metadata.LanguageCode);
			var modalEditFormDismissButtonSrText = modalEditForm == null ? null : Localization.GetLocalizedString(modalEditForm.DismissButtonSrText, Metadata.LanguageCode);
			var modalEditFormSize = (modalEditForm != null && modalEditForm.Size != null) ? modalEditForm.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Large) : BootstrapExtensions.BootstrapModalSize.Large;

			var modalCreateForm = (settings != null) ? settings.CreateFormDialog : null;
			var modalCreateFormCssClass = modalCreateForm == null ? null : modalCreateForm.CssClass;
			var modalCreateFormTitle = modalCreateForm == null ? null : Localization.GetLocalizedString(modalCreateForm.Title, Metadata.LanguageCode);
			var modalCreateFormTitleCssClass = modalCreateForm == null ? null : modalCreateForm.TitleCssClass;
			var modalCreateFormLoadingMessage = modalCreateForm == null ? null : Localization.GetLocalizedString(modalCreateForm.LoadingMessage, Metadata.LanguageCode);
			var modalCreateFormDismissButtonSrText = modalCreateForm == null ? null : Localization.GetLocalizedString(modalCreateForm.DismissButtonSrText, Metadata.LanguageCode);
			var modalCreateFormSize = (modalCreateForm != null && modalCreateForm.Size != null) ? modalCreateForm.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Large) : BootstrapExtensions.BootstrapModalSize.Large;


			var modalDelete = (settings != null) ? settings.DeleteDialog : null;
			var modalDeleteCssClass = modalDelete == null ? null : modalDelete.CssClass;
			var modalDeleteTitle = modalDelete == null ? null : Localization.GetLocalizedString(modalDelete.Title, Metadata.LanguageCode);
			var modalDeleteTitleCssClass = modalDelete == null ? null : modalDelete.TitleCssClass;
			var modalDeletePrimaryButtonText = modalDelete == null ? null : Localization.GetLocalizedString(modalDelete.PrimaryButtonText, Metadata.LanguageCode);
			var modalDeleteCloseButtonText = modalDelete == null ? null : Localization.GetLocalizedString(modalDelete.CloseButtonText, Metadata.LanguageCode);
			var modalDeleteDismissButtonSrText = modalDelete == null ? null : Localization.GetLocalizedString(modalDelete.DismissButtonSrText, Metadata.LanguageCode);
			var modalDeleteSize = (modalDelete != null && modalDelete.Size != null) ? modalDelete.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Default) : BootstrapExtensions.BootstrapModalSize.Default;
			var modalDeleteBody = modalDelete == null ? null : Localization.GetLocalizedString(modalDelete.Confirmation, Metadata.LanguageCode);

			var modalError = (settings != null) ? settings.ErrorDialog : null;
			var modalErrorSize = (modalError != null && modalError.Size != null) ? modalError.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Default) : BootstrapExtensions.BootstrapModalSize.Default;
			var modalErrorCssClass = modalError == null ? null : modalError.CssClass;
			var modalErrorTitle = modalError == null ? null : Localization.GetLocalizedString(modalError.Title, Metadata.LanguageCode);
			var modalErrorTitleCssClass = modalError == null ? null : modalError.TitleCssClass;
			var modalErrorDismissButtonSrText = modalError == null ? null : Localization.GetLocalizedString(modalError.DismissButtonSrText, Metadata.LanguageCode);
			var modalErrorCloseButtonText = modalError == null ? null : Localization.GetLocalizedString(modalError.CloseButtonText, Metadata.LanguageCode);
			var modalErrorBody = modalError == null ? null : Localization.GetLocalizedString(modalError.Body, Metadata.LanguageCode);

			var modalLookupGridPageSize = html.IntegerSetting("Portal/Lookup/Modal/Grid/PageSize") ?? 10;
			var modalLookupSizeSetting = html.Setting("Portal/Lookup/Modal/Size");
			var modalLookupSize = BootstrapExtensions.BootstrapModalSize.Large;
			if (modalLookupSizeSetting != null && modalLookupSizeSetting.ToLower() == "default") modalLookupSize = BootstrapExtensions.BootstrapModalSize.Default;
			if (modalLookupSizeSetting != null && modalLookupSizeSetting.ToLower() == "small") modalLookupSize = BootstrapExtensions.BootstrapModalSize.Small;

			var modalCreateRelatedRecord = (settings != null) ? settings.CreateRelatedRecordDialog : null;
			var modalCreateRelatedRecordCssClass = modalCreateRelatedRecord == null ? null : modalCreateRelatedRecord.CssClass;
			var modalCreateRelatedRecordTitle = modalCreateRelatedRecord == null ? null : Localization.GetLocalizedString(modalCreateRelatedRecord.Title, Metadata.LanguageCode);
			var modalCreateRelatedRecordTitleCssClass = modalCreateRelatedRecord == null ? null : modalCreateRelatedRecord.TitleCssClass;
			var modalCreateRelatedRecordLoadingMessage = modalCreateRelatedRecord == null ? null : Localization.GetLocalizedString(modalCreateRelatedRecord.LoadingMessage, Metadata.LanguageCode);
			var modalCreateRelatedRecordDismissButtonSrText = modalCreateRelatedRecord == null ? null : Localization.GetLocalizedString(modalCreateRelatedRecord.DismissButtonSrText, Metadata.LanguageCode);
			var modalCreateRelatedRecordSize = (modalCreateRelatedRecord != null && modalCreateRelatedRecord.Size != null) ? modalCreateRelatedRecord.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Large) : BootstrapExtensions.BootstrapModalSize.Large;

			foreach (var viewGuid in viewGuids)
			{
				var savedQueryView = new SavedQueryView(context, viewGuid, Metadata.LanguageCode);

				if (savedQueryView.FetchXml == null) continue;

				var fetch = Fetch.Parse(savedQueryView.FetchXml);

				AddFiltersToFetch(context, fetch, entityId.ToString());

				var viewConfiguration = new ViewConfiguration(portalContext, savedQueryView, Metadata.ViewRecordsPerPage ?? 0,
					fetch.ToXml().ToString(), new ViewSearch(Metadata.ViewEnableQuickFind), Metadata.FormView.EnableEntityPermissions,
					settings, Metadata.LanguageCode, ContextName, Metadata.ViewRelationshipName, Metadata.ViewTargetEntityType, viewGuid,
					modalLookupGridPageSize)
				{
					SubgridFormEntityId = entityId,
					SubgridFormEntityLogicalName = Metadata.TargetEntityName
				};

				viewConfigurations.Add(viewConfiguration);
			}

			return !string.IsNullOrWhiteSpace(Metadata.ViewRelationshipName)
				? html.EntitySubGrid(source, relationship, viewConfigurations,
					EntityListFunctions.BuildControllerActionUrl("GetSubgridData", "EntityGrid", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id }), user,
					string.Join(" ", new[] { CssClass, cssClass }).TrimEnd(' '),
					string.Join(" ", new[] { "table-striped", gridCssClass }).TrimEnd(' '),
					gridColumnWidthStyle, EntityGridExtensions.GridSelectMode.None, null, loadingMessage,
					errorMessage, accessDeniedMessage, emptyMessage, Metadata.FormView.ContextName, Metadata.LanguageCode, false, true,
					modalDetailsFormSize, modalDetailsFormCssClass, modalDetailsFormTitle, modalDetailsFormLoadingMessage, modalDetailsFormDismissButtonSrText,
					modalDetailsFormTitleCssClass, modalCreateFormSize, modalCreateFormCssClass, modalCreateFormTitle,
					modalCreateFormLoadingMessage, modalCreateFormDismissButtonSrText, modalCreateFormTitleCssClass, modalEditFormSize,
					modalEditFormCssClass, modalEditFormTitle, modalEditFormLoadingMessage, modalEditFormDismissButtonSrText,
					modalEditFormTitleCssClass, modalDeleteSize, modalDeleteCssClass, modalDeleteTitle, modalDeleteBody,
					modalDeletePrimaryButtonText, modalDeleteCloseButtonText, modalDeleteDismissButtonSrText, modalDeleteTitleCssClass,
					modalErrorSize, modalErrorCssClass, modalErrorTitle, modalErrorBody, modalErrorDismissButtonSrText,
					modalErrorCloseButtonText, modalErrorTitleCssClass, modalLookupCssClass,
					modalLookupTitle, modalLookupSelectedRecordsTitle, modalLookupPrimaryButtonText, modalLookupCloseButtonText,
					modalLookupDismissButtonSrText, modalLookupTitleCssClass, modalLookupPrimaryButtonCssClass,
					modalLookupCloseButtonCssClass, modalLookupGridContainerCssClass, modalLookupGridCssClass,
					modalLookupGridLoadingMessage, modalLookupGridErrorMessage, modalLookupGridAccessDeniedMessage,
					modalLookupGridEmptyMessage, modalLookupDefaultErrorMessage, modalLookupSize, modalCreateRelatedRecordSize, modalCreateRelatedRecordCssClass, modalCreateRelatedRecordTitle, modalCreateRelatedRecordLoadingMessage,
				modalCreateRelatedRecordDismissButtonSrText, modalCreateRelatedRecordTitleCssClass)
				: html.EntitySubGrid(viewConfigurations,
					EntityListFunctions.BuildControllerActionUrl("GetSubgridData", "EntityGrid", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id }), user,
					string.Join(" ", new[] { CssClass, cssClass }).TrimEnd(' '),
					string.Join(" ", new[] { "table-striped", gridCssClass }).TrimEnd(' '), gridColumnWidthStyle,
					EntityGridExtensions.GridSelectMode.None, null, loadingMessage, errorMessage, accessDeniedMessage, emptyMessage,
					Metadata.FormView.ContextName, Metadata.LanguageCode, false, true, modalDetailsFormSize, modalDetailsFormCssClass,
					modalDetailsFormTitle, modalDetailsFormLoadingMessage, modalDetailsFormDismissButtonSrText,
					modalDetailsFormTitleCssClass, modalCreateFormSize, modalCreateFormCssClass, modalCreateFormTitle,
					modalCreateFormLoadingMessage, modalCreateFormDismissButtonSrText, modalCreateFormTitleCssClass, modalEditFormSize,
					modalEditFormCssClass, modalEditFormTitle, modalEditFormLoadingMessage, modalEditFormDismissButtonSrText,
					modalEditFormTitleCssClass, modalDeleteSize, modalDeleteCssClass, modalDeleteTitle, modalDeleteBody,
					modalDeletePrimaryButtonText, modalDeleteCloseButtonText, modalDeleteDismissButtonSrText, modalDeleteTitleCssClass,
					modalErrorSize, modalErrorCssClass, modalErrorTitle, modalErrorBody, modalErrorDismissButtonSrText,
					modalErrorCloseButtonText, modalErrorTitleCssClass, modalCreateRelatedRecordSize, modalCreateRelatedRecordCssClass, modalCreateRelatedRecordTitle, modalCreateRelatedRecordLoadingMessage,
				modalCreateRelatedRecordDismissButtonSrText, modalCreateRelatedRecordTitleCssClass);
		}

		protected EntityMetadata GetEntityMetadata(OrganizationServiceContext serviceContext)
		{
			var entityRequest = new RetrieveEntityRequest
			{
				RetrieveAsIfPublished = false,
				LogicalName = Metadata.ViewTargetEntityType,
				EntityFilters = EntityFilters.All
			};

			var entityResponse = serviceContext.Execute(entityRequest) as RetrieveEntityResponse;

			if (entityResponse == null)
			{
				throw new ApplicationException(string.Format("RetrieveEntityRequest failed for view target entity type {0}", Metadata.ViewTargetEntityType));
			}

			return entityResponse.EntityMetadata;
		}

		/// <summary>
		/// Add the necessary filter conditions to filter related records
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="fetch"><see cref="Fetch"/></param>
		/// <param name="id">Id of the record to filter related</param>
		protected void AddFiltersToFetch(OrganizationServiceContext serviceContext, Fetch fetch, string id)
		{
			if (string.IsNullOrWhiteSpace(Metadata.ViewRelationshipName))
			{
				return;
			}

			var linkEntityAliasGenerator = LinkEntityAliasGenerator.CreateInstance(fetch);
			var metadataManyToMany = EntityMetadata.ManyToManyRelationships.FirstOrDefault(r => r.SchemaName == Metadata.ViewRelationshipName);
			var relationshipManyToOne = EntityMetadata.ManyToOneRelationships.FirstOrDefault(r => r.SchemaName == Metadata.ViewRelationshipName);
			var relationshipOneToMany = EntityMetadata.OneToManyRelationships.FirstOrDefault(r => r.SchemaName == Metadata.ViewRelationshipName);

			if (metadataManyToMany != null)
			{
				var linkIntersectName = metadataManyToMany.IntersectEntityName;
				string linkTargetFromAttribute;
				string linkTargetToAttribute;
				string linkIntersectFromAttribute;
				string linkIntersectToAttribute;
				if (metadataManyToMany.Entity1LogicalName == metadataManyToMany.Entity2LogicalName)
				{
					linkIntersectFromAttribute = metadataManyToMany.Entity2IntersectAttribute;
					linkIntersectToAttribute = EntityMetadata.PrimaryIdAttribute;
					linkTargetFromAttribute = EntityMetadata.PrimaryIdAttribute;
					linkTargetToAttribute = metadataManyToMany.Entity1IntersectAttribute;
				}
				else
				{
					linkTargetFromAttribute = linkTargetToAttribute = metadataManyToMany.Entity1LogicalName == Metadata.TargetEntityName
						? metadataManyToMany.Entity1IntersectAttribute
						: metadataManyToMany.Entity2IntersectAttribute;
					linkIntersectFromAttribute = linkIntersectToAttribute = metadataManyToMany.Entity1LogicalName == Metadata.ViewTargetEntityType
						? metadataManyToMany.Entity1IntersectAttribute
						: metadataManyToMany.Entity2IntersectAttribute;
				}

				var link = new Link
				{
					Name = linkIntersectName,
					FromAttribute = linkIntersectFromAttribute,
					ToAttribute = linkIntersectToAttribute,
					Intersect = true,
					Visible = false,
					Links = new List<Link>
					{
						new Link
						{
							Name = Metadata.TargetEntityName,
							FromAttribute = linkTargetFromAttribute,
							ToAttribute = linkTargetToAttribute,
							Alias = linkEntityAliasGenerator.CreateUniqueAlias(Metadata.TargetEntityName),
							Filters = new List<Filter>
							{
								new Filter
								{
									Type = LogicalOperator.And,
									Conditions = new List<Condition>
									{
										new Condition
										{
											Attribute = linkTargetFromAttribute,
											Operator = ConditionOperator.Equal,
											Value = id
										}
									}
								}
							}
						}
					}
				};

				if (fetch.Entity.Links == null)
				{
					fetch.Entity.Links = new List<Link> { link };
				}
				else
				{
					fetch.Entity.Links.Add(link);
				}
			}
			else if (relationshipManyToOne != null)
			{
				var link = new Link
				{
					Name = Metadata.TargetEntityName,
					FromAttribute = relationshipManyToOne.ReferencedAttribute,
					ToAttribute = relationshipManyToOne.ReferencingAttribute,
					Alias = linkEntityAliasGenerator.CreateUniqueAlias(Metadata.TargetEntityName),
					Filters = new List<Filter>
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Conditions = new List<Condition>
							{
								new Condition
								{
									Attribute = relationshipManyToOne.ReferencedAttribute,
									Operator = ConditionOperator.Equal,
									Value = id
								}
							}
						}
					}
				};

				if (fetch.Entity.Links == null)
				{
					fetch.Entity.Links = new List<Link> { link };
				}
				else
				{
					fetch.Entity.Links.Add(link);
				}
			}
			else if (relationshipOneToMany != null)
			{
				var attribute = relationshipOneToMany.ReferencedEntity == Metadata.TargetEntityName
					? relationshipOneToMany.ReferencedAttribute
					: relationshipOneToMany.ReferencingAttribute;

				var filter = new Filter
				{
					Type = LogicalOperator.And,
					Conditions = new List<Condition>
					{
						new Condition
						{
							Attribute = attribute,
							Operator = ConditionOperator.Equal,
							Value = id
						}
					}
				};

				AddFilterToFetch(fetch, filter);
			}
			else
			{
				throw new ApplicationException(string.Format("RetrieveRelationshipRequest failed for view relationship name {0}", Metadata.ViewRelationshipName));
			}


		}

		private void AddFilterToFetch(Fetch fetch, Filter filter)
		{
			if (fetch.Entity.Filters == null)
			{
				fetch.Entity.Filters = new List<Filter>
				{
					new Filter { Type = LogicalOperator.And, Filters = new List<Filter> { filter } }
				};
			}
			else
			{
				var rootAndFilter = fetch.Entity.Filters.FirstOrDefault(f => f.Type == LogicalOperator.And);

				if (rootAndFilter != null)
				{
					rootAndFilter.Conditions.Add(filter.Conditions.First());
				}
				else
				{
					fetch.Entity.Filters.Add(new Filter
					{
						Type = LogicalOperator.And,
						Filters = new List<Filter> { filter }
					});
				}
			}
		}
	}
}
