/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Feedback;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Text;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Client.Messages;

namespace Adxstudio.Xrm.Blogs
{
	public class BlogPostDataAdapter : RatingDataAdapter, IBlogPostDataAdapter, ICommentDataAdapter
	{
		private BlogPostDataAdapter(EntityReference blogPost, IDataAdapterDependencies dependencies, BlogSecurityInfo security) : base(blogPost, dependencies)
		{
			if (blogPost == null) throw new ArgumentNullException("blogPost");
			if (dependencies == null) throw new ArgumentNullException("dependencies");
			if (security == null) throw new ArgumentNullException("security");

			if (blogPost.LogicalName != "adx_blogpost")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", blogPost.LogicalName), "blogPost");
			}

			BlogPostReference = blogPost;
			BlogDependencies = dependencies;
			Security = security;
		}

		public BlogPostDataAdapter(EntityReference blogPost, IDataAdapterDependencies dependencies) : this(blogPost, dependencies, new BlogSecurityInfo(blogPost, dependencies)) { }

		public BlogPostDataAdapter(Entity blogPost, IDataAdapterDependencies dependencies) : this(blogPost.ToEntityReference(), dependencies, new BlogSecurityInfo(blogPost, dependencies)) { }

		public BlogPostDataAdapter(IBlogPost blogPost, IDataAdapterDependencies dependencies) : this(blogPost.Entity, dependencies)
		{
			BlogPost = blogPost;
		}

		public BlogPostDataAdapter(EntityReference blogPost, string portalName = null) : this(blogPost, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public BlogPostDataAdapter(Entity blogPost, string portalName = null) : this(blogPost, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public BlogPostDataAdapter(IBlogPost blogPost, string portalName = null) : this(blogPost, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected EntityReference BlogPostReference { get; private set; }

		protected IDataAdapterDependencies BlogDependencies { get; private set; }

		protected IBlogPost BlogPost { get; private set; }

		protected BlogSecurityInfo Security { get; private set; }

		protected enum StateCode
		{
			Active = 0
		}

		public IDictionary<string, object> GetCommentAttributes(string content, string authorName = null, string authorEmail = null, string authorUrl = null, HttpContext context = null)
		{
			var post = Select();

			if (post == null)
			{
				throw new InvalidOperationException("Unable to load adx_blogpost {0}. Please ensure that this record exists, and is accessible by the current user.".FormatWith(BlogPostReference.Id));
			}

			var postedOn = DateTime.UtcNow;

			var attributes = new Dictionary<string, object>
			{
				{ "regardingobjectid",   post.Entity.ToEntityReference() },
				{ "createdon",           postedOn },
				{ "adx_approved",        post.CommentPolicy == BlogCommentPolicy.Open || post.CommentPolicy == BlogCommentPolicy.OpenToAuthenticatedUsers },
				{ "title",               StringHelper.GetCommentTitleFromContent(content) },
				{ "adx_createdbycontact", authorName },
				{ "adx_contactemail", authorEmail },
				{ "comments", content },
				{ "source", new OptionSetValue((int)FeedbackSource.Portal) }
			};

			var portalUser = BlogDependencies.GetPortalUser();

			if (portalUser != null && portalUser.LogicalName == "contact")
			{
				attributes[FeedbackMetadataAttributes.UserIdAttributeName] = portalUser;
			}
			else if (context != null && context.Profile != null)
			{
				attributes[FeedbackMetadataAttributes.VisitorAttributeName] = context.Profile.UserName;
			}

			if (authorUrl != null)
			{
				authorUrl = authorUrl.Contains(Uri.SchemeDelimiter) ? authorUrl : "{0}{1}{2}".FormatWith(Uri.UriSchemeHttp, Uri.SchemeDelimiter, authorUrl);

				if (Uri.IsWellFormedUriString(authorUrl, UriKind.Absolute))
				{
					attributes["adx_authorurl"] = authorUrl;
				}
			}
			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Blog, HttpContext.Current, "create_comment_blog", post.CommentCount, post.Entity.ToEntityReference(), "create");
			}

			return attributes;
		}

		public IBlogPost Select()
		{
			var serviceContext = BlogDependencies.GetServiceContext();

			var query = serviceContext.CreateQuery("adx_blogpost")
				.Where(post => post.GetAttributeValue<Guid>("adx_blogpostid") == BlogPostReference.Id);

			if (!Security.UserHasAuthorPermission)
			{
				query = query.Where(post => post.GetAttributeValue<bool?>("adx_published") == true);
			}

			var entity = query.FirstOrDefault();

			if (entity == null)
			{
				return null;
			}

			var securityProvider = BlogDependencies.GetSecurityProvider();

			if (!securityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Read))
			{
				return null;
			}

			var urlProvider = BlogDependencies.GetUrlProvider();
			var tagPathGenerator = new BlogArchiveApplicationPathGenerator(BlogDependencies);

			return new BlogPostFactory(serviceContext, urlProvider, BlogDependencies.GetWebsite(), tagPathGenerator).Create(new[] { entity }).FirstOrDefault();
		}

		public virtual IEnumerable<IComment> SelectComments()
		{
			return SelectComments(0);
		}

		public virtual IEnumerable<IComment> SelectComments(int startRowIndex, int maximumRows = -1)
		{
			var comments = new List<Comment>();
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback) || maximumRows == 0)
			{
				return comments;
			}
			var includeUnapprovedComments = Security.UserHasAuthorPermission;
			var query =
				Cms.OrganizationServiceContextExtensions.SelectCommentsByPage(
					Cms.OrganizationServiceContextExtensions.GetPageInfo(startRowIndex, maximumRows), BlogPostReference.Id,
					includeUnapprovedComments);
			var commentsEntitiesResult = Dependencies.GetServiceContext().RetrieveMultiple(query);
			comments.AddRange(
				commentsEntitiesResult.Entities.Select(
					commentEntity =>
						new Comment(commentEntity,
							new Lazy<ApplicationPath>(() => Dependencies.GetEditPath(commentEntity.ToEntityReference()), LazyThreadSafetyMode.None),
							new Lazy<ApplicationPath>(() => Dependencies.GetDeletePath(commentEntity.ToEntityReference()), LazyThreadSafetyMode.None),
							new Lazy<bool>(() => includeUnapprovedComments, LazyThreadSafetyMode.None), null, RatingsEnabled)));
			return comments;
		}

		public virtual int SelectCommentCount()
		{
			var serviceContext = BlogDependencies.GetServiceContext();
			var includeUnapprovedComments = Security.UserHasAuthorPermission;
			return serviceContext.FetchCount("feedback", "feedbackid", addCondition =>
			{
				addCondition("regardingobjectid", "eq", BlogPostReference.Id.ToString());

				if (!includeUnapprovedComments)
				{
					addCondition("adx_approved", "eq", "true");
				}
			});
		}

		public string GetCommentLogicalName()
		{
			return "feedback";
		}

		public string GetCommentContentAttributeName()
		{
			return "comments";
		}

		public ICommentPolicyReader GetCommentPolicyReader()
		{
			var blogPost = Select();

			return new BlogCommentPolicyReader(blogPost);
		}

		public override bool RatingsEnabled
		{
			get
			{
				var blogpost = Select();

				return blogpost != null && (blogpost.Entity.GetAttributeValue<bool?>("adx_enableratings") ?? false);
			}
		}

		public override IRatingInfo GetRatingInfo()
		{
			var blogPost = BlogPost ?? Select();

			return blogPost.RatingInfo;
		}

	}
}
