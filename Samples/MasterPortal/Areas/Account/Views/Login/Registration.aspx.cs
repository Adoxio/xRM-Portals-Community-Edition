/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Account.Views.Login
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;
	using Microsoft.Owin.Security;
	using Site.Areas.Account.Models;
	using ViewModels;
	using Adxstudio.Xrm.Web.UI.WebControls;

	/// <summary>
	/// Register page with captcha validation
	/// </summary>
	public partial class Registration : Site.Pages.PortalPage
	{

		/// <summary>
		/// Validation summary group name
		/// </summary>
		private const string ValidatorValidationGroup = "loginValidationSummary";

#if TELERIKWEBUI

		/// <summary>
		/// Captcha control cast as IValidator
		/// </summary>
		private IValidator captchaValidator;

		/// <summary>
		/// True if site setting set to true otherwise false.
		/// </summary>
		private bool IsCaptchaEnabled
		{
			get { return this.RegistrationManager.Settings.IsCaptchaEnabledForRegistration; }

		}


#endif

		/// <summary>
		/// Invitation code
		/// </summary>
		public string InvitationCode { get; private set; }

		/// <summary>
		/// Return Url
		/// </summary>
		public string ReturnUrl { get; private set; }

		/// <summary>
		/// Registration manager
		/// </summary>
		public RegistrationManager RegistrationManager { get; private set; }

		/// <summary>
		/// Page Init
		/// </summary>
		/// <param name="sender">Sender of the Page</param>
		/// <param name="e">Event arguments</param>
		protected void Page_Init(object sender, EventArgs e)
		{
			this.RegistrationManager = new RegistrationManager(new HttpContextWrapper(this.Context));
			var model = this.GetExternalAuthenticationTypes();
			this.AddExternalLoginControls(model);

#if TELERIKWEBUI

			this.AddCaptchaControl();

#endif

		}

#if TELERIKWEBUI

		/// <summary>
		/// Add captcha control if enabled
		/// </summary>
		private void AddCaptchaControl()
		{
			if (this.IsCaptchaEnabled)
			{
				this.captchaValidator = RadCaptcha.RenderCaptcha(this.CaptchaControlContainer, "captcha", string.Empty,
					renderScript: true);
			}
		}

#endif

		/// <summary>
		/// Add button controls dynamically for External Login
		/// </summary>
		/// <param name="model">Authentication description</param>
		private void AddExternalLoginControls(IEnumerable<AuthenticationDescription> model)
		{
			if (model != null)
			{
				foreach (var provider in model)
				{
					HtmlButton externalLogin = new HtmlButton();
					externalLogin.Attributes["name"] = "provider";
					externalLogin.Attributes["type"] = "submit";
					externalLogin.Attributes["class"] = "btn btn-primary btn-line";
					externalLogin.Attributes["title"] = provider.Caption;
					externalLogin.Attributes["value"] = provider.AuthenticationType;
					externalLogin.InnerText = provider.Caption;
					externalLogin.ServerClick += new System.EventHandler(this.ExternalLogin_Click);
					ExternalLoginButtons.Controls.Add(externalLogin);

					Literal lbl = new Literal();
					lbl.Text = "&nbsp;";
					ExternalLoginButtons.Controls.Add(lbl);
				}
			}
		}

		/// <summary>
		/// On Click of External Login
		/// </summary>
		/// <param name="sender">Sender of the button</param>
		/// <param name="e">Event Arguments</param>
		private void ExternalLogin_Click(object sender, EventArgs e)
		{
			var requestContext = new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData());
			var urlHelper = new UrlHelper(requestContext, RouteTable.Routes);
			HtmlButton externalLogin = (HtmlButton)sender;
			var provider = externalLogin.Attributes["value"];
			Response.Redirect(urlHelper.Action("ExternalLogin", "Login", new { area = "Account", ReturnUrl = this.ReturnUrl, InvitationCode = this.InvitationCode, Provider = provider }), true);
		}

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender">Sender of the Page</param>
		/// <param name="e">Event arguments</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			this.ReturnUrl = !string.IsNullOrEmpty(Request.QueryString["returnUrl"]) ? Request.QueryString["returnUrl"] : string.Empty;
			this.InvitationCode = !string.IsNullOrEmpty(Request.QueryString["invitationCode"]) ? HttpUtility.HtmlDecode(Request.QueryString["invitationCode"]).Trim() : string.Empty;

			if (!Page.IsPostBack)
			{
				EmailTextBox.Text = this.RegistrationManager.FindEmailByInvitationCode(this.InvitationCode);
			}

			this.ShowControls();
			this.BindRegisterViewData();
		}

		/// <summary>
		/// Binding html viewdata to use across the page
		/// </summary>
		private void BindRegisterViewData()
		{
			var settings = this.RegistrationManager.Settings;

			if (settings.RegistrationEnabled && (settings.OpenRegistrationEnabled || settings.InvitationEnabled))
			{
				this.Html.ViewData["external"] = this.GetExternalAuthenticationTypes();
			}

			this.Html.ViewData["Settings"] = settings;
			this.Html.ViewData["IsESS"] = Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss;
			this.Html.ViewData["AzureAdOrExternalLoginEnabled"] = this.RegistrationManager.AzureAdOrExternalLoginEnabled;
			this.Html.ViewData["IdentityErrors"] = this.RegistrationManager.IdentityErrors;
			this.Html.ViewData["ReturnUrl"] = this.ReturnUrl;
			this.Html.ViewData["InvitationCode"] = this.InvitationCode;
			this.RegistrationManager.ViewData = this.Html.ViewData;
		}

		/// <summary>
		/// Visibles controls based on conditions
		/// </summary>
		private void ShowControls()
		{
			var settings = this.RegistrationManager.Settings;

			this.SecureRegister.Visible =
				(settings.RegistrationEnabled || this.RegistrationManager.IsExternalRegistrationEnabled) &&
				(settings.OpenRegistrationEnabled || settings.InvitationEnabled);
			this.RegistrationDisabled.Visible = !this.SecureRegister.Visible;
			this.InvitationCodeAlert.Visible = !string.IsNullOrWhiteSpace(this.InvitationCode);
			this.LocalLogin.Visible = settings.LocalLoginEnabled;
			this.ExternalLogin.Visible = this.RegistrationManager.AzureAdOrExternalLoginEnabled &&
			                             this.RegistrationManager.IsExternalRegistrationEnabled;
			this.ShowEmail.Visible = settings.LocalLoginByEmail || settings.RequireUniqueEmail;
			this.ShowUserName.Visible = !settings.LocalLoginByEmail;

#if TELERIKWEBUI

			this.CaptchaRowPlaceHolder.Visible = this.IsCaptchaEnabled;
#endif

		}

		/// <summary>
		/// Gets External Authentication Types
		/// </summary>
		/// <returns>Authentication Description</returns>
		private IEnumerable<AuthenticationDescription> GetExternalAuthenticationTypes()
		{
			return HttpContext.Current.GetOwinContext().Authentication.GetExternalAuthenticationTypes().OrderBy(type => type.Caption).ToList();
		}

		/// <summary>
		/// Triggers on submit button click
		/// </summary>
		/// <param name="sender">Sender of the control</param>
		/// <param name="e">Event Arguments</param>
		protected void SubmitButton_Click(object sender, EventArgs e)
		{
			

			RegisterViewModel registerViewModel = new RegisterViewModel();
			registerViewModel.Email = EmailTextBox.Text;
			registerViewModel.Username = UserNameTextBox.Text;
			registerViewModel.Password = PasswordTextBox.Text;
			registerViewModel.ConfirmPassword = ConfirmPasswordTextBox.Text;

#if TELERIKWEBUI

			this.SetCaptchaDetails(registerViewModel);

#endif
			this.RegistrationManager.ValidateAndRegisterUser(registerViewModel, this.ReturnUrl, this.InvitationCode).GetAwaiter().GetResult();

			var errors = this.RegistrationManager.Errors;

			if (errors.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				errors.ForEach(item => sb.Append(item));
				if (sb.Length > 0)
				{
					errors.Clear();
					Page currentPage = HttpContext.Current.Handler as Page;
					var validator = new CustomValidator();
					validator.IsValid = false;
					validator.ErrorMessage = sb.ToString();
					validator.ValidationGroup = ValidatorValidationGroup;
					currentPage.Validators.Add(validator);
				}
			}
		}

#if TELERIKWEBUI

		/// <summary>
		/// Set captcha details in registration view model
		/// </summary>
		/// <param name="registerViewModel">Register View Model</param>
		private void SetCaptchaDetails(RegisterViewModel registerViewModel)
		{
			registerViewModel.IsCaptchaEnabled = this.IsCaptchaEnabled;
			if (this.captchaValidator != null)
			{
				this.captchaValidator.Validate();
				registerViewModel.IsCaptchaValid = this.captchaValidator.IsValid;
				registerViewModel.CaptchaValidationMessage = this.captchaValidator.ErrorMessage;
			}
		}

#endif

	}
}
