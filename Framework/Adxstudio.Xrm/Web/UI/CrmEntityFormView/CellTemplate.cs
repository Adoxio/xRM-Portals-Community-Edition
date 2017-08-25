/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used to create a cell of a form.
	/// </summary>
	public abstract class CellTemplate : ICellTemplate
	{
		protected CellTemplate(FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}

			if (bindings == null)
			{
				throw new ArgumentNullException("bindings");
			}

			Bindings = bindings;
			Metadata = metadata;
			ValidationGroup = validationGroup;
		}

		/// <summary>
		/// Number of columns the cell is to take up.
		/// </summary>
		public virtual int? ColumnSpan
		{
			get { return Metadata.ColumnSpan; }
		}

		/// <summary>
		/// CSS Class name assigned.
		/// </summary>
		public abstract string CssClass { get; }

		/// <summary>
		/// Indicates if the control in the cell is enabled or disabled.
		/// </summary>
		public virtual bool Enabled
		{
			get { return !Metadata.Disabled; }
		}

		protected IDictionary<string, CellBinding> Bindings { get; private set; }

		/// <summary>
		/// The id of the field. Also known as the logical name of the attribute.
		/// </summary>
		public virtual string ControlID
		{
			get { return Metadata.DataFieldName; }
		}

		protected FormXmlCellMetadata Metadata { get; set; }

		/// <summary>
		/// Number of rows a cell should take up
		/// </summary>
		public virtual int? RowSpan
		{
			get { return Metadata.RowSpan; }
		}

		protected virtual string[] ScriptIncludes
		{
			get { return new[] { "~/xrm-adx/js/crmentityformview.js" }; }
		}

		/// <summary>
		/// Tooltip text.
		/// </summary>
		public string ToolTip { get; set; }
		
		/// <summary>
		/// String of HTML markup for a control item in the Validation Summary.
		/// </summary>
		public virtual string ValidationSummaryMarkup(string message)
		{
			if (Metadata.EnableValidationSummaryLinks)
			{
				var anchorId = Metadata.ShowLabel ? string.Format("{0}_label", ControlID) : ControlID;
				var anchorLinkMarkupString =
					"<a href='#{0}' onclick='javascript:scrollToAndFocus(\"{3}\",\"{4}\");return false;'>{1} {2}</a>".FormatWith(
						anchorId, message, Metadata.ValidationSummaryLinkText,
						Metadata.ShowLabel ? string.Format("{0}_label", ControlID) : ControlID, ControlID);
				return anchorLinkMarkupString;
			}
			return message;
		}

		protected string ValidationGroup { get; private set; }

		protected virtual bool LabelIsAssociated
		{
			get { return !Metadata.LabelNotAssociated; }
		}

		public virtual void InstantiateIn(Control container)
		{
			if (!Enabled)
			{
				return;
			}

			var descriptionContainer = new HtmlGenericControl("div");

			if (Metadata.AddDescription && !string.IsNullOrWhiteSpace(Metadata.Description))
			{
				descriptionContainer.InnerHtml = Metadata.Description;

				switch (Metadata.DescriptionPosition)
				{
					case WebFormMetadata.DescriptionPosition.AboveLabel:
						descriptionContainer.Attributes["class"] = !string.IsNullOrWhiteSpace(Metadata.CssClass) ? string.Join(" ", "description", Metadata.CssClass) : "description";
						break;
					case WebFormMetadata.DescriptionPosition.AboveControl:
						descriptionContainer.Attributes["class"] = !string.IsNullOrWhiteSpace(Metadata.CssClass) ? string.Join(" ", "description above", Metadata.CssClass) : "description above";
						break;
					case WebFormMetadata.DescriptionPosition.BelowControl:
						descriptionContainer.Attributes["class"] = !string.IsNullOrWhiteSpace(Metadata.CssClass) ? string.Join(" ", "description below", Metadata.CssClass) : "description below";
						break;
				}
			}

			if (Metadata.AddDescription && !string.IsNullOrWhiteSpace(Metadata.Description) && Metadata.DescriptionPosition == WebFormMetadata.DescriptionPosition.AboveLabel)
			{
				container.Controls.Add(descriptionContainer);
			}

			var cellInfoContainer = new HtmlGenericControl("div");

			container.Controls.Add(cellInfoContainer);

			var cellInfoClasses = new List<string> { "info" };

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired ||
				Metadata.IsFullNameControl && !Metadata.ReadOnly)
			{
				cellInfoClasses.Add("required");
			}

			cellInfoContainer.Attributes["class"] = string.Join(" ", cellInfoClasses.ToArray());

			if (Metadata.ShowLabel)
			{
				//ClientIDMode.Static required for bookmark anchor links in Validation Summary to work.
				var label = new Label
				{
					ClientIDMode = ClientIDMode.Static,
					ID = string.Format("{0}_label", ControlID),
					Text = Metadata.Label,
					ToolTip = Metadata.ToolTip

				};
                if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
                {
                    label.Text = HttpUtility.HtmlEncode(label.Text);
                }

                    if (LabelIsAssociated)
				{
					label.AssociatedControlID = ControlID;
				}

				cellInfoContainer.Controls.Add(label);
			}

			if (!Metadata.ReadOnly)
			{
				var validatorContainer = new HtmlGenericControl("div");

				cellInfoContainer.Controls.Add(validatorContainer);

				validatorContainer.Attributes["class"] = "validators";

				InstantiateValidatorsIn(validatorContainer);
			}

			if (Metadata.AddDescription && !string.IsNullOrWhiteSpace(Metadata.Description) && Metadata.DescriptionPosition == WebFormMetadata.DescriptionPosition.AboveControl)
			{
				container.Controls.Add(descriptionContainer);
			}

			var controlContainer = new HtmlGenericControl("div");

			container.Controls.Add(controlContainer);

			controlContainer.Attributes["class"] = "control";

			InstantiateControlIn(controlContainer);

			if (Metadata.AddDescription && !string.IsNullOrWhiteSpace(Metadata.Description) && Metadata.DescriptionPosition == WebFormMetadata.DescriptionPosition.BelowControl)
			{
				container.Controls.Add(descriptionContainer);
			}

			RegisterClientSideDependencies(controlContainer);
		}

		protected abstract void InstantiateControlIn(Control container);

		protected virtual void InstantiateValidatorsIn(Control container) { }

		protected virtual void RegisterClientSideDependencies(Control control)
		{
			foreach (var script in ScriptIncludes)
			{
				var scriptManager = ScriptManager.GetCurrent(control.Page);

				if (scriptManager == null)
				{
					continue;
				}

				var absolutePath = VirtualPathUtility.ToAbsolute(script);

				scriptManager.Scripts.Add(new ScriptReference(absolutePath));
			}
		}
	}
}
