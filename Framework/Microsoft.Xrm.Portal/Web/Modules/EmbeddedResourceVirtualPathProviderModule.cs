/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Client.Diagnostics;

namespace Microsoft.Xrm.Portal.Web.Modules
{
	/// <summary>
	/// Enables the <see cref="EmbeddedResourceVirtualPathProvider"/>.
	/// </summary>
	/// <remarks>
	/// The functionality of the <see cref="EmbeddedResourceVirtualPathProvider"/> is replaced by the <see cref="PortalRoutingModule"/> but can still be used
	/// if URL routing is not being used.
	/// </remarks>
	public sealed class EmbeddedResourceVirtualPathProviderModule : IHttpModule
	{
		private static readonly Lazy<EmbeddedResourceVirtualPathProvider> _provider = new Lazy<EmbeddedResourceVirtualPathProvider>(Utility.RegisterEmbeddedResourceVirtualPathProvider);

		public void Dispose() { }

		public void Init(HttpApplication context)
		{
			var provider = _provider.Value;
			Tracing.FrameworkInformation("EmbeddedResourceVirtualPathProviderModule", "Init", "Provider '{0}' registered.", provider);

			context.PostResolveRequestCache += OnMapRequestHandler;
		}

		private static void OnMapRequestHandler(object sender, EventArgs e)
		{
			var context = (sender as HttpApplication).Context;
			var filePath = context.Request.FilePath;

			// look for a matching registered EmbeddedResourceVirtualPathProvider mapping

			var mapping = _provider.Value.Mappings.Match(filePath);

			if (mapping != null && mapping.AllowDefaultDocument)
			{
				// look for a matching resource

				var resource = mapping.FindResource(filePath);

				if (resource != null)
				{
					if (resource.IsDirectory)
					{
						// check if this directory has a default.aspx resource

						var exists = (
							from node in resource.Children
							where string.Equals(node.Name, "default.aspx", StringComparison.InvariantCultureIgnoreCase)
							select node).Any();

						if (exists)
						{
							var path = Path.Combine(filePath, "default.aspx").Replace('\\', '/');

							Tracing.FrameworkInformation("EmbeddedResourceVirtualPathProviderModule", "OnMapRequestHandler", "Transfer: {0}", path);

							context.Response.Redirect(path, true);
						}
					}
				}
			}
		}
	}
}
