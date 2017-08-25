/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Adxstudio.Xrm.AspNet.Identity
{
	/// <summary>
	/// A password validator that requries the password to contain at least three out of the four classes:
	/// Upper-case letters, Lower-case letters, Numbers, and Special characters
	/// </summary>
	public class CrmPasswordValidator : PasswordValidator
	{
		/// <summary>
		/// Requires 3 of 4 tests to pass.
		/// </summary>
		public virtual bool EnforcePasswordPolicy { get; set; }

		/// <summary>
		/// Collection of error message text.
		/// </summary>
		public virtual CrmIdentityErrorDescriber IdentityErrors { get; private set; }

		public CrmPasswordValidator(CrmIdentityErrorDescriber identityErrors)
		{
			IdentityErrors = identityErrors;
		}

		public override Task<IdentityResult> ValidateAsync(string item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			var errors = new List<string>();

			if (string.IsNullOrWhiteSpace(item) || item.Length < RequiredLength)
			{
				errors.Add(ToResult(IdentityErrors.PasswordTooShort(RequiredLength)));
			}

			var failsRequireNonLetterOrDigit = item.All(IsLetterOrDigit);

			if (RequireNonLetterOrDigit && failsRequireNonLetterOrDigit)
			{
				errors.Add(ToResult(IdentityErrors.PasswordRequiresNonLetterAndDigit()));
			}

			var failsRequireDigit = item.All(c => !IsDigit(c));

			if (RequireDigit && failsRequireDigit)
			{
				errors.Add(ToResult(IdentityErrors.PasswordRequiresDigit()));
			}

			var failsRequireLowercase = item.All(c => !IsLower(c));

			if (RequireLowercase && failsRequireLowercase)
			{
				errors.Add(ToResult(IdentityErrors.PasswordRequiresLower()));
			}

			var failsRequireUppercase = item.All(c => !IsUpper(c));

			if (RequireUppercase && failsRequireUppercase)
			{
				errors.Add(ToResult(IdentityErrors.PasswordRequiresUpper()));
			}

			var passes = new[] { !failsRequireNonLetterOrDigit, !failsRequireDigit, !failsRequireLowercase, !failsRequireUppercase };
			var passCount = passes.Count(pass => pass);

			if (EnforcePasswordPolicy && passCount < 3)
			{
				errors.Add(ToResult(IdentityErrors.PasswordRequiresThreeClasses()));
			}

			if (errors.Count == 0)
			{
				return Task.FromResult(IdentityResult.Success);
			}

			return Task.FromResult(IdentityResult.Failed(string.Join(" ", errors)));
		}

		private static string ToResult(IdentityError error)
		{
			return error.Description;
		}
	}
}
