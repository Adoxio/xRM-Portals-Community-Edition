/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Adxstudio.Xrm.Search.Services
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class CrmEntityIndexBuilderService : ICrmEntityIndexBuilderService
	{
		[OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		public bool BuildIndex(string searchProvider)
		{
			var provider = SearchManager.GetProvider(searchProvider);

			using (var builder = provider.GetIndexBuilder())
			{
				builder.BuildIndex();
			}

			return true;
		}
	}
}
