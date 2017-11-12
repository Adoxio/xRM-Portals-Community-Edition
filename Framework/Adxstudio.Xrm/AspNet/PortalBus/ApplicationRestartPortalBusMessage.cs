/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.PortalBus
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Adxstudio.Xrm.Core.Telemetry;
	using Adxstudio.Xrm.Web;
	using Microsoft.Owin;
	using Microsoft.Xrm.Client.Services.Messages;

	/// <summary>
	/// A portal bus message that restarts the current web application when invoked.
	/// </summary>
	public class ApplicationRestartPortalBusMessage : PortalBusMessage
	{
		private static readonly IDictionary<string, IEnumerable<string>> _entitiesToRestart = new Dictionary<string, IEnumerable<string>>
		{
			{ "adx_sitesetting", GetSiteSettingPatterns().ToArray() },
			{ "adx_websitebinding", new[] { ".*" } },
			{ "adx_website", new[] { ".*" } }
		};

		public override Task InvokeAsync(IOwinContext context)
		{
			WebEventSource.Log.WriteApplicationLifecycleEvent(ApplicationLifecycleEventCategory.Restart, "PortalBusMessage Initiating Restart");

			TelemetryState.ApplicationEndInfo = ApplicationEndFlags.MetadataDriven;
			Web.Extensions.RestartWebApplication();

			return Task.FromResult(0);
		}

		public virtual bool Validate(PluginMessage message)
		{
			IEnumerable<string> patterns;

			if (message.Target == null
				|| string.IsNullOrWhiteSpace(message.Target.LogicalName)
				|| !_entitiesToRestart.TryGetValue(message.Target.LogicalName, out patterns))
			{
				return false;
			}

			return patterns.Contains(".*")
				|| (!string.IsNullOrWhiteSpace(message.Target.Name)
				&& patterns.Any(pattern => Regex.IsMatch(message.Target.Name, pattern)));
		}

		private static IEnumerable<string> GetSiteSettingPatterns()
		{
			#region Authentication
			// Cookies
			// Authentication/ApplicationCookie/AuthenticationType
			// Authentication/TwoFactorCookie/AuthenticationType

			yield return @"Authentication\/(?<type>(Application|TwoFactor)Cookie)\/(?<name>.+)";

			// WsFederation
			// Authentication/WsFederation/<provider>/AuthenticationType

			yield return @"Authentication\/WsFederation\/(?<provider>.+?)\/(?<name>.+)";

			// OAuth2
			// Authentication/OpenAuth/<provider>/AuthenticationType

			yield return @"Authentication\/OpenAuth\/(?<provider>.+?)\/(?<name>.+)";

			// Saml2
			// Authentication/SAML2/<provider>/AuthenticationType

			yield return @"Authentication\/SAML2\/(?<provider>.+?)\/(?<name>.+)";

			// OpenID Connect
			// Authentication/OpenIdConnect/<provider>/AuthenticationType

			yield return @"Authentication\/OpenIdConnect\/(?<provider>.+?)\/(?<name>.+)";
			#endregion Authentication

			yield return @"HTTP\/(?<name>.+)";

			#region Search
			// Search/Enabled
			yield return @"Search\/Enabled";

			// Search/IndexPath or Search/Global/IndexPath
			yield return @"Search\/(?<global>Global\/)?IndexPath";

			// Search/IndexQueryName or Search/Global/IndexQueryName
			yield return @"Search\/(?<global>Global\/)?IndexQueryName";

			#endregion Search
		}
	}
}
