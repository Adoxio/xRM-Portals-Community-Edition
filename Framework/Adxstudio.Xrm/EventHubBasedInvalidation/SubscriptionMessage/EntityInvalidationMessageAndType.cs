/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	/// <summary>
	/// EntityRecordMessage with type.
	/// </summary>
	public class EntityInvalidationMessageAndType
	{
		/// <summary>
		/// Whether it is a message for search index invalidation or not.
		/// </summary>
		public bool IsSearchIndexInvalidationMessage { get; set; }

		/// <summary>
		/// Entity Record Message
		/// </summary>
		public EntityRecordMessage Message { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityInvalidationMessageAndType"/> class
		/// </summary>
		/// <param name="message">Entity record message</param>
		/// <param name="isSearchIndexInvalidationMessage">Whether it is a message for search index invalidation or not.</param>
		public EntityInvalidationMessageAndType(EntityRecordMessage message, bool isSearchIndexInvalidationMessage)
		{
			this.Message = message;
			this.IsSearchIndexInvalidationMessage = isSearchIndexInvalidationMessage;
		}
	}
}
