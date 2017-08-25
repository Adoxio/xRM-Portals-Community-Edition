/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public static class ForumFunctions
	{
		public static ForumThreadsDrop OrderBy(ForumThreadsDrop threadsDrop, string key, string direction = "asc")
		{
			return new ForumThreadsDrop(threadsDrop.PortalLiquidContext, threadsDrop.Dependencies, threadsDrop.Forum, threadsDrop.StartRowIndex, threadsDrop.PageSize, key, direction);
		}

		public static ForumThreadsDrop Paginate(ForumThreadsDrop threadsDrop, int startRowIndex, int pageSize)
		{
			return new ForumThreadsDrop(threadsDrop.PortalLiquidContext, threadsDrop.Dependencies, threadsDrop.Forum, startRowIndex, pageSize, threadsDrop.OrderByKey, threadsDrop.SortDirection);
		}

		public static ForumThreadsDrop Take(ForumThreadsDrop threadsDrop, int pageSize)
		{
			return new ForumThreadsDrop(threadsDrop.PortalLiquidContext, threadsDrop.Dependencies, threadsDrop.Forum, threadsDrop.StartRowIndex, pageSize, threadsDrop.OrderByKey, threadsDrop.SortDirection);
		}

		public static ForumThreadsDrop FromIndex(ForumThreadsDrop threadsDrop, int startRowIndex)
		{
			return new ForumThreadsDrop(threadsDrop.PortalLiquidContext, threadsDrop.Dependencies, threadsDrop.Forum, startRowIndex, threadsDrop.PageSize, threadsDrop.OrderByKey, threadsDrop.SortDirection);
		}

		public static ForumPostsDrop Paginate(ForumPostsDrop postsDrop, int startRowIndex, int pageSize)
		{
			return new ForumPostsDrop(postsDrop.PortalLiquidContext, postsDrop.Dependencies, postsDrop.ForumThread, startRowIndex, pageSize);
		}

		public static ForumPostsDrop Take(ForumPostsDrop postsDrop, int pageSize)
		{
			return new ForumPostsDrop(postsDrop.PortalLiquidContext, postsDrop.Dependencies, postsDrop.ForumThread, postsDrop.StartRowIndex, pageSize);
		}

		public static ForumPostsDrop FromIndex(ForumPostsDrop postsDrop, int startRowIndex)
		{
			return new ForumPostsDrop(postsDrop.PortalLiquidContext, postsDrop.Dependencies, postsDrop.ForumThread, startRowIndex, postsDrop.PageSize);
		}

		public static IEnumerable<ForumThreadDrop> All(ForumThreadsDrop threadsDrop)
		{
			return threadsDrop.All;
		}

		public static IEnumerable<ForumPostDrop> All(ForumPostsDrop postsDrop)
		{
			return postsDrop.All;
		}
	}
}
