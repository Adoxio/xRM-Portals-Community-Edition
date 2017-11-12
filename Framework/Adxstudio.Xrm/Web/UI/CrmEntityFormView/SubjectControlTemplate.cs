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
using System.Xml.Linq;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Subject Lookup field.
	/// </summary>
	public class SubjectControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// SubjecControlTemplate class initialization.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public SubjectControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
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
			get { return "lookup"; }
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
			var dropDown = new DropDownList { ID = ControlID, CssClass = string.Join(" ", "form-control", CssClass, Metadata.CssClass), ToolTip = Metadata.ToolTip };

			dropDown.Attributes.Add("onchange", "setIsDirty(this.id);");

			container.Controls.Add(dropDown);

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				dropDown.Attributes.Add("required", string.Empty);
			}

			if (Metadata.ReadOnly || ((WebControls.CrmEntityFormView)container.BindingContainer).Mode == FormViewMode.ReadOnly)
			{
				AddSpecialBindingAndHiddenFieldsToPersistDisabledSelect(container, dropDown);
			}
			else
			{
				PopulateDropDown(dropDown);

				Bindings[Metadata.DataFieldName] = new CellBinding
				{
					Get = () =>
					{
						Guid id;
						return !Guid.TryParse(dropDown.SelectedValue, out id) ? null : new EntityReference("subject", id);
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

		private void AddSpecialBindingAndHiddenFieldsToPersistDisabledSelect(Control container, DropDownList dropDown)
		{
			dropDown.CssClass = string.Join(" ", dropDown.CssClass, "readonly");
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
					return !Guid.TryParse(hiddenValue.Value, out id) ? null : new EntityReference("subject", id);
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

		private void PopulateDropDown(DropDownList dropDown)
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

			// By default a lookup field cell defined in the form XML does not contain view parameters unless the user has specified a view that is not the default for that entity and we must query to find the default view.  Saved Query entity's QueryType code 64 indicates a lookup view.

			var view = Metadata.LookupViewID == Guid.Empty ? context.CreateQuery("savedquery").FirstOrDefault(s => s.GetAttributeValue<string>("returnedtypecode") == "subject" && s.GetAttributeValue<bool?>("isdefault").GetValueOrDefault(false) && s.GetAttributeValue<int>("querytype") == 64) : context.CreateQuery("savedquery").FirstOrDefault(s => s.GetAttributeValue<Guid>("savedqueryid") == Metadata.LookupViewID);

			List<Entity> subjects;

			if (view != null)
			{
				var fetchXML = view.GetAttributeValue<string>("fetchxml");

				var xElement = XElement.Parse(fetchXML);

				var parentsubjectElement = xElement.Descendants("attribute").FirstOrDefault(e =>
																					{
																						var xAttribute = e.Attribute("name");
																						return xAttribute != null && xAttribute.Value == "parentsubject";
																					});

				if (parentsubjectElement == null)
				{
					//If fetchxml does not contain the parentsubject attribute then it must be injected so the results can be organized in a hierarchical order.

					var entityElement = xElement.Element("entity");

					if (entityElement == null)
					{
						return;
					}

					var p = new XElement("attribute", new XAttribute("name", "parentsubject"));

					entityElement.Add(p);

					fetchXML = xElement.ToString();
				}
				
				var data = service.RetrieveMultiple(new FetchExpression(fetchXML));

				if (data == null || data.Entities == null)
				{
					return;
				}

				subjects = data.Entities.ToList();
			}
			else
			{
				subjects = context.CreateQuery("subject").ToList();
			}

			var parents = subjects.Where(s => s.GetAttributeValue<EntityReference>("parentsubject") == null).OrderBy(s => s.GetAttributeValue<string>("title"));

			foreach (var parent in parents)
			{
				if (parent == null)
				{
					continue;
				}

				dropDown.Items.Add(new ListItem(parent.GetAttributeValue<string>("title"), parent.Id.ToString()));

				var parentId = parent.Id;

				var children = subjects.Where(s => s.GetAttributeValue<EntityReference>("parentsubject") != null && s.GetAttributeValue<EntityReference>("parentsubject").Id == parentId).OrderBy(s => s.GetAttributeValue<string>("title"));

				AddChildItems(dropDown, subjects, children, 1);
			}
		}

		protected void AddChildItems(DropDownList dropDown, List<Entity> subjects, IEnumerable<Entity> children, int depth)
		{
			foreach (var child in children)
			{
				if (child == null)
				{
					continue;
				}

				var padding = HttpUtility.HtmlDecode(string.Concat(Enumerable.Repeat("&nbsp;-&nbsp;", depth)));

				dropDown.Items.Add(new ListItem(string.Format("{0}{1}", padding, child.GetAttributeValue<string>("title")), child.Id.ToString()));

				var childId = child.Id;

				var grandchildren = subjects.Where(s => s.GetAttributeValue<EntityReference>("parentsubject") != null && s.GetAttributeValue<EntityReference>("parentsubject").Id == childId).OrderBy(s => s.GetAttributeValue<string>("title"));

				depth++;

				AddChildItems(dropDown, subjects, grandchildren, depth);

				depth--;
			}
		}
	}
}
