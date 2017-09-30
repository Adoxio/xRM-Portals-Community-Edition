/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Issues
{
	internal class IssueForumFactory
	{
		private readonly HttpContextBase _httpContext;

		public IssueForumFactory(HttpContextBase httpContext)
		{
			httpContext.ThrowOnNull("httpContext");
			
			_httpContext = httpContext;
		}

		public IEnumerable<IIssueForum> Create(IEnumerable<Entity> issueForumEntities)
		{
			var issueForums = issueForumEntities.ToArray();

			return issueForums.Select(e => new IssueForum(e, _httpContext)).ToArray();
		}
	}
}
