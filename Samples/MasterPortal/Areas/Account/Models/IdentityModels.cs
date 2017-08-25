/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Security.Claims;
using System.Threading.Tasks;
using Adxstudio.Xrm.AspNet;
using Adxstudio.Xrm.AspNet.Identity;
using Adxstudio.Xrm.AspNet.Organization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.Account.Models
{
	// You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
	public class ApplicationUser : CrmUser
	{
		public static CrmUser Create(IdentityFactoryOptions<CrmUser> options, IOwinContext context)
		{
			var user = context.Request.User;

			if (user != null && user.Identity.IsAuthenticated)
			{
				var manager = context.Get<ApplicationUserManager>();
				return manager.FindById(user.Identity.GetUserId());
			}

			return Anonymous;
		}

		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, string> manager)
		{
			// Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
			var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
			// Add custom user claims here
			return userIdentity;
		}
	}

	public class ApplicationDbContext : CrmDbContext
	{
		public ApplicationDbContext(OrganizationServiceManager serviceManager)
			: base(serviceManager)
		{
		}

		public ApplicationDbContext(IOrganizationService service)
			: base(service)
		{
		}

		public static CrmDbContext Create()
		{
			return new ApplicationDbContext(CrmConfigurationManager.CreateService(new CrmConnection("Xrm")));
		}
	}

	public class UserStore : CrmUserStore<ApplicationUser, string>, IUserStore<ApplicationUser>
	{
		public UserStore(CrmDbContext context, CrmEntityStoreSettings settings)
			: base(context, settings)
		{
		}
	}

	public class ApplicationInvitation : CrmInvitation<string>
	{
	}

	public class InvitationStore : CrmInvitationStore<ApplicationInvitation, string>
	{
		public InvitationStore(CrmDbContext context)
			: base(context)
		{
		}
	}

	public class OrganizationStore : CrmOrganizationStore
	{
		public OrganizationStore(CrmDbContext context)
			: base(context)
		{
		}
	}
}
