/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	using System;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// The search saved query.
	/// </summary>
	public class SearchSavedQuery
	{
		/// <summary>
		/// Gets or sets the saved query id.
		/// </summary>
		public Guid SavedQueryId { get; set; }

		/// <summary>
		/// Gets or sets a SavedQueryIdUnique
		/// </summary>
		public Guid SavedQueryIdUnique { get; set; }

		/// <summary>
		/// Gets or sets the entity name.
		/// </summary>
		public string EntityName { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchSavedQuery"/> class.
		/// </summary>
		/// <param name="entity">
		/// The entity.
		/// </param>
		public SearchSavedQuery(Entity entity)
		{
			this.SavedQueryId = entity.Id;
			this.SavedQueryIdUnique = entity.GetAttributeValue<Guid>("savedqueryidunique");
			this.EntityName = entity.GetAttributeValue<string>("returnedtypecode");
		}

		/// <summary>
		/// The get hash code.
		/// </summary>
		/// <returns>
		/// The <see cref="int"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return this.SavedQueryId.GetHashCode();
		}

		/// <summary>
		/// The equals.
		/// </summary>
		/// <param name="obj">
		/// The obj.
		/// </param>
		/// <returns>
		/// The <see cref="bool"/>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj == null || this.GetType() != obj.GetType())
			{
				return false;
			}

			SearchSavedQuery item = obj as SearchSavedQuery;
			return item.SavedQueryId == this.SavedQueryId;
		}
	}
}
