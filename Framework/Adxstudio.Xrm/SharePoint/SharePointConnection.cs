/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Net;
using Adxstudio.SharePoint.Collections.Generic;
using Adxstudio.SharePoint.Configuration;
using Adxstudio.Xrm.Resources;
using Microsoft.SharePoint.Client;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;

namespace Adxstudio.SharePoint
{
	/// <summary>
	/// The connection settings for building an <see cref="ClientContext"/>.
	/// </summary>
	/// <remarks>
	/// <example>
	/// An example configuration. The connection values should be in the <see cref="DbConnectionStringBuilder"/> format.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	///  <connectionStrings>
	///   <add name="SharePoint" connectionString="Url=hostname; Domain=domain; UserName=username; Password=password; ApplicationName=application name; AuthenticationMode=Default|FormsAuthentication|Anonymous; RequestTimeout=milliseconds; ValidateOnClient=false|true;"/>
	///  </connectionStrings>
	/// </configuration>
	/// ]]>
	/// </code>
	/// </example>
	/// <example>
	/// Creating a connection by configuration.
	/// <code>
	/// var connection = new SharePointConnection("SharePoint");
	/// </code>
	/// </example>
	/// <example>
	/// Creating a connection in code.
	/// <code>
	/// var connection = SharePointConnection.Parse("Url=hostname; Domain=domain; UserName=username; Password=password;");
	/// </code>
	/// </example>
	/// </remarks>
	/// <seealso cref="ClientContext"/>
	public class SharePointConnection
	{
		/// <summary>
		/// Gets or sets the name of the runtime where the current client application is located.
		/// </summary>
		public string ApplicationName { get; set; }

		/// <summary>
		/// Gets or sets the authentication mode for the client context.
		/// </summary>
		public ClientAuthenticationMode? AuthenticationMode { get; set; }

		/// <summary>
		/// Gets or sets the authentication information for the client context.
		/// </summary>
		public ICredentials Credentials { get; set; }

		/// <summary>
		/// Gets or sets the requested time-out value in milliseconds.
		/// </summary>
		public int? RequestTimeout { get; set; }

		/// <summary>
		/// Gets or sets the flag that indicates whether the client library needs to validate the method parameters on the client side.
		/// </summary>
		public bool? ValidateOnClient { get; set; }

		/// <summary>
		/// Gets the URL associated with the runtime context.
		/// </summary>
		public Uri Url { get; set; }

		/// <summary>
		/// The time offset prior to the cookie expiration when the cookies should be refreshed.
		/// </summary>
		public TimeSpan? CookieExpiryWindow { get; set; }

		public SharePointConnection()
			: this("SharePoint")
		{
		}

		public SharePointConnection(string connectionStringName)
			: this(GetConnectionStringSettings(connectionStringName))
		{
		}

		public SharePointConnection(ConnectionStringSettings connectionString)
			: this(SharePointConfigurationManager.CreateConnectionDictionary(connectionString))
		{
		}

		private SharePointConnection(IDictionary<string, string> connection)
			: this(
				connection.FirstNotNullOrEmpty("Url"),
				connection.FirstNotNullOrEmpty("Domain"),
				connection.FirstNotNullOrEmpty("UserName", "User Name", "UserId", "User Id"),
				connection.FirstNotNullOrEmpty("Password"),
				connection.FirstNotNullOrEmpty("ApplicationName", "Application Name"),
				connection.FirstNotNullOrEmpty("AuthenticationMode", "Authentication Mode"),
				connection.FirstNotNullOrEmpty("RequestTimeout", "Request Timeout", "Timeout"),
				connection.FirstNotNullOrEmpty("ValidateOnClient", "Validate On Client"),
				connection.FirstNotNullOrEmpty("CookieExpiryWindow", "Cookie Expiry Window"))
		{
		}

		private SharePointConnection(
			string url,
			string domain, string userName, string password,
			string applicationName,
			string authenticationMode,
			string requestTimeout,
			string validateOnClient,
			string cookieExpiryWindow)
		{
			Url = new Uri(url);
			ApplicationName = applicationName;

			if (!string.IsNullOrWhiteSpace(authenticationMode))
			{
				AuthenticationMode = authenticationMode.ToEnum<ClientAuthenticationMode>();
			}

			if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
			{
				Credentials = new NetworkCredential(userName, password, domain);
			}
			else if (!(string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(password)))
			{
				throw new ConfigurationErrorsException("The specified user credentials are invalid.");
			}

			if (!string.IsNullOrWhiteSpace(requestTimeout))
			{
				RequestTimeout = int.Parse(requestTimeout);
			}

			if (!string.IsNullOrWhiteSpace(validateOnClient))
			{
				ValidateOnClient = bool.Parse(validateOnClient);
			}

			CookieExpiryWindow = !string.IsNullOrWhiteSpace(cookieExpiryWindow) ? TimeSpan.Parse(cookieExpiryWindow) as TimeSpan? : null;
		}

		private static ConnectionStringSettings GetConnectionStringSettings(string connectionStringName)
		{
			var settings = ConfigurationManager.ConnectionStrings[connectionStringName];

			if (settings == null)
			{
				if (connectionStringName == "SharePoint")
				{
					// Try to use the CRM connection string "Xrm"
					if (ConfigurationManager.ConnectionStrings.Count != 0)
					{
						settings = ConfigurationManager.ConnectionStrings["Xrm"];
					}
					else if (CrmConfigurationManager.GetCrmSection().ConnectionStrings.Count != 0)
					{
						settings = CrmConfigurationManager.GetCrmSection().ConnectionStrings["Xrm"];
					}
				}

				if (settings == null)
				{
					throw new ConfigurationErrorsException("Unable to find a connection string with the name {0}.".FormatWith(connectionStringName));
				}
			}

			return settings;
		}

		/// <summary>
		/// Parses a string in the <see cref="DbConnectionStringBuilder"/> format.
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		public static SharePointConnection Parse(string connectionString)
		{
			return new SharePointConnection(connectionString.ToDictionary());
		}

		/// <summary>
		/// Creates a client context from the current connection.
		/// </summary>
		/// <returns></returns>
		public ClientContext CreateClientContext()
		{
			var context = new ClientContext(Url);

			if (ApplicationName != null) context.ApplicationName = ApplicationName;
			if (AuthenticationMode != null) context.AuthenticationMode = AuthenticationMode.Value;
			if (Credentials != null) context.Credentials = Credentials;
			if (RequestTimeout != null) context.RequestTimeout = RequestTimeout.Value;
			if (ValidateOnClient != null) context.ValidateOnClient = ValidateOnClient.Value;

			// if this context is using default Windows authentication add a WebRequest Header to stop forms auth from potentially interfering.
			if (context.AuthenticationMode == ClientAuthenticationMode.Default)
			{
				context.ExecutingWebRequest += ClientContext_ExecutingWebRequest;
			}

			return context;
		}

		/// <summary>
		/// Adds a WebRequest Header to disable forms auth.
		/// </summary>
		private static void ClientContext_ExecutingWebRequest(object sender, WebRequestEventArgs e)
		{
			e.WebRequestExecutor.WebRequest.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
		}

		/// <summary>
		/// Builds a text value that is unique to the connection values.
		/// </summary>
		/// <returns></returns>
		public string GetConnectionId()
		{
			var username = GetUserName(Credentials);
			var text = "Url={0};UserName={1};ApplicationName={2};AuthenticationMode={3};RequestTimeout={4};ValidateOnClient={5};CookieExpiryWindow={6};".FormatWith(
				Url, username, ApplicationName, AuthenticationMode, RequestTimeout, ValidateOnClient, CookieExpiryWindow);
			return text;
		}

		private string GetUserName(ICredentials credentials)
		{
			if (credentials == null) return null;

			var authType = AuthenticationMode != null ? AuthenticationMode.Value : ClientAuthenticationMode.Default;
			var nc = credentials.GetCredential(Url, authType.ToString());

			if (nc == null) return null;
			return nc.Domain + (!string.IsNullOrWhiteSpace(nc.Domain) ? @"\" : string.Empty) + nc.UserName;
		}
	}
}
