/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

//Begin Internal Documentation

using System;
using System.Data;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.Security.LiveId
{
	internal sealed class LiveIdUser
	{
		private const string AttributeMapApproved = "adx_logonenabled";
		private const string AttributeMapCreatedAt = "createdon";
		private const string AttributeMapLastLogin = "adx_lastsuccessfullogon";
		private const string AttributeMapUsername = "adx_username";
		private const string MemberEntityName = "contact";

		private readonly OrganizationServiceContext _context;

		public bool Approved { get; set; }

		public DateTime? CreatedAt { get; private set; }

		public bool HasBeenCreated { get { return CreatedAt.HasValue; } }

		public DateTime LastLogin { get; set; }

		public string UserId { get; set; }

		public LiveIdUser(string connectionStringName, string userId)
		{
			AssertStringHasValue(userId, "userId");

			_context = CreateContext(connectionStringName);

			UserId = userId;
		}

		private LiveIdUser(Entity user, string connectionStringName)
		{
			user.ThrowOnNull("user");

			_context = CreateContext(connectionStringName);

			Approved = user.GetAttributeValue<bool?>(AttributeMapApproved).GetValueOrDefault();
			CreatedAt = user.GetAttributeValue<DateTime?>(AttributeMapCreatedAt).GetValueOrDefault();
			LastLogin = user.GetAttributeValue<DateTime?>(AttributeMapLastLogin).GetValueOrDefault();
			UserId = user.GetAttributeValue<string>(AttributeMapUsername);
		}

		private void Create()
		{
			var user = new Entity(MemberEntityName);

			CreatedAt = LastLogin = DateTime.UtcNow;

			user.SetAttributeValue(AttributeMapUsername, UserId);
			user.SetAttributeValue(AttributeMapCreatedAt, CreatedAt.Value);
			user.SetAttributeValue(AttributeMapLastLogin, LastLogin);
			user.SetAttributeValue(AttributeMapApproved, Approved);

			_context.AddObject(user);

			_context.SaveChanges();
		}

		public static void Delete(string userId, string connectionStringName)
		{
			AssertStringHasValue(userId, "userId");

			var context = CreateContext(connectionStringName);

			var user = GetUserEntity(userId, context);

			if (user == null) throw new ActionNotSupportedException("Cannot delete a user that does not exist.");

			user.SetAttributeValue(AttributeMapUsername, null);
			user.SetAttributeValue(AttributeMapCreatedAt, null);
			user.SetAttributeValue(AttributeMapLastLogin, null);
			user.SetAttributeValue(AttributeMapApproved, null);

			context.UpdateObject(user);

			context.SaveChanges();
		}

		public static LiveIdUser GetByUserId(string userId, string connectionStringName)
		{
			AssertStringHasValue(userId, "userId");

			var context = CreateContext(connectionStringName);
			var user = GetUserEntity(userId, context);

			return user == null ? null : new LiveIdUser(user, connectionStringName);
		}

		private void Update()
		{
			var user = GetUserEntity(UserId, _context);

			user.SetAttributeValue(AttributeMapApproved, Approved);
			user.SetAttributeValue(AttributeMapLastLogin, LastLogin.ToUniversalTime());

			_context.Attach(user);
			_context.UpdateObject(user);
			_context.SaveChanges();
			_context.Detach(user);
		}

		public void Save()
		{
			if (CreatedAt.HasValue)
			{
				Update();
			}
			else
			{
				Create();
			}
		}

		private static void AssertStringHasValue(string value, string name)
		{
			if (string.IsNullOrEmpty(value)) { throw new NoNullAllowedException("'{0}' cannot be null or empty".FormatWith(name)); }
		}

		private static Entity GetUserEntity(string userId, OrganizationServiceContext dataContext)
		{
			return dataContext.CreateQuery(MemberEntityName).FirstOrDefault(entity => entity.GetAttributeValue<string>(AttributeMapUsername) == userId);
		}

		private static OrganizationServiceContext CreateContext(string connectionStringName)
		{
			var context = new CrmOrganizationServiceContext(new CrmConnection(connectionStringName)) { MergeOption = MergeOption.NoTracking };
			return context;
		}
	}
}

//End Internal Documentation
