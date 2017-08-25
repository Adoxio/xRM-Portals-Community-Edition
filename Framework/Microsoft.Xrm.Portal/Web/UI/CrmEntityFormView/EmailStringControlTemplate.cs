/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xrm.Client;

namespace Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView
{
	public class EmailStringControlTemplate : StringControlTemplate // MSBug #120061: Won't seal, inheritance is expected extension point.
	{
		public EmailStringControlTemplate(ICellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings) : base(metadata, validationGroup, bindings) { }

		protected override void InstantiateValidatorsIn(Control container)
		{
			base.InstantiateValidatorsIn(container);

			var emailFormatValidator = new CustomValidator
			{
				ControlToValidate = ControlID,
				ValidationGroup = ValidationGroup,
				ErrorMessage = "{0} must be a valid e-mail address.".FormatWith(Metadata.Label),
				Text = "*",
				ValidateEmptyText = false,
			};

			emailFormatValidator.ServerValidate += ValidateEmailFormat;

			container.Controls.Add(emailFormatValidator);
		}

		[SuppressMessage("Microsoft.Security", "CA2109", Justification = "Code reviewed.  Not necessary for ASP.NET 2.0 and above.")]
		protected virtual void ValidateEmailFormat(object source, ServerValidateEventArgs args)
		{
			try
			{
				var address = new MailAddress(args.Value);

				args.IsValid = true;
			}
			catch (FormatException)
			{
				args.IsValid = false;
			}
		}
	}
}
