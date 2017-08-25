/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Blogs;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class BlogDrop : EntityDrop
	{
		private readonly IDataAdapterDependencies _dependencies;

		public BlogDrop(IPortalLiquidContext portalLiquidContext, IDataAdapterDependencies dependencies, IBlog blog)
			: base(portalLiquidContext, blog.Entity)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");
			if (blog == null) throw new ArgumentNullException("blog");

			_dependencies = dependencies;

			Blog = blog;
		}

		public string FeedUrl
		{
			get { return Blog.FeedPath.AbsolutePath; }
		}

		public string Name
		{
			get { return Title; }
		}

		public BlogPostsDrop Posts
		{
			get { return new BlogPostsDrop(this, _dependencies, Blog); }
		}

		public string Summary
		{
			get { return Blog.Summary.ToString(); }
		}

		public string Title
		{
			get { return Blog.Title; }
		}

		public override string Url
		{
			get { return Blog.ApplicationPath.AbsolutePath; }
		}

		protected IBlog Blog { get; private set; }
	}
}
