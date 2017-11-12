/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	/// <summary>
	/// Routes elFinder server-side connector requests to the appropriate handler for a given command.
	/// </summary>
	public class HttpCommandRouter
	{
		/// <summary>
		/// Lookup for command handlers supported by this implementation.
		/// </summary>
		private static readonly IDictionary<string, ICommand> _commands = new Dictionary<string, ICommand>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "open", new OpenCommand() },
			{ "upload", new UploadCommand() },
			{ "ping", new PingCommand() },
			{ "rm", new RmCommand() },
			{ "tmb", new ThumbnailCommand() },
		};

		/// <summary>
		/// List of all valid elFinder (1.2) service commands.
		/// </summary>
		private static readonly string[] _validCommandNames = new[]
		{
			"open",
			"mkdir",
			"mkfile",
			"rename",
			"upload",
			"ping",
			"paste",
			"rm",
			"duplicate",
			"read",
			"edit",
			"extract",
			"archive",
			"tmb",
			"resize"
		};

		private readonly string _portalName;
		private readonly RequestContext _requestContext;

		public HttpCommandRouter(string portalName, RequestContext requestContext)
		{
			_portalName = portalName;
			_requestContext = requestContext;
		}

		public IEnumerable<string> DisabledCommands
		{
			get { return _validCommandNames.Except(SupportedCommands, StringComparer.InvariantCulture); }
		}

		public IEnumerable<string> SupportedCommands
		{
			get { return _commands.Select(pair => pair.Key); }
		}

		public IPortalContext GetPortalContext()
		{
			return PortalCrmConfigurationManager.CreatePortalContext(_portalName, _requestContext);
		}

		public CommandResponse GetResponse(HttpContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var commandName = context.Request.Params["cmd"];

			ICommand command;

			if (commandName == null || !_commands.TryGetValue(commandName, out command))
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Value {0} for parameter cmd isn't a supported command name.", commandName));
			}

			return command.GetResponse(new HttpCommandContext(_portalName, _requestContext, context.Request, DisabledCommands));
		}
	}
}
