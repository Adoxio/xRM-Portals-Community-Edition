/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI
{
	using System;
	using Adxstudio.Xrm.Diagnostics.Trace;

	/// <summary>
	/// Indicates that a savedquery could not be retrieved.
	/// </summary>
	public class SavedQueryNotFoundException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SavedQueryNotFoundException" /> class.
		/// </summary>
		public SavedQueryNotFoundException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SavedQueryNotFoundException" /> class.
		/// </summary>
		/// <param name="savedQueryId">Unique ID of the savedquery record in CRM.</param>
		public SavedQueryNotFoundException(Guid savedQueryId)
			: base($"A saved query with savedqueryid equal to {savedQueryId} couldn't be found.")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SavedQueryNotFoundException" /> class.
		/// </summary>
		/// <param name="entityLogicalName">Logical Name of the entity the savedquery is associated to.</param>
		public SavedQueryNotFoundException(string entityLogicalName)
			: base($"A saved query for the entity {EntityNamePrivacy.GetEntityName(entityLogicalName)} couldn't be found.")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SavedQueryNotFoundException" /> class.
		/// </summary>
		/// <param name="entityLogicalName">Logical Name of the entity the savedquery is associated to.</param>
		/// <param name="queryType">Query Type of the savedquery record in CRM.</param>
		/// <param name="isDefault">Is the default savedquery.</param>
		public SavedQueryNotFoundException(string entityLogicalName, int queryType, bool isDefault)
			: base($"A saved query for entity {EntityNamePrivacy.GetEntityName(entityLogicalName)} with the querytype {queryType} and isdefault {isDefault} couldn't be found.")
		{
		}
	}
}
