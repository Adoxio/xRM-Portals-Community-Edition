/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.ServiceModel.Description;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;

namespace Microsoft.Xrm.Client.Windows.Controls.ConnectionDialog
{
	/// <summary>
	/// Prompts the user to specify a url to a Microsoft Dynamics CRM server and select the authentication
	/// type and provide the appropriate credentials. A list of organizations is populated to allow the user
	/// to select an organization to connect to.  The dialog returns true if connection was successful and the
	/// resulting connection string can be retrieved from the ConnectionString property of the dialog.
	/// </summary>
	public partial class ConnectionDialog
	{
		///<summary>
		/// Gets or sets the url of the Microsoft Dynamics CRM server
		///</summary>
		public string ServerUrl { get; set; }
		///<summary>
		/// Gets the url of the Microsoft Dynamics CRM server including the organization name
		///</summary>
		public string OrganizationUrl { get; set; }
		///<summary>
		/// Gets or sets the organization name
		///</summary>
		public OrganizationDetail Organization { get; set; }
		///<summary>
		/// Gets or sets the authentication type of the connection to a Microsoft Dynamics CRM server
		///</summary>
		public AuthenticationTypeCode AuthenticationType { get; set; }
		///<summary>
		/// Gets or sets a collection of <see cref="OrganizationDetail"/> objects
		///</summary>
		public OrganizationDetailCollection Organizations { get; set; }
		///<summary>
		/// Gets or sets the connection string used to make a connection to a Microsoft Dynamics CRM server and organization
		///</summary>
		public string ConnectionString { get; set; }

		///<summary>
		/// Constructor for the dialog window that provides ui elements for making a connection to a Microsoft Dynamics CRM server
		///</summary>
		public ConnectionDialog()
		{
			InitializeComponent();

			// Launch the connection dialog
			
			var connectionLauncher = new ConnectionLauncher();

			connectionLauncher.ConnectionReturn += connectionLauncher_ConnectionReturn;

			Navigate(connectionLauncher);
		}

		void connectionLauncher_ConnectionReturn(object sender, ConnectionReturnEventArgs e)
		{
			// Handle connection dialog return

			var connectionData = e.Data as ConnectionData;

			if (connectionData != null)
			{
				ServerUrl = connectionData.ServerUrl;
				OrganizationUrl = connectionData.OrganizationUrl;
				Organization = connectionData.Organization;
				AuthenticationType = connectionData.AuthenticationType;
				ConnectionString = connectionData.ConnectionString;
				Organizations = connectionData.Organizations;
			}

			if (DialogResult == null)
			{
				DialogResult = (e.Result == ConnectionResult.Connected);
			}
		}
	}

	public partial class ConnectionDialog
	{
		private const string LiveIdDevicePrefix = "11";

		private static ClientCredentials GetDeviceCredentials(ConnectionData connectionData)
		{
			if (connectionData.AuthenticationType != AuthenticationTypeCode.LiveId)
			{
				return null;
			}

			var credentials = new ClientCredentials();

			credentials.UserName.UserName = LiveIdDevicePrefix + connectionData.DeviceId;
			credentials.UserName.Password = connectionData.DevicePassword;

			return credentials;
		}

		private static ClientCredentials GetUserCredentials(ConnectionData connectionData)
		{
			var credentials = new ClientCredentials();

			switch (connectionData.AuthenticationType)
			{
				case AuthenticationTypeCode.ActiveDirectory:

					if (connectionData.IntegratedEnabled)
					{
						return null;
					}

					credentials.Windows.ClientCredential = new NetworkCredential(connectionData.Username, connectionData.Password, connectionData.Domain);

					break;

				default:

					credentials.UserName.UserName = connectionData.Username;
					credentials.UserName.Password = connectionData.Password;

					break;
			}

			return credentials;
		}

		///<summary>
		/// Using the <see cref="DiscoveryServiceProxy"/> to attempt to make a connection to a CRM server.
		///</summary>
		///<param name="connectionData"></param>
		///<returns>Returns true if the credentials and server are correct, otherwise false.</returns>
		public static bool TestServerConnection(ConnectionData connectionData)
		{
			var connection = new CrmConnection
			{
				ServiceUri = new Uri(connectionData.ServerUrl),
				ClientCredentials = GetUserCredentials(connectionData),
				DeviceCredentials = GetDeviceCredentials(connectionData),
			};

			return IsServerConnectionValid(connection);
		}

		/// <summary>
		/// Discovers the organizations that the calling user belongs to.
		/// </summary>
		/// <param name="service">A Discovery service proxy instance.</param>
		/// <returns>Array containing detailed information on each organization that 
		/// the user belongs to.</returns>
		private static OrganizationDetailCollection DiscoverOrganizations(IDiscoveryService service)
		{
			try
			{
				var orgRequest = new RetrieveOrganizationsRequest();

				var orgResponse = (RetrieveOrganizationsResponse)service.Execute(orgRequest);

				return orgResponse.Details;
			}
			catch (Exception e)
			{
				Tracing.FrameworkError(typeof(ConnectionDialog).FullName, "RetrieveOrganizationsRequest", "{0}", e);

				throw;
			}
		}

		private static bool IsServerConnectionValid(CrmConnection connection)
		{
			using (var serviceProxy = new DiscoveryService(connection))
			{
				// Obtain organization information from the Discovery service. 

				var orgs = DiscoverOrganizations(serviceProxy);

				return orgs != null && orgs.Count > 0;
			}
		}

		///<summary>
		/// Gets the organizations found for the provided user and device credentials for a CRM server.
		///</summary>
		///<param name="connectionData"></param>
		///<returns>Returns a collection of <see cref="OrganizationDetail"/> if found, otherwise returns null.</returns>
		public static OrganizationDetailCollection GetOrganizations(ConnectionData connectionData)
		{
			try
			{
				var connection = new CrmConnection
				{
					ServiceUri = new Uri(connectionData.ServerUrl),
					ClientCredentials = GetUserCredentials(connectionData),
					DeviceCredentials = GetDeviceCredentials(connectionData),
				};

				return GetOrganizations(connection);
			}
			catch (Exception e)
			{
				Tracing.FrameworkError(typeof(ConnectionDialog).FullName, "GetOrganizations", "{0}", e);

				throw;
			}
		}

		private static OrganizationDetailCollection GetOrganizations(CrmConnection connection)
		{
			using (var serviceProxy = new DiscoveryService(connection))
			{
				// Obtain organization information from the Discovery service. 

				var orgs = DiscoverOrganizations(serviceProxy);

				return orgs;
			}
		}

		///<summary>
		/// Tries to connect to a Microsoft Dynamics CRM server and organization and determine if the connection is valid.
		///</summary>
		///<param name="connectionString"></param>
		///<returns>Returns true if the <see cref="T:Microsoft.Crm.Sdk.Messages.WhoAmIRequest"/><seealso cref="T:Microsoft.Crm.Sdk.Messages.WhoAmIResponse"/> is successfull, otherwise returns false.</returns>
		public static bool TestOrganizationConnection(string connectionString)
		{
			try
			{
				using (var service = new OrganizationService(CrmConnection.Parse(connectionString)))
				{
					var whoami = service.Execute(new OrganizationRequest("WhoAmI"));

					return whoami != null;
				}
			}
			catch (Exception e)
			{
				Tracing.FrameworkError(typeof(ConnectionDialog).FullName, "TestOrganizationConnection", "Connection test failed: {0}", e);

				throw;
			}
		}

		///<summary>
		/// Creates a connection string that can be used to make connections to a Microsoft Dynamics CRM server.
		///</summary>
		///<param name="connection"></param>
		///<returns>A string value of the connection to a CRM server and or organization.</returns>
		public static string GenerateCrmConnectionString(ConnectionData connection)
		{
			if (connection == null || !connection.IsValidForConnectionString)
			{
				return string.Empty;
			}

			var connectionString = string.Format("ServiceUri={0}", connection.OrganizationUrl);

			switch (connection.AuthenticationType)
			{
				case AuthenticationTypeCode.ActiveDirectory:

					connectionString += connection.IntegratedEnabled
						? ";"
						: "; Domain={0}; UserName={1}; Password={2};".FormatWith(connection.Domain, connection.Username, connection.Password);

					break;

				case AuthenticationTypeCode.LiveId:

					connectionString += "; UserName={0}; Password={1}; DeviceID={2}; DevicePassword='{3}';".FormatWith(connection.Username, connection.Password, connection.DeviceId, connection.DevicePassword);

					break;

				default:

					connectionString += "; UserName={0}; Password={1};".FormatWith(connection.Username, connection.Password);

					break;
			}

			return connectionString;
		}

		///<summary>
		/// Creates a connection string that can be added to the configuration file of Xrm Portals.
		///</summary>
		///<param name="connection"></param>
		///<returns>Returns a string value of the connection string for the web.config file of an Xrm Portal.</returns>
		public static string GenerateWebConfigConnectionString(ConnectionData connection)
		{
			if (connection == null || !connection.IsValidForConnectionString)
			{
				return string.Empty;
			}

			var connectionString = @"<add name=""Xrm"" connectionString=""{0}""/>".FormatWith(GenerateCrmConnectionString(connection));

			return connectionString;
		}
	}
}
