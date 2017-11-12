/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a Date &amp; Time field.
	/// </summary>
	public class DateTimeControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// Template used when rendering a Date &amp; Time field.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="metadata"></param>
		/// <param name="validationGroup"></param>
		/// <param name="bindings"></param>
		public DateTimeControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{ Field = field; }

		public override string CssClass
		{
			get { return "datetime form-control"; }
		}

		/// <summary>
		/// Form Field
		/// </summary>
		public CrmEntityFormViewField Field { get; private set; }

		protected bool IncludesTime
		{
			get { return string.Equals(Metadata.Format, "dateandtime", StringComparison.InvariantCultureIgnoreCase); }
		}

		protected override string[] ScriptIncludes
		{
			get
			{
				var list = new List<string>();
				var scripts = new[] { "~/xrm-adx/js/crmentityformview-datetime.js" };
				list.AddRange(base.ScriptIncludes);
				list.AddRange(scripts);
				return list.ToArray();
			}
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
			RegisterClientSideDependencies(container);

			var textbox = new TextBox
			{
				ID = ControlID,
				TextMode = TextBoxMode.SingleLine,
				CssClass = string.Join(" ", CssClass, Metadata.CssClass),
				ToolTip = Metadata.ToolTip
			};

			textbox.Attributes["data-ui"] = "datetimepicker";
			textbox.Attributes["data-type"] = IncludesTime ? "datetime" : "date";
			textbox.Attributes["data-attribute"] = Metadata.DataFieldName;
			textbox.Attributes["data-behavior"] = Metadata.DateTimeBehavior.Value;
			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				textbox.Attributes.Add("required", string.Empty);
			}

			if (Metadata.ReadOnly)
			{
				textbox.CssClass += " readonly";
				textbox.Attributes["readonly"] = "readonly";
			}

			container.Controls.Add(textbox);

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					DateTime parsed;

					return TryParse(textbox.Text, out parsed)
						? new DateTime?(parsed)
						: null;
				},
				Set = obj =>
				{
					var initialValue = obj as DateTime?;
					
					textbox.Text = initialValue.HasValue
                        ? initialValue.Value.ToString(Globalization.DateTimeFormatInfo.RoundTripPattern)
						: string.Empty;
				}
			};
		}

		protected override void InstantiateValidatorsIn(Control container)
		{
			var dateFormatValidator = new CustomValidator
			{
				ID = string.Format("DateFormatValidator{0}", ControlID),
				ControlToValidate = ControlID,
				ValidationGroup = ValidationGroup,
				ValidateEmptyText = false,
				ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.ValidationErrorMessage) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("date")) ? ResourceManager.GetString("Invalid_Date_Format_Error").FormatWith(Metadata.Label) : Metadata.Messages["date"].FormatWith(Metadata.Label) : Metadata.ValidationErrorMessage)),
				Text = "*",
			};

			dateFormatValidator.ServerValidate += OnDateFormatValidate;

			container.Controls.Add(dateFormatValidator);

			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				container.Controls.Add(new RequiredFieldValidator
				{
					ID = string.Format("RequiredFieldValidator{0}", ControlID),
					ControlToValidate = ControlID,
					ValidationGroup = ValidationGroup,
					Display = ValidatorDisplay,
					ErrorMessage = ValidationSummaryMarkup((string.IsNullOrWhiteSpace(Metadata.RequiredFieldValidationErrorMessage) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("required")) ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("required")) ? ResourceManager.GetString("Required_Field_Error").FormatWith(Metadata.Label) : Metadata.Messages["required"].FormatWith(Metadata.Label) : Metadata.Messages["required"].FormatWith(Metadata.Label) : Metadata.RequiredFieldValidationErrorMessage)),
					Text = ValidationText
				});
			}

			this.InstantiateCustomValidatorsIn(container);
		}

		private static void OnDateFormatValidate(object source, ServerValidateEventArgs args)
		{
			DateTime parsed;

			args.IsValid = TryParse(args.Value, out parsed);
		}

		private static bool TryParse(string value, out DateTime parsed)
		{
			// DateTimeStyles.AdjustToUniversal: Date and time are returned as a
			// Coordinated Universal Time (UTC). If the input string denotes a
			// local time, through a time zone specifier or AssumeLocal, the
			// date and time are converted from the local time to UTC. If the
			// input string denotes a UTC time, through a time zone specifier
			// or AssumeUniversal, no conversion occurs. If the input string
			// does not denote a local or UTC time, no conversion occurs and
			// the resulting Kind property is Unspecified.
			// http://msdn.microsoft.com/en-us/library/91hfhz89.aspx

			return DateTime.TryParseExact(value, Globalization.DateTimeFormatInfo.RoundTripPattern, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed);
		}
	}
}
