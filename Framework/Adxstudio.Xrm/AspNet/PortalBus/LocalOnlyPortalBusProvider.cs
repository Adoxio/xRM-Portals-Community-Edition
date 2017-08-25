/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

namespace Adxstudio.Xrm.AspNet.PortalBus
{
	/// <summary>
	/// A portal bus that only invokes posted messages on the local application.
	/// </summary>
	/// <typeparam name="TMessage"></typeparam>
	public class LocalOnlyPortalBusProvider<TMessage> : PortalBusProvider<TMessage>
	{
		public LocalOnlyPortalBusProvider(IAppBuilder app)
			: base(app)
		{
		}

		protected override Task SendRemoteAsync(IOwinContext context, TMessage message)
		{
			// do nothing

			return Task.FromResult(0);
		}
	}
}
