/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web.UI.CrmEntityListView;

	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Query;

	/// <summary>
	/// The portal language special case. Only portal languages that are enabled in the org are valid.
	/// </summary>
	internal class PortalLanguageViewSpecialCase : IViewSpecialCase
	{
		/// <summary>Determines if the special case is applicable.</summary>
		/// <param name="configuration">The configuration.</param>
		/// <returns>True if applicable, false otherwise.</returns>
		public bool IsApplicable(IViewConfiguration configuration)
		{
			return string.Equals(configuration.EntityName, "adx_portallanguage", StringComparison.InvariantCulture);
		}

		/// <summary>Try to apply the special case.</summary>
		/// <param name="configuration">The configuration.</param>
		/// <param name="dependencies">The dependencies.</param>
		/// <param name="customParameters">The custom parameters.</param>
		/// <param name="fetch">The fetch.</param>
		/// <returns>True if applied, false otherwise.</returns>
		public bool TryApply(IViewConfiguration configuration, IDataAdapterDependencies dependencies, IDictionary<string, string> customParameters, Fetch fetch)
		{
			if (!this.IsApplicable(configuration))
			{
				return false;
			}

			var contextLanguageInfo = dependencies.GetRequestContext()?.HttpContext?.GetContextLanguageInfo();
			if (contextLanguageInfo == null || !contextLanguageInfo.IsCrmMultiLanguageEnabled)
			{
				return false;
			}

			var serviceContext = dependencies.GetServiceContext();
			var provisionedLanguages = ContextLanguageInfo.GetProvisionedLanugages(serviceContext as IOrganizationService);

			if (!provisionedLanguages.Any())
			{
				return false;
			}

			var languageCondition = new Condition
			{
				Attribute = "adx_systemlanguage",
				Operator = ConditionOperator.In,
				Values = provisionedLanguages.Cast<object>().ToArray()
			};

			var filter = new Filter { Conditions = new[] { languageCondition } };
			if (fetch.Entity.Filters == null)
			{
				fetch.Entity.Filters = new List<Filter> { filter };
			}
			else
			{
				fetch.Entity.Filters.Add(filter);
			}

			return true;
		}
	}
}
