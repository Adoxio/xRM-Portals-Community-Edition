/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.AspNet.Identity;

namespace Adxstudio.Xrm.AspNet.Identity
{
	public class CrmUserManager<TUser, TKey> : InternalUserManager<TUser, TKey>
		where TUser : CrmUser<TKey>
		where TKey : IEquatable<TKey>
	{
		public CrmUserManager(IUserStore<TUser, TKey> store, CrmIdentityErrorDescriber identityErrors)
			: base(store, identityErrors)
		{
		}

		public virtual void Configure<TWebsiteKey>(CrmWebsite<TWebsiteKey> website)
		{
			if (website == null) throw new ArgumentNullException("website");

			var userValidator = UserValidator as UserValidator<TUser, TKey>;

			if (userValidator != null)
			{
				userValidator.AllowOnlyAlphanumericUserNames = website.Settings.Get<bool?>("Authentication/UserManager/UserValidator/AllowOnlyAlphanumericUserNames").GetValueOrDefault(userValidator.AllowOnlyAlphanumericUserNames);
				userValidator.RequireUniqueEmail = website.Settings.Get<bool?>("Authentication/UserManager/UserValidator/RequireUniqueEmail").GetValueOrDefault(userValidator.RequireUniqueEmail);
			}

			var passwordValidator = PasswordValidator as PasswordValidator;

			if (passwordValidator != null)
			{
				passwordValidator.RequiredLength = website.Settings.Get<int?>("Authentication/UserManager/PasswordValidator/RequiredLength").GetValueOrDefault(passwordValidator.RequiredLength);
				passwordValidator.RequireNonLetterOrDigit = website.Settings.Get<bool?>("Authentication/UserManager/PasswordValidator/RequireNonLetterOrDigit").GetValueOrDefault(passwordValidator.RequireNonLetterOrDigit);
				passwordValidator.RequireDigit = website.Settings.Get<bool?>("Authentication/UserManager/PasswordValidator/RequireDigit").GetValueOrDefault(passwordValidator.RequireDigit);
				passwordValidator.RequireLowercase = website.Settings.Get<bool?>("Authentication/UserManager/PasswordValidator/RequireLowercase").GetValueOrDefault(passwordValidator.RequireLowercase);
				passwordValidator.RequireUppercase = website.Settings.Get<bool?>("Authentication/UserManager/PasswordValidator/RequireUppercase").GetValueOrDefault(passwordValidator.RequireUppercase);
			}

			var crmPasswordValidator = PasswordValidator as CrmPasswordValidator;

			if (crmPasswordValidator != null)
			{
				crmPasswordValidator.EnforcePasswordPolicy = website.Settings.Get<bool?>("Authentication/UserManager/PasswordValidator/EnforcePasswordPolicy").GetValueOrDefault(crmPasswordValidator.EnforcePasswordPolicy);
			}

			var minimumLengthValidator = PasswordValidator as MinimumLengthValidator;

			if (minimumLengthValidator != null)
			{
				minimumLengthValidator.RequiredLength = website.Settings.Get<int?>("Authentication/UserManager/PasswordValidator/RequiredLength").GetValueOrDefault(minimumLengthValidator.RequiredLength);
			}

			UserLockoutEnabledByDefault = website.Settings.Get<bool?>("Authentication/UserManager/UserLockoutEnabledByDefault").GetValueOrDefault(UserLockoutEnabledByDefault);
			DefaultAccountLockoutTimeSpan = website.Settings.Get<TimeSpan?>("Authentication/UserManager/DefaultAccountLockoutTimeSpan").GetValueOrDefault(DefaultAccountLockoutTimeSpan);
			MaxFailedAccessAttemptsBeforeLockout = website.Settings.Get<int?>("Authentication/UserManager/MaxFailedAccessAttemptsBeforeLockout").GetValueOrDefault(MaxFailedAccessAttemptsBeforeLockout);
		}

		public virtual async Task<IdentityResult> SetUsernameAndEmailAsync(TKey userId, string userName, string email)
		{
			var user = await FindByIdAsync(userId).WithCurrentCulture();

			if (user == null)
			{
				throw new InvalidOperationException("Account error.");
			}

			var store = GetEmailStore();
			await store.SetEmailAsync(user, email).WithCurrentCulture();
			await store.SetEmailConfirmedAsync(user, false).WithCurrentCulture();
			await UpdateSecurityStampInternal(user).WithCurrentCulture();

			user.UserName = userName;

			return await UpdateAsync(user).WithCurrentCulture();
		}

		public virtual async Task<IdentityResult> AddUsernameAndPasswordAsync(TKey userId, string userName, string password)
		{
			var user = await FindByIdAsync(userId).WithCurrentCulture();

			if (user == null)
			{
				throw new InvalidOperationException("Account error.");
			}

			var passwordStore = GetPasswordStore();
			var hash = await passwordStore.GetPasswordHashAsync(user).WithCurrentCulture();

			if (hash != null)
			{
				throw new InvalidOperationException("Account error. User already has a password.");
			}

			var passwordResult = await UpdatePasswordInternal(passwordStore, user, password).WithCurrentCulture();

			if (!passwordResult.Succeeded) return passwordResult;

			return await UpdateUserNameInternal(user, userName).WithCurrentCulture();
		}

		public virtual async Task<IdentityResult> InitializeUserAsync(TUser user, string userName, string password, string email, bool lockoutEnabled)
		{
			user.UserName = userName;
			user.Email = email;
			user.SecurityStamp = NewSecurityStamp();
			user.LockoutEnabled = lockoutEnabled;
			user.LogonEnabled = true;

			var result = await UpdateAsync(user).WithCurrentCulture();

			// set the password

			return result.Succeeded && !string.IsNullOrWhiteSpace(password)
				? await SetPasswordAsync(user.Id, password).WithCurrentCulture()
				: result;
		}

		protected virtual async Task<IdentityResult> SetPasswordAsync(TKey userId, string password)
		{
			var code = await GeneratePasswordResetTokenAsync(userId).WithCurrentCulture();
			return await ResetPasswordAsync(userId, code, password).WithCurrentCulture();
		}

		private new async Task UpdateSecurityStampInternal(TUser user)
		{
			if (SupportsUserSecurityStamp)
			{
				await GetSecurityStore().SetSecurityStampAsync(user, NewSecurityStamp()).WithCurrentCulture();
			}
		}

		private async Task<IdentityResult> UpdatePasswordInternal(IUserPasswordStore<TUser, TKey> passwordStore, TUser user, string newPassword)
		{
			var result = await PasswordValidator.ValidateAsync(newPassword).WithCurrentCulture();

			if (!result.Succeeded)
			{
				return result;
			}

			await passwordStore.SetPasswordHashAsync(user, PasswordHasher.HashPassword(newPassword)).WithCurrentCulture();
			await UpdateSecurityStampInternal(user).WithCurrentCulture();

			return IdentityResult.Success;
		}

		private async Task<IdentityResult> UpdateUserNameInternal(TUser user, string userName)
		{
			user.UserName = userName;

			return await UpdateAsync(user).WithCurrentCulture();
		}

		private static string NewSecurityStamp()
		{
			return Guid.NewGuid().ToString();
		}

		private IUserSecurityStampStore<TUser, TKey> GetSecurityStore()
		{
			var store = Store as IUserSecurityStampStore<TUser, TKey>;

			if (store == null)
			{
				throw new NotSupportedException();
			}

			return store;
		}

		private new IUserEmailStore<TUser, TKey> GetEmailStore()
		{
			var store = Store as IUserEmailStore<TUser, TKey>;

			if (store == null)
			{
				throw new NotSupportedException();
			}

			return store;
		}

		private IUserPasswordStore<TUser, TKey> GetPasswordStore()
		{
			var store = Store as IUserPasswordStore<TUser, TKey>;

			if (store == null)
			{
				throw new NotSupportedException("Store does not implement IUserPasswordStore<TUser>.");
			}

			return store;
		}
	}

	public class InternalUserManager<TUser, TKey> : UserManager<TUser, TKey>
		where TUser : class, IUser<TKey>
		where TKey : IEquatable<TKey>
	{
		public virtual CrmIdentityErrorDescriber IdentityErrors { get; private set; }

		public InternalUserManager(IUserStore<TUser, TKey> store, CrmIdentityErrorDescriber identityErrors)
			: base(store)
		{
			UserValidator = new InternalUserValidator<TUser, TKey>(this, identityErrors);
			PasswordValidator = new CrmPasswordValidator(identityErrors);
			IdentityErrors = identityErrors;
		}

		public override async Task<IdentityResult> ChangePasswordAsync(TKey userId, string currentPassword, string newPassword)
		{
			var passwordStore = GetPasswordStore();
			var user = await FindByIdAsync(userId).WithCurrentCulture();
			if (user == null)
			{
				throw new InvalidOperationException("UserId not found.");
			}
			if (await VerifyPasswordAsync(passwordStore, user, currentPassword).WithCurrentCulture())
			{
				var result = await UpdatePassword(passwordStore, user, newPassword).WithCurrentCulture();
				if (!result.Succeeded)
				{
					return result;
				}
				return await UpdateAsync(user).WithCurrentCulture();
			}
			return ToResult(IdentityErrors.PasswordMismatch());
		}

		public override async Task<IdentityResult> ResetPasswordAsync(TKey userId, string token, string newPassword)
		{
			var user = await FindByIdAsync(userId).WithCurrentCulture();
			if (user == null)
			{
				throw new InvalidOperationException("UserId not found.");
			}
			// Make sure the token is valid and the stamp matches
			if (!await VerifyUserTokenAsync(userId, "ResetPassword", token).WithCurrentCulture())
			{
				return ToResult(IdentityErrors.InvalidToken());
			}
			var passwordStore = GetPasswordStore();
			var result = await UpdatePassword(passwordStore, user, newPassword).WithCurrentCulture();
			if (!result.Succeeded)
			{
				return result;
			}
			return await UpdateAsync(user).WithCurrentCulture();
		}

		public override async Task<IdentityResult> AddLoginAsync(TKey userId, UserLoginInfo login)
		{
			var loginStore = GetLoginStore();
			if (login == null)
			{
				throw new ArgumentNullException("login");
			}
			var user = await FindByIdAsync(userId).WithCurrentCulture();
			if (user == null)
			{
				throw new InvalidOperationException("UserId not found.");
			}
			var existingUser = await FindAsync(login).WithCurrentCulture();
			if (existingUser != null)
			{
				return ToResult(IdentityErrors.LoginAlreadyAssociated());
			}
			await loginStore.AddLoginAsync(user, login).WithCurrentCulture();
			return await UpdateAsync(user).WithCurrentCulture();
		}

		public override async Task<IdentityResult> ConfirmEmailAsync(TKey userId, string token)
		{
			var store = GetEmailStore();
			var user = await FindByIdAsync(userId).WithCurrentCulture();
			if (user == null)
			{
				throw new InvalidOperationException("UserId not found.");
			}
			if (!await VerifyUserTokenAsync(userId, "Confirmation", token).WithCurrentCulture())
			{
				return ToResult(IdentityErrors.InvalidToken());
			}
			await store.SetEmailConfirmedAsync(user, true).WithCurrentCulture();
			return await UpdateAsync(user).WithCurrentCulture();
		}


		/// <summary>
		/// Manually set email confirmation flag
		/// </summary>
		public async Task<IdentityResult> AutoConfirmEmailAsync(TKey userId)
		{
			var store = GetEmailStore();
			var user = await FindByIdAsync(userId).WithCurrentCulture();
			if (user == null)
			{
				throw new InvalidOperationException("UserId not found.");
			}

			await store.SetEmailConfirmedAsync(user, true).WithCurrentCulture();
			return await UpdateAsync(user).WithCurrentCulture();
		}

		public override async Task<IdentityResult> ChangePhoneNumberAsync(TKey userId, string phoneNumber, string token)
		{
			var store = GetPhoneNumberStore();
			var user = await FindByIdAsync(userId).WithCurrentCulture();
			if (user == null)
			{
				throw new InvalidOperationException("UserId not found.");
			}
			if (await VerifyChangePhoneNumberTokenAsync(userId, token, phoneNumber).WithCurrentCulture())
			{
				await store.SetPhoneNumberAsync(user, phoneNumber).WithCurrentCulture();
				await store.SetPhoneNumberConfirmedAsync(user, true).WithCurrentCulture();
				await UpdateSecurityStampInternal(user).WithCurrentCulture();
				return await UpdateAsync(user).WithCurrentCulture();
			}
			return ToResult(IdentityErrors.InvalidToken());
		}

		public override async Task<IdentityResult> SetLockoutEndDateAsync(TKey userId, DateTimeOffset lockoutEnd)
		{
			var store = GetUserLockoutStore();
			var user = await FindByIdAsync(userId).WithCurrentCulture();
			if (user == null)
			{
				throw new InvalidOperationException("UserId not found.");
			}
			if (!await store.GetLockoutEnabledAsync(user).WithCurrentCulture())
			{
				return ToResult(IdentityErrors.UserLockoutNotEnabled());
			}
			await store.SetLockoutEndDateAsync(user, lockoutEnd).WithCurrentCulture();
			return await UpdateAsync(user).WithCurrentCulture();
		}

		private IUserLoginStore<TUser, TKey> GetLoginStore()
		{
			var cast = Store as IUserLoginStore<TUser, TKey>;
			if (cast == null)
			{
				throw new NotSupportedException("Store does not implement IUserLoginStore<TUser>.");
			}
			return cast;
		}

		private IUserPasswordStore<TUser, TKey> GetPasswordStore()
		{
			var store = Store as IUserPasswordStore<TUser, TKey>;

			if (store == null)
			{
				throw new NotSupportedException("Store does not implement IUserPasswordStore<TUser>.");
			}

			return store;
		}

		internal IUserEmailStore<TUser, TKey> GetEmailStore()
		{
			var cast = Store as IUserEmailStore<TUser, TKey>;
			if (cast == null)
			{
				throw new NotSupportedException("Store does not implement IUserEmailStore<TUser>.");
			}
			return cast;
		}

		internal IUserPhoneNumberStore<TUser, TKey> GetPhoneNumberStore()
		{
			var cast = Store as IUserPhoneNumberStore<TUser, TKey>;
			if (cast == null)
			{
				throw new NotSupportedException("Store does not implement IUserPhoneNumberStore<TUser>.");
			}
			return cast;
		}

		internal IUserLockoutStore<TUser, TKey> GetUserLockoutStore()
		{
			var cast = Store as IUserLockoutStore<TUser, TKey>;
			if (cast == null)
			{
				throw new NotSupportedException("Store does not implement IUserLockoutStore<TUser>.");
			}
			return cast;
		}

		private IUserSecurityStampStore<TUser, TKey> GetSecurityStore()
		{
			var cast = Store as IUserSecurityStampStore<TUser, TKey>;
			if (cast == null)
			{
				throw new NotSupportedException("Store does not implement IUserSecurityStampStore<TUser>.");
			}
			return cast;
		}

		private static string NewSecurityStamp()
		{
			return Guid.NewGuid().ToString();
		}

		internal async Task UpdateSecurityStampInternal(TUser user)
		{
			if (SupportsUserSecurityStamp)
			{
				await GetSecurityStore().SetSecurityStampAsync(user, NewSecurityStamp()).WithCurrentCulture();
			}
		}

		private static IdentityResult ToResult(params IdentityError[] errors)
		{
			return IdentityResult.Failed(errors.Select(error => error.Description).ToArray());
		}
	}

	internal class InternalUserValidator<TUser, TKey> : UserValidator<TUser, TKey>
		where TUser : class, IUser<TKey>
		where TKey : IEquatable<TKey>
	{
		private InternalUserManager<TUser, TKey> Manager { get; set; }

		public virtual CrmIdentityErrorDescriber IdentityErrors { get; private set; }

		public InternalUserValidator(InternalUserManager<TUser, TKey> manager, CrmIdentityErrorDescriber identityErrors)
			: base(manager)
		{
			Manager = manager;
			IdentityErrors = identityErrors;
		}

		public override async Task<IdentityResult> ValidateAsync(TUser item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			var errors = new List<string>();
			await ValidateUserName(item, errors).WithCurrentCulture();
			if (RequireUniqueEmail)
			{
				await ValidateEmailAsync(item, errors).WithCurrentCulture();
			}
			if (errors.Count > 0)
			{
				return IdentityResult.Failed(errors.ToArray());
			}
			return IdentityResult.Success;
		}

		private async Task ValidateUserName(TUser user, List<string> errors)
		{
			if (string.IsNullOrWhiteSpace(user.UserName))
			{
				errors.Add(ToResult(IdentityErrors.InvalidUserName(user.UserName)));
			}
			else if (AllowOnlyAlphanumericUserNames && !Regex.IsMatch(user.UserName, @"^[A-Za-z0-9@_\.]+$"))
			{
				errors.Add(ToResult(IdentityErrors.InvalidUserName(user.UserName)));
			}

			// block commas in usernames to prevent exceptions in the role provider

			else if (user.UserName.Contains(","))
			{
				errors.Add(ToResult(IdentityErrors.InvalidUserNameWithComma(user.UserName)));
			}
			else
			{
				var owner = await Manager.FindByNameAsync(user.UserName).WithCurrentCulture();
				if (owner != null && !EqualityComparer<TKey>.Default.Equals(owner.Id, user.Id))
				{
					errors.Add(ToResult(IdentityErrors.DuplicateUserName(user.UserName)));
				}
			}
		}

		private async Task ValidateEmailAsync(TUser user, List<string> errors)
		{
			var email = await Manager.GetEmailStore().GetEmailAsync(user).WithCurrentCulture();
			if (string.IsNullOrWhiteSpace(email))
			{
				errors.Add(ToResult(IdentityErrors.EmailRequired()));
				return;
			}
			try
			{
				var m = new MailAddress(email);
			}
			catch (FormatException)
			{
				errors.Add(ToResult(IdentityErrors.InvalidEmail(email)));
				return;
			}
			var owner = await Manager.FindByEmailAsync(email).WithCurrentCulture();
			if (owner != null && !EqualityComparer<TKey>.Default.Equals(owner.Id, user.Id))
			{
				errors.Add(ToResult(IdentityErrors.DuplicateEmail(email)));
			}
		}

		private static string ToResult(IdentityError error)
		{
			return error.Description;
		}
	}
}
