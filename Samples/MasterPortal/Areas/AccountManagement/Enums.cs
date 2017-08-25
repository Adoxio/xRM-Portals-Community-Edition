/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.AccountManagement
{
	public class Enums
	{
		public enum ContactAccessScope
		{
			Self = 1,
			Account = 2
		}

		public enum OpportunityAccessScope
		{
			Self = 100000000,
			Account = 100000001
		}

		public enum AzureADGraphAuthResults
		{
			NoErrors,
			UserNotFound,
			UserHasNoEmail,
			NoValidLicense,
			AuthConfigProblem,
			UnknownError
		}

		public enum RedirectTo
		{
			Redeem = 1,
			Profile = 2,
			Local = 3
		}
	}
}
