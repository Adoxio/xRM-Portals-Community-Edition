/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatAreaRegistration.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Site.Areas.Chat
{
	using System.Web.Mvc;

	/// <summary>
	/// The chat area registration.
	/// </summary>
	public class ChatAreaRegistration : AreaRegistration
	{
		/// <summary>
		/// Gets the area name.
		/// </summary>
		public override string AreaName
		{
			get { return "Chat"; }
		}

		/// <summary>
		/// The register area.
		/// </summary>
		/// <param name="context">
		/// The context.
		/// </param>
		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute("Chat_Auth", "_services/auth/{action}", new { controller = "ChatAuth" });
		}
	}
}
