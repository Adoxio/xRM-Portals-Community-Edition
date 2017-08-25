/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Windows;
using System.Windows.Navigation;

namespace Microsoft.Xrm.Client.Windows.Controls.ConnectionDialog
{
	/// <summary>
	/// Interaction logic for ErrorPage.xaml
	/// </summary>
	public partial class ErrorPage
	{
		///<summary>
		/// Constructor of the page that provides ui elements indicating handled errors.
		///</summary>
		///<param name="connectionData"></param>
		public ErrorPage(ConnectionData connectionData)
		{
			InitializeComponent();

			DataContext = connectionData;
		}

		///<summary>
		/// Sets the page title
		///</summary>
		public string TitleText
		{
			set
			{
				pageTitle.Content = value;
			}
		}

		///<summary>
		/// Sets the page title description
		///</summary>
		public string DescriptionText
		{
			set
			{
				pageDescription.Text = value;
			}
		}

		///<summary>
		/// Sets the page caption friendly error message
		///</summary>
		public string CaptionText
		{
			set
			{
				txtCaption.Text = value;
			}
		}

		///<summary>
		/// Sets the details of the exception
		///</summary>
		public string DetailsText
		{
			set
			{
				txtDetails.Text = value;
			}
		}

		///<summary>
		/// Sets the details box to visible or collapsed
		///</summary>
		public bool DetailsIsVisible
		{
			set 
			{ 
				txtDetails.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
			}
		}
		
		private void btnBack_Click(object sender, RoutedEventArgs e)
		{
			if (NavigationService != null)
			{
				NavigationService.GoBack();
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			// Cancel the connection dialog and don't return any data

			OnReturn(new ReturnEventArgs<ConnectionResult>(ConnectionResult.Canceled));
		}
	}
}
