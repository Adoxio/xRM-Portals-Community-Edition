/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Client.Services.Messages;

namespace Adxstudio.Xrm.Services
{
	/// <summary>
	/// Extends functionality of an <see cref="IOrganizationServiceCache"/> provider.
	/// </summary>
	internal interface IExtendedOrganizationServiceCache : IOrganizationServiceCache
	{
		/// <summary>
		/// Locally Removes cache items based on a message description.
		/// </summary>
		void RemoveLocal(OrganizationServiceCachePluginMessage message);
	}

	internal static class OrganizationServiceCacheExtensions
	{
		public static void ExtendedRemoveLocal(this IOrganizationServiceCache cache, OrganizationServiceCachePluginMessage message)
		{
			// this is an untrusted operation so only attempt a local remove

			var extended = cache as IExtendedOrganizationServiceCache;

			if (extended != null)
			{
				extended.RemoveLocal(message);
			}
		}
	}
}
