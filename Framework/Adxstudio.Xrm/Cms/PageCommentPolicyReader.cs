/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Cms
{
	public class PageCommentPolicyReader : ICommentPolicyReader
	{
		private Entity _webPage;

		public PageCommentPolicyReader(Entity webPage)
		{
			_webPage = webPage;
		}

		internal PageCommentPolicy Policy
		{
			get
			{
				PageCommentPolicy pageCommentPolicy;

				var pageCommentPolicyAttributeValue = _webPage.GetAttributeValue<OptionSetValue>("adx_feedbackpolicy");

				if (pageCommentPolicyAttributeValue is OptionSetValue)
				{
					pageCommentPolicy = (PageCommentPolicy)Enum.ToObject(typeof(PageCommentPolicy), ((OptionSetValue)pageCommentPolicyAttributeValue).Value);
				}
				else
				{
					pageCommentPolicy = PageCommentPolicy.Inherit;
				}

				return (pageCommentPolicy != PageCommentPolicy.Inherit) ? pageCommentPolicy : GetParentPagePolicy() ?? GetDefaultPagePolicy();
			}
		}

		private Dictionary<string, PageCommentPolicy> PolicyDictionary
		{
			get
			{
				return new Dictionary<string, PageCommentPolicy> {
					{ "None", PageCommentPolicy.None },
					{ "Open", PageCommentPolicy.Open },
					{ "OpenToAuthenticatedUsers", PageCommentPolicy.OpenToAuthenticatedUsers },
					{ "Moderated", PageCommentPolicy.Moderated },
					{ "Closed", PageCommentPolicy.Closed }
				};
			}
		}

		public bool IsCommentPolicyOpen
		{
			get { return Policy == PageCommentPolicy.Open; }
		}

		public bool IsCommentPolicyOpenToAuthenticatedUsers
		{
			get { return Policy == PageCommentPolicy.OpenToAuthenticatedUsers; }
		}

		public bool IsCommentPolicyModerated
		{
			get { return Policy == PageCommentPolicy.Moderated; }
		}

		public bool IsCommentPolicyClosed
		{
			get { return Policy == PageCommentPolicy.Closed; }
		}

		public bool IsCommentPolicyNone
		{
			get { return Policy == PageCommentPolicy.None; }
		}

		private PageCommentPolicy? GetParentPagePolicy()
		{
			if (_webPage.GetAttributeValue<EntityReference>("adx_parentpageid") == null)
			{
				return null;
			}

			var entityRef = _webPage.GetAttributeValue<EntityReference>("adx_parentpageid");
			var portalOrgService = HttpContext.Current.GetOrganizationService();
			var parentPage = portalOrgService.RetrieveSingle(
				entityRef.LogicalName,
				FetchAttribute.None,
				new Condition("adx_webpageid", ConditionOperator.Equal, entityRef.Id));

			if (parentPage == null)
			{
				return null;
			}

			var parentPolicyReader = new PageCommentPolicyReader(parentPage);

			return parentPolicyReader.Policy;
		}

		private PageCommentPolicy GetDefaultPagePolicy()
		{
			var website = HttpContext.Current.GetWebsite();

			var defaultPageCommentPolicySiteSetting = website.Settings.Get<string>("cms/defaultPageCommentPolicy");

			if (string.IsNullOrWhiteSpace(defaultPageCommentPolicySiteSetting))
			{
				return PageCommentPolicy.None;
			}

			PageCommentPolicy policy;

			return PolicyDictionary.TryGetValue(defaultPageCommentPolicySiteSetting, out policy) ? policy : PageCommentPolicy.None;

		}
	}
}
