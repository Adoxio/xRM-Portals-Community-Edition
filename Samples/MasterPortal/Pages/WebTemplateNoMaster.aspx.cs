/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Sdk.Query;

namespace Site.Pages
{
	public partial class WebTemplateNoMaster : PortalPage
	{
		protected void Page_Init(object sender, EventArgs args)
		{
			if (!System.Web.SiteMap.Enabled)
			{
				return;
			}

			var currentNode = System.Web.SiteMap.CurrentNode;

			if (currentNode == null)
			{
				return;
			}

			var templateIdString = currentNode["adx_webtemplateid"];

			if (string.IsNullOrEmpty(templateIdString))
			{
				return;
			}

			Guid templateId;

			if (!Guid.TryParse(templateIdString, out templateId))
			{
				return;
			}

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_webtemplate")
				{
					Attributes = new[] { new FetchAttribute("adx_source"), new FetchAttribute("adx_mimetype") },
					Filters = new[]
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("adx_webtemplateid", ConditionOperator.Equal, templateId),
								new Condition("statecode", ConditionOperator.Equal, 0)
							}
						}
					}
				}
			};

			var webTemplate = PortalOrganizationService.RetrieveSingle(fetch);
			if (webTemplate == null)
			{
				return;
			}

			var source = webTemplate.GetAttributeValue<string>("adx_source");
			using (var output = new System.IO.StringWriter())
			{
				Html.RenderLiquid(source, string.Format("{0}:{1}", webTemplate.LogicalName, webTemplate.Id), output);
				Liquid.Html = output.ToString();
			}

			var mimetype = webTemplate.GetAttributeValue<string>("adx_mimetype");

			if (!string.IsNullOrWhiteSpace(mimetype))
			{
				ContentType = mimetype;
			}
		}
	}
}
