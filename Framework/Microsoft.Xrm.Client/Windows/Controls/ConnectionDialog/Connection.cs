/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Microsoft.Xrm.Client.Windows.Controls.ConnectionDialog
{
	/// <summary>
	/// Represents the method that will handle the Return event of the PageFunction class.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e">Retrun event arguments</param>
	public delegate void ConnectionReturnEventHandler(object sender, ConnectionReturnEventArgs e);

	/// <summary>
	/// The result for the Return event.
	/// </summary>
	public enum ConnectionResult
	{
		///<summary>
		/// Connection Dialog successfully connected
		///</summary>
		Connected,
		///<summary>
		/// Connection Dialog was canceled
		///</summary>
		Canceled
	}

	/// <summary>
	/// Provides result and data for the Return event.
	/// </summary>
	public class ConnectionReturnEventArgs
	{
		/// <summary>
		/// Method passes the result and data to the Return event.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="data"></param>
		public ConnectionReturnEventArgs(ConnectionResult result, object data)
		{
			Result = result;

			Data = data;
		}

		///<summary>
		/// Result of the dialog passed to the return event
		///</summary>
		public ConnectionResult Result { get; private set; }

		///<summary>
		/// Data passed to the return event
		///</summary>
		public object Data { get; private set; }
	}
}
