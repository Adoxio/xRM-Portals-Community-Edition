/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Blogs
{
	public interface IBlog
	{
		ApplicationPath ApplicationPath { get; }

		BlogCommentPolicy CommentPolicy { get; }

		Entity Entity { get; }

		ApplicationPath FeedPath { get; }

		Guid Id { get; }

		bool IsAggregation { get; }

		IHtmlString Summary { get; }

		string Title { get; }
	}
}
