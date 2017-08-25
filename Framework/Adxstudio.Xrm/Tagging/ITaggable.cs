/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Tagging
{
	/// <summary>
	/// Interface for managing <see cref="ITagInfo">tags</see> on a given object (typically
	/// some type of CRM entity).
	/// </summary>
	public interface ITaggable
	{
		/// <summary>
		/// Gets all <see cref="ITagInfo">tags</see> associated with the current instance.
		/// </summary>
		IEnumerable<Entity> Tags { get; }

		/// <summary>
		/// Associates a tag, by name, with the current instance.
		/// </summary>
		/// <param name="tagName">The name of the tag to be associated.</param>
		void AddTag(string tagName);

		/// <summary>
		/// Dis-associates a tag, by name, from the current instance.
		/// </summary>
		/// <param name="tagName">The name of the tag to be dis-associated.</param>
		void RemoveTag(string tagName);
	}
}
