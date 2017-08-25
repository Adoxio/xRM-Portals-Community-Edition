/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Provides methods to get and set data for an Adxstudio Portals Idea Forum such as ideas.
	/// </summary>
	/// <remarks>Ideas are returned reverse chronologically by their submission date.</remarks>
	public class IdeaForumByNewDataAdapter : IdeaForumDataAdapter, IRollupFreeIdeaForumDataAdapter
	{
		/// <summary>
		/// Returns the attribute to be used in Data Selection process
		/// </summary>
		protected override string OrderAttribute
		{
			get { return "adx_date"; }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ideaForum">The idea forum to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IdeaForumByNewDataAdapter(Entity ideaForum, string portalName = null) : base(ideaForum, portalName) { }
	}
}
