/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adxstudio.Xrm.AspNet.Organization;
using Microsoft.AspNet.Identity;

namespace Adxstudio.Xrm.AspNet.Identity
{
	public class CrmEmailTokenProvider<TUser> : EmailTokenProvider<TUser>
		where TUser : CrmUser
	{
		private static readonly string _defaultProcessName = "adx_SendEmailTwoFactorCodeToContact";
		public OrganizationManager OrganizationManager { get; private set; }
		public string ProcessName { get; set; }

		public CrmEmailTokenProvider(OrganizationManager organizationManager)
		{
			OrganizationManager = organizationManager;
		}

		public override async Task NotifyAsync(string token, UserManager<TUser, string> manager, TUser user)
		{
			if (manager == null) { throw new ArgumentNullException("manager"); }

			await OrganizationManager.InvokeProcessAsync(ProcessName ?? _defaultProcessName, user.ContactId, new Dictionary<string, object> { { "Code", token } }).WithCurrentCulture();
		}
	}
}
