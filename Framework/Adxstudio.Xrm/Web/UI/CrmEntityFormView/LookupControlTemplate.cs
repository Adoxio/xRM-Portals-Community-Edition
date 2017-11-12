/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.ContentAccess;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Lookup field.
	/// </summary>
	public class LookupControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// LookupControlTemplate class initialization.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		/// <remarks>Localization of lookup display text is provided by retrieving the entity metadata and determine the primary name field and if the language code is not the base language for the organization it appends _ + language code to the primary name field and populates the control with the values from the localized attribute. i.e. if the primary name field is new_name and the language code is 1036 for French, the localized attribute name would be new_name_1036. An attribute would be added to the entity in this manner for each language to be supported and the attribute must be added to the view assigned to the lookup on the form.</remarks>
		public LookupControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
		}
		
		/// <summary>
		/// Form field.
		/// </summary>
		public CrmEntityFormViewField Field { get; private set; }

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
		
		protected override void InstantiateControlIn(Control container)
		{
			var dropDown = new DropDownList { ID = ControlID, CssClass = string.Join(" ", CssClass, Metadata.CssClass), ToolTip = Metadata.ToolTip };

			dropDown.Attributes.Add("onchange", "setIsDirty(this.id);");

			container.Controls.Add(dropDown);

			var lookupEntityName = Metadata.LookupTargets[0];

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				dropDown.Attributes.Add("required", string.Empty);
			}

			if (Metadata.ReadOnly || ((WebControls.CrmEntityFormView)container.BindingContainer).Mode == FormViewMode.ReadOnly)
			{
				AddSpecialBindingAndHiddenFieldsToPersistDisabledSelect(container, lookupEntityName, dropDown);
			}
			else
			{
				PopulateDropDownIfFirstLoad(dropDown, lookupEntityName);

				Bindings[Metadata.DataFieldName] = new CellBinding
				{
					Get = () =>
					{
						Guid id;
						return !Guid.TryParse(dropDown.SelectedValue, out id) ? null : new EntityReference(lookupEntityName, id);
					},
					Set = obj =>
					{
						var entityReference = (EntityReference)obj;
						dropDown.SelectedValue = entityReference.Id.ToString();
					}
				};
			}
		}

		protected override void InstantiateValidatorsIn(Control container)
		{
			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				container.Controls.Add(new RequiredFieldValidator
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

		private void AddSpecialBindingAndHiddenFieldsToPersistDisabledSelect(Control container, string lookupEntityName, DropDownList dropDown)
		{
			dropDown.CssClass = "{0} readonly".FormatWith(CssClass);
			dropDown.Attributes["disabled"] = "disabled";
			dropDown.Attributes["aria-disabled"] = "true";


			var hiddenValue = new HiddenField { ID = "{0}_Value".FormatWith(ControlID) };
			container.Controls.Add(hiddenValue);

			var hiddenSelectedIndex = new HiddenField { ID = "{0}_SelectedIndex".FormatWith(ControlID) };
			container.Controls.Add(hiddenSelectedIndex);

			RegisterClientSideDependencies(container);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					Guid id;
					return !Guid.TryParse(hiddenValue.Value, out id) ? null : new EntityReference(lookupEntityName, id);
				},
				Set = obj =>
				{
					var entityReference = (EntityReference)obj;
					dropDown.Items.Add(new ListItem
					{
						Value = entityReference.Id.ToString(),
						Text = entityReference.Name ?? string.Empty
					});
					dropDown.SelectedValue = "{0}".FormatWith(entityReference.Id);
					hiddenValue.Value = dropDown.SelectedValue;
					hiddenSelectedIndex.Value = dropDown.SelectedIndex.ToString(CultureInfo.InvariantCulture);
				}
			};
		}

		private static EntityMetadata GetEntityMetadata(OrganizationServiceContext context, string logicalName, IDictionary<string, EntityMetadata> metadataCache)
		{
			EntityMetadata cachedMetadata;

			if (metadataCache.TryGetValue(logicalName, out cachedMetadata))
			{
				return cachedMetadata;
			}

			var metadataReponse = context.Execute(new RetrieveEntityRequest { LogicalName = logicalName, EntityFilters = EntityFilters.Attributes }) as RetrieveEntityResponse;

			if (metadataReponse != null && metadataReponse.EntityMetadata != null)
			{
				metadataCache[logicalName] = metadataReponse.EntityMetadata;

				return metadataReponse.EntityMetadata;
			}

			throw new InvalidOperationException("Unable to retrieve the metadata for entity name {0}.".FormatWith(logicalName));
		}

		private void PopulateDropDownIfFirstLoad(DropDownList dropDown, string lookupEntityName)
		{
			if (dropDown.Items.Count > 0)
			{
				return;
			}
			
			var empty = new ListItem(string.Empty, string.Empty);
			empty.Attributes["label"] = " ";
			dropDown.Items.Add(empty);

			var context = CrmConfigurationManager.CreateContext();

			var service = CrmConfigurationManager.CreateService();

			var metadataCache = new Dictionary<string, EntityMetadata>();
			var entityMetadata = GetEntityMetadata(context, Metadata.LookupTargets[0], metadataCache);
			var primaryNameAttribute = entityMetadata.PrimaryNameAttribute;
			var primaryKeyAttribute = entityMetadata.PrimaryIdAttribute;
			var localizedPrimaryNameAttribute = primaryNameAttribute;
			
			// get a localized primary attribute name
			if (Metadata.LanguageCode > 0)
			{
				var defaultLanguageCode = RetrieveOrganizationBaseLanguageCode(context);
				if (Metadata.LanguageCode != defaultLanguageCode)
				{
					localizedPrimaryNameAttribute = string.Format("{0}_{1}", primaryNameAttribute, Metadata.LanguageCode);
					foreach (var att in entityMetadata.Attributes.Where(att => att.LogicalName.EndsWith(localizedPrimaryNameAttribute)))
					{
						primaryNameAttribute = att.LogicalName;
						break;
					}
				}
			}

			// By default a lookup field cell defined in the form XML does not contain view parameters unless the user has specified a view that is not the default for that entity and we must query to find the default view.  Saved Query entity's QueryType code 64 indicates a lookup view.

			var view = Metadata.LookupViewID == Guid.Empty ? context.CreateQuery("savedquery").FirstOrDefault(s => s.GetAttributeValue<string>("returnedtypecode") == lookupEntityName && s.GetAttributeValue<bool>("isdefault") && s.GetAttributeValue<int>("querytype") == 64) : context.CreateQuery("savedquery").FirstOrDefault(s => s.GetAttributeValue<Guid>("savedqueryid") == Metadata.LookupViewID);

			IQueryable<Entity> lookupEntities;

			if (view != null)
			{
				var fetchXml = view.GetAttributeValue<string>("fetchxml");

				lookupEntities = GetLookupRecords(fetchXml, context);

				if (lookupEntities == null) return;
			}
			else
			{
				string fetchXml = string.Format(@"
					<fetch mapping='logical'>
						<entity name='{0}'> 
							<attribute name='{1}'/>
							<attrbiute name='{2}'/>
						</entity> 
					</fetch> ", lookupEntityName, primaryKeyAttribute, primaryNameAttribute);

				lookupEntities = GetLookupRecords(fetchXml, context);

				if (lookupEntities == null) return;
			}
			
			foreach (var entity in lookupEntities)
			{
				dropDown.Items.Add(new ListItem
				{
					Value = entity.Id.ToString(),
					Text = entity.Attributes.ContainsKey(localizedPrimaryNameAttribute) ? entity.GetAttributeValue(localizedPrimaryNameAttribute).ToString() : entity.Attributes.ContainsKey(primaryNameAttribute) ? entity.GetAttributeValue(primaryNameAttribute).ToString() : string.Empty
				});
			}
		}

		private IQueryable<Entity> GetLookupRecords(string fetchXml, OrganizationServiceContext context)
		{
			var fetch = Fetch.Parse(fetchXml);

			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			crmEntityPermissionProvider.TryApplyRecordLevelFiltersToFetch(context, CrmEntityPermissionRight.Read, fetch);

			crmEntityPermissionProvider.TryApplyRecordLevelFiltersToFetch(context, CrmEntityPermissionRight.Append, fetch);

            // Apply Content Access Level filtering
            var contentAccessLevelProvider = new ContentAccessLevelProvider();
            contentAccessLevelProvider.TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight.Read, fetch);

            // Apply Product filtering
            var productAccessProvider = new ProductAccessProvider();
            productAccessProvider.TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight.Read, fetch);

            var response = (RetrieveMultipleResponse)context.Execute(fetch.ToRetrieveMultipleRequest());

			var data = response.EntityCollection;

			if (data == null || data.Entities == null) return null;

			return data.Entities.AsQueryable();
		}

		private static int RetrieveOrganizationBaseLanguageCode(OrganizationServiceContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var organizationEntityQuery = new QueryExpression("organization");

			organizationEntityQuery.ColumnSet.AddColumn("languagecode");

			var organizationEntities = context.RetrieveMultiple(organizationEntityQuery);

			if (organizationEntities == null)
			{
				throw new ApplicationException("Failed to retrieve organization entity collection.");
			}

			return (int)organizationEntities[0].Attributes["languagecode"];
		}
	}
}
