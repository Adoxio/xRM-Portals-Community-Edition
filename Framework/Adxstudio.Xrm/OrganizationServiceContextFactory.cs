/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm
{
	internal static class OrganizationServiceContextFactory
	{
		public static CrmOrganizationServiceContext Create(string name = null)
		{
			var context = CrmConfigurationManager.CreateContext(name);
			
			context.MergeOption = MergeOption.NoTracking;

			return context as CrmOrganizationServiceContext;
		}
	}
}
