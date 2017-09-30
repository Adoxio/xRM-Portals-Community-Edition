/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Identity
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Services.Query;
	using Microsoft.AspNet.Identity;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Cms.SolutionVersions;

	internal static class UserConstants
	{
		public static readonly Relationship ContactExternalIdentityRelationship = new Relationship("adx_contact_externalidentity");

		public static readonly ColumnSet ExternalIdentityAttributes = new ColumnSet("adx_username", "adx_identityprovidername");

		public static readonly EntityNodeColumn[] ContactAttributes =
		{
			new EntityNodeColumn("adx_identity_logonenabled", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_identity_username", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_identity_passwordhash", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_identity_securitystamp", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_identity_twofactorenabled", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_identity_accessfailedcount", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_identity_lockoutenabled", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_identity_lockoutenddate", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_identity_emailaddress1confirmed", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_identity_mobilephoneconfirmed", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("emailaddress1",  BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("mobilephone", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_profilealert", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_profilemodifiedon", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("firstname", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("lastname", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_preferredlanguageid", BaseSolutionVersions.CentaurusVersion),
		};
	}

	public class CrmUserStore<TUser, TKey>
		: CrmEntityStore<TUser, TKey>,
		  IUserPasswordStore<TUser, TKey>,
		  IUserEmailStore<TUser, TKey>,
		  IUserPhoneNumberStore<TUser, TKey>,
		  IUserSecurityStampStore<TUser, TKey>,
		  IUserTwoFactorStore<TUser, TKey>,
		  IUserLockoutStore<TUser, TKey>,
		  IUserLoginStore<TUser, TKey>
		where TUser : CrmUser<TKey>, new()
		where TKey : IEquatable<TKey>
	{
		public CrmUserStore(CrmDbContext context, CrmEntityStoreSettings settings)
			: base("contact", "contactid", "adx_identity_username", context, settings)
		{
		}

		protected override RetrieveRequest ToRetrieveRequest(EntityReference id)
		{
			// build the related entity queries

			var externalIdentityFetch = new Fetch
			{
				Entity = new FetchEntity("adx_externalidentity", UserConstants.ExternalIdentityAttributes.Columns)
				{
					Filters = new[] { new Filter {
						Conditions = GetActiveStateConditions().ToArray()
					} }
				}
			};

			var relatedEntitiesQuery = new RelationshipQueryCollection
			{
				{ UserConstants.ContactExternalIdentityRelationship, externalIdentityFetch.ToFetchExpression() },
			};

			// retrieve the contact by ID including its related entities

			var request = new RetrieveRequest
			{
				Target = id,
				ColumnSet = new ColumnSet(this.GetContactAttributes().ToArray()),
				RelatedEntitiesQuery = relatedEntitiesQuery
			};

			return request;
		}

		protected override IEnumerable<OrganizationRequest> ToDeleteRequests(TUser model)
		{
			if (Settings.DeleteByStatusCode)
			{
				var entity = new Entity(LogicalName) { Id = ToGuid(model.Id) };
				entity.SetAttributeValue("adx_identity_logonenabled", false);

				yield return new UpdateRequest { Target = entity };
			}

			foreach (var request in base.ToDeleteRequests(model))
			{
				yield return request;
			}
		}

		protected virtual IEnumerable<string> GetContactAttributes()
		{
			return UserConstants.ContactAttributes.ToFilteredColumns(this.BaseSolutionCrmVersion);
		}

		#region IUserPasswordStore

		async Task IUserStore<TUser, TKey>.CreateAsync(TUser user)
		{
			user.LogonEnabled = true;
			user.Id = await CreateAsync(user).WithCurrentCulture();
		}

		public virtual Task SetPasswordHashAsync(TUser user, string passwordHash)
		{
			ThrowIfDisposed();

			user.PasswordHash = passwordHash;
			return Task.FromResult(0);
		}

		public virtual Task<string> GetPasswordHashAsync(TUser user)
		{
			ThrowIfDisposed();

			return Task.FromResult(user.PasswordHash);
		}

		public virtual Task<bool> HasPasswordAsync(TUser user)
		{
			ThrowIfDisposed();

			return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
		}

		#endregion

		#region IUserEmailStore

		public virtual Task SetEmailAsync(TUser user, string email)
		{
			ThrowIfDisposed();

			user.Email = email;
			return Task.FromResult(0);
		}

		public virtual Task<string> GetEmailAsync(TUser user)
		{
			ThrowIfDisposed();

			return Task.FromResult(user.Email);
		}

		public virtual Task<bool> GetEmailConfirmedAsync(TUser user)
		{
			ThrowIfDisposed();

			return Task.FromResult(user.EmailConfirmed);
		}

		public virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed)
		{
			ThrowIfDisposed();

			user.EmailConfirmed = confirmed;
			return Task.FromResult(0);
		}

		public virtual Task<TUser> FindByEmailAsync(string email)
		{
			ThrowIfDisposed();

			if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("The email {0} isn't valid.");

			return FindByConditionAsync(new Condition("emailaddress1", ConditionOperator.Equal, email));
		}

		#endregion

		#region IUserPhoneNumberStore

		public virtual Task<string> GetPhoneNumberAsync(TUser user)
		{
			ThrowIfDisposed();

			return Task.FromResult(user.PhoneNumber);
		}

		public virtual Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
		{
			ThrowIfDisposed();

			return Task.FromResult(user.PhoneNumberConfirmed);
		}

		public virtual Task SetPhoneNumberAsync(TUser user, string phoneNumber)
		{
			ThrowIfDisposed();

			user.PhoneNumber = phoneNumber;
			return Task.FromResult(0);
		}

		public virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
		{
			ThrowIfDisposed();

			user.PhoneNumberConfirmed = confirmed;
			return Task.FromResult(0);
		}

		#endregion

		#region IUserSecurityStampStore

		public virtual Task<string> GetSecurityStampAsync(TUser user)
		{
			ThrowIfDisposed();

			return Task.FromResult(user.SecurityStamp);
		}

		public virtual Task SetSecurityStampAsync(TUser user, string stamp)
		{
			ThrowIfDisposed();

			user.SecurityStamp = stamp;
			return Task.FromResult(0);
		}

		#endregion

		#region IUserTwoFactorStore

		public virtual Task<bool> GetTwoFactorEnabledAsync(TUser user)
		{
			ThrowIfDisposed();

			return Task.FromResult(user.TwoFactorEnabled);
		}

		public virtual Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
		{
			ThrowIfDisposed();

			user.TwoFactorEnabled = enabled;
			return Task.FromResult(0);
		}

		#endregion

		#region IUserLockoutStore

		public virtual Task<int> GetAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();

			return Task.FromResult(user.AccessFailedCount);
		}

		public virtual Task<bool> GetLockoutEnabledAsync(TUser user)
		{
			ThrowIfDisposed();

			return Task.FromResult(user.LockoutEnabled);
		}

		public virtual Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
		{
			ThrowIfDisposed();

			return Task.FromResult(user.LockoutEndDateUtc != null
				? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
				: new DateTimeOffset());
		}

		public virtual Task<int> IncrementAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();

			++user.AccessFailedCount;
			return Task.FromResult(user.AccessFailedCount);
		}

		public virtual Task ResetAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();

			user.AccessFailedCount = 0;
			return Task.FromResult(0);
		}

		public virtual Task SetLockoutEnabledAsync(TUser user, bool enabled)
		{
			ThrowIfDisposed();

			user.LockoutEnabled = enabled;
			return Task.FromResult(0);
		}

		public virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
		{
			ThrowIfDisposed();

			user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue
				? new DateTime?()
				: lockoutEnd.UtcDateTime;
			return Task.FromResult(0);
		}

		#endregion

		#region IUserLoginStore

		public virtual Task AddLoginAsync(TUser user, UserLoginInfo login)
		{
			ThrowIfDisposed();

			user.AddLogin(login);

			return Task.FromResult(0);
		}

		public virtual async Task<TUser> FindAsync(UserLoginInfo login)
		{
			ThrowIfDisposed();

			if (login == null) throw new ArgumentNullException("login");
			if (string.IsNullOrWhiteSpace(login.LoginProvider)) throw new ArgumentException("Invalid LoginProvider.");
			if (string.IsNullOrWhiteSpace(login.ProviderKey)) throw new ArgumentException("Invalid ProviderKey.");

			var entity = await FetchByConditionOnExternalIdentityAsync(
				new Condition("adx_identityprovidername", ConditionOperator.Equal, login.LoginProvider),
				new Condition("adx_username", ConditionOperator.Equal, login.ProviderKey)).WithCurrentCulture();

			return ToModel(entity);
		}

		public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
		{
			ThrowIfDisposed();

			if (user == null) throw new ArgumentNullException("user");
			if (ToGuid(user.Id) == Guid.Empty) throw new ArgumentException("Invalid user ID.");

			return Task.FromResult<IList<UserLoginInfo>>(user.Logins.Select(ToUserLoginInfo).ToList());
		}

		public virtual Task RemoveLoginAsync(TUser user, UserLoginInfo login)
		{
			ThrowIfDisposed();

			user.RemoveLogin(login);

			return Task.FromResult(0);
		}

		private static UserLoginInfo ToUserLoginInfo(CrmUserLogin login)
		{
			return new UserLoginInfo(login.LoginProvider, login.ProviderKey);
		}

		protected virtual Task<Entity> FetchByConditionOnExternalIdentityAsync(params Condition[] conditions)
		{
			// fetch the contact by a custom condition on the external identity

			var fetch = new Fetch
			{
				Entity = new FetchEntity(LogicalName)
				{
					Attributes = FetchAttribute.None,
					Filters = new[] { new Filter {
						Conditions = GetActiveEntityConditions().ToArray()
					} },
					Links = new[]
					{
						new Link { Name = "adx_externalidentity", FromAttribute = "adx_contactid", Filters = new[] {
							new Filter {
								Conditions = GetActiveStateConditions().Concat(conditions).ToArray()
							}
						} }
					}
				}
			};

			return FetchAsync(fetch);
		}

		#endregion
	}
}
