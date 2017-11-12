/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Windows;

namespace Microsoft.Xrm.Client.Windows.Controls.ConnectionDialog
{
	/// <summary>
	/// Interaction logic for ProgressDialog.xaml
	/// </summary>
	public partial class ProgressDialog
	{
		///<summary>
		/// Constructor for the page that provides ui elements to notify the user of the progress of the current activity.
		///</summary>
		public ProgressDialog()
		{
			InitializeComponent();
		}

		///<summary>
		/// Sets the status of the current activity in progress
		///</summary>
		public string CaptionText
		{
			set
			{
				txtCaption.Text = value;
			}
		}

		///<summary>
		/// Sets a text value of the portion/percent completed of the current activity in progress
		///</summary>
		public string ProgressText
		{
			set
			{
				txtPercent.Text = value;
			}
		}

		///<summary>
		/// Sets an integer value of the portion/percent completed of the current activity in progress.
		///</summary>
		public int ProgressValue
		{
			set
			{
				progressBar.Value = value;
			}
		}


		///<summary>
		/// Sets a value that indicates whether the progress bar reports generic progress with a repeating pattern or reports progress based on the Value property.
		///</summary>
		public bool Indeterminate
		{
			set
			{
				progressBar.IsIndeterminate = value;
			}
		}

		///<summary>
		/// Represents the method that will handle a cancel event.
		///</summary>
		public event EventHandler Cancel = delegate { };

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Cancel(sender, e);
		}
	}
}
