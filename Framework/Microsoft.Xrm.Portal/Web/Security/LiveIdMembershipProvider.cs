/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Security;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Collections.Generic;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Portal.Web.Security.LiveId;

namespace Microsoft.Xrm.Portal.Web.Security
{
	/// <summary>
	/// A pseudo membership provider to enable the use of some .NET login controls.
	/// </summary>
	/// <remarks>
	/// Configuration format.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <connectionStrings>
	///   <add name="Xrm" connectionString="ServiceUri=...; Domain=...; Username=...; Password=..."/>
	///   <add name="Live" connectionString="Application Id=...; Secret=..."/>
	///  </connectionStrings>
	/// 
	///  <system.web>
	///  
	///   <machineKey validationKey="..." decryptionKey="..." validation="SHA1" decryption="AES"/>
	///   
	///   <membership defaultProvider="Xrm">
	///    <providers>
	///     <add
	///      name="Xrm"
	///      type="Microsoft.Xrm.Portal.Web.Security.LiveIdMembershipProvider"
	///      liveIdConnectionStringName="Live"
	///      connectionStringName="Xrm"
	///      contextName="Xrm" [Microsoft.Xrm.Client.Configuration.OrganizationServiceContextElement]
	///     />
	///    </providers>
	///   </membership>
	///   
	///  </system.web>
	///  
	///  <system.webServer>
	///   <handlers>
	///    <add
	///     name="LiveId" verb="*"
	///     path="LiveId.axd"
	///     type="Microsoft.Xrm.Portal.Web.Handlers.LiveIdWebAuthenticationHandler, Microsoft.Xrm.Portal"
	///     preCondition="integratedMode"/>
	///   </handlers>
	///  </system.webServer>
	/// 
	/// </configuration>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <seealso cref="OrganizationServiceContextElement"/>
	/// <seealso cref="PortalCrmConfigurationManager"/>
	/// <seealso cref="CrmConfigurationManager"/>
	public class LiveIdMembershipProvider : MembershipProvider // MSBug #120050: Won't seal, inheritance is expected extension point.
	{
		private string _applicationName;
		private bool _initialized;

		/// <summary>
		/// The Windows Live Application ID.
		/// </summary>
		public string AppId { get; private set; }

		///<summary>
		///The name of the application using the membership provider.
		///</summary>
		public override string ApplicationName
		{
			get { return _applicationName; }

			set
			{
				if (string.IsNullOrEmpty(value)) throw new ArgumentException("{0} - 'ApplicationName' cannot be null or empty.".FormatWith(ToString()));

				if (value.Length > 0x100) throw new ProviderException("{0} - 'ApplicationName too long".FormatWith(ToString()));

				_applicationName = value;
			}
		}

		/// <summary>
		/// The name of the <see cref="CrmConnection"/> used by the provider.
		/// </summary>
		public string ConnectionStringName { get; private set; }
		
		/// <summary>
		/// The name of the Live ID connection used by the provider.
		/// </summary>
		public string LiveIdConnectionStringName { get; private set; }

		/// <summary>
		/// Accessor to the single available <see cref="LiveIdMembershipProvider"/> provider.
		/// </summary>
		public static LiveIdMembershipProvider Current
		{
			get
			{
				// find the single LiveIdMembershipProvider

				var providers =
					from MembershipProvider provider in Membership.Providers
					where provider is LiveIdMembershipProvider
					select provider as LiveIdMembershipProvider;

				if (providers.Count() > 1)
				{
					throw new ProviderException("Only a single '{0}' may added in the web.config. Remove any extra membership providers.".FormatWith(typeof(LiveIdMembershipProvider)));
				}

				return providers.SingleOrDefault();
			}
		}

		/// <summary>
		/// Initializes the provider with the property values specified in the ASP.NET application's configuration file.
		/// </summary>
		public override void Initialize(string name, NameValueCollection config)
		{
			if (_initialized) return;

			config.ThrowOnNull("config");

			if (string.IsNullOrEmpty(name))
			{
				name = GetType().FullName;
			}

			if (string.IsNullOrEmpty(config["description"]))
			{
				config["description"] = "Windows Live Id Membership Provider";
			}

			base.Initialize(name, config);

			ApplicationName = config["applicationName"] ?? Utility.GetDefaultApplicationName();

			var connectionStringName = config["connectionStringName"];
			var contextName = config["contextName"];

			if (!string.IsNullOrWhiteSpace(connectionStringName))
			{
				ConnectionStringName = connectionStringName;
			}
			else if (!string.IsNullOrWhiteSpace(contextName))
			{
				ConnectionStringName = CrmConfigurationManager.GetConnectionStringNameFromContext(contextName);
			}
			else if (CrmConfigurationManager.CreateConnectionStringSettings(connectionStringName) != null)
			{
				ConnectionStringName = name;
			}
			else
			{
				ConnectionStringName = CrmConfigurationManager.GetConnectionStringNameFromContext(name, true);
			}

			if (string.IsNullOrEmpty(ConnectionStringName))
			{
				throw new ConfigurationErrorsException("One of 'connectionStringName' or 'contextName' must be specified on the '{0}' named '{1}'.".FormatWith(this, name));
			}

			LiveIdConnectionStringName = config["liveIdConnectionStringName"];

			if (string.IsNullOrEmpty(LiveIdConnectionStringName))
			{
				throw new ConfigurationErrorsException("The 'liveIdConnectionStringName' must be specified on the '{0}' named '{1}'.".FormatWith(this, name));
			}

			var parameters = LiveIdConnectionStringName.ToDictionaryFromConnectionStringName();

			AppId = parameters.FirstNotNullOrEmpty("AppId", "Application Id", "ApplicationId");

			// Remove all of the known configuration values. If there are any left over, they are unrecognized.
			config.Remove("name");
			config.Remove("applicationName");
			config.Remove("contextName");
			config.Remove("connectionStringName");
			config.Remove("liveIdConnectionStringName");

			if (config.Count > 0)
			{
				string unrecognizedAttribute = config.GetKey(0);

				if (!string.IsNullOrEmpty(unrecognizedAttribute))
				{
					throw new ConfigurationErrorsException("The '{0}' named '{1}' does not currently recognize or support the attribute '{2}'.".FormatWith(this, name, unrecognizedAttribute));
				}
			}

			_initialized = true;
		}

		#region Members Not Implemented

		public override bool EnablePasswordRetrieval { get { return false; } }
		public override bool EnablePasswordReset { get { return false; } }
		public override bool RequiresQuestionAndAnswer { get { return false; } }
		public override int MaxInvalidPasswordAttempts { get { throw new NotImplementedException(); } }
		public override int PasswordAttemptWindow { get { throw new NotImplementedException(); } }
		public override MembershipPasswordFormat PasswordFormat { get { throw new NotImplementedException(); } }
		public override int MinRequiredPasswordLength { get { return 0; } }
		public override int MinRequiredNonAlphanumericCharacters { get { return 0; } }
		public override string PasswordStrengthRegularExpression { get { throw new NotImplementedException(); } }
		public override bool RequiresUniqueEmail { get { return false; } }

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			throw new NotImplementedException();
		}

		public override string GetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override int GetNumberOfUsersOnline()
		{
			throw new NotImplementedException();
		}

		public override string GetUserNameByEmail(string email)
		{
			throw new NotImplementedException();
		}

		public override string ResetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override bool UnlockUser(string userName)
		{
			throw new NotImplementedException();
		}

		#endregion

		/// <summary>
		/// Creates the Live ID user and authenticates with forms authentication if <paramref name="setUserOnline"/> is true.
		/// </summary>
		/// <param name="token">The token returned from Live ID that contains information about the user.</param>
		/// <param name="setUserOnline">If true, the user will be authenticated once created.</param>
		/// <returns>
		/// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the information for the newly created user.
		/// </returns>
		public MembershipUser CreateUser(string token, bool setUserOnline)
		{
			var user = GetWindowsLiveLoginUser(token);

			if (user == null)
			{
				return null;
			}

			MembershipCreateStatus status;

			var membershipUser = CreateUser(user.Id, null, null, null, null, true, null, out status);

			if (membershipUser == null) throw new MembershipCreateUserException(status);

			if (setUserOnline)
			{
				FormsAuthentication.SetAuthCookie(membershipUser.UserName, user.UsePersistentCookie);
			}

			return membershipUser;
		}

		/// <summary>
		/// Adds a new membership user to the data source.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Web.Security.MembershipUser" /> object populated with the information for the newly created user.
		/// </returns>
		/// <param name="username">The user name for the new user.</param>
		/// <param name="password">NOT SUPPORTED.</param>
		/// <param name="email">NOT SUPPORTED.</param>
		/// <param name="passwordQuestion">NOT SUPPORTED.</param>
		/// <param name="passwordAnswer">NOT SUPPORTED.</param>
		/// <param name="isApproved">NOT SUPPORTED.</param>
		/// <param name="providerUserKey">NOT SUPPORTED.</param>
		/// <param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus" /> enumeration value indicating whether the user was created successfully.</param>
		public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			if (string.IsNullOrEmpty(username))
			{
				status = MembershipCreateStatus.InvalidUserName;
				return null;
			}

			var user = new LiveIdUser(ConnectionStringName, username) { Approved = isApproved };

			user.Save();

			status = MembershipCreateStatus.Success;

			return new MembershipUser(Name, username, username, string.Empty, string.Empty, string.Empty, isApproved, false, user.CreatedAt.Value, user.LastLogin, user.LastLogin, DateTime.MinValue, DateTime.MinValue);
		}

		/// <summary>
		/// Removes a user from the membership data source. 
		/// </summary>
		/// <returns>
		/// true if the user was successfully deleted; otherwise, false.
		/// </returns>
		/// <param name="username">The name of the user to delete.</param>
		/// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			LiveIdUser.Delete(username, ConnectionStringName);

			return LiveIdUser.GetByUserId(username, ConnectionStringName) == null;
		}

		/// <summary>
		/// Gets the Passport Unique Identifier from a Windows Live ID token.
		/// </summary>
		public string GetPuid(string token)
		{
			var user = GetWindowsLiveLoginUser(token);
			
			return user == null ? null : user.Id;
		}

		/// <summary>
		/// Gets user information from the data source based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Web.Security.MembershipUser" /> object populated with the specified user's information from the data source.
		/// </returns>
		/// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
		/// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			if (!(providerUserKey is string)) throw new NotSupportedException();

			return GetUser(providerUserKey as string, userIsOnline);
		}

		/// <summary>
		/// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Web.Security.MembershipUser" /> object populated with the specified user's information from the data source.
		/// </returns>
		/// <param name="username">The name of the user to get information for. </param>
		/// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user. </param>
		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
			if (string.IsNullOrEmpty(username)) return null;

			var user = LiveIdUser.GetByUserId(username, ConnectionStringName);

			if (user == null) return null;

			if (userIsOnline)
			{
				user.LastLogin = DateTime.UtcNow;
				user.Save();
			}

			return new MembershipUser(Name, user.UserId, user.UserId, string.Empty, string.Empty, string.Empty, user.Approved, false, user.CreatedAt.Value, user.LastLogin, user.LastLogin, DateTime.MinValue, DateTime.MinValue);
		}

		/// <summary>
		/// Sets a user as authenticated using forms authentication and the Windows Live ID token.
		/// </summary>
		/// <param name="token">The Windows Live ID token that contains the user to set as authenticated.</param>
		/// <returns>true if successful; otherwise, false</returns>
		public bool SetUserOnline(string token)
		{
			var user = GetWindowsLiveLoginUser(token);

			if (user == null) return false;

			ValidateUser(user.Id, user.Id);

			FormsAuthentication.SetAuthCookie(user.Id, user.UsePersistentCookie);

			return true;
		}

		/// <summary>
		/// Updates information about a user in the data source.
		/// </summary>
		/// <param name="user">A <see cref="T:System.Web.Security.MembershipUser" /> object that represents the user to update and the updated information for the user. </param>
		public override void UpdateUser(MembershipUser user)
		{
			user.ThrowOnNull("user");

			var liveIdUser = LiveIdUser.GetByUserId(user.UserName, ConnectionStringName);

			if (liveIdUser == null) throw new ArgumentException("Unable to find the user with ID '{0}'.".FormatWith(user.UserName));

			liveIdUser.LastLogin = user.LastLoginDate;
			liveIdUser.Approved = user.IsApproved;

			liveIdUser.Save();
		}

		/// <summary>
		/// Verifies that the specified user id exists in the data source and is approved.
		/// </summary>
		/// <returns>
		/// true if the specified username exists and is approved; otherwise, false.
		/// </returns>
		/// <param name="username">The id of the user.</param>
		/// <param name="password">NOT SUPPORTED.</param>
		public override bool ValidateUser(string username, string password)
		{
			username.ThrowOnNullOrWhitespace("username");

			var user = LiveIdUser.GetByUserId(username, ConnectionStringName);

			if (user == null || !user.Approved) return false;

			user.LastLogin = DateTime.UtcNow;

			user.Save();

			return true;
		}

		private WindowsLiveLogin.User GetWindowsLiveLoginUser(string token)
		{
			var user = new WindowsLiveLogin(true).ProcessToken(token);

			if (user == null)
			{
				Tracing.FrameworkError(ToString(), "GetPuid", "The Live ID token was not valid or could not be parsed -- No user created");
				return null;
			}

			return user;
		}
	}
}
