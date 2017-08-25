/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Threading.Tasks;
using Adxstudio.Xrm.AspNet.PortalBus;
using Microsoft.Owin;
using Microsoft.Xrm.Client.Services.Messages;
using Owin;

namespace Adxstudio.Xrm.AspNet.Cms
{
	/// <summary>
	/// Middleware that posts a <see cref="ApplicationRestartPortalBusMessage"/> message to the portal bus when handling incoming messages posted from a CRM plugin.
	/// </summary>
	public class ApplicationRestartPluginMessageMiddleware : PluginMessageMiddleware
	{
		public ApplicationRestartPluginMessageMiddleware(OwinMiddleware next, IAppBuilder app, PluginMessageOptions options)
			: base(next, app, options)
		{
		}

		protected override async Task PostAsync(IOwinContext context, PluginMessage message)
		{
			var restartMessage = new ApplicationRestartPortalBusMessage();

			if (restartMessage.Validate(message))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Post: name={0}", message.Target.Name));

				await PortalBusManager<ApplicationRestartPortalBusMessage>.PostAsync(context, new ApplicationRestartPortalBusMessage()).WithCurrentCulture();
			}
		}
	}
}
