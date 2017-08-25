/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Adxstudio.Xrm.Resources.ResourceFiles;
using Microsoft.AspNet.Identity.Owin;

namespace Site.Areas.Account.ViewModels
{
	public class ExternalLoginConfirmationViewModel
	{
		[Required(ErrorMessageResourceName = "Email_Field_Required_Exception", ErrorMessageResourceType = typeof(strings))]
		[EmailAddress(ErrorMessageResourceName = "Invalid_Email_Message", ErrorMessageResourceType = typeof(strings), ErrorMessage = null)]
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Username { get; set; }
	}

	public class ExternalLoginListViewModel
	{
		public string ReturnUrl { get; set; }
	}

	public class SendCodeViewModel
	{
		public string SelectedProvider { get; set; }
		public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
		public string ReturnUrl { get; set; }
		public bool RememberMe { get; set; }
		public string InvitationCode { get; set; }
	}

	public class VerifyCodeViewModel
	{
		[Required]
		public string Provider { get; set; }

		[Required]
		public string Code { get; set; }
		public string ReturnUrl { get; set; }

		[Display(Name = "Remember this browser?")]
		public bool RememberBrowser { get; set; }

		public bool RememberMe { get; set; }
		public string InvitationCode { get; set; }
	}

	public class ForgotViewModel
	{
		[Required(ErrorMessageResourceName = "Email_Field_Required_Exception", ErrorMessageResourceType = typeof(strings))]
		[EmailAddress(ErrorMessageResourceName = "Invalid_Email_Message", ErrorMessageResourceType = typeof(strings), ErrorMessage = null)]
		public string Email { get; set; }
	}

	public class LoginViewModel
	{
		[EmailAddress(ErrorMessageResourceName = "Invalid_Email_Message", ErrorMessageResourceType = typeof(strings), ErrorMessage = null)]
		public string Email { get; set; }

		public string Username { get; set; }

		[Required(ErrorMessageResourceName = "Password_Is_Required_Field_Error", ErrorMessageResourceType = typeof(strings))]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		public bool RememberMe { get; set; }
	}

	public class RegisterViewModel
	{
		[EmailAddress(ErrorMessageResourceName = "Invalid_Email_Message", ErrorMessageResourceType = typeof(strings), ErrorMessage = null)]
		public string Email { get; set; }

		public string Username { get; set; }

		[Required(ErrorMessageResourceName = "Password_Is_Required_Field_Error", ErrorMessageResourceType = typeof(strings))]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; }

		/// <summary>
		/// Is captcha enabled
		/// </summary>
		public bool IsCaptchaEnabled { get; set; }

		/// <summary>
		/// Captach Validation Message
		/// </summary>
		public string CaptchaValidationMessage { get; set; }

		/// <summary>
		/// Is captcha valid
		/// </summary>
		public bool IsCaptchaValid { get; set; }

	}

	public class ResetPasswordViewModel
	{
		[Required(ErrorMessageResourceName = "Password_Is_Required_Field_Error", ErrorMessageResourceType = typeof(strings))]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; }

		public string UserId { get; set; }
		public string Code { get; set; }
	}

	public class ForgotPasswordViewModel
	{
		[Required(ErrorMessageResourceName = "Email_Field_Required_Exception", ErrorMessageResourceType = typeof(strings))]
		[EmailAddress(ErrorMessageResourceName = "Invalid_Email_Message", ErrorMessageResourceType = typeof(strings), ErrorMessage = null)]
		public string Email { get; set; }
	}

	public class RedeemInvitationViewModel
	{
		[Required(ErrorMessageResourceName = "InvitationCode_Required_Exception", ErrorMessageResourceType = typeof(strings))]
		public string InvitationCode { get; set; }

		public bool RedeemByLogin { get; set; }
	}
}
