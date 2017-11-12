/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Xrm.Sdk.Discovery;

namespace Microsoft.Xrm.Client.Windows.Controls.ConnectionDialog
{
	/// <summary>
	/// Interaction logic for OrganizationPage.xaml
	/// </summary>
	public partial class OrganizationPage
	{
		private BackgroundWorker _worker;
		private ProgressDialog _progress;
		private readonly ConnectionData _connectionData;

		///<summary>
		/// Constructor for the page that provides ui elements to specify the organization to connect to.
		///</summary>
		///<param name="connectionData"></param>
		public OrganizationPage(ConnectionData connectionData)
		{
			InitializeComponent();

			btnConnect.IsEnabled = false;

			DataContext = _connectionData = connectionData;
			
			PopulateOrganizationNames(connectionData);
		}

		private void PopulateOrganizationNames(ConnectionData connectionData)
		{
			cmbOrganizationName.Items.Clear();

			foreach (var org in connectionData.Organizations.OrderBy(o => o.FriendlyName))
			{
				cmbOrganizationName.Items.Add(org);
			}

			if (cmbOrganizationName.Items.Count == 1)
			{
				cmbOrganizationName.SelectedIndex = 0;
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			// Cancel the connection dialog and don't return any data

			OnReturn(new ReturnEventArgs<ConnectionResult>(ConnectionResult.Canceled));
		}

		private void btnConnect_Click(object sender, RoutedEventArgs e)
		{
			ProcessInput();
		}

		private void cmbOrganizationName_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			btnConnect.IsEnabled = cmbOrganizationName.SelectedItem != null;
		}

		private void ProcessInput()
		{
			if (!IsInputValid())
			{
				return;
			}

			_connectionData.Organization = (OrganizationDetail)cmbOrganizationName.SelectedItem;

			var connectionString = ConnectionDialog.GenerateCrmConnectionString(_connectionData);

			_connectionData.ConnectionString = connectionString;

			if (string.IsNullOrEmpty(connectionString))
			{
				throw new NullReferenceException("ErrorPage generating connection string.");
			}

			Connect(connectionString);
		}

		private bool IsInputValid()
		{
			return cmbOrganizationName.SelectedItem != null && !string.IsNullOrEmpty(cmbOrganizationName.SelectedItem.ToString());
		}

		private void Connect(string connection)
		{
			_progress = new ProgressDialog
							{
								Title = "Connect to Microsoft Dynamics CRM Server",
								Indeterminate = true,
								CaptionText = "Connecting to Microsoft Dynamics CRM organization...",
								Owner = (NavigationWindow)Parent
							};

			_progress.Cancel += CancelProcess;

			_worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

			_worker.DoWork += DoWork;

			_worker.RunWorkerCompleted += WorkerCompleted;

			_worker.RunWorkerAsync(connection);

			_progress.ShowDialog();
		}

		private static void DoWork(object sender, DoWorkEventArgs e)
		{
			// Try to connect to CRM server

			var connectionString = e.Argument as string;

			if (connectionString == null)
			{
				throw new Exception("Connection String must not be null.");
			}

			if (!ConnectionDialog.TestOrganizationConnection(connectionString))
			{
				throw new Exception();
			}
		}

		private void CancelProcess(object sender, EventArgs e)
		{
			_worker.CancelAsync();
		}

		private void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_progress.Close();

			if (e.Error != null)
			{
				var errorPage = new ErrorPage(_connectionData)
				{
					CaptionText = "Failed to connect to your Microsoft Dynamics CRM organization.",
					DetailsText = e.Error.Message,
					DetailsIsVisible = true
				};

				errorPage.Return += connectionPage_Return;

				if (NavigationService != null)
				{
					NavigationService.Navigate(errorPage);
				}
			}
			else if (e.Cancelled)
			{
				var errorPage = new ErrorPage(_connectionData)
				{
					CaptionText = "User cancelled the process connecting to Microsoft Dynamics CRM organization."
				};

				errorPage.Return += connectionPage_Return;

				if (NavigationService != null)
				{
					NavigationService.Navigate(errorPage);
				}
			}
			else
			{
				// Finish the connection dialog and return bound data to calling page
				OnReturn(new ReturnEventArgs<ConnectionResult>(ConnectionResult.Connected));
			}
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
	}
}
