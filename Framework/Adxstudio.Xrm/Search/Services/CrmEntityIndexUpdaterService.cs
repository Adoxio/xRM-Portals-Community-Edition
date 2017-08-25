/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Adxstudio.Xrm.Search.Index;

namespace Adxstudio.Xrm.Search.Services
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class CrmEntityIndexUpdaterService : ICrmEntityIndexUpdaterService
	{
		[OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		public void DeleteEntity(string entityLogicalName, Guid id, string searchProvider)
		{
			UsingUpdater(searchProvider, updater => updater.DeleteEntity(entityLogicalName, id));
		}

		[OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		public void DeleteEntitySet(string entityLogicalName, string searchProvider)
		{
			UsingUpdater(searchProvider, updater => updater.DeleteEntitySet(entityLogicalName));
		}

		[OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		public void UpdateEntity(string entityLogicalName, Guid id, string searchProvider)
		{
			UsingUpdater(searchProvider, updater => updater.UpdateEntity(entityLogicalName, id));
		}

		[OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		public void UpdateEntitySet(string entityLogicalName, string searchProvider)
		{
			UsingUpdater(searchProvider, updater => updater.UpdateEntitySet(entityLogicalName));
		}

		private static void UsingUpdater(string searchProvider, Action<ICrmEntityIndexUpdater> action)
		{
			var provider = SearchManager.GetProvider(searchProvider);

			using (var updater = provider.GetIndexUpdater())
			{
				action(updater);
			}
		}
	}
}
