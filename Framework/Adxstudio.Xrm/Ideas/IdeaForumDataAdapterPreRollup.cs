/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Provides methods to get and set data for an Adxstudio Portals Idea Forum such as ideas.
	/// This DataAdapter is to be used with 'legacy' solutions before the VotesSum and VoteCount rollup fields.
	/// </summary>
	/// <remarks>Ideas are returned ordered by their vote sum (positive votes minus negative votes, highest first).</remarks>
	public class IdeaForumDataAdapterPreRollup : IdeaForumDataAdapter, IRollupFreeIdeaForumDataAdapter
	{
		/// <summary>
		/// Returns the attribute to be used in Data Selection process
		/// </summary>
		protected override string OrderAttribute
		{
			get { return "adx_votesum"; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IdeaForumDataAdapterPreRollup" /> class.
		/// </summary>
		/// <param name="ideaForum">The idea forum to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IdeaForumDataAdapterPreRollup(Entity ideaForum, string portalName = null)
			: base(ideaForum.ToEntityReference(), new PortalConfigurationDataAdapterDependencies(portalName))
		{

		}
	}
}
