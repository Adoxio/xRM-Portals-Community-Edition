/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Account
{
	public static class OrganizationServiceContextExtensions
	{
		/// <summary>
		/// Determines if a contact has successfully saved their profile.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="contact"></param>
		/// <returns></returns>
		public static bool ValidateProfileSuccessfullySaved(this OrganizationServiceContext context, Entity contact)
		{
			contact.ThrowOnNull("contact");
			contact.AssertEntityName("contact");
			return ValidateProfileSuccessfullySaved(context, contact.Id);
		}

		/// <summary>
		/// Determines if a contact has successfully saved their profile.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static bool ValidateProfileSuccessfullySaved(this OrganizationServiceContext context, MembershipUser user)
		{
			user.ThrowOnNull("user");

			if (user.ProviderUserKey is Guid)
			{
				return ValidateProfileSuccessfullySaved(context, (Guid)user.ProviderUserKey);
			}

			return false;
		}

		/// <summary>
		/// Determines if a contact has successfully saved their profile.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="contactId"></param>
		/// <returns></returns>
		public static bool ValidateProfileSuccessfullySaved(this OrganizationServiceContext context, Guid contactId)
		{
			var contact = context.RetrieveSingle(
				"contact",
				new[] { "adx_profilemodifiedon", "adx_profilealert" },
				new[] {
					new Condition("statecode", ConditionOperator.Equal, 0),
					new Condition("contactid", ConditionOperator.Equal, contactId)
				});

			return contact != null && contact.GetAttributeValue<DateTime?>("adx_profilemodifiedon") != null && !contact.GetAttributeValue<bool?>("adx_profilealert").GetValueOrDefault();
		}

		/// <summary>
		/// Returns a unique invitation code
		/// </summary>
		/// <returns></returns>
		public static string CreateInvitationCode(this OrganizationServiceContext context)
		{
			//If any changes to this implementation,should also 
			//go to "..\Portals\Framework\Adxstudio.Xrm.Workflow.Invitation\Adxstudio.Xrm.Workflow.Invitation\UpdateInvitationCode.cs"
			byte[] code = new byte[128];
			using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
			{
				rngCsp.GetBytes(code);
			}
			string invitationCode = Convert.ToBase64String(code);
			return Regex.Replace(invitationCode, @"[+/=]", "-");
		}
	}
}
