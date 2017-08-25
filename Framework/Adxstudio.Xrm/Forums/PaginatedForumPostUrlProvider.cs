/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Forums
{
	public class PaginatedForumPostUrlProvider : IPaginatedForumPostUrlProvider
	{
		private readonly string _anchorFormat;
		private readonly string _pageQueryStringField;
		private readonly int _pageSize;

		public PaginatedForumPostUrlProvider(string pageQueryStringField, int pageSize, string anchorFormat = "post-{0}")
		{
			if (pageQueryStringField == null) throw new ArgumentNullException("pageQueryStringField");
			if (pageSize < 1) throw new ArgumentOutOfRangeException("pageSize", ResourceManager.GetString("Value_Must_Be_Greater_ThanOrEqualTo_One_Exception"));

			_pageQueryStringField = pageQueryStringField;
			_pageSize = pageSize;
			_anchorFormat = anchorFormat;
		}

		public string GetPostUrl(IForumThread forumThread, int forumThreadPostCount, Guid forumPostId)
		{
			string forumThreadUrl = forumThread.Url;

			if (forumThreadUrl == null)
			{
				return null;
			}

			if (forumThreadPostCount < 1)
			{
				return "{0}#{1}".FormatWith(forumThread.Url, _anchorFormat.FormatWith(forumPostId));
			}

			var pageNumber = ((forumThreadPostCount - 1) / _pageSize) + 1;

			return "{0}#{1}".FormatWith(
				forumThread.Url.AppendQueryString(_pageQueryStringField, pageNumber.ToString(CultureInfo.InvariantCulture)),
				_anchorFormat.FormatWith(forumPostId));
		}
	}
}
