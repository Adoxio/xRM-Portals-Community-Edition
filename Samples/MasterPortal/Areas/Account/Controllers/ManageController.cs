/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.AspNet.Mvc;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Web.Security.AntiXss;
using Site.Areas.Account.Models;
using Site.Areas.Account.ViewModels;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web;
using Microsoft.Owin.Security.OpenIdConnect;
using Adxstudio.Xrm;

namespace Site.Areas.Account.Controllers
{
    [Authorize]
	[PortalView, UnwrapNotFoundException]
	[OutputCache(NoStore = true, Duration = 0)]
	public class ManageController : Controller
	{
		public ManageController()
		{
		}

		public ManageController(ApplicationUserManager userManager, ApplicationOrganizationManager organizationManager)
		{
			UserManager = userManager;
			OrganizationManager = organizationManager;
		}

		private ApplicationUserManager _userManager;
		public ApplicationUserManager UserManager
		{
			get
			{
				return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set
			{
				_userManager = value;
			}
		}

		private ApplicationOrganizationManager _organizationManager;
		public ApplicationOrganizationManager OrganizationManager
		{
			get
			{
				return _organizationManager ?? HttpContext.GetOwinContext().Get<ApplicationOrganizationManager>();
			}
			private set
			{
				_organizationManager = value;
			}
		}

		private ApplicationStartupSettingsManager _startupSettingsManager;
		public ApplicationStartupSettingsManager StartupSettingsManager
		{
			get
			{
				return _startupSettingsManager ?? HttpContext.GetOwinContext().Get<ApplicationStartupSettingsManager>();
			}
			private set
			{
				_startupSettingsManager = value;
			}
		}

		protected override void Initialize(RequestContext requestContext)
		{
			base.Initialize(requestContext);

			var isLocal = requestContext.HttpContext.IsDebuggingEnabled && requestContext.HttpContext.Request.IsLocal;
			var website = requestContext.HttpContext.GetWebsite();

			ViewBag.Website = website;
			ViewBag.Settings = website.GetAuthenticationSettings(isLocal);
			ViewBag.IdentityErrors = website.GetIdentityErrors(this.HttpContext.GetOwinContext());
			ViewBag.AzureAdOrExternalLoginEnabled = ViewBag.Settings.ExternalLoginEnabled || StartupSettingsManager.AzureAdOptions != null;

			var userId = User.Identity.GetUserId();
			var user = !string.IsNullOrWhiteSpace(userId) ? UserManager.FindById(userId) : null;

			ViewBag.Nav = user == null
				? new ManageNavSettings()
				: new ManageNavSettings
				{
					HasPassword = UserManager.HasPassword(user.Id),
					IsEmailConfirmed = string.IsNullOrWhiteSpace(user.Email) || user.EmailConfirmed,
					IsMobilePhoneConfirmed = string.IsNullOrWhiteSpace(user.PhoneNumber) || user.PhoneNumberConfirmed,
					IsTwoFactorEnabled = user.TwoFactorEnabled,
				};

			var contextLanguageInfo = requestContext.HttpContext.GetContextLanguageInfo();
			var region = contextLanguageInfo.IsCrmMultiLanguageEnabled ? contextLanguageInfo.ContextLanguage.Code : null;

			ViewBag.Region = region;
		}

		//
		// GET: /Manage/Index
		[HttpGet]
		public ActionResult Index(ManageMessageId? message)
		{
			return RedirectToProfile(message);
		}

		//
		// POST: /Manage/RemoveLogin
		[HttpPost]
		[ValidateAntiForgeryToken]
		[ExternalLogin]
		public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
		{
			if (Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss)
			{
				return HttpNotFound();
			}

			ManageMessageId? message;
			var userId = User.Identity.GetUserId();
			var user = await UserManager.FindByIdAsync(userId);
			var userLogins = await UserManager.GetLoginsAsync(userId);

			if (user != null && userLogins != null)
			{
				if (user.PasswordHash == null && userLogins.Count() <= 1)
				{
					return RedirectToAction("ChangeLogin", new { Message = ManageMessageId.RemoveLoginFailure });
				}
			}

			var result = await UserManager.RemoveLoginAsync(userId, new UserLoginInfo(loginProvider, providerKey));

			if (result.Succeeded)
			{
				if (user != null)
				{
					await SignInAsync(user, isPersistent: false);
				}

				message = ManageMessageId.RemoveLoginSuccess;
			}
			else
			{
				message = ManageMessageId.RemoveLoginFailure;
			}

			return RedirectToAction("ChangeLogin", new { Message = message });
		}

		//
		// GET: /Manage/ChangePhoneNumber
		[HttpGet]
		public async Task<ActionResult> ChangePhoneNumber()
		{
			if (!ViewBag.Settings.MobilePhoneEnabled)
			{
				return HttpNotFound();
			}

			var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

			if (user == null)
			{
				throw new ApplicationException("Account error.");
			}

			ViewBag.ShowRemoveButton = user.PhoneNumber != null;

			return View(new AddPhoneNumberViewModel { Number = user.PhoneNumber });
		}

		//
		// POST: /Manage/ChangePhoneNumber
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangePhoneNumber(AddPhoneNumberViewModel model)
		{
			if (!ViewBag.Settings.MobilePhoneEnabled)
			{
				return HttpNotFound();
			}

			if (!ModelState.IsValid)
			{
				return await ChangePhoneNumber();
			}

			var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

			if (user == null)
			{
				throw new ApplicationException("Account error.");
			}

			// Generate the token and send it
			var code = await UserManager.GenerateChangePhoneNumberTokenAsync(user.Id, model.Number);
			var parameters = new Dictionary<string, object> { { "Code", code }, { "PhoneNumber", model.Number } };
			await OrganizationManager.InvokeProcessAsync("adx_SendSmsConfirmationToContact", user.ContactId, parameters);

			//if (UserManager.SmsService != null)
			//{
			//	var message = new IdentityMessage
			//	{
			//		Destination = model.Number,
			//		Body = "Your security code is: " + code
			//	};
			//	await UserManager.SmsService.SendAsync(message);
			//}

			return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
		}

		//
		// POST: /Manage/RememberBrowser
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult RememberBrowser()
		{
			if (Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss)
			{
				return HttpNotFound();
			}

			if (!ViewBag.Settings.TwoFactorEnabled || !ViewBag.Settings.RememberBrowserEnabled)
			{
				return HttpNotFound();
			}

			var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(User.Identity.GetUserId());
			AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = true }, rememberBrowserIdentity);
			return RedirectToProfile(ManageMessageId.RememberBrowserSuccess);
		}

		//
		// POST: /Manage/ForgetBrowser
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ForgetBrowser()
		{
			AuthenticationManager.SignOut(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
			return RedirectToProfile(ManageMessageId.ForgetBrowserSuccess);
		}

		//
		// POST: /Manage/EnableTFA
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> EnableTFA()
		{
			if (Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss)
			{
				return HttpNotFound();
			}

			if (!ViewBag.Settings.TwoFactorEnabled)
			{
				return HttpNotFound();
			}

			var userId = User.Identity.GetUserId();
			await UserManager.SetTwoFactorEnabledAsync(userId, true);
			var user = await UserManager.FindByIdAsync(userId);
			if (user != null)
			{
				await SignInAsync(user, isPersistent: false);
			}
			return RedirectToProfile(ManageMessageId.EnableTwoFactorSuccess);
		}

		//
		// POST: /Manage/DisableTFA
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DisableTFA()
		{
			if (Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss)
			{
				return HttpNotFound();
			}

			if (!ViewBag.Settings.TwoFactorEnabled)
			{
				return HttpNotFound();
			}

			var userId = User.Identity.GetUserId();
			await UserManager.SetTwoFactorEnabledAsync(userId, false);
			var user = await UserManager.FindByIdAsync(userId);
			if (user != null)
			{
				await SignInAsync(user, isPersistent: false);
			}
			return RedirectToProfile(ManageMessageId.DisableTwoFactorSuccess);
		}

		//
		// GET: /Manage/VerifyPhoneNumber
		[HttpGet]
		public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
		{
			if (string.IsNullOrWhiteSpace(phoneNumber))
			{
				return HttpNotFound();
			}

			if (ViewBag.Settings.IsDemoMode)
			{
				// This code allows you exercise the flow without actually sending codes
				// For production use please register a SMS provider in IdentityConfig and generate a code here.
				var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
				ViewBag.DemoModeCode = code;
			}

			return View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
		}

		//
		// POST: /Manage/VerifyPhoneNumber
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
		{
			if (ModelState.IsValid)
			{
				var userId = User.Identity.GetUserId();
				var result = await UserManager.ChangePhoneNumberAsync(userId, model.PhoneNumber, model.Code);
				if (result.Succeeded)
				{
					var user = await UserManager.FindByIdAsync(userId);
					if (user != null)
					{
						await SignInAsync(user, isPersistent: false);
					}
					return RedirectToProfile(ManageMessageId.ChangePhoneNumberSuccess);
				}
			}

			// If we got this far, something failed, redisplay form
			ViewBag.Message = ManageMessageId.ChangePhoneNumberFailure.ToString();
			return await VerifyPhoneNumber(model.PhoneNumber);
		}

		//
		// GET: /Manage/RemovePhoneNumber
		[HttpGet]
		public async Task<ActionResult> RemovePhoneNumber()
		{
			var userId = User.Identity.GetUserId();
			var result = await UserManager.SetPhoneNumberAsync(userId, null);
			if (!result.Succeeded)
			{
				throw new ApplicationException("Account error.");
			}
			var user = await UserManager.FindByIdAsync(userId);
			if (user != null)
			{
				await SignInAsync(user, isPersistent: false);
			}
			return RedirectToProfile(ManageMessageId.RemovePhoneNumberSuccess);
		}

		//
		// GET: /Manage/ChangePassword
		[HttpGet]
		[LocalLogin]
		public async Task<ActionResult> ChangePassword()
		{
			var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

			if (user == null)
			{
				throw new ApplicationException("Account error.");
			}

            var snippets = new SnippetDataAdapter(new PortalConfigurationDataAdapterDependencies());

            var breadcrumbSnippetName = snippets.Select("Profile/SecurityNav/ChangePassword");
            ViewBag.PageTitle = breadcrumbSnippetName != null ? breadcrumbSnippetName.Value.Value : ResourceManager.GetString("Change_Password_DefaultText");

            return View(new ChangePasswordViewModel { Username = user.UserName, Email = user.Email });
		}

		//
		// POST: /Manage/ChangePassword
		[HttpPost]
		[ValidateAntiForgeryToken]
		[LocalLogin]
		public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (!string.Equals(model.NewPassword, model.ConfirmPassword))
			{
				ModelState.AddModelError("Password", ViewBag.IdentityErrors.PasswordConfirmationFailure().Description);
			}

			if (string.Equals(model.OldPassword, model.NewPassword))
			{
				ModelState.AddModelError("IncorrectPassword", ViewBag.IdentityErrors.NewPasswordConfirmationFailure().Description);
			}

			if (ModelState.IsValid)
			{
				var userId = User.Identity.GetUserId();
				var result = await UserManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
				if (result.Succeeded)
				{
					var user = await UserManager.FindByIdAsync(userId);
					if (user != null)
					{
						await SignInAsync(user, isPersistent: false);
					}
					return RedirectToProfile(ManageMessageId.ChangePasswordSuccess);
				}
				AddErrors(result);
			}

			return View(model);
		}

		//
		// GET: /Manage/SetPassword
		[HttpGet]
		[LocalLogin]
		public async Task<ActionResult> SetPassword()
		{
			var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

			if (user == null)
			{
				throw new ApplicationException("Account error.");
			}

			ViewBag.HasEmail = !string.IsNullOrWhiteSpace(user.Email);

			return View(new SetPasswordViewModel { Email = user.Email });
		}

		//
		// POST: /Manage/SetPassword
		[HttpPost]
		[ValidateAntiForgeryToken]
		[LocalLogin]
		public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
		{
			if (!ViewBag.Settings.LocalLoginByEmail && string.IsNullOrWhiteSpace(model.Username))
			{
				ModelState.AddModelError("Username", ViewBag.IdentityErrors.UserNameRequired().Description);
			}

			if (!string.Equals(model.NewPassword, model.ConfirmPassword))
			{
				ModelState.AddModelError("Password", ViewBag.IdentityErrors.PasswordConfirmationFailure().Description);
			}

			if (ModelState.IsValid)
			{
				var userId = User.Identity.GetUserId();

				var result = ViewBag.Settings.LocalLoginByEmail
					? await UserManager.AddPasswordAsync(userId, model.NewPassword)
					: await UserManager.AddUsernameAndPasswordAsync(userId, model.Username, model.NewPassword);

				if (result.Succeeded)
				{
					var user = await UserManager.FindByIdAsync(userId);
					if (user != null)
					{
						await SignInAsync(user, isPersistent: false);
					}
					return RedirectToProfile(ManageMessageId.SetPasswordSuccess);
				}
				AddErrors(result);
			}

			// If we got this far, something failed, redisplay form
			return await SetPassword();
		}

		//
		// POST: /Manage/LinkLogin
		[HttpPost]
		[ValidateAntiForgeryToken]
		[ExternalLogin]
		public ActionResult LinkLogin(string provider)
		{
			if (Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss)
			{
                return HttpNotFound();
			}

			// Request a redirect to the external login provider to link a login for the current user
			return new LoginController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
		}

		//
		// GET: /Manage/LinkLoginCallback
		[ExternalLogin]
		public async Task<ActionResult> LinkLoginCallback()
		{
			if (Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss)
			{
                return HttpNotFound();
			}

			var userId = User.Identity.GetUserId();
			var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, userId);
			if (loginInfo == null)
			{
				return RedirectToAction("ChangeLogin", new { Message = ManageMessageId.LinkLoginFailure });
			}
			var result = await UserManager.AddLoginAsync(userId, loginInfo.Login);
			return RedirectToAction("ChangeLogin", new { Message = result.Succeeded ? ManageMessageId.LinkLoginSuccess : ManageMessageId.LinkLoginFailure });
		}

		//
		// GET: /Manage/ConfirmEmailRequest
		[HttpGet]
		public async Task<ActionResult> ConfirmEmailRequest()
		{

			if (!ViewBag.Settings.EmailConfirmationEnabled)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application,
					"ManageController.ConfirmEmailRequest: EmailConfirmationEnabled not enabled");
				return HttpNotFound();
			}

			var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

			if (user == null)
			{
				throw new ApplicationException("Account error.");
			}

			var code = await GetEmailConfirmationCode(user);
			var callbackUrl = Url.Action("ConfirmEmail", "Manage", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
			var parameters = new Dictionary<string, object> { { "UserId", user.Id }, { "Code", code }, { "UrlCode", AntiXssEncoder.UrlEncode(code) }, { "CallbackUrl", callbackUrl } };
			try
			{
				await OrganizationManager.InvokeProcessAsync("adx_SendEmailConfirmationToContact", user.ContactId, parameters);
				//await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
			}
			catch (Exception e)
			{
				Adxstudio.Xrm.ADXTrace.Instance.TraceError(Adxstudio.Xrm.TraceCategory.Application, e.ToString());
			}

			if (ViewBag.Settings.IsDemoMode)
			{
				ViewBag.DemoModeLink = callbackUrl;
			}

			return View(new RegisterViewModel { Email = user.Email });
		}
		
		/// <summary>
		/// Returns the email confirmation code. This put safety check around user and code.
		/// </summary>
		/// <param name="user">Application user</param>
		/// <returns>returns email confirmation code</returns>
		private async Task<string> GetEmailConfirmationCode(ApplicationUser user)
		{
			if (user != null)
			{
				var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

				if (string.IsNullOrEmpty(code))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application,
						$"ManageController.GetEmailConfirmationCode: userId == {user.Id} Failed to get the code from GenerateEmailConfirmationTokenAsync.");
				}
				return code;
			}
			
			//user should not be null so this must be an error.
			ADXTrace.Instance.TraceError(TraceCategory.Application, "ManageController.GetEmailConfirmationCode: user is null.");
			return null;
		}

		//
		// GET: /Manage/ConfirmEmail
		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult> ConfirmEmail(string userId, string code)
		{
			if (!ViewBag.Settings.EmailConfirmationEnabled)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application,
					"ManageController.ConfirmEmail: EmailConfirmationEnabled not enabled");
				return HttpNotFound();
			}

			if (userId == null || code == null)
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application,
					$"ManageController.ConfirmEmail: userId == {userId} and Passed code == {code}");
				return HttpNotFound();
			}

			var result = await UserManager.ConfirmEmailAsync(userId, code);
			var message = result.Succeeded ? ManageMessageId.ConfirmEmailSuccess : ManageMessageId.ConfirmEmailFailure;

			if (User.Identity.IsAuthenticated && userId == User.Identity.GetUserId())
			{
				return RedirectToProfile(message);
			}

			return RedirectToAction("ConfirmEmail", "Login", new { Message = message });
		}

		//
		// GET: /Manage/ChangeEmail
		[HttpGet]
		public async Task<ActionResult> ChangeEmail()
		{
			if (Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss)
			{
                return HttpNotFound();
			}

			var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

			if (user == null)
			{
				throw new ApplicationException("Account error.");
			}

            var snippets = new SnippetDataAdapter(new PortalConfigurationDataAdapterDependencies());

            var breadcrumbSnippetName = snippets.Select("Profile/SecurityNav/ChangeEmail");
            ViewBag.PageTitle = breadcrumbSnippetName != null ? breadcrumbSnippetName.Value.Value : ResourceManager.GetString("Change_Email_Defaulttext");

            return View(new ChangeEmailViewModel { Email = user.Email });
		}

		//
		// POST: /Manage/ChangeEmail
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangeEmail(ChangeEmailViewModel model)
		{
			if (Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss)
			{
                return HttpNotFound();
			}

			if (ModelState.IsValid)
			{
				var userId = User.Identity.GetUserId();

				var result = ViewBag.Settings.LocalLoginByEmail
					? await UserManager.SetUsernameAndEmailAsync(userId, model.Email, model.Email)
					: await UserManager.SetEmailAsync(userId, model.Email);

				if (result.Succeeded)
				{
					var user = await UserManager.FindByIdAsync(userId);
					if (user != null)
					{
						await SignInAsync(user, isPersistent: false);
					}

					return ViewBag.Settings.EmailConfirmationEnabled
						? RedirectToAction("ConfirmEmailRequest", new { Message = ManageMessageId.ChangeEmailSuccess })
						: RedirectToProfile(ManageMessageId.ChangeEmailSuccess);
				}

				AddErrors(result);
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		//
		// GET: /Manage/ChangeTwoFactor
		[HttpGet]
		public async Task<ActionResult> ChangeTwoFactor()
		{
			if (Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss)
			{
                return HttpNotFound();
			}

			if (!ViewBag.Settings.TwoFactorEnabled)
			{
                return HttpNotFound();
			}

			var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

			if (user == null)
			{
				throw new ApplicationException("Account error.");
			}

			ViewBag.TwoFactorBrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(user.Id);
			ViewBag.HasEmail = !string.IsNullOrWhiteSpace(user.Email);
			ViewBag.IsEmailConfirmed = user.EmailConfirmed;
			ViewBag.IsTwoFactorEnabled = user.TwoFactorEnabled;

			return View();
		}

		//
		// GET: /Manage/ChangeLogin
		[HttpGet]
		[ExternalLogin]
		public async Task<ActionResult> ChangeLogin(CancellationToken cancellationToken)
		{
			if (Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss)
			{
				return HttpNotFound();
			}

			var userId = User.Identity.GetUserId();
			var user = await UserManager.FindByIdAsync(userId);

			if (user == null)
			{
				throw new ApplicationException("Account error.");
			}

			var id = 0;
			var userLogins = await UserManager.GetLoginsAsync(userId);
			var tasks = AuthenticationManager.GetExternalAuthenticationTypes().Select(p => ToExternalAuthenticationTypes(id++, userLogins, p, cancellationToken));
			var externalAuthenticationTypes = await Task.WhenAll(tasks);

			var logins = externalAuthenticationTypes
				.OrderBy(pair => pair.Provider.Caption)
				.ToList();

			ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count() > 1;

			var snippets = new SnippetDataAdapter(new PortalConfigurationDataAdapterDependencies());

			var breadcrumbSnippetName = snippets.Select("Profile/SecurityNav/ChangeLogin");
			ViewBag.PageTitle = breadcrumbSnippetName != null ? breadcrumbSnippetName.Value.Value : ResourceManager.GetString("Manage_External_Authentication");

			return View(new ChangeLoginViewModel { Logins = logins });
		}

		private async Task<LoginPair> ToExternalAuthenticationTypes(int id, IList<UserLoginInfo> userLogins, AuthenticationDescription p, CancellationToken cancellationToken)
		{
			var loginProvider = await ToLoginProvider(p.AuthenticationType, cancellationToken);
			var user = userLogins.SingleOrDefault(u => u.LoginProvider == loginProvider);
			var authenticationType = user == null ? p.AuthenticationType : loginProvider;
			var properties = new Dictionary<string, object>(p.Properties, StringComparer.Ordinal);
			var provider = new AuthenticationDescription(properties) { AuthenticationType = authenticationType };

			return new LoginPair
			{
				Id = id,
				Provider = provider,
				User = user
			};
		}

		private async Task<string> ToLoginProvider(string authenticationType, CancellationToken cancellationToken)
		{
			if (StartupSettingsManager.AzureAdOptions == null
				|| !string.Equals(StartupSettingsManager.AzureAdOptions.AuthenticationType, authenticationType, StringComparison.OrdinalIgnoreCase))
			{
				return authenticationType;
			}

			return await ToIssuer(StartupSettingsManager.AzureAdOptions, cancellationToken);
		}

		private static async Task<string> ToIssuer(OpenIdConnectAuthenticationOptions options, CancellationToken cancellationToken)
		{
			var configuration = await options.ConfigurationManager.GetConfigurationAsync(cancellationToken);
			return configuration.Issuer;
		}

		#region Helpers

		// Used for XSRF protection when adding external logins
		private const string XsrfKey = "XsrfId";

		private IAuthenticationManager AuthenticationManager
		{
			get
			{
				return HttpContext.GetOwinContext().Authentication;
			}
		}

		private async Task SignInAsync(ApplicationUser user, bool isPersistent)
		{
			AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
			AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(UserManager));
		}

		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError("", error);
			}
		}

		public enum ManageMessageId
		{
			SetPasswordSuccess,
			ChangePasswordSuccess,

			ChangeEmailSuccess,

			ConfirmEmailSuccess,
			ConfirmEmailFailure,

			ChangePhoneNumberSuccess,
			ChangePhoneNumberFailure,
			RemovePhoneNumberSuccess,

			ForgetBrowserSuccess,
			RememberBrowserSuccess,

			DisableTwoFactorSuccess,
			EnableTwoFactorSuccess,

			RemoveLoginSuccess,
			RemoveLoginFailure,
			LinkLoginSuccess,
			LinkLoginFailure,
		}

		private static ActionResult RedirectToProfile(ManageMessageId? message)
		{
			var query = message != null ? new NameValueCollection { { "Message", message.Value.ToString() } } : null;

			return new RedirectToSiteMarkerResult("Profile", query);
		}

		#endregion
	}
}
