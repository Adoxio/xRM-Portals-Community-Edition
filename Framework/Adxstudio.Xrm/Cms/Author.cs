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
	public class Author : IAuthor
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="entityReference">A reference to the entity representing this author.</param>
		/// <param name="displayName">The name to display for this author.</param>
		/// <param name="emailAddress">The author's e-mail address.</param>
		/// <param name="websiteUrl">Url to the author's website.</param>
		public Author(EntityReference entityReference, string displayName, string emailAddress = null, string websiteUrl = null)
		{
			EntityReference = entityReference;
			DisplayName = displayName;
			EmailAddress = emailAddress;
			WebsiteUrl = websiteUrl;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="displayName">The name to display for this author.</param>
		/// <param name="emailAddress">The author's e-mail address.</param>
		/// <param name="websiteUrl">Url to the author's website.</param>
		public Author(string displayName, string emailAddress = null, string websiteUrl = null) : this(null, displayName, emailAddress, websiteUrl) { }

		public string DisplayName { get; private set; }

		public string EmailAddress { get; private set; }

		public EntityReference EntityReference { get; private set; }

		public string WebsiteUrl { get; private set; }
	}
}
