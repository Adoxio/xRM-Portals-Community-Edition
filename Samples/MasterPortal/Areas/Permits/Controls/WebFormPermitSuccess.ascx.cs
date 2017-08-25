/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Web.UI.WebForms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal.Configuration;

namespace Site.Areas.Permits.Controls
{
	public partial class WebFormPermitSuccess : WebFormUserControl
	{
		protected void Page_PreRender(object sender, EventArgs e)
		{
			WebForm.ShowHideNextButton(false);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (PreviousStepEntityID == Guid.Empty)
			{
				throw new NullReferenceException("The ID of the previous web form step's created entity is null.");
			}

			CustomSuccessMessage.Text = WebForm.SuccessMessage ?? string.Empty;

			DefaultSuccessMessageSnippet.Visible = string.IsNullOrWhiteSpace(WebForm.SuccessMessage);
			
			var context = PortalCrmConfigurationManager.CreateServiceContext();

			var entity = context.CreateQuery(CurrentStepEntityLogicalName).FirstOrDefault(o => o.GetAttributeValue<Guid>(PreviousStepEntityPrimaryKeyLogicalName) == PreviousStepEntityID);

			if (entity == null)
			{
				throw new NullReferenceException(string.Format("The {0} record with primary key {1} equal to {2} couldn't be found.", PreviousStepEntityLogicalName, PreviousStepEntityPrimaryKeyLogicalName, PreviousStepEntityID));
			}

			var type = context.CreateQuery("adx_permittype").FirstOrDefault(s => s.GetAttributeValue<string>("adx_entityname") == PreviousStepEntityLogicalName);

			if (type == null)
			{
				throw new NullReferenceException(string.Format("The {0} record couldn't be found with the entity name of {1}.", "adx_permittype", PreviousStepEntityLogicalName));
			}

			var field = type.GetAttributeValue<string>("adx_permitnumberfieldname");

			if (!string.IsNullOrWhiteSpace(field))
			{
				try
				{
					var permitNumber = entity.GetAttributeValue<string>(field);

					if (!string.IsNullOrWhiteSpace(permitNumber))
					{
						PermitNumber.Text = permitNumber;

						PermitNumberPanel.Visible = true;
					}
				}
				catch (Exception)
				{
					PermitNumberPanel.Visible = false;
				}
			}

			PanelSuccess.Visible = true;
		}
	}
}
