/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Windows.Navigation;

namespace Microsoft.Xrm.Client.Windows.Controls.ConnectionDialog
{
	///<summary>
	/// Custom Return event.
	///</summary>
	public class ConnectionLauncher : PageFunction<ConnectionResult>
	{
		readonly ConnectionData _connectionData = new ConnectionData { ServerUrl = "https://" };
		///<summary>
		/// Handles the Return event of the ConnectionDialog.
		///</summary>
		public event ConnectionReturnEventHandler ConnectionReturn;

		/// <summary>
		/// Override this method to initialize a <see cref="T:System.Windows.Navigation.PageFunction`1"/> when it is navigated to for the first time.
		/// </summary>
		protected override void Start()
		{
			base.Start();

			KeepAlive = true; // Remember the Connection Finished event registration

			var authentication = new AuthenticationPage(_connectionData);

			authentication.Return += connectionPage_Return;

			if (NavigationService != null)
			{
				NavigationService.Navigate(authentication);
			}
		}

		///<summary>
		/// Notify client that connection dialog has completed.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		///<remarks>We need this custom event because the Return event cannot be registered by window code - if ConnectionDialog registers an event handler with the ConnectionLauncher's Return event, the event is not raised.</remarks>
		public void connectionPage_Return(object sender, ReturnEventArgs<ConnectionResult> e)
		{
			if (ConnectionReturn != null)
			{
				ConnectionReturn(this, new ConnectionReturnEventArgs(e.Result, _connectionData));
			}

			OnReturn(null);
		}
	}
}
