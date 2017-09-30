/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Profile;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Profile
{
	public class CrmProfileProvider : ProfileProvider
	{
		private string _applicationName;
		private string _attributeMapStateCode;
		private string _attributeMapIsDisabled;
		private string _attributeMapIsAnonymous;
		private string _attributeMapLastActivityDate;
		private string _attributeMapLastUpdatedDate;
		private string _attributeMapUsername;
		private bool _enableActivityTracking;
		private bool _initialized;
		private string _profileEntityName;

		protected static List<string> RequiredCustomAttributes = new List<string>
		{
			"attributeMapIsAnonymous",
			"attributeMapLastActivityDate",
			"attributeMapLastUpdatedDate",
			"attributeMapUsername",
			"profileEntityName"
		};

		public override void Initialize(string name, NameValueCollection config)
		{
			if (_initialized) return;

			if (config == null) throw new ArgumentNullException("config");

			if (string.IsNullOrEmpty(name))
			{
				name = GetType().FullName;
			}

			if (string.IsNullOrEmpty(config["description"]))
			{
				config["description"] = "Adxstudio CRM Profile Provider";
			}

			base.Initialize(name, config);

			AssertRequiredCustomAttributes(config);

			ApplicationName = config["applicationName"] ?? "/";

			_attributeMapStateCode = config["attributeMapStateCode"];

			_attributeMapIsDisabled = config["attributeMapIsDisabled"];

			_attributeMapIsAnonymous = config["attributeMapIsAnonymous"];

			_attributeMapLastActivityDate = config["attributeMapLastActivityDate"];

			_attributeMapLastUpdatedDate = config["attributeMapLastUpdatedDate"];

			_attributeMapUsername = config["attributeMapUsername"];

			ContextName = config["contextName"];

			var enableActivityTrackingConfig = config["enableActivityTracking"];
			bool enableActivityTrackingValue;

			// Profile activity tracking is disabled by default, for performance reasons. Updating the profile
			// activity timestamp on each request generally adds over 100ms--too much.
			_enableActivityTracking = bool.TryParse(enableActivityTrackingConfig, out enableActivityTrackingValue)
				? enableActivityTrackingValue
				: false;

			_profileEntityName = config["profileEntityName"];

			var recognizedAttributes = new List<string>
			{
				"name",
				"applicationName",
				"attributeMapStateCode",
				"attributeMapIsDisabled",
				"attributeMapIsAnonymous",
				"attributeMapLastActivityDate",
				"attributeMapLastUpdatedDate",
				"attributeMapUsername",
				"crmDataContextName",
				"enableActivityTracking",
				"profileEntityName"
			};

			// Remove all of the known configuration values. If there are any left over, they are unrecognized.
			recognizedAttributes.ForEach(config.Remove);

			if (config.Count > 0)
			{
				var unrecognizedAttribute = config.GetKey(0);

				if (!string.IsNullOrEmpty(unrecognizedAttribute))
				{
					throw new ConfigurationErrorsException("The {0} doesn't recognize or support the attribute {1}.".FormatWith(name, unrecognizedAttribute));
				}
			}

			_initialized = true;
		}

		public override string ApplicationName
		{
			get { return _applicationName; }

			set
			{
				if (string.IsNullOrEmpty(value)) throw new ArgumentException("{0} - ApplicationName can't be null or empty.".FormatWith(ToString()));

				if (value.Length > 0x100) throw new ProviderException("{0} - ApplicationName is too long.".FormatWith(ToString()));

				_applicationName = value;
			}
		}

		/// <summary>
		/// The data context name to use to connect to the CRM.
		/// </summary>
		public string ContextName { get; private set; }

		public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new NotSupportedException("Profiles for this provider cannot currently be deleted.");
		}

		public override int DeleteProfiles(ProfileInfoCollection profiles)
		{
			throw new NotSupportedException("Profiles for this provider cannot currently be deleted.");
		}

		public override int DeleteProfiles(string[] usernames)
		{
			throw new NotSupportedException("Profiles for this provider cannot currently be deleted.");
		}

		public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			var wherePredicate = GetUserNameInactiveProfilePredicate(usernameToMatch, userInactiveSinceDate);

			return GetProfiles(wherePredicate, authenticationOption, pageIndex, pageSize, out totalRecords);
		}

		private Expression<Func<Entity, bool>> GetUserNameInactiveProfilePredicate(string usernameToMatch, DateTime userInactiveSinceDate)
		{
			if (!string.IsNullOrWhiteSpace(_attributeMapStateCode))
			{
				return entity => entity.GetAttributeValue<int>(_attributeMapStateCode) == 0
					&& entity.GetAttributeValue<string>(_attributeMapUsername) == usernameToMatch
					&& (
						entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate) == null
						|| entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate) <= userInactiveSinceDate.ToUniversalTime());
			}
			else if (!string.IsNullOrWhiteSpace(_attributeMapIsDisabled))
			{
				return entity => entity.GetAttributeValue<bool>(_attributeMapIsDisabled) == false
					&& entity.GetAttributeValue<string>(_attributeMapUsername) == usernameToMatch
					&& (
						entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate) == null
						|| entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate) <= userInactiveSinceDate.ToUniversalTime());
			}
			else
			{
				return entity => entity.GetAttributeValue<string>(_attributeMapUsername) == usernameToMatch
					&& (
						entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate) == null
						|| entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate) <= userInactiveSinceDate.ToUniversalTime());
			}
		}

		public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			var wherePredicate = GetUserNamePredicate(usernameToMatch);

			return GetProfiles(wherePredicate, authenticationOption, pageIndex, pageSize, out totalRecords);
		}

		private Expression<Func<Entity, bool>> GetUserNamePredicate(string usernameToMatch)
		{
			if (!string.IsNullOrWhiteSpace(_attributeMapStateCode))
			{
				return entity => entity.GetAttributeValue<int>(_attributeMapStateCode) == 0
					&& entity.GetAttributeValue<string>(_attributeMapUsername) == usernameToMatch;
			}
			else if (!string.IsNullOrWhiteSpace(_attributeMapIsDisabled))
			{
				return entity => entity.GetAttributeValue<bool>(_attributeMapIsDisabled) == false
					&& entity.GetAttributeValue<string>(_attributeMapUsername) == usernameToMatch;
			}
			else
			{
				return entity => entity.GetAttributeValue<string>(_attributeMapUsername) == usernameToMatch;
			}
		}

		public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
		{
			var wherePredicate = GetAllProfilesPredicate();

			return GetProfiles(wherePredicate, authenticationOption, pageIndex, pageSize, out totalRecords);
		}

		private Expression<Func<Entity, bool>> GetAllProfilesPredicate()
		{
			if (!string.IsNullOrWhiteSpace(_attributeMapStateCode))
			{
				return entity => entity.GetAttributeValue<int>(_attributeMapStateCode) == 0
					&& entity.GetAttributeValue<string>(_attributeMapUsername) != null;
			}
			else if (!string.IsNullOrWhiteSpace(_attributeMapIsDisabled))
			{
				return entity => entity.GetAttributeValue<bool>(_attributeMapIsDisabled) == false
					&& entity.GetAttributeValue<string>(_attributeMapUsername) != null;
			}
			else
			{
				return entity => entity.GetAttributeValue<string>(_attributeMapUsername) != null;
			}
		}

		public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			var wherePredicate = GetInactiveProfilePredicate(userInactiveSinceDate);

			return GetProfiles(wherePredicate, authenticationOption, pageIndex, pageSize, out totalRecords);
		}

		public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			var wherePredicate = GetInactiveProfilePredicate(userInactiveSinceDate);

			if (authenticationOption != ProfileAuthenticationOption.All)
			{
				wherePredicate = CreateOrModifyWherePredicate(wherePredicate, authenticationOption);
			}

			var context = CrmConfigurationManager.CreateContext(ContextName);

			// NOTE: At the time this was implemented, the CrmQueryProvider was unable to handle the line below.
			// return context.CreateQuery(_profileEntityName).Count(wherePredicate);

			return context.CreateQuery(_profileEntityName).Where(wherePredicate).ToList().Count();
		}

		private Expression<Func<Entity, bool>> GetInactiveProfilePredicate(DateTime userInactiveSinceDate)
		{
			if (!string.IsNullOrWhiteSpace(_attributeMapStateCode))
			{
				return entity => entity.GetAttributeValue<int>(_attributeMapStateCode) == 0
					&& entity.GetAttributeValue<string>(_attributeMapUsername) != null
					&& (
						entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate) == null
						|| entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate) <= userInactiveSinceDate.ToUniversalTime());
			}
			else if (!string.IsNullOrWhiteSpace(_attributeMapIsDisabled))
			{
				return entity => entity.GetAttributeValue<bool>(_attributeMapIsDisabled) == false
					&& entity.GetAttributeValue<string>(_attributeMapUsername) != null
					&& (
						entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate) == null
						|| entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate) <= userInactiveSinceDate.ToUniversalTime());
			}
			else
			{
				return entity => entity.GetAttributeValue<string>(_attributeMapUsername) != null
					&& (
						entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate) == null
						|| entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate) <= userInactiveSinceDate.ToUniversalTime());
			}
		}

		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection propertyCollection)
		{
			var valueCollection = new SettingsPropertyValueCollection();

			if (propertyCollection.Count < 1)
			{
				return valueCollection;
			}

			var username = context["UserName"] as string;

			var xrm = CrmConfigurationManager.CreateContext(ContextName);

			var entity = GetProfileEntity(xrm, username);

			foreach (SettingsProperty property in propertyCollection)
			{
				// NOTE: We just map directly to CRM proerties and ignore the serialization/deserialization capabilities of an individual SettingsPropertyValue.
				property.SerializeAs = SettingsSerializeAs.String;

				var logicalName = GetCustomProviderData(property);

				var value = entity == null ? null : entity.GetAttributeValue(logicalName);

				var settingsPropertyValue = new SettingsPropertyValue(property);

				if (value != null)
				{
					settingsPropertyValue.Deserialized = true;
					settingsPropertyValue.IsDirty = false;
					settingsPropertyValue.PropertyValue = value;
				}

				valueCollection.Add(settingsPropertyValue);
			}

			if (_enableActivityTracking && entity != null)
			{
				entity.SetAttributeValue(_attributeMapLastActivityDate, DateTime.UtcNow);

				xrm.UpdateObject(entity);

				xrm.SaveChanges();
			}

			return valueCollection;
		}

		public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
		{
			var username = context["UserName"] as string;

			var xrm = CrmConfigurationManager.CreateContext(ContextName);

			var entity = GetProfileEntity(xrm, username);

			if (collection.Count < 1 || string.IsNullOrEmpty(username) || entity == null)
			{
				return;
			}

			var userIsAuthenticated = (context["IsAuthenticated"] as bool?).GetValueOrDefault();

			if (!userIsAuthenticated && collection.Cast<SettingsPropertyValue>().Any(propertyValue => (propertyValue.Property.Attributes["AllowAnonymous"] as bool?).GetValueOrDefault()))
			{
				throw new NotSupportedException("Anonymous properties aren't supported.");
			}

			var propertyValuesToUpdate = collection.Cast<SettingsPropertyValue>().Where(value => value.IsDirty && !value.UsingDefaultValue);

			foreach (var propertyValue in propertyValuesToUpdate)
			{
				var logicalName = GetCustomProviderData(propertyValue.Property);

				entity.SetAttributeValue(logicalName, propertyValue.PropertyValue);
			}

			entity.SetAttributeValue(_attributeMapLastActivityDate, DateTime.UtcNow);
			
			entity.SetAttributeValue(_attributeMapLastUpdatedDate, DateTime.UtcNow);

			xrm.UpdateObject(entity);

			xrm.SaveChanges();
		}

		private void AssertRequiredCustomAttributes(NameValueCollection config)
		{
			var requiredCustomAttributesNotFound = RequiredCustomAttributes.Where(attribute => string.IsNullOrEmpty(config[attribute]));

			if (requiredCustomAttributesNotFound.Any())
			{
				throw new ConfigurationErrorsException("The {0} requires the following attribute(s) to be specified:\n{1}".FormatWith(Name, string.Join("\n", requiredCustomAttributesNotFound.ToArray())));
			}
		}

		private Expression<Func<Entity, bool>> CreateOrModifyWherePredicate(Expression<Func<Entity, bool>> wherePredicate, ProfileAuthenticationOption authenticationOption)
		{
			var isAnonymous = authenticationOption == ProfileAuthenticationOption.Anonymous;

			if (wherePredicate == null)
			{
				return entity => entity.GetAttributeValue<bool?>(_attributeMapIsAnonymous).GetValueOrDefault() == isAnonymous;
			}
			
			// Set the wherePredicate so that the clause is equivilant to:
			// entity => wherePreicate.Body && entity.GetAttributeValue<bool?>(_attributeMapIsAnonymous).GetValueOrDefault() == isAnonymous

			var entityParameter = wherePredicate.Parameters.Single();

			Expression getPropertyValue = Expression.Call(entityParameter, "GetPropertyValue", new[] { typeof(bool?) }, Expression.Constant(_attributeMapIsAnonymous));

			var left = Expression.Call(getPropertyValue, typeof(bool?).GetMethod("GetValueOrDefault", Type.EmptyTypes));

			var anonymousPredicateBody = Expression.Equal(left, Expression.Constant(isAnonymous));

			return Expression.Lambda<Func<Entity, bool>>(Expression.AndAlso(wherePredicate.Body, anonymousPredicateBody), entityParameter);
		}

		private static string GetCustomProviderData(SettingsProperty property)
		{
			var value = property.Attributes["CustomProviderData"] as string;

			if (string.IsNullOrEmpty(value))
			{
				throw new NotSupportedException("Add the customProviderData attribute to the profile property {0}. The value should be the logical name of the attribute that will correspond to the property.".FormatWith(property.Name));
			}
			
			return value;
		}

		private Entity GetProfileEntity(OrganizationServiceContext context, string username)
		{
			if (string.IsNullOrEmpty(username)) return null;

			if (!string.IsNullOrWhiteSpace(_attributeMapStateCode))
			{
				return context.CreateQuery(_profileEntityName).SingleOrDefault(
					e => e.GetAttributeValue<int>(_attributeMapStateCode) == 0
					&& e.GetAttributeValue<string>(_attributeMapUsername) == username);
			}

			if (!string.IsNullOrWhiteSpace(_attributeMapIsDisabled))
			{
				return context.CreateQuery(_profileEntityName).SingleOrDefault(
					e => e.GetAttributeValue<bool>(_attributeMapIsDisabled) == false
					&& e.GetAttributeValue<string>(_attributeMapUsername) == username);
			}

			return context.CreateQuery(_profileEntityName).SingleOrDefault(e => e.GetAttributeValue<string>(_attributeMapUsername) == username);
		}

		private ProfileInfoCollection GetProfiles(Expression<Func<Entity, bool>> wherePredicate, ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
		{
			var profileInfoCollection = new ProfileInfoCollection();

			if (authenticationOption != ProfileAuthenticationOption.All)
			{
				wherePredicate = CreateOrModifyWherePredicate(wherePredicate, authenticationOption);
			}

			var context = CrmConfigurationManager.CreateContext(ContextName);

			var entities = context.CreateQuery(_profileEntityName)
				.Where(wherePredicate)
				.OrderBy(entity => entity.GetAttributeValue<string>(_attributeMapUsername))
				.Skip(pageIndex * pageSize)
				.Take(pageSize);

			foreach (var entity in entities)
			{
				var username = entity.GetAttributeValue<string>(_attributeMapUsername);

				if (string.IsNullOrEmpty(username)) continue;

				profileInfoCollection.Add(
					new ProfileInfo(
						username,
						entity.GetAttributeValue<bool?>(_attributeMapIsAnonymous).GetValueOrDefault(),
						entity.GetAttributeValue<DateTime?>(_attributeMapLastActivityDate).GetValueOrDefault(),
						entity.GetAttributeValue<DateTime?>(_attributeMapLastUpdatedDate).GetValueOrDefault(),
						-1));
			}

			totalRecords = context.CreateQuery(_profileEntityName).Where(wherePredicate).ToList().Count();

			return profileInfoCollection;
		}
	}
}
