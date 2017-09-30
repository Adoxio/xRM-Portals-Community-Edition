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
using Adxstudio.Xrm.Feedback;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Text;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Provides methods to get and set data for an Adxstudio Portals Issue such as comments.
	/// </summary>
	public class IssueDataAdapter : IIssueDataAdapter, ICommentDataAdapter
	{
		private bool? _hasCommentModerationPermission;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="issue">The issue to get and set data for.</param>
		/// <param name="dependencies">The dependencies to use for getting and setting data.</param>
		public IssueDataAdapter(EntityReference issue, IDataAdapterDependencies dependencies)
		{
			issue.ThrowOnNull("issue");
			issue.AssertLogicalName("adx_issue");
			dependencies.ThrowOnNull("dependencies");

			Issue = issue;
			Dependencies = dependencies;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="issue">The issue to get and set data for.</param>
		/// <param name="dependencies">The dependencies to use for getting and setting data.</param>
		public IssueDataAdapter(Entity issue, IDataAdapterDependencies dependencies) : this(issue.ToEntityReference(), dependencies) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="issue">The issue to get and set data for.</param>
		/// <param name="dependencies">The dependencies to use for getting and setting data.</param>
		public IssueDataAdapter(IIssue issue, IDataAdapterDependencies dependencies) : this(issue.Entity, dependencies) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="issue">The issue to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IssueDataAdapter(EntityReference issue, string portalName = null) : this(issue, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="issue">The issue to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IssueDataAdapter(Entity issue, string portalName = null) : this(issue.ToEntityReference(), portalName) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="issue">The issue to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IssueDataAdapter(IIssue issue, string portalName = null) : this(issue.Entity, portalName) { }

		protected EntityReference Issue { get; private set; }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		/// <summary>
		/// Gets or sets whether or not comments should be in chronological order (default false [reverse chronological]).
		/// </summary>
		public bool? ChronologicalComments { get; set; }

		/// <summary>
		/// Create an issue alert entity (subscription) for the user.
		/// </summary>
		/// <param name="user">The user to create an issue alert entity (subsciption) for.</param>
		public void CreateAlert(EntityReference user)
		{
			user.ThrowOnNull("user");

			if (user.LogicalName != "contact")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", user.LogicalName), "user");
			}

			var serviceContext = Dependencies.GetServiceContextForWrite();

			var existingAlert = GetIssueAlertEntity(serviceContext, user);

			if (existingAlert != null)
			{
				return;
			}

			var alert = new Entity("adx_issuealert");

			alert["adx_subscriberid"] = user;
			alert["adx_issueid"] = Issue;

			serviceContext.AddObject(alert);
			serviceContext.SaveChanges();
		}

		/// <summary>
		/// Post a comment for the issue this adapter applies to.
		/// </summary>
		/// <param name="content">The comment copy.</param>
		/// <param name="authorName">The name of the author for this comment (ignored if user is authenticated).</param>
		/// <param name="authorEmail">The email of the author for this comment (ignored if user is authenticated).</param>
		public virtual void CreateComment(string content, string authorName = null, string authorEmail = null)
		{
			content.ThrowOnNullOrWhitespace("content");

			var httpContext = Dependencies.GetHttpContext();
			var author = Dependencies.GetPortalUser();

			if (!httpContext.Request.IsAuthenticated || author == null)
			{
				authorName.ThrowOnNullOrWhitespace("authorName", string.Format(ResourceManager.GetString("Error_Creating_IdeaAndIssue_Comment_WithNullOrWhitespace"), "issue comment"));
				authorEmail.ThrowOnNullOrWhitespace("authorEmail", string.Format(ResourceManager.GetString("Error_Creating_IdeaAndIssue_Comment_WithNullOrWhitespace"), "issue comment"));
			}

			var context = Dependencies.GetServiceContext();

			var issue = Select();

			if (!issue.CurrentUserCanComment)
			{
				throw new InvalidOperationException("An issue comment can't be created with the current issue comment policy.");
			}

			var comment = new Entity("feedback");

			comment["title"] = StringHelper.GetCommentTitleFromContent(content);
			comment["comments"] = content;
			comment["regardingobjectid"] = Issue;
			comment["createdon"] = DateTime.UtcNow;
			comment["adx_approved"] = issue.CommentPolicy != IssueForumCommentPolicy.Moderated;
			comment["adx_createdbycontact"] = authorName;
			comment["adx_contactemail"] = authorEmail;
			comment["source"] = new OptionSetValue((int)FeedbackSource.Portal);

			if (author != null && author.LogicalName == "contact")
			{
				comment[FeedbackMetadataAttributes.UserIdAttributeName] = author;
			}
			else if (context != null)
			{
				comment[FeedbackMetadataAttributes.VisitorAttributeName] = httpContext.Profile.UserName;
			}

			context.AddObject(comment);
			context.SaveChanges();

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Issue, HttpContext.Current, "create_comment_issue", 1, comment.ToEntityReference(), "create");
			}
		}

		/// <summary>
		/// Delete an issue alert entity (subscription) for the user.
		/// </summary>
		/// <param name="user">The user to remove an issue alert entity (subsciption) for.</param>
		public void DeleteAlert(EntityReference user)
		{
			user.ThrowOnNull("user");

			var serviceContext = Dependencies.GetServiceContextForWrite();

			var existingAlert = GetIssueAlertEntity(serviceContext, user);

			if (existingAlert == null)
			{
				return;
			}

			serviceContext.DeleteObject(existingAlert);
			serviceContext.SaveChanges();
		}

		/// <summary>
		/// Gets attributes to be added to a new comment that has just been created. Likely from the CrmEntityFormView.
		/// </summary>
		public IDictionary<string, object> GetCommentAttributes(string content, string authorName = null, string authorEmail = null, string authorUrl = null, HttpContext httpContext = null)
		{
			var issue = Select();

			if (issue == null)
			{
				throw new InvalidOperationException("Unable to load adx_issue entity with ID {0}. Make sure that this record exists and is accessible by the current user.".FormatWith(Issue.Id));
			}

			var postedOn = DateTime.UtcNow;

			var attributes = new Dictionary<string, object>
			{
				{ "regardingobjectid",	Issue },
				{ "createdon",	postedOn },
				{ "title",	StringHelper.GetCommentTitleFromContent(content) },
				{ "adx_approved",	issue.CommentPolicy == IssueForumCommentPolicy.Open || issue.CommentPolicy == IssueForumCommentPolicy.OpenToAuthenticatedUsers },
				{ "adx_createdbycontact", authorName },
				{ "adx_contactemail", authorEmail },
				{ "comments",	content },
			};

			var portalUser = Dependencies.GetPortalUser();

			if (portalUser != null && portalUser.LogicalName == "contact")
			{
				attributes[FeedbackMetadataAttributes.UserIdAttributeName] = portalUser;
			}
			else if (httpContext != null && httpContext.Profile != null)
			{
				attributes[FeedbackMetadataAttributes.VisitorAttributeName] = httpContext.Profile.UserName;
			}

			return attributes;
		}

		public string GetCommentContentAttributeName()
		{
			return "comments";
		}

		public string GetCommentLogicalName()
		{
			return "feedback";
		}

		public ICommentPolicyReader GetCommentPolicyReader()
		{
			var issue = Select();

			return new IssueCommentPolicyReader(issue);
		}

		/// <summary>
		/// Returns whether or not an issue alert entity (subscription) exists for the user.
		/// </summary>
		public bool HasAlert()
		{
			var user = Dependencies.GetPortalUser();

			if (user == null)
			{
				return false;
			}
			
			return GetIssueAlertEntity(Dependencies.GetServiceContext(), user) != null;
		}

		/// <summary>
		/// Returns the <see cref="IIssue"/> that this adapter applies to.
		/// </summary>
		public virtual IIssue Select()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var issue = GetIssueEntity(serviceContext);

			return new IssueFactory(serviceContext, Dependencies.GetHttpContext()).Create(new[] { issue }).FirstOrDefault();
		}

		public virtual IEnumerable<IComment> SelectComments()
		{
			return SelectComments(0);
		}
		
		/// <summary>
		/// Returns comments that have been posted for the issue this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first comment to be returned.</param>
		/// <param name="maximumRows">The maximum number of comments to return.</param>
		public virtual IEnumerable<IComment> SelectComments(int startRowIndex, int maximumRows = -1)
		{
			var comments = new List<Comment>();
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback) || maximumRows == 0)
			{
				return comments;
			}
			var includeUnapprovedComments = TryAssertCommentModerationPermission(Dependencies.GetServiceContext());
			var query =
				Cms.OrganizationServiceContextExtensions.SelectCommentsByPage(
					Cms.OrganizationServiceContextExtensions.GetPageInfo(startRowIndex, maximumRows), Issue.Id,
					includeUnapprovedComments);
			var commentsEntitiesResult = Dependencies.GetServiceContext().RetrieveMultiple(query);
			comments.AddRange(
				commentsEntitiesResult.Entities.Select(
					commentEntity =>
						new Comment(commentEntity,
							new Lazy<ApplicationPath>(() => Dependencies.GetEditPath(commentEntity.ToEntityReference()), LazyThreadSafetyMode.None),
							new Lazy<ApplicationPath>(() => Dependencies.GetDeletePath(commentEntity.ToEntityReference()), LazyThreadSafetyMode.None),
							new Lazy<bool>(() => includeUnapprovedComments, LazyThreadSafetyMode.None))));
			return comments;
		}

		/// <summary>
		/// Returns the number of comments that have been posted for the issue this adapter applies to.
		/// </summary>
		public virtual int SelectCommentCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var includeUnapprovedComments = TryAssertCommentModerationPermission(serviceContext);

			return serviceContext.FetchCount("feedback", "feedbackid", addCondition =>
			{
				addCondition("regardingobjectid", "eq", Issue.Id.ToString());

				if (!includeUnapprovedComments)
				{
					addCondition("adx_approved", "eq", "true");
				}
			});
		}

		protected virtual bool TryAssertCommentModerationPermission(OrganizationServiceContext serviceContext)
		{
			if (_hasCommentModerationPermission.HasValue)
			{
				return _hasCommentModerationPermission.Value;
			}

			var security = Dependencies.GetSecurityProvider();

			_hasCommentModerationPermission = security.TryAssert(serviceContext, GetIssueEntity(serviceContext), CrmEntityRight.Change);

			return _hasCommentModerationPermission.Value;
		}

		private Entity GetIssueAlertEntity(OrganizationServiceContext serviceContext, EntityReference user)
		{
			serviceContext.ThrowOnNull("serviceContext");
			user.ThrowOnNull("user");

			return serviceContext.CreateQuery("adx_issuealert")
				.FirstOrDefault(e => e.GetAttributeValue<EntityReference>("adx_subscriberid") == user
					&& e.GetAttributeValue<EntityReference>("adx_issueid") == Issue);
		}

		private Entity GetIssueEntity(OrganizationServiceContext serviceContext)
		{
			var issue = serviceContext.CreateQuery("adx_issue")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_issueid") == Issue.Id);

			if (issue == null)
			{
				throw new InvalidOperationException(string.Format("Can't find {0} having ID {1}.", "adx_issue", Issue.Id));
			}
			
			return issue;
		}
	}
}
