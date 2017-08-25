/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.Blogs;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class BlogsDrop : PortalUrlDrop
	{
		private readonly IBlogAggregationDataAdapter _adapter;
		private readonly Lazy<IBlog> _aggregation;
		private readonly IDataAdapterDependencies _dependencies;

		public BlogsDrop(IPortalLiquidContext portalLiquidContext, IDataAdapterDependencies dependencies)
			: base(portalLiquidContext)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			_dependencies = dependencies;
			_adapter = new WebsiteBlogAggregationDataAdapter(dependencies);
			_aggregation = new Lazy<IBlog>(() => _adapter.Select(), LazyThreadSafetyMode.None);
		}

		public string FeedUrl
		{
			get { return Aggregation.FeedPath.AbsolutePath; }
		}

		public BlogPostsDrop Posts
		{
			get { return new BlogPostsDrop(this, _dependencies); }
		}

		public override string Url
		{
			get { return Aggregation.ApplicationPath.AbsolutePath; }
		}

		protected IBlog Aggregation
		{
			get { return _aggregation.Value; }
		}

		/// <summary>
		/// Used to pass in a string representing either the name or GUID of a blog. 
		/// </summary>
		/// <param name="method">The ID or name of the blog</param>
		/// <returns>A BlogDrop object</returns>
		public override object BeforeMethod(string method)
		{
			if (method == null)
			{
				return null;
			}

			Guid parsed;

			// If the method can be parsed as a Guid, look up the set by that.
			if (Guid.TryParse(method, out parsed))
			{
				var blogById = _adapter.Select(parsed);

				return blogById == null ? null : new BlogDrop(this, _dependencies, blogById);
			}

			var blogByName = _adapter.Select(method);

			return blogByName == null ? null : new BlogDrop(this, _dependencies, blogByName);
		}
	}
}
