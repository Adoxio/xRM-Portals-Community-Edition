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
using Adxstudio.Xrm.Search;
using Adxstudio.Xrm.Web;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.SharePoint.Client.Utilities;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Site.Areas.Issues.ViewModels;
using Adxstudio.Xrm.AspNet.Cms;

namespace Site.Areas.Issues.Controllers
{
	[PortalView, PortalSecurity]
	public class IssueController : Controller
	{
		[HttpGet]
		public ActionResult Index()
		{
			return RedirectToAction("Issues", "Issues");
		}

		// AJAX: /issue/AlertCreate/{id}
		[HttpPost, ValidateInput(false), AjaxValidateAntiForgeryToken]
		public ActionResult AlertCreate(Guid id)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext();

			var issue = context.CreateQuery("adx_issue").FirstOrDefault(adxIssue => adxIssue.GetAttributeValue<Guid>("adx_issueid") == id);

			if (issue == null || !Authorized(context, issue))
			{
				return new EmptyResult();
			}

			var issueDataAdapter = new IssueDataAdapter(issue);
			var user = PortalCrmConfigurationManager.CreatePortalContext().User;

			if (user == null)
			{
				return new EmptyResult();
			}

			issueDataAdapter.CreateAlert(user.ToEntityReference());

			var issueViewModel = new IssueViewModel
			{
				Issue = issueDataAdapter.Select(),
				CurrentUserHasAlert = issueDataAdapter.HasAlert()
			};

			return PartialView("Tracking", issueViewModel);
		}

		// AJAX: /issue/AlertRemove/{id}
		[HttpPost, ValidateInput(false), AjaxValidateAntiForgeryToken]
		public ActionResult AlertRemove(Guid id)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext();

			var issue = context.CreateQuery("adx_issue").FirstOrDefault(adxIssue => adxIssue.GetAttributeValue<Guid>("adx_issueid") == id);

			if (issue == null || !Authorized(context, issue))
			{
				return new EmptyResult();
			}

			var issueDataAdapter = new IssueDataAdapter(issue);
			var user = PortalCrmConfigurationManager.CreatePortalContext().User;

			if (user == null)
			{
				return new EmptyResult();
			}

			issueDataAdapter.DeleteAlert(user.ToEntityReference());

			var issueViewModel = new IssueViewModel
			{
				Issue = issueDataAdapter.Select(),
				CurrentUserHasAlert = issueDataAdapter.HasAlert()
			};

			return PartialView("Tracking", issueViewModel);
		}

		// AJAX: /issue/Create/{id}
		[HttpPost, ValidateInput(false), ValidateAntiForgeryToken]
		public ActionResult Create(Guid id, string title, string authorName, string authorEmail, string copy, bool track)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext();

			var issueForum = context.CreateQuery("adx_issueforum").FirstOrDefault(issueforum => issueforum.GetAttributeValue<Guid>("adx_issueforumid") == id);

			if (issueForum == null || !Authorized(context, issueForum))
			{
				return new EmptyResult();
			}

			var issueForumDataAdapter = new IssueForumDataAdapter(issueForum);

			var sanitizedCopy = SafeHtml.SafeHtmSanitizer.GetSafeHtml(copy ?? string.Empty);

			TryAddIssue(issueForumDataAdapter, title, authorName, authorEmail, sanitizedCopy, track);

			return PartialView("CreateIssue", issueForumDataAdapter.Select());
		}

		// AJAX: /issue/CommentCreate/{id}
		[HttpPost, ValidateInput(false), ValidateAntiForgeryToken]
		public ActionResult CommentCreate(Guid id, string authorName, string authorEmail, string copy)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext();

			var issue = context.CreateQuery("adx_issue").FirstOrDefault(adxIssue => adxIssue.GetAttributeValue<Guid>("adx_issueid") == id);

			if (issue == null || !Authorized(context, issue))
			{
				return new EmptyResult();
			}

			var issueDataAdapter = new IssueDataAdapter(issue) { ChronologicalComments = true };

			var sanitizedCopy = SafeHtml.SafeHtmSanitizer.GetSafeHtml(copy ?? string.Empty);

			TryAddComment(issueDataAdapter, authorName, authorEmail, sanitizedCopy);

			var comments = FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback)
				? new PaginatedList<IComment>(PaginatedList.Page.Last, issueDataAdapter.SelectCommentCount(), issueDataAdapter.SelectComments)
				: null;

			var commentsViewModel = new IssueCommentsViewModel
			{
				Issue = issueDataAdapter.Select(),
				Comments = comments
			};

			return PartialView("Comments", commentsViewModel);
		}

		// GET: /issues/search
		[HttpGet]
		public ActionResult Search(string q, int? page)
		{
			if (string.IsNullOrWhiteSpace(q))
			{
				return new EmptyResult();
			}

			var pageNumber = page.GetValueOrDefault(1);

			var searchProvider = SearchManager.Provider;

			var contextLanguage = this.HttpContext.GetContextLanguageInfo();

			var query = new CrmEntityQuery(q, pageNumber, PaginatedList.PageSize, new[] { "adx_issue" }, contextLanguage.ContextLanguage, contextLanguage.IsCrmMultiLanguageEnabled);

			ICrmEntityIndexSearcher searcher;
			
			try
			{
				searcher = searchProvider.GetIndexSearcher();
			}
			catch (IndexNotFoundException)
			{
				searchProvider.GetIndexBuilder().BuildIndex();
				
				searcher = searchProvider.GetIndexSearcher();
			}
			
			var results = searcher.Search(query);

			searcher.Dispose();

			return View("SearchResults", results);
		}

		private static bool Authorized(OrganizationServiceContext context, Entity entity)
		{
			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider();

			return securityProvider.TryAssert(context, entity, CrmEntityRight.Read);
		}

		private bool TryAddComment(IssueDataAdapter issueDataAdapter, string authorName, string authorEmail, string content)
		{
			if (!Request.IsAuthenticated)
			{
				if (string.IsNullOrWhiteSpace(authorName))
				{
					ModelState.AddModelError("authorName", ResourceManager.GetString("Name_Required_Error"));
				}

				if (string.IsNullOrWhiteSpace(authorEmail))
				{
					ModelState.AddModelError("authorEmail", ResourceManager.GetString("Email_Required_Error"));
				}
			}

			if (string.IsNullOrWhiteSpace(content))
			{
				ModelState.AddModelError("content", ResourceManager.GetString("Comment_Required_Error"));
			}

			if (!ModelState.IsValid)
			{
				return false;
			}

			issueDataAdapter.CreateComment(content, authorName, authorEmail);

			return true;
		}

		private bool TryAddIssue(IssueForumDataAdapter issueForumDataAdapter, string title, string authorName, string authorEmail, string copy, bool track)
		{
			if (!Request.IsAuthenticated)
			{
				if (string.IsNullOrWhiteSpace(authorName))
				{
					ModelState.AddModelError("authorName", ResourceManager.GetString("Name_Required_Error"));
				}

				if (string.IsNullOrWhiteSpace(authorEmail))
				{
					ModelState.AddModelError("authorEmail", ResourceManager.GetString("Email_Required_Error"));
				}
			}

			if (string.IsNullOrWhiteSpace(title))
			{
				ModelState.AddModelError("title", ResourceManager.GetString("Issue_Required_Error"));
			}

			if (!ModelState.IsValid)
			{
				return false;
			}

			issueForumDataAdapter.CreateIssue(title, copy, track, authorName, authorEmail);

			return true;
		}
	}
}
