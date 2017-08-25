/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Newtonsoft.Json;
using Owin;

namespace Adxstudio.Xrm.AspNet.PortalBus
{
	/// <summary>
	/// Settings related to the <see cref="PortalBusMiddleware{TMessage}"/>.
	/// </summary>
	public class PortalBusOptions<TMessage>
	{
		public PathString CallbackPath { get; set; }

		public PortalBusOptions()
		{
			CallbackPath = new PathString("/_services/bus/" + typeof(TMessage).Name);
		}
	}

	/// <summary>
	/// Middleware for handling incoming portal bus messages that are posted to a web application endpoint.
	/// </summary>
	/// <typeparam name="TMessage"></typeparam>
	public class PortalBusMiddleware<TMessage> : OwinMiddleware
	{
		private readonly JsonSerializer _seralizer;
		private readonly PortalBusOptions<TMessage> _options;
		private readonly IDataProtector _protector;

		public PortalBusMiddleware(OwinMiddleware next, IAppBuilder app, PortalBusOptions<TMessage> options)
			: base(next)
		{
			_seralizer = new JsonSerializer();
			_options = options;
			_protector = app.GetDataProtectionProvider().Create("Adxstudio.Xrm.AspNet.PortalBus");
		}

		public override async Task Invoke(IOwinContext context)
		{
			if (Equals(context.Request.Path, _options.CallbackPath))
			{
				if (string.Equals(context.Request.Method, "POST"))
				{
					var message = Deserialize(context) as IPortalBusMessage;

					if (message != null && message.Validate(context, _protector))
					{
						await message.InvokeAsync(context).WithCurrentCulture();

						context.Response.StatusCode = 200;
						return;
					}
				}

				context.Response.StatusCode = 403;
				return;
			}

			await Next.Invoke(context);
		}

		protected virtual TMessage Deserialize(IOwinContext context)
		{
			using (var sr = new StreamReader(context.Request.Body))
			using (var jr = new JsonTextReader(sr))
			{
				return _seralizer.Deserialize<TMessage>(jr);
			}
		}
	}
}
