/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;

namespace Microsoft.Xrm.Client.Windows.Controls.ConnectionDialog
{
	/// <summary>
	/// Data that is collected by the ConnectionDialog
	/// </summary>
	public class ConnectionData : ObservableObject, IDataErrorInfo
	{
		private string _serverUrl;

		///<summary>
		/// Gets or sets the url of the Microsoft Dynamics CRM server
		///</summary>
		public string ServerUrl
		{
			get { return _serverUrl; }

			set
			{
				RaisePropertyChanging("ServerUrl");
				_serverUrl = value;
				RaisePropertyChanged("ServerUrl");
			}
		}

		///<summary>
		/// Gets the Organization Service Endpoint
		///</summary>
		public string OrganizationUrl
		{
			get
			{
				if (Organization == null)
				{
					return null;
				}

				return Organization.Endpoints[EndpointType.OrganizationService];
			}
		}

		///<summary>
		/// Gets or sets the organization name
		///</summary>
		public OrganizationDetail Organization { get; set; }

		private AuthenticationTypeCode _authenticationType;

		///<summary>
		/// Gets or sets the authentication type of the connection to a Microsoft Dynamics CRM server
		///</summary>
		public AuthenticationTypeCode AuthenticationType
		{
			get { return _authenticationType; }

			set
			{
				RaisePropertyChanging("AuthenticationType");
				_authenticationType = value;
				RaisePropertyChanged("AuthenticationType");
			}
		}

		private string _domain;

		///<summary>
		/// Gets or sets the domain name of the user's account used for Active Directory and Windows Live ID authentication
		///</summary>
		public string Domain
		{
			get { return _domain; }

			set
			{
				RaisePropertyChanging("Domain");
				_domain = value;
				RaisePropertyChanged("Domain");
			}
		}

		private string _username;

		///<summary>
		/// Gets or sets the username of the account used for Active Directory and Windows Live ID authentication
		///</summary>
		public string Username
		{
			get { return _username; }

			set
			{
				RaisePropertyChanging("Username");
				_username = value;
				RaisePropertyChanged("Username");
			}
		}

		private string _password;

		///<summary>
		/// Gets or sets the password of the account used for Active Directory and Windows Live ID authentication
		///</summary>
		public string Password
		{
			get { return _password; }

			set
			{
				RaisePropertyChanging("Password");
				_password = value;
				RaisePropertyChanged("Password");
			}
		}

		private string _formPassword;

		///<summary>
		/// Gets or sets the password of the account used for Active Directory and Windows Live ID authentication
		///</summary>
		public string FormPassword
		{
			get { return _formPassword; }

			set
			{
				RaisePropertyChanging("FormPassword");
				_formPassword = value;
				RaisePropertyChanged("FormPassword");
			}
		}

		private string _deviceId;

		///<summary>
		/// Gets or sets the device id used for Windows Live ID authentication
		///</summary>
		public string DeviceId
		{
			get { return _deviceId; }

			set
			{
				RaisePropertyChanging("DeviceId");
				_deviceId = value;
				RaisePropertyChanged("DeviceId");
			}
		}

		private string _devicePassword;

		///<summary>
		/// Gets or sets the device password used for Windows Live ID authentication
		///</summary>
		public string DevicePassword
		{
			get { return _devicePassword; }

			set
			{
				RaisePropertyChanging("DevicePassword");
				_devicePassword = value;
				RaisePropertyChanged("DevicePassword");
			}
		}

		private bool _integratedEnabled;

		/// <summary>
		/// Gets or sets the flag for integrated authentication.
		/// </summary>
		public bool IntegratedEnabled
		{
			get { return _integratedEnabled; }

			set
			{
				RaisePropertyChanging("IntegratedEnabled");
				_integratedEnabled = value;
				RaisePropertyChanged("IntegratedEnabled");
			}
		}

			///<summary>
		/// Gets or sets the connection string used to make a connection to a Microsoft Dynamics CRM server and organization
		///</summary>
		public string ConnectionString { get; set; }

		///<summary>
		/// Gets or sets a collection of <see cref="OrganizationDetail"/> objects
		///</summary>
		public OrganizationDetailCollection Organizations { get; set; }

		/// <summary>
		/// Gets whether or not this instance contains sufficient data to build a valid <see cref="CrmConnection">connection string</see>.
		/// </summary>
		public bool IsValidForConnectionString
		{
			get { return !string.IsNullOrEmpty(ServerUrl) && Organization != null; }
		}

		public string Error
		{
			get { return null; }
		}

		public string this[string columnName]
		{
			get
			{
				if (columnName == "ServerUrl")
				{
					Uri uri;

					if (!Uri.TryCreate(ServerUrl, UriKind.Absolute, out uri))
					{
						return "The 'Discovery URL' is required.";
					}
				}
				else if (columnName == "AuthenticationType" && AuthenticationType == AuthenticationTypeCode.None)
				{
					return "The 'Authentication Type' is required.";
				}

				if (AuthenticationType == AuthenticationTypeCode.ActiveDirectory)
				{
					if (columnName == "Domain" && string.IsNullOrWhiteSpace(Domain))
					{
						return "The 'Domain' is required.";
					}
				}
				else if (AuthenticationType == AuthenticationTypeCode.LiveId)
				{
					if (columnName == "DeviceId" && string.IsNullOrWhiteSpace(DeviceId))
					{
						return "The 'Device ID' is required.";
					}

					if (columnName == "DevicePassword" && string.IsNullOrWhiteSpace(DevicePassword))
					{
						return "The 'Device Password' is required.";
					}
				}

				if (columnName == "Username" && string.IsNullOrWhiteSpace(Username))
				{
					return "The 'Username' is required.";
				}

				if (columnName == "FormPassword" && string.IsNullOrWhiteSpace(FormPassword))
				{
					return "The 'Password' is required.";
				}

				return null;
			}
		}
	}

	///<summary>
	/// Enumeration used to provide name labels to the <see cref="AuthenticationProviderType"/> enumeration.
	///</summary>
	[TypeConverter(typeof(EnumToStringUsingDescription))]
	public enum AuthenticationTypeCode
	{
		[Description("None")]
		None = 0,

		[Description("Active Directory")]
		ActiveDirectory = 1,

		[Description("Claims/IFD")]
		Federation = 2,

		[Description("Windows Live ID")]
		LiveId = 3,

		[Description("Microsoft Online Services")]
		OnlineFederation = 4,
	}

	/// <summary>
	/// TypeConverter that reads an enums description attribute and use reflection to convert to and from enum value to string.
	/// </summary>
	/// <example>
	/// To use, simply add the TypeConverter Attribute to your enum declaration as follows:
	/// <code>
	/// <![CDATA[
	/// [TypeConverter(typeof(EnumToStringUsingDescription))]
	/// public enum AuthenticationTypeCode
	/// {
	///		[Description("Integrated")]
	///		Integrated = 1,
	///		[Description("Active Directory")]
	///		ActiveDirectory = 2,
	///		[Description("Windows Live ID")]
	///		WindowsLiveID = 3
	/// }
	/// ]]>
	/// </code>
	/// </example>
	public class EnumToStringUsingDescription : TypeConverter
	{
		/// <summary>
		/// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
		/// </summary>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context. </param><param name="sourceType">A <see cref="T:System.Type"/> that represents the type you want to convert from. </param>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return (sourceType.Equals(typeof(Enum)));
		}

		/// <summary>
		/// Returns whether this converter can convert the object to the specified type, using the specified context.
		/// </summary>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context. </param><param name="destinationType">A <see cref="T:System.Type"/> that represents the type you want to convert to. </param>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return (destinationType.Equals(typeof(string)));
		}

		/// <summary>
		/// Converts the given value object to the specified type, using the specified context and culture information.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context. </param><param name="culture">A <see cref="T:System.Globalization.CultureInfo"/>. If null is passed, the current culture is assumed. </param><param name="value">The <see cref="T:System.Object"/> to convert. </param><param name="destinationType">The <see cref="T:System.Type"/> to convert the <paramref name="value"/> parameter to. </param><exception cref="T:System.ArgumentNullException">The <paramref name="destinationType"/> parameter is null. </exception><exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (!destinationType.Equals(typeof(string)))
			{
				throw new ArgumentException(@"Can only convert to string.", "destinationType");
			}

			var name = value.ToString();

			var attrs = value.GetType().GetField(name).GetCustomAttributes(typeof(DescriptionAttribute), false);

			return (attrs.Length > 0) ? ((DescriptionAttribute)attrs[0]).Description : name;
		}
	}

	[Serializable]
	public class ObservableObject : INotifyPropertyChanging, INotifyPropertyChanged
	{
		[field: NonSerialized]
		public event PropertyChangingEventHandler PropertyChanging;

		protected void OnPropertyChanging(PropertyChangingEventArgs args)
		{
			var handler = PropertyChanging;

			if (handler != null)
			{
				handler(this, args);
			}
		}

		protected void RaisePropertyChanging(string propertyName)
		{
			OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
		}

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(PropertyChangedEventArgs args)
		{
			var handler = PropertyChanged;

			if (handler != null)
			{
				handler(this, args);
			}
		}

		protected void RaisePropertyChanged(string propertyName)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}
	}
}
