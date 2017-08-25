/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Identity
{
	using System;
	using System.Globalization;
	using System.Runtime.CompilerServices;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Web;
	using Microsoft.Owin;
	using Xrm.Cms;

	public class IdentityError
	{
		public string Code { get; set; }
		public string Description { get; set; }
	}

	public class IdentityErrorDescriber
	{
		public IContentMapProvider ContentMapProvider { get; private set; }

		public ContextLanguageInfo Language { get; private set; }

		public IdentityErrorDescriber(IOwinContext context)
			: this(context.GetContentMapProvider(), context.GetContextLanguageInfo())
		{
		}

		public IdentityErrorDescriber(IContentMapProvider contentMapProvider, ContextLanguageInfo language)
		{
			if (contentMapProvider == null)
			{
				throw new ArgumentNullException("contentMapProvider");
			}

			this.ContentMapProvider = contentMapProvider;
			this.Language = language;
		}

		public virtual IdentityError DefaultError()
		{
			var guid = WebEventSource.Log.GenericErrorException(null);
			return GetError(string.Format(ResourceManager.GetString("Generic_Error_Message"), guid));
		}

		public virtual IdentityError ConcurrencyFailure()
		{
			return GetError(ResourceManager.GetString("Optimistic_Concurrency_Failure_Exception"));
		}

		public virtual IdentityError PasswordMismatch()
		{
			return GetError(ResourceManager.GetString("Incorrect_Password_Exception"));
		}

		public virtual IdentityError InvalidToken()
		{
			return GetError(ResourceManager.GetString("Invalid_Token_Exception"));
		}

		public virtual IdentityError LoginAlreadyAssociated()
		{
			return GetError(ResourceManager.GetString("User_Already_Exists_Exception"));
		}

		public virtual IdentityError InvalidUserName(string name)
		{
			return GetError(ResourceManager.GetString("User_Name_Invalid_Can_Only_Contain_Letters_Digits_Exception"), name);
		}

		public virtual IdentityError InvalidEmail(string email)
		{
			return GetError(ResourceManager.GetString("Invalid_Email_Exception"), email);
		}

		public virtual IdentityError DuplicateUserName(string name)
		{
			return GetError(ResourceManager.GetString("User_Name_Alredy_Taken_Exception"), name);
		}

		public virtual IdentityError DuplicateEmail(string email)
		{
			return GetError(ResourceManager.GetString("Email_Already_Used_Exception"), email);
		}

		public virtual IdentityError InvalidRoleName(string name)
		{
			return GetError(ResourceManager.GetString("Invalid_Rolename_Exception"), name);
		}

		public virtual IdentityError DuplicateRoleName(string name)
		{
			return GetError(ResourceManager.GetString("Role_Name_Already_Taken_Exception"), name);
		}

		public virtual IdentityError UserAlreadyHasPassword()
		{
			return GetError(ResourceManager.GetString("User_Already_Has_Password_Set_Exception"));
		}

		public virtual IdentityError UserLockoutNotEnabled()
		{
			return GetError(ResourceManager.GetString("Lockout_Is_Disabled_For_This_User"));
		}

		public virtual IdentityError UserAlreadyInRole(string role)
		{
			return GetError(ResourceManager.GetString("User_Alredy_In_Role_Exception"), role);
		}

		public virtual IdentityError UserNotInRole(string role)
		{
			return GetError(ResourceManager.GetString("User_Not_In_Role_Exception"), role);
		}

		public virtual IdentityError PasswordTooShort(int length)
		{
			return GetError(ResourceManager.GetString("Password_Error_For_Characters"), length);
		}

		public virtual IdentityError PasswordRequiresNonLetterAndDigit()
		{
			return GetError(string.Format(ResourceManager.GetString("Password_Errors"), "non letter and non digit character"));
		}

		public virtual IdentityError PasswordRequiresDigit()
		{
			return GetError(string.Format(ResourceManager.GetString("Password_Errors"), "digit ('0'-'9')"));
		}

		public virtual IdentityError PasswordRequiresLower()
		{
			return GetError(string.Format(ResourceManager.GetString("Password_Errors"), "lowercase ('a'-'z')"));
		}

		public virtual IdentityError PasswordRequiresUpper()
		{
			return GetError(string.Format(ResourceManager.GetString("Password_Errors"), "uppercase ('A'-'Z')"));
		}
		public virtual IdentityError PasswordRequiresThreeClasses()
		{
			return GetError(ResourceManager.GetString("Passwords_Not_Meeting_Requirement"));
		}

		protected virtual IdentityError GetError(string message, object arg = null, [CallerMemberName] string code = null)
		{
			return this.ContentMapProvider.Using(map =>
			{
				var description = map.GetSnippet("Account/Errors/" + code, this.Language) ?? message;

				return new IdentityError
				{
					Code = code,
					Description = string.Format(CultureInfo.CurrentCulture, description, new[] { arg })
				};
			});
		}
	}

	public class CrmIdentityErrorDescriber : IdentityErrorDescriber
	{
		public CrmIdentityErrorDescriber(IOwinContext context)
			: base(context)
		{
		}

		public virtual IdentityError UserLocked()
		{
			return GetError(ResourceManager.GetString("User_Account_Locked_Exception"));
		}

		public virtual IdentityError InvalidLogin()
		{
			return GetError(ResourceManager.GetString("Authentication_Error_Invalid_Login"));
		}

		public virtual IdentityError InvalidTwoFactorCode()
		{
			return GetError(ResourceManager.GetString("Invalid_Code_Exception"));
		}

		public virtual IdentityError InvalidInvitationCode()
		{
			return GetError(ResourceManager.GetString("Invalid_Invitation_Code_Exception"));
		}

		public virtual IdentityError EmailRequired()
		{
			return GetError(ResourceManager.GetString("Email_Field_Required_Exception"));
		}

		public virtual IdentityError UserNameRequired()
		{
			return GetError(ResourceManager.GetString("User_Name_Field_Required_Exception"));
		}

		public virtual IdentityError PasswordConfirmationFailure()
		{
			return GetError(ResourceManager.GetString("Registration_Error_Password_Confirmation_Failure"));
		}

		public virtual IdentityError NewPasswordConfirmationFailure()
		{
			return GetError(ResourceManager.GetString("Old_New_Password_Should_Not_Match_Exception"));
		}

		internal IdentityError InvalidUserNameWithComma(string name)
		{
			return GetError(ResourceManager.GetString("User_Name_Invalid_Exception"), name);
		}

		public virtual IdentityError TooManyAttempts()
		{
			return GetError(ResourceManager.GetString("Authentication_Error_TooMany_Attempts"));
		}


		/// <summary>
		/// Returns a localized message for invalid email.
		/// </summary>
		/// <returns>localized message as string</returns>
		public virtual IdentityError ValidEmailRequired()
		{
			return GetError(ResourceManager.GetString("Invalid_Email_Message"));
		}

		/// <summary>
		/// Localized message for required password
		/// </summary>
		/// <returns>Localized message as string</returns>
		public virtual IdentityError PasswordRequired()
		{
			return GetError(ResourceManager.GetString("Password_Is_Required_Field_Error"));
		}

		/// <summary>
		/// Using the existing pattern for captcha message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns>Error message</returns>
		public virtual IdentityError CaptchaRequired(string message)
		{
			return GetError(message);
		}
	}
}
