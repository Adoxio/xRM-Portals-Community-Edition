/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Adxstudio.Xrm.AspNet.Identity
{
	public class InvitationManager<TInvitation, TKey> : BaseManager<IInvitationStore<TInvitation, TKey>>
		where TInvitation : class, IInvitation<TKey>
		where TKey : IEquatable<TKey>
	{
		public virtual CrmIdentityErrorDescriber IdentityErrors { get; private set; }

		public InvitationManager(IInvitationStore<TInvitation, TKey> store, CrmIdentityErrorDescriber identityErrors)
			: base(store)
		{
			IdentityErrors = identityErrors;
		}

		public virtual async Task<TInvitation> FindByCodeAsync(string invitationCode)
		{
			ThrowIfDisposed();

			if (invitationCode == null) throw new ArgumentNullException("invitationCode");

			var invitation = await Store.FindByCodeAsync(invitationCode).WithCurrentCulture();

			// validate the invitation

			return invitation != null && ((invitation.MaximumRedemptions == 0) || (invitation.Redemptions < invitation.MaximumRedemptions))
				? invitation
				: null;
		}

		public virtual async Task<IdentityResult> RedeemAsync(TInvitation invitation, CrmUser<TKey> user, string userHostAddress)
		{
			ThrowIfDisposed();

			if (invitation == null) throw new ArgumentNullException("invitation");
			if (user == null) throw new ArgumentNullException("user");

			// validate the invitation

			if (invitation.MaximumRedemptions > 0 && invitation.Redemptions >= invitation.MaximumRedemptions)
			{
				var message = IdentityErrors.InvalidInvitationCode();
				return IdentityResult.Failed(message.Description);
			}

			await Store.RedeemAsync(invitation, user, userHostAddress).WithCurrentCulture();

			return IdentityResult.Success;
		}
	}
}
