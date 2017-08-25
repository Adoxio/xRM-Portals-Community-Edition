/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.Mvc;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Issues;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Site.Areas.Issues.ViewModels;
using OrganizationServiceContextExtensions = Adxstudio.Xrm.Cms.OrganizationServiceContextExtensions;

namespace Site.Areas.Issues.Controllers
{
	[PortalView, PortalSecurity]
	public class IssuesController : Controller
	{
		private const string PageNotFoundSiteMarker = "Page Not Found";

		// GET: /issues/{issueForumPartialUrl}/{issuePartialUrl}
		[HttpGet]
		public ActionResult Issues(string issueForumPartialUrl, string issuePartialUrl, int? page)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext();
			
			if (string.IsNullOrWhiteSpace(issueForumPartialUrl))
			{
				return GetIssuesViewOrRedirectToOnlyIssueForum();
			}

			var issueForum = GetIssueForum(issueForumPartialUrl, context);

			if (issueForum == null || !Authorized(context, issueForum))
			{
				return RedirectToPageNotFound();
			}
			
			if (string.IsNullOrWhiteSpace(issuePartialUrl))
			{
				return GetIssueForumView(issueForum, page);
			}

			var issue = GetIssue(issuePartialUrl, issueForum, context);

			if (issue == null)
			{
				return RedirectToPageNotFound();
			}

			return GetIssueView(issueForum, issue, page);
		}

		// GET: /issues/{issueForumPartialUrl}/filter/{filter}/{status}/{priority}
		[HttpGet]
		public ActionResult Filter(string issueForumPartialUrl, string filter, string status, string priority, int? page)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext();

			if (string.IsNullOrWhiteSpace(issueForumPartialUrl))
			{
				return RedirectToAction("Issues");
			}

			var issueForum = GetIssueForum(issueForumPartialUrl, context);

			if (issueForum == null || !Authorized(context, issueForum))
			{
				return RedirectToAction("Issues");
			}

			return GetIssueForumView(issueForum, page, filter, status, priority);
		}

		private static bool Authorized(OrganizationServiceContext context, Entity entity)
		{
			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider();

			return securityProvider.TryAssert(context, entity, CrmEntityRight.Read);
		}

		private static Entity GetIssue(string issuePartialUrl, Entity issueForum, OrganizationServiceContext context)
		{
			var issue = context.CreateQuery("adx_issue")
				.FirstOrDefault(adxIssue => adxIssue.GetAttributeValue<EntityReference>("adx_issueforumid") == issueForum.ToEntityReference()
					&& adxIssue.GetAttributeValue<string>("adx_partialurl") == issuePartialUrl
					&& adxIssue.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false)
					&& adxIssue.GetAttributeValue<OptionSetValue>("statecode") != null && adxIssue.GetAttributeValue<OptionSetValue>("statecode").Value == 0);
			
			return issue;
		}

		private static Entity GetIssueForum(string issueForumPartialUrl, OrganizationServiceContext context)
		{
			var website = PortalCrmConfigurationManager.CreatePortalContext().Website;

			var issueForum = context.CreateQuery("adx_issueforum")
				.FirstOrDefault(forum => forum.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference()
					&& forum.GetAttributeValue<string>("adx_partialurl") == issueForumPartialUrl
					&& forum.GetAttributeValue<OptionSetValue>("statecode") != null && forum.GetAttributeValue<OptionSetValue>("statecode").Value == 0);
			
			return issueForum;
		}

		private ActionResult GetIssueForumView(Entity issueForum, int? page, string filter = "open", string status = "all", string priority = null)
		{
			IssuePriority issuePriority;

			var issueForumDataAdapter = new IssueForumDataAdapter(issueForum);
			
			if (status == "all" && string.Equals(filter, "open", StringComparison.InvariantCultureIgnoreCase))
			{
				issueForumDataAdapter.Status = new []
				{
					(int)IssueStatus.NewOrUnconfirmed,
					(int)IssueStatus.Confirmed,
					(int)IssueStatus.WorkaroundAvailable,
				};
			}
			else if (status == "all" && string.Equals(filter, "closed", StringComparison.InvariantCultureIgnoreCase))
			{
				issueForumDataAdapter.Status = new []
				{
					(int)IssueStatus.Resolved,
					(int)IssueStatus.WillNotFix,
					(int)IssueStatus.ByDesign,
					(int)IssueStatus.UnableToReproduce,
				};
			}
			else
			{
				var statusWithoutHyphens = status.Replace("-", string.Empty);

				IssueStatus issueStatus;
				
				if (Enum.TryParse(statusWithoutHyphens, true, out issueStatus))
				{
					issueForumDataAdapter.Status = new[] { (int)issueStatus };
				}
			}

			issueForumDataAdapter.Priority = Enum.TryParse(priority, true, out issuePriority) ? (int)issuePriority : (int?)null;

			var issueForumViewModel = new IssueForumViewModel
			{
				IssueForum = issueForumDataAdapter.Select(),
				Issues = new PaginatedList<IIssue>(page, issueForumDataAdapter.SelectIssueCount(), issueForumDataAdapter.SelectIssues)
			};

			return View("IssueForum", issueForumViewModel);
		}

		private ActionResult GetIssuesViewOrRedirectToOnlyIssueForum()
		{
			var websiteDataAdapter = new WebsiteDataAdapter();

			var issueForums = websiteDataAdapter.SelectIssueForums().ToArray();

			var issueForumCount = websiteDataAdapter.SelectIssueForumCount();

			if (issueForums.Count() == 1)
			{
				return RedirectToAction("Issues", new { issueForumPartialUrl = issueForums.First().PartialUrl });
			}

			var issuesViewModel = new IssuesViewModel
			{
				IssueForums = issueForums,
				IssueForumCount = issueForumCount
			};

			return View("Issues", issuesViewModel);
		}

		private ActionResult GetIssueView(Entity adxIssueForum, Entity adxIssue, int? page)
		{
			var issueDataAdapter = new IssueDataAdapter(adxIssue) { ChronologicalComments = true };

			var issue = issueDataAdapter.Select();

			var comments = FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback)
				? new PaginatedList<IComment>(page, issueDataAdapter.SelectCommentCount(), issueDataAdapter.SelectComments)
				: null;

			var issueViewModel = new IssueViewModel
			{
				IssueForum = new IssueForumDataAdapter(adxIssueForum).Select(),
				Issue = issue,
				Comments = new IssueCommentsViewModel { Comments = comments, Issue = issue },
				CurrentUserHasAlert = issueDataAdapter.HasAlert()
			};

			return View("Issue", issueViewModel);
		}

		private ActionResult RedirectToPageNotFound()
		{
			var context = PortalCrmConfigurationManager.CreatePortalContext();

			var page = context.ServiceContext.GetPageBySiteMarkerName(context.Website, PageNotFoundSiteMarker);

			if (page == null)
			{
				throw new Exception("Please contact your system administrator. The required site marker {0} is missing.".FormatWith(PageNotFoundSiteMarker));
			}

			var path = OrganizationServiceContextExtensions.GetUrl(context.ServiceContext, page);

			if (path == null)
			{
				throw new Exception("Please contact your System Administrator. Unable to build URL for Site Marker {0}.".FormatWith(PageNotFoundSiteMarker));
			}

			return Redirect(path);
		}
	}
}
