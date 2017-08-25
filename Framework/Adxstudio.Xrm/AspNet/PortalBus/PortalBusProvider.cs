/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Owin;

namespace Adxstudio.Xrm.AspNet.PortalBus
{
	/// <summary>
	/// A service for sending messages to remote instances.
	/// </summary>
	/// <typeparam name="TMessage"></typeparam>
	public interface IPortalBusProvider<in TMessage>
	{
		Task SendAsync(IOwinContext context, TMessage message);
	}

	/// <summary>
	/// A service for sending messages to remote instances.
	/// </summary>
	/// <typeparam name="TMessage"></typeparam>
	public abstract class PortalBusProvider<TMessage> : IPortalBusProvider<TMessage>
	{
		protected IDataProtector Protector { get; private set; }

		protected PortalBusProvider(IAppBuilder app)
		{
			Protector = app.GetDataProtectionProvider().Create("Adxstudio.Xrm.AspNet.PortalBus");
		}

		public virtual async Task SendAsync(IOwinContext context, TMessage message)
		{
			var protectedMessage = message as IPortalBusMessage;

			if (protectedMessage != null)
			{
				protectedMessage.Initialize(context, Protector);
			}

			await SendRemoteAsync(context, message).WithCurrentCulture();
			await SendLocalAsync(context, message).WithCurrentCulture();
		}

		protected virtual async Task SendLocalAsync(IOwinContext context, TMessage message)
		{
			// invoke the message locally

			var protectedMessage = message as IPortalBusMessage;

			if (protectedMessage != null)
			{
				await protectedMessage.InvokeAsync(context).WithCurrentCulture();
			}
		}

		protected abstract Task SendRemoteAsync(IOwinContext context, TMessage message);
	}
}
