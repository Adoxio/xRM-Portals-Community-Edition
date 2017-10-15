/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Feedback;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Text;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Cms
{
	public class WebPageDataAdapter : RatingDataAdapter, ICommentDataAdapter
	{
		private bool? _hasCommentModerationPermission;

		protected enum StateCode
		{
			Active = 0
		}

		public WebPageDataAdapter(EntityReference pageReference, IDataAdapterDependencies dependencies) : base(pageReference, dependencies)
		{
			if (pageReference == null)
			{
				throw new ArgumentNullException("pageReference");
			}

			if (pageReference.LogicalName != "adx_webpage")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", pageReference.LogicalName), "pageReference");
			}

			WebPageReference = pageReference;

		}

		public WebPageDataAdapter(Entity page, IDataAdapterDependencies dependencies) : this(page.ToEntityReference(), dependencies) { }

		public WebPageDataAdapter(EntityReference pageReference, string portalName = null) : this(pageReference, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public WebPageDataAdapter(Entity page, string portalName = null) : this(page.ToEntityReference(), new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected EntityReference WebPageReference { get; private set; }

		public IDictionary<string, object> GetCommentAttributes(string content, string authorName = null, string authorEmail = null, string authorUrl = null, HttpContext context = null)
		{
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return new Dictionary<string, object>();
			}

			// Use write service context to ensure we're not getting the content map cached
			// version of the web page from the PortalContext.ServiceContext.
			var serviceContext = Dependencies.GetServiceContextForWrite();

			var page = serviceContext.CreateQuery("adx_webpage").FirstOrDefault(p => p.GetAttributeValue<Guid>("adx_webpageid") == WebPageReference.Id);

			if (page == null) throw new Exception("adx_webpage not found.");

			var postedOn = DateTime.UtcNow;

			var policyReader = GetCommentPolicyReader();

			Dictionary<string, object> attributes;

			attributes = new Dictionary<string, object>
			{
				{ "regardingobjectid", page.ToEntityReference() },
				{ "createdon", postedOn },
				{ "title", StringHelper.GetCommentTitleFromContent(content) },
				{ "adx_approved",     (policyReader.IsCommentPolicyOpen || policyReader.IsCommentPolicyOpenToAuthenticatedUsers) },
				{ "adx_createdbycontact", authorName },
				{ "adx_contactemail", authorEmail },
				{ "comments", content },
				{ "source", new OptionSetValue((int)FeedbackSource.Portal) }
			};

			var portalUser = Dependencies.GetPortalUser();

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
			return attributes;
		}

		public IEnumerable<IComment> SelectComments()
		{
			return SelectComments(0);
		}

		public IEnumerable<IComment> SelectComments(int startRowIndex, int maximumRows = -1)
		{
			var comments = new List<Comment>();
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback) || maximumRows == 0)
			{
				return comments;
			}
			var includeUnapprovedComments = TryAssertCommentModerationPermission(Dependencies);
			var query =
				OrganizationServiceContextExtensions.SelectCommentsByPage(
					OrganizationServiceContextExtensions.GetPageInfo(startRowIndex, maximumRows), WebPageReference.Id,
					includeUnapprovedComments);
			var commentsEntitiesResult = Dependencies.GetServiceContext().RetrieveMultiple(query);

			comments.AddRange(
				commentsEntitiesResult.Entities.Select(
					commentEntity =>
						new Comment(
							commentEntity,
							new Lazy<ApplicationPath>(() => Dependencies.GetEditPath(commentEntity.ToEntityReference()), LazyThreadSafetyMode.None),
							new Lazy<ApplicationPath>(() => Dependencies.GetDeletePath(commentEntity.ToEntityReference()), LazyThreadSafetyMode.None),
							new Lazy<bool>(() => includeUnapprovedComments, LazyThreadSafetyMode.None), (new RatingDataAdapter(commentEntity)).GetRatingInfo(), RatingsEnabled)));
			return comments;
		}

		public int SelectCommentCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var includeUnapprovedComments = TryAssertCommentModerationPermission(Dependencies);

			return serviceContext.FetchCount(FeedbackMetadataAttributes.PageCommentEntityName, FeedbackMetadataAttributes.PageCommentIdAttribute, addCondition =>
			{
				addCondition(FeedbackMetadataAttributes.WebPageIdAttribute, "eq", WebPageReference.Id.ToString());

				if (!includeUnapprovedComments)
				{
					addCondition("adx_approved", "eq", "true");
				}
			});
		}

		public string GetCommentLogicalName()
		{
			return  FeedbackMetadataAttributes.PageCommentEntityName;
		}

		public string GetCommentContentAttributeName()
		{
			return FeedbackMetadataAttributes.CommentAttribute;
		}

		public ICommentPolicyReader GetCommentPolicyReader()
		{
			// Use write service context to ensure we're not getting the content map cached
			// version of the web page from the PortalContext.ServiceContext.
			var serviceContext = Dependencies.GetServiceContextForWrite();

			var page = serviceContext.CreateQuery("adx_webpage").FirstOrDefault(p => p.GetAttributeValue<Guid>("adx_webpageid") == WebPageReference.Id);

			return new PageCommentPolicyReader(page);
		}

		protected virtual bool TryAssertCommentModerationPermission(IDataAdapterDependencies dependencies)
		{
			if (_hasCommentModerationPermission.HasValue)
			{
				return _hasCommentModerationPermission.Value;
			}
			var serviceContext = dependencies.GetServiceContext();
			var page = serviceContext.RetrieveSingle(
				WebPageReference.LogicalName,
				FetchAttribute.All,
				new Condition("adx_webpageid", ConditionOperator.Equal, WebPageReference.Id));

			if (page == null)
			{
				throw new InvalidOperationException("Unable to load the adx_webpage {0}.".FormatWith(WebPageReference.Id));
			}

			var security = Dependencies.GetSecurityProvider();

			_hasCommentModerationPermission = security.TryAssert(serviceContext, page, CrmEntityRight.Change);

			return _hasCommentModerationPermission.Value;
		}

		public override bool RatingsEnabled
		{
			get
			{
				// Use write service context to ensure we're not getting the content map cached
				// version of the web page from the PortalContext.ServiceContext.
				var serviceContext = Dependencies.GetServiceContextForWrite();

				var page = serviceContext.CreateQuery("adx_webpage").FirstOrDefault(p => p.GetAttributeValue<Guid>("adx_webpageid") == WebPageReference.Id);

				return page != null && (page.GetAttributeValue<bool?>("adx_enablerating") ?? false);
			}
		}

	}
}
