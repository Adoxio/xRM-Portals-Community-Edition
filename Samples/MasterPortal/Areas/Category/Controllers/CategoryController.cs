/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;
using Adxstudio.Xrm;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.ContentAccess;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Providers;
using Site.Areas.Category.ViewModels;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Category;
using Adxstudio.Xrm.Services;
using Microsoft.Xrm.Sdk.Query;

namespace Site.Areas.Category.Controllers
{
	[PortalView, PortalSecurity]
	public class CategoryController : Controller
	{
		/// <summary>
		/// property for contentaccessprovider
		/// </summary>
		public ContentAccessProvider contentAccessProvider { get; set; }

		/// <summary>
		/// Default contstructor to initialize property
		/// </summary>
		public CategoryController() {
			this.contentAccessProvider = new ProductAccessProvider();
		}
		
		/// <summary>
		/// Parameterized constructor for apply product filtering
		/// </summary>
		/// <param name="_contentAccessProvider"></param>
		public CategoryController(ContentAccessProvider _contentAccessProvider)
		{
			this.contentAccessProvider = _contentAccessProvider;
		}
		[HttpGet]
		public ActionResult Index(string number)
		{
			OrganizationServiceContext serviceContext = PortalCrmConfigurationManager.CreateServiceContext();
			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider();
			var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider().GetDependency<IEntityUrlProvider>();

			return this.GetIndexView(number, serviceContext, securityProvider, urlProvider);
		}

		[HttpGet]
		public ActionResult GetIndexView(string number, OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, IEntityUrlProvider urlProvider)
		{
			var category = serviceContext.RetrieveSingle("category",
				FetchAttribute.All,
				new Condition("categorynumber", ConditionOperator.Equal, number));

			if (category == null)
			{
				return RedirectToPageNotFound();
			}

			if (!securityProvider.TryAssert(serviceContext, category, CrmEntityRight.Read)) 
			{
				return RedirectToAccessDeniedPage();
			}
			var categoryDataAdapter = new CategoryDataAdapter(category);

			// Retrieve related articles from knowledgearticlescategories
			var relatedArticles = categoryDataAdapter.SelectRelatedArticles();

			// Retrieve Child Categories
			var childCategories = categoryDataAdapter.SelectChildCategories();
			var categoryViewModel = new CategoryViewModel
			{
				RelatedArticles = relatedArticles,
				ChildCategories = childCategories,
				Number = category.GetAttributeValue<string>("categorynumber"),
				Title = category.GetAttributeValue<string>("title")

			};

			return View(categoryViewModel);
		}

		private ActionResult RedirectToPageNotFound()
		{
			return RedirectToPageBySiteMarkerName("Page Not Found");
		}

		private ActionResult RedirectToAccessDeniedPage()
		{
			return RedirectToPageBySiteMarkerName("Access Denied");
		}

		private ActionResult RedirectToPageBySiteMarkerName(string name)
		{
			var context = PortalCrmConfigurationManager.CreatePortalContext();

			var page = context.ServiceContext.GetPageBySiteMarkerName(context.Website, name);

			if (page == null)
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, "User Config issue:Page Not Found by website and page name: GetPageBySiteMarkerName");
				return HttpNotFound();
			}

			var path = context.ServiceContext.GetUrl(page);

			if (path == null)
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, "User Config issue: path Not Found for the context.ServiceContext.GetUrl(page)");
				return HttpNotFound();
			}

			return Redirect(path);
		}
	}
}
