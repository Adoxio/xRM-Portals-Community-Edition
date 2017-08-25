/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Resources;
using System.Web;
using Adxstudio.Xrm.Core.Flighting;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Blogs
{
	public class Blog : IBlog
	{
		public Blog(Entity entity, ApplicationPath applicationPath, ApplicationPath feedPath = null)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.LogicalName != "adx_blog")
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

			CommentPolicy = entity.GetAttributeValue<OptionSetValue>("adx_commentpolicy") == null ? BlogCommentPolicy.Open : (BlogCommentPolicy)entity.GetAttributeValue<OptionSetValue>("adx_commentpolicy").Value;
			Id = entity.Id;
			Summary = new HtmlString(entity.GetAttributeValue<string>("adx_summary"));
			Title = entity.GetAttributeValue<string>("adx_name");

            if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
            {
                PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Blog, HttpContext.Current, "read_blog", 1, entity.ToEntityReference(), "read");
            }
        }

		public ApplicationPath ApplicationPath { get; private set; }

		public BlogCommentPolicy CommentPolicy { get; private set; }

		public Entity Entity { get; private set; }

		public ApplicationPath FeedPath { get; private set; }

		public Guid Id { get; private set; }

		public bool IsAggregation
		{
			get { return false; }
		}

		public IHtmlString Summary { get; private set; }

		public string Title { get; private set; }
	}
}
