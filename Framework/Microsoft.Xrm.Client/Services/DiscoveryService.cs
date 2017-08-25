/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Runtime;
using Microsoft.Xrm.Client.Services.Samples;
using Microsoft.Xrm.Client.Threading;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;

namespace Microsoft.Xrm.Client.Services
{
	/// <summary>
	/// An <see cref="IDiscoveryService"/> that is constructed from a <see cref="CrmConnection"/>.
	/// </summary>
	public class DiscoveryService : IDisposable, IDiscoveryService, IInitializable
	{
		private static readonly string _discoveryUri = "/XRMServices/2011/Discovery.svc";
		private static IServiceConfiguration<IDiscoveryService> _config;
		private static readonly ConcurrentDictionary<string, IServiceConfiguration<IDiscoveryService>> _configLookup = new ConcurrentDictionary<string, IServiceConfiguration<IDiscoveryService>>();
		private static readonly ConcurrentDictionary<string, SecurityTokenResponse> _userTokenLookup = new ConcurrentDictionary<string, SecurityTokenResponse>();
		private readonly Lazy<IDiscoveryService> _service;

		internal static void Reset()
		{
			_config = null;
			_configLookup.Clear();
			_userTokenLookup.Clear();
		}

		/// <summary>
		/// The nested proxy service.
		/// </summary>
		public IDiscoveryService InnerService
		{
			get { return _service.Value; }
		}

		public DiscoveryService(string connectionStringName)
			: this(new CrmConnection(connectionStringName))
		{
		}

		public DiscoveryService(CrmConnection connection)
		{
			_service = new Lazy<IDiscoveryService>(() => ToDiscoveryService(connection));
		}

		public DiscoveryService(IDiscoveryService service)
		{
			_service = new Lazy<IDiscoveryService>(() => service);
		}

		/// <summary>
		/// Initializes custom settings.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="config"></param>
		public virtual void Initialize(string name, NameValueCollection config)
		{
		}

		protected virtual IDiscoveryService ToDiscoveryService(CrmConnection connection)
		{
			var service = ToDiscoveryServiceProxy(connection);
			return service;
		}

		protected virtual DiscoveryServiceProxy ToDiscoveryServiceProxy(CrmConnection connection)
		{
			connection.ThrowOnNull("connection");
			if (connection.ServiceUri == null) throw new ConfigurationErrorsException("The connection's 'ServiceUri' must be specified.");

			var clientCredentials = connection.ClientCredentials;

			DiscoveryServiceProxy service;

			// retrieve the ServiceConfiguration from cache

			var config = GetServiceConfiguration(connection);

			var isClaimsMode = config.AuthenticationType == AuthenticationProviderType.Federation
				|| config.AuthenticationType == AuthenticationProviderType.OnlineFederation
				|| config.AuthenticationType == AuthenticationProviderType.LiveId;

			if (isClaimsMode && clientCredentials != null)
			{
				// get the user token for claims authentication

				var userTokenResponse = GetUserTokenResponse(connection, config);

				Assert(userTokenResponse != null && userTokenResponse.Token != null, "The user authentication failed!");

				service = new DiscoveryServiceProxy(config, userTokenResponse);
			}
			else
			{
				// AD authentication

				service = new DiscoveryServiceProxy(config, clientCredentials);
			}

			if (connection.Timeout != null) service.Timeout = connection.Timeout.Value;

			return service;
		}

		protected virtual IServiceConfiguration<IDiscoveryService> GetServiceConfiguration(CrmConnection connection)
		{
			var mode = connection.ServiceConfigurationInstanceMode;

			if (mode == ServiceConfigurationInstanceMode.Static)
			{
				return _config ?? (_config = CreateServiceConfiguration(connection));
			}

			if (mode == ServiceConfigurationInstanceMode.PerName)
			{
				var key = connection.GetConnectionId();

				if (!_configLookup.ContainsKey(key))
				{
					_configLookup[key] = CreateServiceConfiguration(connection);
				}

				return _configLookup[key];
			}

			if (mode == ServiceConfigurationInstanceMode.PerRequest && HttpSingleton<IServiceConfiguration<IDiscoveryService>>.Enabled)
			{
				var key = connection.GetConnectionId();

				return HttpSingleton<IServiceConfiguration<IDiscoveryService>>.GetInstance(key, () => CreateServiceConfiguration(connection));
			}

			var config = CreateServiceConfiguration(connection);
			return config;
		}

		protected virtual IServiceConfiguration<IDiscoveryService> CreateServiceConfiguration(CrmConnection connection)
		{
			var uri = connection.ServiceUri;
			var fullServiceUri = uri.AbsolutePath.EndsWith(_discoveryUri, StringComparison.OrdinalIgnoreCase)
				? uri
				: new Uri(uri, uri.AbsolutePath.TrimEnd('/') + _discoveryUri);

			var config = ServiceConfigurationFactory.CreateConfiguration<IDiscoveryService>(fullServiceUri);

			return config;
		}

		private static void Assert(bool condition, string message)
		{
			if (!condition)
			{
				throw new InvalidOperationException(message);
			}
		}

		protected virtual SecurityTokenResponse GetUserTokenResponse(CrmConnection connection, IServiceConfiguration<IDiscoveryService> config)
		{
			var expiryWindow = connection.UserTokenExpiryWindow;

			if (expiryWindow != TimeSpan.Zero)
			{
				var key = connection.GetConnectionId();
				SecurityTokenResponse token;

				// check if the token has expired

				if (!_userTokenLookup.TryGetValue(key, out token) || CheckIfTokenIsExpired(token, expiryWindow))
				{
					token = CreateUserTokenResponse(connection, config);
					_userTokenLookup[key] = token;
				}

				return token;
			}

			var userTokenResponse = CreateUserTokenResponse(connection, config);
			return userTokenResponse;
		}

		protected bool CheckIfTokenIsExpired(SecurityTokenResponse token, TimeSpan? expiryWindow)
		{
			var now = DateTime.UtcNow;
			var duration = token.Token.ValidTo - token.Token.ValidFrom;

			if (expiryWindow == null || duration < expiryWindow.Value) return now >= token.Token.ValidTo;

			var expired = (now + expiryWindow.Value) > token.Token.ValidTo;

			return expired;
		}

		protected virtual SecurityTokenResponse CreateUserTokenResponse(CrmConnection connection, IServiceConfiguration<IDiscoveryService> config)
		{
			var homeRealmUri = connection.HomeRealmUri;
			var clientCredentials = connection.ClientCredentials;
			var deviceCredentials = connection.DeviceCredentials;

			if (clientCredentials == null) throw new ConfigurationErrorsException("The connection's user credentials must be specified.");

			SecurityTokenResponse userTokenResponse;

			if (config.AuthenticationType == AuthenticationProviderType.LiveId)
			{
				if (deviceCredentials == null || deviceCredentials.UserName == null)
				{
					throw new ConfigurationErrorsException("The connection's device credentials must be specified.");
				}

				var deviceUserName = deviceCredentials.UserName.UserName;
				var devicePassword = deviceCredentials.UserName.Password;

				if (string.IsNullOrWhiteSpace(deviceUserName)) throw new ConfigurationErrorsException("The connection's device Id must be specified.");
				if (string.IsNullOrWhiteSpace(devicePassword)) throw new ConfigurationErrorsException("The connection's device password must be specified.");
				if (devicePassword.Length < 6) throw new ConfigurationErrorsException("The connection's device password must be at least 6 characters.");

				// prepend the DevicePrefix to the device Id

				var extendedDeviceCredentials = new ClientCredentials();
				extendedDeviceCredentials.UserName.UserName = DeviceIdManager.DevicePrefix + deviceCredentials.UserName.UserName;
				extendedDeviceCredentials.UserName.Password = deviceCredentials.UserName.Password;

				SecurityTokenResponse deviceTokenResponse;

				try
				{
					deviceTokenResponse = config.AuthenticateDevice(extendedDeviceCredentials);
				}
				catch (MessageSecurityException)
				{
					// try register the device credentials

					deviceTokenResponse = RegisterDeviceCredentials(deviceCredentials)
						// try re-authenticate
						? config.AuthenticateDevice(extendedDeviceCredentials)
						: null;
				}

				Assert(deviceTokenResponse != null && deviceTokenResponse.Token != null, "The device authentication failed!");

				userTokenResponse = config.Authenticate(clientCredentials, deviceTokenResponse);
			}
			else
			{
				if (homeRealmUri != null)
				{
					var appliesTo = config.PolicyConfiguration.SecureTokenServiceIdentifier;
					var homeRealmSecurityTokenResponse = config.AuthenticateCrossRealm(clientCredentials, appliesTo, homeRealmUri);

					Assert(homeRealmSecurityTokenResponse != null && homeRealmSecurityTokenResponse.Token != null, "The user authentication failed!");

					userTokenResponse = config.Authenticate(homeRealmSecurityTokenResponse.Token);
				}
				else
				{
					userTokenResponse = config.Authenticate(clientCredentials);
				}
			}

			return userTokenResponse;
		}

		protected bool RegisterDeviceCredentials(ClientCredentials deviceCredentials)
		{
			var response = DeviceIdManager.RegisterDevice(deviceCredentials);

			return response.IsSuccess;
		}

		#region IDiscoveryService Members

		public virtual DiscoveryResponse Execute(DiscoveryRequest request)
		{
			return _service.Value.Execute(request);
		}

		#endregion

		#region IDisposable Members

		public virtual void Dispose()
		{
			var service = _service.Value as IDisposable;

			if (service != null)
			{
				service.Dispose();
			}
		}

		#endregion
	}
}
