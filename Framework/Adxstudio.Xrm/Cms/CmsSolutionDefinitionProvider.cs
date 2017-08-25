/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Cms
{
	public class CmsSolutionDefinitionProvider : SolutionDefinitionProvider
	{
		private readonly CrmWebsite _website;

		public CmsSolutionDefinitionProvider(PortalSolutions portalSolutions, CrmWebsite website)
			: base(portalSolutions)
		{
			_website = website;
		}

		public override IDictionary<string, object> GetQueryParameters()
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("website.Id={0}", _website.Id));

			var websiteId = _website.Id;

			var links = new[]
			{
				new Link
				{
					Name = "adx_website", FromAttribute = "adx_websiteid", ToAttribute = "adx_websiteid",
					Filters = new[]
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Conditions = new[]
							{
								new Condition("statecode", ConditionOperator.Equal, 0),
								new Condition("adx_websiteid", ConditionOperator.Equal, websiteId),
							},
						}
					}
				}
			};

			return new Dictionary<string, object> { { "Links", links } };
		}
	}
}
