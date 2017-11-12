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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Partner;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Lookup field with a record lookup dialog.
	/// </summary>
	public class ModalLookupControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		/// <remarks>Localization of lookup display text is provided by retrieving the entity metadata and determine the primary name field and if the language code is not the base language for the organization it appends _ + language code to the primary name field and populates the control with the values from the localized attribute. i.e. if the primary name field is new_name and the language code is 1036 for French, the localized attribute name would be new_name_1036. An attribute would be added to the entity in this manner for each language to be supported and the attribute must be added to the view assigned to the lookup on the form.</remarks>
		public ModalLookupControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
		}

		/// <summary>
		/// Override the default RequiredFieldValidator class to allow validation of a hidden field.
		/// </summary>
		public class HiddenFieldValidator : RequiredFieldValidator
		{
			protected override bool ControlPropertiesValid()
			{
				return true;
			}
		}
		
		/// <summary>
		/// Form field.
		/// </summary>
		public CrmEntityFormViewField Field { get; private set; }

		/// <summary>
		/// CSS Class name(s) added to the table cell containing this control
		/// </summary>
		public override string CssClass
		{
			get { return "lookup form-control"; }
		}

		private string ValidationText
		{
			get { return Metadata.ValidationText; }
		}

		private ValidatorDisplay ValidatorDisplay
		{
			get { return string.IsNullOrWhiteSpace(ValidationText) ? ValidatorDisplay.None : ValidatorDisplay.Dynamic; }
		}
		
		protected override bool LabelIsAssociated
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Initialize an instance of the control
		/// </summary>
		protected override void InstantiateControlIn(Control container)
		{
			var inputGroup = new HtmlGenericControl("div");
			if (Metadata.FormView.Mode != FormViewMode.ReadOnly && !Metadata.ReadOnly && !Metadata.Disabled)
			{
				inputGroup.Attributes.Add("class", "input-group");
			}
			var textbox = new TextBox { ID = string.Format("{0}_name", ControlID), CssClass = string.Join(" ", "text form-control", CssClass, Metadata.CssClass), ToolTip = Metadata.ToolTip };
			textbox.Attributes.Add("readonly", string.Empty);
			textbox.Attributes.Add("aria-readonly", "true");
			textbox.Attributes.Add("aria-labelledby", string.Format("{0}_label", ControlID));
			textbox.Attributes.Add("aria-label", Metadata.Label);
			if (Metadata.FormView.Mode == FormViewMode.ReadOnly || Metadata.ReadOnly)
			{
				textbox.Enabled = false;
				textbox.Attributes.Add("aria-disabled", "true");
			}
			var hiddenValue = new HtmlInputHidden { ID = ControlID };
			var hiddenValueEntityName = new HtmlInputHidden { ID = string.Format("{0}_entityname", ControlID) };
			inputGroup.Controls.Add(textbox);
			inputGroup.Controls.Add(hiddenValue);
			inputGroup.Controls.Add(hiddenValueEntityName);
			container.Controls.Add(inputGroup);

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				hiddenValue.Attributes.Add("aria-required", "true");
				textbox.Attributes.Add("aria-labelledby", string.Empty);
				textbox.Attributes["aria-label"] = string.Format(ResourceManager.GetString("Required_Field_Error"), Metadata.Label);
			}

			if (Metadata.FormView.Mode != FormViewMode.ReadOnly && !Metadata.ReadOnly && !Metadata.Disabled)
			{
				var buttonGroup = new HtmlGenericControl("div");
				buttonGroup.Attributes.Add("class", "input-group-btn");
				var clearLookupField = new HtmlGenericControl("button");
				clearLookupField.Attributes.Add("type", "button");
				clearLookupField.Attributes.Add("class", "btn btn-default clearlookupfield");
				clearLookupField.InnerHtml = "<span class='sr-only'>" + ResourceManager.GetString("Clear_Lookup_Field") + "</span><span class='fa fa-times' aria-hidden='true'></span>";
				clearLookupField.Attributes.Add("title", ResourceManager.GetString("Clear_Lookup_Field"));
				clearLookupField.Attributes.Add("aria-label", ResourceManager.GetString("Clear_Lookup_Field"));
				buttonGroup.Controls.Add(clearLookupField);
				var launchModalLink = new HtmlGenericControl("button");
				launchModalLink.Attributes.Add("type", "button");
				launchModalLink.Attributes.Add("class", "btn btn-default launchentitylookup");
				launchModalLink.InnerHtml = "<span class='sr-only'>" + ResourceManager.GetString("Launch_Lookup_Modal") + "</span><span class='fa fa-search' aria-hidden='true'></span>";
				launchModalLink.Attributes.Add("title", ResourceManager.GetString("Launch_Lookup_Modal"));
				launchModalLink.Attributes.Add("aria-label", ResourceManager.GetString("Launch_Lookup_Modal"));
				buttonGroup.Controls.Add(launchModalLink);
				inputGroup.Controls.Add(buttonGroup);
				var lookupModalHtml = BuildLookupModal(container);
				var lookupModalControl = new HtmlGenericControl("div")
				{
					ID = string.Format("{0}_lookupmodal", Metadata.ControlID),
					InnerHtml = lookupModalHtml.ToString()
				};
				lookupModalControl.Attributes.Add("class", "lookup-modal");
				container.Controls.Add(lookupModalControl);
			}

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					Guid id;
					return !Guid.TryParse(hiddenValue.Value, out id) || string.IsNullOrWhiteSpace(hiddenValueEntityName.Value) ||
						   !Metadata.LookupTargets.Contains(hiddenValueEntityName.Value)
						? null
						: new EntityReference(hiddenValueEntityName.Value, id);
				},
				Set = obj =>
				{
					var entityReference = (EntityReference)obj;
					hiddenValue.Value = entityReference.Id.ToString();
					textbox.Text = entityReference.Name ?? string.Empty;
					hiddenValueEntityName.Value = entityReference.LogicalName;
				}
			};
		}

		/// <summary>
		/// Initialize validator controls
		/// </summary>
		protected override void InstantiateValidatorsIn(Control container)
		{
			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				container.Controls.Add(new HiddenFieldValidator
				{
					ID = string.Format("RequiredFieldValidator{0}", ControlID),
					ControlToValidate = ControlID,
					ValidationGroup = ValidationGroup,
					Display = ValidatorDisplay,
					ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.RequiredFieldValidationErrorMessage) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("required")) ? ResourceManager.GetString("Required_Field_Error").FormatWith(Metadata.Label) : Metadata.Messages["required"].FormatWith(Metadata.Label) : Metadata.RequiredFieldValidationErrorMessage)),
					Text = Metadata.ValidationText,
				});
			}

			this.InstantiateCustomValidatorsIn(container);
		}

		/// <summary>
		/// Generate the HTML to render a modal lookup records dialog.
		/// </summary>
		protected virtual IHtmlString BuildLookupModal(Control container)
		{
			var html = Mvc.Html.EntityExtensions.GetHtmlHelper(Metadata.FormView.ContextName, container.Page.Request.RequestContext, container.Page.Response); //new HtmlHelper(new ViewContext(), new ViewPage());
			var context = CrmConfigurationManager.CreateContext(Metadata.FormView.ContextName);
			var portal = PortalCrmConfigurationManager.CreatePortalContext(Metadata.FormView.ContextName);
			var user = portal == null ? null : portal.User;
			var viewConfigurations = new List<ViewConfiguration>();
			var defaultViewId = Metadata.LookupViewID;
			var modalGridSearchPlaceholderText = html.SnippetLiteral("Portal/Lookup/Modal/Grid/Search/PlaceholderText");
			var modalGridSearchTooltipText = html.SnippetLiteral("Portal/Lookup/Modal/Grid/Search/TooltipText");
			var modalGridPageSize = html.IntegerSetting("Portal/Lookup/Modal/Grid/PageSize") ?? 10;
			var modalSizeSetting = html.Setting("Portal/Lookup/Modal/Size");
			var modalSize = BootstrapExtensions.BootstrapModalSize.Large;
			if (modalSizeSetting != null && modalSizeSetting.ToLower() == "default") modalSize = BootstrapExtensions.BootstrapModalSize.Default;
			if (modalSizeSetting != null && modalSizeSetting.ToLower() == "small") modalSize = BootstrapExtensions.BootstrapModalSize.Small;

			var formEntityReferenceInfo = GetFormEntityReferenceInfo(container.Page.Request);
			
			if (defaultViewId == Guid.Empty)
			{
				// By default a lookup field cell defined in the form XML does not contain view parameters unless the user has specified a view that is not the default for that entity and we must query to find the default view.  Saved Query entity's QueryType code 64 indicates a lookup view.

				viewConfigurations.AddRange(
					Metadata.LookupTargets.Select(
						target => new ViewConfiguration(new SavedQueryView(context, target, 64, true, Metadata.LanguageCode), modalGridPageSize)
						{
							Search = new ViewSearch(!Metadata.LookupDisableQuickFind) { PlaceholderText = modalGridSearchPlaceholderText, TooltipText = modalGridSearchTooltipText },
							EnableEntityPermissions = Metadata.FormView.EnableEntityPermissions,
							PortalName = Metadata.FormView.ContextName,
							LanguageCode = Metadata.LanguageCode,
							ModalLookupAttributeLogicalName = Metadata.DataFieldName,
							ModalLookupEntityLogicalName = Metadata.TargetEntityName,
							ModalLookupFormReferenceEntityId = formEntityReferenceInfo.Item2,
							ModalLookupFormReferenceEntityLogicalName = formEntityReferenceInfo.Item1,
							ModalLookupFormReferenceRelationshipName = formEntityReferenceInfo.Item3,
							ModalLookupFormReferenceRelationshipRole = formEntityReferenceInfo.Item4
						}));
			}
			else
			{
				viewConfigurations.Add(new ViewConfiguration(new SavedQueryView(context, defaultViewId, Metadata.LanguageCode), modalGridPageSize)
				{
					Search = new ViewSearch(!Metadata.LookupDisableQuickFind) { PlaceholderText = modalGridSearchPlaceholderText, TooltipText = modalGridSearchTooltipText },
					EnableEntityPermissions = Metadata.FormView.EnableEntityPermissions,
					PortalName = Metadata.FormView.ContextName,
					LanguageCode = Metadata.LanguageCode,
					ModalLookupAttributeLogicalName = Metadata.DataFieldName,
					ModalLookupEntityLogicalName = Metadata.TargetEntityName,
					ModalLookupFormReferenceEntityId = formEntityReferenceInfo.Item2,
					ModalLookupFormReferenceEntityLogicalName = formEntityReferenceInfo.Item1,
					ModalLookupFormReferenceRelationshipName = formEntityReferenceInfo.Item3,
					ModalLookupFormReferenceRelationshipRole = formEntityReferenceInfo.Item4
				});
			}

			if (!Metadata.LookupDisableViewPicker && !string.IsNullOrWhiteSpace(Metadata.LookupAvailableViewIds))
			{
				var addViewConfigurations = new List<ViewConfiguration>();
				var viewids = Metadata.LookupAvailableViewIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (viewids.Length >= 1)
				{
					var viewGuids = Array.ConvertAll(viewids, Guid.Parse).AsEnumerable().OrderBy(o => o != defaultViewId).ThenBy(o => o).ToArray();
					addViewConfigurations.AddRange(from viewGuid in viewGuids
						from viewConfiguration in viewConfigurations
						where viewConfiguration.ViewId != viewGuid
						select new ViewConfiguration(new SavedQueryView(context, viewGuid, Metadata.LanguageCode), modalGridPageSize)
						{
							Search = new ViewSearch(!Metadata.LookupDisableQuickFind) { PlaceholderText = modalGridSearchPlaceholderText, TooltipText = modalGridSearchTooltipText },
							EnableEntityPermissions = Metadata.FormView.EnableEntityPermissions,
							PortalName = Metadata.FormView.ContextName,
							LanguageCode = Metadata.LanguageCode,
							ModalLookupAttributeLogicalName = Metadata.DataFieldName,
							ModalLookupEntityLogicalName = Metadata.TargetEntityName,
							ModalLookupFormReferenceEntityId = formEntityReferenceInfo.Item2,
							ModalLookupFormReferenceEntityLogicalName = formEntityReferenceInfo.Item1,
							ModalLookupFormReferenceRelationshipName = formEntityReferenceInfo.Item3,
							ModalLookupFormReferenceRelationshipRole = formEntityReferenceInfo.Item4
						});
				}
				viewConfigurations.AddRange(addViewConfigurations);
			}
			
			var applyRelatedRecordFilter = !string.IsNullOrWhiteSpace(Metadata.LookupFilterRelationshipName);
			string filterFieldName = null;
			if (!string.IsNullOrWhiteSpace(Metadata.LookupDependentAttributeName)) // entity.attribute (i.e. contact.adx_subject)
			{
				var pos = Metadata.LookupDependentAttributeName.IndexOf(".", StringComparison.InvariantCulture);
				filterFieldName = pos >= 0
					? Metadata.LookupDependentAttributeName.Substring(pos + 1)
					: Metadata.LookupDependentAttributeName;
			}
			
			var modalTitle = html.SnippetLiteral("Portal/Lookup/Modal/Title");
			var modalPrimaryButtonText = html.SnippetLiteral("Portal/Lookup/Modal/PrimaryButtonText");
			var modalCancelButtonText = html.SnippetLiteral("Portal/Lookup/Modal/CancelButtonText");
			var modalDismissButtonSrText = html.SnippetLiteral("Portal/Lookup/Modal/DismissButtonSrText");
			var modalRemoveValueButtonText = html.SnippetLiteral("Portal/Lookup/Modal/RemoveValueButtonText");
			var modalNewValueButtonText = html.SnippetLiteral("Portal/Lookup/Modal/NewValueButtonText");
			var modalDefaultErrorMessage = html.SnippetLiteral("Portal/Lookup/Modal/DefaultErrorMessage");
			var modalGridLoadingMessage = html.SnippetLiteral("Portal/Lookup/Modal/Grid/LoadingMessage");
			var modalGridErrorMessage = html.SnippetLiteral("Portal/Lookup/Modal/Grid/ErrorMessage");
			var modalGridAccessDeniedMessage = html.SnippetLiteral("Portal/Lookup/Modal/Grid/AccessDeniedMessage");
			var modalGridEmptyMessage = html.SnippetLiteral("Portal/Lookup/Modal/Grid/EmptyMessage");
			var modalGridToggleFilterText = html.SnippetLiteral("Portal/Lookup/Modal/Grid/ToggleFilterText");
			
			return html.LookupModal(ControlID, viewConfigurations,
				BuildControllerActionUrl("GetLookupGridData", "EntityGrid", new { area = "Portal", __portalScopeId__ = portal == null ? Guid.Empty : portal.Website.Id }), user, applyRelatedRecordFilter,
				Metadata.LookupAllowFilterOff, Metadata.LookupFilterRelationshipName, Metadata.LookupDependentAttributeType,
				filterFieldName, null, null, modalTitle, modalPrimaryButtonText, modalCancelButtonText, modalDismissButtonSrText,
				modalRemoveValueButtonText, modalNewValueButtonText, null, null, modalGridLoadingMessage, modalGridErrorMessage, modalGridAccessDeniedMessage,
				modalGridEmptyMessage, modalGridToggleFilterText, modalDefaultErrorMessage, null, null, null, Metadata.FormView.ContextName,
				Metadata.LanguageCode, null, modalSize, Metadata.LookupReferenceEntityFormId, EvaluateCreatePrivilege(portal.ServiceContext),
				ControlID == "entitlementid" ? BuildControllerActionUrl("GetDefaultEntitlements", "Entitlements", new { area = "CaseManagement", __portalScopeId__ = portal == null ? Guid.Empty : portal.Website.Id }) : null);
		}

		/// <summary>
		/// Generates a URL to a controller action
		/// </summary>
		protected string BuildControllerActionUrl(string actionName, string controllerName, object routeValues)
		{
			var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper) ?? new RouteData();

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			return urlHelper.Action(actionName, controllerName, routeValues);
		}

		/// <summary>
		/// Evaluating whether user has create privilege or not
		/// <param name="serviceContext">serviceContext</param>
		/// <returns>True if user has create Privilege otherwise returns False</returns>
		/// </summary>
		protected bool EvaluateCreatePrivilege(OrganizationServiceContext serviceContext)
		{
			bool hasCreatePrivilege = false;
			if (Metadata.LookupReferenceEntityFormId != null)
			{
				var entityForm = serviceContext.RetrieveSingle(
					"adx_entityform",
					new[] { "adx_entityname", "adx_mode" },
					new[] {
						new Condition("adx_entityformid", ConditionOperator.Equal, Metadata.LookupReferenceEntityFormId),
						new Condition("statuscode", ConditionOperator.NotNull),
						new Condition("statuscode", ConditionOperator.Equal, (int)Enums.EntityFormStatusCode.Active)
					});

				if (entityForm != null)
				{
					var entityLogicalName = entityForm.GetAttributeValue<string>("adx_entityname");
					var mode = entityForm.GetAttributeValue<OptionSetValue>("adx_mode");

					if ((mode.Value == (int)WebFormStepMode.Insert) && (Metadata.LookupTargets.Contains(entityLogicalName))) // Insert
					{
						var crmEntityPermissionProvider = new CrmEntityPermissionProvider();
						hasCreatePrivilege = crmEntityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Create, entityLogicalName);
						return hasCreatePrivilege;
					}
					else { return hasCreatePrivilege; }
				}
				else { return hasCreatePrivilege; }
			}
			else { return hasCreatePrivilege; }
		}

		private static Tuple<string, Guid?, string, string> GetFormEntityReferenceInfo(HttpRequest request)
		{
			var entityLogicalName = request["refentity"];

			Guid parsedEntityId;
			var entityId = Guid.TryParse(request["refid"], out parsedEntityId)
				? new Guid?(parsedEntityId)
				: null;

			var relationshipName = request["refrel"];
			var relationshipRole = request["refrelrole"];

			return new Tuple<string, Guid?, string, string>(entityLogicalName, entityId, relationshipName, relationshipRole);
		}
	}
}
