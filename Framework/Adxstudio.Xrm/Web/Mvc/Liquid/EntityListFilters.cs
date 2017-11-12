/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Web;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Diagnostics.Trace;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Adxstudio.Xrm.Web.UI.WebControls;
using DotLiquid;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public static class EntityListFilters
	{
		public static string CurrentSort(string sortExpression, string attribute)
		{
			if (string.IsNullOrEmpty(sortExpression) || string.IsNullOrEmpty(attribute))
			{
				return null;
			}

			var match = Regex.Match(sortExpression, "{0} (?<direction>ASC|DESC)".FormatWith(attribute), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

			return match.Success ? match.Groups["direction"].Value.ToUpperInvariant() : null;
		}

		public static string ReverseSort(string sortDirection)
		{
			if (string.IsNullOrEmpty(sortDirection))
			{
				return null;
			}

			if (string.Equals(sortDirection.Trim(), "asc", StringComparison.InvariantCultureIgnoreCase))
			{
				return "DESC";
			}

			if (string.Equals(sortDirection.Trim(), "desc", StringComparison.InvariantCultureIgnoreCase))
			{
				return "ASC";
			}

			return null;
		}

		public static IEnumerable<FilterOptionGroupDrop> Metafilters(EntityListDrop entityList, string query = null, EntityViewDrop entityView = null)
		{
			if (entityList == null || string.IsNullOrWhiteSpace(entityList.FilterDefinition))
			{
				return Enumerable.Empty<FilterOptionGroupDrop>();
			}

			Fetch fetch;

			try
			{
				fetch = Fetch.FromJson(entityList.FilterDefinition);
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error parsing entity list filter definition: {0}", e.ToString()));

                return Enumerable.Empty<FilterOptionGroupDrop>();
			}

			if (fetch == null)
			{
				return Enumerable.Empty<FilterOptionGroupDrop>();
			}

			var serviceContext = entityList.PortalViewContext.CreateServiceContext();
			var portalContext = entityList.PortalViewContext.CreatePortalContext();

			EntityMetadata entityMetadata;

			try
			{
				entityMetadata = serviceContext.GetEntityMetadata(entityList.EntityLogicalName, EntityFilters.Attributes);
			}
			catch (FaultException<OrganizationServiceFault> e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format(@"Error retrieving metadata for entity ""{0}"": {1}", EntityNamePrivacy.GetEntityName(entityList.EntityLogicalName), e.ToString()));

                return Enumerable.Empty<FilterOptionGroupDrop>();
			}

			if (entityMetadata == null)
			{
				return Enumerable.Empty<FilterOptionGroupDrop>();
			}

			var currentFilters = string.IsNullOrWhiteSpace(query)
				? new NameValueCollection()
				: HttpUtility.ParseQueryString(query);

			return FilterOptionGroup.FromFetch(
				serviceContext,
				portalContext,
				entityMetadata,
				fetch,
				currentFilters,
				entityList.LanguageCode,
				entityView == null ? null : entityView.Columns.ToDictionary(e => e.LogicalName, e => e.Name))
				.Select(@group => new FilterOptionGroupDrop(@group));
		}
	}

	public class FilterOptionGroupDrop : Drop
	{
		internal FilterOptionGroupDrop(FilterOptionGroup @group)
		{
			if (@group == null) throw new ArgumentNullException("group");

			Id = @group.Id;
			Label = @group.Label;
			Order = @group.Order;
			SelectionMode = @group.SelectionMode;

			Options = @group.Options.Select(option => new FilterOptionDrop(option)).ToArray();
		}

		public string Id { get; private set; }

		public string Label { get; private set; }

		public string Order { get; private set; }

		public string SelectionMode { get; private set; }

		public IEnumerable<FilterOptionDrop> Options { get; private set; }
	}

	public class FilterOptionDrop : Drop
	{
		internal FilterOptionDrop(FilterOption option)
		{
			if (option == null) throw new ArgumentNullException("option");

			Id = option.Id;
			Type = option.Type;
			Label = option.Label;
			Checked = option.Checked;
			Text = option.Text;
		}

		public string Id { get; private set; }

		public string Type { get; private set; }

		public string Label { get; private set; }

		public bool Checked { get; private set; }

		public string Text { get; private set; }
	}
}
