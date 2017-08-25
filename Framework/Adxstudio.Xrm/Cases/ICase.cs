/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cases
{
	/// <summary>
	/// Represents full, extended info about a support case.
	/// </summary>
	public interface ICase
	{
		string CaseTypeLabel { get; }

		DateTime CreatedOn { get; }

		EntityReference Customer { get; }

		string Description { get; }

		Entity Entity { get; }

		EntityReference EntityReference { get; }

		bool IsActive { get; }

		bool IsCanceled { get; }

		bool IsResolved { get; }

		bool PublishToWeb { get; }

		EntityReference ResponsibleContact { get; }

		string ResponsibleContactEmailAddress { get; }

		string ResponsibleContactName { get; }

		string Resolution { get; }

		DateTime? ResolutionDate { get; }

		string StateLabel { get; }

		string StatusLabel { get; }

		string TicketNumber { get; }

		string Title { get; }

		string Url { get; }
	}

	internal static class CaseExtensions
	{
		public static bool HasCustomer(this ICase @case, EntityReference entityReference)
		{
			if (@case == null) throw new ArgumentNullException("case");

			return @case.Customer != null
				&& entityReference != null
				&& @case.Customer.Equals(entityReference);
		}
	}
}
