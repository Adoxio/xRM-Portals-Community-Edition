/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	/// <summary>
	/// Represents an author in Adxstudio Portals.
	/// </summary>
	public interface IAuthor
	{
		/// <summary>
		/// The name to display for this author.
		/// </summary>
		string DisplayName { get; }
		
		/// <summary>
		/// The author's e-mail address.
		/// </summary>
		string EmailAddress { get; }

		/// <summary>
		/// A reference to the entity representing this author.
		/// </summary>
		EntityReference EntityReference { get; }

		/// <summary>
		/// Url to the author's website.
		/// </summary>
		string WebsiteUrl { get; }
	}
}
