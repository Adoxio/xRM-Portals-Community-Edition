/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.Routing;
using Adxstudio.Xrm.Blogs;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public class RatingDataAdapterFactory : IRatingDataAdapterFactory
	{
		private EntityReference _entity;

		public RatingDataAdapterFactory(EntityReference entityReference)
		{
			_entity = entityReference;
		}

		public IRatingDataAdapter GetAdapter()
		{
			switch (_entity.LogicalName)
			{
				case "kbarticle":
				case "feedback":
					return new RatingDataAdapter(_entity);
				case "adx_webpage":
					return new WebPageDataAdapter(_entity);
				case "adx_blogpost":
					return new BlogPostDataAdapter(_entity);
				default:
					throw new Exception("Currently this entity type is not supported.");
			}
		}

		public IRatingDataAdapter GetAdapter(IDataAdapterDependencies dependencies)
		{
			switch (_entity.LogicalName)
			{
				case "kbarticle":
				case "feedback":
					return new RatingDataAdapter(_entity, dependencies);
				case "adx_webpage":
					return new WebPageDataAdapter(_entity, dependencies);
				case "adx_blogpost":
					return new BlogPostDataAdapter(_entity, dependencies as Adxstudio.Xrm.Blogs.IDataAdapterDependencies);
				default:
					throw new Exception("Currently this entity type is not supported.");
			}
		}

		public IRatingDataAdapter GetAdapter(IPortalContext portal, RequestContext requestContext)
		{
			switch (_entity.LogicalName)
			{
				case "kbarticle":
				case "feedback":
					var commentDependencies = new PortalContextDataAdapterDependencies(portal, requestContext: requestContext);
					return new RatingDataAdapter(_entity, commentDependencies);
				case "adx_webpage":
					var pageDependencies = new PortalContextDataAdapterDependencies(portal, requestContext: requestContext);
					return new WebPageDataAdapter(_entity, pageDependencies);
				case "adx_blogpost":
					var blogDependencies = new Blogs.PortalContextDataAdapterDependencies(portal, requestContext: requestContext);
					return new BlogPostDataAdapter(_entity, blogDependencies);
				default:
					throw new Exception("Currently this entity type is not supported.");
			}
		}
	}
}
