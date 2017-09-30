/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.AspNet.Identity
{
	public enum InvitationType
	{
		Single = 756150000,
		Group = 756150001
	}

	public interface IInvitation<out TKey>
	{
		TKey Id { get; }
		DateTime? ExpireDate { get; }
		int Redemptions { get; }
		int MaximumRedemptions { get; }
		InvitationType? Type { get; }
	}

	public interface IInvitationStore<TInvitation, TKey> : IDisposable
		where TKey : IEquatable<TKey>
	{
		Task<TInvitation> FindByCodeAsync(string invitationCode);
		Task RedeemAsync(TInvitation invitation, CrmUser<TKey> user, string userHostAddress);
	}

	public class CrmInvitation<TKey> : CrmModel<TKey>, IInvitation<TKey>
		where TKey : IEquatable<TKey>
	{
		public virtual DateTime? ExpireDate
		{
			get { return Entity.GetAttributeValue<DateTime?>("adx_expirydate"); }
		}

		public virtual string InvitationCode
		{
			get { return Entity.GetAttributeValue<string>("adx_invitationcode"); }
		}

		public virtual Guid InvitationId
		{
			get { return Entity.GetAttributeValue<Guid>("adx_invitationid"); }
		}

		public virtual InvitationType? Type
		{
			get { return Entity.GetAttributeEnumValue<InvitationType>("adx_type"); }
		}

		public virtual int Redemptions
		{
			get { return Entity.GetAttributeValue<int>("adx_redemptions"); }
		}

		public virtual int MaximumRedemptions
		{
			get { return Entity.GetAttributeValue<int>("adx_maximumredemptions"); }
		}

		public virtual EntityReference InvitedContact
		{
			get { return Entity.GetAttributeValue<EntityReference>("adx_invitecontact"); }
		}

		public virtual string Email
		{
			get { return Entity.GetAttributeAliasedValue<string>("emailaddress1", "contact"); }
		}

		public virtual string PhoneNumber
		{
			get { return Entity.GetAttributeAliasedValue<string>("mobilephone", "contact"); }
		}

		public CrmInvitation() : base("adx_invitation") { }
	}

	public class CrmInvitationStore<TInvitation, TKey> : BaseStore, IInvitationStore<TInvitation, TKey>
		where TInvitation : CrmInvitation<TKey>, new()
		where TKey : IEquatable<TKey>
	{
		private static readonly string _contactEntityName = "contact";
		private static readonly string _contactToAttributeName = "adx_invitecontact";

		public CrmInvitationStore(CrmDbContext context)
			: base(context)
		{
		}

		#region IInvitationStore

		private static readonly ColumnSet _invitationAttributes = new ColumnSet(new[]
		{
			"adx_name",
			"adx_expirydate",
			"adx_invitationcode",
			"adx_redemptions",
			"adx_maximumredemptions",
			"adx_type",
			"adx_invitecontact",
		});

		public virtual async Task<TInvitation> FindByCodeAsync(string invitationCode)
		{
			ThrowIfDisposed();

			var entity = await FetchByInvitationAsync(invitationCode).WithCurrentCulture();
			return ToInvitation(entity);
		}

		protected virtual TInvitation ToInvitation(Entity entity)
		{
			if (entity == null) return null;

			var invitation = new TInvitation();
			invitation.SetEntity(entity);

			return invitation;
		}

		protected virtual Guid ToGuid(TKey key)
		{
			return ToGuid<TKey>(key);
		}

		protected virtual TKey ToKey(Guid guid)
		{
			return ToKey<TKey>(guid);
		}

		protected virtual Task<Entity> FetchByInvitationAsync(string invitationCode)
		{
			var now = DateTime.UtcNow.Round(RoundTo.Hour);

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_invitation", _invitationAttributes.Columns)
				{
					Filters = new[] { new Filter {
						Filters = new[] { new Filter {
							Type = LogicalOperator.Or,
							Conditions = new[]
							{
								new Condition("adx_expirydate", ConditionOperator.Null),
								new Condition("adx_expirydate", ConditionOperator.GreaterEqual, now),
							}
						} },
						Conditions = new[]
						{
							GetActiveStateCondition(),
							new Condition("adx_invitationcode", ConditionOperator.Equal, invitationCode),
						}
					} },
					Links = new[] {
						new Link {
							Alias = _contactEntityName, Name = _contactEntityName, ToAttribute = _contactToAttributeName, Type = JoinOperator.LeftOuter,
							Attributes = new[]
							{
								new FetchAttribute("emailaddress1"),
								new FetchAttribute("mobilephone")
							},
							Filters = new[] {
								new Filter {
									Conditions = new[] { GetActiveStateCondition() }
								}
							}
						}
					}
				}
			};

			return FetchSingleOrDefaultAsync(fetch);
		}

		public virtual Task RedeemAsync(TInvitation invitation, CrmUser<TKey> user, string userHostAddress)
		{
			ThrowIfDisposed();

			if (invitation == null) throw new ArgumentNullException("invitation");
			if (user == null) throw new ArgumentNullException("user");

			Execute(ToRedeemRequests(invitation, user, userHostAddress));

			return Task.FromResult(0);
		}

		protected virtual IEnumerable<OrganizationRequest> ToRedeemRequests(TInvitation invitation, CrmUser<TKey> user, string userHostAddress)
		{
			// updating the invitation

			var entity = new Entity("adx_invitation") { Id = ToGuid(invitation.InvitationId), EntityState = EntityState.Changed };

			// update the redemption counter

			var redemptions = invitation.Redemptions + 1;
			entity.SetAttributeValue("adx_redemptions", redemptions);

			// associate a new redemption entity to the invitation

			var redemption = new Entity("adx_inviteredemption") { EntityState = EntityState.Created };
			redemption.SetAttributeValue("subject", user.UserName);
			redemption.SetAttributeValue("adx_ipaddress", userHostAddress);
			redemption.SetAttributeValue("regardingobjectid", entity.ToEntityReference());

			redemption.RelatedEntities[new Relationship("adx_invitation_adx_inviteredemptions")] = new EntityCollection(new[] { entity });

			if (user.ContactId != null)
			{
				// associate the invited contact to the invitation

				if (invitation.Type == InvitationType.Single)
				{
					entity.SetAttributeValue("adx_redeemedcontact", user.ContactId);
				}
				else if (invitation.Type == InvitationType.Group)
				{
					var contact = new Entity(user.ContactId.LogicalName) { Id = user.ContactId.Id, EntityState = EntityState.Unchanged };
					entity.RelatedEntities[new Relationship("adx_invitation_redeemedcontacts")] = new EntityCollection(new[] { contact });
				}

				var activityparty = new Entity("activityparty");
				activityparty["partyid"] = user.ContactId;

				redemption.SetAttributeValue("customers", new EntityCollection(new[] { activityparty }));
			}

			yield return new CreateRequest { Target = redemption };

			if (redemptions >= invitation.MaximumRedemptions)
			{
				// set invitation to redeemed state

				yield return new SetStateRequest
				{
					EntityMoniker = entity.ToEntityReference(),
					State = new OptionSetValue(0),
					Status = new OptionSetValue(756150001) //Redeemed
				};
			}
		}

		protected virtual Condition GetActiveStateCondition()
		{
			return EntityExtensions.ActiveStateCondition;
		}

		private void Execute(IEnumerable<OrganizationRequest> requests)
		{
			// the current OrganizationServiceCache implementation does not support ExecuteMultiple

			//Context.Service.ExecuteMultiple(requests);

			foreach (var request in requests)
			{
				Context.Service.Execute(request);
			}
		}

		#endregion
	}
}
