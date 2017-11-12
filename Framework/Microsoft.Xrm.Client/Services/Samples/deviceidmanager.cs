/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// =====================================================================
//
//  This file is part of the Microsoft Dynamics CRM SDK code samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  This source code is intended only as a supplement to Microsoft
//  Development Tools and/or on-line documentation.  See these other
//  materials for detailed information regarding Microsoft code samples.
//
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
//
// =====================================================================
//<snippetDeviceIdManager>
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Xrm.Client.Services.Samples
{
	#region Custom Extensions

	// the customizations are:
	// - new namespace
	// - partial class and members
	// - public to internal classes

	internal static partial class DeviceIdManager
	{
		public const string DevicePrefix = "11";

		public static DeviceRegistrationResponse RegisterDevice(ClientCredentials deviceCredentials)
		{
			return RegisterDevice(Guid.NewGuid(), deviceCredentials);
		}

		private static DeviceRegistrationResponse RegisterDevice(Guid applicationId, ClientCredentials deviceCredentials)
		{
			var userName = new DeviceUserName() { DeviceName = deviceCredentials.UserName.UserName, DecryptedPassword = deviceCredentials.UserName.Password };
			
			var device = new LiveDevice() { User = userName, Version = 1 };

			var request = new DeviceRegistrationRequest(applicationId, device);

			string url = string.Format(CultureInfo.InvariantCulture, LiveIdConstants.RegistrationEndpointUriFormat, string.Empty);

			return ExecuteRegistrationRequest(url, request);
		}

		public static ClientCredentials RegisterDevice(bool persist)
		{
			var persistValue = PersistToFile;

			PersistToFile = persist;

			if (persist)
			{
				// Kill an existing live device file.  RegisterDevice will create new file with new id and password values.

				var file = GetDeviceFile(null);

				if (file.Exists)
				{
					file.Delete();
				}
			}

			var userNameCredentials = GenerateDeviceUserName();
			
			var credentials = RegisterDevice(Guid.NewGuid(), null, userNameCredentials);

			PersistToFile = persistValue;

			return credentials;
		}

		public static void WriteDevice(ClientCredentials deviceCredentials)
		{
			var deviceId = deviceCredentials.UserName.UserName;

			var deviceName = deviceId.StartsWith(DevicePrefix) & deviceId.Length > MaxDeviceNameLength ? deviceId.Substring(DevicePrefix.Length) : deviceId;

			var userName = new DeviceUserName() { DeviceName = deviceName, DecryptedPassword = deviceCredentials.UserName.Password };

			var device = new LiveDevice() { User = userName, Version = 1 };

			var file = GetDeviceFile(null);

			if (file.Exists)
			{
				file.Delete();
			}

			WriteDevice(null, device);
		}
	}

	#endregion

	/// <summary>
	/// Management utility for the Device Id
	/// </summary>
	internal static partial class DeviceIdManager
	{
		#region Fields
		private static readonly Random RandomInstance = new Random();

		public const int MaxDeviceNameLength = 24;
		public const int MaxDevicePasswordLength = 24;
		#endregion

		#region Constructor
		static DeviceIdManager()
		{
			PersistToFile = true;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Indicates whether the registered device credentials should be persisted to the database
		/// </summary>
		public static bool PersistToFile { get; set; }
		#endregion

		#region Methods
		/// <summary>
		/// Loads the device credentials (if they exist).
		/// </summary>
		/// <returns></returns>
		public static ClientCredentials LoadOrRegisterDevice()
		{
			return LoadOrRegisterDevice(null);
		}

		/// <summary>
		/// Loads the device credentials (if they exist).
		/// </summary>
		/// <param name="deviceName">Device name that should be registered</param>
		/// <param name="devicePassword">Device password that should be registered</param>
		public static ClientCredentials LoadOrRegisterDevice(string deviceName, string devicePassword)
		{
			return LoadOrRegisterDevice(null, deviceName, devicePassword);
		}

		/// <summary>
		/// Loads the device credentials (if they exist).
		/// </summary>
		/// <param name="issuerUri">URL for the current token issuer</param>
		/// <remarks>
		/// The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
		/// </remarks>
		public static ClientCredentials LoadOrRegisterDevice(Uri issuerUri)
		{
			return LoadOrRegisterDevice(issuerUri, null, null);
		}

		/// <summary>
		/// Loads the device credentials (if they exist).
		/// </summary>
		/// <param name="issuerUri">URL for the current token issuer</param>
		/// <param name="deviceName">Device name that should be registered</param>
		/// <param name="devicePassword">Device password that should be registered</param>
		/// <remarks>
		/// The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
		/// </remarks>
		public static ClientCredentials LoadOrRegisterDevice(Uri issuerUri, string deviceName, string devicePassword)
		{
			ClientCredentials credentials = LoadDeviceCredentials(issuerUri);
			if (null == credentials)
			{
				credentials = RegisterDevice(Guid.NewGuid(), issuerUri, deviceName, devicePassword);
			}

			return credentials;
		}

		/// <summary>
		/// Registers the given device with Live ID with a random application ID
		/// </summary>
		/// <returns>ClientCredentials that were registered</returns>
		public static ClientCredentials RegisterDevice()
		{
			return RegisterDevice(Guid.NewGuid());
		}

		/// <summary>
		/// Registers the given device with Live ID
		/// </summary>
		/// <param name="applicationId">ID for the application</param>
		/// <returns>ClientCredentials that were registered</returns>
		public static ClientCredentials RegisterDevice(Guid applicationId)
		{
			return RegisterDevice(applicationId, (Uri)null);
		}

		/// <summary>
		/// Registers the given device with Live ID
		/// </summary>
		/// <param name="applicationId">ID for the application</param>
		/// <param name="issuerUri">URL for the current token issuer</param>
		/// <returns>ClientCredentials that were registered</returns>
		/// <remarks>
		/// The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
		/// </remarks>
		public static ClientCredentials RegisterDevice(Guid applicationId, Uri issuerUri)
		{
			return RegisterDevice(applicationId, issuerUri, null, null);
		}

		/// <summary>
		/// Registers the given device with Live ID
		/// </summary>
		/// <param name="applicationId">ID for the application</param>
		/// <param name="deviceName">Device name that should be registered</param>
		/// <param name="devicePassword">Device password that should be registered</param>
		/// <returns>ClientCredentials that were registered</returns>
		public static ClientCredentials RegisterDevice(Guid applicationId, string deviceName, string devicePassword)
		{
			return RegisterDevice(applicationId, (Uri)null, deviceName, devicePassword);
		}

		/// <summary>
		/// Registers the given device with Live ID
		/// </summary>
		/// <param name="applicationId">ID for the application</param>
		/// <param name="issuerUri">URL for the current token issuer</param>
		/// <param name="deviceName">Device name that should be registered</param>
		/// <param name="devicePassword">Device password that should be registered</param>
		/// <returns>ClientCredentials that were registered</returns>
		/// <remarks>
		/// The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
		/// </remarks>
		public static ClientCredentials RegisterDevice(Guid applicationId, Uri issuerUri, string deviceName, string devicePassword)
		{
			if (string.IsNullOrEmpty(deviceName) && !PersistToFile)
			{
				throw new ArgumentNullException("deviceName", "If PersistToFile is false, then deviceName must be specified.");
			}
			else if (string.IsNullOrEmpty(deviceName) != string.IsNullOrEmpty(devicePassword))
			{
				throw new ArgumentNullException("deviceName", "Either deviceName/devicePassword should both be specified or they should be null.");
			}

			DeviceUserName userNameCredentials;
			if (string.IsNullOrEmpty(deviceName))
			{
				userNameCredentials = GenerateDeviceUserName();
			}
			else
			{
				userNameCredentials = new DeviceUserName() { DeviceName = deviceName, DecryptedPassword = devicePassword };
			}

			return RegisterDevice(applicationId, issuerUri, userNameCredentials);
		}

		/// <summary>
		/// Loads the device's credentials from the file system
		/// </summary>
		/// <returns>Device Credentials (if set) or null</returns>
		public static ClientCredentials LoadDeviceCredentials()
		{
			return LoadDeviceCredentials(null);
		}

		/// <summary>
		/// Loads the device's credentials from the file system
		/// </summary>
		/// <param name="issuerUri">URL for the current token issuer</param>
		/// <returns>Device Credentials (if set) or null</returns>
		/// <remarks>
		/// The issuerUri can be retrieved from the IServiceConfiguration interface's CurrentIssuer property.
		/// </remarks>
		public static ClientCredentials LoadDeviceCredentials(Uri issuerUri)
		{
			//If the credentials should not be persisted to a file, then they won't be present on the disk.
			if (!PersistToFile)
			{
				return null;
			}

			string environment = DiscoverEnvironment(issuerUri);

			LiveDevice device = ReadExistingDevice(environment);
			if (null == device || null == device.User)
			{
				return null;
			}

			return device.User.ToClientCredentials();
		}

		/// <summary>
		/// Discovers the Windows Live environment based on the Token Issuer
		/// </summary>
		public static string DiscoverEnvironment(Uri issuerUri)
		{
			if (null == issuerUri)
			{
				return null;
			}

			const string HostSearchString = "login.live";
			if (issuerUri.Host.Length > HostSearchString.Length &&
				issuerUri.Host.StartsWith(HostSearchString, StringComparison.OrdinalIgnoreCase))
			{
				string environment = issuerUri.Host.Substring(HostSearchString.Length);

				if ('-' == environment[0])
				{
					int separatorIndex = environment.IndexOf('.', 1);
					if (-1 != separatorIndex)
					{
						return environment.Substring(1, separatorIndex - 1);
					}
				}
			}

			//In all other cases the environment is either not applicable or it is a production system
			return null;
		}
		#endregion

		#region Private Methods
		private static void Serialize<T>(Stream stream, T value)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(T), string.Empty);

			XmlSerializerNamespaces xmlNamespaces = new XmlSerializerNamespaces();
			xmlNamespaces.Add(string.Empty, string.Empty);

			serializer.Serialize(stream, value, xmlNamespaces);
		}

		private static T Deserialize<T>(Stream stream)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(T), string.Empty);
			return (T)serializer.Deserialize(stream);
		}

		private static FileInfo GetDeviceFile(string environment)
		{
			return new FileInfo(string.Format(CultureInfo.InvariantCulture, LiveIdConstants.LiveDeviceFileNameFormat,
				string.IsNullOrEmpty(environment) ? null : "-" + environment.ToUpperInvariant()));
		}

		private static ClientCredentials RegisterDevice(Guid applicationId, Uri issuerUri, DeviceUserName userName)
		{
			string environment = DiscoverEnvironment(issuerUri);

			LiveDevice device = new LiveDevice() { User = userName, Version = 1 };

			DeviceRegistrationRequest request = new DeviceRegistrationRequest(applicationId, device);

			string url = string.Format(CultureInfo.InvariantCulture, LiveIdConstants.RegistrationEndpointUriFormat,
				string.IsNullOrEmpty(environment) ? null : "-" + environment);

			DeviceRegistrationResponse response = ExecuteRegistrationRequest(url, request);
			if (!response.IsSuccess)
			{
				//If the file is not persisted, the registration will always occur (since the credentials are not
				//persisted to the disk. However, the credentials may already exist. To avoid an exception being continually
				//processed by the calling user, DeviceAlreadyExists will be ignored if the credentials are not persisted to the disk.
				if (!PersistToFile && DeviceRegistrationErrorCode.DeviceAlreadyExists == response.Error.RegistrationErrorCode)
				{
					return device.User.ToClientCredentials();
				}

				throw new DeviceRegistrationFailedException(response.Error.RegistrationErrorCode, response.ErrorSubCode);
			}

			if (PersistToFile)
			{
				WriteDevice(environment, device);
			}

			return device.User.ToClientCredentials();
		}

		private static LiveDevice ReadExistingDevice(string environment)
		{
			//Retrieve the file info
			FileInfo file = GetDeviceFile(environment);
			if (!file.Exists)
			{
				return null;
			}

			using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return Deserialize<LiveDevice>(stream);
			}
		}

		private static void WriteDevice(string environment, LiveDevice device)
		{
			FileInfo file = GetDeviceFile(environment);
			if (!file.Directory.Exists)
			{
				file.Directory.Create();
			}

			using (FileStream stream = file.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None))
			{
				Serialize(stream, device);
			}
		}

		private static DeviceRegistrationResponse ExecuteRegistrationRequest(string url, DeviceRegistrationRequest registrationRequest)
		{
			//Create the request that will submit the request to the server
			WebRequest request = WebRequest.Create(url);
			request.ContentType = "application/soap+xml; charset=UTF-8";
			request.Method = "POST";
			request.Timeout = 180000;

			//Write the envelope to the RequestStream
			using (Stream stream = request.GetRequestStream())
			{
				Serialize(stream, registrationRequest);
			}

			// Read the response into an XmlDocument and return that doc
			try
			{
				using (WebResponse response = request.GetResponse())
				{
					using (Stream stream = response.GetResponseStream())
					{
						return Deserialize<DeviceRegistrationResponse>(stream);
					}
				}
			}
			catch (WebException ex)
			{
				if (null != ex.Response)
				{
					using (Stream stream = ex.Response.GetResponseStream())
					{
						return Deserialize<DeviceRegistrationResponse>(stream);
					}
				}

				throw;
			}
		}

		private static DeviceUserName GenerateDeviceUserName()
		{
			DeviceUserName userName = new DeviceUserName();
			userName.DeviceName = GenerateRandomString(LiveIdConstants.ValidDeviceNameCharacters, MaxDeviceNameLength);
			userName.DecryptedPassword = GenerateRandomString(LiveIdConstants.ValidDevicePasswordCharacters, MaxDevicePasswordLength);

			return userName;
		}

		private static string GenerateRandomString(string characterSet, int count)
		{
			//Create an array of the characters that will hold the final list of random characters
			char[] value = new char[count];

			//Convert the character set to an array that can be randomly accessed
			char[] set = characterSet.ToCharArray();

			lock (RandomInstance)
			{
				//Populate the array with random characters from the character set
				for (int i = 0; i < count; i++)
				{
					value[i] = set[RandomInstance.Next(0, set.Length)];
				}
			}

			return new string(value);
		}
		#endregion

		#region Private Classes
		private static class LiveIdConstants
		{
			public const string RegistrationEndpointUriFormat = @"https://login.live{0}.com/ppsecure/DeviceAddCredential.srf";

			public static readonly string LiveDeviceFileNameFormat = Path.Combine(
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "LiveDeviceID"),
				"LiveDevice{0}.xml");

			public const string ValidDeviceNameCharacters = "0123456789abcdefghijklmnopqrstuvqxyz";

			//Consists of the list of characters specified in the documentation
			public const string ValidDevicePasswordCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^*()-_=+;,./?`~";
		}
		#endregion
	}

	#region Public Classes & Enums
	/// <summary>
	/// Indicates an error during registration
	/// </summary>
	public enum DeviceRegistrationErrorCode
	{
		/// <summary>
		/// Unspecified or Unknown Error occurred
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Interface Disabled
		/// </summary>
		InterfaceDisabled = 1,

		/// <summary>
		/// Invalid Request Format
		/// </summary>
		InvalidRequestFormat = 3,

		/// <summary>
		/// Unknown Client Version
		/// </summary>
		UnknownClientVersion = 4,

		/// <summary>
		/// Blank Password
		/// </summary>
		BlankPassword = 6,

		/// <summary>
		/// Missing Device User Name or Password
		/// </summary>
		MissingDeviceUserNameOrPassword = 7,

		/// <summary>
		/// Invalid Parameter Syntax
		/// </summary>
		InvalidParameterSyntax = 8,

		/// <summary>
		/// Invalid Characters are used in the device credentials.
		/// </summary>
		InvalidCharactersInCredentials = 9,

		/// <summary>
		/// Internal Error
		/// </summary>
		InternalError = 11,

		/// <summary>
		/// Device Already Exists
		/// </summary>
		DeviceAlreadyExists = 13
	}

	/// <summary>
	/// Indicates that Device Registration failed
	/// </summary>
	[Serializable]
	public sealed class DeviceRegistrationFailedException : Exception
	{
		/// <summary>
		/// Construct an instance of the DeviceRegistrationFailedException class
		/// </summary>
		public DeviceRegistrationFailedException()
			: base()
		{
		}

		/// <summary>
		/// Construct an instance of the DeviceRegistrationFailedException class
		/// </summary>
		/// <param name="message">Message to pass</param>
		public DeviceRegistrationFailedException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Construct an instance of the DeviceRegistrationFailedException class
		/// </summary>
		/// <param name="message">Message to pass</param>
		/// <param name="innerException">Exception to include</param>
		public DeviceRegistrationFailedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Construct an instance of the DeviceRegistrationFailedException class
		/// </summary>
		/// <param name="code">Error code that occurred</param>
		/// <param name="subCode">Subcode that occurred</param>
		public DeviceRegistrationFailedException(DeviceRegistrationErrorCode code, string subCode)
			: this(code, subCode, null)
		{
		}

		/// <summary>
		/// Construct an instance of the DeviceRegistrationFailedException class
		/// </summary>
		/// <param name="code">Error code that occurred</param>
		/// <param name="subCode">Subcode that occurred</param>
		/// <param name="innerException">Inner exception</param>
		public DeviceRegistrationFailedException(DeviceRegistrationErrorCode code, string subCode, Exception innerException)
			: base(string.Concat(code.ToString(), ": ", subCode), innerException)
		{
			this.RegistrationErrorCode = code;
		}

		/// <summary>
		/// Construct an instance of the DeviceRegistrationFailedException class
		/// </summary>
		/// <param name="si"></param>
		/// <param name="sc"></param>
		private DeviceRegistrationFailedException(SerializationInfo si, StreamingContext sc)
			: base(si, sc)
		{
		}

		#region Properties
		/// <summary>
		/// Error code that occurred during registration
		/// </summary>
		public DeviceRegistrationErrorCode RegistrationErrorCode { get; private set; }
		#endregion

		#region Methods
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
		#endregion
	}

	#region Serialization Classes
	#region DeviceRegistrationRequest Class
	[EditorBrowsable(EditorBrowsableState.Never)]
	[XmlRoot("DeviceAddRequest")]
	public sealed class DeviceRegistrationRequest
	{
		#region Constructors
		public DeviceRegistrationRequest()
		{
		}

		public DeviceRegistrationRequest(Guid applicationId, LiveDevice device)
			: this()
		{
			if (null == device)
			{
				throw new ArgumentNullException("device");
			}

			this.ClientInfo = new DeviceRegistrationClientInfo() { ApplicationId = applicationId, Version = "1.0" };
			this.Authentication = new DeviceRegistrationAuthentication()
			{
				MemberName = device.User.DeviceId,
				Password = device.User.DecryptedPassword
			};
		}
		#endregion

		#region Properties
		[XmlElement("ClientInfo")]
		public DeviceRegistrationClientInfo ClientInfo { get; set; }

		[XmlElement("Authentication")]
		public DeviceRegistrationAuthentication Authentication { get; set; }
		#endregion
	}
	#endregion

	#region DeviceRegistrationClientInfo Class
	[EditorBrowsable(EditorBrowsableState.Never)]
	[XmlRoot("ClientInfo")]
	public sealed class DeviceRegistrationClientInfo
	{
		#region Properties
		[XmlAttribute("name")]
		public Guid ApplicationId { get; set; }

		[XmlAttribute("version")]
		public string Version { get; set; }
		#endregion
	}
	#endregion

	#region DeviceRegistrationAuthentication Class
	[EditorBrowsable(EditorBrowsableState.Never)]
	[XmlRoot("Authentication")]
	public sealed class DeviceRegistrationAuthentication
	{
		#region Properties
		[XmlElement("Membername")]
		public string MemberName { get; set; }

		[XmlElement("Password")]
		public string Password { get; set; }
		#endregion
	}
	#endregion

	#region DeviceRegistrationResponse Class
	[EditorBrowsable(EditorBrowsableState.Never)]
	[XmlRoot("DeviceAddResponse")]
	public sealed class DeviceRegistrationResponse
	{
		#region Properties
		[XmlElement("success")]
		public bool IsSuccess { get; set; }

		[XmlElement("puid")]
		public string Puid { get; set; }

		[XmlElement("Error")]
		public DeviceRegistrationResponseError Error { get; set; }

		[XmlElement("ErrorSubcode")]
		public string ErrorSubCode { get; set; }
		#endregion
	}
	#endregion

	#region DeviceRegistrationResponse Class
	[EditorBrowsable(EditorBrowsableState.Never)]
	[XmlRoot("Error")]
	public sealed class DeviceRegistrationResponseError
	{
		private string _code;

		#region Properties
		[XmlAttribute("Code")]
		public string Code
		{
			get
			{
				return this._code;
			}

			set
			{
				this._code = value;

				//Parse the error code
				if (!string.IsNullOrEmpty(value))
				{
					//Parse the error code
					if (value.StartsWith("dc", StringComparison.Ordinal))
					{
						int code;
						if (int.TryParse(value.Substring(2), NumberStyles.Integer,
							CultureInfo.InvariantCulture, out code) &&
							Enum.IsDefined(typeof(DeviceRegistrationErrorCode), code))
						{
							this.RegistrationErrorCode = (DeviceRegistrationErrorCode)Enum.ToObject(
								typeof(DeviceRegistrationErrorCode), code);
						}
					}
				}
			}
		}

		[XmlIgnore]
		public DeviceRegistrationErrorCode RegistrationErrorCode { get; private set; }
		#endregion
	}
	#endregion

	#region LiveDevice Class
	[EditorBrowsable(EditorBrowsableState.Never)]
	[XmlRoot("Data")]
	public sealed class LiveDevice
	{
		#region Properties
		[XmlAttribute("version")]
		public int Version { get; set; }

		[XmlElement("User")]
		public DeviceUserName User { get; set; }

		[SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This is required for proper XML Serialization")]
		[XmlElement("Token")]
		public XmlNode Token { get; set; }

		[XmlElement("Expiry")]
		public string Expiry { get; set; }

		[XmlElement("ClockSkew")]
		public string ClockSkew { get; set; }
		#endregion
	}
	#endregion

	#region DeviceUserName Class
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class DeviceUserName
	{
		private string _encryptedPassword;
		private string _decryptedPassword;
		private bool _encryptedValueIsUpdated;

		#region Constants
		private const string UserNamePrefix = "11";
		#endregion

		#region Constructors
		public DeviceUserName()
		{
			this.UserNameType = "Logical";
		}
		#endregion

		#region Properties
		[XmlAttribute("username")]
		public string DeviceName { get; set; }

		[XmlAttribute("type")]
		public string UserNameType { get; set; }

		[XmlElement("Pwd")]
		public string EncryptedPassword
		{
			get
			{
				this.ThrowIfNoEncryption();

				if (!this._encryptedValueIsUpdated)
				{
					this._encryptedPassword = this.Encrypt(this._decryptedPassword);
					this._encryptedValueIsUpdated = true;
				}

				return this._encryptedPassword;
			}

			set
			{
				this.ThrowIfNoEncryption();
				this.UpdateCredentials(value, null);
			}
		}

		public string DeviceId
		{
			get
			{
				return UserNamePrefix + DeviceName;
			}
		}

		[XmlIgnore]
		public string DecryptedPassword
		{
			get
			{
				return this._decryptedPassword;
			}

			set
			{
				this.UpdateCredentials(null, value);
			}
		}

		private bool IsEncryptionEnabled
		{
			get
			{
				//If the object is not going to be persisted to a file, then the value does not need to be encrypted. This is extra
				//overhead and will not function in partial trust.
				return DeviceIdManager.PersistToFile;
			}
		}
		#endregion

		#region Methods
		public ClientCredentials ToClientCredentials()
		{
			ClientCredentials credentials = new ClientCredentials();
			credentials.UserName.UserName = this.DeviceId;
			credentials.UserName.Password = this.DecryptedPassword;

			return credentials;
		}

		private void ThrowIfNoEncryption()
		{
			if (!this.IsEncryptionEnabled)
			{
				throw new NotSupportedException("Not supported when DeviceIdManager.UseEncryptionApis is false.");
			}
		}

		private void UpdateCredentials(string encryptedValue, string decryptedValue)
		{
			bool isValueUpdated = false;
			if (string.IsNullOrEmpty(encryptedValue) && string.IsNullOrEmpty(decryptedValue))
			{
				isValueUpdated = true;
			}
			else if (string.IsNullOrEmpty(encryptedValue))
			{
				if (this.IsEncryptionEnabled)
				{
					encryptedValue = this.Encrypt(decryptedValue);
					isValueUpdated = true;
				}
				else
				{
					encryptedValue = null;
					isValueUpdated = false;
				}
			}
			else
			{
				this.ThrowIfNoEncryption();

				decryptedValue = this.Decrypt(encryptedValue);
				isValueUpdated = true;
			}

			this._encryptedPassword = encryptedValue;
			this._decryptedPassword = decryptedValue;
			this._encryptedValueIsUpdated = isValueUpdated;
		}

		private string Encrypt(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}

			byte[] encryptedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.CurrentUser);
			return Convert.ToBase64String(encryptedBytes);
		}

		private string Decrypt(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}

			byte[] decryptedBytes = ProtectedData.Unprotect(Convert.FromBase64String(value), null, DataProtectionScope.CurrentUser);
			if (null == decryptedBytes || 0 == decryptedBytes.Length)
			{
				return null;
			}

			return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
		}
		#endregion
	}
	#endregion
	#endregion
	#endregion
}
//</snippetDeviceIdManager>
