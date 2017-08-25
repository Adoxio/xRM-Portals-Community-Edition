/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Account.Models
{
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Net;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using Adxstudio.Xrm.AspNet.Identity;
	using Adxstudio.Xrm.AspNet.Mvc;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Web.Mvc;
	using Microsoft.AspNet.Identity;
	using Site.Areas.Account.ViewModels;
	using Site.Areas.AccountManagement;

	/// <summary>
	/// User registration handling.
	/// </summary>
	public class RegistrationManager
	{
		/// <summary>
		/// Collect errors
		/// </summary>
		public List<string> Errors;

		/// <summary>
		/// Model that handles the logic
		/// </summary>
		private LoginManager loginManager;

		/// <summary>
		/// Holds Authentication settings required for the page
		/// </summary>
		public AuthenticationSettings Settings { get; private set; }

		/// <summary>
		/// Azure AD Or External Login Enabled variable used for external login
		/// </summary>
		public bool AzureAdOrExternalLoginEnabled { get; private set; }

		/// <summary>
		/// Is External Registration Enabled
		/// </summary>
		public bool IsExternalRegistrationEnabled { get; private set; }


		/// <summary>
		/// Holds protal view context to redirect to profile page
		/// </summary>
		public ViewDataDictionary ViewData { get; set; }

		/// <summary>
		/// Identity Errors to get error descriptions
		/// </summary>
		public CrmIdentityErrorDescriber IdentityErrors { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RegistrationManager" /> class.
		/// </summary>
		/// <param name="httpContext">The context.</param>
		public RegistrationManager(HttpContextBase httpContext)
		{
			this.loginManager = new LoginManager(httpContext);
			var isLocal = httpContext.IsDebuggingEnabled && httpContext.Request.IsLocal;
			var website = httpContext.GetWebsite();
			this.Settings = website.GetAuthenticationSettings(isLocal);
			this.AzureAdOrExternalLoginEnabled = this.Settings.ExternalLoginEnabled || this.loginManager.StartupSettingsManager.AzureAdOptions != null;
			this.loginManager.IdentityErrors = this.IdentityErrors = website.GetIdentityErrors(httpContext.GetOwinContext());
			this.IsExternalRegistrationEnabled = this.loginManager.StartupSettingsManager.ExternalRegistrationEnabled;
		}

		/// <summary>
		/// On Page Init
		/// </summary>
		/// <param name="invitationCode">invitation Code</param>
		/// <returns>returns email value</returns>
		public string FindEmailByInvitationCode(string invitationCode)
		{
			if (Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss || !this.Settings.RegistrationEnabled
				|| (!this.Settings.OpenRegistrationEnabled && !this.Settings.InvitationEnabled)
				|| (this.Settings.OpenRegistrationEnabled && !this.Settings.InvitationEnabled && !string.IsNullOrWhiteSpace(invitationCode))
				|| (!this.Settings.OpenRegistrationEnabled && this.Settings.InvitationEnabled && string.IsNullOrWhiteSpace(invitationCode)))
			{
				this.loginManager.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
				this.loginManager.HttpContext.Response.ContentType = "text/plain";
				this.loginManager.HttpContext.Response.Write(ResourceManager.GetString("Not_Found_Exception"));
				this.loginManager.HttpContext.Response.End();
			}
			Task<ApplicationInvitation> invitation = this.loginManager.FindInvitationByCodeAsync(invitationCode);
			invitation.Wait();
			var contactId = this.loginManager.ToContactId(invitation.Result);
			var email = contactId != null ? contactId.Name : null;

			return email;
		}

		/// <summary>
		/// Validate and register 
		/// </summary>
		/// <param name="registerViewModel">Register values to validate</param>
		/// <param name="returnUrl">return URL</param>
		/// <param name="invitationCode">invitation code</param>
		/// <returns>redirects to respective pages on validated else loads errors</returns>
		public async Task ValidateAndRegisterUser(RegisterViewModel registerViewModel, string returnUrl, string invitationCode)
		{
			if (Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss || !this.Settings.RegistrationEnabled
				|| (!this.Settings.OpenRegistrationEnabled && !this.Settings.InvitationEnabled)
				|| (this.Settings.OpenRegistrationEnabled && !this.Settings.InvitationEnabled && !string.IsNullOrWhiteSpace(invitationCode))
				|| (!this.Settings.OpenRegistrationEnabled && this.Settings.InvitationEnabled && string.IsNullOrWhiteSpace(invitationCode)))
			{
				this.loginManager.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
				this.loginManager.HttpContext.Response.ContentType = "text/plain";
				this.loginManager.HttpContext.Response.Write(ResourceManager.GetString("Not_Found_Exception"));
				this.loginManager.HttpContext.Response.End();
			}

			if ((this.Settings.LocalLoginByEmail || this.Settings.RequireUniqueEmail) && string.IsNullOrWhiteSpace(registerViewModel.Email))
			{
				this.loginManager.AddErrors(this.loginManager.IdentityErrors.EmailRequired());
			}

			if (!string.IsNullOrWhiteSpace(registerViewModel.Email) && !this.loginManager.ValidateEmail(registerViewModel.Email))
			{
				this.loginManager.AddErrors(this.loginManager.IdentityErrors.ValidEmailRequired());
			}

			if (!this.Settings.LocalLoginByEmail && string.IsNullOrWhiteSpace(registerViewModel.Username))
			{
				this.loginManager.AddErrors(this.loginManager.IdentityErrors.UserNameRequired());
			}

			if (string.IsNullOrWhiteSpace(registerViewModel.Password))
			{
				this.loginManager.AddErrors(this.loginManager.IdentityErrors.PasswordRequired());
			}

			if (!string.Equals(registerViewModel.Password, registerViewModel.ConfirmPassword))
			{
				this.loginManager.AddErrors(this.loginManager.IdentityErrors.PasswordConfirmationFailure());
			}

#if TELERIKWEBUI

			if (registerViewModel.IsCaptchaEnabled && !registerViewModel.IsCaptchaValid)
			{
				this.loginManager.AddErrors(
					this.loginManager.IdentityErrors.CaptchaRequired(registerViewModel.CaptchaValidationMessage));
			}

#endif

			if (this.loginManager.Error.Count <= 0)
			{ 
				var invitation = await this.loginManager.FindInvitationByCodeAsync(invitationCode);

				// Is there a contact?
				var contactId = this.loginManager.ToContactId(invitation);

				if (!string.IsNullOrWhiteSpace(invitationCode) && contactId == null)
				{
					this.loginManager.AddErrors(this.loginManager.IdentityErrors.InvalidInvitationCode());
				}

				if (this.loginManager.Error.Count <= 0)
				{
					ApplicationUser user;
					IdentityResult result;

					if (contactId == null)
					{
						// Create a new user
						user = new ApplicationUser
						{
							UserName = this.Settings.LocalLoginByEmail ? registerViewModel.Email : registerViewModel.Username,
							Email = registerViewModel.Email
						};

						result = await this.loginManager.UserManager.CreateAsync(user, registerViewModel.Password);
					}
					else
					{
						// Update the existing invited user
						user = await this.loginManager.UserManager.FindByIdAsync(contactId.Id.ToString());

						if (user != null)
						{
							result = await this.loginManager.UserManager.InitializeUserAsync(
								user,
								this.Settings.LocalLoginByEmail ? registerViewModel.Email : registerViewModel.Username,
								registerViewModel.Password,
								!string.IsNullOrWhiteSpace(registerViewModel.Email) ? registerViewModel.Email : contactId.Name,
								this.Settings.TriggerLockoutOnFailedPassword);
						}
						else
						{
							// Contact does not exist or login is disabled
							result = IdentityResult.Failed(this.loginManager.IdentityErrors.InvalidInvitationCode().Description);
						}

						if (!result.Succeeded)
						{
							var urlHelper = this.GetUrlHelper();

							this.loginManager.AddErrors(result);

							this.loginManager.HttpContext.Response.Redirect(urlHelper.Action("RedeemInvitation", "Login", new { InvitationCode = invitationCode }));
						}
					}

					if (result.Succeeded)
					{
						if (invitation != null)
						{
							var redeemResult = await this.loginManager.InvitationManager.RedeemAsync(invitation, user, this.loginManager.HttpContext.Request.UserHostAddress);

							if (redeemResult.Succeeded)
							{
								var redirectTo = await this.loginManager.SignInAsync(user, returnUrl);
								this.RedirectTo(redirectTo, returnUrl);
							}
							else
							{
								this.loginManager.AddErrors(redeemResult);
							}
						}
						else
						{
							var redirectTo = await this.loginManager.SignInAsync(user, returnUrl);
							this.RedirectTo(redirectTo, returnUrl);
						}
					}
					else
					{
						this.loginManager.AddErrors(result);
					}
				}
			}

			// If we got this far, something failed, redisplay form
			this.Errors = this.loginManager.Error;
		}

		/// <summary>
		/// Redirects to appropriate page based on enum value
		/// </summary>
		/// <param name="redirectTo">Holds information to redirect page</param>
		/// <param name="returnUrl">return value</param>
		private void RedirectTo(Enums.RedirectTo redirectTo, string returnUrl)
		{
			switch (redirectTo)
			{
				case Enums.RedirectTo.Redeem:
					var urlHelper = this.GetUrlHelper();
					this.loginManager.HttpContext.Response.Redirect(urlHelper.Action("RedeemInvitation", "Login", new { ReturnUrl = returnUrl, invalid = true }));
					break;
				case Enums.RedirectTo.Profile:
					this.RedirectToProfile(returnUrl);
					break;
				case Enums.RedirectTo.Local:
					break;
			}
		}

		/// <summary>
		/// Returns a <see cref="UrlHelper"/>.
		/// </summary>
		/// <returns>The UrlHelper.</returns>
		private UrlHelper GetUrlHelper()
		{
			var requestContext = new RequestContext(this.loginManager.HttpContext, new RouteData());
			var urlHelper = new UrlHelper(requestContext, RouteTable.Routes);

			return urlHelper;
		}

		/// <summary>
		/// Redirects to profile page
		/// </summary>
		/// <param name="returnUrl">Return url to append</param>
		private void RedirectToProfile(string returnUrl)
		{
			var query = !string.IsNullOrWhiteSpace(returnUrl) ? new NameValueCollection { { "ReturnUrl", returnUrl } } : null;
			var obj = new RedirectToSiteMarkerResult("Profile", query);
			obj.ExecutePageResult(this.ViewData, this.loginManager.HttpContext);
		}
	}
}
