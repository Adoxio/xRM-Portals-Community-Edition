/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using System.ServiceModel.Description;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Xrm.Client.Services.Samples;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;

namespace Microsoft.Xrm.Client.Windows.Controls.ConnectionDialog
{
	/// <summary>
	/// Interaction logic for AuthenticationPage.xaml
	/// </summary>
	public partial class AuthenticationPage
	{
		private static readonly string _discoveryUri = "/XRMServices/2011/Discovery.svc";

		private readonly ConnectionData _connectionData;

		///<summary>
		/// Constructor for the page that provides ui elements for CRM server authentication
		///</summary>
		///<param name="connectionData"></param>
		public AuthenticationPage(ConnectionData connectionData)
		{
			InitializeComponent();

			DataContext = _connectionData = connectionData;
		}

		private void ConnectToServer(ConnectionData connectionData)
		{
			var progress = new ProgressDialog
			{
				Title = "Connect to Microsoft Dynamics CRM Server",
				Indeterminate = true,
				CaptionText = "Connecting to Microsoft Dynamics CRM server...",
				Owner = (NavigationWindow)Parent
			};

			var worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

			progress.Cancel += GetCancel(worker);

			worker.DoWork += DoWork;

			worker.RunWorkerCompleted += (sender, args) => { progress.Close(); OnWorkerCompleted(args); };

			worker.RunWorkerAsync(connectionData);

			progress.ShowDialog();
		}
		
		private static void DoWork(object sender, DoWorkEventArgs e)
		{
			// connect to CRM server

			var connectionData = e.Argument as ConnectionData;

			if (connectionData == null)
			{
				throw new Exception("Connection String must not be null.");
			}

			// copy the password over to a new property
			// this is necessary because the form will automatically blank out the password field

			connectionData.Password = connectionData.FormPassword;

			connectionData.Organizations = ConnectionDialog.GetOrganizations(connectionData);
		}

		private void OnWorkerCompleted(RunWorkerCompletedEventArgs args)
		{
			if (args.Error != null)
			{
				var errorPage = new ErrorPage(_connectionData)
				{
					CaptionText = "Failed to connect to your Microsoft Dynamics CRM server.",
					DetailsText = args.Error.Message,
					DetailsIsVisible = true
				};

				errorPage.Return += connectionPage_Return;

				if (NavigationService != null)
				{
					NavigationService.Navigate(errorPage);
				}
			}
			else if (args.Cancelled)
			{
				var errorPage = new ErrorPage(_connectionData)
				{
					CaptionText = "User canceled the process connecting to Microsoft Dynamics CRM server."
				};

				errorPage.Return += connectionPage_Return;

				if (NavigationService != null)
				{
					NavigationService.Navigate(errorPage);
				}
			}
			else
			{
				//process completed successfully

				if (_connectionData.Organizations == null)
				{
					var errorPage = new ErrorPage(_connectionData)
					{
						CaptionText = "There were no organizations found on the server specified.",
					};

					errorPage.Return += connectionPage_Return;

					if (NavigationService != null)
					{
						NavigationService.Navigate(errorPage);
					}
				}
				else
				{
					var organizationPage = new OrganizationPage(_connectionData);

					organizationPage.Return += connectionPage_Return;

					if (NavigationService != null)
					{
						NavigationService.Navigate(organizationPage);
					}
				}
			}
		}
		
		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			// Cancel the connection dialog and don't return any data

			OnReturn(new ReturnEventArgs<ConnectionResult>(ConnectionResult.Canceled));
		}

		private void btnConnect_Click(object sender, RoutedEventArgs e)
		{
			ConnectToServer(_connectionData);
		}

		///<summary>
		/// Return event. If returning, connection dialog was completed (finished or canceled),
		/// continue returning to calling page
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		public void connectionPage_Return(object sender, ReturnEventArgs<ConnectionResult> e)
		{
			OnReturn(e);
		}

		private void UpdateDeviceCredentials(ClientCredentials deviceCredentials)
		{
			var deviceId = deviceCredentials.UserName.UserName ?? string.Empty;

			txtDeviceId.Text = deviceId.StartsWith(DeviceIdManager.DevicePrefix) & deviceId.Length > DeviceIdManager.MaxDeviceNameLength ? deviceId.Substring(DeviceIdManager.DevicePrefix.Length) : deviceId;

			txtDevicePassword.Text = deviceCredentials.UserName.Password ?? string.Empty;
		}

		private void btnRegisterDevice_Click(object sender, RoutedEventArgs e)
		{
			RegisterDevice(false);
		}

		private void RegisterDevice(bool persistToFile)
		{
			var progress = new ProgressDialog
			{
				Title = "Device Credentials",
				Indeterminate = true,
				CaptionText = "Generating new credentials and registering device...",
				Owner = (NavigationWindow)Parent
			};

			var worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

			progress.Cancel += GetCancel(worker);

			worker.DoWork += DoWorkRegisterDevice;

			worker.RunWorkerCompleted += (sender, args) => { progress.Close(); OnWorkerCompletedRegisterDevice(args); };

			worker.RunWorkerAsync(persistToFile);

			progress.ShowDialog();
		}

		private static void DoWorkRegisterDevice(object sender, DoWorkEventArgs e)
		{
			var persistToFile = e.Argument is bool && (bool)e.Argument;

			e.Result = DeviceIdManager.RegisterDevice(persistToFile);
		}

		private void OnWorkerCompletedRegisterDevice(RunWorkerCompletedEventArgs args)
		{
			var deviceCredentials = args.Result as ClientCredentials;

			if (args.Error != null)
			{
				var errorPage = new ErrorPage(_connectionData)
				{
					CaptionText = "Failed to register device credentials.",
					DetailsText = args.Error.Message,
					DetailsIsVisible = true
				};

				errorPage.Return += connectionPage_Return;

				if (NavigationService != null)
				{
					NavigationService.Navigate(errorPage);
				}
			}
			else if (!args.Cancelled & deviceCredentials != null)
			{
				UpdateDeviceCredentials(deviceCredentials);

				const string messageBoxText = "New Device ID and Device Password values have been generated and registered.\n\nSave device credentials for future use?";
				const string caption = "Device Credentials";
				const MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;
				const MessageBoxImage messageBoxImage = MessageBoxImage.None;

				var result = MessageBox.Show(messageBoxText, caption, messageBoxButton, messageBoxImage);

				switch (result)
				{
					case MessageBoxResult.Yes:

						WriteDevice(deviceCredentials);

						break;

					case MessageBoxResult.No:

						return;
				}
			}
		}

		private void WriteDevice(ClientCredentials deviceCredentials)
		{
			var progress = new ProgressDialog
			{
				Title = "Device Credentials",
				Indeterminate = true,
				CaptionText = "Saving device credentials...",
				Owner = (NavigationWindow)Parent
			};

			var worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

			progress.Cancel += GetCancel(worker);

			worker.DoWork += DoWorkWriteDevice;

			worker.RunWorkerCompleted += GetWorkerCompleted(progress, "Failed to save device credentials to file.");

			worker.RunWorkerAsync(deviceCredentials);

			progress.ShowDialog();
		}

		private static void DoWorkWriteDevice(object sender, DoWorkEventArgs e)
		{
			var deviceCredentials = e.Argument as ClientCredentials;

			DeviceIdManager.WriteDevice(deviceCredentials);
		}

		private void btnGo_Click(object sender, RoutedEventArgs e)
		{
			var progress = new ProgressDialog
			{
				Title = "Connect to Microsoft Dynamics CRM Server",
				Indeterminate = true,
				CaptionText = "Retrieving authentication settings...",
				Owner = (NavigationWindow)Parent
			};

			var worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

			progress.Cancel += GetCancel(worker);

			worker.DoWork += (s, args) =>
			{
				Uri uri;

				if (Uri.TryCreate(args.Argument as string, UriKind.Absolute, out uri))
				{
					var config = CreateServiceConfiguration(uri);

					if (config != null)
					{
						Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
						{
							_connectionData.AuthenticationType = (AuthenticationTypeCode)(int)config.AuthenticationType;

							_connectionData.IntegratedEnabled = config.AuthenticationType == AuthenticationProviderType.ActiveDirectory;
							_connectionData.Domain = string.Empty;
							_connectionData.Username = string.Empty;
							_connectionData.FormPassword = string.Empty;

							if (config.AuthenticationType == AuthenticationProviderType.LiveId)
							{
								var deviceCredentials = DeviceIdManager.LoadDeviceCredentials();

								if (deviceCredentials != null)
								{
									var deviceId = deviceCredentials.UserName.UserName ?? string.Empty;
									_connectionData.DeviceId = deviceId.StartsWith(DeviceIdManager.DevicePrefix) & deviceId.Length > DeviceIdManager.MaxDeviceNameLength
										? deviceId.Substring(DeviceIdManager.DevicePrefix.Length)
										: deviceId;
									_connectionData.DevicePassword = deviceCredentials.UserName.Password ?? string.Empty;
								}
							}
						}));
					}
				}
			};

			worker.RunWorkerCompleted += GetWorkerCompleted(progress, "Failed to retrieve authentication settings.");

			worker.RunWorkerAsync(txtServerUrl.Text);

			progress.ShowDialog();
		}

		protected virtual IServiceConfiguration<IDiscoveryService> CreateServiceConfiguration(Uri uri)
		{
			var fullServiceUri = uri.AbsolutePath.EndsWith(_discoveryUri, StringComparison.OrdinalIgnoreCase)
				? uri
				: new Uri(uri, uri.AbsolutePath.TrimEnd('/') + _discoveryUri);

			var config = ServiceConfigurationFactory.CreateConfiguration<IDiscoveryService>(fullServiceUri);

			return config;
		}

		private RunWorkerCompletedEventHandler GetWorkerCompleted(ProgressDialog progress, string captionText)
		{
			return (sender, args) =>
			{
				progress.Close();

				if (args.Error != null)
				{
					var errorPage = new ErrorPage(_connectionData)
					{
						CaptionText = captionText,
						DetailsText = args.Error.Message,
						DetailsIsVisible = true
					};

					errorPage.Return += connectionPage_Return;

					if (NavigationService != null)
					{
						NavigationService.Navigate(errorPage);
					}
				}
			};
		}

		private static EventHandler GetCancel(BackgroundWorker worker)
		{
			return (sender, args) => worker.CancelAsync();
		}
	}
}
