/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	using System;
	using System.Web;
	using Microsoft.Xrm.Sdk;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.Cms.SolutionVersions;
	using Adxstudio.Xrm.Web;

	/// <summary>
	/// Factory Class to initialize an IIdeaForumDataAdapter
	/// </summary>
	public sealed class IdeaForumDataAdapterFactory
	{

		/// <summary>
		/// Prevents a default instance of the <see cref="IdeaForumDataAdapterFactory" /> class from being created.
		/// </summary>
		private IdeaForumDataAdapterFactory()
		{

		}

		/// <summary>
		/// Creates an instance of the IdeaDataAdapterFactory
		/// </summary>
		public static IdeaForumDataAdapterFactory Instance
		{
			get
			{
				return new IdeaForumDataAdapterFactory();
			}
		}

		/// <summary>
		/// Creates an instance of IdeaForumDataAdapter
		/// </summary>
		/// <param name="ideaForum">Forum Entity to create an adapter for</param>
		/// <param name="filter">Type of filter to create ex.) new, hot, 'other'</param>
		/// <param name="timeSpan">Timespan from which to query ideas</param>
		/// <param name="status">Type of ideas in which to include</param>
		/// <returns>type: IIdeaForumDataAdapter</returns>
		public IIdeaForumDataAdapter CreateIdeaForumDataAdapter(Entity ideaForum, string filter, string timeSpan, int? status = 1)
		{
			IIdeaForumDataAdapter ideaForumDataAdapter = null;

			if (string.Equals(filter, "new", StringComparison.InvariantCultureIgnoreCase))
			{
				ideaForumDataAdapter = new IdeaForumByNewDataAdapter(ideaForum);
			}
			else
			{
				ideaForumDataAdapter = this.IsIdeasPreRollup()
					? this.CreateIdeaDataAdapterPreRollup(ideaForum, filter)
					: this.CreateIdeaDataAdapter(ideaForum, filter);
			}

			ideaForumDataAdapter.MinDate = timeSpan == "this-year" ? DateTime.UtcNow.AddYears(-1).Date
				: timeSpan == "this-month" ? DateTime.UtcNow.AddMonths(-1).Date
				: timeSpan == "this-week" ? DateTime.UtcNow.AddDays(-7).Date
				: timeSpan == "today" ? DateTime.UtcNow.AddHours(-24)
				: (DateTime?)null;

			ideaForumDataAdapter.Status = status != (int?)IdeaStatus.Any ? status : null;

			return ideaForumDataAdapter;
		}

		/// <summary>
		/// Create an IdeaDataAdapter that utilizes the rollup fields (v8.3+)
		/// </summary>
		/// <param name="ideaForum">Forum Entity to create an adapter for</param>
		/// <param name="filter">Type of filter to create ex.) new, hot, 'other'</param>
		/// <returns>type: IIdeaForumDataAdapter</returns>
		public IIdeaForumDataAdapter CreateIdeaDataAdapter(Entity ideaForum, string filter)
		{
			IIdeaForumDataAdapter ideaForumDataAdapter = null;

			if (string.Equals(filter, "hot", StringComparison.InvariantCultureIgnoreCase))
			{
				ideaForumDataAdapter = new IdeaForumByHotDataAdapter(ideaForum);
			}
			else
			{
				ideaForumDataAdapter = new IdeaForumDataAdapter(ideaForum);
			}

			return ideaForumDataAdapter;
		}

		/// <summary>
		/// Create an IdeaDataAdapter for versions before the rollup schema was introduced (v8.3)
		/// </summary>
		/// <param name="ideaForum">Forum Entity to create an adapter for</param>
		/// <param name="filter">Type of filter to create ex.) new, hot, 'other'</param>
		/// <returns>type: IIdeaForumDataAdapter</returns>
		public IIdeaForumDataAdapter CreateIdeaDataAdapterPreRollup(Entity ideaForum, string filter)
		{
			IIdeaForumDataAdapter ideaForumDataAdapter = null;

			if (string.Equals(filter, "hot", StringComparison.InvariantCultureIgnoreCase))
			{
				ideaForumDataAdapter = new IdeaForumByHotDataAdapterPreRollup(ideaForum);
			}
			else
			{
				ideaForumDataAdapter = new IdeaForumDataAdapterPreRollup(ideaForum);
			}

			return ideaForumDataAdapter;
		}

		/// <summary>
		/// Determines if MicrosoftIdeas solution is pre Potassium when the rollup schema was released(v8.3)
		/// </summary>
		/// <returns>true if version lt 8.3.0.0; otherise false</returns>
		private bool IsIdeasPreRollup()
		{
			bool preRollup = true;
			if (HttpContext.Current != null)
			{
				var solution = HttpContext.Current.GetPortalSolutionsDetails().Solutions[PortalSolutions.SolutionNames.IdeasSolutionName];
				preRollup = solution.SolutionVersion < BaseSolutionVersions.PotassiumVersion;
			}
			return preRollup;
		}
	}
}
