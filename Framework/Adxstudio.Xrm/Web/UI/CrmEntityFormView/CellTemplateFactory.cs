/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Factory pattern class to create a cell template.
	/// </summary>
	public class CellTemplateFactory : Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.CellTemplateFactory, ICellTemplateFactory
	{
		/// <summary>
		/// CellTemplateFactory Initialization.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="fields"></param>
		/// <param name="metadataFactory"></param>
		/// <param name="cellBindings"></param>
		/// <param name="languageCode"></param>
		/// <param name="validationGroup"></param>
		/// <param name="enableUnsupportedFields"></param>
		public void Initialize(Control control, Collection<CrmEntityFormViewField> fields, ICellMetadataFactory metadataFactory,
			IDictionary<string, CellBinding> cellBindings, int languageCode, string validationGroup, bool enableUnsupportedFields)
		{
			Fields = fields;
			FormView = control as WebControls.CrmEntityFormView;
			Initialize(control, metadataFactory, cellBindings, languageCode, validationGroup, enableUnsupportedFields);
		}

		/// <summary>
		/// CellTemplateFactory Initialization.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="fields"></param>
		/// <param name="metadataFactory"></param>
		/// <param name="cellBindings"></param>
		/// <param name="languageCode"></param>
		/// <param name="validationGroup"></param>
		/// <param name="enableUnsupportedFields"></param>
		/// <param name="toolTipEnabled"></param>
		/// <param name="recommendedFieldsRequired"></param>
		/// <param name="validationText"></param>
		/// <param name="contextName"></param>
		/// <param name="renderWebResourcesInline"></param>
		/// <param name="webFormMetadata"></param>
		/// <param name="forceAllFieldsRequired"></param>
		/// <param name="enableValidationSummaryLinks"></param>
		/// <param name="messages"> </param>
		public void Initialize(Control control, Collection<CrmEntityFormViewField> fields, ICellMetadataFactory metadataFactory,
			IDictionary<string, CellBinding> cellBindings, int languageCode, string validationGroup, bool enableUnsupportedFields,
			bool? toolTipEnabled, bool? recommendedFieldsRequired, string validationText, string contextName, bool? renderWebResourcesInline, IEnumerable<Entity> webFormMetadata, bool? forceAllFieldsRequired, bool? enableValidationSummaryLinks, Dictionary<string, string> messages, bool? showOwnerFields, int baseOrganizationLanguageCode = 0)
		{
			Messages = messages;
			Fields = fields;
			ToolTipEnabled = toolTipEnabled;
			ValidationText = validationText;
			RecommendedFieldsRequired = recommendedFieldsRequired;
			RenderWebResourcesInline = renderWebResourcesInline;
			ContextName = contextName;
			WebFormMetadata = webFormMetadata;
			ForceAllFieldsRequired = forceAllFieldsRequired;
			ShowOwnerFields = showOwnerFields;
			EnableValidationSummaryLinks = enableValidationSummaryLinks;
			FormView = control as WebControls.CrmEntityFormView;
			BaseOrganizationLanguageCode = baseOrganizationLanguageCode;
			Initialize(control, metadataFactory, cellBindings, languageCode, validationGroup, enableUnsupportedFields);
		}

		protected Dictionary<string, string> Messages { get; private set; }

		protected Collection<CrmEntityFormViewField> Fields { get; private set; }

		protected bool? ToolTipEnabled { get; set; }

		protected bool? RecommendedFieldsRequired { get; set; }

		protected bool? RenderWebResourcesInline { get; set; }

		protected bool? ForceAllFieldsRequired { get; set; }

		protected bool? ShowOwnerFields { get; set; }

		protected bool? EnableValidationSummaryLinks { get; set; }

		protected string ValidationSummaryLinkText { get; set; }

		protected string ValidationText { get; set; }

		protected string ContextName { get; set; }

		protected IEnumerable<Entity> WebFormMetadata { get; private set; }

		public WebControls.CrmEntityFormView FormView { get; private set; }

		public int BaseOrganizationLanguageCode { get; set; }

		/// <summary>
		/// Method to create the cell template.
		/// </summary>
		/// <param name="cellNode"></param>
		/// <param name="entityMetadata"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ApplicationException"></exception>
		public override ICellTemplate CreateTemplate(System.Xml.Linq.XNode cellNode, Microsoft.Xrm.Sdk.Metadata.EntityMetadata entityMetadata)
		{
			if (!IsInitialized)
				throw new InvalidOperationException("Factory is not initialized.");

			ICellMetadata cellMetadata;

			if (MetadataFactory is FormXmlCellMetadataFactory)
			{
				var formMetadataFactory = MetadataFactory as FormXmlCellMetadataFactory;

				cellMetadata = formMetadataFactory.GetMetadata(cellNode, entityMetadata, LanguageCode, ToolTipEnabled, RecommendedFieldsRequired, ValidationText, WebFormMetadata, ForceAllFieldsRequired, EnableValidationSummaryLinks, ValidationSummaryLinkText, Messages, BaseOrganizationLanguageCode);
			}
			else
			{
				cellMetadata = MetadataFactory.GetMetadata(cellNode, entityMetadata, LanguageCode);
			}

			return CreateCellTemplate(cellMetadata, entityMetadata);
		}

		/// <summary>
		/// Method to create cell template
		/// </summary>
		/// <param name="cellMetadata"></param>
		/// <param name="entityMetadata"></param>
		/// <returns></returns>
		/// <exception cref="ApplicationException"></exception>
		public virtual ICellTemplate CreateCellTemplate(ICellMetadata cellMetadata, Microsoft.Xrm.Sdk.Metadata.EntityMetadata entityMetadata)
		{
			var fields = Fields.Where(f => f.AttributeName == cellMetadata.DataFieldName).ToList();

			if (fields.Count() > 1)
			{
				throw new ApplicationException("Only one CrmEntityFormViewField with an AttributeName {0} can be specified.".FormatWith(cellMetadata.DataFieldName));
			}

			var field = fields.FirstOrDefault();

			var formXmlCellMetadata = cellMetadata as FormXmlCellMetadata;

			if (formXmlCellMetadata != null)
			{
				formXmlCellMetadata.FormView = FormView;

				if (formXmlCellMetadata.FormView != null && formXmlCellMetadata.FormView.Mode.HasValue)
				{
					if (formXmlCellMetadata.FormView.Mode.Value == FormViewMode.Insert && !formXmlCellMetadata.IsValidForCreate)
					{
						formXmlCellMetadata.ReadOnly = true;
					}

					if (formXmlCellMetadata.FormView.Mode.Value == FormViewMode.Edit && !formXmlCellMetadata.IsValidForUpdate)
					{
						formXmlCellMetadata.ReadOnly = true;
					}
				}

				if ((formXmlCellMetadata.IsNotesControl || formXmlCellMetadata.IsActivityTimelineControl) && (FormView.Mode == FormViewMode.Edit || FormView.Mode == FormViewMode.ReadOnly))
				{
					return new NotesControlTemplate(formXmlCellMetadata, ContextName, CellBindings, formXmlCellMetadata.IsActivityTimelineControl);
				}

				if (formXmlCellMetadata.IsWebResource)
				{
					if (formXmlCellMetadata.WebResourceIsHtml) return new HtmlWebResourceControlTemplate(formXmlCellMetadata, ContextName, RenderWebResourcesInline);

					if (formXmlCellMetadata.WebResourceIsImage) return new ImageWebResourceControlTemplate(formXmlCellMetadata);
				}

				if (formXmlCellMetadata.IsSharePointDocuments)
				{
					return new SharePointDocumentsControlTemplate(formXmlCellMetadata, ContextName, CellBindings);
				}

				if (formXmlCellMetadata.IsSubgrid && (FormView.Mode == FormViewMode.Edit || FormView.Mode == FormViewMode.ReadOnly))
				{
					return new SubgridControlTemplate(formXmlCellMetadata, ContextName, CellBindings);
				}

				if (formXmlCellMetadata.IsQuickForm)
				{
					return new CrmQuickFormControlTemplate(formXmlCellMetadata, ContextName, CellBindings);
				}

				if (formXmlCellMetadata.HasAttributeType("lookup"))
				{
					if (formXmlCellMetadata.LookupTargets.Length >= 1 && formXmlCellMetadata.LookupTargets[0] == "subject")
					{
						return new SubjectControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
					}

					if ((field != null && field.Type == FieldType.Dropdown) || formXmlCellMetadata.ControlStyle == WebForms.WebFormMetadata.ControlStyle.LookupDropdown)
					{
						return new LookupControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
					}

					return new ModalLookupControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
				}

				if (formXmlCellMetadata.HasAttributeType("customer"))
				{
					if ((field != null && field.Type == FieldType.Dropdown) || formXmlCellMetadata.ControlStyle == WebForms.WebFormMetadata.ControlStyle.LookupDropdown)
					{
						return new CustomerControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
					}

					return new ModalLookupControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
				}

				if (formXmlCellMetadata.IsFullNameControl) return new FullNameControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings, entityMetadata, FormView.Mode);

				if (formXmlCellMetadata.IsAddressCompositeControl)
				{
					return new AddressCompositeControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings, entityMetadata, FormView.Mode);
				}

				if (ShowOwnerFields.GetValueOrDefault(false) && formXmlCellMetadata.HasAttributeType("owner"))
				{
					if ((field != null && field.Type == FieldType.Dropdown) || formXmlCellMetadata.ControlStyle == WebForms.WebFormMetadata.ControlStyle.LookupDropdown)
					{
						return new LookupControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
					}

					return new ModalLookupControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
				}

				if (cellMetadata.HasAttributeType("string"))
				{
					switch (cellMetadata.Format)
					{
						case "Email":
							return new EmailStringControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
						case "Url":
							return new UrlStringControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
						case "TickerSymbol":
							return new TickerSymbolStringControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
						case "TextArea":
							return new MemoControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
						default:
							return new StringControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
					}
				}

				if (cellMetadata.HasAttributeType("picklist"))
				{
					// determine if the picklist should be a multi-select picklist

					var picklistvaluesfield = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == string.Format("{0}selectedvalues", cellMetadata.DataFieldName));

					if (picklistvaluesfield != null)
					{
						return new MultiSelectPicklistControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
					}

					return new PicklistControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
				}

				if (cellMetadata.HasAttributeType("boolean"))
				{
					return formXmlCellMetadata.ControlStyle == WebForms.WebFormMetadata.ControlStyle.MultipleChoice ? new MultipleChoiceControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings) : new BooleanControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
				}

				if (cellMetadata.HasAttributeType("memo")) return new MemoControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("decimal")) return new DecimalControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("datetime")) return new DateTimeControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("double")) return new DoubleControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("integer"))
				{
					switch (cellMetadata.Format)
					{
						case "Duration":
							return new DurationControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
						case "Language":
							return new LanguageControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
						case "TimeZone":
							return new TimeZoneControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
						default:
							if (formXmlCellMetadata.ControlStyle == WebForms.WebFormMetadata.ControlStyle.RankOrderAllowTies || formXmlCellMetadata.ControlStyle == WebForms.WebFormMetadata.ControlStyle.RankOrderNoTies)
							{
								return new RankOrderControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
							}
							if (formXmlCellMetadata.ControlStyle == WebForms.WebFormMetadata.ControlStyle.ConstantSum)
							{
								return new ConstantSumControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
							}
							if (formXmlCellMetadata.ControlStyle == WebForms.WebFormMetadata.ControlStyle.StackRank)
							{
								return new StackRankControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
							}
							return new IntegerControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);
					}
				}

				if (cellMetadata.HasAttributeType("state")) return new StateControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("status")) return new StatusReasonControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("money")) return new MoneyControlTemplate(field, formXmlCellMetadata, ValidationGroup, CellBindings, ContextName);
			}
			else
			{
				if (cellMetadata.HasAttributeType("string"))
					return string.Equals("email", cellMetadata.Format, StringComparison.InvariantCultureIgnoreCase)
						? new Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.EmailStringControlTemplate(cellMetadata, ValidationGroup, CellBindings)
						: new Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.StringControlTemplate(cellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("picklist"))
					return new Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.PicklistControlTemplate(cellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("boolean"))
					return new Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.BooleanControlTemplate(cellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("memo"))
					return new Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.MemoControlTemplate(cellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("datetime"))
					return new Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.DateTimeControlTemplate(cellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("integer"))
					return new Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.IntegerControlTemplate(cellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("money"))
					return new Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.MoneyControlTemplate(cellMetadata, ValidationGroup, CellBindings);

				if (cellMetadata.HasAttributeType("datetime"))
					return new Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.DateTimeControlTemplate(cellMetadata, ValidationGroup, CellBindings);
			}

			if (!string.IsNullOrEmpty(cellMetadata.AttributeType) && EnableUnsupportedFields)
				return new UnsupportedControlTemplate(cellMetadata, ValidationGroup, CellBindings, EnableUnsupportedFields);

			return new EmptyCellTemplate(cellMetadata);
		}
	}
}
