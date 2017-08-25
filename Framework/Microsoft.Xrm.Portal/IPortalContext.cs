/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal
{
	/// <summary>
	/// Contains the <see cref="Entity"/> instances that are relevant to a single portal page request.
	/// </summary>
	public interface IPortalContext
	{
		/// <summary>
		/// The <see cref="OrganizationServiceContext"/> used to obtain the <see cref="Entity"/> instances of the portal context.
		/// </summary>
		OrganizationServiceContext ServiceContext { get; }

		/// <summary>
		/// The configured website.
		/// </summary>
		Entity Website { get; }

		/// <summary>
		/// The current authenticated contact.
		/// </summary>
		Entity User { get; }

		/// <summary>
		/// The current entity.
		/// </summary>
		Entity Entity { get; }

		/// <summary>
		/// The current virtual path.
		/// </summary>
		string Path { get; }

		/// <summary>
		/// The current status code.
		/// </summary>
		HttpStatusCode StatusCode { get; }
	}

	/// <summary>
	/// Helper methods on the <see cref="IPortalContext"/>.
	/// </summary>
	public static class PortalContextExtensions
	{
		/// <summary>
		/// Returns the timezone for the website.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static TimeZoneInfo GetTimeZone(this IPortalContext context)
		{
			var timeZone = context.ServiceContext.GetTimeZone(context.Website);
			return timeZone;
		}
	}
}
