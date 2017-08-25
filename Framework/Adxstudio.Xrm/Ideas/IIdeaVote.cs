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
	public interface IIdeaVote
	{
		/// <summary>
		/// An adx_ideavote entity.
		/// </summary>
		Entity Entity { get; }

		/// <summary>
		/// When the vote was casted.
		/// </summary>
		DateTime SubmittedOn { get; }

		/// <summary>
		/// The whole number value of this vote.
		/// </summary>
		int VoteValue { get; }
	}
}
