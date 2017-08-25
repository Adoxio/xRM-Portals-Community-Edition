/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public static class BlogFunctions
	{
		public static BlogPostsDrop OrderBy(BlogPostsDrop postsDrop, string key, string direction = "asc")
		{
			return new BlogPostsDrop(postsDrop.PortalLiquidContext, postsDrop.Dependencies, postsDrop.StartRowIndex, postsDrop.PageSize, key, direction);
		}

		public static BlogPostsDrop Paginate(BlogPostsDrop postsDrop, int startRowIndex, int pageSize)
		{
			return new BlogPostsDrop(postsDrop.PortalLiquidContext, postsDrop.Dependencies, startRowIndex, pageSize, postsDrop.OrderByKey, postsDrop.SortDirection);
		}

		public static BlogPostsDrop Take(BlogPostsDrop postsDrop, int pageSize)
		{
			return new BlogPostsDrop(postsDrop.PortalLiquidContext, postsDrop.Dependencies, postsDrop.StartRowIndex, pageSize, postsDrop.OrderByKey, postsDrop.SortDirection);
		}

		public static BlogPostsDrop FromIndex(BlogPostsDrop postsDrop, int startRowIndex)
		{
			return new BlogPostsDrop(postsDrop.PortalLiquidContext, postsDrop.Dependencies, startRowIndex, postsDrop.PageSize, postsDrop.OrderByKey, postsDrop.SortDirection);
		}

		public static IEnumerable<BlogPostDrop> All(BlogPostsDrop postsDrop)
		{
			return postsDrop.All;
		}
	}
}
