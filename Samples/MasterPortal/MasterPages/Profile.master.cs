/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.AspNet.Mvc;
using Adxstudio.Xrm.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Site.Areas.Account.Models;
using Site.Areas.Account.ViewModels;

namespace Site.MasterPages
{
	public partial class Profile : PortalMasterPage
	{
		public ApplicationUserManager UserManager
		{
			get { return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
		}

		public ApplicationWebsiteManager WebsiteManager
		{
			get { return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationWebsiteManager>(); }
		}

		public ApplicationStartupSettingsManager StartupSettingsManager
		{
			get { return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationStartupSettingsManager>(); }
		}

		private ViewDataDictionary _viewData;

		public ViewDataDictionary ViewData
		{
			get
			{
				if (_viewData == null)
				{
					var website = Context.GetWebsite();
					var settings = website.GetAuthenticationSettings();
					var azureAdOrExternalLoginEnabled = settings.ExternalLoginEnabled || StartupSettingsManager.AzureAdOptions != null;
					var contextLanguageInfo = this.Context.GetContextLanguageInfo();
					var region = contextLanguageInfo.IsCrmMultiLanguageEnabled ? contextLanguageInfo.ContextLanguage.Code : null;

					var user = HttpContext.Current.User.Identity.GetUserId() != null ? UserManager.FindById(HttpContext.Current.User.Identity.GetUserId()) : null;

					if (user == null)
					{
						Adxstudio.Xrm.ADXTrace.Instance.TraceWarning(Adxstudio.Xrm.TraceCategory.Application, "Unable to retrieve current user. Using default profile nav settings.");
					}

					var nav = user == null
						? new ManageNavSettings()
						: new ManageNavSettings
						{
							HasPassword = UserManager.HasPassword(HttpContext.Current.User.Identity.GetUserId()),
							IsEmailConfirmed = string.IsNullOrWhiteSpace(user.Email) || user.EmailConfirmed,
							IsMobilePhoneConfirmed = string.IsNullOrWhiteSpace(user.PhoneNumber) || user.PhoneNumberConfirmed,
							IsTwoFactorEnabled = user.TwoFactorEnabled,
						};

					_viewData = new ViewDataDictionary(Html.ViewData) { { "Region", region }, { "Settings", settings }, { "Nav", nav }, { "AzureAdOrExternalLoginEnabled", azureAdOrExternalLoginEnabled } };
				}

				return _viewData;
			}
		}

		public bool EmailConfirmationEnabled
		{
			get
			{
				if (ViewData == null) return true;
				var settings = ViewData["Settings"] as AuthenticationSettings;
				return settings != null && settings.EmailConfirmationEnabled;
			}
		}

		public bool IsEmailConfirmed
		{
			get
			{
				if (ViewData == null) return false;
				var nav = ViewData["Nav"] as ManageNavSettings;
				return nav != null && nav.IsEmailConfirmed;
			}
		}

		public string Region
		{
			get
			{
				if (ViewData == null) return null;
				var region = ViewData["Region"] as string;
				return region;
			}
		}

		protected void Page_Load(object sender, EventArgs e) { }
	}
}
