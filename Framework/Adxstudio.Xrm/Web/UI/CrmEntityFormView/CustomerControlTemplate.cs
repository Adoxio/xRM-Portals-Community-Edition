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
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Customer field.
	/// </summary>
	public class CustomerControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// CustomerControlTemplate class initialization.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public CustomerControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
		}

		public override string CssClass
		{
			get { return "lookup form-control"; }
		}
		
		/// <summary>
		/// Form field.
		/// </summary>
		public CrmEntityFormViewField Field { get; private set; }

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

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				dropDown.Attributes.Add("required", string.Empty);
			}

			var context = CrmConfigurationManager.CreateContext();

			if (Metadata.ReadOnly || ((WebControls.CrmEntityFormView)container.BindingContainer).Mode == FormViewMode.ReadOnly)
			{
				AddSpecialBindingAndHiddenFieldsToPersistDisabledSelect(container, context, dropDown);
			}
			else
			{
				PopulateDropDownIfFirstLoad(context, dropDown);

				Bindings[Metadata.DataFieldName] = new CellBinding
				{
					Get = () =>
					{
						Guid id;

						if (Guid.TryParse(dropDown.SelectedValue, out id))
						{
							foreach (var lookupTarget in Metadata.LookupTargets)
							{
								var lookupTargetId = GetEntityMetadata(context, lookupTarget).PrimaryIdAttribute;

								var foundEntity = context.CreateQuery(lookupTarget).FirstOrDefault(e => e.GetAttributeValue<Guid?>(lookupTargetId) == id);

								if (foundEntity != null)
								{
									return new EntityReference(lookupTarget, id);
								}
							}
						}

						return null;
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

		private void AddSpecialBindingAndHiddenFieldsToPersistDisabledSelect(Control container, OrganizationServiceContext context, DropDownList dropDown)
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

					if (Guid.TryParse(hiddenValue.Value, out id))
					{
						foreach (var lookupTarget in Metadata.LookupTargets)
						{
							var lookupTargetId = GetEntityMetadata(context, lookupTarget).PrimaryIdAttribute;

							var foundEntity = context.CreateQuery(lookupTarget).FirstOrDefault(e => e.GetAttributeValue<Guid?>(lookupTargetId) == id);

							if (foundEntity != null)
							{
								return new EntityReference(lookupTarget, id);
							}
						}
					}

					return null;
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

		private static string GetEntityPrimaryNameAttribute(OrganizationServiceContext context, Entity entity)
		{
			var entityMetadata = GetEntityMetadata(context, entity.LogicalName);

			var primaryAttributeName = entityMetadata.PrimaryNameAttribute;

			return entity.GetAttributeValue<string>(primaryAttributeName);
		}

		private static EntityMetadata GetEntityMetadata(OrganizationServiceContext context, string logicalName)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (string.IsNullOrEmpty(logicalName))
			{
				throw new ArgumentNullException("logicalName");
			}

			var metadataReponse = context.Execute(new RetrieveEntityRequest { LogicalName = logicalName, EntityFilters = EntityFilters.Entity }) as RetrieveEntityResponse;

			if (metadataReponse != null && metadataReponse.EntityMetadata != null)
			{
				return metadataReponse.EntityMetadata;
			}

			throw new InvalidOperationException("Unable to retrieve the metadata for entity name {0}.".FormatWith(logicalName));
		}

		private void PopulateDropDownIfFirstLoad(OrganizationServiceContext context, ListControl dropDown)
		{
			if (dropDown.Items.Count > 0)
			{
				return;
			}
			
			var empty = new ListItem(string.Empty, string.Empty);
			empty.Attributes["label"] = " ";
			dropDown.Items.Add(empty);

			var lookupEntities = new List<Entity>();

			foreach (var entities in Metadata.LookupTargets.Select(context.CreateQuery))
			{
				lookupEntities.AddRange(entities);
			}

			foreach (var entity in lookupEntities)
			{
				var listitem = new ListItem
				{
					Value = entity.Id.ToString(),
					Text = GetEntityPrimaryNameAttribute(context, entity)
				};

				// We may have to check if the entity is the same as the CrmEntityFormView
				// Adding a lookup back to oneself may cause an error.
				
				dropDown.Items.Add(listitem);
			}
		}
	}
}
