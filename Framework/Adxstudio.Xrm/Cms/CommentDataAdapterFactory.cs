/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Routing;
using Adxstudio.Xrm.Blogs;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public class CommentDataAdapterFactory : ICommentDataAdapterFactory
	{
		private readonly EntityReference _entity;

		public CommentDataAdapterFactory(EntityReference entityReference)
		{
			_entity = entityReference;
		}

		public ICommentDataAdapter GetAdapter()
		{
			switch (_entity.LogicalName)
			{
				case "adx_webpage":
					return new WebPageDataAdapter(_entity);
				case "adx_blogpost":
					return new BlogPostDataAdapter(_entity);
				default:
					return null;
			}
		}

		public ICommentDataAdapter GetAdapter(IPortalContext portal, RequestContext requestContext)
		{
			switch (_entity.LogicalName)
			{
				case "adx_webpage":
					var pageDependencies = new PortalContextDataAdapterDependencies(portal, requestContext: requestContext);
					return new WebPageDataAdapter(_entity, pageDependencies);
				case "adx_blogpost":
					var blogDependencies = new Blogs.PortalContextDataAdapterDependencies(portal, requestContext: requestContext);
					return new BlogPostDataAdapter(_entity, blogDependencies);
				default:
					return null;
			}
		}

		public ICommentDataAdapter GetAdapter(IDataAdapterDependencies dependencies)
		{
			switch (_entity.LogicalName)
			{
				case "adx_webpage": 
					return new WebPageDataAdapter(_entity, dependencies);
				case "adx_blogpost":
					var blogDependencies = dependencies as Blogs.IDataAdapterDependencies;
					return new BlogPostDataAdapter(_entity, blogDependencies);
				default:
					return null;
			}
		} 
	}
}
