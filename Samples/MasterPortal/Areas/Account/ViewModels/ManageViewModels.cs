/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Adxstudio.Xrm.Resources.ResourceFiles;

namespace Site.Areas.Account.ViewModels
{
	public class IndexViewModel
	{
		public bool HasPassword { get; set; }
		public IList<UserLoginInfo> Logins { get; set; }
		public string PhoneNumber { get; set; }
		public bool TwoFactor { get; set; }
		public bool BrowserRemembered { get; set; }
	}

	public class ManageLoginsViewModel
	{
		public IList<UserLoginInfo> CurrentLogins { get; set; }
		public IList<AuthenticationDescription> OtherLogins { get; set; }
	}

	public class FactorViewModel
	{
		public string Purpose { get; set; }
	}

	public class SetPasswordViewModel
	{
        [EmailAddress(ErrorMessageResourceName = "Invalid_Email_Message", ErrorMessageResourceType = typeof(strings), ErrorMessage = null)]
        public string Email { get; set; }

		public string Username { get; set; }

        [Required(ErrorMessageResourceName = "New_Password_Required_Field_Validation_Message", ErrorMessageResourceType = typeof(strings))]
        [DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; }
	}

	public class ChangePasswordViewModel
	{
        [EmailAddress(ErrorMessageResourceName = "Invalid_Email_Message", ErrorMessageResourceType = typeof(strings), ErrorMessage = null)]
        public string Email { get; set; }

		public string Username { get; set; }

        [Required(ErrorMessageResourceName = "Current_Password_Required_Field_Validation_Message", ErrorMessageResourceType = typeof(strings))]
        [DataType(DataType.Password)]
		public string OldPassword { get; set; }

        [Required(ErrorMessageResourceName = "New_Password_Required_Field_Validation_Message", ErrorMessageResourceType = typeof(strings))]
        [DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; }
	}


	public class AddPhoneNumberViewModel
	{
        [Required(ErrorMessageResourceName = "Phone_Number_Required_Field_Validation_Message", ErrorMessageResourceType = typeof(strings))]
        [Phone]
		public string Number { get; set; }
	}

	public class VerifyPhoneNumberViewModel
	{
		[Required]
		public string Code { get; set; }

        [Required(ErrorMessageResourceName = "Phone_Number_Required_Field_Validation_Message", ErrorMessageResourceType = typeof(strings))]
        [Phone]
		public string PhoneNumber { get; set; }
	}

	public class ConfigureTwoFactorViewModel
	{
		public string SelectedProvider { get; set; }
		public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
	}

	public class ChangeEmailViewModel
	{
        [Required(ErrorMessageResourceName = "Email_Field_Required_Exception", ErrorMessageResourceType = typeof(strings))]
        [EmailAddress(ErrorMessageResourceName = "Invalid_Email_Message", ErrorMessageResourceType = typeof(strings), ErrorMessage = null)]
        public string Email { get; set; }
	}

	public class LoginPair
	{
		public int Id { get; set; }
		public AuthenticationDescription Provider { get; set; }
		public UserLoginInfo User { get; set; }
	}

	public class ChangeLoginViewModel
	{
		public IList<LoginPair> Logins { get; set; }
	}

	public class ManageNavSettings
	{
		public bool HasPassword { get; set; }
		public bool IsEmailConfirmed { get; set; }
		public bool IsMobilePhoneConfirmed { get; set; }
		public bool IsTwoFactorEnabled { get; set; }
	}
}
