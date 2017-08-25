/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Adxstudio.Xrm.Core.Flighting;
using System.Web;
using Adxstudio.Xrm.Diagnostics.Trace;
using Adxstudio.Xrm.Web;

namespace Adxstudio.Xrm.AspNet.Identity
{
	public class CrmSignInManager<TUser> : SignInManager<TUser, string>
		where TUser : CrmUser
	{
		public CrmSignInManager(UserManager<TUser, string> userManager, IAuthenticationManager authenticationManager)
			: base(userManager, authenticationManager)
		{
		}

		public virtual async Task<SignInStatus> PasswordSignInByEmailAsync(string email, string password, bool isPersistent, bool shouldLockout)
		{
			if (UserManager == null)
			{
				return SignInStatus.Failure;
			}

			var user = await UserManager.FindByEmailAsync(email).WithCurrentCulture();

			if (user == null)
			{
				return SignInStatus.Failure;
			}

			if (string.IsNullOrEmpty(user.UserName))
			{
				if (!await AutoAssignUsernameAsync(user))
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not able to auto assign username based on email");

					return SignInStatus.Failure;
				}

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Username was auto assigned based on email");
			}

			LogAuthentication(user, "internal");

			return await PasswordSignInAsync(user.UserName, password, isPersistent, shouldLockout).WithCurrentCulture();
		}

		public override async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
		{
			if (UserManager == null)
			{
				return SignInStatus.Failure;
			}

			var user = await UserManager.FindByNameAsync(userName).WithCurrentCulture();

			if (user == null || !user.LogonEnabled)
			{
				return SignInStatus.Failure;
			}

			LogAuthentication(user, "internal");

			return await base.PasswordSignInAsync(userName, password, isPersistent, shouldLockout).WithCurrentCulture();
		}

		public virtual async Task<SignInStatus> ExternalSignInAsync(ExternalLoginInfo loginInfo, bool isPersistent, bool shouldLockout)
		{
			if (UserManager == null)
			{
				return SignInStatus.Failure;
			}

			var user = await UserManager.FindAsync(loginInfo.Login).WithCurrentCulture();

			if (user == null || !user.LogonEnabled)
			{
				return SignInStatus.Failure;
			}

			if (Configuration.PortalSettings.Instance.Ess.IsEss && user.Email == null && loginInfo.Email != null)
			{
				await UserManager.SetEmailAsync(user.Id, loginInfo.Email);
			}

			LogAuthentication(user, "external");

			return await ExternalSignInAsync(loginInfo, isPersistent).WithCurrentCulture();
		}

		private async Task<bool> AutoAssignUsernameAsync(TUser user)
		{
			user.UserName = user.Email;

			var validateResult = await UserManager.UserValidator.ValidateAsync(user);

			if (!validateResult.Succeeded)
			{
				return false;
			}

			return UserManager.Update(user).Succeeded;
		}

		private static void LogAuthentication(TUser user, string authenticationType)
		{
			try
			{
				if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
				{
					var userId = HashPii.ComputeHashPiiSha256(user.Id);
					
					PortalFeatureTrace.TraceInstance.LogAuthentication(FeatureTraceCategory.Authentication, userId, HttpContext.Current.Session.SessionID, "logIn", "authentication", authenticationType);
				}
			}
			catch (Exception e)
			{
				WebEventSource.Log.GenericErrorException(e);
			}
		}
	}
}
