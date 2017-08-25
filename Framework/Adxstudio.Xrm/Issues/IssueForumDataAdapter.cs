/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Core.Flighting;

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Provides methods to get and set data for an Adxstudio Portals Issue Forum such as issues.
	/// </summary>
	/// <remarks>Issues are returned ordered reverse chronologically by their submitted date.</remarks>
	public class IssueForumDataAdapter : IIssueForumDataAdapter
	{
		private bool? _hasIssuePreviewPermission;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="issueForum">The issue forum to get and set data for.</param>
		/// <param name="dependencies">The dependencies to use for getting and setting data.</param>
		public IssueForumDataAdapter(EntityReference issueForum, IDataAdapterDependencies dependencies)
		{
			issueForum.ThrowOnNull("issueForum");
			issueForum.AssertLogicalName("adx_issueforum");
			dependencies.ThrowOnNull("dependencies");

			IssueForum = issueForum;
			Dependencies = dependencies;
			Status = new int[] { };
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="issueForum">The issue forum to get and set data for.</param>
		/// <param name="dependencies">The dependencies to use for getting and setting data.</param>
		public IssueForumDataAdapter(Entity issueForum, IDataAdapterDependencies dependencies) : this(issueForum.ToEntityReference(), dependencies) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="issueForum">The issue forum to get and set data for.</param>
		/// <param name="dependencies">The dependencies to use for getting and setting data.</param>
		public IssueForumDataAdapter(IIssueForum issueForum, IDataAdapterDependencies dependencies) : this(issueForum.Entity, dependencies) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="issueForum">The issue forum to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IssueForumDataAdapter(EntityReference issueForum, string portalName = null) : this(issueForum, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="issueForum">The issue forum to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IssueForumDataAdapter(Entity issueForum, string portalName = null) : this(issueForum.ToEntityReference(), portalName) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="issueForum">The issue forum to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IssueForumDataAdapter(IIssueForum issueForum, string portalName = null) : this(issueForum.Entity, portalName) { }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected EntityReference IssueForum { get; private set; }

		public int? Priority { get; set; }

		public IEnumerable<int> Status { get; set; }

		/// <summary>
		/// Submit an issue to the issue forum this adapter applies to.
		/// </summary>
		/// <param name="title">The title of the issue.</param>
		/// <param name="copy">The copy of the issue.</param>
		/// <param name="track">Create an issue alert for the current user (user must be authenticated).</param>
		/// <param name="authorName">The name of the author for the issue (ignored if user is authenticated).</param>
		/// <param name="authorEmail">The email of the author for the issue (ignored if user is authenticated).</param>
		public virtual void CreateIssue(string title, string copy, bool track = false, string authorName = null, string authorEmail = null)
		{
			title.ThrowOnNullOrWhitespace("title");

			var httpContext = Dependencies.GetHttpContext();
			var author = Dependencies.GetPortalUser();

			if (!httpContext.Request.IsAuthenticated || author == null)
			{
				authorName.ThrowOnNullOrWhitespace("authorName", string.Format(ResourceManager.GetString("Error_Creating_IdeaAndIssue_Comment_WithNullOrWhitespace"), "issue"));
				authorEmail.ThrowOnNullOrWhitespace("authorEmail", string.Format(ResourceManager.GetString("Error_Creating_IdeaAndIssue_Comment_WithNullOrWhitespace"), "issue"));
			}

			var context = Dependencies.GetServiceContext();

			var issueForum = Select();

			if (!issueForum.CurrentUserCanSubmitIssues)
			{
				throw new InvalidOperationException(string.Format("The current user can't create an {0} with the current {0} submission policy.", "Issue"));
			}

			var username = httpContext.Request.IsAuthenticated
				? httpContext.User.Identity.Name
				: httpContext.Request.AnonymousID;

			var issue = new Entity("adx_issue");

			issue["adx_name"] = title;
			issue["adx_copy"] = copy;
			issue["adx_issueforumid"] = IssueForum;
			issue["adx_date"] = DateTime.UtcNow;
			issue["adx_partialurl"] = GetDefaultIssuePartialUrl(title);
			issue["adx_createdbyusername"] = username;
			// issue["adx_createdbyipaddress"] = httpContext.Request.UserHostAddress;
			issue["adx_approved"] = issueForum.IssueSubmissionPolicy != IssueForumIssueSubmissionPolicy.Moderated;

			if (author != null)
			{
				issue["adx_authorid"] = author;
			}
			else
			{
				issue["adx_authorname"] = authorName;
				issue["adx_authoremail"] = authorEmail;
			}

			context.AddObject(issue);
			context.SaveChanges();

			if (track)
			{
				new IssueDataAdapter(issue).CreateAlert(Dependencies.GetPortalUser());
			}

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Issue, HttpContext.Current, "create_issue", 1, issue.ToEntityReference(), "create");
			}
		}

		/// <summary>
		/// Returns the <see cref="IIssueForum"/> that this adapter applies to.
		/// </summary>
		public virtual IIssueForum Select()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var issueForum = serviceContext.CreateQuery("adx_issueforum")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_issueforumid") == IssueForum.Id);

			if (issueForum == null)
			{
				throw new InvalidOperationException("Can't find adx_issueforum having ID {0}.".FormatWith(IssueForum.Id));
			}

			return new IssueForumFactory(Dependencies.GetHttpContext()).Create(new[] { issueForum }).FirstOrDefault();
		}

		/// <summary>
		/// Returns issues that have been submitted to the issue forum this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first issue to be returned.</param>
		/// <param name="maximumRows">The maximum number of issues to return.</param>
		public virtual IEnumerable<IIssue> SelectIssues(int startRowIndex = 0, int maximumRows = -1)
		{
			if (startRowIndex < 0)
			{
				throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IIssue[] { };
			}

			var serviceContext = Dependencies.GetServiceContext();

			var includeUnapprovedIssues = TryAssertIssuePreviewPermission(serviceContext);

			var query = serviceContext.CreateQuery("adx_issue")
				.Where(issue => issue.GetAttributeValue<EntityReference>("adx_issueforumid") == IssueForum
					&& issue.GetAttributeValue<OptionSetValue>("statecode") != null && issue.GetAttributeValue<OptionSetValue>("statecode").Value == 0);

			if (!includeUnapprovedIssues)
			{
				query = query.Where(issue => issue.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false));
			}

			if (Priority.HasValue)
			{
				query = query.Where(issue => issue.GetAttributeValue<OptionSetValue>("adx_priority") != null && issue.GetAttributeValue<OptionSetValue>("adx_priority").Value == Priority.Value);
			}

			if (Status.Any())
			{
				var param = Expression.Parameter(typeof(Entity), "issue");

				var left =
					Expression.Call(
						param,
						"GetAttributeValue",
						new[] { typeof(int) },
						Expression.Constant("statuscode"));

				var statusEquals = Status.Aggregate<int, Expression>(null, (current, status) =>
					current == null
						? Expression.Equal(left, Expression.Constant(status))
						: Expression.OrElse(current, Expression.Equal(left, Expression.Constant(status))));

				var statusPredicate = Expression.Lambda(statusEquals, param) as Expression<Func<Entity, bool>>;

				query = query.Where(statusPredicate);
			}

			query = query.OrderByDescending(issue => issue.GetAttributeValue<DateTime?>("adx_date"));

			if (startRowIndex > 0)
			{
				query = query.Skip(startRowIndex);
			}

			if (maximumRows > 0)
			{
				query = query.Take(maximumRows);
			}

			return new IssueFactory(serviceContext, Dependencies.GetHttpContext()).Create(query);
		}

		/// <summary>
		/// Returns the number of issues that have been submitted to the issue forum this adapter applies to.
		/// </summary>
		/// <returns></returns>
		public virtual int SelectIssueCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var includeUnapprovedIssues = TryAssertIssuePreviewPermission(serviceContext);

			return serviceContext.FetchCount("adx_issue", "adx_issueid", addCondition =>
			{
				addCondition("adx_issueforumid", "eq", IssueForum.Id.ToString());
				addCondition("statecode", "eq", "0");

				if (!includeUnapprovedIssues)
				{
					addCondition("adx_approved", "eq", "true");
				}

				if (Priority.HasValue)
				{
					addCondition("adx_priority", "eq", "{0}".FormatWith(Priority.Value));
				}
			},
			addOrCondition =>
			{
				foreach (var status in Status)
				{
					addOrCondition("statuscode", "eq", "{0}".FormatWith(status));
				}
			});
		}

		protected virtual bool TryAssertIssuePreviewPermission(OrganizationServiceContext serviceContext)
		{
			if (_hasIssuePreviewPermission.HasValue)
			{
				return _hasIssuePreviewPermission.Value;
			}

			var issueForum = serviceContext.CreateQuery("adx_issueforum")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_issueforumid") == IssueForum.Id);

			if (issueForum == null)
			{
				throw new InvalidOperationException("Can't find adx_issueforum having ID {0}.".FormatWith(IssueForum.Id));
			}

			var security = Dependencies.GetSecurityProvider();

			_hasIssuePreviewPermission = security.TryAssert(serviceContext, issueForum, CrmEntityRight.Change);

			return _hasIssuePreviewPermission.Value;
		}

		private static string GetDefaultIssuePartialUrl(string title)
		{
			if (string.IsNullOrWhiteSpace(title))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "title");
			}

			string titleSlug;

			try
			{
				// Encoding the title as Cyrillic, and then back to ASCII, converts accented characters to their
				// unaccented version. We'll try/catch this, since it depends on the underlying platform whether
				// the Cyrillic code page is available.
				titleSlug = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(title)).ToLowerInvariant();
			}
			catch
			{
				titleSlug = title.ToLowerInvariant();
			}

			// Strip all disallowed characters.
			titleSlug = Regex.Replace(titleSlug, @"[^a-z0-9\s-]", string.Empty);

			// Convert all runs of multiple spaces to a single space.
			titleSlug = Regex.Replace(titleSlug, @"\s+", " ").Trim();

			// Cap the length of the title slug.
			titleSlug = titleSlug.Substring(0, titleSlug.Length <= 50 ? titleSlug.Length : 50).Trim();

			// Replace all spaces with hyphens.
			titleSlug = Regex.Replace(titleSlug, @"\s", "-").Trim('-');

			return titleSlug;
		}
	}
}
