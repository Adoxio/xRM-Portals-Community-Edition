/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.AspNet.Identity
{
	public class CrmUser : CrmUser<string>, IDisposable
	{
		public static readonly CrmUser Anonymous = new CrmUser();

		public bool IsDirty { get; set; }

		public CrmUser()
		{
		}

		public CrmUser(Entity entity)
			: base(entity)
		{
		}

		void IDisposable.Dispose() { }
	}

	public class CrmUser<TKey> : CrmModel<TKey>, IUser<TKey>
		where TKey : IEquatable<TKey>
	{
		private Lazy<IEnumerable<CrmUserLogin>> _logins;

		public virtual IEnumerable<CrmUserLogin> Logins
		{
			get { return _logins.Value; }
		}

		public virtual EntityReference ContactId
		{
			get { return Entity.ToEntityReference(); }
		}

		public virtual string UserName
		{
			get { return Name; }
			set { Name = value; }
		}

		public virtual bool LogonEnabled
		{
			get { return Entity.GetAttributeValue<bool>("adx_identity_logonenabled"); }
			set { Entity.SetAttributeValue("adx_identity_logonenabled", value); }
		}

		public virtual string Email
		{
			get { return Entity.GetAttributeValue<string>("emailaddress1"); }
			set { Entity.SetAttributeValue("emailaddress1", value); }
		}

		public virtual bool EmailConfirmed
		{
			get { return Entity.GetAttributeValue<bool>("adx_identity_emailaddress1confirmed"); }
			set { Entity.SetAttributeValue("adx_identity_emailaddress1confirmed", value); }
		}

		public virtual string PasswordHash
		{
			get { return Entity.GetAttributeValue<string>("adx_identity_passwordhash"); }
			set { Entity.SetAttributeValue("adx_identity_passwordhash", value); }
		}

		public virtual string SecurityStamp
		{
			get { return Entity.GetAttributeValue<string>("adx_identity_securitystamp"); }
			set { Entity.SetAttributeValue("adx_identity_securitystamp", value); }
		}

		public virtual string PhoneNumber
		{
			get { return Entity.GetAttributeValue<string>("mobilephone"); }
			set { Entity.SetAttributeValue("mobilephone", value); }
		}

		public virtual bool PhoneNumberConfirmed
		{
			get { return Entity.GetAttributeValue<bool>("adx_identity_mobilephoneconfirmed"); }
			set { Entity.SetAttributeValue("adx_identity_mobilephoneconfirmed", value); }
		}

		public virtual bool TwoFactorEnabled
		{
			get { return Entity.GetAttributeValue<bool>("adx_identity_twofactorenabled"); }
			set { Entity.SetAttributeValue("adx_identity_twofactorenabled", value); }
		}

		public virtual DateTime? LockoutEndDateUtc
		{
			get { return Entity.GetAttributeValue<DateTime?>("adx_identity_lockoutenddate"); }
			set { Entity.SetAttributeValue("adx_identity_lockoutenddate", value); }
		}

		public virtual bool LockoutEnabled
		{
			get { return Entity.GetAttributeValue<bool>("adx_identity_lockoutenabled"); }
			set { Entity.SetAttributeValue("adx_identity_lockoutenabled", value); }
		}

		public virtual int AccessFailedCount
		{
			get { return Entity.GetAttributeValue<int>("adx_identity_accessfailedcount"); }
			set { Entity.SetAttributeValue("adx_identity_accessfailedcount", value); }
		}

		public virtual bool HasProfileAlert
		{
			get { return Entity.GetAttributeValue<bool>("adx_profilealert"); }
			set { Entity.SetAttributeValue("adx_profilealert", value); }
		}

		public virtual DateTime? ProfileModifiedOn
		{
			get { return Entity.GetAttributeValue<DateTime?>("adx_profilemodifiedon"); }
			set { Entity.SetAttributeValue("adx_profilemodifiedon", value); }
		}

		public virtual string FirstName
		{
			get { return Entity.GetAttributeValue<string>("firstname"); }
			set { Entity.SetAttributeValue("firstname", value); }
		}

		public virtual string LastName
		{
			get { return Entity.GetAttributeValue<string>("lastname"); }
			set { Entity.SetAttributeValue("lastname", value); }
		}

		public virtual EntityReference PreferredLanguage
		{
			get { return Entity.GetAttributeValue<EntityReference>("adx_preferredlanguageid"); }
			set { Entity.SetAttributeValue("adx_preferredlanguageid", value); }
		}

		public CrmUser()
			: this(null)
		{
		}

		public CrmUser(Entity entity)
			: base("contact", "adx_identity_username", entity)
		{
			_logins = new Lazy<IEnumerable<CrmUserLogin>>(GetUserLogins);
		}

		public virtual void AddLogin(UserLoginInfo login)
		{
			var entity = new Entity("adx_externalidentity");
			entity.SetAttributeValue("adx_identityprovidername", login.LoginProvider);
			entity.SetAttributeValue("adx_username", login.ProviderKey);

			if (!Entity.RelatedEntities.ContainsKey(UserConstants.ContactExternalIdentityRelationship))
			{
				Entity.RelatedEntities.Add(UserConstants.ContactExternalIdentityRelationship, new EntityCollection(new[] { entity }));
			}
			else
			{
				Entity.RelatedEntities[UserConstants.ContactExternalIdentityRelationship].Entities.Add(entity);
			}

			_logins = new Lazy<IEnumerable<CrmUserLogin>>(GetUserLogins);
		}

		public virtual void RemoveLogin(UserLoginInfo login)
		{
			if (!Entity.RelatedEntities.ContainsKey(UserConstants.ContactExternalIdentityRelationship)) return;

			var entities = Entity
				.RelatedEntities[UserConstants.ContactExternalIdentityRelationship].Entities
				.Where(e => e.GetAttributeValue<string>("adx_identityprovidername") == login.LoginProvider && e.GetAttributeValue<string>("adx_username") == login.ProviderKey)
				.ToArray();

			foreach (var entity in entities)
			{
				Entity.RelatedEntities[UserConstants.ContactExternalIdentityRelationship].Entities.Remove(entity);
			}

			_logins = new Lazy<IEnumerable<CrmUserLogin>>(GetUserLogins);
		}

		protected virtual IEnumerable<CrmUserLogin> GetUserLogins()
		{
			return GetRelatedEntities(UserConstants.ContactExternalIdentityRelationship).Select(ToLogin).ToList();
		}

		protected virtual CrmUserLogin ToLogin(Entity entity)
		{
			return new CrmUserLogin(entity);
		}
	}
}
