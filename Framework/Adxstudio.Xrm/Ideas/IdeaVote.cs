/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Represents a Vote for an Idea in an Adxstudio Portals Idea Forum.
	/// </summary>
	public class IdeaVote : IIdeaVote
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="entity">An adx_ideavote entity.</param>
		public IdeaVote(Entity entity)
		{
			entity.ThrowOnNull("entity");
			entity.AssertEntityName("feedback");

			Entity = entity;
			SubmittedOn = entity.GetAttributeValue<DateTime?>("createdon").GetValueOrDefault();
			VoteValue = entity.GetAttributeValue<int?>("rating").GetValueOrDefault();
		}

		/// <summary>
		/// An adx_ideavote entity.
		/// </summary>
		public Entity Entity { get; private set; }

		/// <summary>
		/// When the vote was casted.
		/// </summary>
		public DateTime SubmittedOn { get; private set; }

		/// <summary>
		/// The whole number value of this vote.
		/// </summary>
		public int VoteValue { get; private set; }
	}
}
