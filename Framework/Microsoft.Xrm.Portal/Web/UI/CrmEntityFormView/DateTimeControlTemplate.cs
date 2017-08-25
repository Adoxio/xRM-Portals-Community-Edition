/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public class DateTimeControlTemplate : CellTemplate // MSBug #120060: Won't seal, inheritance is used extension point.
	{
		public DateTimeControlTemplate(ICellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings) : base(metadata, validationGroup, bindings) { }

		public override string CssClass
		{
			get { return "datetime"; }
		}

		protected bool IncludesTime
		{
			get { return string.Equals(Metadata.Format, "dateandtime", StringComparison.InvariantCultureIgnoreCase); }
		}

		protected override void InstantiateControlIn(Control container)
		{
			RegisterClientSideDependencies(container);

			var textbox = new TextBox
			{
				ID = ControlID,
				TextMode = TextBoxMode.SingleLine,
				CssClass = "datetime",
				ToolTip = Metadata.ToolTip // MSBug #120060: Encode will be handled by TextBox control.
			};

			container.Controls.Add(textbox);

			var dateTextBoxID = "{0}_Date".FormatWith(ControlID);

			var dateTextBox = new TextBox
			{
				ID = dateTextBoxID,
				TextMode = TextBoxMode.SingleLine,
				CssClass = "date",
				ToolTip = Metadata.ToolTip // MSBug #120060: Encode will be handled by TextBox control.
			};

			dateTextBox.Attributes["style"] = "display:none;";

			container.Controls.Add(dateTextBox);

			var timeDropDown = GetTimeDropDown();

			if (IncludesTime)
			{
				timeDropDown.Attributes["style"] = "display:none;";

				container.Controls.Add(timeDropDown);
			}

			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () =>
				{
					DateTime value;

					return DateTime.TryParse(textbox.Text, out value) ? new DateTime?(value) : null;
				},
				Set = obj =>
				{
					var dateTime = obj as DateTime?;

					textbox.Text = dateTime.HasValue
						? dateTime.Value.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'")
						: string.Empty;
				}
			};
		}

		protected override void InstantiateValidatorsIn(Control container)
		{
			var dateFormatValidator = new CustomValidator
			{
				ControlToValidate = ControlID,
				ValidationGroup = ValidationGroup,
				ValidateEmptyText = false,
				ErrorMessage = "{0} must have a valid date format.".FormatWith(Metadata.Label),
				Text = "*",
			};

			dateFormatValidator.ServerValidate += OnDateFormatValidate;

			container.Controls.Add(dateFormatValidator);

			if (Metadata.IsRequired)
			{
				container.Controls.Add(new RequiredFieldValidator
				{
					ControlToValidate = ControlID,
					ValidationGroup = ValidationGroup,
					ErrorMessage = "{0} is a required field.".FormatWith(Metadata.Label),
					Text = "*",
					EnableClientScript = false,
				});
			}
		}

		private DropDownList GetTimeDropDown()
		{
			var timeDropDown = new DropDownList { ID = "{0}_Time".FormatWith(ControlID), CssClass = "time" };

			timeDropDown.Items.Clear();

			var time = DateTime.Today;
			var date = time.Date;

			while (time.Date == date)
			{
				timeDropDown.Items.Add(new ListItem
				{
					Value = "{0}:{1}".FormatWith(time.Hour, time.Minute),
					Text = time.ToShortTimeString()
				});

				time = time.AddMinutes(30);
			}

			return timeDropDown;
		}

		private static void OnDateFormatValidate(object source, ServerValidateEventArgs args)
		{
			DateTime value;

			args.IsValid = DateTime.TryParse(args.Value, out value);
		}

		private static readonly string[] ScriptIncludes = new[]
		{
			PortalContextElement.DefaultXrmFilesBaseUri + "/js/crmentityformview.js"
		};

		private static void RegisterClientSideDependencies(Control control)
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
