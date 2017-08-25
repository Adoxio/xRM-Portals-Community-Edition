/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Forums
{
	using System;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Services.Query;

	/// <summary>
	/// Provides query access to all Forums (adx_communityforum) that are children of a given Web Page (adx_webpage).
	/// </summary>
	public class WebPageChildForumDataAdapter : ForumAggregationDataAdapter
	{
		public WebPageChildForumDataAdapter(EntityReference webPage, IDataAdapterDependencies dependencies) : base(dependencies)
		{
			if (webPage == null) throw new ArgumentNullException("webPage");

			if (webPage.LogicalName != "adx_webpage")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", webPage.LogicalName), "webPage");
			}

			WebPage = webPage;
		}

		public EntityReference WebPage { get; set; }

		protected override Filter GetWhereExpression()
		{
			var filter = new Filter
			{
				Conditions = new[]
				{
					new Condition("adx_parentpageid", ConditionOperator.Equal, WebPage.Id),
					new Condition("statecode", ConditionOperator.Equal, 0),
					new Condition("adx_hiddenfromsitemap", ConditionOperator.Equal, false)
				}
			};
			return filter;
		}
	}
}
