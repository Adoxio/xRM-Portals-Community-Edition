/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	using System.Globalization;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Provides methods to get and set data for an Adxstudio Portals Idea Forum such as ideas.
	/// </summary>
	/// <remarks>Ideas are returned ordered by the number of votes it has (highest first).</remarks>
	public class IdeaForumByHotDataAdapter : IdeaForumDataAdapter
	{
		/// <summary>
		/// Returns the attribute to be used in Data Selection process
		/// </summary>
		protected override string OrderAttribute
		{
			get { return "adx_totalvotes"; }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ideaForum">The idea forum to get and set data for.</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public IdeaForumByHotDataAdapter(Entity ideaForum, string portalName = null) : base(ideaForum, portalName) { }

		/// <summary>
		/// Returns the number of ideas that have been submitted to the idea forum this adapter applies to.
		/// </summary>
		/// <returns></returns>
		public override int SelectIdeaCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var includeUnapprovedIdeas = TryAssertIdeaPreviewPermission(serviceContext);

			return serviceContext.FetchCount("adx_idea", "adx_ideaid",
				addCondition =>
				{
					addCondition("adx_ideaforumid", "eq", IdeaForum.Id.ToString());
					addCondition("statecode", "eq", "0");

					if (!includeUnapprovedIdeas)
					{
						addCondition("adx_approved", "eq", "true");
					}

					if (Status.HasValue)
					{
						addCondition("statuscode", "eq", "{0}".FormatWith((int)Status.Value));
					}
				},
				addLinkEntity => addLinkEntity("feedback", "regardingobjectid", "adx_ideaid",
					addCondition =>
					{
						addCondition("statecode", "eq", "0");

						if (MaxDate.HasValue)
						{
							addCondition("createdon", "le", MaxDate.Value.ToUniversalTime().ToString(CultureInfo.InvariantCulture));
						}

						if (MinDate.HasValue)
						{
							addCondition("createdon", "ge", MinDate.Value.ToUniversalTime().ToString(CultureInfo.InvariantCulture));
						}
					},
					null));
		}
	}
}
