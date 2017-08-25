/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Adxstudio.Xrm.Marketing;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Portal.Configuration;

namespace Site.Areas.Marketing.Controllers
{
	public class MarketingController : Controller
	{
		private readonly Lazy<IMarketingDataAdapter> _dataAdapter = new Lazy<IMarketingDataAdapter>(GetMarketingDataAdapter, LazyThreadSafetyMode.None);

		private IMarketingDataAdapter DataAdapter
		{
			get { return _dataAdapter.Value; }
		}

		[HttpGet]
		public ActionResult ManageSubscriptions(string encodedEmail, string signature)
		{
			var marketingLists = DataAdapter.GetMarketingLists(encodedEmail, signature);

			return ToRazor("ManageSubscriptions", marketingLists, "Manage Subscriptions");
		}

		[HttpPost]
        [ValidateAntiForgeryToken]
		public ActionResult ManageSubscriptions(string encodedEmail, string signature, FormCollection form)
		{
			IEnumerable<IMarketingList> unsubscribedLists;
			if (form.GetValue("fromList") != null)
			{
				var listsValue = form.GetValue("lists");
				if (listsValue != null)
				{
					var lists = listsValue.AttemptedValue.Split(',');
					unsubscribedLists = DataAdapter.Unsubscribe(encodedEmail, lists, signature);
				}
				else
				{
					unsubscribedLists = new List<IMarketingList>();
				}
			}
			else
			{
				unsubscribedLists = DataAdapter.Unsubscribe(encodedEmail, signature);
			}

			AddLinkRoute(encodedEmail, signature);
			return ToRazor("Success", unsubscribedLists, "Unsubscription Success");
		}

		[HttpGet]
		public ActionResult Unsubscribe(string encodedEmail, string encodedList, string signature)
		{
			var unsubscribedLists = DataAdapter.Unsubscribe(encodedEmail, encodedList, signature);
			var newSignature = DataAdapter.ConstructSignature(encodedEmail);
			AddLinkRoute(encodedEmail, newSignature);
			return ToRazor("Success", unsubscribedLists, "Unsubscription Success");
		}

		private void AddLinkRoute(string encodedEmail, string signature)
		{
			ViewData["LinkRoute"] = new
			{
				encodedEmail,
				signature
			};
		}

		private PortalViewContext PortalViewContext()
		{
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var portalViewContext = new PortalViewContext(dataAdapterDependencies, requestContext: Request.RequestContext);
			return portalViewContext;
		}

		private ViewResult ToRazor(string viewName, object model, string title)
		{
			ViewData.Model = model;
			ViewBag.Title = title;
			ViewBag._ViewName = viewName;
			
			ViewData[PortalExtensions.PortalViewContextKey] = PortalViewContext();

			return new ViewResult
			{
				ViewName = "_RazorLayout",
				ViewData = ViewData
			};
		}
		
		private static IMarketingDataAdapter GetMarketingDataAdapter()
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var dependencies = new PortalContextDataAdapterDependencies(portalContext);
			return new MarketingDataAdapter(dependencies);
		}
	}
}
