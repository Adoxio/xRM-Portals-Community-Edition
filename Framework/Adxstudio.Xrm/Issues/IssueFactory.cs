/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Issues
{
	internal class IssueFactory
	{
		private readonly OrganizationServiceContext _serviceContext;
		private readonly HttpContextBase _httpContext;
		
		public IssueFactory(OrganizationServiceContext serviceContext, HttpContextBase httpContext)
		{
			serviceContext.ThrowOnNull("serviceContext");
			httpContext.ThrowOnNull("httpContext");

			_serviceContext = serviceContext;
			_httpContext = httpContext;
		}

		public IEnumerable<IIssue> Create(IEnumerable<Entity> issueEntities)
		{
			var issues = issueEntities.ToArray();
			var issueIds = issues.Select(e => e.Id).ToArray();

			var extendedDatas = _serviceContext.FetchIssueExtendedData(issueIds);
			var commentCounts = _serviceContext.FetchIssueCommentCounts(issueIds);

			return issues.Select(e =>
			{
				IssueExtendedData extendedDataValue;
				var extendedData = extendedDatas.TryGetValue(e.Id, out extendedDataValue)
					? extendedDataValue
					: IssueExtendedData.Default;

				int commentCountValue;
				var commentCount = commentCounts.TryGetValue(e.Id, out commentCountValue) ? commentCountValue : 0;

				var authorName = extendedData.AuthorName;
				var authorEmail = e.GetAttributeValue<string>("adx_authoremail") ?? extendedData.AuthorEmail;

				var issueCommentPolicy = e.GetAttributeValue<OptionSetValue>("adx_commentpolicy") == null ? IssueCommentPolicy.Inherit : (IssueCommentPolicy)e.GetAttributeValue<OptionSetValue>("adx_commentpolicy").Value;
				var issueForumCommentPolicy = issueCommentPolicy == IssueCommentPolicy.Inherit
					? extendedData.IssueForumCommentPolicy
					: (IssueForumCommentPolicy)(int)issueCommentPolicy;

				return new Issue(
					e,
					_httpContext,
					commentCount,
					authorName,
					authorEmail,
					extendedData.IssueForumTitle,
					extendedData.IssueForumPartialUrl,
					issueForumCommentPolicy);
			}).ToArray();
		}
	}
}
