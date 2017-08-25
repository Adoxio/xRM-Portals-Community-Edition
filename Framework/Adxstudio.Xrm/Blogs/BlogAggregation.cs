/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Blogs
{
	internal class BlogAggregation : IBlog
	{
		public BlogAggregation(Entity entity, ApplicationPath applicationPath, ApplicationPath feedPath = null)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.LogicalName != "adx_webpage")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", entity.LogicalName), "entity");
			}

			if (applicationPath == null)
			{
				throw new ArgumentNullException("applicationPath");
			}

			Entity = entity;
			ApplicationPath = applicationPath;
			FeedPath = feedPath;

			Id = entity.Id;
			Summary = new HtmlString(entity.GetAttributeValue<string>("adx_summary"));

			var title = entity.GetAttributeValue<string>("adx_title");
			Title = string.IsNullOrWhiteSpace(title) ? entity.GetAttributeValue<string>("adx_name") : title;
		}

		public ApplicationPath ApplicationPath { get; private set; }

		public BlogCommentPolicy CommentPolicy
		{
			get { return BlogCommentPolicy.None; }
		}

		public Entity Entity { get; private set; }

		public ApplicationPath FeedPath { get; private set; }

		public Guid Id { get; private set; }

		public bool IsAggregation
		{
			get { return true; }
		}

		public IHtmlString Summary { get; private set; }

		public string Title { get; private set; }
	}
}
