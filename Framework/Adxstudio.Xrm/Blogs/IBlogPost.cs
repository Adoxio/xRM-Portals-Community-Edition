/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Blogs
{
	public interface IBlogPost : IRateable
	{
		ApplicationPath ApplicationPath { get; }

		IBlogAuthor Author { get; }

		int CommentCount { get; }

		BlogCommentPolicy CommentPolicy { get; }

		IHtmlString Content { get; }

		Entity Entity { get; }

		bool HasExcerpt { get; }

		Guid Id { get; }

		bool IsPublished { get; }

		DateTime LastUpdatedTime { get; }

		DateTime PublishDate { get; }

		IHtmlString Summary { get; }

		IEnumerable<IBlogPostTag> Tags { get; }

		string Title { get; }
	}
}
