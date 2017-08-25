/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Adxstudio.Xrm.AspNet;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.AspNet.Identity;
using Adxstudio.Xrm.AspNet.Organization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Xrm.Portal.Configuration;

namespace Site.Areas.Account.Models
{
	// Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

	public class ApplicationUserManager : CrmUserManager<ApplicationUser, string>
	{
		public ApplicationUserManager(IUserStore<ApplicationUser> store, CrmIdentityErrorDescriber identityErrors)
			: base(store, identityErrors)
		{
		}

		public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
		{
			var dbContext = context.Get<CrmDbContext>();
			var website = context.Get<CrmWebsite>();

			var manager = new ApplicationUserManager(
				new UserStore(dbContext, website.GetCrmUserStoreSettings(context)),
				website.GetCrmIdentityErrorDescriber(context));

			// Configure default validation logic for usernames
			var userValidator = manager.UserValidator as UserValidator<ApplicationUser, string>;

			if (userValidator != null)
			{
				userValidator.AllowOnlyAlphanumericUserNames = false;
				userValidator.RequireUniqueEmail = true;
			}

			// Configure default validation logic for passwords
			var passwordValidator = manager.PasswordValidator as PasswordValidator;

			if (passwordValidator != null)
			{
				passwordValidator.RequiredLength = 8;
				passwordValidator.RequireNonLetterOrDigit = false;
				passwordValidator.RequireDigit = false;
				passwordValidator.RequireLowercase = false;
				passwordValidator.RequireUppercase = false;
			}

			var crmPasswordValidator = manager.PasswordValidator as CrmPasswordValidator;

			if (crmPasswordValidator != null)
			{
				crmPasswordValidator.EnforcePasswordPolicy = true;
			}

			var minimumLengthValidator = manager.PasswordValidator as MinimumLengthValidator;

			if (minimumLengthValidator != null)
			{
				minimumLengthValidator.RequiredLength = 8;
			}

			// Configure user lockout defaults
			manager.UserLockoutEnabledByDefault = true;
			manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(1);
			manager.MaxFailedAccessAttemptsBeforeLockout = 5;

			// Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
			// You can write your own provider and plug in here.
			manager.RegisterTwoFactorProvider("PhoneCode", new CrmPhoneNumberTokenProvider<ApplicationUser>(context.Get<ApplicationOrganizationManager>()));
			manager.RegisterTwoFactorProvider("EmailCode", new CrmEmailTokenProvider<ApplicationUser>(context.Get<ApplicationOrganizationManager>()));
			manager.EmailService = new EmailService();
			manager.SmsService = new SmsService();

			var dataProtectionProvider = options.DataProtectionProvider;

			if (dataProtectionProvider != null)
			{
				manager.UserTokenProvider =
					new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
			}

			var claimsIdentityFactory = new CrmClaimsIdentityFactory<ApplicationUser>(context.Authentication)
			{
				KeepExternalLoginClaims = true,
			};

			manager.ClaimsIdentityFactory = claimsIdentityFactory;

			manager.Configure(website);

			return manager;
		}
	}

	public class EmailService : IIdentityMessageService
	{
		public Task SendAsync(IdentityMessage message)
		{
			// Plug in your email service here to send an email.
			return Task.FromResult(0);
		}
	}

	public class SmsService : IIdentityMessageService
	{
		public Task SendAsync(IdentityMessage message)
		{
			// Plug in your sms service here to send a text message.
			return Task.FromResult(0);
		}
	}

	public class ApplicationSignInManager : CrmSignInManager<ApplicationUser>
	{
		public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
			: base(userManager, authenticationManager)
		{
		}

		public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
		{
			var manager = new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);

			return manager;
		}

		public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
		{
			return user.GenerateUserIdentityAsync(UserManager);
		}
	}

	public class ApplicationInvitationManager : InvitationManager<ApplicationInvitation, string>
	{
		public ApplicationInvitationManager(IInvitationStore<ApplicationInvitation, string> store, CrmIdentityErrorDescriber identityErrors)
			: base(store, identityErrors)
		{
		}

		public static ApplicationInvitationManager Create(IdentityFactoryOptions<ApplicationInvitationManager> options, IOwinContext context)
		{
			return new ApplicationInvitationManager(
				new InvitationStore(context.Get<CrmDbContext>()),
				context.Get<CrmWebsite>().GetCrmIdentityErrorDescriber(context));
		}
	}

	public class ApplicationOrganizationManager : OrganizationManager
	{
		public ApplicationOrganizationManager(IOrganizationStore store)
			: base(store)
		{
		}

		public static ApplicationOrganizationManager Create(IdentityFactoryOptions<ApplicationOrganizationManager> options, IOwinContext context)
		{
			return new ApplicationOrganizationManager(new OrganizationStore(context.Get<CrmDbContext>()));
		}
	}

	public class ApplicationWebsite : CrmWebsite
	{
		public static CrmWebsite Create(IdentityFactoryOptions<CrmWebsite> options, IOwinContext context, PortalHostingEnvironment environment)
		{
			var websiteManager = context.Get<ApplicationWebsiteManager>();
			return websiteManager.Find(context, environment);
		}
	}

	public class ApplicationWebsiteManager : WebsiteManager<ApplicationWebsite, Guid>
	{
		public ApplicationWebsiteManager(IWebsiteStore<ApplicationWebsite, Guid> store)
			: base(store)
		{
		}

		public static ApplicationWebsiteManager Create(CrmDbContext dbContext, CrmEntityStoreSettings settings)
		{
			return CreateWebsiteManager(dbContext, settings);
		}

		public static ApplicationWebsiteManager Create(IdentityFactoryOptions<ApplicationWebsiteManager> options, IOwinContext context)
		{
			var settings = new CrmEntityStoreSettings { PortalSolutions = context.Get<PortalSolutions>() };
			return CreateWebsiteManager(context.Get<CrmDbContext>(), settings);
		}

		private static ApplicationWebsiteManager CreateWebsiteManager(CrmDbContext dbContext, CrmEntityStoreSettings settings)
		{
			var manager = new ApplicationWebsiteManager(new CrmWebsiteStore<ApplicationWebsite, Guid>(dbContext, settings))
			{
				WebsiteName = GetWebsiteName()
			};

			return manager;
		}

		private static string GetWebsiteName()
		{
			var portal = PortalCrmConfigurationManager.GetPortalContextElement("Xrm");
			return portal == null ? null : portal.Parameters["websiteName"];
		}
	}


	public class ApplicationStartupSettingsManager : StartupSettingsManager<ApplicationUser>, IDisposable
	{
		public ApplicationStartupSettingsManager(
			CrmWebsite website,
			Func<UserManager<ApplicationUser, string>, ApplicationUser, Task<ClaimsIdentity>> regenerateIdentityCallback,
			PathString loginPath,
			PathString externalLoginCallbackPath,
			PathString externalAuthenticationFailedPath,
			PathString externalPasswordResetPath)
			: base(website, regenerateIdentityCallback, loginPath, externalLoginCallbackPath, externalAuthenticationFailedPath, externalPasswordResetPath)
		{
		}

		protected override UserManager<ApplicationUser, string> GetUserManager(IOwinContext context)
		{
			return context.GetUserManager<ApplicationUserManager>();
		}

		void IDisposable.Dispose()
		{
		}
	}
}
