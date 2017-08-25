/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Diagnostics.Trace
{
	using System;
	using System.Reflection;

	/// <summary>
	/// Cloud hosted portal details.
	/// </summary>
	public class JourneyDetail
	{
		/// <summary>
		/// Interaction Name
		/// </summary>
		private string InteractionName { get; set; }

		/// <summary>
		/// Interaction Json
		/// </summary>
		private string InteractionJson { get; set; }

		/// <summary>
		/// Interaction Json
		/// </summary>
		private DateTime TimeStamp { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="JourneyDetail" /> class.
		/// </summary>
		/// <param name="interactionName">Interaction Name</param>
		/// <param name="interactionJson">Interaction Json</param>
		/// <param name="timeStamp">Time Stamp</param>
		public JourneyDetail(string interactionName, string interactionJson, DateTime timeStamp)
		{
			this.InteractionName = interactionName;
			this.InteractionJson = interactionJson;
			this.TimeStamp = timeStamp;
		}
	}
}
