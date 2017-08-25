/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;
using Adxstudio.Xrm.Forums;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Tagging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// The default view used by <see cref="TagCloudDataSource"/>.
	/// </summary>
	/// <seealso cref="TagCloudDataSource"/>
	/// <seealso cref="TagCloudData"/>
	/// <seealso cref="TagCloudDataItem"/>
	public class TagCloudDataSourceView : DataSourceView
	{
		public TagCloudDataSourceView(IDataSource owner, string viewName) : base(owner, viewName)
		{
			if (!(owner is TagCloudDataSource))
			{
				throw new ArgumentException("Must be of type {0}.".FormatWith(typeof(TagCloudDataSource)), "owner");
			}

			Owner = owner as TagCloudDataSource;
		}

		protected TagCloudDataSource Owner { get; private set; }

		protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
		{
			var context = PortalContext.Current.ServiceContext;

			var pageTags = context.GetPageTags().ToList().Select(tag => new PageTagInfo(tag)).Where(tagInfo => tagInfo.TaggedItemCount > 0);
			var eventTags = context.GetEventTags().ToList().Select(tag => new EventTagInfo(tag)).Where(tagInfo => tagInfo.TaggedItemCount > 0);
			var threadTags = context.GetForumThreadTags().ToList().Select(tag => new ForumThreadTagInfo(tag)).Where(tagInfo => tagInfo.TaggedItemCount > 0);

			IEnumerable<TagCloudDataItem> items = new TagCloudData(
				Owner.NumberOfWeights,
				StringComparer.InvariantCultureIgnoreCase,
				pageTags.Cast<ITagInfo>(),
				eventTags.Cast<ITagInfo>(),
				threadTags.Cast<ITagInfo>()).ToList();

			if (string.Equals(Owner.SortExpression, "TaggedItemCount", StringComparison.InvariantCultureIgnoreCase))
			{
				items = Owner.SortDirection == SortDirection.Descending
					? items.OrderByDescending(item => item.TaggedItemCount)
					: items.OrderBy(item => item.TaggedItemCount);
			}
			else
			{
				items = Owner.SortDirection == SortDirection.Descending
					? items.OrderByDescending(item => item.Name)
					: items.OrderBy(item => item.Name);
			}

			foreach (var item in items)
			{
				item.CssClass = Owner.WeightCssClassPrefix + item.Weight;
			}

			return items.ToList();
		}
	}
}
